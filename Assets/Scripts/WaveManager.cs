
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WaveData
{
    [Header("Wave Settings")]
    public int enemiesToSpawn = 5;
    public int formationsToSpawn = 0;

    [Header("Boss Settings")]
    public bool isBossWave = false;
    public int enemiesPerFormation = 5;

    [Header("Spawn Timing")]
    public float enemySpawnInterval = 2f;
    public float formationSpawnInterval = 10f;

    [Header("Difficulty Settings")]
    public float healthMultiplier = 1f;
    public float speedMultiplier = 1f;
    public float attackMultiplier = 1f;
}

public class WaveManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemySpawner enemySpawner;

    [Header("Wave Configuration")]
    [SerializeField] private List<WaveData> waves = new List<WaveData>();

    [Header("Start Settings")]
    [SerializeField] private bool startAutomatically = true;
    [SerializeField] private float delayBeforeFirstWave = 3f;
    [SerializeField] private float delayBetweenWaves = 5f;

    private int currentWaveIndex = -1;
    private int activeEnemyCount;

    private bool isWaveActive;
    private Coroutine waveCoroutine;

    public int CurrentWaveNumber => currentWaveIndex + 1;
    public int ActiveEnemyCount => activeEnemyCount;
    public bool IsWaveActive => isWaveActive;

    public event Action<int> OnWaveStarted;
    public event Action<int> OnWaveCompleted;

    private void Start()
    {
        if (enemySpawner == null)
        {
            Debug.LogError("EnemySpawner reference is missing!");
            enabled = false;
            return;
        }

        if (waves.Count == 0)
        {
            Debug.LogError("No waves configured!");
            enabled = false;
            return;
        }

        if (startAutomatically)
        {
            StartNextWave();
        }
    }

    public void StartNextWave()
    {
        if (isWaveActive || waveCoroutine != null)
            return;

        if (currentWaveIndex >= waves.Count - 1)
        {
            Debug.Log("All configured waves completed!");
            return;
        }

        waveCoroutine = StartCoroutine(StartWaveRoutine());
    }

    private IEnumerator StartWaveRoutine()
    {
        float delay = currentWaveIndex == -1
            ? delayBeforeFirstWave
            : delayBetweenWaves;

        yield return new WaitForSeconds(delay);

        currentWaveIndex++;

        WaveData currentWave = waves[currentWaveIndex];

        isWaveActive = true;
        activeEnemyCount = 0;

        Debug.Log("Wave " + CurrentWaveNumber + " Started!");

        OnWaveStarted?.Invoke(CurrentWaveNumber);

        // =====================================
        // BOSS WAVE
        // =====================================

        if (currentWave.isBossWave)
        {
            Debug.Log("BOSS WAVE!");

            bool bossSpawned = enemySpawner.SpawnWaveBoss(
                this,
                currentWave.healthMultiplier,
                currentWave.speedMultiplier,
                currentWave.attackMultiplier
            );

            if (!bossSpawned)
            {
                Debug.LogError(
                    "Boss wave could not start. Check the boss prefab and pool setup."
                );

                isWaveActive = false;
                waveCoroutine = null;
                yield break;
            }
        }
        else
        {
            // =====================================
            // INDIVIDUAL ENEMIES
            // =====================================

            for (int i = 0; i < currentWave.enemiesToSpawn; i++)
            {
                enemySpawner.SpawnWaveEnemy(
                    this,
                    currentWave.healthMultiplier,
                    currentWave.speedMultiplier,
                    currentWave.attackMultiplier
                );

                if (i < currentWave.enemiesToSpawn - 1)
                {
                    yield return new WaitForSeconds(
                        Mathf.Max(0.01f, currentWave.enemySpawnInterval)
                    );
                }
            }

            // =====================================
            // FORMATIONS
            // =====================================

            for (int i = 0; i < currentWave.formationsToSpawn; i++)
            {
                enemySpawner.SpawnWaveFormation(
                    this,
                    currentWave.healthMultiplier,
                    currentWave.speedMultiplier,
                    currentWave.attackMultiplier,
                    currentWave.enemiesPerFormation
                );

                if (i < currentWave.formationsToSpawn - 1)
                {
                    yield return new WaitForSeconds(
                        Mathf.Max(0.01f, currentWave.formationSpawnInterval)
                    );
                }
            }
        }

        Debug.Log(
            "Wave " + CurrentWaveNumber +
            " finished spawning. Active enemies: " + activeEnemyCount
        );

        // Wait until all tracked enemies, including the boss, return to pool.
        while (activeEnemyCount > 0)
        {
            yield return null;
        }

        isWaveActive = false;

        Debug.Log("Wave " + CurrentWaveNumber + " Completed!");

        OnWaveCompleted?.Invoke(CurrentWaveNumber);

        waveCoroutine = null;

        // Automatically start the next wave.
        if (currentWaveIndex < waves.Count - 1)
        {
            StartNextWave();
        }
        else
        {
            Debug.Log("Congratulations! All waves completed!");
        }
    }

    public void RegisterEnemy()
    {
        activeEnemyCount++;

        Debug.Log("Active Enemies: " + activeEnemyCount);
    }

    public void UnregisterEnemy()
    {
        activeEnemyCount = Mathf.Max(0, activeEnemyCount - 1);

        Debug.Log("Active Enemies: " + activeEnemyCount);
    }
}
