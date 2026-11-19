using System;
using UnityEngine;

public class PooledObject : MonoBehaviour
{
    private ObjectPoolManager poolManager;

    public GameObject Prefab { get; private set; }

    public bool IsInPool { get; private set; }
public static event Action<PooledObject> OnReturnedToPool;
    public void Initialize(
        ObjectPoolManager manager,
        GameObject prefab)
    {
        poolManager = manager;
        Prefab = prefab;

        // Newly created objects are not yet stored in the pool.
        IsInPool = false;
    }

    public void MarkAsActive()
    {
        IsInPool = false;
    }

    public void MarkAsInPool()
    {
        IsInPool = true;
    }

    
public void ReturnToPool()
{
    if (IsInPool)
        return;

    // Notify the wave tracker before the object is disabled.
    OnReturnedToPool?.Invoke(this);

    if (poolManager != null)
    {
        poolManager.ReturnToPool(this);
    }
}

}
