
using UnityEngine;

[RequireComponent(typeof(PooledObject))]
public class EnemyWaveTracker : MonoBehaviour
{
    private WaveManager waveManager;
    private PooledObject pooledObject;
    private bool isRegistered;

    private void Awake()
    {
        pooledObject = GetComponent<PooledObject>();
    }

    private void OnEnable()
    {
        PooledObject.OnReturnedToPool += HandleReturnedToPool;
    }

    private void OnDisable()
    {
        PooledObject.OnReturnedToPool -= HandleReturnedToPool;
    }

    public void Register(WaveManager manager)
    {
        if (isRegistered)
            return;

        waveManager = manager;
        isRegistered = true;

        waveManager.RegisterEnemy();
    }

    private void HandleReturnedToPool(PooledObject returnedObject)
    {
        if (!isRegistered || returnedObject != pooledObject)
            return;

        isRegistered = false;

        if (waveManager != null)
        {
            waveManager.UnregisterEnemy();
        }

        waveManager = null;
    }
}
