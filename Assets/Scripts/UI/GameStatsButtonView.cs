using UnityEngine;
using UnityEngine.UI;
using R3;

public class GameStatsButtonView : MonoBehaviour
{
    [SerializeField] private Button statsButton;

    private readonly Subject<Unit> _onStatsButtonClicked = new();

    public Observable<Unit> OnStatsButtonClicked => _onStatsButtonClicked;

    private void Awake()
    {
        statsButton.OnClickAsObservable()
            .Subscribe(_ => _onStatsButtonClicked.OnNext(Unit.Default))
            .AddTo(this);
    }

    private void OnDestroy() => _onStatsButtonClicked.Dispose();
}
