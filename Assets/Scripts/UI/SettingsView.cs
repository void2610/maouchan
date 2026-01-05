using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;
using UnityEngine.UI;
using R3;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class SettingsView : MonoBehaviour
{
    [Header("UIコンポーネント")]
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private Slider seVolumeSlider;
    [SerializeField] private Button returnToTitleButton;
    [SerializeField] private Button closeButton;

    [Header("スライド設定")]
    [SerializeField] private float hiddenOffsetX = -800f;

    private const float SLIDE_DURATION = 0.35f;

    private RectTransform _rectTransform;
    private CanvasGroup _canvasGroup;
    private Vector2 _shownPosition;
    private MotionHandle _motionHandle;

    public R3.Observable<float> BgmVolumeChanged => bgmVolumeSlider.onValueChanged.AsObservable();
    public R3.Observable<float> SeVolumeChanged => seVolumeSlider.onValueChanged.AsObservable();
    public R3.Observable<Unit> CloseButtonClicked => closeButton.OnClickAsObservable();
    public R3.Observable<Unit> ReturnToTitleButtonClicked => returnToTitleButton.OnClickAsObservable();
    
    public void SetBgmVolume(float volume) => bgmVolumeSlider.value = volume;
    public void SetSeVolume(float volume) => seVolumeSlider.value = volume;

    public async UniTask Show()
    {
        _motionHandle.TryCancel();
        gameObject.SetActive(true);
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;

        DoFController.Instance.Focus();
        _motionHandle = LMotion.Create(GetHiddenPosition(), _shownPosition, SLIDE_DURATION)
            .WithEase(Ease.OutCubic)
            .BindToAnchoredPosition(_rectTransform);

        await _motionHandle.ToUniTask();

        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
    }

    public async UniTask Hide()
    {
        _motionHandle.TryCancel();
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;

        DoFController.Instance.Defocus();
        _motionHandle = LMotion.Create(_rectTransform.anchoredPosition, GetHiddenPosition(), SLIDE_DURATION)
            .WithEase(Ease.InCubic)
            .BindToAnchoredPosition(_rectTransform);

        await _motionHandle.ToUniTask();

        gameObject.SetActive(false);
    }

    private Vector2 GetHiddenPosition() => new(_shownPosition.x + hiddenOffsetX, _shownPosition.y);
    
    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
        _shownPosition = _rectTransform.anchoredPosition;

        // 初期状態は非表示
        _rectTransform.anchoredPosition = GetHiddenPosition();
        gameObject.SetActive(false);
    }

}
