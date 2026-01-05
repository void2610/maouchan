using System;
using Cysharp.Threading.Tasks;
using R3;

public class GameStatsPresenter : IDisposable
{
    private readonly GameStatsWindowView _windowView;
    private readonly CompositeDisposable _disposables = new();

    public GameStatsPresenter(GameStatsModel statsModel)
    {
        _windowView = UnityEngine.Object.FindFirstObjectByType<GameStatsWindowView>();

        // ボタンView取得・購読
        var buttonView = UnityEngine.Object.FindFirstObjectByType<GameStatsButtonView>();
        buttonView.OnStatsButtonClicked
            .Subscribe(_ => ShowStats())
            .AddTo(_disposables);

        // 閉じるボタン購読
        _windowView.OnCloseButtonClicked
            .Subscribe(_ => HideStats())
            .AddTo(_disposables);

        // 統計値の変化を購読してUI更新
        statsModel.TotalClickCount
            .Subscribe(_windowView.UpdateTotalClickCount)
            .AddTo(_disposables);

        statsModel.TotalClonesFromClick
            .Subscribe(_windowView.UpdateTotalClonesFromClick)
            .AddTo(_disposables);

        statsModel.TotalClonesFromDps
            .Subscribe(_windowView.UpdateTotalClonesFromDps)
            .AddTo(_disposables);

        statsModel.TotalClonesConsumed
            .Subscribe(_windowView.UpdateTotalClonesConsumed)
            .AddTo(_disposables);

        statsModel.TotalPlayTimeSeconds
            .Subscribe(_windowView.UpdateTotalPlayTime)
            .AddTo(_disposables);
    }

    private void ShowStats() => _windowView.Show().Forget();

    private void HideStats() => _windowView.Hide().Forget();

    public void Dispose() => _disposables.Dispose();
}
