
using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance { get; private set; }

    [Serializable]
    private class Pool
    {
        public GameObject prefab;
        public List<GameObject> availableObjects = new List<GameObject>();
    }

    [Header("Pools")]
    [SerializeField] private List<Pool> pools = new List<Pool>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Prewarm(GameObject prefab, int amount)
    {
        if (prefab == null || amount <= 0)
            return;

        Pool pool = GetPool(prefab);

        for (int i = pool.availableObjects.Count; i < amount; i++)
        {
            GameObject obj = CreateObject(prefab);

            StoreObject(pool, obj.GetComponent<PooledObject>());
        }

        Debug.Log($"Prewarmed {amount} objects for {prefab.name}");
    }

    public GameObject Get(
        GameObject prefab,
        Vector3 position,
        Quaternion rotation)
    {
        if (prefab == null)
            return null;

        Pool pool = GetPool(prefab);

        GameObject obj;

        if (pool.availableObjects.Count > 0)
        {
            int lastIndex = pool.availableObjects.Count - 1;

            obj = pool.availableObjects[lastIndex];

            pool.availableObjects.RemoveAt(lastIndex);
        }
        else
        {
            obj = CreateObject(prefab);
        }

        obj.transform.SetPositionAndRotation(position, rotation);

        PooledObject pooledObject = obj.GetComponent<PooledObject>();

        pooledObject.MarkAsActive();

        obj.SetActive(true);

        return obj;
    }

    private GameObject CreateObject(GameObject prefab)
    {
        GameObject obj = Instantiate(prefab, transform);

        PooledObject pooledObject = obj.GetComponent<PooledObject>();

        if (pooledObject == null)
        {
            Debug.LogError(
                $"PooledObject is missing on prefab: {prefab.name}",
                prefab
            );

            Destroy(obj);
            return null;
        }

        pooledObject.Initialize(this, prefab);

        obj.SetActive(false);

        return obj;
    }

    public void ReturnToPool(PooledObject pooledObject)
    {
        if (pooledObject == null || pooledObject.IsInPool)
            return;

        Pool pool = GetPool(pooledObject.Prefab);

        StoreObject(pool, pooledObject);
    }

    private void StoreObject(Pool pool, PooledObject pooledObject)
    {
        if (pooledObject == null || pooledObject.IsInPool)
            return;

        pooledObject.MarkAsInPool();

        pooledObject.transform.SetParent(transform);

        pooledObject.gameObject.SetActive(false);

        pool.availableObjects.Add(pooledObject.gameObject);
    }

    private Pool GetPool(GameObject prefab)
    {
        foreach (Pool pool in pools)
        {
            if (pool.prefab == prefab)
                return pool;
        }

        Pool newPool = new Pool
        {
            prefab = prefab
        };

        pools.Add(newPool);

        return newPool;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
