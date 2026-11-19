
using System;
using UnityEngine;

public class XPManager : MonoBehaviour
{
    public static XPManager Instance { get; private set; }

    [Header("Level Settings")]
    [SerializeField] private int startingLevel = 1;
    [SerializeField] private int baseRequiredXP = 100;
    [SerializeField] private float xpGrowthMultiplier = 1.25f;

    public int CurrentLevel { get; private set; }
    public int CurrentXP { get; private set; }
    public int RequiredXP { get; private set; }

    public event Action<int, int> OnXPChanged;
    public event Action<int> OnLevelUp;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        CurrentLevel = startingLevel;
        CurrentXP = 0;
        RequiredXP = baseRequiredXP;
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
        if (type != RewardPickup.RewardType.Experience)
            return;

        AddXP(amount);
    }

    public void AddXP(int amount)
    {
        if (amount <= 0)
            return;

        CurrentXP += amount;

        // Handle multiple level-ups if enough XP is collected.
        while (CurrentXP >= RequiredXP)
        {
            CurrentXP -= RequiredXP;

            LevelUp();
        }

        OnXPChanged?.Invoke(CurrentXP, RequiredXP);

        Debug.Log(
            "XP: " + CurrentXP + " / " + RequiredXP +
            " | Level: " + CurrentLevel
        );
    }

    private void LevelUp()
    {
        CurrentLevel++;

        RequiredXP = Mathf.CeilToInt(
            RequiredXP * xpGrowthMultiplier
        );

        Debug.Log("DRAGON LEVEL UP! Level: " + CurrentLevel);

        OnLevelUp?.Invoke(CurrentLevel);
    }

    public void ResetProgress()
    {
        CurrentLevel = startingLevel;
        CurrentXP = 0;
        RequiredXP = baseRequiredXP;

        OnXPChanged?.Invoke(CurrentXP, RequiredXP);
    }
}
