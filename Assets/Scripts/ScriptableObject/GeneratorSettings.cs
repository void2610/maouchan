using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GeneratorData
{
    public string name;
    public string description;
    public double effect;
    public long basePrice;
}

[CreateAssetMenu(fileName = "GeneratorSettings", menuName = "Settings/GeneratorSettings")]
public class GeneratorSettings : ScriptableObject
{
    [SerializeField] private List<GeneratorData> generators;

    public List<GeneratorData> Generators => generators;
    public int Count => generators.Count;
}
