
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnDirector : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera gameplayCamera;

    [Header("Screen Density")]
    [SerializeField] private int maximumVisibleEnemies = 6;

    [Header("Spawn Position")]
    [SerializeField] private float minimumSpawnDistance = 2.5f;
    [SerializeField] private float minimumHorizontalDistance = 2.5f;
    [SerializeField] private float horizontalSeparationDepth = 1.5f;
    [SerializeField] private int maximumSpawnAttempts = 30;

    [Header("Spawn Reservations")]
    [SerializeField] private float reservationReleaseDistance = 3.5f;

    private class SpawnRecord
    {
        public Transform enemy;
        public Vector3 reservedPosition;
        public bool reservationActive;
    }

    private readonly List<SpawnRecord> activeEnemies =
        new List<SpawnRecord>();

    public int ActiveEnemyCount
    {
        get
        {
            CleanInactiveEnemies();
            return activeEnemies.Count;
        }
    }

    public int VisibleEnemyCount
    {
        get
        {
            CleanInactiveEnemies();
            return CountVisibleEnemies();
        }
    }

    private void Awake()
    {
        if (gameplayCamera == null)
            gameplayCamera = Camera.main;

        if (gameplayCamera == null)
        {
            Debug.LogError(
                "EnemySpawnDirector: Gameplay Camera is missing!"
            );
        }
    }

    // =========================================
    // FIND RANDOM SPAWN POSITION
    // =========================================

    public bool TryGetSpawnPosition(
        Vector2 spawnXRange,
        float spawnY,
        float fixedSpawnZ,
        out Vector3 spawnPosition)
    {
        spawnPosition = Vector3.zero;

        CleanInactiveEnemies();

        if (!CanSpawnMoreEnemies())
            return false;

        int attempts = Mathf.Max(1, maximumSpawnAttempts);

        for (int i = 0; i < attempts; i++)
        {
            float randomX = Random.Range(
                spawnXRange.x,
                spawnXRange.y
            );

            Vector3 candidate = new Vector3(
                randomX,
                spawnY,
                fixedSpawnZ
            );

            if (IsSpawnPositionAvailable(candidate))
            {
                spawnPosition = candidate;
                return true;
            }
        }

        return false;
    }

    // =========================================
    // CHECK SCREEN DENSITY
    // =========================================

    public bool CanSpawnMoreEnemies()
    {
        return CountVisibleEnemies() < maximumVisibleEnemies;
    }

    private int CountVisibleEnemies()
    {
        if (gameplayCamera == null)
            return activeEnemies.Count;

        int count = 0;

        foreach (SpawnRecord record in activeEnemies)
        {
            if (record.enemy == null)
                continue;

            Vector3 viewportPosition =
                gameplayCamera.WorldToViewportPoint(
                    record.enemy.position
                );

            bool visible =
                viewportPosition.z > 0f &&
                viewportPosition.x >= 0f &&
                viewportPosition.x <= 1f &&
                viewportPosition.y >= 0f &&
                viewportPosition.y <= 1f;

            if (visible)
                count++;
        }

        return count;
    }

    // =========================================
    // POSITION VALIDATION
    // =========================================

    public bool IsSpawnPositionAvailable(Vector3 candidate)
    {
        CleanInactiveEnemies();

        foreach (SpawnRecord record in activeEnemies)
        {
            if (record.enemy == null ||
                !record.enemy.gameObject.activeInHierarchy)
                continue;

            // Check actual enemy position.
            if (!CheckDistance(candidate, record.enemy.position))
                return false;

            // Also check reserved entry position.
            // This prevents several incoming enemies
            // from selecting the same spawn position.
            if (record.reservationActive)
            {
                if (!CheckDistance(
                    candidate,
                    record.reservedPosition))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private bool CheckDistance(
        Vector3 candidate,
        Vector3 otherPosition)
    {
        Vector2 candidateXZ =
            new Vector2(candidate.x, candidate.z);

        Vector2 otherXZ =
            new Vector2(otherPosition.x, otherPosition.z);

        float distance = Vector2.Distance(
            candidateXZ,
            otherXZ
        );

        if (distance < minimumSpawnDistance)
            return false;

        float xDifference =
            Mathf.Abs(candidate.x - otherPosition.x);

        float zDifference =
            Mathf.Abs(candidate.z - otherPosition.z);

        if (zDifference <= horizontalSeparationDepth &&
            xDifference < minimumHorizontalDistance)
        {
            return false;
        }

        return true;
    }

    // =========================================
    // REGISTER SPAWNED ENEMY
    // =========================================

    public void RegisterEnemy(
        Transform enemy,
        Vector3 spawnPosition)
    {
        if (enemy == null)
            return;

        CleanInactiveEnemies();

        // Avoid duplicate registration.
        foreach (SpawnRecord record in activeEnemies)
        {
            if (record.enemy == enemy)
                return;
        }

        SpawnRecord newRecord = new SpawnRecord
        {
            enemy = enemy,
            reservedPosition = spawnPosition,
            reservationActive = true
        };

        activeEnemies.Add(newRecord);
    }

    // =========================================
    // CLEAN AND RELEASE RESERVATIONS
    // =========================================

    private void Update()
    {
        CleanInactiveEnemies();
        ReleaseOldReservations();
    }

    private void ReleaseOldReservations()
    {
        foreach (SpawnRecord record in activeEnemies)
        {
            if (record.enemy == null ||
                !record.reservationActive)
                continue;

            Vector2 currentPosition = new Vector2(
                record.enemy.position.x,
                record.enemy.position.z
            );

            Vector2 originalPosition = new Vector2(
                record.reservedPosition.x,
                record.reservedPosition.z
            );

            float distance = Vector2.Distance(
                currentPosition,
                originalPosition
            );

            if (distance >= reservationReleaseDistance)
            {
                record.reservationActive = false;
            }
        }
    }

    private void CleanInactiveEnemies()
    {
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            Transform enemy = activeEnemies[i].enemy;

            if (enemy == null ||
                !enemy.gameObject.activeInHierarchy)
            {
                activeEnemies.RemoveAt(i);
            }
        }
    }

    // =========================================
    // CLEAR TRACKING
    // =========================================

    public void ClearTracking()
    {
        activeEnemies.Clear();
    }

    // =========================================
    // DEBUG
    // =========================================

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        foreach (SpawnRecord record in activeEnemies)
        {
            if (record.enemy == null)
                continue;

            Gizmos.DrawWireSphere(
                record.enemy.position,
                minimumSpawnDistance
            );

            if (record.reservationActive)
            {
                Gizmos.color = Color.yellow;

                Gizmos.DrawWireSphere(
                    record.reservedPosition,
                    0.5f
                );

                Gizmos.color = Color.green;
            }
        }
    }
}
