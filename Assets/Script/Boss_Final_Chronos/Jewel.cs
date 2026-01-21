using UnityEngine;

public class Jewel : MonoBehaviour
{
    [SerializeField] private string poolTag = "Jewel";

    // Cache des composants et références
    private static ChronosGameManager gameManager;
    private static ObjectPooler pooler;
    private Transform cachedTransform;

    // Optimisation: CompareTag est plus rapide que GetHashCode
    private const string PLAYER_TAG = "PlayerSoul";

    private void Awake()
    {
        // Cache le transform une seule fois
        cachedTransform = transform;
    }

    private void OnEnable()
    {
        // Cache les singletons au premier enable si nécessaire
        if (gameManager == null)
            gameManager = ChronosGameManager.Instance;

        if (pooler == null)
            pooler = ObjectPooler.Instance;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // CompareTag est plus performant que GetHashCode
        if (!other.CompareTag(PLAYER_TAG)) return;

        // Appel au GameManager
        gameManager.OnJewelCollected();

        // Désactive immédiatement
        gameObject.SetActive(false);

        // Retourne à la pool de manière optimisée
        if (pooler != null && pooler.poolDictionary.ContainsKey(poolTag))
        {
            // Le SetActive(false) est déjà fait, ReturnToPool le gérera
            pooler.ReturnToPool(poolTag, gameObject);
        }
        else
        {
            // Fallback: reparenter en utilisant le transform caché
            cachedTransform.SetParent(pooler.transform);
        }
    }
}