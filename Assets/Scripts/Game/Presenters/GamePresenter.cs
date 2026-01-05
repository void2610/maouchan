using System;
using UnityEngine;
using R3;
using VContainer.Unity;

public class GamePresenter : ITickable, IDisposable
{
    private readonly GameModel _gameModel;
    private readonly PlayerView _playerView;
    private readonly CompositeDisposable _disposables = new();

    public GamePresenter(GameModel gameModel)
    {
        _gameModel = gameModel;
        _playerView = UnityEngine.Object.FindFirstObjectByType<PlayerView>();
        var clickView = UnityEngine.Object.FindFirstObjectByType<ClickView>();
        clickView.OnClicked.Subscribe(_ => _gameModel.OnClick()).AddTo(_disposables);

        // クローン生成時にPlayerViewのアニメーション発火
        _gameModel.OnClickClonesGenerated.Subscribe(_ => _playerView.OnClick()).AddTo(_disposables);
    }

    public void Tick()
    {
        _gameModel.AddDpsPoints(Time.deltaTime);

        // デバッグ用: Spaceキーでクローンを100体追加
        if (Application.isEditor && Input.GetKeyDown(KeyCode.Space))
        {
            _gameModel.AddClones(100);
        }
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}
