using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using UnityEngine;
using Void2610.UnityTemplate;

namespace Game.Animation
{
    public class CloneAnimator : IDisposable
    {
        private const float JUMP_DURATION = 0.4f;
        private const float STRETCH_AMOUNT = 0.15f;
        private const float SQUASH_AMOUNT = 0.2f;
        private const float LANDING_DURATION = 0.15f;

        // コトコト移動の設定
        private const float SHUFFLE_MOVE_DISTANCE = 0.3f;
        private const float SHUFFLE_TILT_ANGLE = 15f;
        private const float SHUFFLE_DURATION = 0.5f;
        private const float SHUFFLE_MIN_INTERVAL = 1f;
        private const float SHUFFLE_MAX_INTERVAL = 4f;

        private readonly Vector3 _initialScale;
        private readonly Vector3 _initialPosition;
        private readonly Quaternion _initialRotation;
        private readonly Transform _transform;
        private float _currentRotationZ;

        private MotionHandle _jumpMotion;
        private MotionHandle _scaleMotion;
        private MotionHandle _rotationMotion;
        private MotionHandle _shuffleMotion;
        private MotionHandle _shuffleTiltMotion;
        private bool _isJumping;
        private bool _isShuffling;
        private CancellationTokenSource _shuffleCts;

        public CloneAnimator(Transform t)
        {
            _transform = t;
            _initialScale = _transform.localScale;
            _initialPosition = _transform.localPosition;
            _initialRotation = _transform.localRotation;
        }

        public void StartShuffleLoop() => ShuffleLoopAsync().Forget();

        public async UniTask PushTo(Vector3 targetWorldPosition, float duration)
        {
            // シャッフルをキャンセル
            CancelShuffle();

            _shuffleMotion.TryCancel();

            // ワールド座標で動作
            var startPos = _transform.position;

            _shuffleMotion = LMotion.Create(0f, 1f, duration)
                .WithEase(Ease.OutQuad)
                .Bind(t => _transform.position = Vector3.Lerp(startPos, targetWorldPosition, t));

            await _shuffleMotion.ToUniTask();

            // initialPositionを現在の位置に更新（localPosition基準）
            // ジャンプ時に元の位置に戻る挙動を防ぐ
        }

        public void Jump()
        {
            if (_isJumping) return;
            _isJumping = true;

            // シャッフル中ならキャンセル
            CancelShuffle();

            _jumpMotion.TryCancel();
            _scaleMotion.TryCancel();
            _rotationMotion.TryCancel();

            JumpAsync().Forget();
        }

        private async UniTaskVoid JumpAsync()
        {
            var currentX = _transform.localPosition.x;
            var peakY = _initialPosition.y + 0.5f;

            // ジャンプ中にY軸回転（180度ずつ加算）
            var startRotation = _currentRotationZ;
            var endRotation = _currentRotationZ + 180f;
            _currentRotationZ = endRotation;

            _rotationMotion = LMotion.Create(startRotation, endRotation, JUMP_DURATION)
                .WithEase(Ease.InOutQuad)
                .Bind(angle => _transform.localRotation = _initialRotation * Quaternion.Euler(0, 0, angle));

            // ジャンプ開始時に縦に伸びる
            var stretchScale = new Vector3(
                _initialScale.x * (1f - STRETCH_AMOUNT * 0.5f),
                _initialScale.y * (1f + STRETCH_AMOUNT),
                _initialScale.z
            );

            _scaleMotion = LMotion.Create(_initialScale, stretchScale, JUMP_DURATION * 0.2f)
                .WithEase(Ease.OutQuad)
                .Bind(s => _transform.localScale = s);

            // 上昇（現在のX位置を維持）
            _jumpMotion = LMotion.Create(_initialPosition.y, peakY, JUMP_DURATION * 0.5f)
                .WithEase(Ease.OutQuad)
                .Bind(y => _transform.localPosition = new Vector3(currentX, y, _initialPosition.z));

            await _jumpMotion.ToUniTask();

            // 頂点で通常スケールに戻す
            _scaleMotion = LMotion.Create(_transform.localScale, _initialScale, JUMP_DURATION * 0.2f)
                .WithEase(Ease.Linear)
                .Bind(s => _transform.localScale = s);

            // 落下（元のX位置に戻りながら）
            _jumpMotion = LMotion.Create(peakY, _initialPosition.y, JUMP_DURATION * 0.5f)
                .WithEase(Ease.InQuad)
                .Bind(y => _transform.localPosition = new Vector3(
                    Mathf.Lerp(currentX, _initialPosition.x, (peakY - y) / (peakY - _initialPosition.y)),
                    y,
                    _initialPosition.z));

            await _jumpMotion.ToUniTask();

            await PlayLandingAsync();
        }

        public async UniTask OnSpawned()
        {
            _transform.localScale = Vector3.zero;
            await _transform.ScaleTo(_initialScale, 0.3f, Ease.OutBack);
        }

        public async UniTask OnSpawnedFromAbove(float startY, float targetY)
        {
            // 初期位置を上空に設定、スケールは0から開始
            var pos = _transform.localPosition;
            _transform.localPosition = new Vector3(pos.x, startY, pos.z);
            _transform.localScale = Vector3.zero;

            const float fallDuration = 0.3f;

            // 落下アニメーション
            var fallMotion = LMotion.Create(startY, targetY, fallDuration)
                .WithEase(Ease.InQuad)
                .Bind(y => _transform.localPosition = new Vector3(_transform.localPosition.x, y, _transform.localPosition.z));

            // スケールアニメーション（落下と同時に再生）
            var scaleMotion = LMotion.Create(Vector3.zero, _initialScale, fallDuration)
                .WithEase(Ease.OutBack)
                .Bind(s => _transform.localScale = s);

            // 両方のアニメーションを並行して待機
            await UniTask.WhenAll(fallMotion.ToUniTask(), scaleMotion.ToUniTask());

            // 着地エフェクト
            await PlayLandingAsync();
        }

        public async UniTask OnDestroyed()
        {
            await _transform.ScaleTo(Vector3.zero, 0.2f, Ease.InCirc);
        }
        
        private async UniTask PlayLandingAsync()
        {
            // 着地時に潰れる
            var squashScale = new Vector3(
                _initialScale.x * (1f + SQUASH_AMOUNT),
                _initialScale.y * (1f - SQUASH_AMOUNT),
                _initialScale.z
            );

            _scaleMotion = LMotion.Create(_initialScale, squashScale, LANDING_DURATION * 0.4f)
                .WithEase(Ease.OutQuad)
                .Bind(s => _transform.localScale = s);

            await _scaleMotion.ToUniTask();

            // バウンスなしで元に戻る
            _scaleMotion = LMotion.Create(squashScale, _initialScale, LANDING_DURATION * 0.6f)
                .WithEase(Ease.OutQuad)
                .Bind(s => _transform.localScale = s);

            await _scaleMotion.ToUniTask();

            _isJumping = false;
        }

        private async UniTaskVoid ShuffleLoopAsync()
        {
            _shuffleCts = new CancellationTokenSource();
            var ct = _shuffleCts.Token;

            try
            {
                while (!ct.IsCancellationRequested && _transform)
                {
                    // ランダムな間隔で待機
                    var interval = UnityEngine.Random.Range(SHUFFLE_MIN_INTERVAL, SHUFFLE_MAX_INTERVAL);
                    await UniTask.Delay(TimeSpan.FromSeconds(interval), cancellationToken: ct);

                    if (ct.IsCancellationRequested || _isJumping || !_transform) continue;

                    // コトコト移動を実行
                    await PlayShuffleAsync(ct);
                }
            }
            catch (OperationCanceledException)
            {
                // キャンセルは正常終了
            }
        }

        private async UniTask PlayShuffleAsync(CancellationToken ct)
        {
            if (_isShuffling || _isJumping || !_transform) return;
            _isShuffling = true;

            try
            {
                // 現在位置から開始
                var startX = _transform.localPosition.x;
                var direction = UnityEngine.Random.value > 0.5f ? 1f : -1f;
                var targetX = startX + SHUFFLE_MOVE_DISTANCE * direction;
                var tiltAngle = SHUFFLE_TILT_ANGLE * direction;

                // 傾きながら移動（戻らない）
                _shuffleMotion = LMotion.Create(startX, targetX, SHUFFLE_DURATION)
                    .WithEase(Ease.OutQuad)
                    .Bind(x => _transform.localPosition = new Vector3(x, _transform.localPosition.y, _transform.localPosition.z));

                // 傾き
                _shuffleTiltMotion = LMotion.Create(0f, tiltAngle, SHUFFLE_DURATION * 0.3f)
                    .WithEase(Ease.OutQuad)
                    .Bind(angle => _transform.localRotation = _initialRotation * Quaternion.Euler(0, 0, _currentRotationZ + angle));

                await _shuffleTiltMotion.ToUniTask(ct);

                if (ct.IsCancellationRequested || _isJumping || !_transform) return;

                // 傾きだけ戻す
                _shuffleTiltMotion = LMotion.Create(tiltAngle, 0f, SHUFFLE_DURATION * 0.3f)
                    .WithEase(Ease.OutQuad)
                    .Bind(angle => _transform.localRotation = _initialRotation * Quaternion.Euler(0, 0, _currentRotationZ + angle));

                await UniTask.Delay(TimeSpan.FromSeconds(SHUFFLE_DURATION * 0.4f), cancellationToken: ct);
            }
            catch (OperationCanceledException)
            {
                // キャンセルは正常終了
            }
            finally
            {
                _isShuffling = false;
            }
        }

        private void CancelShuffle()
        {
            _shuffleCts?.Cancel();
            _shuffleCts?.Dispose();
            _shuffleCts = null;

            _shuffleMotion.TryCancel();
            _shuffleTiltMotion.TryCancel();
            _isShuffling = false;

            // 傾きをリセット
            _transform.localRotation = _initialRotation * Quaternion.Euler(0, 0, _currentRotationZ);
        }

        public void Dispose()
        {
            CancelShuffle();
            _jumpMotion.TryCancel();
            _scaleMotion.TryCancel();
            _rotationMotion.TryCancel();
        }
    }
}
