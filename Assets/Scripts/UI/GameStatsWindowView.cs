using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using R3;

public class GameStatsWindowView : WindowBase
{
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI statsText;

    private readonly Subject<Unit> _onCloseButtonClicked = new();

    private long _totalClickCount;
    private long _totalClonesFromClick;
    private long _totalClonesFromDps;
    private long _totalClonesConsumed;
    private float _basePlayTimeSeconds;
    private float _sessionStartTime;

    public Observable<Unit> OnCloseButtonClicked => _onCloseButtonClicked;
    public void UpdateTotalClickCount(long count) => _totalClickCount = count;
    public void UpdateTotalClonesFromClick(long count) => _totalClonesFromClick = count;
    public void UpdateTotalClonesFromDps(long count) => _totalClonesFromDps = count;
    public void UpdateTotalClonesConsumed(long count) => _totalClonesConsumed = count;

    public void UpdateTotalPlayTime(float seconds)
    {
        _basePlayTimeSeconds = seconds;
        // セッション開始時間をリセットして二重カウントを防ぐ
        _sessionStartTime = Time.realtimeSinceStartup;
    }

    protected override void Awake()
    {
        base.Awake();
        _sessionStartTime = Time.realtimeSinceStartup;
        closeButton.OnClickAsObservable()
            .Subscribe(_ => _onCloseButtonClicked.OnNext(Unit.Default))
            .AddTo(this);
    }

    private void Update()
    {
        var currentPlayTime = _basePlayTimeSeconds + (Time.realtimeSinceStartup - _sessionStartTime);
        var timeSpan = TimeSpan.FromSeconds(currentPlayTime);
        statsText.text = $"総クリック回数: {_totalClickCount:N0}\n" +
                         $"クリック生成分身: {_totalClonesFromClick:N0}\n" +
                         $"自動生成分身: {_totalClonesFromDps:N0}\n" +
                         $"累計消費分身: {_totalClonesConsumed:N0}\n" +
                         $"総プレイ時間: {timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _onCloseButtonClicked.Dispose();
    }
}
