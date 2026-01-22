using UnityEngine;

public class LevelGen : MonoBehaviour
{
    [Header("Les Prefabs")]
    public GameObject platformPrefab;      // Blanc
    public GameObject trapPlatformPrefab;  // Violet

    [Header("Configuration")]
    public float levelWidth = 5f;
    public float finishLineY = 200f;

    [Header("Difficulté")]
    public float trapStartHeight = 100f;
    [Range(0f, 1f)]
    public float trapChance = 0.2f;

    public float hardModeHeight = 150f;

    // Écart FACILE (Petit saut)
    public float minGapEasy = 1.3f; // J'ai réduit un peu pour éviter le collage
    public float maxGapEasy = 2.4f;

    // Écart DIFFICILE (Grand saut)
    public float minGapHard = 2.8f;
    public float maxGapHard = 4.0f;

    void Start()
    {
        Vector3 spawnPosition = new Vector3();

        // 1. CORRECTION "NUAGES COLLÉS" :
        // On met la plateforme de départ plus bas (-2 au lieu de -1)
        spawnPosition.y = -2f;
        Instantiate(platformPrefab, spawnPosition, Quaternion.identity);

        // Cette variable retient si le nuage d'avant était un piège
        bool previousWasTrap = false;

        // Boucle de génération
        while (spawnPosition.y < finishLineY)
        {
            // --- ÉTAPE 1 : ON DÉCIDE D'ABORD SI C'EST UN PIÈGE ---
            bool isTrap = false;

            // On ne met des pièges que si on est assez haut
            if (spawnPosition.y > trapStartHeight && Random.value < trapChance)
            {
                isTrap = true;
            }

            // --- ÉTAPE 2 : CALCUL INTELLIGENT DE L'ÉCART ---
            float gap;

            // RÈGLE D'OR : Si c'est un piège OU que le précédent était un piège...
            // ... on force un PETIT saut. Sinon c'est impossible à franchir.
            if (isTrap || previousWasTrap)
            {
                gap = Random.Range(minGapEasy, maxGapEasy);
            }
            // Sinon, si on est en mode difficile (>150m), on met des grands sauts
            else if (spawnPosition.y > hardModeHeight)
            {
                gap = Random.Range(minGapHard, maxGapHard);
            }
            // Sinon, mode normal
            else
            {
                gap = Random.Range(minGapEasy, maxGapEasy);
            }

            // On applique l'écart
            spawnPosition.y += gap;
            spawnPosition.x = Random.Range(-levelWidth, levelWidth);

            // --- ÉTAPE 3 : CRÉATION ---
            if (spawnPosition.y < finishLineY)
            {
                if (isTrap)
                {
                    Instantiate(trapPlatformPrefab, spawnPosition, Quaternion.identity);
                }
                else
                {
                    Instantiate(platformPrefab, spawnPosition, Quaternion.identity);
                }
            }

            // On mémorise si c'était un piège pour le prochain tour de boucle
            previousWasTrap = isTrap;
        }

        // Le nuage final (Victoire)
        Vector3 finalPos = new Vector3(0, finishLineY, 0);
        Instantiate(platformPrefab, finalPos, Quaternion.identity);
    }
}