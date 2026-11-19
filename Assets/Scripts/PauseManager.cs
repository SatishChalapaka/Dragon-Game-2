
using UnityEngine;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Restart Reference")]
    [SerializeField] private GameRestartManager gameRestartManager;

    private GameStateManager gameStateManager;
    private bool isSubscribed;

    private void OnEnable()
    {
        if (pauseButton != null)
            pauseButton.onClick.AddListener(PauseGame);

        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
    }

    private void Start()
    {
        gameStateManager = GameStateManager.Instance;

        if (gameStateManager == null)
        {
            Debug.LogError("PauseManager: GameStateManager not found!", this);
            return;
        }

        gameStateManager.OnGameStateChanged += HandleGameStateChanged;
        isSubscribed = true;

        HandleGameStateChanged(gameStateManager.CurrentState);
    }

    private void OnDisable()
    {
        if (pauseButton != null)
            pauseButton.onClick.RemoveListener(PauseGame);

        if (resumeButton != null)
            resumeButton.onClick.RemoveListener(ResumeGame);

        if (restartButton != null)
            restartButton.onClick.RemoveListener(RestartGame);

        if (mainMenuButton != null)
            mainMenuButton.onClick.RemoveListener(ReturnToMainMenu);

        if (isSubscribed && gameStateManager != null)
        {
            gameStateManager.OnGameStateChanged -= HandleGameStateChanged;
            isSubscribed = false;
        }
    }

    public void PauseGame()
    {
        if (gameStateManager == null)
            return;

        if (gameStateManager.CurrentState != GameStateManager.GameState.Playing)
            return;

        gameStateManager.SetState(GameStateManager.GameState.Paused);
    }

    public void ResumeGame()
    {
        if (gameStateManager == null)
            return;

        if (gameStateManager.CurrentState != GameStateManager.GameState.Paused)
            return;

        gameStateManager.SetState(GameStateManager.GameState.Playing);
    }

    public void RestartGame()
    {
        if (gameRestartManager == null)
        {
            Debug.LogError("GameRestartManager is not assigned!", this);
            return;
        }

        gameRestartManager.RestartGame();
    }

    public void ReturnToMainMenu()
    {
        if (gameStateManager == null)
            return;

        gameStateManager.SetState(GameStateManager.GameState.MainMenu);
    }

    private void HandleGameStateChanged(GameStateManager.GameState newState)
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(
                newState == GameStateManager.GameState.Paused
            );
        }
    }
}
