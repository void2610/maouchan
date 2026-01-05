using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatsView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI cloneCountText;
    [SerializeField] private TextMeshProUGUI dpsText;
    [SerializeField] private TextMeshProUGUI areaProgressText;
    [SerializeField] private Button areaUnlockButton;
    [SerializeField] private TextMeshProUGUI areaUnlockButtonText;

    public Observable<Unit> OnAreaUnlockClicked => areaUnlockButton.OnClickAsObservable();
    public void UpdateCloneCount(long count) => cloneCountText.text = $"分身数: {count:N0}";
    public void UpdateDps(double dps) => dpsText.text = $"自動生産: {dps:N2}/秒";

    public void UpdateAreaProgress(long current, long required, string nextAreaName)
    {
        if (string.IsNullOrEmpty(nextAreaName))
        {
            areaProgressText.text = "世界征服はこれからだ！！";
            return;
        }
        areaProgressText.text = $"必要数: {required:N0}";
    }

    public void SetAreaUnlockButtonVisible(bool visible)
    {
        areaUnlockButton.gameObject.SetActive(visible);
    }
}
