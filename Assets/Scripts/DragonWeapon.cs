
using UnityEngine;

public class DragonWeapon : MonoBehaviour
{
    public enum ShootingPattern
    {
        Single,
        Double,
        Triple,
        Fan
    }

    [Header("References")]
    [SerializeField] private Transform[] firePoints;
    [SerializeField] private GameObject projectilePrefab;

    [Header("Shooting Settings")]
    [SerializeField]
    private ShootingPattern shootingPattern =
        ShootingPattern.Single;

    [SerializeField, Min(0.1f)] private float fireRate = 5f;

    [Header("Projectile Pattern")]
    [SerializeField, Min(2)] private int fanProjectileCount = 5;
    [SerializeField, Range(0f, 90f)] private float fanSpreadAngle = 40f;
    [SerializeField, Min(0f)] private float parallelSpacing = 0.3f;

    [Header("Object Pool")]
    [SerializeField, Min(1)] private int initialPoolSize = 30;

    private float nextFireTime;

    private void Start()
    {
        if (ObjectPoolManager.Instance == null)
        {
            Debug.LogError("ObjectPoolManager is missing!");
            return;
        }

        if (projectilePrefab == null)
        {
            Debug.LogError("Projectile prefab is missing!");
            return;
        }

        ObjectPoolManager.Instance.Prewarm(
            projectilePrefab,
            initialPoolSize
        );
    }

    private void Update()
    {
        if (Time.time < nextFireTime)
            return;

        if (firePoints == null || firePoints.Length == 0)
            return;

        if (projectilePrefab == null)
            return;

        Shoot();

        nextFireTime = Time.time + (1f / fireRate);
    }

    private void Shoot()
    {
        foreach (Transform firePoint in firePoints)
        {
            if (firePoint == null)
                continue;

            FirePattern(firePoint);
        }
    }

    private void FirePattern(Transform firePoint)
    {
        int projectileCount = GetProjectileCount();

        for (int i = 0; i < projectileCount; i++)
        {
            float angle = GetProjectileAngle(i, projectileCount);
            float offset = GetProjectileOffset(i, projectileCount);

            Vector3 direction =
                Quaternion.AngleAxis(angle, Vector3.up) *
                firePoint.forward;

            Vector3 spawnPosition =
                firePoint.position + firePoint.right * offset;

            Quaternion spawnRotation =
                Quaternion.LookRotation(direction, Vector3.up);

            GameObject projectile =
                ObjectPoolManager.Instance.Get(
                    projectilePrefab,
                    spawnPosition,
                    spawnRotation
                );

            if (projectile == null)
                continue;

            DragonProjectile dragonProjectile =
                projectile.GetComponent<DragonProjectile>();

            if (dragonProjectile != null)
            {
                dragonProjectile.Launch(direction);
            }
            else
            {
                Debug.LogError(
                    "DragonProjectile component is missing!",
                    projectile
                );
            }
        }
    }

    private int GetProjectileCount()
    {
        switch (shootingPattern)
        {
            case ShootingPattern.Single:
                return 1;

            case ShootingPattern.Double:
                return 2;

            case ShootingPattern.Triple:
                return 3;

            case ShootingPattern.Fan:
                return fanProjectileCount;

            default:
                return 1;
        }
    }

    private float GetProjectileAngle(int index, int count)
    {
        if (shootingPattern != ShootingPattern.Fan || count <= 1)
            return 0f;

        float step = fanSpreadAngle / (count - 1);

        return -fanSpreadAngle * 0.5f + step * index;
    }

    private float GetProjectileOffset(int index, int count)
    {
        switch (shootingPattern)
        {
            case ShootingPattern.Double:
                return (index == 0 ? -0.5f : 0.5f)
                    * parallelSpacing;

            case ShootingPattern.Triple:
                return (index - 1) * parallelSpacing;

            default:
                return 0f;
        }
    }

    public void SetFireRate(float newFireRate)
    {
        fireRate = Mathf.Max(0.1f, newFireRate);
    }

    public void SetPattern(ShootingPattern newPattern)
    {
        shootingPattern = newPattern;
    }
}
