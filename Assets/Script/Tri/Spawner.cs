using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour
{
    public GameObject[] soulPrefabs;
    public float spawnInterval = 1.5f;
    public Transform[] spawnPoints;

    private bool spawning = false;
    private Coroutine spawnCoroutine;

    public void StartSpawning()
    {
        if (spawning) return; // Empêche le double lancement
        spawning = true;
        spawnCoroutine = StartCoroutine(SpawnLoop());
    }

    public void StopSpawning()
    {
        spawning = false;
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    IEnumerator SpawnLoop()
    {
        while (spawning)
        {
            // Choix aléatoire du spawn point
            Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
            GameObject prefab = soulPrefabs[Random.Range(0, soulPrefabs.Length)];

            GameObject soulObj = Instantiate(prefab, point.position, Quaternion.identity);
            Soul soulScript = soulObj.GetComponent<Soul>();

            // Définir le type de mouvement
            if (Mathf.Abs(point.position.x) > 7f) // spawn à gauche ou à droite
            {
                soulScript.movementType = Soul.MovementType.Horizontal;

                // Définir direction horizontale
                soulScript.fallSpeed = Random.Range(2f, 4f) * (point.position.x > 0 ? -1f : 1f);
            }
            else // haut
            {
                soulScript.movementType = Soul.MovementType.Vertical;
                soulScript.fallSpeed = Random.Range(0.5f, 3f);
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }
}
