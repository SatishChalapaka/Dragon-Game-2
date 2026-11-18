
using System;
using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField, Min(1f)] private float maxHealth = 100f;

    public float MaxHealth => maxHealth;
    public float CurrentHealth { get; private set; }

    public bool IsDead { get; private set; }

    public event Action<Health, float, float> OnHealthChanged;
    public event Action<Health> OnDied;

    private void Awake()
    {
        ResetHealth();
    }

   public void TakeDamage(float damage)
{
    if (IsDead || damage <= 0f)
        return;

    float previousHealth = CurrentHealth;

    CurrentHealth = Mathf.Max(0f, CurrentHealth - damage);

    OnHealthChanged?.Invoke(
        this,
        previousHealth,
        CurrentHealth
    );

    if (CurrentHealth <= 0f)
    {
        Die();
    }

    Debug.Log($"Current Health: {CurrentHealth}");
}

    public void Heal(float amount)
    {
        if (IsDead || amount <= 0f)
            return;

        float previousHealth = CurrentHealth;

        CurrentHealth = Mathf.Min(
            maxHealth,
            CurrentHealth + amount
        );

        OnHealthChanged?.Invoke(
            this,
            previousHealth,
            CurrentHealth
        );
    }

    public void ResetHealth()
    {
        CurrentHealth = maxHealth;
        IsDead = false;
    }

    private void Die()
    {
        if (IsDead)
            return;

        IsDead = true;

        OnDied?.Invoke(this);
    }
}
