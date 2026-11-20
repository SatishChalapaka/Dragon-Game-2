
using UnityEngine;

public class BossContactDamage : MonoBehaviour
{
    [Header("Collision Settings")]
    [SerializeField] private LayerMask dragonLayers;

    [SerializeField, Min(0f)]
    private float collisionDamage = 25f;

    [SerializeField, Min(0.1f)]
    private float damageInterval = 1f;

    private float nextDamageTime;

    private void OnEnable()
    {
        nextDamageTime = 0f;
    }

    private void OnTriggerStay(Collider other)
    {
        if (Time.time < nextDamageTime)
            return;

        Health dragonHealth =
            other.GetComponentInParent<Health>();

        if (dragonHealth == null)
            return;

        int dragonLayer = 1 << dragonHealth.gameObject.layer;

        if ((dragonLayers.value & dragonLayer) == 0)
            return;

        dragonHealth.TakeDamage(collisionDamage);

        nextDamageTime = Time.time + damageInterval;

        Debug.Log("Boss damaged the dragon!");
    }
}
