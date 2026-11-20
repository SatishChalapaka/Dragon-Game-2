
using UnityEngine;
using UnityEngine.UI;

public class BossWorldHealthBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health bossHealth;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Transform healthBarVisual;

    [Header("Position Settings")]
    [SerializeField] private Vector3 headOffset =
        new Vector3(0f, 2.5f, 0f);

    private Camera mainCamera;

    private void Awake()
    {
        if (bossHealth == null)
            bossHealth = GetComponentInParent<Health>();

        mainCamera = Camera.main;
    }

    private void OnEnable()
{
    if (bossHealth == null)
        return;

    bossHealth.OnHealthChanged += UpdateHealthBar;
    bossHealth.OnDied += HandleBossDeath;

    if (healthBarVisual != null)
        healthBarVisual.gameObject.SetActive(true);

    RefreshHealthBar();
}
    private void Start()
    {
        RefreshHealthBar();
    }

    private void OnDisable()
    {
        if (bossHealth == null)
            return;

        bossHealth.OnHealthChanged -= UpdateHealthBar;
        bossHealth.OnDied -= HandleBossDeath;
    }

    private void LateUpdate()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null || healthBarVisual == null)
            return;

        healthBarVisual.position =
            bossHealth.transform.position + headOffset;

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
        if (bossHealth == null || healthSlider == null)
            return;

        healthSlider.minValue = 0f;
        healthSlider.maxValue = bossHealth.MaxHealth;

        healthSlider.SetValueWithoutNotify(
            bossHealth.CurrentHealth
        );
    }

    private void HandleBossDeath(Health health)
    {
        if (healthBarVisual != null)
            healthBarVisual.gameObject.SetActive(false);
    }
}
