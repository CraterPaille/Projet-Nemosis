using UnityEngine;
using UnityEngine.Rendering;

public class SpeedVolumeWeight : MonoBehaviour
{
    [Header("Références")]
    public chyronGameManager gameManager; // source de la vitesse
    public Volume volume;                 // volume dont on veut piloter le weight

    [Header("Mapping vitesse -> weight")]
    public float minSpeed = 3f;   // vitesse à laquelle le volume commence à apparaître
    public float maxSpeed = 6f;   // vitesse à laquelle il est à 100%
    public float minWeight = 0f;  // poids à minSpeed
    public float maxWeight = 1f;  // poids à maxSpeed

    [Header("Lissage")]
    public float lerpSpeed = 5f;

    private float currentWeight;

    private void Awake()
    {
        if (volume == null)
            volume = GetComponent<Volume>();

        if (gameManager == null)
            gameManager = FindFirstObjectByType<chyronGameManager>();
    }

    private void Update()
    {
        if (gameManager == null || volume == null)
            return;

        float speed = gameManager.scrollSpeed;

        // Normalise vitesse -> [0,1]
        float t = Mathf.InverseLerp(minSpeed, maxSpeed, speed);

        float targetWeight = Mathf.Lerp(minWeight, maxWeight, t);
        currentWeight = Mathf.Lerp(currentWeight, targetWeight, Time.deltaTime * lerpSpeed);

        volume.weight = currentWeight;
    }
}