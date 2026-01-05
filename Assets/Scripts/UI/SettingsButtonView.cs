using UnityEngine;
using UnityEngine.UI;
using R3;

public class SettingsButtonView : MonoBehaviour
{
    [SerializeField] private Button settingsButton;

    private readonly Subject<Unit> _onSettingsButtonClicked = new();

    public Observable<Unit> OnSettingsButtonClicked => _onSettingsButtonClicked;

    private void Awake()
    {
        settingsButton.OnClickAsObservable()
            .Subscribe(_ => _onSettingsButtonClicked.OnNext(Unit.Default))
            .AddTo(this);
    }

    private void OnDestroy()
    {
        _onSettingsButtonClicked.Dispose();
    }
}
