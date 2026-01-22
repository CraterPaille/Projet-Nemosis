using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public Rigidbody2D rb;
    public float moveSpeed = 5f;

    [Header("Fall / Death")]
    [Tooltip("Distance sous la caméra au-delà de laquelle le joueur meurt")]
    [SerializeField] private float fallDeathDistance = 6f;
    [Tooltip("Référence optionnelle au ScoreManager (si non assignée, est recherchée au Start)")]
    [SerializeField] private ScoreManager scoreManager;

    private bool hasFallen = false;

    void Start()
    {
        if (scoreManager == null)
        {
            scoreManager = FindFirstObjectByType<ScoreManager>();
        }
    }

    void Update()
    {
        // --- 1. Déplacements (clavier / gauche joystick) + support joystick droit ---
        float leftInput = 0f;
        try
        {
            leftInput = Input.GetAxis("Horizontal");
        }
        catch (System.Exception)
        {
            leftInput = 0f;
        }

        // Lire le stick droit via le nouveau Input System si disponible,
        // sinon tenter l'ancien Input.GetAxis en le protégeant d'une exception.
        float rightInput = 0f;
        if (Gamepad.current != null)
        {
            rightInput = Gamepad.current.rightStick.ReadValue().x;
        }
        else
        {
            try
            {
                rightInput = Input.GetAxis("RightStickHorizontal");
            }
            catch (System.ArgumentException)
            {
                rightInput = 0f;
            }
        }

        // si le stick droit est plus utilisé, on le priorise (permet d'utiliser l'un ou l'autre)
        float moveInput = Mathf.Abs(rightInput) > Mathf.Abs(leftInput) ? rightInput : leftInput;

        // fallback : addition douce si tu veux que les deux se combinent (décommenter si désiré)
        // moveInput = Mathf.Clamp(leftInput + rightInput, -1f, 1f);

        if (rb != null)
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        // --- 2. Traversée de l'écran (Pac-Man) ---
        if (transform.position.x > 6f)
            transform.position = new Vector3(-6f, transform.position.y, 0);
        else if (transform.position.x < -6f)
            transform.position = new Vector3(6f, transform.position.y, 0);

        // --- 3. Détection de la "chute" -> DEATH ---
        if (Camera.main == null) return;
        float cameraY = Camera.main.transform.position.y;

        // Si le joueur tombe plus bas que cameraY - fallDeathDistance => EndGame
        if (!hasFallen && transform.position.y < cameraY - fallDeathDistance)
        {
            hasFallen = true;
            Debug.Log("[PlayerController] Player fell below camera -> triggering EndGame.");

            if (scoreManager != null)
            {
                // Utilise ScoreManager si présent (méthode publique EndGame)
                scoreManager.EndGame();
            }
            else
            {
                // Fallback : charger la scène principale si aucun ScoreManager trouvé
                SceneManager.LoadScene("SampleScene");
            }
        }
    }
}