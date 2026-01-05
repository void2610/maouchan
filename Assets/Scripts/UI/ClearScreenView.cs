using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using R3;

public class ClearScreenView : WindowBase
{
    [SerializeField] private TextMeshProUGUI cloneCountText;
    [SerializeField] private TextMeshProUGUI clickCountText;
    [SerializeField] private TextMeshProUGUI clearTimeText;
    [SerializeField] private Button tweetButton;
    [SerializeField] private Button closeButton;

    public Observable<Unit> OnTweetButtonClicked => _onTweetButtonClicked;
    
    private readonly Subject<Unit> _onTweetButtonClicked = new();

    public void SetStats(long cloneCount, long clickCount, float playTimeSeconds)
    {
        cloneCountText.text = $"分身数: {cloneCount:N0}";
        clickCountText.text = $"クリック回数: {clickCount:N0}";

        var timeSpan = TimeSpan.FromSeconds(playTimeSeconds);
        clearTimeText.text = $"クリアタイム: {timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
    }

    protected override void Awake()
    {
        base.Awake();

        tweetButton.OnClickAsObservable().Subscribe(_ => _onTweetButtonClicked.OnNext(Unit.Default)).AddTo(this);
        closeButton.OnClickAsObservable().Subscribe(_ => this.Hide().Forget()).AddTo(this);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _onTweetButtonClicked.Dispose();
    }
}
