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

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"ObjectPooler: pool with tag '{tag}' not found.");
            return null;
        }

        Queue<GameObject> queue = poolDictionary[tag];
        GameObject objectToSpawn = null;

        // Parcours la queue en la faisant tourner : on ré-enqueue tous les éléments valides.
        int initialCount = queue.Count;
        for (int i = 0; i < initialCount; i++)
        {
            GameObject candidate = queue.Dequeue();
            if (candidate == null)
            {
                // candidate a été détruit : ne pas le ré-enqueue (on le supprime du pool)
                continue;
            }

            // Si l'objet est inactif, on peut le réutiliser.
            if (!candidate.activeInHierarchy)
            {
                objectToSpawn = candidate;
                // On remet le candidat à la fin pour garder la rotation du pool
                queue.Enqueue(candidate);
                break;
            }

            // L'objet est encore utilisé : on le remet en fin de queue et on continue la recherche.
            queue.Enqueue(candidate);
        }

        // Si aucun objet inactif trouvé, instancier un nouveau (si possible) et l'ajouter au pool.
        if (objectToSpawn == null)
        {
            Pool poolDef = pools.Find(p => p.tag == tag);
            if (poolDef != null && poolDef.prefab != null)
            {
                objectToSpawn = Instantiate(poolDef.prefab);
                objectToSpawn.SetActive(false);
                queue.Enqueue(objectToSpawn);
            }
            else
            {
                Debug.LogWarning($"ObjectPooler: no available object and prefab missing for tag '{tag}'.");
                return null;
            }
        }

        // Prépare et active
        objectToSpawn.transform.SetParent(parent);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;
        objectToSpawn.SetActive(true);

        return objectToSpawn;
    }
}
