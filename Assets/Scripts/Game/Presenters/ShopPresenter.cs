using System;
using R3;
using Void2610.UnityTemplate;

public class ShopPresenter : IDisposable
{
    private readonly GameModel _gameModel;
    private readonly UpgradeModel _upgradeModel;
    private readonly ShopView _shopView;
    private readonly CompositeDisposable _disposables = new();

    public ShopPresenter(GameModel gameModel, UpgradeModel upgradeModel)
    {
        _gameModel = gameModel;
        _upgradeModel = upgradeModel;
        _shopView = UnityEngine.Object.FindFirstObjectByType<ShopView>();
        
        // 初期状態でクリックパワーとDPSを設定
        _gameModel.SetClickPower(_upgradeModel.GetCurrentClickPower());
        _gameModel.SetDps(_upgradeModel.GetTotalDps());

        // UI初期化
        _shopView.Initialize(_upgradeModel.GeneratorTypeCount);
        _shopView.ClickUpgradeView.UpdateButtonLabel("強化");
        UpdateClickUpgradeView();
        InitializeGeneratorViews();
        
        // クリックのアップグレード
        _shopView.ClickUpgradeView.OnActionClicked.Subscribe(_ => OnClickUpgrade()).AddTo(_disposables);
        // ポイント変更時にボタン状態更新
        _gameModel.Points.Subscribe(_ => UpdateButtonStates()).AddTo(_disposables);
        // クリックレベル変更時に表示更新
        _upgradeModel.CurrentClickLevel.Subscribe(_ => UpdateClickUpgradeView()).AddTo(_disposables);
        // 設備数変更時に表示更新
        _upgradeModel.GeneratorCounts.Subscribe(_ => UpdateGeneratorViews()).AddTo(_disposables);
    }

    private void InitializeGeneratorViews()
    {
        for (var i = 0; i < _shopView.GeneratorViews.Count; i++)
        {
            var index = i;
            var view = _shopView.GeneratorViews[i];
            var effect = _upgradeModel.GetGeneratorEffect(i);
            var count = _upgradeModel.GetGeneratorCount(i);
            var isLocked = IsGeneratorLocked(i);

            view.Initialize(
                _upgradeModel.GetGeneratorName(i),
                _upgradeModel.GetGeneratorDescription(i),
                _upgradeModel.GetGeneratorPrice(i),
                "購入"
            );
            view.UpdateCount(count);
            view.UpdateEffect(effect * count, effect * (count + 1));

            // ロック状態を設定（初期化の後に呼び出すことで表示を上書き）
            view.SetLocked(isLocked);

            view.OnActionClicked.Subscribe(_ => OnGeneratorPurchase(index)).AddTo(_disposables);
        }
    }

    private void UpdateClickUpgradeView()
    {
        var view = _shopView.ClickUpgradeView;
        var canUpgrade = _upgradeModel.CanUpgradeClick();
        var currentPower = _upgradeModel.GetCurrentClickPower();
        var nextPower = _upgradeModel.GetNextClickPower();

        view.UpdateName(_upgradeModel.GetCurrentClickName());
        view.UpdateDescription(_upgradeModel.GetCurrentClickDescription());
        view.UpdatePrice(canUpgrade ? _upgradeModel.GetNextClickPrice() : 0);

        if (nextPower.HasValue)
            view.UpdateEffect(currentPower, nextPower.Value);
        else
            view.UpdateEffectMax(currentPower);

        UpdateButtonStates();
    }

    private void UpdateGeneratorViews()
    {
        for (var i = 0; i < _shopView.GeneratorViews.Count; i++)
        {
            var view = _shopView.GeneratorViews[i];
            var effect = _upgradeModel.GetGeneratorEffect(i);
            var count = _upgradeModel.GetGeneratorCount(i);
            var isLocked = IsGeneratorLocked(i);

            if (isLocked)
            {
                // ロック中は「？？？」で表示
                view.SetLocked(true);
            }
            else
            {
                // アンロック時は通常表示を再設定
                view.UpdateName(_upgradeModel.GetGeneratorName(i));
                view.UpdateDescription(_upgradeModel.GetGeneratorDescription(i));
                view.UpdatePrice(_upgradeModel.GetGeneratorPrice(i));
                view.UpdateCount(count);
                view.UpdateEffect(effect * count, effect * (count + 1));
            }
        }
        UpdateButtonStates();
    }

    private void UpdateButtonStates()
    {
        // クリックアップグレードボタン
        var canUpgrade = _upgradeModel.CanUpgradeClick();
        var clickPrice = _upgradeModel.GetNextClickPrice();
        _shopView.ClickUpgradeView.SetButtonInteractable(canUpgrade && _gameModel.CurrentPoints >= clickPrice);

        // 設備ボタン
        for (var i = 0; i < _shopView.GeneratorViews.Count; i++)
        {
            var price = _upgradeModel.GetGeneratorPrice(i);
            var isLocked = IsGeneratorLocked(i);
            // ロック中はボタン無効
            _shopView.GeneratorViews[i].SetButtonInteractable(!isLocked && _gameModel.CurrentPoints >= price);
        }
    }

    private void OnClickUpgrade()
    {
        if (!_upgradeModel.CanUpgradeClick()) return;

        var price = _upgradeModel.GetNextClickPrice();
        if (!_gameModel.SpendPoints(price)) return;

        _upgradeModel.UpgradeClick();
        _gameModel.SetClickPower(_upgradeModel.GetCurrentClickPower());
        SeManager.Instance.PlaySe("Purchase");
    }

    private void OnGeneratorPurchase(int index)
    {
        var price = _upgradeModel.GetGeneratorPrice(index);
        if (!_gameModel.SpendPoints(price)) return;

        _upgradeModel.AddGenerator(index);
        _gameModel.SetDps(_upgradeModel.GetTotalDps());
        SeManager.Instance.PlaySe("Purchase");
    }

    private bool IsGeneratorLocked(int index)
    {
        // 最初のジェネレータは常にアンロック
        if (index == 0) return false;
        // 前のジェネレータを1つ以上購入していればアンロック
        return _upgradeModel.GetGeneratorCount(index - 1) < 1;
    }

    public void Dispose() => _disposables.Dispose();
}
