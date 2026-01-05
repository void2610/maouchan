using System;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine.SceneManagement;
using Void2610.UnityTemplate;

public class SettingsPresenter : IDisposable
{
    private readonly SettingsView _settingsView;
    private readonly FadeImageView _fadeView;
    private readonly CompositeDisposable _disposables = new();

    public SettingsPresenter()
    {
        _settingsView = UnityEngine.Object.FindFirstObjectByType<SettingsView>();
        _fadeView = UnityEngine.Object.FindFirstObjectByType<FadeImageView>();
        var settingsButtonView = UnityEngine.Object.FindFirstObjectByType<SettingsButtonView>();

        _settingsView.BgmVolumeChanged.Subscribe(OnBgmVolumeChanged).AddTo(_disposables);
        _settingsView.SeVolumeChanged.Subscribe(OnSeVolumeChanged).AddTo(_disposables);
        _settingsView.CloseButtonClicked.Subscribe(_ => OnCloseButtonClicked()).AddTo(_disposables);
        _settingsView.ReturnToTitleButtonClicked.Subscribe(_ => OnReturnToTitleButtonClicked().Forget()).AddTo(_disposables);
        settingsButtonView.OnSettingsButtonClicked.Subscribe(_ => Show()).AddTo(_disposables);

        LoadVolume();
    }

    public void Show() => _settingsView.Show().Forget();

    private void OnBgmVolumeChanged(float volume) => CriBgmController.Instance.BgmVolume = volume;

    private void OnSeVolumeChanged(float volume) => SeManager.Instance.SeVolume = volume;

    private void OnCloseButtonClicked() => _settingsView.Hide().Forget();

    private async UniTaskVoid OnReturnToTitleButtonClicked()
    {
        // 画面フェードとBGMフェードを並列実行
        await UniTask.WhenAll(
            _fadeView.FadeIn(),
            CriBgmController.Instance.FadeOut());
        SceneManager.LoadScene("TitleScene");
    }

    private void LoadVolume()
    {
        var bgmVolume = CriBgmController.Instance.BgmVolume;
        var seVolume = SeManager.Instance.SeVolume;

        _settingsView.SetBgmVolume(bgmVolume);
        _settingsView.SetSeVolume(seVolume);
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}
