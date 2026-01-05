using System;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using VContainer.Unity;

public class DialoguePresenter : IDisposable
{
    private readonly AreaModel _areaModel;
    private readonly DialogueSettings _dialogueSettings;
    private readonly DialogueView _dialogueView;
    private readonly CompositeDisposable _disposables = new();

    public DialoguePresenter(AreaModel areaModel, DialogueSettings dialogueSettings)
    {
        _areaModel = areaModel;
        _dialogueSettings = dialogueSettings;
        _dialogueView = UnityEngine.Object.FindFirstObjectByType<DialogueView>();

        // エリア遷移演出完了後にセリフ表示
        _areaModel.OnAreaTransitionCompleted.Subscribe(_ => ShowCurrentAreaDialogue().Forget()).AddTo(_disposables);
    }

    public async UniTask ShowCurrentAreaDialogue()
    {
        var dialogues = _dialogueSettings.GetDialogues(_areaModel.CurrentAreaIndex.CurrentValue);
        await _dialogueView.ShowDialogues(dialogues);
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}
