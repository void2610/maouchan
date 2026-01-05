using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI effectText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private Button actionButton;
    [SerializeField] private TextMeshProUGUI buttonText;

    public Observable<Unit> OnActionClicked => actionButton.OnClickAsObservable();

    public void Initialize(string itemName, string itemDescription, long price, string buttonLabel)
    {
        nameText.text = itemName;
        descriptionText.text = itemDescription;
        UpdatePrice(price);
        buttonText.text = buttonLabel;
    }

    public void UpdateName(string itemName) => nameText.text = itemName;
    public void UpdateDescription(string itemDescription) => descriptionText.text = itemDescription;
    public void UpdateEffect(double current, double next) => effectText.text = $"{current:F2}→{next:F2}";
    public void UpdateEffectMax(double current) => effectText.text = $"{current:F1}";

    public void UpdatePrice(long price) => priceText.text = price > 0 ? $"{price:N0}" : "MAX";

    public void UpdateCount(int count) => countText.text = $"×{count}";

    public void SetCountVisible(bool visible) => countText.gameObject.SetActive(visible);

    public void SetButtonInteractable(bool interactable) => actionButton.interactable = interactable;

    public void UpdateButtonLabel(string label) => buttonText.text = label;

    public void SetLocked(bool locked)
    {
        if (locked)
        {
            nameText.text = "？？？";
            descriptionText.text = "？？？";
            priceText.text = "？？？";
            effectText.text = "？？？";
            countText.text = "？？？";
            actionButton.interactable = false;
        }
    }
}
