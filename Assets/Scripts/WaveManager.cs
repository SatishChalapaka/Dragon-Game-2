
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
    private bool isSuspendedForMenu;
    private int waveIndexToResume;
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
            StartNextWave();
    }

    public void StartNextWave()
    {
        if (!enabled || isWaveActive || waveCoroutine != null)
            return;

        if (currentWaveIndex >= waves.Count - 1)
        {
            Debug.Log("All configured waves completed!");
            return;
        }

        waveCoroutine = StartCoroutine(StartWaveRoutine());
    }

    // Called when the player returns to the Main Menu.
    public void PrepareForMainMenu()
    {
        if (isSuspendedForMenu)
            return;

        // If a wave is running, restart that same wave.
        // If waiting for the next wave, continue with the next one.
        if (isWaveActive)
            waveIndexToResume = currentWaveIndex;
        else
            waveIndexToResume = currentWaveIndex + 1;

        waveIndexToResume = Mathf.Clamp(
            waveIndexToResume,
            0,
            waves.Count - 1
        );

        if (waveCoroutine != null)
        {
            StopCoroutine(waveCoroutine);
            waveCoroutine = null;
        }

        isWaveActive = false;
        activeEnemyCount = 0;
        isSuspendedForMenu = true;

        Debug.Log(
            "Gameplay suspended. Will resume Wave " +
            (waveIndexToResume + 1)
        );
    }

    // Called when the player presses Play from the Main Menu.
    public void ResumeAfterMainMenu()
    {
        if (!isSuspendedForMenu)
            return;

        currentWaveIndex = waveIndexToResume - 1;

        activeEnemyCount = 0;
        isWaveActive = false;
        waveCoroutine = null;
        isSuspendedForMenu = false;

        Debug.Log(
            "Resuming Wave " + (waveIndexToResume + 1)
        );

        StartNextWave();
    }

    // Used when the player presses Restart during a paused wave.
    public void RestartCurrentWave()
    {
        if (waves.Count == 0)
            return;

        PrepareForMainMenu();
        ResumeAfterMainMenu();
    }

    public void ResetWaves()
    {
        if (waveCoroutine != null)
        {
            StopCoroutine(waveCoroutine);
            waveCoroutine = null;
        }

        currentWaveIndex = -1;
        activeEnemyCount = 0;
        isWaveActive = false;
        isSuspendedForMenu = false;

        Debug.Log("Wave progress reset.");
    }

    public void StartFromBeginning()
    {
        ResetWaves();
        StartNextWave();
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
                Debug.LogError("Boss wave could not start!");
                isWaveActive = false;
                waveCoroutine = null;
                yield break;
            }
        }
        else
        {
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

        while (activeEnemyCount > 0)
            yield return null;

        isWaveActive = false;

        Debug.Log("Wave " + CurrentWaveNumber + " Completed!");
        OnWaveCompleted?.Invoke(CurrentWaveNumber);

        waveCoroutine = null;

        if (currentWaveIndex < waves.Count - 1)
            StartNextWave();
        else
            Debug.Log("Congratulations! All waves completed!");
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
