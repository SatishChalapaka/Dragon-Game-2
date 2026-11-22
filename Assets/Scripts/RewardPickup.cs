
using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(PooledObject))]
public class RewardPickup : MonoBehaviour
{
    public enum RewardType
    {
        Gold,
        Experience,
        Health
    }

    [Header("Reward Settings")]
    [SerializeField] private RewardType rewardType;
    [SerializeField] private int amount = 1;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    private Transform dragon;
    private PooledObject pooledObject;

    private bool isCollected;

    public static event Action<RewardType, int> OnRewardCollected;

    public RewardType Type => rewardType;
    public int Amount => amount;

    private void Awake()
    {
        pooledObject = GetComponent<PooledObject>();
    }

    private void OnEnable()
    {
        isCollected = false;
    }

    public void Initialize(
        RewardType type,
        int rewardAmount,
        Transform dragonTarget)
    {
        rewardType = type;
        amount = Mathf.Max(0, rewardAmount);
        dragon = dragonTarget;

        isCollected = false;
    }

    private void Update()
    {
        if (isCollected)
            return;

        // Move continuously toward the dragon along -Z.
        transform.position +=
            Vector3.back * moveSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isCollected || dragon == null)
            return;

        Health dragonHealth =
            other.GetComponentInParent<Health>();

        if (dragonHealth == null)
            return;

        if (dragonHealth.transform != dragon)
            return;

        CollectReward(dragonHealth);
    }

    private void CollectReward(Health dragonHealth)
    {
        if (isCollected)
            return;

        // Apply health reward directly.
        if (rewardType == RewardType.Health)
        {
            if (dragonHealth.CurrentHealth >= dragonHealth.MaxHealth)
                return;

            dragonHealth.Heal(amount);
        }

        isCollected = true;

        OnRewardCollected?.Invoke(rewardType, amount);

        Debug.Log(
            "Collected " + rewardType + ": " + amount
        );

        pooledObject.ReturnToPool();
    }

    private void OnDisable()
    {
        dragon = null;
        isCollected = false;
    }
}
