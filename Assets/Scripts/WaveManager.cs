
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnemySpawnEntry
{
    public GameObject enemyPrefab;

    [Min(0)]
    public int count = 5;

    [Min(0.01f)]
    public float spawnInterval = 2f;
}

[Serializable]
public class FormationSpawnEntry
{
    public GameObject enemyPrefab;

    [Min(0)]
    public int formationsToSpawn = 1;

    [Min(1)]
    public int enemiesPerFormation = 5;

    [Min(0.01f)]
    public float formationSpawnInterval = 10f;
}

[Serializable]
public class WaveData
{
    [Header("Enemy Configuration")]
    public List<EnemySpawnEntry> enemies = new List<EnemySpawnEntry>();

    [Header("Formation Configuration")]
    public List<FormationSpawnEntry> formations =
        new List<FormationSpawnEntry>();

    [Header("Boss Configuration")]
    public GameObject bossPrefab;

    [Header("Difficulty Settings")]
    public float healthMultiplier = 1f;
    public float speedMultiplier = 1f;
    public float attackMultiplier = 1f;
}

[Serializable]
public class LevelData
{
    public string levelName = "Level 1";

    public List<WaveData> waves = new List<WaveData>();
}

public class WaveManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemySpawner enemySpawner;

    [Header("Level Configuration")]
    [SerializeField] private List<LevelData> levels =
        new List<LevelData>();

    [Header("Start Settings")]
    [SerializeField] private bool startAutomatically = true;
    [SerializeField] private float delayBeforeFirstWave = 3f;

    [Header("Object Pool Settings")]
    [SerializeField] private int prewarmAmountPerPrefab = 10;

    [Header("Save Settings")]
    [SerializeField] private string saveKey = "DragonFury_Progress";

    private int currentLevelIndex;
    private int currentWaveIndex = -1;
    private int completedWaveIndex = -1;
    public int TotalLevelCount => levels.Count;

    private int activeEnemyCount;

    private bool isWaveActive;
    private bool isWaitingForNextWave;
    private bool isSuspendedForMenu;

    private int waveIndexToResume = -1;

    private Coroutine waveCoroutine;

    public int CurrentLevelNumber => currentLevelIndex + 1;
    public int CurrentWaveNumber => currentWaveIndex + 1;
    public int ActiveEnemyCount => activeEnemyCount;

    public bool IsWaveActive => isWaveActive;
    public bool IsWaitingForNextWave => isWaitingForNextWave;

    public int TotalWavesInCurrentLevel =>
        GetCurrentLevelWaveCount();

    public event Action<int> OnWaveStarted;
    public event Action<int> OnWaveCompleted;
    public event Action<int> OnLevelCompleted;
    public event Action OnAllLevelsCompleted;

    private void Start()
    {
        if (enemySpawner == null)
        {
            Debug.LogError("EnemySpawner reference is missing!");
            enabled = false;
            return;
        }

        if (!ValidateLevels())
        {
            enabled = false;
            return;
        }

        LoadProgress();
        PrewarmConfiguredPrefabs();

        if (startAutomatically)
            StartNextWave();
    }

    // =========================================
    // LEVEL VALIDATION
    // =========================================

    private bool ValidateLevels()
    {
        if (levels == null || levels.Count == 0)
        {
            Debug.LogError("No levels configured!");
            return false;
        }

        for (int i = 0; i < levels.Count; i++)
        {
            if (levels[i] == null || levels[i].waves.Count == 0)
            {
                Debug.LogError("Level " + (i + 1) + " has no waves!");
                return false;
            }
        }

        return true;
    }

    private int GetCurrentLevelWaveCount()
    {
        if (currentLevelIndex < 0 ||
            currentLevelIndex >= levels.Count)
            return 0;

        return levels[currentLevelIndex].waves.Count;
    }

    // =========================================
    // PREWARM MANUAL PREFABS
    // =========================================

    private void PrewarmConfiguredPrefabs()
    {
        foreach (LevelData level in levels)
        {
            foreach (WaveData wave in level.waves)
            {
                foreach (EnemySpawnEntry entry in wave.enemies)
                {
                    if (entry.enemyPrefab != null)
                    {
                        enemySpawner.PrewarmWavePrefab(
                            entry.enemyPrefab,
                            prewarmAmountPerPrefab
                        );
                    }
                }

                foreach (FormationSpawnEntry entry in wave.formations)
                {
                    if (entry.enemyPrefab != null)
                    {
                        enemySpawner.PrewarmWavePrefab(
                            entry.enemyPrefab,
                            prewarmAmountPerPrefab
                        );
                    }
                }

                if (wave.bossPrefab != null)
                {
                    enemySpawner.PrewarmWavePrefab(
                        wave.bossPrefab,
                        1
                    );
                }
            }
        }
    }

    // =========================================
    // START NEXT WAVE
    // =========================================

    public void StartNextWave()
    {
        if (!enabled || isWaveActive ||
            waveCoroutine != null || isSuspendedForMenu)
            return;

        if (currentWaveIndex >= GetCurrentLevelWaveCount() - 1)
        {
            if (completedWaveIndex >= GetCurrentLevelWaveCount() - 1)
            {
                if (!AdvanceToNextLevel())
                    return;
            }
        }

        if (currentWaveIndex >= GetCurrentLevelWaveCount() - 1)
        {
            Debug.Log("No more waves available in this level.");
            return;
        }

        isWaitingForNextWave = false;

        waveCoroutine = StartCoroutine(StartWaveRoutine());

        
    }

    private bool AdvanceToNextLevel()
    {
        if (currentLevelIndex >= levels.Count - 1)
        {
            Debug.Log("All levels completed!");
            OnAllLevelsCompleted?.Invoke();
            return false;
        }

        OnLevelCompleted?.Invoke(CurrentLevelNumber);

        currentLevelIndex++;
        currentWaveIndex = -1;
        completedWaveIndex = -1;

        SaveProgress();

        Debug.Log("Starting " + levels[currentLevelIndex].levelName);

        return true;
    }

    // =========================================
    // WAVE ROUTINE
    // =========================================

    private IEnumerator StartWaveRoutine()
    {
        float delay = currentWaveIndex == -1
            ? delayBeforeFirstWave
            : 0f;

        yield return new WaitForSeconds(delay);

        if (isSuspendedForMenu)
        {
            waveCoroutine = null;
            yield break;
        }

        currentWaveIndex++;

        LevelData currentLevel = levels[currentLevelIndex];
        WaveData currentWave = currentLevel.waves[currentWaveIndex];

        isWaveActive = true;
        isWaitingForNextWave = false;
        activeEnemyCount = 0;

        Debug.Log(
            currentLevel.levelName +
            " - Wave " + CurrentWaveNumber + " Started!"
        );

        OnWaveStarted?.Invoke(CurrentWaveNumber);

        // Spawn individual enemies.
        foreach (EnemySpawnEntry entry in currentWave.enemies)
        {
            if (entry.enemyPrefab == null)
            {
                Debug.LogWarning("An enemy prefab is missing in Wave " +
                                 CurrentWaveNumber);
                continue;
            }

            for (int i = 0; i < entry.count; i++)
            {
                enemySpawner.SpawnWaveEnemy(
                    this,
                    entry.enemyPrefab,
                    currentWave.healthMultiplier,
                    currentWave.speedMultiplier,
                    currentWave.attackMultiplier
                );

                if (i < entry.count - 1)
                {
                    yield return new WaitForSeconds(
                        Mathf.Max(0.01f, entry.spawnInterval)
                    );
                }

                if (isSuspendedForMenu)
                {
                    waveCoroutine = null;
                    yield break;
                }
            }
        }

        // Spawn formations.
        foreach (FormationSpawnEntry entry in currentWave.formations)
        {
            if (entry.enemyPrefab == null)
            {
                Debug.LogWarning("A formation prefab is missing in Wave " +
                                 CurrentWaveNumber);
                continue;
            }

            for (int i = 0; i < entry.formationsToSpawn; i++)
            {
                enemySpawner.SpawnWaveFormation(
                    this,
                    entry.enemyPrefab,
                    currentWave.healthMultiplier,
                    currentWave.speedMultiplier,
                    currentWave.attackMultiplier,
                    entry.enemiesPerFormation
                );

                if (i < entry.formationsToSpawn - 1)
                {
                    yield return new WaitForSeconds(
                        Mathf.Max(0.01f, entry.formationSpawnInterval)
                    );
                }

                if (isSuspendedForMenu)
                {
                    waveCoroutine = null;
                    yield break;
                }
            }
        }

        // Spawn boss if assigned.
        if (currentWave.bossPrefab != null)
        {
            bool bossSpawned = enemySpawner.SpawnWaveBoss(
                this,
                currentWave.bossPrefab,
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

        Debug.Log(
            "Wave spawning finished. Active enemies: " +
            activeEnemyCount
        );

        // Wait until every registered enemy and boss is defeated.
        while (activeEnemyCount > 0)
        {
            if (isSuspendedForMenu)
            {
                waveCoroutine = null;
                yield break;
            }

            yield return null;
        }

        isWaveActive = false;
        isWaitingForNextWave = true;

        completedWaveIndex = currentWaveIndex;

        SaveProgress();

        Debug.Log(
            "Wave " + CurrentWaveNumber + " Completed!"
        );

        OnWaveCompleted?.Invoke(CurrentWaveNumber);

        if (currentWaveIndex == GetCurrentLevelWaveCount() - 1 &&
            currentLevelIndex == levels.Count - 1)
        {
            Debug.Log("Congratulations! All levels completed!");
            OnAllLevelsCompleted?.Invoke();
        }

        waveCoroutine = null;

        // Important: Do not automatically start another wave.
        // The player must press the Next button.
    }

    // =========================================
    // ENEMY TRACKING
    // =========================================

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

    // =========================================
    // MAIN MENU SUSPENSION
    // =========================================

    public void PrepareForMainMenu()
    {
        if (isSuspendedForMenu)
            return;

        if (isWaveActive)
            waveIndexToResume = currentWaveIndex;
        else
            waveIndexToResume = currentWaveIndex + 1;

        if (waveCoroutine != null)
        {
            StopCoroutine(waveCoroutine);
            waveCoroutine = null;
        }

        isWaveActive = false;
        isSuspendedForMenu = true;

        Debug.Log("Gameplay suspended.");
    }

    public void ResumeAfterMainMenu()
    {
        if (!isSuspendedForMenu)
            return;

        if (isWaveActive == false &&
            waveIndexToResume >= 0 &&
            waveIndexToResume <= GetCurrentLevelWaveCount())
        {
            currentWaveIndex = waveIndexToResume - 1;
        }

        activeEnemyCount = 0;

        isWaveActive = false;
        isSuspendedForMenu = false;
        waveCoroutine = null;

        if (completedWaveIndex == currentWaveIndex &&
            isWaitingForNextWave)
        {
            StartNextWave();
        }
        else
        {
            isWaitingForNextWave = false;
            StartNextWave();
        }
    }

    public void RestartCurrentWave()
    {
        if (GetCurrentLevelWaveCount() == 0)
            return;

        if (waveCoroutine != null)
        {
            StopCoroutine(waveCoroutine);
            waveCoroutine = null;
        }

        currentWaveIndex = Mathf.Max(-1, currentWaveIndex - 1);
        completedWaveIndex = Mathf.Min(
            completedWaveIndex,
            currentWaveIndex
        );

        isWaveActive = false;
        isWaitingForNextWave = false;
        isSuspendedForMenu = false;
        activeEnemyCount = 0;

        enemySpawner.ClearFormations();

        StartNextWave();
    }

    // =========================================
    // PLAYER PREFS SAVE SYSTEM
    // =========================================

    private void SaveProgress()
    {
        PlayerPrefs.SetInt(
            saveKey + "_Level",
            currentLevelIndex
        );

        PlayerPrefs.SetInt(
            saveKey + "_CompletedWave",
            completedWaveIndex
        );

        PlayerPrefs.Save();

        Debug.Log(
            "Progress saved. Level: " + CurrentLevelNumber +
            ", Completed Wave: " + (completedWaveIndex + 1)
        );
    }

    private void LoadProgress()
    {
        currentLevelIndex = PlayerPrefs.GetInt(
            saveKey + "_Level",
            0
        );

        currentLevelIndex = Mathf.Clamp(
            currentLevelIndex,
            0,
            levels.Count - 1
        );

        completedWaveIndex = PlayerPrefs.GetInt(
            saveKey + "_CompletedWave",
            -1
        );

        completedWaveIndex = Mathf.Clamp(
            completedWaveIndex,
            -1,
            levels[currentLevelIndex].waves.Count - 1
        );

        currentWaveIndex = completedWaveIndex;

        Debug.Log(
            "Loaded progress. Level: " + CurrentLevelNumber +
            ", Completed Wave: " + (completedWaveIndex + 1)
        );
    }

    public void ResetSavedProgress()
    {
        PlayerPrefs.DeleteKey(saveKey + "_Level");
        PlayerPrefs.DeleteKey(saveKey + "_CompletedWave");
        PlayerPrefs.Save();

        currentLevelIndex = 0;
        currentWaveIndex = -1;
        completedWaveIndex = -1;

        activeEnemyCount = 0;
        isWaveActive = false;
        isWaitingForNextWave = false;
        isSuspendedForMenu = false;

        Debug.Log("Saved wave progress reset.");
    }

    public void StartFromBeginning()
    {
        if (waveCoroutine != null)
        {
            StopCoroutine(waveCoroutine);
            waveCoroutine = null;
        }

        ResetSavedProgress();

        StartNextWave();
    }
}
