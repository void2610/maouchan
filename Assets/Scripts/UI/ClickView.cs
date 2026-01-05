using R3;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ClickView : MonoBehaviour
{
    [SerializeField] private Button clickButton;

    public Observable<Unit> OnClicked => clickButton.OnClickAsObservable();
}
