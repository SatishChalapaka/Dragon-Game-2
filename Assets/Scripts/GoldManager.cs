
using System;
using UnityEngine;

public class GoldManager : MonoBehaviour
{
    public static GoldManager Instance { get; private set; }

    [Header("Gold Settings")]
    [SerializeField] private int startingGold = 0;

    private const string GOLD_KEY = "DragonFury_TotalGold";

    public int TotalGold { get; private set; }

    public event Action<int> OnGoldChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        TotalGold = PlayerPrefs.GetInt(
            GOLD_KEY,
            startingGold
        );
    }
private void Update()
{
    if (Input.GetKeyDown(KeyCode.G))
    {
        AddGold(10);
    }

    if (Input.GetKeyDown(KeyCode.H))
    {
        SpendGold(5);
    }
}
    private void OnEnable()
    {
        RewardPickup.OnRewardCollected += HandleRewardCollected;
    }

    private void OnDisable()
    {
        RewardPickup.OnRewardCollected -= HandleRewardCollected;
    }

    private void HandleRewardCollected(
        RewardPickup.RewardType type,
        int amount)
    {
        if (type != RewardPickup.RewardType.Gold)
            return;

        AddGold(amount);
    }

    public void AddGold(int amount)
    {
        if (amount <= 0)
            return;

        TotalGold += amount;

        SaveGold();

        OnGoldChanged?.Invoke(TotalGold);

        Debug.Log("Total Gold: " + TotalGold);
    }

    public bool SpendGold(int amount)
    {
        if (amount <= 0 || TotalGold < amount)
            return false;

        TotalGold -= amount;

        SaveGold();

        OnGoldChanged?.Invoke(TotalGold);

        Debug.Log("Gold Spent: " + amount);

        return true;
    }

    private void SaveGold()
    {
        PlayerPrefs.SetInt(GOLD_KEY, TotalGold);
        PlayerPrefs.Save();
    }

    public void ResetGold()
    {
        TotalGold = startingGold;

        SaveGold();

        OnGoldChanged?.Invoke(TotalGold);
    }
}
