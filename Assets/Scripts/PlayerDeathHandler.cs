
using System;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class PlayerDeathHandler : MonoBehaviour
{
    [Header("Death Effect")]
    [SerializeField] private GameObject deathEffectPrefab;
    [SerializeField] private ObjectPoolManager poolManager;

    [Header("Disable On Death")]
    [SerializeField] private MonoBehaviour[] disableOnDeath;
    [SerializeField] private Collider[] collidersToDisable;

    private Health health;
    private bool hasHandledDeath;

    public event Action PlayerDied;

    private void Awake()
    {
        health = GetComponent<Health>();

        if (poolManager == null)
            poolManager = ObjectPoolManager.Instance;
    }

    private void OnEnable()
    {
        hasHandledDeath = false;

        if (health != null)
            health.OnDied += HandlePlayerDeath;
    }

    private void OnDisable()
    {
        if (health != null)
            health.OnDied -= HandlePlayerDeath;
    }

    private void HandlePlayerDeath(Health deadHealth)
    {
        if (hasHandledDeath)
            return;

        hasHandledDeath = true;

        PlayDeathEffect();

        DisablePlayerControls();

        DisablePlayerColliders();

        PlayerDied?.Invoke();

        Debug.Log("Player has died!");
    }

    private void PlayDeathEffect()
    {
        if (deathEffectPrefab == null)
            return;

        if (poolManager == null)
            poolManager = ObjectPoolManager.Instance;

        if (poolManager == null)
        {
            Debug.LogWarning(
                "ObjectPoolManager not found. Cannot play player death effect."
            );
            return;
        }

        poolManager.Get(
            deathEffectPrefab,
            transform.position,
            Quaternion.identity
        );
    }

    private void DisablePlayerControls()
    {
        foreach (MonoBehaviour component in disableOnDeath)
        {
            if (component == null || component == this)
                continue;

            component.enabled = false;
        }
    }

    private void DisablePlayerColliders()
    {
        foreach (Collider playerCollider in collidersToDisable)
        {
            if (playerCollider != null)
                playerCollider.enabled = false;
        }
    }
}
