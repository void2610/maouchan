using UnityEngine;
using VContainer;
using VContainer.Unity;

public class MainLifetimeScope : LifetimeScope
{
    [Header("Settings")]
    [SerializeField] private GameBalanceSettings gameBalanceSettings;
    [SerializeField] private ClickUpgradeSettings clickUpgradeSettings;
    [SerializeField] private GeneratorSettings generatorSettings;
    [SerializeField] private AreaSettings areaSettings;
    [SerializeField] private DialogueSettings dialogueSettings;
    [SerializeField] private EffectSettings effectSettings;

    [Header("Debug")]
    [SerializeField] private bool resetSaveOnStart;

    protected override void Configure(IContainerBuilder builder)
    {
        // デバッグ用：セーブデータリセット
        if (resetSaveOnStart && Application.isEditor)
            SaveService.ResetSave();

        // Settings
        builder.RegisterInstance(gameBalanceSettings);
        builder.RegisterInstance(clickUpgradeSettings);
        builder.RegisterInstance(generatorSettings);
        builder.RegisterInstance(areaSettings);
        builder.RegisterInstance(dialogueSettings);
        builder.RegisterInstance(effectSettings);

        // Model
        builder.Register<GameModel>(Lifetime.Singleton);
        builder.Register<UpgradeModel>(Lifetime.Singleton);
        builder.Register<AreaModel>(Lifetime.Singleton);
        builder.Register<GameStatsModel>(Lifetime.Singleton);

        // Service
        builder.Register<SaveService>(Lifetime.Singleton).AsSelf();
        builder.Register<UnityroomScoreService>(Lifetime.Singleton).AsSelf();

        // Presenter
        builder.Register<StatsPresenter>(Lifetime.Singleton);
        builder.Register<ClonePresenter>(Lifetime.Singleton);
        builder.Register<ShopPresenter>(Lifetime.Singleton);
        builder.Register<AreaPresenter>(Lifetime.Singleton);
        builder.Register<EffectPresenter>(Lifetime.Singleton);
        builder.Register<DialoguePresenter>(Lifetime.Singleton);
        builder.Register<TutorialPresenter>(Lifetime.Singleton);
        builder.Register<SettingsPresenter>(Lifetime.Singleton);
        builder.Register<GameStatsPresenter>(Lifetime.Singleton);

        // EntryPoint
        builder.RegisterEntryPoint<GamePresenter>();
        builder.RegisterEntryPoint<GameInitializer>();
        builder.RegisterEntryPoint<ClearScreenPresenter>();
    }
}
