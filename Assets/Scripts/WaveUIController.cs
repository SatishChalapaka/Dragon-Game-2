
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WaveUIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WaveManager waveManager;

    [Header("Wave Complete UI")]
    [SerializeField] private GameObject waveCompletePanel;
    [SerializeField] private TMP_Text waveNumberText;
    [SerializeField] private Button nextButton;

    private void Awake()
    {
        if (waveCompletePanel != null)
            waveCompletePanel.SetActive(false);

        if (nextButton != null)
            nextButton.onClick.AddListener(OnNextButtonClicked);
    }

    private void OnEnable()
    {
        if (waveManager == null)
            return;

        waveManager.OnWaveCompleted += ShowWaveCompletePanel;
        waveManager.OnWaveStarted += HideWaveCompletePanel;
    }

    private void OnDisable()
    {
        if (waveManager != null)
        {
            waveManager.OnWaveCompleted -= ShowWaveCompletePanel;
            waveManager.OnWaveStarted -= HideWaveCompletePanel;
        }

        if (nextButton != null)
            nextButton.onClick.RemoveListener(OnNextButtonClicked);
    }

    private void ShowWaveCompletePanel(int waveNumber)
    {
        if (waveCompletePanel != null)
            waveCompletePanel.SetActive(true);

        if (waveNumberText != null)
        {
            waveNumberText.text = "STAGE " +
                                  waveNumber +
                                  " COMPLETED!";
        }

        // Hide Next on the final wave of the final level.
        if (nextButton != null)
        {
            bool allLevelsCompleted =
                waveManager.CurrentLevelNumber ==
                GetTotalLevels() &&
                waveManager.CurrentWaveNumber ==
                waveManager.TotalWavesInCurrentLevel;

            nextButton.gameObject.SetActive(!allLevelsCompleted);
        }
    }

    private void HideWaveCompletePanel(int waveNumber)
    {
        if (waveCompletePanel != null)
            waveCompletePanel.SetActive(false);
    }

    private void OnNextButtonClicked()
    {
        if (waveManager == null)
            return;

        // if (nextButton != null)
        //     nextButton.interactable = false;

        waveManager.StartNextWave();
    }

    private int GetTotalLevels()
    {
        // The level count is supplied through the WaveManager below.
        return waveManager.TotalLevelCount;
    }
}
