
using UnityEngine;

[RequireComponent(typeof(PooledObject))]
public class EnemyCollisionHandler : MonoBehaviour
{
    [Header("Collision Settings")]
    [SerializeField] private LayerMask dragonLayers;
    [SerializeField] private float collisionDamage = 20f;

    [Header("Cleanup Settings")]
    [SerializeField] private float behindDragonDistance = 20f;
    [SerializeField] private float aheadDragonDistance = 80f;
    [SerializeField] private float sideDistance = 30f;

    private Transform dragon;
    private PooledObject pooledObject;

    private bool hasCollided;

    private void Awake()
    {
        pooledObject = GetComponent<PooledObject>();
    }

    private void OnEnable()
    {
        hasCollided = false;
    }

    public void Initialize(Transform dragonTarget)
    {
        dragon = dragonTarget;
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

        int dragonLayer =
            1 << dragonHealth.gameObject.layer;

        if ((dragonLayers.value & dragonLayer) == 0)
            return;

        hasCollided = true;

        // Damage the dragon.
        dragonHealth.TakeDamage(collisionDamage);

        // Return the enemy to the pool.
        pooledObject.ReturnToPool();
    }

    private void Update()
    {
        if (dragon == null)
            return;

        float distanceZ =
            transform.position.z - dragon.position.z;

        float distanceX =
            Mathf.Abs(transform.position.x - dragon.position.x);

        // Enemy has passed too far behind the dragon.
        bool behindDragon =
            distanceZ < -behindDragonDistance;

        // Enemy is too far ahead.
        bool aheadOfDragon =
            distanceZ > aheadDragonDistance;

        // Enemy has moved too far sideways.
        bool outsideSideBounds =
            distanceX > sideDistance;

        if (behindDragon || aheadOfDragon || outsideSideBounds)
        {
            pooledObject.ReturnToPool();
        }
    }
}
