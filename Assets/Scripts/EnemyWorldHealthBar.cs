
using UnityEngine;
using UnityEngine.UI;

public class EnemyWorldHealthBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health enemyHealth;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Transform healthBarVisual;

    [Header("Position Settings")]
    [SerializeField] private Vector3 headOffset =
        new Vector3(0f, 1.5f, 0f);

    private Camera mainCamera;

    private void Awake()
    {
        if (enemyHealth == null)
            enemyHealth = GetComponentInParent<Health>();

        mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        if (enemyHealth != null)
            enemyHealth.OnHealthChanged += UpdateHealthBar;
    }

    private void Start()
    {
        RefreshHealthBar();
    }

    private void OnDisable()
    {
        if (enemyHealth != null)
            enemyHealth.OnHealthChanged -= UpdateHealthBar;
    }

    private void LateUpdate()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null || healthBarVisual == null)
            return;

        // Keep the health bar above the enemy.
        healthBarVisual.position =
            transform.position + headOffset;

        // Make the health bar face the camera.
        healthBarVisual.rotation =
            mainCamera.transform.rotation;
    }

    private void UpdateHealthBar(
        Health health,
        float previousHealth,
        float currentHealth)
    {
        if (healthSlider == null)
            return;

        healthSlider.maxValue = health.MaxHealth;
        healthSlider.SetValueWithoutNotify(currentHealth);
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
}
