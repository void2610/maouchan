using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Void2610.UnityTemplate;

[RequireComponent(typeof(CanvasGroup))]
public class DialogueView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Button tapArea;
    [SerializeField] private RectTransform characterImage;
    [SerializeField] private Sprite defaultCharacterSprite;

    private const float FADE_DURATION = 0.3f;

    private CanvasGroup _canvasGroup;
    private MotionHandle _fadeMotion;
    private DialogueCharacterAnimator _characterAnimator;
    private Image _characterImageComponent;

    public async UniTask ShowDialogues(List<DialogueEntry> dialogues)
    {
        dialogueText.text = "";
        gameObject.SetActive(true);
        _canvasGroup.blocksRaycasts = true;

        // フェードイン + DoFフォーカス
        DoFController.Instance.Focus();
        _fadeMotion.TryCancel();
        _fadeMotion = _canvasGroup.FadeIn(FADE_DURATION, Ease.OutCubic);
        await _fadeMotion;

        // 各セリフを順番に表示
        foreach (var entry in dialogues)
        {
            if (string.IsNullOrEmpty(entry.text)) continue;

            // キャラクター画像の切り替え（nullの場合はデフォルト画像を使用）
            _characterImageComponent.sprite = entry.characterSprite
                ? entry.characterSprite
                : defaultCharacterSprite;

            // セリフごとにアニメーション切り替え
            _characterAnimator.Play(entry.animationType);

            // 会話SEをループ再生
            var dialogueSeCts = new CancellationTokenSource();
            SeManager.Instance.PlaySeLoop("Dialogue", interval: 0.18f, cancellationToken: dialogueSeCts.Token).Forget();

            var wasSkipped = await dialogueText.TypewriterAnimation(entry.text, skipOnClick: true);

            // 会話SE停止
            dialogueSeCts.Cancel();
            dialogueSeCts.Dispose();
            // スキップされた場合は1フレーム待って同じクリックがOnClickAsyncに伝わるのを防ぐ
            if (wasSkipped)
                await UniTask.Yield();

            await tapArea.OnClickAsync();

            _characterAnimator.Stop();
        }

        // フェードアウト + DoFデフォーカス
        DoFController.Instance.Defocus();
        _fadeMotion.TryCancel();
        _fadeMotion = _canvasGroup.FadeOut(FADE_DURATION, Ease.OutCubic);
        await _fadeMotion;

        _canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _characterAnimator = new DialogueCharacterAnimator(characterImage);
        _characterImageComponent = characterImage.GetComponent<Image>();

        _canvasGroup.alpha = 1f;
        _canvasGroup.blocksRaycasts = false;
    }

    private void OnDestroy()
    {
        _fadeMotion.TryCancel();
        _characterAnimator.Dispose();
    }
}
