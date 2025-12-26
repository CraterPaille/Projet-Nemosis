using UnityEngine;

public class VillageCameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float edgeScrollMargin = 20f; // Distance du bord pour scroll
    
    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float minZoom = 3f;
    [SerializeField] private float maxZoom = 15f;
    
    [Header("Bounds (optionnel)")]
    [SerializeField] private bool useBounds = false;
    [SerializeField] private Vector2 minBounds = new Vector2(-10, -10);
    [SerializeField] private Vector2 maxBounds = new Vector2(10, 10);

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Update()
    {
        // Active uniquement si on est en mode village 2D
        if (VillageManager.Instance == null)
            return;

        // Vérifie qu'on est bien en mode 2D (la grille est active)
        if (VillageManager.Instance.villageGrid == null || !VillageManager.Instance.villageGrid.activeSelf)
            return;

        HandleMovement();
        HandleZoom();
    }

    private void HandleMovement()
    {
        Vector3 moveDirection = Vector3.zero;

        // Déplacement avec ZQSD / WASD
        if (Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.W))
            moveDirection += Vector3.up;
        if (Input.GetKey(KeyCode.S))
            moveDirection += Vector3.down;
        if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.A))
            moveDirection += Vector3.left;
        if (Input.GetKey(KeyCode.D))
            moveDirection += Vector3.right;

        // Déplacement avec flèches
        moveDirection += new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);

        // Edge scrolling (bord de l'écran)
        Vector3 mousePos = Input.mousePosition;
        if (mousePos.x < edgeScrollMargin)
            moveDirection += Vector3.left;
        if (mousePos.x > Screen.width - edgeScrollMargin)
            moveDirection += Vector3.right;
        if (mousePos.y < edgeScrollMargin)
            moveDirection += Vector3.down;
        if (mousePos.y > Screen.height - edgeScrollMargin)
            moveDirection += Vector3.up;

        // Applique le mouvement
        if (moveDirection != Vector3.zero)
        {
            Vector3 newPosition = transform.position + moveDirection.normalized * moveSpeed * Time.deltaTime;

            // Applique les limites si activées
            if (useBounds)
            {
                newPosition.x = Mathf.Clamp(newPosition.x, minBounds.x, maxBounds.x);
                newPosition.y = Mathf.Clamp(newPosition.y, minBounds.y, maxBounds.y);
            }

            transform.position = newPosition;
        }
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f && cam != null)
        {
            cam.orthographicSize -= scroll * zoomSpeed;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
        }
    }

    /// <summary>
    /// Recentre la caméra sur une position
    /// </summary>
    public void FocusOn(Vector3 position)
    {
        transform.position = new Vector3(position.x, position.y, transform.position.z);
    }
}
