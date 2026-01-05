using System;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using Void2610.UnityTemplate;

public class AreaPresenter : IDisposable
{
    private static readonly string[] _bgmNames = { "Bgm1", "Bgm2", "Bgm3" };

    private readonly GameModel _gameModel;
    private readonly AreaModel _areaModel;
    private readonly StatsView _statsView;
    private readonly AreaTransitionView _areaTransitionView;
    private readonly CompositeDisposable _disposables = new();
    private string _currentBgmName;
    private bool _isTransitioning;

    public AreaPresenter(GameModel gameModel, AreaModel areaModel)
    {
        _gameModel = gameModel;
        _areaModel = areaModel;
        _statsView = UnityEngine.Object.FindFirstObjectByType<StatsView>();
        _areaTransitionView = UnityEngine.Object.FindFirstObjectByType<AreaTransitionView>();

        // 初期状態でボタン非表示
        _statsView.SetAreaUnlockButtonVisible(false);

        // ポイント変更時にボタン表示更新
        _gameModel.Points.Subscribe(UpdateUnlockButtonVisibility).AddTo(_disposables);
        // 解放ボタンクリック時にエリア進行
        _statsView.OnAreaUnlockClicked.Subscribe(_ => TryAdvanceArea()).AddTo(_disposables);
        // エリア解放時に演出再生
        _areaModel.OnAreaAdvanced.Subscribe(areaName => PlayAreaTransition(areaName).Forget()).AddTo(_disposables);
        // 暗転完了時に必要クローン数を更新
        _areaTransitionView.OnBlackout.Subscribe(_ => _areaModel.UpdateRequiredClones()).AddTo(_disposables);
    }

    // セーブデータから復元したエリアに対応する環境を設定
    public void InitializeEnvironment()
    {
        if (_areaModel.CurrentAreaIndex.CurrentValue > 0)
        {
            _areaTransitionView.ReplaceEnvironment(_areaModel.GetCurrentEnvironmentPrefab());
        }
    }

    public void PlayRandomBgm()
    {
        _currentBgmName = GetRandomBgmName();
        CriBgmController.Instance.PlayBgm(_currentBgmName);
    }

    private void UpdateUnlockButtonVisibility(long currentClones)
    {
        // 演出中は常に非表示
        if (_isTransitioning)
        {
            _statsView.SetAreaUnlockButtonVisible(false);
            return;
        }

        var canAdvance = _areaModel.CanAdvanceArea(currentClones);
        _statsView.SetAreaUnlockButtonVisible(canAdvance);
    }

    private void TryAdvanceArea() => _areaModel.TryAdvanceArea(_gameModel.CurrentPoints);

    private async UniTaskVoid PlayAreaTransition(string areaName)
    {
        // 演出中フラグをセット
        _isTransitioning = true;

        // ボタン非表示
        _statsView.SetAreaUnlockButtonVisible(false);

        // ランダムなBGM名を取得
        var newBgmName = GetRandomBgmName();

        // 新エリアの環境Prefabを取得して演出再生
        var environmentPrefab = _areaModel.GetCurrentEnvironmentPrefab();
        await _areaTransitionView.PlayTransition(areaName, environmentPrefab, newBgmName);

        // 演出中フラグを解除
        _isTransitioning = false;

        // 遷移完了を通知（ダイアログ表示のトリガー）
        _areaModel.NotifyTransitionCompleted();
    }

    private string GetRandomBgmName()
    {
        // 現在と異なるBGMをランダムに選択
        string newBgmName;
        do
        {
            newBgmName = _bgmNames[UnityEngine.Random.Range(0, _bgmNames.Length)];
        } while (newBgmName == _currentBgmName && _bgmNames.Length > 1);

        return newBgmName;
    }

    public void Dispose() => _disposables.Dispose();
}
