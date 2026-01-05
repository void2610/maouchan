using R3;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmWindowView : WindowBase
{
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    public Observable<Unit> YesButtonClicked => yesButton.OnClickAsObservable();
    public Observable<Unit> NoButtonClicked => noButton.OnClickAsObservable();
}
