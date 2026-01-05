using System;
using R3;

public class GameStatsModel : IDisposable
{
    public ReadOnlyReactiveProperty<long> TotalClickCount => _totalClickCount;
    public ReadOnlyReactiveProperty<long> TotalClonesFromClick => _totalClonesFromClick;
    public ReadOnlyReactiveProperty<long> TotalClonesFromDps => _totalClonesFromDps;
    public ReadOnlyReactiveProperty<long> TotalClonesConsumed => _totalClonesConsumed;
    public ReadOnlyReactiveProperty<float> TotalPlayTimeSeconds => _totalPlayTimeSeconds;

    public long CurrentTotalClickCount => _totalClickCount.Value;
    public long CurrentTotalClonesFromClick => _totalClonesFromClick.Value;
    public long CurrentTotalClonesFromDps => _totalClonesFromDps.Value;
    public long CurrentTotalClonesConsumed => _totalClonesConsumed.Value;
    public float CurrentTotalPlayTimeSeconds => _totalPlayTimeSeconds.Value;

    private readonly ReactiveProperty<long> _totalClickCount = new(0);
    private readonly ReactiveProperty<long> _totalClonesFromClick = new(0);
    private readonly ReactiveProperty<long> _totalClonesFromDps = new(0);
    private readonly ReactiveProperty<long> _totalClonesConsumed = new(0);
    private readonly ReactiveProperty<float> _totalPlayTimeSeconds = new(0f);

    private readonly CompositeDisposable _disposables = new();

    public GameStatsModel(GameModel gameModel)
    {
        // クリック回数
        gameModel.OnClicked
            .Subscribe(_ => _totalClickCount.Value++)
            .AddTo(_disposables);

        // クリック由来のクローン生成
        gameModel.OnClickClonesGenerated
            .Subscribe(count => _totalClonesFromClick.Value += count)
            .AddTo(_disposables);

        // DPS由来のクローン生成
        gameModel.OnClonesGenerated
            .Subscribe(count => _totalClonesFromDps.Value += count)
            .AddTo(_disposables);

        // クローン消費
        gameModel.OnClonesConsumed
            .Subscribe(amount => _totalClonesConsumed.Value += amount)
            .AddTo(_disposables);
    }

    public void AddPlayTime(float deltaTime) => _totalPlayTimeSeconds.Value += deltaTime;

    public void SetStats(long clickCount, long clonesFromClick, long clonesFromDps,
        long clonesConsumed, float playTime)
    {
        _totalClickCount.Value = clickCount;
        _totalClonesFromClick.Value = clonesFromClick;
        _totalClonesFromDps.Value = clonesFromDps;
        _totalClonesConsumed.Value = clonesConsumed;
        _totalPlayTimeSeconds.Value = playTime;
    }

    public void Dispose()
    {
        _disposables.Dispose();
        _totalClickCount.Dispose();
        _totalClonesFromClick.Dispose();
        _totalClonesFromDps.Dispose();
        _totalClonesConsumed.Dispose();
        _totalPlayTimeSeconds.Dispose();
    }
}
