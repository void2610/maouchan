using System;
using R3;
using VContainer.Unity;

public class StatsPresenter : IDisposable
{
    private readonly AreaModel _areaModel;
    private readonly StatsView _statsView;
    private readonly CompositeDisposable _disposables = new();

    public StatsPresenter(GameModel gameModel, AreaModel areaModel)
    {
        var gameModel1 = gameModel;
        _areaModel = areaModel;
        _statsView = UnityEngine.Object.FindFirstObjectByType<StatsView>();

        // クローン数変更時にUI更新
        gameModel1.Points.Subscribe(points =>
            {
                _statsView.UpdateCloneCount(points);
                UpdateAreaProgress(points);
            }).AddTo(_disposables);
        // DPS変更時にUI更新
        gameModel1.Dps.Subscribe(_statsView.UpdateDps).AddTo(_disposables);
        // 必要クローン数変更時にUI更新（暗転後に更新される）
        _areaModel.RequiredClones.Subscribe(_ => UpdateAreaProgress(gameModel1.CurrentPoints)).AddTo(_disposables);
    }

    private void UpdateAreaProgress(long currentClones)
    {
        var requiredClones = _areaModel.RequiredClones.CurrentValue;
        var nextAreaName = _areaModel.GetNextAreaName();
        _statsView.UpdateAreaProgress(currentClones, requiredClones, nextAreaName);
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}
