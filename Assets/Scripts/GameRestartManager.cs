
using UnityEngine;

public class GameRestartManager : MonoBehaviour
{
    [Header("Gameplay References")]
    [SerializeField] private WaveManager waveManager;
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private ObjectPoolManager poolManager;

    [Header("Dragon References")]
    [SerializeField] private DragonMovement dragonMovement;
    [SerializeField] private DragonRotation dragonRotation;
    [SerializeField] private Health dragonHealth;

    private bool isRestarting;

    public void PrepareForMainMenu()
    {
        if (waveManager != null)
            waveManager.PrepareForMainMenu();

        ClearTransientGameplayObjects();

        Debug.Log("Gameplay objects cleared before Main Menu.");
    }

    public void ResumeAfterMainMenu()
    {
        if (waveManager != null)
            waveManager.ResumeAfterMainMenu();
    }

    public void RestartGame()
    {
        if (isRestarting)
            return;

        if (GameStateManager.Instance == null)
        {
            Debug.LogError("GameStateManager instance is missing!");
            return;
        }

        isRestarting = true;

        // Remember the current wave and stop its coroutine.
        if (waveManager != null)
            waveManager.PrepareForMainMenu();

        // Clear old enemies, fireballs, rewards and formations.
        ClearTransientGameplayObjects();

        // Restore the dragon.
        if (dragonMovement != null)
            dragonMovement.ResetMovement();

        if (dragonRotation != null)
            dragonRotation.ResetRotation();

        if (dragonHealth != null)
            dragonHealth.ResetHealth();

        // Do not reset XP or permanent gold here.

        GameStateManager.Instance.SetState(
            GameStateManager.GameState.Playing
        );

        // Start the same wave again.
        if (waveManager != null)
            waveManager.ResumeAfterMainMenu();

        Debug.Log("Current wave restarted.");

        isRestarting = false;
    }

    private void ClearTransientGameplayObjects()
    {
        // Return pooled objects so old fireballs and enemies
        // cannot reappear when GameplayRoot is enabled.
        if (poolManager != null)
            poolManager.ReturnAllActiveObjects();

        // Formation roots are not pooled.
        if (enemySpawner != null)
            enemySpawner.ClearFormations();
    }
}
