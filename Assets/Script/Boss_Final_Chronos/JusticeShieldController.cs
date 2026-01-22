using UnityEngine;

public class JusticeShieldController : MonoBehaviour
{
    [Header("Shield")]
    public GameObject shield;
    public float rotationSpeed = 90f;
    public float shieldDistance = 1.5f;

    [Header("Visual")]
    public SpriteRenderer playerSprite;
    public Color justiceColor = Color.yellow;
    private Color originalColor;

    // SFX spécifique au parry des needles
    [Header("SFX")]
    [Tooltip("Clip joué quand une needle est bloquée par le bouclier")]
    public AudioClip needleBlockedSfx;

    private bool isActive = false;

    // Direction du bouclier unique : 0=droite,1=haut,2=gauche,3=bas
    [SerializeField]
    private int shieldDirection = 2;

    // Sauvegarde des états initiaux (éditeur) pour le positionnement relatif
    private Vector3 initialLocalPos;
    private Quaternion initialLocalRot;
    private float initialAngle;

    void Start()
    {
        if (playerSprite != null)
            originalColor = playerSprite.color;

        if (shield != null)
        {
            initialLocalPos = transform.InverseTransformPoint(shield.transform.position);
            initialLocalRot = Quaternion.Inverse(transform.rotation) * shield.transform.rotation;
            initialAngle = Mathf.Atan2(initialLocalPos.y, initialLocalPos.x) * Mathf.Rad2Deg;
            initialAngle = (initialAngle + 360f) % 360f;
        }

        DeactivateShields();
    }

    void Update()
    {
        if (!isActive) return;

        // Lecture d'entrée simple :
        // Gauche / A -> bouclier à gauche (direction = 2)
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            shieldDirection = 2; // gauche
            Debug.Log($"Shield: direction set LEFT ({shieldDirection})");
        }

        // Droite / D -> bouclier à droite (direction = 0)
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            shieldDirection = 0; // droite
            Debug.Log($"Shield: direction set RIGHT ({shieldDirection})");
        }

        // Haut / W -> bouclier au-dessus (direction = 1)
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            shieldDirection = 1; // haut
            Debug.Log($"Shield: direction set UP ({shieldDirection})");
        }

        // Bas / S -> bouclier en dessous (direction = 3)
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            shieldDirection = 3; // bas
            Debug.Log($"Shield: direction set DOWN ({shieldDirection})");
        }

        UpdateShieldPositions();
    }

    void UpdateShieldPositions()
    {
        if (shield == null) return;

        float targetAngle = shieldDirection * 90f;
        float delta = Mathf.DeltaAngle(initialAngle, targetAngle);

        Vector3 basis = initialLocalPos.sqrMagnitude > 1e-6f ? initialLocalPos.normalized : Vector3.right;
        Vector3 rotatedLocal = Quaternion.Euler(0, 0, delta) * basis * shieldDistance;
        shield.transform.position = transform.TransformPoint(rotatedLocal);

        Quaternion newLocalRot = Quaternion.Euler(0, 0, delta) * initialLocalRot;
        shield.transform.rotation = transform.rotation * newLocalRot;
    }

    public void ActivateShields()
    {
        isActive = true;
        if (shield != null)
            shield.SetActive(true);

        if (playerSprite != null)
            playerSprite.color = justiceColor;

        UpdateShieldPositions();

        Debug.Log("Justice Shield: ACTIVATED (single shield mode)");
    }

    public void DeactivateShields()
    {
        isActive = false;
        if (shield != null)
            shield.SetActive(false);

        if (playerSprite != null)
            playerSprite.color = originalColor;

        Debug.Log("Justice Shield: DEACTIVATED");
    }

    /// <summary>
    /// Retourne la direction actuelle du bouclier (0-3)
    /// </summary>
    public int GetShieldDirection()
    {
        return shieldDirection;
    }

    public bool IsActive()
    {
        return isActive;
    }

    void OnDrawGizmosSelected()
    {
        if (shield == null) return;

        Vector3 DirFromIndex(int idx) =>
            idx == 0 ? Vector3.right :
            idx == 1 ? Vector3.up :
            idx == 2 ? Vector3.left :
            idx == 3 ? Vector3.down : Vector3.right;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(shield.transform.position, 0.05f);
        Vector3 normal = DirFromIndex(shieldDirection);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(shield.transform.position, shield.transform.position + normal * 0.8f);
        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, shield.transform.position);
    }
}