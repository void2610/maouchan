using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Void2610.UnityTemplate;
using R3;

public class SaveService : IDisposable
{
    private const string SAVE_KEY = "clicker_save";
    private const float AUTO_SAVE_INTERVAL = 5f;

    private readonly GameModel _gameModel;
    private readonly UpgradeModel _upgradeModel;
    private readonly GameStatsModel _statsModel;
    private readonly AreaModel _areaModel;
    private readonly CancellationTokenSource _cts = new();

    private float _lastSaveTime;

    // シーン間でセーブを読み込むかどうかを制御するフラグ
    public static bool ShouldLoadSave { get; set; } = true;

    public static bool HasSaveData() => !string.IsNullOrEmpty(DataPersistence.LoadData(SAVE_KEY));

    public static void ResetSave() => DataPersistence.DeleteData(SAVE_KEY);

    public SaveService(
        GameModel gameModel,
        UpgradeModel upgradeModel,
        GameStatsModel statsModel,
        AreaModel areaModel)
    {
        _gameModel = gameModel;
        _upgradeModel = upgradeModel;
        _statsModel = statsModel;
        _areaModel = areaModel;

        // フラグに応じてセーブデータを読み込む
        if (ShouldLoadSave)
            Load();
        ShouldLoadSave = true;

        _lastSaveTime = Time.realtimeSinceStartup;

        // セーブの自動化
        StartAutoSave(_cts.Token).Forget();
        _areaModel.OnAreaAdvanced.Subscribe(_ => Save());
    }

    // 現在のセッション時間を確定してプレイ時間に加算
    public void FlushPlayTime()
    {
        var currentTime = Time.realtimeSinceStartup;
        var delta = currentTime - _lastSaveTime;
        if (delta > 0)
        {
            _statsModel.AddPlayTime(delta);
        }
        _lastSaveTime = currentTime;
    }

    private void Save()
    {
        FlushPlayTime();

        var data = new SaveData
        {
            points = _gameModel.CurrentPoints,
            clickLevel = _upgradeModel.CurrentClickLevel.CurrentValue,
            generatorCounts = _upgradeModel.GeneratorCounts.CurrentValue,
            areaIndex = _areaModel.CurrentAreaIndex.CurrentValue,
            // 統計情報
            totalClickCount = _statsModel.CurrentTotalClickCount,
            totalClonesFromClick = _statsModel.CurrentTotalClonesFromClick,
            totalClonesFromDps = _statsModel.CurrentTotalClonesFromDps,
            totalClonesConsumed = _statsModel.CurrentTotalClonesConsumed,
            totalPlayTimeSeconds = _statsModel.CurrentTotalPlayTimeSeconds
        };
        DataPersistence.SaveData(SAVE_KEY, JsonUtility.ToJson(data));
    }

    private async UniTaskVoid StartAutoSave(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(AUTO_SAVE_INTERVAL), cancellationToken: ct);
            Save();
        }
    }

    private void Load()
    {
        var json = DataPersistence.LoadData(SAVE_KEY);
        if (string.IsNullOrEmpty(json)) return;

        var data = JsonUtility.FromJson<SaveData>(json);

        // エリア進行状況を先に復元（ポイント復元時のUI更新で正しい必要クローン数を参照するため）
        _areaModel.SetAreaIndex(data.areaIndex);

        // ポイント復元
        _gameModel.SetPoints(data.points);

        // アップグレードデータ復元
        _upgradeModel.SetClickLevel(data.clickLevel);
        _upgradeModel.SetGeneratorCounts(data.generatorCounts.ToList());

        // 派生値を再計算
        _gameModel.SetClickPower(_upgradeModel.GetCurrentClickPower());
        _gameModel.SetDps(_upgradeModel.GetTotalDps());

        // 統計情報を復元（負の値は0にクリップ）
        _statsModel.SetStats(
            data.totalClickCount,
            data.totalClonesFromClick,
            data.totalClonesFromDps,
            data.totalClonesConsumed,
            Mathf.Max(0, data.totalPlayTimeSeconds)
        );
    }

    public void Dispose()
    {
        Save();
        _cts.Cancel();
        _cts.Dispose();
    }

    [Serializable]
    private sealed class SaveData
    {
        public long points;
        public int clickLevel;
        public List<int> generatorCounts;
        public int areaIndex;

        // 統計情報
        public long totalClickCount;
        public long totalClonesFromClick;
        public long totalClonesFromDps;
        public long totalClonesConsumed;
        public float totalPlayTimeSeconds;
    }
}
