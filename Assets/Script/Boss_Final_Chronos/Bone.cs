using UnityEngine;

public class Bone : MonoBehaviour
{
    public enum MoveMode
    {
        Down = 0,
        Directional = 1
    }

    public enum VisualForward
    {
        Right = 0,
        Up = 1
    }

    [Header("Movement")]
    public MoveMode moveMode = MoveMode.Down;
    public Vector2 moveDirection = Vector2.down; // utilisé si MoveMode.Directional
    public float speed = 4f;
    public int damage = 5;
    [SerializeField] private string poolTag = "needle";

    [Header("Visual (optionnel)")]
    [Tooltip("Transform child qui contient le sprite/visuel. Si null, on oriente le GameObject lui-même.")]
    public Transform visual;
    [Tooltip("Choisir l'axe du visuel correspondant à l'art (ex: si sprite pointe vers le haut choisissez Up).")]
    public VisualForward visualForward = VisualForward.Right;
    [Tooltip("Offset en degrés appliqué après calcul de l'angle (utile si le sprite 'point' art est orienté différemment).")]
    public float visualRotationOffset = 0f;

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

    // Timer de durée maximale (sécurité pour projectiles directionnels)
    public float maxLifetime = 10f;
    private float lifeElapsed;

    void Awake()
    {
        cachedTransform = transform;
    }

    void OnEnable()
    {
        // Reset le flag
        isReturning = false;

        // Reset timer
        lifeElapsed = 0f;

        // Cache les singletons une seule fois
        if (gameManager == null)
            gameManager = ChronosGameManager.Instance;

        if (pooler == null)
            pooler = ObjectPooler.Instance;

        // Normalisation prudente de la direction pour les modes directionnels
        if (moveMode == MoveMode.Directional && moveDirection.sqrMagnitude > 0f)
            moveDirection = moveDirection.normalized;

        // Forcer l'orientation visuelle si on est en mode Directional
        if (moveMode == MoveMode.Directional && moveDirection.sqrMagnitude > 0f)
        {
            Transform t = visual != null ? visual : cachedTransform;
            if (t != null)
            {
                // Calculer l'angle à appliquer (0° correspond à la droite)
                float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
                // Si ton visuel "forward" est l'axe up (Y) on n'a pas besoin d'ajouter 90°, mais on laisse l'offset éditable
                angle += visualRotationOffset;
                t.rotation = Quaternion.Euler(0f, 0f, angle);
            }
        }
    }

    void Update()
    {
        // Évite les calculs si l'objet est déjà en train d'être retourné
        if (isReturning) return;

        // Timer de vie
        if (maxLifetime > 0f)
        {
            lifeElapsed += Time.deltaTime;
            if (lifeElapsed >= maxLifetime)
            {
                ReturnToPoolOrDisable();
                return;
            }
        }

        // Pré-calcule speed * Time.deltaTime une seule fois
        speedDeltaTime = speed * Time.deltaTime;

        // Cache la position actuelle
        cachedPosition = cachedTransform.position;

        // Déplace l'objet selon le mode
        if (moveMode == MoveMode.Directional)
        {
            cachedPosition += (Vector3)(moveDirection * speedDeltaTime);
        }
        else // Down
        {
            cachedPosition.y -= speedDeltaTime;
        }

        cachedTransform.position = cachedPosition;

        // Vérification optimisée de la position Y (seulement pour le mode Down et fallback)
        if (moveMode == MoveMode.Down && cachedPosition.y < DESPAWN_Y)
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