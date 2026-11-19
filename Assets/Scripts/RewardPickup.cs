
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

    [Header("Collection Settings")]
    [SerializeField] private float collectionRadius = 5f;
    [SerializeField] private float attractionSpeed = 12f;

    private Transform dragon;
    private PooledObject pooledObject;

    private bool isAttracted;
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
        isAttracted = false;
        isCollected = false;
    }

    // Correct initialization method.
    public void Initialize(
        RewardType type,
        int rewardAmount,
        Transform dragonTarget)
    {
        rewardType = type;
        amount = Mathf.Max(0, rewardAmount);
        dragon = dragonTarget;

        isAttracted = false;
        isCollected = false;
    }

    private void Update()
    {
        if (dragon == null || isCollected)
            return;

        float distance =
            Vector3.Distance(transform.position, dragon.position);

        if (distance <= collectionRadius)
        {
            isAttracted = true;
        }

        if (isAttracted)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                dragon.position,
                attractionSpeed * Time.deltaTime
            );
        }
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

        CollectReward();
    }

    
private void CollectReward()
{
    if (isCollected)
        return;

    // Apply health reward directly to the Dragon.
    if (rewardType == RewardType.Health)
    {
        Health dragonHealth = dragon.GetComponent<Health>();

        if (dragonHealth == null)
            return;

        // Don't collect health if already at maximum.
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

}
