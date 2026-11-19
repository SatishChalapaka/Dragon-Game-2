
using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(PooledObject))]
public class EnemyDeathHandler : MonoBehaviour
{
    [Header("Death Effect")]
    [SerializeField] private GameObject deathEffectPrefab;

    [Header("Gold Rewards")]
    [SerializeField] private GameObject goldPrefab;
    [SerializeField] private int goldDropAmount = 5;

    [Header("XP Rewards")]
    [SerializeField] private GameObject xpPrefab;
    [SerializeField] private int xpDropAmount = 10;

    [Header("Drop Settings")]
    [SerializeField] private float scatterRadius = 0.8f;

    private Health health;
    private PooledObject pooledObject;
    private ObjectPoolManager poolManager;
    private Transform dragon;

[Header("Health Rewards")]
[SerializeField] private GameObject healthPrefab;
[SerializeField] private float healthDropChance = 0.15f;
[SerializeField] private int healthRestoreAmount = 25;


    private void Awake()
    {
        health = GetComponent<Health>();
        pooledObject = GetComponent<PooledObject>();
    }

    private void OnEnable()
    {
        if (health != null)
        {
            health.OnDied += HandleDeath;
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.OnDied -= HandleDeath;
        }
    }

public void Initialize(
    ObjectPoolManager manager,
    Transform dragonTarget)
{
    poolManager = manager;
    dragon = dragonTarget;
}

    private void HandleDeath(Health enemyHealth)
    {
        SpawnDeathEffect();
        SpawnRewards();

        pooledObject.ReturnToPool();
    }

    private void SpawnDeathEffect()
    {
        if (deathEffectPrefab == null || poolManager == null)
            return;

        GameObject effect = poolManager.Get(
            deathEffectPrefab,
            transform.position,
            Quaternion.identity
        );

        if (effect == null)
            return;

        PooledParticleEffect particleEffect =
            effect.GetComponent<PooledParticleEffect>();

        if (particleEffect != null)
        {
            particleEffect.PlayEffect();
        }
    }

    
private void SpawnRewards()
{
    if (poolManager == null)
        return;

    SpawnReward(
        goldPrefab,
        RewardPickup.RewardType.Gold,
        goldDropAmount
    );

    SpawnReward(
        xpPrefab,
        RewardPickup.RewardType.Experience,
        xpDropAmount
    );

    // Chance-based health drop.
    if (Random.value <= healthDropChance)
    {
        SpawnReward(
            healthPrefab,
            RewardPickup.RewardType.Health,
            healthRestoreAmount
        );
    }
}


    
private void SpawnReward(
    GameObject prefab,
    RewardPickup.RewardType rewardType,
    int amount)
{
    if (prefab == null || amount <= 0)
        return;

    int dropCount =
        rewardType == RewardPickup.RewardType.Health
        ? 1
        : amount;

    for (int i = 0; i < dropCount; i++)
    {
        Vector2 randomOffset =
            Random.insideUnitCircle * scatterRadius;

        Vector3 spawnPosition = transform.position +
            new Vector3(randomOffset.x, 0f, randomOffset.y);

        GameObject reward = poolManager.Get(
            prefab,
            spawnPosition,
            Quaternion.identity
        );

        if (reward == null)
            continue;

        RewardPickup pickup =
            reward.GetComponent<RewardPickup>();

        if (pickup != null)
        {
            int rewardAmount =
                rewardType == RewardPickup.RewardType.Health
                ? amount
                : 1;

            pickup.Initialize(
                rewardType,
                rewardAmount,
                dragon
            );
        }
    }
}

}
