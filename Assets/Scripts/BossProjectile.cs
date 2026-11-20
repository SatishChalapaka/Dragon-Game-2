
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PooledObject))]
public class BossProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifetime = 6f;
    [SerializeField] private float damage = 15f;

    [Header("Collision Settings")]
    [SerializeField] private LayerMask dragonLayers;

    private Rigidbody rb;
    private PooledObject pooledObject;

    private Vector3 direction;
    private float remainingLifetime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        pooledObject = GetComponent<PooledObject>();

        rb.useGravity = false;
        rb.isKinematic = true;
        rb.collisionDetectionMode =
            CollisionDetectionMode.ContinuousSpeculative;
    }

    public void Launch(Vector3 shootDirection)
    {
        direction = shootDirection.normalized;
        remainingLifetime = lifetime;

        rb.position = transform.position;

        if (direction.sqrMagnitude > 0f)
            rb.rotation = Quaternion.LookRotation(direction);
    }

    private void FixedUpdate()
    {
        if (direction.sqrMagnitude <= 0f)
            return;

        rb.MovePosition(
            rb.position + direction * speed * Time.fixedDeltaTime
        );

        remainingLifetime -= Time.fixedDeltaTime;

        if (remainingLifetime <= 0f)
            ReturnToPool();
    }

    private void OnTriggerEnter(Collider other)
    {
        Health dragonHealth =
            other.GetComponentInParent<Health>();

        if (dragonHealth == null)
            return;

        int layer = 1 << dragonHealth.gameObject.layer;

        if ((dragonLayers.value & layer) == 0)
            return;

        dragonHealth.TakeDamage(damage);

        ReturnToPool();
    }

    private void ReturnToPool()
    {
        if (pooledObject == null || pooledObject.IsInPool)
            return;

        direction = Vector3.zero;
        pooledObject.ReturnToPool();
    }

    private void OnDisable()
    {
        direction = Vector3.zero;
        remainingLifetime = 0f;
    }
}
