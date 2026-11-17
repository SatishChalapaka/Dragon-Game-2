
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PooledObject))]
public class DragonProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifetime = 4f;
    [SerializeField] private float damage = 10f;

    [Header("Collision Settings")]
    [SerializeField] private LayerMask enemyLayers;

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
        {
            rb.rotation = Quaternion.LookRotation(direction);
        }
    }

    private void FixedUpdate()
    {
        if (direction.sqrMagnitude == 0f)
            return;

        rb.MovePosition(
            rb.position + direction * speed * Time.fixedDeltaTime
        );

        remainingLifetime -= Time.fixedDeltaTime;

        if (remainingLifetime <= 0f)
        {
            ReturnToPool();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        int otherLayer = 1 << other.gameObject.layer;

        if ((enemyLayers.value & otherLayer) == 0)
            return;

        IDamageable damageable =
            other.GetComponentInParent<IDamageable>();

        if (damageable == null)
            return;

        damageable.TakeDamage(damage);

        ReturnToPool();
    }

    private void ReturnToPool()
    {
        if (pooledObject == null)
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
