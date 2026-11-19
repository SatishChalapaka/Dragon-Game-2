
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject gameplayHUD;
    [SerializeField] private GameObject settingsPanel;

    [Header("Gameplay Reference")]
    [SerializeField] private GameObject gameplayRoot;
    [SerializeField] private GameRestartManager gameRestartManager;

    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    private GameStateManager gameStateManager;
    private bool isSubscribed;
    private bool hasEnteredGameplay;

    private void OnEnable()
    {
        if (playButton != null)
            playButton.onClick.AddListener(StartGame);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
    }

    private void Start()
    {
        gameStateManager = GameStateManager.Instance;

        if (gameStateManager == null)
        {
            Debug.LogError(
                "MainMenuController: GameStateManager instance not found!",
                this
            );
            return;
        }

        gameStateManager.OnGameStateChanged += HandleGameStateChanged;
        isSubscribed = true;

        HandleGameStateChanged(gameStateManager.CurrentState);
    }

    private void OnDisable()
    {
        if (playButton != null)
            playButton.onClick.RemoveListener(StartGame);

        if (settingsButton != null)
            settingsButton.onClick.RemoveListener(OpenSettings);

        if (quitButton != null)
            quitButton.onClick.RemoveListener(QuitGame);

        if (isSubscribed && gameStateManager != null)
        {
            gameStateManager.OnGameStateChanged -= HandleGameStateChanged;
            isSubscribed = false;
        }
    }

    public void StartGame()
    {
        if (gameStateManager == null)
            return;

        if (gameStateManager.CurrentState !=
            GameStateManager.GameState.MainMenu)
            return;

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        gameStateManager.SetState(
            GameStateManager.GameState.Playing
        );
    }

    private void OpenSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    private void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }

    private void HandleGameStateChanged(
        GameStateManager.GameState newState)
    {
        bool isMainMenu =
            newState == GameStateManager.GameState.MainMenu;

        if (isMainMenu)
        {
            // Only clear objects if we are returning from gameplay.
            if (hasEnteredGameplay && gameRestartManager != null)
                gameRestartManager.PrepareForMainMenu();

            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(true);

            if (gameplayHUD != null)
                gameplayHUD.SetActive(false);

            if (gameplayRoot != null)
                gameplayRoot.SetActive(false);

            return;
        }

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        if (gameplayHUD != null)
            gameplayHUD.SetActive(true);

        if (gameplayRoot != null)
            gameplayRoot.SetActive(true);

        // On the first Play, WaveManager.Start handles automatic startup.
        // On later Plays, resume the interrupted wave explicitly.
        if (hasEnteredGameplay && gameRestartManager != null)
            gameRestartManager.ResumeAfterMainMenu();

        hasEnteredGameplay = true;

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }
}
