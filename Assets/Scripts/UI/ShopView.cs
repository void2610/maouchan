using System.Collections.Generic;
using UnityEngine;

public class ShopView : MonoBehaviour
{
    [SerializeField] private ShopItemView itemPrefab;
    [SerializeField] private Transform clickUpgradeContainer;
    [SerializeField] private Transform generatorContainer;

    public ShopItemView ClickUpgradeView { get; private set; }
    public IReadOnlyList<ShopItemView> GeneratorViews => _generatorViews;

    private readonly List<ShopItemView> _generatorViews = new();

    public void Initialize(int generatorCount)
    {
        GenerateClickUpgradeView();
        GenerateGeneratorViews(generatorCount);
    }

    private void GenerateClickUpgradeView()
    {
        ClickUpgradeView = Instantiate(itemPrefab, clickUpgradeContainer);
        ClickUpgradeView.SetCountVisible(false);
    }

    private void GenerateGeneratorViews(int count)
    {
        for (var i = 0; i < count; i++)
        {
            var view = Instantiate(itemPrefab, generatorContainer);
            _generatorViews.Add(view);
        }
    }
}
