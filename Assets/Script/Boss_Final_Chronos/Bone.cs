using UnityEngine;

public class Bone : MonoBehaviour
{
    public float speed = 4f;
    public int damage = 5;
    [SerializeField] private string poolTag = "needle";

    // Constantes
    private const float DESPAWN_Y = -6f;
    private const string PLAYER_TAG = "PlayerSoul";

    // Cache pour éviter les allocations et les appels répétés
    private static ChronosGameManager gameManager;
    private static ObjectPooler pooler;
    private Transform cachedTransform;
    private Vector3 cachedPosition;
    private float speedDeltaTime;

    // Flag pour éviter les appels multiples
    private bool isReturning;

    void Awake()
    {
        cachedTransform = transform;
    }

    void OnEnable()
    {
        // Reset le flag
        isReturning = false;

        // Cache les singletons une seule fois
        if (gameManager == null)
            gameManager = ChronosGameManager.Instance;

        if (pooler == null)
            pooler = ObjectPooler.Instance;
    }

    void Update()
    {
        // Évite les calculs si l'objet est déjà en train d'être retourné
        if (isReturning) return;

        // Pré-calcule speed * Time.deltaTime une seule fois
        speedDeltaTime = speed * Time.deltaTime;

        // Cache la position actuelle
        cachedPosition = cachedTransform.position;

        // Déplace l'objet vers le bas
        cachedPosition.y -= speedDeltaTime;
        cachedTransform.position = cachedPosition;

        // Vérification optimisée de la position Y
        if (cachedPosition.y < DESPAWN_Y)
        {
            ReturnToPoolOrDisable();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Évite les traitements multiples
        if (isReturning) return;

        // CompareTag est plus performant que GetHashCode
        if (!other.CompareTag(PLAYER_TAG)) return;

        // Applique les dégâts
        gameManager.DamagePlayer(damage);

        // Retourne à la pool
        ReturnToPoolOrDisable();
    }

    // Retourne l'objet à la pool si possible
    private void ReturnToPoolOrDisable()
    {
        // Évite les appels multiples
        if (isReturning) return;
        isReturning = true;

        // Vérification simplifiée
        if (pooler != null &&
            pooler.poolDictionary != null &&
            pooler.poolDictionary.ContainsKey(poolTag))
        {
            pooler.ReturnToPool(poolTag, gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}