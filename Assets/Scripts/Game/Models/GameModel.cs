using System;
using R3;

public class GameModel : IDisposable
{
    public ReadOnlyReactiveProperty<long> Points => _points;
    public ReadOnlyReactiveProperty<double> ClickPower => _clickPower;
    public ReadOnlyReactiveProperty<double> Dps => _dps;

    // クローン生成・削除用イベント
    public Observable<int> OnClonesGenerated => _onClonesGenerated;
    public Observable<int> OnClickClonesGenerated => _onClickClonesGenerated;
    public Observable<long> OnClonesConsumed => _onClonesConsumed;
    public Observable<long> OnClonesRestored => _onClonesRestored;
    public Observable<Unit> OnClicked => _onClicked;

    public long CurrentPoints => _points.Value;
    public double CurrentClickPower => _clickPower.Value;
    public double CurrentDps => _dps.Value;

    private readonly ReactiveProperty<long> _points;
    private readonly ReactiveProperty<double> _clickPower;
    private readonly ReactiveProperty<double> _dps;
    private readonly Subject<int> _onClonesGenerated = new();
    private readonly Subject<int> _onClickClonesGenerated = new();
    private readonly Subject<long> _onClonesConsumed = new();
    private readonly Subject<long> _onClonesRestored = new();
    private readonly Subject<Unit> _onClicked = new();

    // DPSによる小数クローンの蓄積
    private double _dpsAccumulator;
    // クリックによる小数クローンの蓄積
    private double _clickAccumulator;
    
    public void SetClickPower(double value) => _clickPower.Value = value;
    public void SetDps(double value) => _dps.Value = value;

    public void SetPoints(long value)
    {
        _points.Value = value;
        // ロード時にクローンオブジェクトを復元するためのイベント発火
        if (value > 0)
        {
            _onClonesRestored.OnNext(value);
        }
    }

    public GameModel(GameBalanceSettings settings)
    {
        _points = new ReactiveProperty<long>(settings.InitialPoints);
        _clickPower = new ReactiveProperty<double>(settings.BaseClickPower);
        _dps = new ReactiveProperty<double>(settings.BaseDps);
    }

    public void OnClick()
    {
        _onClicked.OnNext(Unit.Default);
        _clickAccumulator += _clickPower.Value;

        // 整数分のクローンを生成
        var wholePart = (long)_clickAccumulator;
        if (wholePart > 0)
        {
            _points.Value += wholePart;
            _clickAccumulator -= wholePart;
            _onClickClonesGenerated.OnNext((int)wholePart);
        }
    }

    public void AddClones(int amount)
    {
        _points.Value += amount;
        _onClonesGenerated.OnNext(amount);
    }

    public void AddDpsPoints(float deltaTime)
    {
        if (_dps.Value <= 0) return;

        _dpsAccumulator += _dps.Value * deltaTime;

        // 整数分のクローンを生成
        var wholePart = (int)_dpsAccumulator;
        if (wholePart > 0)
        {
            _points.Value += wholePart;
            _dpsAccumulator -= wholePart;
            _onClonesGenerated.OnNext(wholePart);
        }
    }

    public bool SpendPoints(long amount)
    {
        if (_points.Value < amount) return false;
        
        _points.Value -= amount;
        _onClonesConsumed.OnNext(amount);
        return true;
    }

    public void Dispose()
    {
        _points.Dispose();
        _clickPower.Dispose();
        _dps.Dispose();
        _onClonesGenerated.Dispose();
        _onClickClonesGenerated.Dispose();
        _onClonesConsumed.Dispose();
        _onClonesRestored.Dispose();
        _onClicked.Dispose();
    }
}
