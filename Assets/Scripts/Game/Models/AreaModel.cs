using System;
using R3;

public class AreaModel : IDisposable
{
    public ReadOnlyReactiveProperty<int> CurrentAreaIndex => _currentAreaIndex;
    public ReadOnlyReactiveProperty<long> RequiredClones => _requiredClones;
    public Observable<string> OnAreaAdvanced => _onAreaAdvanced;
    public Observable<Unit> OnAreaTransitionCompleted => _onAreaTransitionCompleted;
    public bool IsMaxArea => _currentAreaIndex.Value >= _areaSettings.Count - 1;

    private readonly AreaSettings _areaSettings;
    private readonly ReactiveProperty<int> _currentAreaIndex;
    private readonly ReactiveProperty<long> _requiredClones;
    private readonly Subject<string> _onAreaAdvanced = new();
    private readonly Subject<Unit> _onAreaTransitionCompleted = new();

    public AreaModel(AreaSettings areaSettings)
    {
        _areaSettings = areaSettings;
        _currentAreaIndex = new ReactiveProperty<int>(0);
        _requiredClones = new ReactiveProperty<long>(_areaSettings.GetRequiredClones(0));
    }

    public EnvironmentView GetCurrentEnvironmentPrefab() => _areaSettings.GetEnvironmentPrefab(_currentAreaIndex.Value);
    public bool CanAdvanceArea(long currentClones) => !IsMaxArea && currentClones >= _requiredClones.Value;
    public float GetProgressRatio(long currentClones) => IsMaxArea ? 1f : UnityEngine.Mathf.Clamp01((float)currentClones / _requiredClones.Value);
    public void UpdateRequiredClones() => _requiredClones.Value = _areaSettings.GetRequiredClones(_currentAreaIndex.Value);
    public void NotifyTransitionCompleted() => _onAreaTransitionCompleted.OnNext(Unit.Default);

    public string GetNextAreaName()
    {
        var nextIndex = _currentAreaIndex.Value + 1;
        // 最後のエリアの場合は空文字を返す
        return nextIndex < _areaSettings.Count ? _areaSettings.GetAreaName(nextIndex) : "";
    }

    public bool TryAdvanceArea(long currentClones)
    {
        if (IsMaxArea) return false;
        if (currentClones < _requiredClones.Value) return false;

        _currentAreaIndex.Value++;

        // エリア解放イベント発火（暗転演出開始のトリガー）
        // 必要クローン数は暗転後にUpdateRequiredClonesで更新する
        _onAreaAdvanced.OnNext(_areaSettings.GetAreaName(_currentAreaIndex.Value));

        return true;
    }

    // セーブデータからエリアインデックスを復元
    public void SetAreaIndex(int index)
    {
        _currentAreaIndex.Value = index;
        _requiredClones.Value = _areaSettings.GetRequiredClones(index);
    }

    public void Dispose()
    {
        _currentAreaIndex.Dispose();
        _requiredClones.Dispose();
        _onAreaAdvanced.Dispose();
        _onAreaTransitionCompleted.Dispose();
    }
}
