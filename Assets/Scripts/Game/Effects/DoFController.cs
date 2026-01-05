using LitMotion;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Void2610.UnityTemplate;

/// <summary>
/// DepthOfField操作を担当するコントローラー
/// SettingsViewやDialogueViewなど、複数のUIから共通で使用
/// </summary>
public class DoFController : SingletonMonoBehaviour<DoFController>
{
    [SerializeField] private Volume settingsVolume;

    private const float DEFAULT_DURATION = 0.3f;
    private const float FOCUS_VALUE = 10f;
    private const float DEFOCUS_VALUE = 32f;

    private DepthOfField _depthOfField;
    private MotionHandle _motionHandle;

    protected override void Awake()
    {
        IsDontDestroyOnLoad = false;
        base.Awake();
        settingsVolume.profile.TryGet(out _depthOfField);
    }

    public void Focus(float duration = DEFAULT_DURATION)
    {
        _motionHandle.TryCancel();
        _motionHandle = LMotion.Create(_depthOfField.aperture.value, FOCUS_VALUE, duration)
            .WithEase(Ease.OutCubic)
            .Bind(v => _depthOfField.aperture.value = v);
    }

    public void Defocus(float duration = DEFAULT_DURATION)
    {
        _motionHandle.TryCancel();
        _motionHandle = LMotion.Create(_depthOfField.aperture.value, DEFOCUS_VALUE, duration)
            .WithEase(Ease.InCubic)
            .Bind(v => _depthOfField.aperture.value = v);
    }
}
