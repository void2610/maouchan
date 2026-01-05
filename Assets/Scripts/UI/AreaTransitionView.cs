using Cysharp.Threading.Tasks;
using LitMotion;
using R3;
using TMPro;
using UnityEngine;
using Void2610.UnityTemplate;

[RequireComponent(typeof(CanvasGroup))]
public class AreaTransitionView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI areaNameText;
    [SerializeField] private EnvironmentView currentEnvironment;
    [SerializeField] private CloneSpawnerView cloneSpawnerView;

    private const float FADE_DURATION = 0.5f;
    private const float DISPLAY_DURATION = 2f;
    private const float SHAKE_MAGNITUDE = 0.3f;
    private const float SHAKE_DURATION = 2f;
    private const float SHAKE_TIME_BEFORE_BREAK = 2.5f;

    // 暗転完了イベント
    public Observable<Unit> OnBlackout => _onBlackout;
    private readonly Subject<Unit> _onBlackout = new();

    private CanvasGroup _canvasGroup;
    private MotionHandle _fadeMotion;

    public async UniTask PlayTransition(string areaName, EnvironmentView environmentPrefab, string newBgmName)
    {
        _fadeMotion.TryCancel();

        // カメラシェイク開始
        CameraShake.Instance.ShakeCamera(SHAKE_MAGNITUDE, SHAKE_DURATION);
        SeManager.Instance.PlaySeWithFadeIn("Rumble", 1f, pitch: 1.0f).Forget();

        // シェイク中に待機
        await UniTask.Delay((int)(SHAKE_TIME_BEFORE_BREAK * 1000));

        // シェイク停止
        CameraShake.Instance.StopShake();

        // 壁破壊
        var wallPosition = currentEnvironment.WallPosition;
        currentEnvironment.BreakWall();
        SeManager.Instance.PlaySe("WallBreak");

        // 壁近くのクローンを押し出す
        cloneSpawnerView.PushClonesFromWall(wallPosition);
        await UniTask.Delay(2000);

        // 成功SE
        SeManager.Instance.PlaySe("Success");
        await UniTask.Delay(1500);

        // フェードアウト（画面を暗く）+ BGMフェードアウト
        _canvasGroup.blocksRaycasts = true;
        _fadeMotion = _canvasGroup.FadeIn(FADE_DURATION, Ease.OutCubic, ignoreTimeScale: true);
        await UniTask.WhenAll(_fadeMotion.ToUniTask(), CriBgmController.Instance.FadeOut());
        await UniTask.Delay(500, ignoreTimeScale: true);

        // 暗転完了イベント発火（UI/エフェクト更新用）
        _onBlackout.OnNext(Unit.Default);

        // 環境を新しいPrefabに入れ替え
        ReplaceEnvironment(environmentPrefab);

        // テキスト表示
        areaNameText.text = $"~ {areaName} ~";
        areaNameText.alpha = 1f;

        // 表示待機
        await UniTask.Delay((int)(DISPLAY_DURATION * 1000), ignoreTimeScale: true);

        // フェードイン（画面を明るく）+ 新BGM再生
        CriBgmController.Instance.PlayBgm(newBgmName);
        _fadeMotion = _canvasGroup.FadeOut(FADE_DURATION, Ease.OutCubic, ignoreTimeScale: true);
        areaNameText.alpha = 0f;
        await _fadeMotion;
        _canvasGroup.blocksRaycasts = false;
    }

    public void ReplaceEnvironment(EnvironmentView prefab)
    {
        var position = currentEnvironment.transform.position;
        var rotation = currentEnvironment.transform.rotation;
        var parent = currentEnvironment.transform.parent;

        Destroy(currentEnvironment.gameObject);
        currentEnvironment = Instantiate(prefab, position, rotation, parent);
    }

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();

        // 初期状態は非表示
        _canvasGroup.alpha = 0f;
        _canvasGroup.blocksRaycasts = false;
        areaNameText.alpha = 0f;
    }

    private void OnDestroy()
    {
        _fadeMotion.TryCancel();
        _onBlackout.Dispose();
        if (currentEnvironment)
        {
            Destroy(currentEnvironment.gameObject);
        }
    }
}
