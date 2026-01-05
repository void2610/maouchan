using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using Unityroom.Client;

public class UnityroomScoreService : IDisposable
{
    private const int CLONE_COUNT_SCOREBOARD_ID = 2;
    private const int CLEAR_TIME_SCOREBOARD_ID = 1;
    private const float SEND_INTERVAL = 5f;

    private readonly GameModel _gameModel;
    private readonly GameStatsModel _statsModel;
    private readonly AreaModel _areaModel;
    private readonly SaveService _saveService;
    private readonly UnityroomClient _client;
    private readonly CancellationTokenSource _cts = new();
    private readonly CompositeDisposable _disposables = new();

    public UnityroomScoreService(
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
        _client = new UnityroomClient { HmacKey = settings.UnityroomHmacKey };

        // 最終エリア解放時にクリアタイムを送信
        _areaModel.OnAreaAdvanced.Subscribe(_ => TrySendClearTime()).AddTo(_disposables);

        StartAutoSend(_cts.Token).Forget();
    }

    private void TrySendClearTime()
    {
        // 最終エリアに到達した場合のみ送信
        if (!_areaModel.IsMaxArea) return;

        // プレイ時間を確定してから送信
        _saveService.FlushPlayTime();
        SendClearTimeAsync().Forget();
    }

    private async UniTaskVoid SendClearTimeAsync()
    {
        try
        {
            // クリアタイム（秒）を送信
            var clearTime = _statsModel.CurrentTotalPlayTimeSeconds;
            await _client.Scoreboards.SendAsync(new SendScoreRequest
            {
                ScoreboardId = CLEAR_TIME_SCOREBOARD_ID,
                Score = clearTime
            }, CancellationToken.None);
        }
        catch (Exception)
        {
            // ignored
        }
    }

    private async UniTaskVoid StartAutoSend(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(SEND_INTERVAL), cancellationToken: ct);
            await SendScoreAsync(ct);
        }
    }

    private async UniTask SendScoreAsync(CancellationToken ct)
    {
        try
        {
            var score = (float)_gameModel.CurrentPoints;
            await _client.Scoreboards.SendAsync(new SendScoreRequest
            {
                ScoreboardId = CLONE_COUNT_SCOREBOARD_ID,
                Score = score
            }, ct);
        }
        catch (Exception)
        {
            // ignored
        }
    }

    public void Dispose()
    {
        SendScoreAsync(CancellationToken.None).Forget();
        _disposables.Dispose();
        _cts.Cancel();
        _cts.Dispose();
        _client.Dispose();
    }
}
