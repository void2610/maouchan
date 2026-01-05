using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using UnityEngine;

public class PlayerView : MonoBehaviour
{
    [Header("Body Animation")]
    [SerializeField] private float verticalAmplitude = 0.15f;
    [SerializeField] private float verticalSpeed = 2f;
    [SerializeField] private float horizontalAmplitude = 0.05f;
    [SerializeField] private float horizontalSpeed = 1.2f;

    [Header("Wing Animation")]
    [SerializeField] private Transform leftWing;
    [SerializeField] private Transform rightWing;
    [SerializeField] private float wingRotationAmplitude = 15f;
    [SerializeField] private float wingSpeed = 8f;

    [Header("Click Texture")]
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Texture2D normalTexture;
    [SerializeField] private Texture2D clickTexture;
    [SerializeField] private float clickTextureDuration = 0.2f;
    [SerializeField] private float spinDuration = 0.15f;

    private static readonly int _baseMap = Shader.PropertyToID("_BaseMap");

    private Vector3 _initialPosition;
    private Quaternion _initialRotation;
    private Quaternion _leftWingInitialRotation;
    private Quaternion _rightWingInitialRotation;
    private float _timeOffset;
    private float _clickTextureTimer;
    private CancellationTokenSource _spinCts;
    private bool _isClickTexture;

    public void OnClick()
    {
        _clickTextureTimer = clickTextureDuration;

        // 既にクリックテクスチャ中ならタイマーリセットのみ
        if (_isClickTexture) return;

        SpinAndChangeTexture(clickTexture, true);
    }

    private void UpdateBodyAnimation()
    {
        var time = Time.time + _timeOffset;

        // 上下のふわふわ
        var verticalOffset = Mathf.Sin(time * verticalSpeed) * verticalAmplitude;
        // 左右の微小な揺れ
        var horizontalOffset = Mathf.Sin(time * horizontalSpeed) * horizontalAmplitude;

        transform.localPosition = _initialPosition + new Vector3(horizontalOffset, verticalOffset, 0f);

        // 羽のパタパタアニメーション
        var wingAngle = Mathf.Sin(time * wingSpeed) * wingRotationAmplitude;
        leftWing.localRotation = _leftWingInitialRotation * Quaternion.Euler(0f, wingAngle, 0f);
        rightWing.localRotation = _rightWingInitialRotation * Quaternion.Euler(0f, -wingAngle, 0f);
    }

    private void UpdateClickTexture()
    {
        if (_clickTextureTimer <= 0f) return;

        _clickTextureTimer -= Time.deltaTime;
        if (_clickTextureTimer <= 0f)
        {
            // 時間経過で元に戻る時は回転なし
            SetTexture(normalTexture);
            _isClickTexture = false;
        }
    }

    private void SpinAndChangeTexture(Texture2D texture, bool toClickTexture)
    {
        // 既存のアニメーションをキャンセルして新しいトークンを作成
        _spinCts.Cancel();
        _spinCts.Dispose();
        _spinCts = new CancellationTokenSource();

        SpinAndChangeTextureAsync(texture, toClickTexture, _spinCts.Token).Forget();
    }

    private async UniTaskVoid SpinAndChangeTextureAsync(Texture2D texture, bool toClickTexture, CancellationToken ct)
    {
        // 回転を初期位置にリセットしてから開始
        transform.localRotation = _initialRotation;

        var halfDuration = spinDuration * 0.5f;

        try
        {
            // 前半: 0→180度
            await LMotion.Create(0f, 180f, halfDuration)
                .WithEase(Ease.InQuad)
                .Bind(angle => transform.localRotation = _initialRotation * Quaternion.Euler(0f, 0f, angle))
                .ToUniTask(ct);

            // 180度でテクスチャ切り替え
            SetTexture(texture);
            _isClickTexture = toClickTexture;

            // 後半: 180→360度
            await LMotion.Create(180f, 360f, halfDuration)
                .WithEase(Ease.OutQuad)
                .Bind(angle => transform.localRotation = _initialRotation * Quaternion.Euler(0f, 0f, angle))
                .ToUniTask(ct);

            transform.localRotation = _initialRotation;
        }
        catch (System.OperationCanceledException)
        {
            // キャンセル時は回転を初期位置に戻す
            transform.localRotation = _initialRotation;
        }
    }

    private void SetTexture(Texture2D texture) => targetRenderer.material.SetTexture(_baseMap, texture);

    private void Awake()
    {
        _initialPosition = transform.localPosition;
        _initialRotation = transform.localRotation;
        _leftWingInitialRotation = leftWing.localRotation;
        _rightWingInitialRotation = rightWing.localRotation;
        _spinCts = new CancellationTokenSource();
        // ランダムな開始位相でアニメーションが単調にならないようにする
        _timeOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    private void Update()
    {
        UpdateBodyAnimation();
        UpdateClickTexture();
    }

    private void OnDestroy()
    {
        _spinCts.Cancel();
        _spinCts.Dispose();
    }
}
