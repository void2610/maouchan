using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using R3;

public class TutorialView : WindowBase
{
    [SerializeField] private Button closeButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button prevButton;
    [SerializeField] private Transform pageContainer;
    [SerializeField] private List<GameObject> pagePrefabs;

    private readonly Subject<Unit> _onCloseButtonClicked = new();
    private readonly List<GameObject> _pageInstances = new();
    private int _currentPageIndex;
    private bool _forceComplete;

    public Observable<Unit> OnCloseButtonClicked => _onCloseButtonClicked;

    protected override void Awake()
    {
        base.Awake();

        // ページを生成
        foreach (var prefab in pagePrefabs)
        {
            var page = Instantiate(prefab, pageContainer);
            page.SetActive(false);
            _pageInstances.Add(page);
        }

        closeButton.OnClickAsObservable().Subscribe(_ => _onCloseButtonClicked.OnNext(Unit.Default)).AddTo(this);
        nextButton.OnClickAsObservable().Subscribe(_ => ShowPage(_currentPageIndex + 1)).AddTo(this);
        prevButton.OnClickAsObservable().Subscribe(_ => ShowPage(_currentPageIndex - 1)).AddTo(this);

        ShowPage(0);
    }

    public void SetForceCompleteMode(bool force)
    {
        _forceComplete = force;
        UpdateCloseButtonVisibility();
    }

    private void ShowPage(int index)
    {
        _currentPageIndex = Mathf.Clamp(index, 0, _pageInstances.Count - 1);

        for (var i = 0; i < _pageInstances.Count; i++)
            _pageInstances[i].SetActive(i == _currentPageIndex);

        prevButton.gameObject.SetActive(_currentPageIndex > 0);
        nextButton.gameObject.SetActive(_currentPageIndex < _pageInstances.Count - 1);
        UpdateCloseButtonVisibility();
    }

    private void UpdateCloseButtonVisibility()
    {
        // 強制モードでは最後のページでのみ閉じるボタンを表示
        var isLastPage = _currentPageIndex >= _pageInstances.Count - 1;
        closeButton.gameObject.SetActive(!_forceComplete || isLastPage);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _onCloseButtonClicked.Dispose();
    }
}
