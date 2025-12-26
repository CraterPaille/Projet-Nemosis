using UnityEngine;

public class VillageGridController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float edgeScrollMargin = 20f;
    
    [Header("Zoom Settings (via Canvas Scale ou Camera)")]
    [SerializeField] private bool useZoom = true;
    [SerializeField] private float zoomSpeed = 0.1f;
    [SerializeField] private Vector3 minScale = new Vector3(0.5f, 0.5f, 1f);
    [SerializeField] private Vector3 maxScale = new Vector3(2f, 2f, 1f);
    
    [Header("Bounds (limites de déplacement)")]
    [SerializeField] private bool useBounds = true;
    [SerializeField] private Vector2 minBounds = new Vector2(-20, -20);
    [SerializeField] private Vector2 maxBounds = new Vector2(20, 20);

    private Vector3 currentScale = Vector3.one;

    private void Update()
    {
        // Active uniquement si la grille est active ET qu'aucun menu n'est ouvert
        if (!gameObject.activeSelf)
            return;

        // Désactive si le menu d'interaction est ouvert
        if (UIManager.Instance != null && UIManager.Instance.IsInteractionMenuOpen())
            return;

        HandleMovement();
        if (useZoom)
            HandleZoom();
    }

    private void HandleMovement()
    {
        Vector3 moveDirection = Vector3.zero;

        // Déplacement avec ZQSD / WASD (inverse car on bouge le monde)
        if (Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.W))
            moveDirection += Vector3.down;
        if (Input.GetKey(KeyCode.S))
            moveDirection += Vector3.up;
        if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.A))
            moveDirection += Vector3.right;
        if (Input.GetKey(KeyCode.D))
            moveDirection += Vector3.left;

        // Déplacement avec flèches (inverse)
        moveDirection -= new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);

        // Edge scrolling (inverse)
        Vector3 mousePos = Input.mousePosition;
        if (mousePos.x < edgeScrollMargin)
            moveDirection += Vector3.right;
        if (mousePos.x > Screen.width - edgeScrollMargin)
            moveDirection += Vector3.left;
        if (mousePos.y < edgeScrollMargin)
            moveDirection += Vector3.up;
        if (mousePos.y > Screen.height - edgeScrollMargin)
            moveDirection += Vector3.down;

        // Applique le mouvement
        if (moveDirection != Vector3.zero)
        {
            Vector3 newPosition = transform.position + moveDirection.normalized * moveSpeed * Time.deltaTime;

            // Applique les limites
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
        if (scroll != 0f)
        {
            // Augmente/diminue l'échelle
            currentScale += Vector3.one * scroll * zoomSpeed;
            
            // Clamp l'échelle
            currentScale.x = Mathf.Clamp(currentScale.x, minScale.x, maxScale.x);
            currentScale.y = Mathf.Clamp(currentScale.y, minScale.y, maxScale.y);
            currentScale.z = 1f; // Z reste à 1
            
            transform.localScale = currentScale;
        }
    }

    /// <summary>
    /// Recentre la grille sur la position initiale
    /// </summary>
    public void ResetPosition()
    {
        transform.position = Vector3.zero;
        transform.localScale = Vector3.one;
        currentScale = Vector3.one;
    }
}
