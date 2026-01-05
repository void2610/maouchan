using System;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using VContainer.Unity;
using Void2610.UnityTemplate;

public class ClearScreenPresenter : IDisposable, ITickable
{
    private readonly GameModel _gameModel;
    private readonly GameStatsModel _statsModel;
    private readonly AreaModel _areaModel;
    private readonly SaveService _saveService;
    private readonly GameBalanceSettings _settings;
    private readonly ClearScreenView _clearScreenView;
    private readonly CompositeDisposable _disposables = new();

    public ClearScreenPresenter(
        GameModel gameModel,
        GameStatsModel statsModel,
        AreaModel areaModel,
        SaveService saveService,
        GameBalanceSettings settings)
    {
        _gameModel = gameModel;
        _statsModel = statsModel;
        _areaModel = areaModel;
        _saveService = saveService;
        _settings = settings;
        _clearScreenView = UnityEngine.Object.FindFirstObjectByType<ClearScreenView>();

        // エリア遷移演出完了後にクリア画面表示を試行
        _areaModel.OnAreaTransitionCompleted
            .Subscribe(_ => TryShowClearScreen())
            .AddTo(_disposables);

        // ツイートボタン
        _clearScreenView.OnTweetButtonClicked
            .Subscribe(_ => OnTweetButtonClicked())
            .AddTo(_disposables);
    }

    private void TryShowClearScreen()
    {
        if (!_areaModel.IsMaxArea) return;
        ShowClearScreen();
    }

    private void ShowClearScreen()
    {
        // プレイ時間を確定
        _saveService.FlushPlayTime();

        // 統計情報を設定
        _clearScreenView.SetStats(
            _gameModel.CurrentPoints,
            _statsModel.CurrentTotalClickCount,
            _statsModel.CurrentTotalPlayTimeSeconds
        );

        _clearScreenView.Show().Forget();
    }

    private void OnTweetButtonClicked()
    {
        OnTweetButtonClickedAsync().Forget();
    }

    private async UniTaskVoid OnTweetButtonClickedAsync()
    {
        var timeSpan = TimeSpan.FromSeconds(_statsModel.CurrentTotalPlayTimeSeconds);
        var timeText = $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";

        var tweetText = $"#がんばれまおうちゃん で世界をピンクに染め上げました！\n" +
                        $"分身数: {_gameModel.CurrentPoints:N0}\n" +
                        $"クリック数: {_statsModel.CurrentTotalClickCount:N0}\n" +
                        $"クリアタイム: {timeText}\n\n" +
                        $"#unityroom #unity1week";

        await TweetService.OpenTweetWithScreenshotAsync(tweetText, _settings.ImgBBApiKey);
    }

    public void Tick()
    {
#if UNITY_EDITOR
        // デバッグ用: Cキーでクリア画面を表示
        if (Input.GetKeyDown(KeyCode.C))
        {
            ShowClearScreen();
        }
#endif
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}
