
using UnityEngine;
using UnityEngine.UI;

public class DragonHealthUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health playerHealth;
    [SerializeField] private Slider healthSlider;

    private void OnEnable()
    {
        if (playerHealth == null || healthSlider == null)
        {
            Debug.LogError("DragonHealthUI: Missing references!", this);
            return;
        }

        playerHealth.OnHealthChanged += UpdateHealthBar;

        // Initialize the slider with current dragon health.
        healthSlider.minValue = 0f;
        healthSlider.maxValue = playerHealth.MaxHealth;

        healthSlider.SetValueWithoutNotify(
            playerHealth.CurrentHealth
        );
    }

    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateHealthBar;
        }
    }

    private void UpdateHealthBar(
        Health health,
        float previousHealth,
        float currentHealth)
    {
        healthSlider.SetValueWithoutNotify(currentHealth);
    }
}
