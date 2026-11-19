
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DragonXPUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private XPManager xpManager;
    [SerializeField] private Slider xpSlider;
    [SerializeField] private TMP_Text levelText;

    private void OnEnable()
    {
        if (xpManager == null)
        {
            Debug.LogError(
                "DragonXPUI: Assign XPManager in the Inspector!",
                this
            );
            return;
        }

        xpManager.OnXPChanged += UpdateXPBar;
        xpManager.OnLevelUp += UpdateLevelText;
    }

    private void Start()
    {
        RefreshUI();
    }

    private void OnDisable()
    {
        if (xpManager == null)
            return;

        xpManager.OnXPChanged -= UpdateXPBar;
        xpManager.OnLevelUp -= UpdateLevelText;
    }

    private void RefreshUI()
    {
        if (xpManager == null)
            return;

        UpdateXPBar(
            xpManager.CurrentXP,
            xpManager.RequiredXP
        );

        UpdateLevelText(xpManager.CurrentLevel);
    }

    private void UpdateXPBar(int currentXP, int requiredXP)
    {
        if (xpSlider == null)
            return;

        xpSlider.minValue = 0f;
        xpSlider.maxValue = Mathf.Max(1, requiredXP);

        xpSlider.SetValueWithoutNotify(currentXP);
    }

    private void UpdateLevelText(int level)
    {
        if (levelText == null)
            return;

        levelText.text = "LEVEL " + level;
    }
}
