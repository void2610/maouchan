using UnityEngine;

[CreateAssetMenu(fileName = "EffectSettings", menuName = "Settings/EffectSettings")]
public class EffectSettings : ScriptableObject
{
    [Header("ホワイトバランス（色調）")]
    [SerializeField] private AnimationCurve whiteBalanceCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    [SerializeField] private float whiteBalanceMin;
    [SerializeField] private float whiteBalanceMax = 40f;

    [Header("カメラシェイク")]
    [SerializeField] private AnimationCurve cameraShakeCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    [SerializeField] private float cameraShakeMin;
    [SerializeField] private float cameraShakeMax = 0.05f;

    // カーブを評価して最終値を返す
    public float EvaluateWhiteBalance(float progress) =>
        Mathf.Lerp(whiteBalanceMin, whiteBalanceMax, whiteBalanceCurve.Evaluate(progress));

    public float EvaluateCameraShake(float progress) =>
        Mathf.Lerp(cameraShakeMin, cameraShakeMax, cameraShakeCurve.Evaluate(progress));
}
