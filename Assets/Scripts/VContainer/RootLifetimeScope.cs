using UnityEngine;
using VContainer;
using VContainer.Unity;
using Void2610.UnityTemplate;

public class RootLifetimeScope : LifetimeScope
{
    [SerializeField] private GameObject criWareInitializerPrefab;
    [SerializeField] private CriBgmController bgmManager;
    [SerializeField] private SeManager seManager;

    protected override void Configure(IContainerBuilder builder)
    {
        Instantiate(criWareInitializerPrefab);
        Instantiate(bgmManager);
        Instantiate(seManager);
    }
}
