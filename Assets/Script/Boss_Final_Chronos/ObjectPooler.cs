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

    void Awake()
    {
        Instance = this;
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();
            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab, transform);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }
            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    // VERSION AVEC PARAMÈTRE activateImmediately
    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation, Transform parent = null, bool activateImmediately = true)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"ObjectPooler: pool with tag '{tag}' not found.");
            return null;
        }

        Queue<GameObject> queue = poolDictionary[tag];
        GameObject objectToSpawn = null;

        // Cherche un objet inactif dans la queue
        if (queue.Count > 0)
        {
            objectToSpawn = queue.Dequeue();
            if (objectToSpawn.activeInHierarchy)
            {
                // Si actif, remets en queue et cherche un autre
                queue.Enqueue(objectToSpawn);
                objectToSpawn = null;
            }
        }

        // Si aucun inactif trouvé, instancie un nouveau (éviter si possible)
        if (objectToSpawn == null)
        {
            Pool poolDef = pools.Find(p => p.tag == tag);
            if (poolDef != null && poolDef.prefab != null)
            {
                objectToSpawn = Instantiate(poolDef.prefab);
                objectToSpawn.SetActive(false);
                // Ne pas enqueue ici, car il sera retourné via ReturnToPool
            }
            else
            {
                Debug.LogWarning($"ObjectPooler: no available object and prefab missing for tag '{tag}'.");
                return null;
            }
        }

        // Configure position/rotation/parent
        objectToSpawn.transform.SetParent(parent);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        // Active seulement si demandé
        if (activateImmediately)
        {
            objectToSpawn.SetActive(true);
        }

        return objectToSpawn;
    }

    // Méthode pour retourner un objet à la pool
    public void ReturnToPool(string tag, GameObject obj)
    {
      

        obj.SetActive(false);
        obj.transform.SetParent(transform);
        poolDictionary[tag].Enqueue(obj);
    }

    public static class PoolReturnManager
    {
        private static List<(string tag, GameObject obj)> pending = new List<(string, GameObject)>();

        public static void AddPendingReturn(string tag, GameObject obj)
        {
            pending.Add((tag, obj));
        }

        public static void ProcessPendingReturns()
        {
            for (int i = pending.Count - 1; i >= 0; i--)
            {
                var (tag, obj) = pending[i];
                if (obj != null && !obj.activeInHierarchy)
                {
                    ObjectPooler.Instance.ReturnToPool(tag, obj);
                    pending.RemoveAt(i);
                }
            }
        }
    }

    void LateUpdate()
    {
        PoolReturnManager.ProcessPendingReturns();
    }
}