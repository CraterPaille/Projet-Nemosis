using UnityEngine;
using UnityEngine.EventSystems;

public class Building2D : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Building Data")]
    [SerializeField] private BuildingData buildingData;
    
    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Collider2D buildingCollider;
    
    [Header("Highlight on Hover (Optional)")]
    [SerializeField] private Color highlightColor = new Color(1f, 1f, 1f, 1.3f);
    private Color originalColor;
    private bool isHighlighted = false;

    private VillageManager manager;

    private void Awake()
    {
        // Récupère le SpriteRenderer si non assigné
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        // Récupère le Collider2D
        if (buildingCollider == null)
        {
            buildingCollider = GetComponent<Collider2D>();
        }
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    private void Update()
    {
        // Détection du hover manuel (fallback si Physics2D Raycaster ne marche pas)
        if (buildingCollider == null) return;

        // Désactive si le menu d'interaction est ouvert
        if (UIManager.Instance != null && UIManager.Instance.IsInteractionMenuOpen())
        {
            // Cache le tooltip et highlight si le menu est ouvert
            if (isHighlighted)
            {
                OnPointerExit(null);
            }
            return;
        }

        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10f;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

        bool isHoveringNow = buildingCollider.bounds.Contains(worldPos);

        if (isHoveringNow && !isHighlighted)
        {
            OnPointerEnter(null);
        }
        else if (!isHoveringNow && isHighlighted)
        {
            OnPointerExit(null);
        }
    }

    /// <summary>
    /// Initialise le bâtiment avec ses données et adapte le sprite à la taille
    /// </summary>
    public void Init(BuildingData data, VillageManager villageManager)
    {
        buildingData = data;
        manager = villageManager;
        
        // Assigne le sprite depuis les données
        if (spriteRenderer != null && data.icon != null)
        {
            spriteRenderer.sprite = data.icon;
            
            // Auto-scale le sprite selon la taille du footprint
            ScaleSpriteToFootprint(data.gridSize, villageManager);
        }
        
        Debug.Log($"[Building2D] Initialized {data.buildingName} at position {transform.position}");
    }

    /// <summary>
    /// Adapte l'échelle du sprite pour qu'il occupe exactement la taille du footprint en monde
    /// </summary>
    private void ScaleSpriteToFootprint(int gridSize, VillageManager villageManager)
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null)
            return;

<<<<<<< Updated upstream
        // Calcule la taille visuelle en monde isométrique
        // Footprint carré (gridSize x gridSize) correspond à :
        float worldWidth = gridSize * villageManager.isoTileWidth;   // Largeur iso
        float worldHeight = gridSize * villageManager.isoTileHeight;  // Hauteur iso

        // Récupère la taille du sprite en unités monde
        Sprite sprite = spriteRenderer.sprite;
        float spriteWidth = sprite.bounds.size.x;
        float spriteHeight = sprite.bounds.size.y;

        if (spriteWidth > 0 && spriteHeight > 0)
        {
            // Calcule l'échelle nécessaire
            float scaleX = worldWidth / spriteWidth;
            float scaleY = worldHeight / spriteHeight;
            
            // Utilise l'échelle moyenne ou le minimum selon votre préférence
            // Ici on utilise le minimum pour bien rentrer dans le carré
            float scale = Mathf.Min(scaleX, scaleY);
            
            transform.localScale = new Vector3(scale, scale, 1f);
            
            Debug.Log($"[Building2D] Scaled {buildingData.buildingName} to {scale:F2}x (footprint {gridSize}x{gridSize})");
        }
=======
        // gridSize = nombre de tuiles de côté (1 = 1x1 tuiles, 2 = 2x2 tuiles, etc.)
        float tilesSize = Mathf.Max(1, gridSize);

        // En isométrique, une zone de NxN tuiles a:
        // - Largeur = N * isoTileWidth (ex: 2 * 100 = 200)
        // - Hauteur = N * isoTileWidth (on utilise la même valeur car le sprite est carré)
        // Le sprite fait 1x1 unité car PPU = dimensions du sprite
        float targetSize = tilesSize * villageManager.isoTileWidth;

        // Applique la même échelle sur les deux côtés pour garder le sprite carré
        transform.localScale = new Vector3(targetSize, targetSize, 1f);
        
        // Mets à jour le collider pour qu'il corresponde au sprite scalé
        UpdateColliderSize();
>>>>>>> Stashed changes
    }

    /// <summary>
    /// Appelé quand la souris entre dans le collider
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (buildingData == null) return;
        
        Debug.Log($"Building {buildingData.buildingName} hovered.");
        
        // Highlight visuel
        if (spriteRenderer != null && !isHighlighted)
        {
            spriteRenderer.color = highlightColor;
            isHighlighted = true;
        }
        
        // Affiche le tooltip
        if (manager != null)
        {
            manager.OnBuildingHovered(this);
        }
    }

    /// <summary>
    /// Appelé quand la souris sort du collider
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        // Retire le highlight
        if (spriteRenderer != null && isHighlighted)
        {
            spriteRenderer.color = originalColor;
            isHighlighted = false;
        }
        
        // Cache le tooltip
        UIManager.Instance.HideTooltip();
    }

    /// <summary>
    /// Appelé quand on clique sur le bâtiment
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (buildingData == null) return;
        
        Debug.Log($"Building {buildingData.buildingName} clicked.");
        
        if (manager != null)
        {
            manager.OnBuildingClicked(this);
        }
    }

    /// <summary>
    /// Détection du clic manuel (fallback)
    /// </summary>
    public bool IsMouseOverBuilding()
    {
        if (buildingCollider == null) return false;

        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10f;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

        return buildingCollider.bounds.Contains(worldPos);
    }

    /// <summary>
    /// Retourne les données du bâtiment
    /// </summary>
    public BuildingData GetData()
    {
        return buildingData;
    }

    // Optionnel : visualisation en mode Scene
    private void OnDrawGizmosSelected()
    {
        if (buildingData != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
        }
    }
}
