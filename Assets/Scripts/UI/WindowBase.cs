using UnityEngine;
using LitMotion;
using Cysharp.Threading.Tasks;
using Void2610.UnityTemplate;

[RequireComponent(typeof(CanvasGroup))]
public abstract class WindowBase : MonoBehaviour
{
    private const float FADE_DURATION = 0.3f;

    private CanvasGroup _canvasGroup;
    private MotionHandle _fadeMotion;

    protected virtual void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();

        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }

    public virtual async UniTask Show()
    {
        _fadeMotion.TryCancel();

        gameObject.SetActive(true);

        _fadeMotion = _canvasGroup.FadeIn(FADE_DURATION, Ease.OutCubic, ignoreTimeScale: true);
        await _fadeMotion;

        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
    }

    public virtual async UniTask Hide()
    {
        _fadeMotion.TryCancel();

        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;

        _fadeMotion = _canvasGroup.FadeOut(FADE_DURATION, Ease.OutCubic, ignoreTimeScale: true);
        await _fadeMotion;

        gameObject.SetActive(false);
    }

    protected virtual void OnDestroy()
    {
        _fadeMotion.TryCancel();
    }
}
