using UnityEngine;
using UnityEngine.UI;
using R3;

public class TutorialButtonView : MonoBehaviour
{
    [SerializeField] private Button tutorialButton;

    private readonly Subject<Unit> _onTutorialButtonClicked = new();

    public Observable<Unit> OnTutorialButtonClicked => _onTutorialButtonClicked;

    private void Awake()
    {
        tutorialButton.OnClickAsObservable()
            .Subscribe(_ => _onTutorialButtonClicked.OnNext(Unit.Default))
            .AddTo(this);
    }

    private void OnDestroy()
    {
        _onTutorialButtonClicked.Dispose();
    }
}
