using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using UnityEngine;

public enum CharacterAnimationType
{
    None,
    Nod,      // 上下に小さく動く（同意、理解）
    HeadTilt, // 首を傾ける（疑問、興味）
    Jolt,     // 上にピクッと跳ねる（驚き、ビックリ）
    JoltLoop, // 上にピクッと跳ねる（ループ）
    Shake,    // 横に小刻みに揺れる（否定、拒否）
    Shiver    // 細かく震える（恐怖、寒さ）
}

public class DialogueCharacterAnimator : IDisposable
{
    // アニメーションパラメータ
    private const float NOD_DURATION = 0.3f;
    private const float NOD_DISTANCE = 20f;

    private const float HEAD_TILT_DURATION = 0.25f;
    private const float HEAD_TILT_ANGLE = 10f;

    private const float JOLT_DURATION = 0.25f;
    private const float JOLT_DISTANCE = 50f;
    private const float JOLT_LOOP_INTERVAL = 0.2f;

    private const float SHAKE_DURATION = 0.08f;
    private const float SHAKE_DISTANCE = 8f;
    private const int SHAKE_COUNT = 4;

    private const float SHIVER_DURATION = 0.05f;
    private const float SHIVER_DISTANCE = 5f;
    private const int SHIVER_COUNT = 8;

    private readonly RectTransform _rectTransform;
    private readonly Vector3 _initialScale;
    private readonly Vector2 _initialPosition;
    private readonly Quaternion _initialRotation;

    private MotionHandle _motionHandle;
    private CancellationTokenSource _loopCts;

    public DialogueCharacterAnimator(RectTransform rectTransform)
    {
        _rectTransform = rectTransform;
        _initialScale = _rectTransform.localScale;
        _initialPosition = _rectTransform.anchoredPosition;
        _initialRotation = _rectTransform.localRotation;
    }

    public void Play(CharacterAnimationType animationType)
    {
        Stop();

        switch (animationType)
        {
            case CharacterAnimationType.None:
                break;
            case CharacterAnimationType.Nod:
                PlayNod().Forget();
                break;
            case CharacterAnimationType.HeadTilt:
                PlayHeadTilt().Forget();
                break;
            case CharacterAnimationType.Jolt:
                PlayJolt().Forget();
                break;
            case CharacterAnimationType.JoltLoop:
                _loopCts = new CancellationTokenSource();
                PlayJoltLoop(_loopCts.Token).Forget();
                break;
            case CharacterAnimationType.Shake:
                PlayShake().Forget();
                break;
            case CharacterAnimationType.Shiver:
                _loopCts = new CancellationTokenSource();
                PlayShiverLoop(_loopCts.Token).Forget();
                break;
        }
    }

    public void Stop()
    {
        _loopCts?.Cancel();
        _loopCts?.Dispose();
        _loopCts = null;

        _motionHandle.TryCancel();

        if (_rectTransform)
        {
            _rectTransform.localScale = _initialScale;
            _rectTransform.anchoredPosition = _initialPosition;
            _rectTransform.localRotation = _initialRotation;
        }
    }

    // 上下に小さく動く（同意、理解）
    private async UniTaskVoid PlayNod()
    {
        var downPos = _initialPosition + new Vector2(0, -NOD_DISTANCE);

        // 下に動く
        _motionHandle.TryCancel();
        _motionHandle = LMotion.Create(_initialPosition, downPos, NOD_DURATION * 0.4f)
            .WithEase(Ease.OutQuad)
            .Bind(p => _rectTransform.anchoredPosition = p);
        await _motionHandle.ToUniTask();

        // 戻る
        _motionHandle.TryCancel();
        _motionHandle = LMotion.Create(downPos, _initialPosition, NOD_DURATION * 0.6f)
            .WithEase(Ease.OutQuad)
            .Bind(p => _rectTransform.anchoredPosition = p);
        await _motionHandle.ToUniTask();
    }

    // 首を傾ける（疑問、興味）
    private async UniTaskVoid PlayHeadTilt()
    {
        var tiltRotation = _initialRotation * Quaternion.Euler(0, 0, HEAD_TILT_ANGLE);

        // 傾ける
        _motionHandle.TryCancel();
        _motionHandle = LMotion.Create(_initialRotation, tiltRotation, HEAD_TILT_DURATION * 0.5f)
            .WithEase(Ease.OutQuad)
            .Bind(r => _rectTransform.localRotation = r);
        await _motionHandle.ToUniTask();

        // 戻る
        _motionHandle.TryCancel();
        _motionHandle = LMotion.Create(tiltRotation, _initialRotation, HEAD_TILT_DURATION * 0.5f)
            .WithEase(Ease.OutQuad)
            .Bind(r => _rectTransform.localRotation = r);
        await _motionHandle.ToUniTask();
    }

    // 上にピクッと跳ねる（驚き、ビックリ）
    private async UniTaskVoid PlayJolt() => await PlayJoltCycle();

    // 上にピクッと跳ねる（ループ）
    private async UniTaskVoid PlayJoltLoop(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested && _rectTransform)
            {
                await PlayJoltCycle(ct);
                await UniTask.Delay(TimeSpan.FromSeconds(JOLT_LOOP_INTERVAL), cancellationToken: ct);
            }
        }
        catch (OperationCanceledException)
        {
            // キャンセルは正常終了
        }
    }

    private async UniTask PlayJoltCycle(CancellationToken ct = default)
    {
        var upPos = _initialPosition + new Vector2(0, JOLT_DISTANCE);

        // 上に跳ねる
        _motionHandle.TryCancel();
        _motionHandle = LMotion.Create(_initialPosition, upPos, JOLT_DURATION * 0.4f)
            .WithEase(Ease.OutQuad)
            .Bind(p => _rectTransform.anchoredPosition = p);
        await _motionHandle.ToUniTask(ct);

        // 戻る
        _motionHandle.TryCancel();
        _motionHandle = LMotion.Create(upPos, _initialPosition, JOLT_DURATION * 0.6f)
            .WithEase(Ease.OutBounce)
            .Bind(p => _rectTransform.anchoredPosition = p);
        await _motionHandle.ToUniTask(ct);
    }

    // 横に小刻みに揺れる（否定、拒否）
    private async UniTaskVoid PlayShake()
    {
        for (var i = 0; i < SHAKE_COUNT; i++)
        {
            // 右に動く
            var rightPos = _initialPosition + new Vector2(SHAKE_DISTANCE, 0);
            _motionHandle.TryCancel();
            _motionHandle = LMotion.Create(_initialPosition, rightPos, SHAKE_DURATION * 0.5f)
                .WithEase(Ease.OutQuad)
                .Bind(p => _rectTransform.anchoredPosition = p);
            await _motionHandle.ToUniTask();

            // 左に動く
            var leftPos = _initialPosition + new Vector2(-SHAKE_DISTANCE, 0);
            _motionHandle.TryCancel();
            _motionHandle = LMotion.Create(rightPos, leftPos, SHAKE_DURATION)
                .WithEase(Ease.InOutQuad)
                .Bind(p => _rectTransform.anchoredPosition = p);
            await _motionHandle.ToUniTask();

            // 中央に戻る
            _motionHandle.TryCancel();
            _motionHandle = LMotion.Create(leftPos, _initialPosition, SHAKE_DURATION * 0.5f)
                .WithEase(Ease.OutQuad)
                .Bind(p => _rectTransform.anchoredPosition = p);
            await _motionHandle.ToUniTask();
        }
    }

    // 細かく震える（恐怖、寒さ）- ループ
    private async UniTaskVoid PlayShiverLoop(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested && _rectTransform)
            {
                await PlayShiverCycle(ct);
            }
        }
        catch (OperationCanceledException)
        {
            // キャンセルは正常終了
        }
    }

    private async UniTask PlayShiverCycle(CancellationToken ct)
    {
        for (var i = 0; i < SHIVER_COUNT; i++)
        {
            ct.ThrowIfCancellationRequested();

            // ランダムな方向に小さく動く
            var randomOffset = new Vector2(
                UnityEngine.Random.Range(-SHIVER_DISTANCE, SHIVER_DISTANCE),
                UnityEngine.Random.Range(-SHIVER_DISTANCE, SHIVER_DISTANCE)
            );
            var targetPos = _initialPosition + randomOffset;

            _motionHandle.TryCancel();
            _motionHandle = LMotion.Create(_rectTransform.anchoredPosition, targetPos, SHIVER_DURATION)
                .WithEase(Ease.OutQuad)
                .Bind(p => _rectTransform.anchoredPosition = p);
            await _motionHandle.ToUniTask(ct);
        }

        // 中央に戻る
        _motionHandle.TryCancel();
        _motionHandle = LMotion.Create(_rectTransform.anchoredPosition, _initialPosition, SHIVER_DURATION)
            .WithEase(Ease.OutQuad)
            .Bind(p => _rectTransform.anchoredPosition = p);
        await _motionHandle.ToUniTask(ct);
    }

    public void Dispose() => Stop();
}
