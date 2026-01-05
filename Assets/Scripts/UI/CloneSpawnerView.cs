using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.VFX;
using Void2610.UnityTemplate;
using Random = UnityEngine.Random;

public class CloneSpawnerView : MonoBehaviour
{
    [Header("Clone Prefabs")]
    [SerializeField] private CloneView clonePrefab;
    [SerializeField] private CloneView clone10Prefab;
    [SerializeField] private CloneView clone100Prefab;

    [Header("Spawn Settings")]
    [SerializeField] private Vector3 spawnAreaSize = new(5f, 0f, 5f);
    [Header("Merge Settings")]
    [SerializeField] private int maxCloneCount = 200;

    [Header("References")]
    [SerializeField] private PlayerView playerView;

    private readonly List<CloneView> _clones = new();

    public void SpawnClones(int count) => SpawnClonesInternal(count, fromAbove: false);

    public void SpawnClonesFromAbove(int count) => SpawnClonesInternal(count, fromAbove: true);

    public void RestoreClones(long totalCount)
    {
        // 既存のクローンをクリア
        foreach (var clone in _clones)
        {
            if (clone)
            {
                clone.StopAllAnimations();
                Destroy(clone.gameObject);
            }
        }
        _clones.Clear();

        RestoreClonesInternal(totalCount);
    }

    private void SpawnClonesInternal(int count, bool fromAbove)
    {
        for (var i = 0; i < count; i++)
            SpawnCloneWithWeight(clonePrefab, 1, fromAbove);

        // 200を超えたらマージ
        TryMergeClones();
    }

    private void SpawnCloneWithWeight(CloneView prefab, int weight, bool fromAbove, bool playSe = true)
    {
        var pos = GetRandomSpawnPosition();
        var clone = Instantiate(prefab, pos, Quaternion.identity, transform);
        clone.SetWeight(weight);

        if (fromAbove)
        {
            // クリック生成: プレイヤーからクローンへのエフェクト
            clone.SetFallMode(1.5f);
            ParticleManager.Instance.PlaySpawnEffectWithPath(playerView.transform.position, pos);
        }
        else
        {
            // 自動生成: スポーンエフェクト
            ParticleManager.Instance.PlaySpawnEffect(pos);
        }

        // クローン生成SE再生（復元時は鳴らさない）
        if (playSe)
            SeManager.Instance.PlaySe("CloneSpawn");
        _clones.Add(clone);
    }

    public void SyncClonesToPoints(long currentPoints)
    {
        // 現在のクローン総重量を計算
        var currentWeight = _clones.Sum(c => c.Weight);

        // 差分がなければ何もしない
        if (currentWeight == currentPoints) return;

        // 消費分だけクローンを削除（小さいクローンから優先的に削除）
        var toRemove = currentWeight - currentPoints;
        while (toRemove > 0 && _clones.Count > 0)
        {
            // 重みが小さい順にソートして削除
            var smallestClone = _clones.OrderBy(c => c.Weight).First();
            _clones.Remove(smallestClone);
            toRemove -= smallestClone.Weight;
            smallestClone.StopAllAnimations();
            smallestClone.PlayAnimationAndDestroy().Forget();
        }
    }

    private void RestoreClonesInternal(long totalCount)
    {
        // maxCloneCount以下ならすべて通常クローンで生成
        if (totalCount <= maxCloneCount)
        {
            for (var i = 0; i < totalCount; i++)
                SpawnCloneWithWeight(clonePrefab, 1, fromAbove: false, playSe: false);
            return;
        }

        // maxCloneCountを超える場合は最小限のマージで復元
        // できるだけ多くのクローンオブジェクトを残す
        var excessCount = totalCount - maxCloneCount;
        var clone100Count = 0L;
        var clone10Count = 0L;
        var clone1Count = totalCount;

        // clone10にマージすると9体削減（10体→1体）
        // clone100にマージすると99体削減（100体→1体）
        // まずclone100で大きく削減
        if (excessCount >= 99)
        {
            clone100Count = (excessCount / 99) + 1;
            // clone100で表現できる最大体数を超えないように調整
            var maxClone100 = totalCount / 100;
            if (clone100Count > maxClone100) clone100Count = maxClone100;
            clone1Count -= clone100Count * 100;
            excessCount = clone1Count - (maxCloneCount - clone100Count);
        }

        // 残りの超過分をclone10で削減
        if (excessCount > 0)
        {
            clone10Count = (excessCount / 9) + 1;
            var maxClone10 = clone1Count / 10;
            if (clone10Count > maxClone10) clone10Count = maxClone10;
            clone1Count -= clone10Count * 10;
        }

        // 生成（復元時はSE再生しない）
        for (var i = 0; i < clone100Count; i++)
            SpawnCloneWithWeight(clone100Prefab, 100, fromAbove: false, playSe: false);
        for (var i = 0; i < clone10Count; i++)
            SpawnCloneWithWeight(clone10Prefab, 10, fromAbove: false, playSe: false);
        for (var i = 0; i < clone1Count; i++)
            SpawnCloneWithWeight(clonePrefab, 1, fromAbove: false, playSe: false);
    }

    private void TryMergeClones()
    {
        // 最大存在数以下なら合成しない
        if (_clones.Count <= maxCloneCount) return;

        // 超過分だけマージ（最小限のマージでできるだけ多くのクローンを残す）
        // clone10にマージすると9体削減（10体→1体）
        var normalClones = _clones.Where(c => c.Weight == 1).ToList();
        while (_clones.Count > maxCloneCount && normalClones.Count >= 10)
        {
            MergeClonesIntoLarger(normalClones, 10, clone10Prefab, 10);
            normalClones = _clones.Where(c => c.Weight == 1).ToList();
        }

        // それでも超過していればclone100にマージ（99体削減）
        var clone10S = _clones.Where(c => c.Weight == 10).ToList();
        while (_clones.Count > maxCloneCount && clone10S.Count >= 10)
        {
            MergeClonesIntoLarger(clone10S, 10, clone100Prefab, 100);
            clone10S = _clones.Where(c => c.Weight == 10).ToList();
        }
    }

    private void MergeClonesIntoLarger(List<CloneView> sources, int mergeCount, CloneView prefab, int newWeight)
    {
        // 指定数を削除
        for (var i = 0; i < mergeCount && sources.Count > 0; i++)
        {
            var clone = sources[0];
            sources.RemoveAt(0);
            _clones.Remove(clone);
            Destroy(clone.gameObject);
        }

        // 1体の大きなクローンを生成
        var pos = GetRandomSpawnPosition();
        var newClone = Instantiate(prefab, pos, Quaternion.identity, transform);
        newClone.SetWeight(newWeight);
        _clones.Add(newClone);
    }

    public void PushClonesFromWall(Vector3 wallPosition)
    {
        const float radius = 6f;
        const float pushForce = 5f;

        foreach (var clone in _clones)
        {
            if (!clone) continue;
            var clonePos = clone.transform.position;
            var distance = Mathf.Abs(clonePos.x - wallPosition.x);
            if (distance > radius) continue;

            // 距離に応じて押し出し量を調整（近いほど強く）
            var targetPos = clonePos - Vector3.right * pushForce * (1 - (radius / distance));
            clone.PushTo(targetPos, 0.75f).Forget();
        }
    }

    private Vector3 GetRandomSpawnPosition()
    {
        var halfSize = spawnAreaSize * 0.5f;

        return new Vector3(
            transform.position.x + Random.Range(-halfSize.x, halfSize.x),
            transform.position.y,
            transform.position.z + Random.Range(-halfSize.z, halfSize.z)
        );
    }
}
