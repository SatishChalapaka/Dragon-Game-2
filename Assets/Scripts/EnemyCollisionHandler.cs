
using UnityEngine;

[RequireComponent(typeof(PooledObject))]
public class EnemyCollisionHandler : MonoBehaviour
{
    [Header("Collision Settings")]
    [SerializeField] private LayerMask dragonLayers;
    [SerializeField] private float collisionDamage = 20f;

    [Header("Collision Particle Effects")]
    [SerializeField] private GameObject playerCollisionEffectPrefab;
    [SerializeField] private GameObject enemyImpactEffectPrefab;

    [Header("Cleanup Settings")]
    [SerializeField] private float behindDragonDistance = 20f;
    [SerializeField] private float aheadDragonDistance = 80f;
    [SerializeField] private float sideDistance = 30f;

    private Transform dragon;
    private PooledObject pooledObject;
    private ObjectPoolManager poolManager;

    private bool hasCollided;

    private void Awake()
    {
        pooledObject = GetComponent<PooledObject>();
        poolManager = ObjectPoolManager.Instance;
    }

    private void OnEnable()
    {
        hasCollided = false;
    }

    public void Initialize(Transform dragonTarget)
    {
        dragon = dragonTarget;

        if (poolManager == null)
            poolManager = ObjectPoolManager.Instance;

        hasCollided = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasCollided)
            return;

        Health dragonHealth =
            other.GetComponentInParent<Health>();

        if (dragonHealth == null)
            return;

        int dragonLayer = 1 << dragonHealth.gameObject.layer;

        if ((dragonLayers.value & dragonLayer) == 0)
            return;

        hasCollided = true;

        // Find an approximate contact point on the player collider.
        Vector3 impactPosition =
            other.ClosestPoint(transform.position);

        // Play particle around the player.
        PlayCollisionEffect(
            playerCollisionEffectPrefab,
            dragonHealth.transform.position
        );

        // Play particle at the collision point.
        PlayCollisionEffect(
            enemyImpactEffectPrefab,
            impactPosition
        );

        // Apply damage to the player.
        dragonHealth.TakeDamage(collisionDamage);

        // Return the enemy to the pool.
        pooledObject.ReturnToPool();
    }

    private void PlayCollisionEffect(
        GameObject effectPrefab,
        Vector3 position)
    {
        if (effectPrefab == null)
            return;

        if (poolManager == null)
            poolManager = ObjectPoolManager.Instance;

        if (poolManager == null)
        {
            Debug.LogWarning(
                "ObjectPoolManager not found. Cannot play collision effect."
            );
            return;
        }

        poolManager.Get(
            effectPrefab,
            position,
            Quaternion.identity
        );
    }

    private void Update()
    {
        if (dragon == null || pooledObject.IsInPool)
            return;

        float distanceZ =
            transform.position.z - dragon.position.z;

        float distanceX =
            Mathf.Abs(transform.position.x - dragon.position.x);

        bool behindDragon =
            distanceZ < -behindDragonDistance;

        bool aheadOfDragon =
            distanceZ > aheadDragonDistance;

        bool outsideSideBounds =
            distanceX > sideDistance;

        if (behindDragon || aheadOfDragon || outsideSideBounds)
        {
            pooledObject.ReturnToPool();
        }
    }
}
