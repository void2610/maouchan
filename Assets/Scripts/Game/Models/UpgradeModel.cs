using System;
using System.Collections.Generic;
using System.Linq;
using R3;

public class UpgradeModel : IDisposable
{
    public ReadOnlyReactiveProperty<int> CurrentClickLevel => _currentClickLevel;
    public ReadOnlyReactiveProperty<List<int>> GeneratorCounts => _generatorCounts;
    public int GeneratorTypeCount => _generatorSettings.Count;

    private readonly ReactiveProperty<int> _currentClickLevel;
    private readonly ReactiveProperty<List<int>> _generatorCounts;

    private readonly ClickUpgradeSettings _clickUpgradeSettings;
    private readonly GeneratorSettings _generatorSettings;
    private readonly GameBalanceSettings _balanceSettings;

    public UpgradeModel(
        ClickUpgradeSettings clickUpgradeSettings,
        GeneratorSettings generatorSettings,
        GameBalanceSettings balanceSettings)
    {
        _clickUpgradeSettings = clickUpgradeSettings;
        _generatorSettings = generatorSettings;
        _balanceSettings = balanceSettings;

        _currentClickLevel = new ReactiveProperty<int>(0);
        _generatorCounts = new ReactiveProperty<List<int>>();
        _generatorCounts.Value = new int[_generatorSettings.Count].ToList();
    }

    public bool CanUpgradeClick() => _currentClickLevel.Value < _clickUpgradeSettings.Count - 1;
    public double GetCurrentClickPower() => _clickUpgradeSettings.Items[_currentClickLevel.Value].effect;
    public string GetCurrentClickName() => _clickUpgradeSettings.Items[_currentClickLevel.Value].name;
    public string GetCurrentClickDescription() => _clickUpgradeSettings.Items[_currentClickLevel.Value].description;
    public string GetGeneratorName(int index) => _generatorSettings.Generators[index].name;
    public string GetGeneratorDescription(int index) => _generatorSettings.Generators[index].description;
    public void SetClickLevel(int level) => _currentClickLevel.Value = level;
    public int GetGeneratorCount(int index) => _generatorCounts.Value[index];
    
    public int? GetNextClickLevel()
    {
        var next = _currentClickLevel.Value + 1;
        return next < _clickUpgradeSettings.Count ? next : null;
    }

    public long GetNextClickPrice()
    {
        var next = GetNextClickLevel();
        return next == null ? 0 : _clickUpgradeSettings.Items[next.Value].price;
    }

    public void UpgradeClick()
    {
        if (CanUpgradeClick())
            _currentClickLevel.Value++;
    }

    public string GetNextClickName()
    {
        var next = GetNextClickLevel();
        return next == null ? null : _clickUpgradeSettings.Items[next.Value].name;
    }

    public string GetNextDescription()
    {
        var next = GetNextClickLevel();
        return next == null ? null : _clickUpgradeSettings.Items[next.Value].description;
    }

    public double? GetNextClickPower()
    {
        var next = GetNextClickLevel();
        return next == null ? null : _clickUpgradeSettings.Items[next.Value].effect;
    }

    public double GetGeneratorEffect(int index) => _generatorSettings.Generators[index].effect;

    public void AddGenerator(int index)
    {
        var counts = _generatorCounts.Value;
        counts[index]++;
        _generatorCounts.ForceNotify();
    }

    public long GetGeneratorPrice(int index)
    {
        var basePrice = _generatorSettings.Generators[index].basePrice;
        var count = _generatorCounts.Value[index];
        return (long)(basePrice * Math.Pow(_balanceSettings.PriceMultiplier, count));
    }

    public double GetTotalDps()
    {
        double total = 0;
        for (var i = 0; i < _generatorSettings.Count; i++)
        {
            total += _generatorSettings.Generators[i].effect * _generatorCounts.Value[i];
        }
        return total;
    }

    public void SetGeneratorCounts(List<int> counts)
    {
        for (var i = 0; i < counts.Count; i++)
            _generatorCounts.Value[i] = counts[i];
        _generatorCounts.ForceNotify();
    }

    public void Dispose()
    {
        _currentClickLevel.Dispose();
        _generatorCounts.Dispose();
    }
}
