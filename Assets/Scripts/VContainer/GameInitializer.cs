using Cysharp.Threading.Tasks;
using VContainer.Unity;
using Void2610.UnityTemplate;

public class GameInitializer : IStartable
{
    private readonly AreaPresenter _areaPresenter;
    private readonly AreaModel _areaModel;
    private readonly DialoguePresenter _dialoguePresenter;
    private readonly TutorialPresenter _tutorialPresenter;

    public GameInitializer(StatsPresenter statsPresenter, ClonePresenter clonePresenter, ShopPresenter shopPresenter,
        AreaPresenter areaPresenter, EffectPresenter effectPresenter, DialoguePresenter dialoguePresenter,
        TutorialPresenter tutorialPresenter, GameStatsPresenter gameStatsPresenter, SaveService saveService,
        UnityroomScoreService unityroomScoreService, SettingsPresenter settingsPresenter, AreaModel areaModel)
    {
        _areaPresenter = areaPresenter;
        _areaModel = areaModel;
        _dialoguePresenter = dialoguePresenter;
        _tutorialPresenter = tutorialPresenter;
    }

    public void Start()
    {
        InitializeAsync().Forget();
        WaitAndPlayBgm().Forget();
    } 
    
    private async UniTask InitializeAsync()
    {
        // セーブデータから復元したエリアの環境を設定
        _areaPresenter.InitializeEnvironment();

        var fadeImage = UnityEngine.Object.FindFirstObjectByType<FadeImageView>();
        await fadeImage.FadeOut();

        await _dialoguePresenter.ShowCurrentAreaDialogue();

        // 初回プレイ時は強制完了モードでチュートリアルを表示
        if (_areaModel.CurrentAreaIndex.CurrentValue == 0)
            _tutorialPresenter.ShowTutorial(true);
    }

    private async UniTask WaitAndPlayBgm()
    {
        await UniTask.WaitUntil(() => CriBgmController.Instance.IsInitialized);
        _areaPresenter.PlayRandomBgm();
    }
}