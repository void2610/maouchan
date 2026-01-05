using UnityEngine;
using UnityEngine.UI;
using TMPro;
using R3;

public class CreditView : WindowBase
{
    [SerializeField] private Button iconButton1;
    [SerializeField] private string linkString1;
    [SerializeField] private Button iconButton2;
    [SerializeField] private string linkString2;
    [SerializeField] private Button iconButton3;
    [SerializeField] private string linkString3;
    [SerializeField] private TextMeshProUGUI creditText;
    [SerializeField] private Button closeButton;

    public Observable<Unit> CloseButtonClicked => closeButton.OnClickAsObservable();

    protected override void Awake()
    {
        base.Awake();
        
        iconButton1.OnClickAsObservable().Subscribe(_ => Application.OpenURL(linkString1)).AddTo(this);
        iconButton2.OnClickAsObservable().Subscribe(_ => Application.OpenURL(linkString2)).AddTo(this);
        iconButton3.OnClickAsObservable().Subscribe(_ => Application.OpenURL(linkString3)).AddTo(this);
    }
}
