
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public enum AttackPattern
    {
        Single,
        Spread,
        Triple,
        Circular,
        Random
    }

    public enum AttackDirection
    {
        ForwardOnly,
        BackwardOnly,
        TowardsDragon,
        AllDirections
    }

    [Header("Attack Direction")]
    [SerializeField] private AttackDirection attackDirection;

    [Header("References")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject projectilePrefab;

    [Header("Attack Settings")]
    [SerializeField] private AttackPattern attackPattern;

    [SerializeField, Min(0.1f)]
    private float attackInterval = 2f;

    [SerializeField, Min(2)]
    private int spreadProjectileCount = 3;

    [SerializeField]
    private float spreadAngle = 30f;

    [Header("Circular Attack Settings")]
    [SerializeField, Min(4)]
    private int circularProjectileCount = 8;

    [Header("Random Attack Settings")]
    [SerializeField]
    private bool allowRandomCircular = true;

    private Transform dragon;
    private ObjectPoolManager poolManager;

    private float attackTimer;
    private float damageMultiplier = 1f;

    public void Initialize(
        Transform dragonTarget,
        ObjectPoolManager objectPool)
    {
        dragon = dragonTarget;
        poolManager = objectPool;

        attackTimer = 0f;
        damageMultiplier = 1f;
    }

    public void ApplyWaveDifficulty(float newDamageMultiplier)
    {
        damageMultiplier = Mathf.Max(
            0.1f,
            newDamageMultiplier
        );
    }

    private void Update()
    {
        if (dragon == null ||
            firePoint == null ||
            poolManager == null ||
            projectilePrefab == null)
            return;

        attackTimer += Time.deltaTime;

        if (attackTimer >= attackInterval)
        {
            attackTimer = 0f;
            Attack();
        }
    }

    // =========================================
    // ATTACK SELECTION
    // =========================================

    private void Attack()
    {
        if (attackDirection == AttackDirection.AllDirections)
        {
            FireCircular();
            return;
        }

        Vector3 direction = GetAttackDirection();

        if (direction.sqrMagnitude < 0.001f)
            return;

        AttackPattern selectedPattern = attackPattern;

        if (attackPattern == AttackPattern.Random)
        {
            selectedPattern = GetRandomAttack();
        }

        switch (selectedPattern)
        {
            case AttackPattern.Single:
                FireProjectile(direction);
                break;

            case AttackPattern.Spread:
                FireSpread(direction);
                break;

            case AttackPattern.Triple:
                FireTriple(direction);
                break;

            case AttackPattern.Circular:
                FireCircular();
                break;
        }
    }

    private AttackPattern GetRandomAttack()
    {
        int maximum = allowRandomCircular ? 4 : 3;

        int randomIndex = Random.Range(0, maximum);

        switch (randomIndex)
        {
            case 0:
                return AttackPattern.Single;

            case 1:
                return AttackPattern.Spread;

            case 2:
                return AttackPattern.Triple;

            default:
                return AttackPattern.Circular;
        }
    }

    // =========================================
    // SINGLE SHOT
    // =========================================

    private void FireProjectile(Vector3 direction)
    {
        SpawnProjectile(direction);
    }

    // =========================================
    // SPREAD SHOT
    // =========================================

    private void FireSpread(Vector3 direction)
    {
        int count = Mathf.Max(1, spreadProjectileCount);

        for (int i = 0; i < count; i++)
        {
            float angle = count == 1
                ? 0f
                : -spreadAngle * 0.5f +
                  (spreadAngle / (count - 1)) * i;

            Vector3 spreadDirection =
                Quaternion.AngleAxis(angle, Vector3.up)
                * direction;

            SpawnProjectile(spreadDirection);
        }
    }

    // =========================================
    // TRIPLE SHOT
    // =========================================

    private void FireTriple(Vector3 direction)
    {
        float angle = spreadAngle * 0.5f;

        Vector3 leftDirection =
            Quaternion.AngleAxis(-angle, Vector3.up)
            * direction;

        Vector3 rightDirection =
            Quaternion.AngleAxis(angle, Vector3.up)
            * direction;

        SpawnProjectile(leftDirection);
        SpawnProjectile(direction);
        SpawnProjectile(rightDirection);
    }

    // =========================================
    // CIRCULAR SHOT
    // =========================================

    private void FireCircular()
    {
        int count = Mathf.Max(4, circularProjectileCount);

        for (int i = 0; i < count; i++)
        {
            float angle = (360f / count) * i;

            Vector3 direction =
                Quaternion.AngleAxis(angle, Vector3.up)
                * transform.forward;

            SpawnProjectile(direction);
        }
    }

    // =========================================
    // PROJECTILE SPAWNING
    // =========================================

    private void SpawnProjectile(Vector3 direction)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        direction.Normalize();

        GameObject projectile = poolManager.Get(
            projectilePrefab,
            firePoint.position,
            Quaternion.LookRotation(direction)
        );

        if (projectile == null)
            return;

        EnemyProjectile enemyProjectile =
            projectile.GetComponent<EnemyProjectile>();

        if (enemyProjectile != null)
        {
            enemyProjectile.Launch(
                direction,
                damageMultiplier
            );
        }
        else
        {
            Debug.LogError(
                "EnemyProjectile component missing on " +
                projectilePrefab.name
            );

            PooledObject pooledObject =
                projectile.GetComponent<PooledObject>();

            if (pooledObject != null)
                pooledObject.ReturnToPool();
        }
    }

    // =========================================
    // ATTACK DIRECTION
    // =========================================

    private Vector3 GetAttackDirection()
    {
        switch (attackDirection)
        {
            case AttackDirection.ForwardOnly:
                return transform.forward;

            case AttackDirection.BackwardOnly:
                return -transform.forward;

            case AttackDirection.TowardsDragon:

                // Don't attack if the dragon has passed this enemy.
                if (dragon.position.z > transform.position.z)
                    return Vector3.zero;

                Vector3 direction =
                    dragon.position - firePoint.position;

                direction.y = 0f;

                return direction.normalized;
        }

        return transform.forward;
    }

    // =========================================
    // POOL RESET
    // =========================================

    private void OnDisable()
    {
        attackTimer = 0f;
        dragon = null;
        poolManager = null;
        damageMultiplier = 1f;
    }
}
