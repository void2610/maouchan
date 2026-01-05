using UnityEngine;
using Void2610.UnityTemplate;

/// <summary>
/// ゲーム専用のカメラコントローラー
/// CameraShakeを継承し、継続的な揺れとマウス追従を追加
/// </summary>
public class GameCameraController : CameraShake
{
    [Header("Continuous Shake")]
    [SerializeField] private float continuousMagnitude = 0.05f;
    [SerializeField] private float noiseSpeed = 10f;

    [Header("Mouse Follow")]
    [SerializeField] private float mouseFollowStrength = 2f;

    private Vector3 _basePosition;
    
    public void SetContinuousMagnitude(float magnitude) => continuousMagnitude = magnitude;

    protected override void Awake()
    {
        base.Awake();
        _basePosition = transform.position;
    }

    private void Update()
    {
        // ShakeCamera中はスキップ
        if (IsShaking()) return;

        var offset = Vector3.zero;

        // Perlin Noiseによる継続的な揺れ
        if (continuousMagnitude > 0f)
        {
            var time = Time.time * noiseSpeed;
            var offsetX = (Mathf.PerlinNoise(time, 0f) - 0.5f) * 2f * continuousMagnitude;
            var offsetY = (Mathf.PerlinNoise(0f, time) - 0.5f) * 2f * continuousMagnitude;
            offset += new Vector3(offsetX, offsetY, 0f);
        }

        // マウス位置による追従
        if (mouseFollowStrength > 0f)
        {
            var mousePos = Input.mousePosition;
            var normalizedX = (mousePos.x / Screen.width) - 0.5f;
            var normalizedY = (mousePos.y / Screen.height) - 0.5f;
            offset += new Vector3(normalizedX, normalizedY, 0f) * mouseFollowStrength;
        }

        transform.position = _basePosition + offset;
    }
}
