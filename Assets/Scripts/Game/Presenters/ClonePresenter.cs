using System;
using R3;
using VContainer.Unity;

public class ClonePresenter : IDisposable
{
    private readonly CompositeDisposable _disposables = new();

    public ClonePresenter(GameModel gameModel)
    {
        var cloneSpawnerView = UnityEngine.Object.FindFirstObjectByType<CloneSpawnerView>();

        // クリック時にクローン生成（上から落下）
        gameModel.OnClickClonesGenerated
            .Subscribe(amount => cloneSpawnerView.SpawnClonesFromAbove(amount))
            .AddTo(_disposables);
        // DPS時にクローン生成（通常スポーン）
        gameModel.OnClonesGenerated
            .Subscribe(amount => cloneSpawnerView.SpawnClones(amount))
            .AddTo(_disposables);
        // ポイント消費時にクローンを同期
        gameModel.OnClonesConsumed
            .Subscribe(_ => cloneSpawnerView.SyncClonesToPoints(gameModel.CurrentPoints))
            .AddTo(_disposables);
        // セーブデータからクローン復元
        gameModel.OnClonesRestored
            .Subscribe(amount => cloneSpawnerView.RestoreClones(amount))
            .AddTo(_disposables);
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}
