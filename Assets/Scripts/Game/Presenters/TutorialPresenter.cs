using System;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using VContainer.Unity;

public class TutorialPresenter : IDisposable
{
    private readonly TutorialView _tutorialView;
    private readonly CompositeDisposable _disposables = new();

    public TutorialPresenter()
    {
        _tutorialView = UnityEngine.Object.FindFirstObjectByType<TutorialView>();
        _tutorialView.OnCloseButtonClicked.Subscribe(_ => OnTutorialClosed()).AddTo(_disposables);
        
        var tutorialButtonView = UnityEngine.Object.FindFirstObjectByType<TutorialButtonView>();
        tutorialButtonView.OnTutorialButtonClicked.Subscribe(_ => ShowTutorial()).AddTo(_disposables);
    }

    public void ShowTutorial(bool forceComplete = false)
    {
        _tutorialView.SetForceCompleteMode(forceComplete);
        _tutorialView.Show().Forget();
    }

    private void OnTutorialClosed()
    {
        _tutorialView.Hide().Forget();
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}
