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
    private BoxCollider2D boxCollider2D;

    private void Awake()
    {
        // Récupère le SpriteRenderer si non assigné
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        // IMPORTANT: Nettoie les anciens colliders qui pourraient traîner
        Collider2D[] oldColliders = GetComponents<Collider2D>();
        foreach (var col in oldColliders)
        {
            // Garde seulement le BoxCollider2D, supprime les autres
            if (!(col is BoxCollider2D))
            {
                Destroy(col);
            }
        }
        
        // Crée ou récupère le BoxCollider2D (si n'existe pas, on le crée)
        boxCollider2D = GetComponent<BoxCollider2D>();
        if (boxCollider2D == null)
        {
            boxCollider2D = gameObject.AddComponent<BoxCollider2D>();
        }
        buildingCollider = boxCollider2D;
        
        // Configure le collider pour la détection sans gravité
        boxCollider2D.isTrigger = true;
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    private void Update()
    {
        // Détection du hover basée sur le sprite plutôt que sur le collider
        if (spriteRenderer == null || manager == null) 
        {
            if (manager == null && spriteRenderer != null && buildingData != null)
            {
                Debug.LogWarning($"[Building2D] Manager is null for {buildingData.buildingName}");
            }
            return;
        }

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

        // Détecte si la souris est sur le sprite
        bool isHoveringNow = IsMouseOnSprite();

        if (isHoveringNow && !isHighlighted)
        {
            OnPointerEnter(null);
        }
        else if (!isHoveringNow && isHighlighted)
        {
            OnPointerExit(null);
        }

        // Détecte le clic basé sur le sprite
        if (isHoveringNow && Input.GetMouseButtonDown(0))
        {
            OnPointerClick(null);
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
    }

    /// <summary>
    /// Adapte l'échelle du sprite pour qu'il occupe exactement la taille du footprint en monde
    /// </summary>
    private void ScaleSpriteToFootprint(int gridSize, VillageManager villageManager)
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null)
            return;

        // gridSize = nombre de tuiles (1 = 1 tuile, 2 = 2 tuiles, etc.)
        float tilesSize = Mathf.Max(1, gridSize);

        // Calcule la taille visuelle en monde isométrique
        float worldWidth = tilesSize * villageManager.isoTileWidth;   // Largeur iso
        float worldHeight = tilesSize * villageManager.isoTileHeight;  // Hauteur iso

        // Récupère la taille du sprite en unités monde
        Sprite sprite = spriteRenderer.sprite;
        float spriteWidth = sprite.bounds.size.x;
        float spriteHeight = sprite.bounds.size.y;

        if (spriteWidth > 0 && spriteHeight > 0)
        {
            // Calcule l'échelle nécessaire
            float scaleX = worldWidth / spriteWidth;
            float scaleY = worldHeight / spriteHeight;
            
            // Utilise le minimum pour rester dans le footprint puis multiplie par 10
            float scale = Mathf.Min(scaleX, scaleY);
            
            transform.localScale = new Vector3(scale, scale, 1f);
            
            // Mets à jour le collider pour qu'il corresponde au sprite scalé
            UpdateColliderSize();
        }
    }

    /// <summary>
    /// Met à jour la taille du BoxCollider2D pour correspondre au sprite
    /// </summary>
    private void UpdateColliderSize()
    {
        if (boxCollider2D == null || spriteRenderer == null || spriteRenderer.sprite == null)
            return;

        // Récupère les bounds du sprite en prenant en compte le scale
        Bounds spriteBounds = spriteRenderer.bounds;
        
        // Convertit à l'espace local du collider
        Vector3 spriteSize = spriteRenderer.sprite.bounds.size;
        boxCollider2D.size = new Vector2(spriteSize.x, spriteSize.y);
        
        // Centre le collider sur le sprite
        boxCollider2D.offset = spriteRenderer.sprite.bounds.center;
    }

    /// <summary>
    /// Appelé quand la souris entre dans le bâtiment
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (buildingData == null) return;
        
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
    /// Appelé quand la souris sort du bâtiment
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        // Retire le highlight
        if (spriteRenderer != null && isHighlighted)
        {
            spriteRenderer.color = originalColor;
            isHighlighted = false;
        }
        
        // Cache le tooltip SEULEMENT si c'est celui de ce bâtiment
        // (pour éviter que d'autres bâtiments cachent le tooltip du bâtiment actuel)
        if (UIManager.Instance != null && UIManager.Instance.GetCurrentTooltipData() == buildingData)
        {
            UIManager.Instance.HideTooltip();
        }
    }

    /// <summary>
    /// Appelé quand on clique sur le bâtiment
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (buildingData == null) return;
        
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

    /// <summary>
    /// Détecte si la souris est sur le sprite en tenant compte du scale et du pivot
    /// </summary>
    private bool IsMouseOnSprite()
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null || Camera.main == null)
            return false;

        Vector3 mousePos = Input.mousePosition;
        
        // Convertit la position écran en position monde
        // Important: utiliser la profondeur Z du bâtiment, pas la caméra
        Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(
            new Vector3(mousePos.x, mousePos.y, Mathf.Abs(Camera.main.transform.position.z - transform.position.z))
        );
        
        // Première vérification: les bounds du sprite (plus fiable)
        if (spriteRenderer.bounds.Contains(worldMousePos))
        {
            // Double vérification avec OverlapPoint pour être certain
            Collider2D overlap = Physics2D.OverlapPoint(worldMousePos);
            if (overlap != null && overlap.gameObject == gameObject)
            {
                return true;
            }
            // Si le collider n'existe pas ou problème, on retourne quand même true pour fallback
            return true;
        }
        
        return false;
    }

        
}
    

