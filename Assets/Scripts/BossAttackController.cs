
using UnityEngine;

public class BossAttackController : MonoBehaviour
{
    public enum AttackPattern
    {
        StraightBarrage,
        FanSpread,
        AimedShot,
        CircularBurst,
        SpiralAttack,
        DoubleSpiral,
        WaveAttack,
        CrossBarrage,
        RotatingRings,
        MeteorRain
    }

    [Header("References")]
    [SerializeField] private Transform[] firePoints;
    [SerializeField] private GameObject projectilePrefab;

    [Header("Attack Pattern")]
    [SerializeField] private AttackPattern attackPattern =
        AttackPattern.StraightBarrage;

    [Header("Attack Timing")]
    [SerializeField, Min(0.1f)]
    private float attackInterval = 1.5f;

    [Header("Fan Settings")]
    [SerializeField, Min(2)]
    private int fanProjectileCount = 5;
    [SerializeField, Range(0f, 180f)]
    private float fanSpreadAngle = 60f;

    [Header("Circular Burst Settings")]
    [SerializeField, Min(4)]
    private int circularProjectileCount = 12;

    [Header("Spiral Settings")]
    [SerializeField, Min(2)]
    private int spiralProjectileCount = 2;
    [SerializeField]
    private float spiralAngleStep = 20f;

    [Header("Wave Settings")]
    [SerializeField, Min(3)]
    private int waveProjectileCount = 9;
    [SerializeField]
    private float waveAmplitude = 35f;
    [SerializeField]
    private float waveFrequency = 0.8f;

    [Header("Cross Barrage Settings")]
    [SerializeField, Range(0f, 90f)]
    private float crossAngle = 45f;

    [Header("Rotating Ring Settings")]
    [SerializeField]
    private float ringRotationStep = 15f;

    [Header("Meteor Rain Settings")]
    [SerializeField, Min(2)]
    private int meteorProjectileCount = 7;
    [SerializeField, Range(0f, 90f)]
    private float meteorSpreadAngle = 50f;

    [Header("Object Pool")]
    [SerializeField, Min(1)]
    private int initialPoolSize = 30;

    private ObjectPoolManager poolManager;
    private Transform dragon;

    private float nextAttackTime;
    private float spiralAngle;
    private float ringRotationAngle;

    private bool isInitialized;

    private void Start()
    {
        poolManager = ObjectPoolManager.Instance;

        if (poolManager == null)
        {
            Debug.LogError("ObjectPoolManager not found!");
            enabled = false;
            return;
        }

        if (projectilePrefab == null)
        {
            Debug.LogError("Boss projectile prefab is missing!");
            enabled = false;
            return;
        }

        poolManager.Prewarm(projectilePrefab, initialPoolSize);
    }

    public void Initialize(Transform dragonTarget)
    {
        dragon = dragonTarget;

        nextAttackTime = Time.time + attackInterval;

        spiralAngle = 0f;
        ringRotationAngle = 0f;

        isInitialized = dragon != null;
    }

    private void Update()
    {
        if (!isInitialized || dragon == null)
            return;

        if (Time.time < nextAttackTime)
            return;

        Attack();

        nextAttackTime = Time.time + attackInterval;
    }

    private void Attack()
    {
        if (firePoints == null || firePoints.Length == 0)
            return;

        foreach (Transform firePoint in firePoints)
        {
            if (firePoint == null)
                continue;

            switch (attackPattern)
            {
                case AttackPattern.StraightBarrage:
                    FireStraight(firePoint);
                    break;

                case AttackPattern.FanSpread:
                    FireFan(firePoint);
                    break;

                case AttackPattern.AimedShot:
                    FireAimedShot(firePoint);
                    break;

                case AttackPattern.CircularBurst:
                    FireCircularBurst(firePoint);
                    break;

                case AttackPattern.SpiralAttack:
                    FireSpiral(firePoint);
                    break;

                case AttackPattern.DoubleSpiral:
                    FireDoubleSpiral(firePoint);
                    break;

                case AttackPattern.WaveAttack:
                    FireWave(firePoint);
                    break;

                case AttackPattern.CrossBarrage:
                    FireCrossBarrage(firePoint);
                    break;

                case AttackPattern.RotatingRings:
                    FireRotatingRings(firePoint);
                    break;

                case AttackPattern.MeteorRain:
                    FireMeteorRain(firePoint);
                    break;
            }
        }
    }

    // 1. Straight Barrage
    private void FireStraight(Transform firePoint)
    {
        SpawnProjectile(firePoint, Vector3.back);
    }

    // 2. Fan Spread
    private void FireFan(Transform firePoint)
    {
        int count = Mathf.Max(2, fanProjectileCount);

        float angleStep = fanSpreadAngle / (count - 1);

        for (int i = 0; i < count; i++)
        {
            float angle = -fanSpreadAngle * 0.5f
                          + angleStep * i;

            Vector3 direction =
                Quaternion.AngleAxis(angle, Vector3.up)
                * Vector3.back;

            SpawnProjectile(firePoint, direction);
        }
    }

    // 3. Aimed Shot
    private void FireAimedShot(Transform firePoint)
    {
        Vector3 direction = GetDirectionToDragon(firePoint);

        if (direction.sqrMagnitude > 0.001f)
            SpawnProjectile(firePoint, direction);
    }

    // 4. Circular Burst
    private void FireCircularBurst(Transform firePoint)
    {
        FireCircularPattern(
            firePoint,
            circularProjectileCount,
            0f
        );
    }

    // 5. Spiral Attack
    private void FireSpiral(Transform firePoint)
    {
        int count = Mathf.Max(2, spiralProjectileCount);

        float angleStep = 360f / count;

        for (int i = 0; i < count; i++)
        {
            float angle = spiralAngle + angleStep * i;

            Vector3 direction =
                Quaternion.AngleAxis(angle, Vector3.up)
                * Vector3.back;

            SpawnProjectile(firePoint, direction);
        }

        spiralAngle = Mathf.Repeat(
            spiralAngle + spiralAngleStep,
            360f
        );
    }

    // 6. Double Spiral
    private void FireDoubleSpiral(Transform firePoint)
    {
        for (int i = 0; i < spiralProjectileCount; i++)
        {
            float angle = spiralAngle + i * 360f /
                          Mathf.Max(2, spiralProjectileCount);

            Vector3 firstDirection =
                Quaternion.AngleAxis(angle, Vector3.up)
                * Vector3.back;

            Vector3 secondDirection =
                Quaternion.AngleAxis(angle + 180f, Vector3.up)
                * Vector3.back;

            SpawnProjectile(firePoint, firstDirection);
            SpawnProjectile(firePoint, secondDirection);
        }

        spiralAngle = Mathf.Repeat(
            spiralAngle + spiralAngleStep,
            360f
        );
    }

    // 7. Wave Attack
    private void FireWave(Transform firePoint)
    {
        int count = Mathf.Max(3, waveProjectileCount);

        for (int i = 0; i < count; i++)
        {
            float normalizedPosition =
                (float)i / (count - 1);

            float waveAngle =
                Mathf.Sin(normalizedPosition *
                          Mathf.PI * 2f * waveFrequency)
                * waveAmplitude;

            Vector3 direction =
                Quaternion.AngleAxis(waveAngle, Vector3.up)
                * Vector3.back;

            SpawnProjectile(firePoint, direction);
        }
    }

    // 8. Cross Barrage
    private void FireCrossBarrage(Transform firePoint)
    {
        SpawnProjectile(firePoint, Vector3.back);

        SpawnProjectile(
            firePoint,
            RotateDirection(Vector3.back, -crossAngle)
        );

        SpawnProjectile(
            firePoint,
            RotateDirection(Vector3.back, crossAngle)
        );

        SpawnProjectile(firePoint, Vector3.left);
        SpawnProjectile(firePoint, Vector3.right);
    }

    // 9. Rotating Rings
    private void FireRotatingRings(Transform firePoint)
    {
        FireCircularPattern(
            firePoint,
            circularProjectileCount,
            ringRotationAngle
        );

        ringRotationAngle = Mathf.Repeat(
            ringRotationAngle + ringRotationStep,
            360f
        );
    }

    // 10. Meteor Rain
    private void FireMeteorRain(Transform firePoint)
    {
        Vector3 targetDirection = GetDirectionToDragon(firePoint);

        if (targetDirection.sqrMagnitude <= 0.001f)
            return;

        int count = Mathf.Max(2, meteorProjectileCount);

        float angleStep = meteorSpreadAngle / (count - 1);

        for (int i = 0; i < count; i++)
        {
            float angle = -meteorSpreadAngle * 0.5f
                          + angleStep * i;

            Vector3 direction =
                Quaternion.AngleAxis(angle, Vector3.up)
                * targetDirection;

            SpawnProjectile(firePoint, direction);
        }
    }

    // Reusable circular firing method
    private void FireCircularPattern(
        Transform firePoint,
        int projectileCount,
        float startingAngle)
    {
        int count = Mathf.Max(4, projectileCount);

        float angleStep = 360f / count;

        for (int i = 0; i < count; i++)
        {
            float angle = startingAngle + angleStep * i;

            Vector3 direction =
                Quaternion.AngleAxis(angle, Vector3.up)
                * Vector3.back;

            SpawnProjectile(firePoint, direction);
        }
    }

    // Finds the dragon direction on the X-Z plane
    private Vector3 GetDirectionToDragon(Transform firePoint)
    {
        Vector3 direction = dragon.position - firePoint.position;

        direction.y = 0f;

        return direction.normalized;
    }

    // Rotates a direction around the Y axis
    private Vector3 RotateDirection(Vector3 direction, float angle)
    {
        return Quaternion.AngleAxis(angle, Vector3.up) * direction;
    }

    // Shared projectile spawning
    private void SpawnProjectile(
        Transform firePoint,
        Vector3 direction)
    {
        if (poolManager == null || direction.sqrMagnitude <= 0.001f)
            return;

        direction.Normalize();

        GameObject projectile = poolManager.Get(
            projectilePrefab,
            firePoint.position,
            Quaternion.LookRotation(direction)
        );

        if (projectile == null)
            return;

        BossProjectile bossProjectile =
            projectile.GetComponent<BossProjectile>();

        if (bossProjectile != null)
        {
            bossProjectile.Launch(direction);
        }
        else
        {
            Debug.LogError(
                "BossProjectile component missing from projectile prefab!"
            );

            PooledObject pooledObject =
                projectile.GetComponent<PooledObject>();

            if (pooledObject != null)
                pooledObject.ReturnToPool();
        }
    }

    public void SetAttackPattern(AttackPattern pattern)
    {
        attackPattern = pattern;
    }

    public void SetAttackInterval(float interval)
    {
        attackInterval = Mathf.Max(0.1f, interval);
    }

    private void OnEnable()
    {
        isInitialized = false;
    }

    private void OnDisable()
    {
        dragon = null;
        isInitialized = false;

        nextAttackTime = 0f;
        spiralAngle = 0f;
        ringRotationAngle = 0f;
    }
}
