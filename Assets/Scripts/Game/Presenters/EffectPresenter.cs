using System;
using R3;
using UnityEngine.Rendering.Universal;
using Void2610.UnityTemplate;

public class EffectPresenter : IDisposable
{
    public float CurrentProgressRatio { get; private set; }

    private readonly GameModel _gameModel;
    private readonly AreaModel _areaModel;
    private readonly EffectSettings _effectSettings;
    private readonly PostEffectManager _postEffectManager;
    private readonly CompositeDisposable _disposables = new();

    public EffectPresenter(GameModel gameModel, AreaModel areaModel, EffectSettings effectSettings)
    {
        _gameModel = gameModel;
        _areaModel = areaModel;
        _effectSettings = effectSettings;
        _postEffectManager = PostEffectManager.Instance;

        // ポイント変更時に進捗率を更新して演出を適用
        _gameModel.Points.Subscribe(OnPointsChanged).AddTo(_disposables);
        // 必要クローン数変更時に進捗率をリセット（暗転後に更新される）
        _areaModel.RequiredClones.Subscribe(_ => OnAreaAdvanced()).AddTo(_disposables);
    }

    private void OnPointsChanged(long currentPoints)
    {
        CurrentProgressRatio = _areaModel.GetProgressRatio(currentPoints);
        ApplyEffects(CurrentProgressRatio);
    }

    private void OnAreaAdvanced()
    {
        // エリア解放後、進捗率をリセットして演出を更新
        CurrentProgressRatio = _areaModel.GetProgressRatio(_gameModel.CurrentPoints);
        ApplyEffects(CurrentProgressRatio);
    }

    private void ApplyEffects(float progress)
    {
        // カーブを使用して非線形の変化を適用
        var whiteBalanceValue = _effectSettings.EvaluateWhiteBalance(progress);
        var cameraShakeValue = _effectSettings.EvaluateCameraShake(progress);

        _postEffectManager.SetEffect<WhiteBalance>(wb => wb.tint.value = whiteBalanceValue);
        ((GameCameraController)CameraShake.Instance).SetContinuousMagnitude(cameraShakeValue);
    }

    public void Dispose() => _disposables.Dispose();
}
