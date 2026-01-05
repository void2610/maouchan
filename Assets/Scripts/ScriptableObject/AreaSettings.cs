using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AreaData
{
    public string name;
    public long requiredClones;
    public EnvironmentView environmentPrefab;
}

[CreateAssetMenu(fileName = "AreaSettings", menuName = "Settings/AreaSettings")]
public class AreaSettings : ScriptableObject
{
    [SerializeField] private List<AreaData> areas;

    public List<AreaData> Areas => areas;
    public int Count => areas.Count;

    public long GetRequiredClones(int index)
    {
        if (index >= areas.Count) return areas[^1].requiredClones;
        return areas[index].requiredClones;
    }

    public string GetAreaName(int index) => areas[index].name;
    public EnvironmentView GetEnvironmentPrefab(int index) => areas[index].environmentPrefab;
}
