using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ClickUpgradeData
{
    public string name;
    public string description;
    public double effect;
    public long price;
}

[CreateAssetMenu(fileName = "ClickUpgradeSettings", menuName = "Settings/ClickUpgradeSettings")]
public class ClickUpgradeSettings : ScriptableObject
{
    [SerializeField] private List<ClickUpgradeData> items;

    public List<ClickUpgradeData> Items => items;
    public int Count => items.Count;
}
