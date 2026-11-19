
using UnityEngine;
using TMPro;

public class GoldCounterUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text goldText;

    private int totalGold;

    private void OnEnable()
    {
        RewardPickup.OnRewardCollected += HandleRewardCollected;
    }

    private void OnDisable()
    {
        RewardPickup.OnRewardCollected -= HandleRewardCollected;
    }

    private void Start()
    {
        UpdateGoldUI();
    }

    private void HandleRewardCollected(
        RewardPickup.RewardType type,
        int amount)
    {
        if (type != RewardPickup.RewardType.Gold)
            return;

        totalGold += amount;
        UpdateGoldUI();

        Debug.Log("Total Gold: " + totalGold);
    }

    public void ResetGold()
    {
        totalGold = 0;
        UpdateGoldUI();
    }

    private void UpdateGoldUI()
    {
        if (goldText != null)
            goldText.text = totalGold.ToString();
    }
}
