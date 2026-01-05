using SyskenTLib.LicenseMaster;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Void2610.UnityTemplate;

public class TitleLifetimeScope : LifetimeScope
{
    [SerializeField] private LicenseManager licenseManager;
    
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<LicenseService>(Lifetime.Singleton).WithParameter("licenseManager", licenseManager);
        builder.RegisterEntryPoint<TitlePresenter>();
    }
}
