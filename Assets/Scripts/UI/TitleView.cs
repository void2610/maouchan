using UnityEngine;
using UnityEngine.UI;
using R3;

public class TitleView : MonoBehaviour
{
    [Header("UIコンポーネント")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button licenseButton;
    [SerializeField] private Button creditButton;

    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private Slider seVolumeSlider;

    // ボタンクリックイベントをObservableとして公開
    public Observable<Unit> NewGameButtonClicked => newGameButton.OnClickAsObservable();
    public Observable<Unit> ContinueButtonClicked => continueButton.OnClickAsObservable();
    public Observable<Unit> LicenseButtonClicked => licenseButton.OnClickAsObservable();
    public Observable<Unit> CreditButtonClicked => creditButton.OnClickAsObservable();

    // 音量変更イベント
    public Observable<float> BgmVolumeChanged => bgmVolumeSlider.onValueChanged.AsObservable();
    public Observable<float> SeVolumeChanged => seVolumeSlider.onValueChanged.AsObservable();

    public void SetContinueButtonVisible(bool visible) => continueButton.gameObject.SetActive(visible);

    public void SetBgmVolume(float volume) => bgmVolumeSlider.value = volume;
    public void SetSeVolume(float volume) => seVolumeSlider.value = volume;
}
