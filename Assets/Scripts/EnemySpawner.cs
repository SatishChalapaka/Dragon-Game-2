
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform dragon;
    [SerializeField] private ObjectPoolManager poolManager;

    [Header("Individual Enemies")]
    [SerializeField] private List<GameObject> enemyPrefabs;
    [SerializeField] private float spawnInterval = 2f;

    [Header("Formation Enemies")]
    [SerializeField] private List<GameObject> formationEnemyPrefabs;
    [SerializeField] private float formationInterval = 10f;
    [SerializeField] private float formationMoveSpeed = 5f;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnDistance = 30f;
    [SerializeField] private Vector2 spawnXRange = new Vector2(-8f, 8f);

    [Header("Formation Settings")]
    [SerializeField] private Vector3[] formationOffsets =
    {
        new Vector3(0f, 0f, 0f),
        new Vector3(-2f, 0f, 2f),
        new Vector3(2f, 0f, 2f),
        new Vector3(-4f, 0f, 4f),
        new Vector3(4f, 0f, 4f)
    };

    [Header("Pool Settings")]
    [SerializeField] private int initialPoolSize = 10;

    private float spawnTimer;
    private float formationTimer;

    private readonly List<Transform> activeFormations =
        new List<Transform>();

    private void Start()
    {
        if (dragon == null || poolManager == null)
        {
            Debug.LogError("Dragon or ObjectPoolManager is missing!");
            enabled = false;
            return;
        }

        PrewarmPrefabs(enemyPrefabs);
        PrewarmPrefabs(formationEnemyPrefabs);
    }

    private void Update()
    {
        spawnTimer += Time.deltaTime;
        formationTimer += Time.deltaTime;

        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            SpawnEnemy();
        }

        if (formationTimer >= formationInterval)
        {
            formationTimer = 0f;
            SpawnFormation();
        }

        MoveFormations();
    }

    private void PrewarmPrefabs(List<GameObject> prefabs)
    {
        foreach (GameObject prefab in prefabs)
        {
            if (prefab != null)
                poolManager.Prewarm(prefab, initialPoolSize);
        }
    }

    private void InitializeEnemy(GameObject enemy)
    {
        if (enemy == null)
            return;

        EnemyAttack enemyAttack =
            enemy.GetComponent<EnemyAttack>();

        if (enemyAttack != null)
        {
            enemyAttack.Initialize(dragon, poolManager);
        }
    }

    private void SpawnEnemy()
    {
        if (enemyPrefabs.Count == 0)
            return;

        GameObject prefab = enemyPrefabs[
            Random.Range(0, enemyPrefabs.Count)
        ];

        if (prefab == null)
            return;

        float randomX = Random.Range(
            spawnXRange.x,
            spawnXRange.y
        );

        Vector3 spawnPosition = new Vector3(
            dragon.position.x + randomX,
            dragon.position.y,
            dragon.position.z + spawnDistance
        );

        GameObject enemy = poolManager.Get(
            prefab,
            spawnPosition,
            Quaternion.identity
        );

        InitializeEnemy(enemy);
    }

    private void SpawnFormation()
    {
        if (formationEnemyPrefabs.Count == 0)
            return;

        GameObject prefab = formationEnemyPrefabs[
            Random.Range(0, formationEnemyPrefabs.Count)
        ];

        if (prefab == null)
            return;

        Vector3 rootPosition = new Vector3(
            dragon.position.x,
            dragon.position.y,
            dragon.position.z + spawnDistance
        );

        GameObject rootObject = new GameObject("EnemyFormationRoot");
        rootObject.transform.position = rootPosition;

        activeFormations.Add(rootObject.transform);

        foreach (Vector3 offset in formationOffsets)
        {
            Vector3 spawnPosition =
                rootObject.transform.TransformPoint(offset);

            GameObject enemy = poolManager.Get(
                prefab,
                spawnPosition,
                Quaternion.identity
            );

            if (enemy == null)
                continue;

            EnemyMovement movement =
                enemy.GetComponent<EnemyMovement>();

            if (movement != null)
            {
                movement.SetFormation(
                    rootObject.transform,
                    offset
                );
            }

            InitializeEnemy(enemy);
        }
    }

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

            if (root.position.z < dragon.position.z - 20f)
            {
                Destroy(root.gameObject);
                activeFormations.RemoveAt(i);
            }
        }
    }
}
