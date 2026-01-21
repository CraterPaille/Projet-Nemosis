using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    public static ObjectPooler Instance;
    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictionary;

    // Cache pour éviter les lookups répétés
    private Dictionary<string, Pool> poolDefinitions;
    private Transform cachedTransform;

    void Awake()
    {
        Instance = this;
        cachedTransform = transform;
        poolDictionary = new Dictionary<string, Queue<GameObject>>(pools.Count);
        poolDefinitions = new Dictionary<string, Pool>(pools.Count);

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>(pool.size);

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab, cachedTransform);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectPool);
            poolDefinitions.Add(pool.tag, pool);
        }
    }

    // Version optimisée avec TryGetValue
    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation, Transform parent = null, bool activateImmediately = true)
    {
        if (!poolDictionary.TryGetValue(tag, out Queue<GameObject> queue))
        {
            Debug.LogWarning($"ObjectPooler: pool with tag '{tag}' not found.");
            return null;
        }

        GameObject objectToSpawn = null;
        int attempts = queue.Count;

        // Parcours optimisé pour trouver un objet inactif
        for (int i = 0; i < attempts; i++)
        {
            GameObject candidate = queue.Dequeue();

            if (!candidate.activeInHierarchy)
            {
                objectToSpawn = candidate;
                break;
            }

            queue.Enqueue(candidate);
        }

        // Fallback: instancier si aucun objet disponible
        if (objectToSpawn == null)
        {
            if (poolDefinitions.TryGetValue(tag, out Pool poolDef) && poolDef.prefab != null)
            {
                objectToSpawn = Instantiate(poolDef.prefab, cachedTransform);
                objectToSpawn.SetActive(false);
            }
            else
            {
                Debug.LogWarning($"ObjectPooler: no available object and prefab missing for tag '{tag}'.");
                return null;
            }
        }

        // Configure l'objet
        Transform objTransform = objectToSpawn.transform;
        objTransform.SetParent(parent);
        objTransform.position = position;
        objTransform.rotation = rotation;

        if (activateImmediately)
        {
            objectToSpawn.SetActive(true);
        }

        return objectToSpawn;
    }

    // Version optimisée avec TryGetValue
    public bool TryReturnToPool(string tag, GameObject obj)
    {
        if (obj == null) return false;

        if (poolDictionary != null && poolDictionary.TryGetValue(tag, out Queue<GameObject> queue))
        {
            obj.SetActive(false);
            obj.transform.SetParent(cachedTransform);
            queue.Enqueue(obj);
            return true;
        }

        return false;
    }

    // Méthode legacy pour compatibilité
    public void ReturnToPool(string tag, GameObject obj)
    {
        if (!TryReturnToPool(tag, obj))
        {
            obj.SetActive(false);
            if (Instance != null)
                obj.transform.SetParent(cachedTransform);
        }
    }

    // Gestionnaire de retours différés optimisé
    public static class PoolReturnManager
    {
        private static List<(string tag, GameObject obj)> pending = new List<(string, GameObject)>(32);

        public static void AddPendingReturn(string tag, GameObject obj)
        {
            pending.Add((tag, obj));
        }

        public static void ProcessPendingReturns()
        {
            if (Instance == null || pending.Count == 0) return;

            for (int i = pending.Count - 1; i >= 0; i--)
            {
                var (tag, obj) = pending[i];

                if (obj != null && !obj.activeInHierarchy)
                {
                    Instance.TryReturnToPool(tag, obj);
                    pending.RemoveAt(i);
                }
            }
        }
    }

    void LateUpdate()
    {
        PoolReturnManager.ProcessPendingReturns();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}