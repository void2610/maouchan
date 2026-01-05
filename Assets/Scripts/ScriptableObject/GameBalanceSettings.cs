using UnityEngine;

[CreateAssetMenu(fileName = "GameBalanceSettings", menuName = "Settings/GameBalanceSettings")]
public class GameBalanceSettings : ScriptableObject
{
    [Header("初期値")]
    [Tooltip("初期クローン数")]
    [SerializeField] private long initialPoints;

    [Header("クリック設定")]
    [Tooltip("1クリックあたりの基本生成数")]
    [SerializeField] private double baseClickPower = 0.2;

    [Header("自動生成設定")]
    [Tooltip("1秒あたりの基本自動生成数（DPS）")]
    [SerializeField] private double baseDps;

    [Header("価格設定")]
    [Tooltip("設備価格の上昇倍率（購入ごとに価格がこの倍率で増加）")]
    [SerializeField] private double priceMultiplier = 1.15;

    [Tooltip("unityroomのHMACキー")]
    [SerializeField] private string unityroomHmacKey;

    [Tooltip("ImgBBのAPIキー")]
    [SerializeField] private string imgBBApiKey;

    public long InitialPoints => initialPoints;
    public double BaseClickPower => baseClickPower;
    public double BaseDps => baseDps;
    public double PriceMultiplier => priceMultiplier;
    public string UnityroomHmacKey => unityroomHmacKey;
    public string ImgBBApiKey => imgBBApiKey;
}
