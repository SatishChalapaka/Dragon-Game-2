
using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(PooledObject))]
public class BossDeathHandler : MonoBehaviour
{
    [Header("Death Effect")]
    [SerializeField] private GameObject deathEffectPrefab;

    private Health bossHealth;
    private PooledObject pooledObject;
    private ObjectPoolManager poolManager;

    private void Awake()
    {
        bossHealth = GetComponent<Health>();
        pooledObject = GetComponent<PooledObject>();
    }

    private void OnEnable()
    {
        if (bossHealth != null)
            bossHealth.OnDied += HandleBossDeath;
    }

    private void OnDisable()
    {
        if (bossHealth != null)
            bossHealth.OnDied -= HandleBossDeath;
    }

    private void Start()
    {
        poolManager = ObjectPoolManager.Instance;
    }

    private void HandleBossDeath(Health health)
    {
        SpawnDeathEffect();

        Debug.Log(name + " defeated!");

        // Returning the boss also notifies EnemyWaveTracker.
        pooledObject.ReturnToPool();
    }

    private void SpawnDeathEffect()
    {
        if (deathEffectPrefab == null)
            return;

        if (poolManager == null)
            poolManager = ObjectPoolManager.Instance;

        if (poolManager == null)
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
            particleEffect.PlayEffect();
    }
}
