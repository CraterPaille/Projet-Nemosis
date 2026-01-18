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
    private int leftShieldDirection = 2; // 0=droite, 1=haut, 2=gauche, 3=bas
    private int rightShieldDirection = 0;

    void Start()
    {
        if (playerSprite != null)
            originalColor = playerSprite.color;
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
        // Convertit les directions (0-3) en angles (0°, 90°, 180°, 270°)
        float leftAngle = leftShieldDirection * 90f;
        float rightAngle = rightShieldDirection * 90f;

        // Positionne les boucliers
        leftShield.transform.position = transform.position +
            new Vector3(Mathf.Cos(leftAngle * Mathf.Deg2Rad),
                       Mathf.Sin(leftAngle * Mathf.Deg2Rad), 0) * shieldDistance;
        leftShield.transform.rotation = Quaternion.Euler(0, 0, leftAngle);

        rightShield.transform.position = transform.position +
            new Vector3(Mathf.Cos(rightAngle * Mathf.Deg2Rad),
                       Mathf.Sin(rightAngle * Mathf.Deg2Rad), 0) * shieldDistance;
        rightShield.transform.rotation = Quaternion.Euler(0, 0, rightAngle);
    }

    public void ActivateShields()
    {
        isActive = true;
        leftShield.SetActive(true);
        rightShield.SetActive(true);

        if (playerSprite != null)
            playerSprite.color = justiceColor;

        // Position initiale : gauche à gauche (180°), droit à droite (0°)
        leftShieldDirection = 2;
        rightShieldDirection = 0;
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