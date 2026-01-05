using UnityEngine;
using UnityEngine.UI;
using TMPro;
using R3;

public class LicenseView : WindowBase
{
    [Header("UIコンポーネント")]
    [SerializeField] private TextMeshProUGUI licenseText;
    [SerializeField] private Button closeButton;

    public Observable<Unit> CloseButtonClicked => closeButton.OnClickAsObservable();

    public void SetLicenseText(string text) => licenseText.text = text;
}
