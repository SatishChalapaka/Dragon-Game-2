
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public enum AttackPattern
    {
        Single,
        Spread
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
    [SerializeField] private float attackInterval = 2f;
    [SerializeField] private int spreadProjectileCount = 3;
    [SerializeField] private float spreadAngle = 30f;

    private Transform dragon;
    private ObjectPoolManager poolManager;

    private float attackTimer;

    public void Initialize(
        Transform dragonTarget,
        ObjectPoolManager objectPool)
    {
        dragon = dragonTarget;
        poolManager = objectPool;
        attackTimer = 0f;
    }

    private void Update()
    {
        if (dragon == null || firePoint == null ||
            poolManager == null || projectilePrefab == null)
            return;

        attackTimer += Time.deltaTime;

        if (attackTimer >= attackInterval)
        {
            attackTimer = 0f;
            Attack();
        }
    }

    
private void Attack()
{
    Vector3 direction = GetAttackDirection();

    if (attackDirection == AttackDirection.TowardsDragon &&
        direction == Vector3.zero)
        return;

    switch (attackDirection)
    {
        case AttackDirection.AllDirections:
            FireAllDirections();
            break;

        default:
            switch (attackPattern)
            {
                case AttackPattern.Single:
                    FireProjectile(direction);
                    break;

                case AttackPattern.Spread:
                    FireSpread(direction);
                    break;
            }
            break;
    }
}

    private void FireAllDirections()
{
    int count = Mathf.Max(1, spreadProjectileCount);

    for (int i = 0; i < count; i++)
    {
        float angle = (360f / count) * i;

        Vector3 direction =
            Quaternion.AngleAxis(angle, Vector3.up) *
            transform.forward;

        FireProjectile(direction);
    }
}
    private void FireProjectile(Vector3 direction)
    {
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
            enemyProjectile.Launch(direction);
    }

    private void FireSpread(Vector3 direction)
    {
        int count = Mathf.Max(1, spreadProjectileCount);

        for (int i = 0; i < count; i++)
        {
            float angle = count == 1
                ? 0f
                : -spreadAngle / 2f +
                  (spreadAngle / (count - 1)) * i;

            Vector3 spreadDirection =
                Quaternion.AngleAxis(angle, Vector3.up) * direction;

            FireProjectile(spreadDirection);
        }
    }


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

        case AttackDirection.AllDirections:
            return Vector3.zero;
    }

    return transform.forward;
}


}
