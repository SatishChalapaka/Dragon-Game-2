
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

    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    private GameStateManager gameStateManager;
    private bool isSubscribed;

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
        // Get the singleton after all active Awake methods have run.
        gameStateManager = GameStateManager.Instance;

        if (gameStateManager == null)
        {
            Debug.LogError(
                "MainMenuController: GameStateManager instance not found! " +
                "Check that GameStateManager is active and outside GameplayRoot.",
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

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(isMainMenu);

        if (gameplayHUD != null)
            gameplayHUD.SetActive(!isMainMenu);

        if (gameplayRoot != null)
            gameplayRoot.SetActive(!isMainMenu);

        if (settingsPanel != null && !isMainMenu)
            settingsPanel.SetActive(false);
    }
}
