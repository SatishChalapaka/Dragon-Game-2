
using System;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        LevelUp,
        GameOver,
        Victory
    }

    public static GameStateManager Instance { get; private set; }

    [Header("Game Settings")]
    [SerializeField] private GameState startingState = GameState.Playing;

    public GameState CurrentState { get; private set; }

    public event Action<GameState> OnGameStateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        CurrentState = startingState;

        ApplyTimeScale();
    }

    public void SetState(GameState newState)
    {
        if (CurrentState == newState)
            return;

        CurrentState = newState;

        ApplyTimeScale();

        OnGameStateChanged?.Invoke(CurrentState);

        Debug.Log("Game State Changed: " + CurrentState);
    }

    private void ApplyTimeScale()
    {
        Time.timeScale =
            CurrentState == GameState.Playing ? 1f : 0f;
    }

    private void OnApplicationPause(bool isPaused)
    {
        // Automatically pause when the mobile app goes
        // into the background during gameplay.
        if (isPaused && CurrentState == GameState.Playing)
        {
            SetState(GameState.Paused);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Time.timeScale = 1f;
            Instance = null;
        }
    }
}
