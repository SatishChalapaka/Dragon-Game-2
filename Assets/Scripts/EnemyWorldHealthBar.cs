
using UnityEngine;
using UnityEngine.UI;

public class EnemyWorldHealthBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health enemyHealth;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Transform healthBarVisual;

    [Header("Position Settings")]
    [SerializeField]
    private Vector3 headOffset =
        new Vector3(0f, 1.5f, 0f);

    private Camera mainCamera;
    private CanvasGroup canvasGroup;

    private bool hasBeenHit;

    private void Awake()
    {
        if (enemyHealth == null)
            enemyHealth = GetComponentInParent<Health>();

        if (healthBarVisual != null)
        {
            canvasGroup =
                healthBarVisual.GetComponent<CanvasGroup>();

            if (canvasGroup == null)
                canvasGroup =
                    healthBarVisual.gameObject.AddComponent<CanvasGroup>();
        }

        mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        hasBeenHit = false;

        if (enemyHealth != null)
        {
            enemyHealth.OnHealthChanged += UpdateHealthBar;
            RefreshHealthBar();
        }

        HideHealthBar();
    }

    private void OnDisable()
    {
        if (enemyHealth != null)
            enemyHealth.OnHealthChanged -= UpdateHealthBar;

        HideHealthBar();
    }

    private void LateUpdate()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null ||
            healthBarVisual == null ||
            enemyHealth == null)
            return;

        healthBarVisual.position =
            enemyHealth.transform.position + headOffset;

        healthBarVisual.rotation =
            mainCamera.transform.rotation;
    }

    private void UpdateHealthBar(
        Health health,
        float previousHealth,
        float currentHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = health.MaxHealth;

            healthSlider.SetValueWithoutNotify(currentHealth);
        }

        // Show the health bar only when actual damage occurs.
        if (currentHealth < previousHealth && !hasBeenHit)
        {
            hasBeenHit = true;
            ShowHealthBar();
        }
    }

    private void RefreshHealthBar()
    {
        if (enemyHealth == null || healthSlider == null)
            return;

        healthSlider.minValue = 0f;
        healthSlider.maxValue = enemyHealth.MaxHealth;

        healthSlider.SetValueWithoutNotify(
            enemyHealth.CurrentHealth
        );
    }

    private void ShowHealthBar()
    {
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = 1f;
    }

    private void HideHealthBar()
    {
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = 0f;
    }
}
