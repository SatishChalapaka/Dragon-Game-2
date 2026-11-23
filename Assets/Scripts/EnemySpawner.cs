
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemySpawnDirector))]
public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform dragon;
    [SerializeField] private ObjectPoolManager poolManager;
    [SerializeField] private EnemySpawnDirector spawnDirector;

    [Header("Legacy Enemy Lists")]
    [SerializeField] private List<GameObject> enemyPrefabs;
    [SerializeField] private List<GameObject> formationEnemyPrefabs;

    [Header("Formation Settings")]
    [SerializeField] private float formationMoveSpeed = 5f;

    [SerializeField]
    private Vector3[] formationOffsets =
    {
        new Vector3(0f, 0f, 0f),
        new Vector3(-2f, 0f, 2f),
        new Vector3(2f, 0f, 2f),
        new Vector3(-4f, 0f, 4f),
        new Vector3(4f, 0f, 4f)
    };

    [Header("Spawn Settings")]
    [SerializeField] private float fixedSpawnZ = 20f;
    [SerializeField]
    private Vector2 spawnXRange =
        new Vector2(-4f, 4f);

    [Header("Pool Settings")]
    [SerializeField] private int initialPoolSize = 10;

    [Header("Reward Pool Settings")]
    [SerializeField] private List<GameObject> rewardPrefabs;
    [SerializeField] private int initialRewardPoolSize = 50;

    private readonly List<Transform> activeFormations =
        new List<Transform>();

    private void Start()
    {
        if (dragon == null || poolManager == null)
        {
            Debug.LogError(
                "Dragon or ObjectPoolManager is missing!"
            );

            enabled = false;
            return;
        }

        if (spawnDirector == null)
            spawnDirector = GetComponent<EnemySpawnDirector>();

        if (spawnDirector == null)
        {
            Debug.LogError(
                "EnemySpawnDirector is missing!"
            );

            enabled = false;
            return;
        }

        PrewarmPrefabs(enemyPrefabs);
        PrewarmPrefabs(formationEnemyPrefabs);
        PrewarmPrefabs(rewardPrefabs, initialRewardPoolSize);
    }

    private void Update()
    {
        MoveFormations();
    }

    // =========================================
    // PREWARM WAVE PREFAB
    // =========================================

    public void PrewarmWavePrefab(GameObject prefab, int amount)
    {
        if (prefab == null || poolManager == null)
            return;

        poolManager.Prewarm(prefab, Mathf.Max(1, amount));
    }

    // =========================================
    // WAVE ENEMY
    // =========================================

    public void SpawnWaveEnemy(
        WaveManager waveManager,
        GameObject prefab,
        float healthMultiplier,
        float speedMultiplier,
        float attackMultiplier)
    {
        SpawnEnemy(
            waveManager,
            prefab,
            healthMultiplier,
            speedMultiplier,
            attackMultiplier
        );
    }

    // =========================================
    // WAVE FORMATION
    // =========================================

    public void SpawnWaveFormation(
        WaveManager waveManager,
        GameObject prefab,
        float healthMultiplier,
        float speedMultiplier,
        float attackMultiplier,
        int enemiesPerFormation)
    {
        SpawnFormation(
            waveManager,
            prefab,
            healthMultiplier,
            speedMultiplier,
            attackMultiplier,
            enemiesPerFormation
        );
    }

    // =========================================
    // BOSS SPAWNING
    // =========================================

    public bool SpawnWaveBoss(
        WaveManager waveManager,
        GameObject bossPrefab,
        float healthMultiplier,
        float speedMultiplier,
        float attackMultiplier)
    {
        if (bossPrefab == null)
        {
            Debug.LogError("Boss Prefab is not assigned!");
            return false;
        }

        Vector3 spawnPosition = new Vector3(
    0f,
    dragon.position.y,
    20f
);

        GameObject boss = poolManager.Get(
            bossPrefab,
            spawnPosition,
            Quaternion.identity
        );

        if (boss == null)
            return false;

        InitializeEnemy(boss);

        ApplyEnemyDifficulty(
            boss,
            healthMultiplier,
            speedMultiplier,
            attackMultiplier
        );

        BossMovement bossMovement =
            boss.GetComponent<BossMovement>();

        if (bossMovement == null)
        {
            Debug.LogError(
                "BossMovement is missing on the boss prefab!"
            );

            ReturnToPool(boss);
            return false;
        }

        bossMovement.Initialize(dragon, speedMultiplier);

        BossAttackController attackController =
            boss.GetComponent<BossAttackController>();

        if (attackController != null)
            attackController.Initialize(dragon);

        EnemyWaveTracker tracker =
            boss.GetComponent<EnemyWaveTracker>();

        if (tracker == null)
        {
            Debug.LogError(
                "EnemyWaveTracker is missing on the boss prefab!"
            );

            ReturnToPool(boss);
            return false;
        }

        spawnDirector.RegisterEnemy(
            boss.transform,
            spawnPosition
        );

        tracker.Register(waveManager);

        Debug.Log("Boss spawned successfully: " + bossPrefab.name);

        return true;
    }

    // =========================================
    // RETURN TO POOL
    // =========================================

    private void ReturnToPool(GameObject target)
    {
        if (target == null)
            return;

        PooledObject pooledObject =
            target.GetComponent<PooledObject>();

        if (pooledObject != null)
            pooledObject.ReturnToPool();
    }

    // =========================================
    // PREWARM
    // =========================================

    private void PrewarmPrefabs(List<GameObject> prefabs)
    {
        PrewarmPrefabs(prefabs, initialPoolSize);
    }

    private void PrewarmPrefabs(
        List<GameObject> prefabs,
        int poolSize)
    {
        if (prefabs == null)
            return;

        foreach (GameObject prefab in prefabs)
        {
            if (prefab != null)
                poolManager.Prewarm(prefab, poolSize);
        }
    }

    // =========================================
    // INITIALIZE ENEMY
    // =========================================

    private void InitializeEnemy(GameObject enemy)
    {
        if (enemy == null)
            return;

        EnemyAttack enemyAttack =
            enemy.GetComponent<EnemyAttack>();

        if (enemyAttack != null)
            enemyAttack.Initialize(dragon, poolManager);

        EnemyDeathHandler deathHandler =
            enemy.GetComponent<EnemyDeathHandler>();

        if (deathHandler != null)
            deathHandler.Initialize(poolManager, dragon);

        EnemyCollisionHandler collisionHandler =
            enemy.GetComponent<EnemyCollisionHandler>();

        if (collisionHandler != null)
            collisionHandler.Initialize(dragon);

        EnemyMovement movement =
            enemy.GetComponent<EnemyMovement>();

        if (movement != null)
            movement.ResetForSpawn();
    }

    // =========================================
    // FIND SPAWN POSITION
    // =========================================

    private bool TryFindSpawnPosition(out Vector3 spawnPosition)
    {
        return spawnDirector.TryGetSpawnPosition(
            spawnXRange,
            dragon.position.y,
            fixedSpawnZ,
            out spawnPosition
        );
    }

    // =========================================
    // NORMAL ENEMY SPAWNING
    // =========================================

    private void SpawnEnemy(
        WaveManager waveManager,
        GameObject prefab,
        float healthMultiplier,
        float speedMultiplier,
        float attackMultiplier)
    {
        if (prefab == null)
            return;

        if (!TryFindSpawnPosition(out Vector3 spawnPosition))
        {
            Debug.LogWarning(
                "Enemy spawn rejected: no safe position or screen is full. " +
                prefab.name
            );

            return;
        }

        GameObject enemy = poolManager.Get(
            prefab,
            spawnPosition,
            Quaternion.identity
        );

        if (enemy == null)
            return;

        InitializeEnemy(enemy);

        ApplyEnemyDifficulty(
            enemy,
            healthMultiplier,
            speedMultiplier,
            attackMultiplier
        );

        spawnDirector.RegisterEnemy(
            enemy.transform,
            spawnPosition
        );

        RegisterWaveEnemy(enemy, waveManager, prefab);
    }

    // =========================================
    // FORMATION SPAWNING
    // =========================================

    private void SpawnFormation(
        WaveManager waveManager,
        GameObject prefab,
        float healthMultiplier,
        float speedMultiplier,
        float attackMultiplier,
        int enemiesPerFormation)
    {
        if (prefab == null ||
            formationOffsets == null ||
            formationOffsets.Length == 0)
            return;

        int formationCount = Mathf.Clamp(
            enemiesPerFormation,
            1,
            formationOffsets.Length
        );

        if (!TryFindSpawnPosition(out Vector3 rootPosition))
        {
            Debug.LogWarning(
                "Formation spawn rejected: no safe position available."
            );

            return;
        }

        GameObject rootObject =
            new GameObject("EnemyFormationRoot");

        rootObject.transform.position = rootPosition;

        activeFormations.Add(rootObject.transform);

        int spawnedCount = 0;

        for (int i = 0; i < formationCount; i++)
        {
            Vector3 offset = formationOffsets[i];

            Vector3 spawnPosition =
                rootObject.transform.TransformPoint(offset);

            if (!spawnDirector.IsSpawnPositionAvailable(spawnPosition))
                continue;

            GameObject enemy = poolManager.Get(
                prefab,
                spawnPosition,
                Quaternion.identity
            );

            if (enemy == null)
                continue;

            InitializeEnemy(enemy);

            ApplyEnemyDifficulty(
                enemy,
                healthMultiplier,
                speedMultiplier,
                attackMultiplier
            );

            spawnDirector.RegisterEnemy(
                enemy.transform,
                spawnPosition
            );

            RegisterWaveEnemy(enemy, waveManager, prefab);

            EnemyMovement movement =
                enemy.GetComponent<EnemyMovement>();

            if (movement != null)
            {
                movement.SetFormation(
                    rootObject.transform,
                    offset
                );
            }

            spawnedCount++;
        }

        if (spawnedCount == 0)
        {
            activeFormations.Remove(rootObject.transform);
            Destroy(rootObject);
        }
    }

    // =========================================
    // WAVE TRACKING
    // =========================================

    private void RegisterWaveEnemy(
        GameObject enemy,
        WaveManager waveManager,
        GameObject prefab)
    {
        EnemyWaveTracker tracker =
            enemy.GetComponent<EnemyWaveTracker>();

        if (tracker != null)
        {
            tracker.Register(waveManager);
        }
        else
        {
            Debug.LogError(
                "EnemyWaveTracker is missing on " + prefab.name
            );
        }
    }

    // =========================================
    // DIFFICULTY
    // =========================================

    private void ApplyEnemyDifficulty(
        GameObject enemy,
        float healthMultiplier,
        float speedMultiplier,
        float attackMultiplier)
    {
        Health health = enemy.GetComponent<Health>();

        if (health != null)
            health.ApplyWaveDifficulty(healthMultiplier);

        EnemyMovement movement =
            enemy.GetComponent<EnemyMovement>();

        if (movement != null)
            movement.ApplyWaveDifficulty(speedMultiplier);

        EnemyAttack enemyAttack =
            enemy.GetComponent<EnemyAttack>();

        if (enemyAttack != null)
            enemyAttack.ApplyWaveDifficulty(attackMultiplier);
    }

    // =========================================
    // FORMATION MOVEMENT
    // =========================================

    private void MoveFormations()
    {
        for (int i = activeFormations.Count - 1; i >= 0; i--)
        {
            Transform root = activeFormations[i];

            if (root == null)
            {
                activeFormations.RemoveAt(i);
                continue;
            }

            root.position +=
                Vector3.back * formationMoveSpeed * Time.deltaTime;

            if (root.position.z < fixedSpawnZ - 50f)
            {
                Destroy(root.gameObject);
                activeFormations.RemoveAt(i);
            }
        }
    }

    // =========================================
    // CLEAR FORMATIONS
    // =========================================

    public void ClearFormations()
    {
        for (int i = activeFormations.Count - 1; i >= 0; i--)
        {
            if (activeFormations[i] != null)
                Destroy(activeFormations[i].gameObject);
        }

        activeFormations.Clear();
    }
}
