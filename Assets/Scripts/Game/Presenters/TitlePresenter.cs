using System;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine.SceneManagement;
using VContainer.Unity;
using Void2610.UnityTemplate;

public class TitlePresenter : IStartable, IDisposable
{
    private readonly TitleView _titleView;
    private readonly CreditView _creditView;
    private readonly LicenseView _licenseView;
    private readonly ConfirmWindowView _confirmWindowView;
    private readonly FadeImageView _fadeView;
    private readonly CompositeDisposable _disposables = new();

    public TitlePresenter(LicenseService licenseService)
    {
        // FindFirstObjectByTypeでViewを取得
        _titleView = UnityEngine.Object.FindFirstObjectByType<TitleView>();
        _creditView = UnityEngine.Object.FindFirstObjectByType<CreditView>();
        _licenseView = UnityEngine.Object.FindFirstObjectByType<LicenseView>();
        _confirmWindowView = UnityEngine.Object.FindFirstObjectByType<ConfirmWindowView>();
        _fadeView = UnityEngine.Object.FindFirstObjectByType<FadeImageView>();

        _licenseView.SetLicenseText(licenseService.GetLicenseText());
    }

    public void Start()
    {
        // セーブデータの有無で続きからボタンの表示を切り替え
        _titleView.SetContinueButtonVisible(SaveService.HasSaveData());

        // ボタンイベントの購読
        _titleView.NewGameButtonClicked.Subscribe(_ => OnNewGameButtonClicked().Forget()).AddTo(_disposables);
        _titleView.ContinueButtonClicked.Subscribe(_ => OnContinueButtonClicked().Forget()).AddTo(_disposables);
        _titleView.LicenseButtonClicked.Subscribe(_ => OnLicenseButtonClicked()).AddTo(_disposables);
        _titleView.CreditButtonClicked.Subscribe(_ => OnCreditButtonClicked()).AddTo(_disposables);

        // 音量設定（SE）
        _titleView.SeVolumeChanged.Subscribe(v => SeManager.Instance.SeVolume = v).AddTo(_disposables);
        _titleView.SetSeVolume(SeManager.Instance.SeVolume);

        // クレジット/ライセンス画面の閉じるボタンイベントの購読
        _creditView.CloseButtonClicked.Subscribe(_ => _creditView.Hide().Forget()).AddTo(_disposables);
        _licenseView.CloseButtonClicked.Subscribe(_ => _licenseView.Hide().Forget()).AddTo(_disposables);

        // 確認ウィンドウのボタンイベントの購読
        _confirmWindowView.YesButtonClicked.Subscribe(_ => OnConfirmYesClicked().Forget()).AddTo(_disposables);
        _confirmWindowView.NoButtonClicked.Subscribe(_ => _confirmWindowView.Hide().Forget()).AddTo(_disposables);

        _fadeView.FadeOut().Forget();
        InitializeBgmAsync().Forget();
    }

    private async UniTask InitializeBgmAsync()
    {
        await UniTask.WaitUntil(() => CriBgmController.Instance.IsInitialized);

        // BGM音量設定（初期化後に設定）
        _titleView.BgmVolumeChanged.Subscribe(v => CriBgmController.Instance.BgmVolume = v).AddTo(_disposables);
        _titleView.SetBgmVolume(CriBgmController.Instance.BgmVolume);

        CriBgmController.Instance.PlayBgm("Title");
    }

    private async UniTask OnNewGameButtonClicked()
    {
        // セーブデータがある場合は確認ウィンドウを表示
        if (SaveService.HasSaveData())
        {
            await _confirmWindowView.Show();
            return;
        }

        await StartNewGame();
    }

    private async UniTask OnConfirmYesClicked()
    {
        await _confirmWindowView.Hide();
        await StartNewGame();
    }

    private async UniTask StartNewGame()
    {
        // セーブデータをリセットしてからメインシーンへ
        SaveService.ResetSave();
        SaveService.ShouldLoadSave = false;
        // 画面フェードとBGMフェードを並列実行
        await UniTask.WhenAll(
            _fadeView.FadeIn(),
            CriBgmController.Instance.FadeOut());
        SceneManager.LoadScene("MainScene");
    }

    private async UniTask OnContinueButtonClicked()
    {
        // セーブデータを読み込んでメインシーンへ
        SaveService.ShouldLoadSave = true;
        // 画面フェードとBGMフェードを並列実行
        await UniTask.WhenAll(
            _fadeView.FadeIn(),
            CriBgmController.Instance.FadeOut());
        SceneManager.LoadScene("MainScene");
    }

    private void OnLicenseButtonClicked() => _licenseView.Show().Forget();

    private void OnCreditButtonClicked() => _creditView.Show().Forget();

    public void Dispose() => _disposables.Dispose();
}
