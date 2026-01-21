using UnityEngine;

public class JusticeShieldController : MonoBehaviour
{
    [Header("Shields")]
    public GameObject leftShield;
    public GameObject rightShield;
    public float rotationSpeed = 90f; // 90° pour snap aux 4 directions
    public float shieldDistance = 1.5f;

    [Header("Visual")]
    public SpriteRenderer playerSprite;
    public Color justiceColor = Color.yellow;
    private Color originalColor;

    private bool isActive = false;

    // Angles fixes pour les 4 directions : 0°=droite, 90°=haut, 180°=gauche, 270°=bas
    [SerializeField]
    private int leftShieldDirection = 2; // 0=droite, 1=haut, 2=gauche, 3=bas
    [SerializeField]
    private int rightShieldDirection = 0;

    // Sauvegarde des états initiaux (éditeur)
    private Vector3 leftInitialLocalPos;
    private Vector3 rightInitialLocalPos;
    private Quaternion leftInitialLocalRot;
    private Quaternion rightInitialLocalRot;
    private float leftInitialAngle;
    private float rightInitialAngle;

    void Start()
    {
        if (playerSprite != null)
            originalColor = playerSprite.color;

        // Mémoriser position/rotation locales initiales par rapport au joueur
        if (leftShield != null)
        {
            leftInitialLocalPos = transform.InverseTransformPoint(leftShield.transform.position);
            leftInitialLocalRot = Quaternion.Inverse(transform.rotation) * leftShield.transform.rotation;
            leftInitialAngle = Mathf.Atan2(leftInitialLocalPos.y, leftInitialLocalPos.x) * Mathf.Rad2Deg;
            leftInitialAngle = (leftInitialAngle + 360f) % 360f;
        }

        if (rightShield != null)
        {
            rightInitialLocalPos = transform.InverseTransformPoint(rightShield.transform.position);
            rightInitialLocalRot = Quaternion.Inverse(transform.rotation) * rightShield.transform.rotation;
            rightInitialAngle = Mathf.Atan2(rightInitialLocalPos.y, rightInitialLocalPos.x) * Mathf.Rad2Deg;
            rightInitialAngle = (rightInitialAngle + 360f) % 360f;
        }

        DeactivateShields();
    }

    void Update()
    {
        if (!isActive) return;

        // BOUCLIER GAUCHE : Q/D pour rotation par 90°
        if (Input.GetKeyDown(KeyCode.Q))
        {
            leftShieldDirection = (leftShieldDirection + 1) % 4; // Anti-horaire
            Debug.Log($"Left shield: direction {leftShieldDirection}");
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            leftShieldDirection = (leftShieldDirection + 3) % 4; // Horaire
            Debug.Log($"Left shield: direction {leftShieldDirection}");
        }

        // BOUCLIER DROIT : Flèches pour rotation par 90°
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            rightShieldDirection = (rightShieldDirection + 1) % 4; // Anti-horaire
            Debug.Log($"Right shield: direction {rightShieldDirection}");
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            rightShieldDirection = (rightShieldDirection + 3) % 4; // Horaire
            Debug.Log($"Right shield: direction {rightShieldDirection}");
        }

        UpdateShieldPositions();
    }

    void UpdateShieldPositions()
    {
        // Target angles (global) déterminés par l'index
        float targetLeftAngle = leftShieldDirection * 90f;
        float targetRightAngle = rightShieldDirection * 90f;

        // --- Gauche ---
        if (leftShield != null)
        {
            // delta = rotation à appliquer par rapport à la position/rotation initiale
            float deltaLeft = Mathf.DeltaAngle(leftInitialAngle, targetLeftAngle);
            // nouvelle position locale : on utilise la direction initiale normalisée et la distance souhaitée
            Vector3 basisLeft = leftInitialLocalPos.sqrMagnitude > 1e-6f ? leftInitialLocalPos.normalized : Vector3.right;
            Vector3 rotatedLocalLeft = Quaternion.Euler(0, 0, deltaLeft) * basisLeft * shieldDistance;
            leftShield.transform.position = transform.TransformPoint(rotatedLocalLeft);

            // nouvelle rotation : on applique la même rotation relative sur la rotation locale initiale
            Quaternion newLocalRotLeft = Quaternion.Euler(0, 0, deltaLeft) * leftInitialLocalRot;
            leftShield.transform.rotation = transform.rotation * newLocalRotLeft;
        }

        // --- Droite ---
        if (rightShield != null)
        {
            float deltaRight = Mathf.DeltaAngle(rightInitialAngle, targetRightAngle);
            Vector3 basisRight = rightInitialLocalPos.sqrMagnitude > 1e-6f ? rightInitialLocalPos.normalized : Vector3.right;
            Vector3 rotatedLocalRight = Quaternion.Euler(0, 0, deltaRight) * basisRight * shieldDistance;
            rightShield.transform.position = transform.TransformPoint(rotatedLocalRight);

            Quaternion newLocalRotRight = Quaternion.Euler(0, 0, deltaRight) * rightInitialLocalRot;
            rightShield.transform.rotation = transform.rotation * newLocalRotRight;
        }
    }

    public void ActivateShields()
    {
        isActive = true;
        leftShield.SetActive(true);
        rightShield.SetActive(true);

        if (playerSprite != null)
            playerSprite.color = justiceColor;

        UpdateShieldPositions();

        Debug.Log("Justice Shields: ACTIVATED (4 directions mode)");
    }

    public void DeactivateShields()
    {
        isActive = false;
        leftShield.SetActive(false);
        rightShield.SetActive(false);

        if (playerSprite != null)
            playerSprite.color = originalColor;

        Debug.Log("Justice Shields: DEACTIVATED");
    }
}