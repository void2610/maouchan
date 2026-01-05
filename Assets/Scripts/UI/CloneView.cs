using Cysharp.Threading.Tasks;
using Game.Animation;
using UnityEngine;

public class CloneView : MonoBehaviour
{
    private enum State
    {
        Spawning,
        Falling,
        Idle
    }

    private CloneAnimator _cloneAnimator;
    private State _state = State.Spawning;
    private float _fallStartY;

    // このクローンが表す体数（1, 10, 100）
    public int Weight { get; private set; } = 1;

    public void SetWeight(int weight)
    {
        Weight = weight;
    }

    public void StopAllAnimations() => _cloneAnimator?.Dispose();
    public UniTask PushTo(Vector3 targetPosition, float duration) => _cloneAnimator.PushTo(targetPosition, duration);

    public async UniTask PlayAnimationAndDestroy()
    {
        await _cloneAnimator.OnDestroyed();
        Destroy(gameObject);
    }

    public void SetFallMode(float startY)
    {
        _state = State.Falling;
        _fallStartY = startY;
    }

    private async UniTaskVoid InitializeAsync()
    {
        if (_state == State.Falling)
        {
            // 落下アニメーション（着地先のY座標は0固定）
            await _cloneAnimator.OnSpawnedFromAbove(_fallStartY, 0f);
        }
        else
        {
            await _cloneAnimator.OnSpawned();
        }
        _state = State.Idle;
        _cloneAnimator.StartShuffleLoop();
    }

    private void Awake()
    {
        transform.rotation = Quaternion.Euler(new Vector3(90, 180, 0));
        _cloneAnimator = new CloneAnimator(transform);
    }

    private void Start() => InitializeAsync().Forget();

    private void OnMouseEnter()
    {
        if (_state != State.Idle) return;
        _cloneAnimator.Jump();
    }

    private void OnDestroy()
    {
        StopAllAnimations();
    }
}
