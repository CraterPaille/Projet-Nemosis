
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

        // Parcours sécurisé de la queue pour trouver le premier objet INACTIF
        int attempts = queue.Count;
        for (int i = 0; i < attempts; i++)
        {
            var candidate = queue.Dequeue();
            // Si l'objet est inactif, on le récupère (on ne le remet pas en queue maintenant)
            if (!candidate.activeInHierarchy)
            {
                objectToSpawn = candidate;
                break;
            }
            // Sinon on le remet en queue pour préserver l'ordre
            queue.Enqueue(candidate);
        }

        // Si aucun objet inactif trouvé, instancie un nouveau (fallback)
        if (objectToSpawn == null)
        {
            Pool poolDef = pools.Find(p => p.tag == tag);
            if (poolDef != null && poolDef.prefab != null)
            {
                objectToSpawn = Instantiate(poolDef.prefab);
                objectToSpawn.SetActive(false);
                // Ne pas enqueue ici : il sera retourné via ReturnToPool
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
        if (obj == null) return;

        if (poolDictionary != null && poolDictionary.ContainsKey(tag))
        {
            obj.SetActive(false);
            obj.transform.SetParent(transform);
            poolDictionary[tag].Enqueue(obj);
        }
        else
        {
            // fallback: désactive et parent au pool root si disponible
            obj.SetActive(false);
            if (Instance != null)
                obj.transform.SetParent(Instance.transform);
        }
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