using UnityEngine;
using System.Collections.Generic;
using System.Collections;
public class MinotaurAI : MonoBehaviour
{
    [Header("Configuration")]
    public float speed = 5f;                // Vitesse du fantôme (légèrement plus lent que Pacman)
    public float gridSize = 2f;             // Taille de la grille (même que Pacman)
    public LayerMask wallLayer;             // Layer des murs
    
    [Header("Comportement")]
    public GhostBehavior currentBehavior = GhostBehavior.Chase;
    public Transform pacmanTransform;       // Référence à Pacman
    
    [Header("Timers")]
    public float frightenedDuration = 10f;  // Durée du mode Frightened
    
    [Header("Debug")]
    public bool showDebugGizmos = true;
    public bool debugCanMove = false;
    
    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private Vector2 currentDirection = Vector2.zero;
    private Vector2 nextDirection = Vector2.zero;  // Direction après le prochain virage
    private Vector2 targetPosition;
    private float behaviorTimer = 0f;
    private bool isChangingDirection = false;  // Indique si on est en train de virer
    
    // Pour détecter le collider size
    private Vector2 colliderSize;
    
    // Directions possibles (haut, bas, gauche, droite)


    private readonly Vector2[] possibleDirections = new Vector2[]
    {
        Vector2.up,
        Vector2.down,
        Vector2.left,
        Vector2.right
    };
    
    public enum GhostBehavior
    {
        Chase,      // Poursuite de Pacman
        Scatter,    // Retour vers le coin assigné
        Frightened  // Fuite aléatoire
    }
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        
        // Stocke la taille du collider
        if (boxCollider != null)
        {
            colliderSize = boxCollider.size;
        }
        else
        {
            colliderSize = new Vector2(0.9f, 0.9f); // Valeur par défaut
        }
        
        // Aligne sur la grille au départ
        Vector2 snapped = SnapToGrid(transform.position);
        rb.position = snapped;
        transform.position = snapped;
        
        // Trouve Pacman automatiquement si non assigné
        if (pacmanTransform == null)
        {
            GameObject pacman = GameObject.FindGameObjectWithTag("Player");
            if (pacman != null)
                pacmanTransform = pacman.transform;
        }
        
        // Direction initiale aléatoire
        currentDirection = possibleDirections[Random.Range(0, possibleDirections.Length)];
    }
    
    void Start()
    {
        // Le Minotaure reste toujours en mode Chase
    }
 





    void FixedUpdate()
    {
        // Gestion des cycles Chase/Scatter
        UpdateBehaviorTimer();
        
        // Si on est à une intersection, choisir une nouvelle direction
        if (IsAtIntersection())
        {
            ChooseNextDirection();
        }
        
        // Alignement progressif lors des virages
        if (isChangingDirection)
        {
            AlignToGrid();
        }
        
        // Déplacement continu
        MoveForward();
    }
    
    void UpdateBehaviorTimer()
    {
        if (currentBehavior == GhostBehavior.Frightened)
        {
            behaviorTimer -= Time.fixedDeltaTime;
            if (behaviorTimer <= 0f)
            {
                // Retour au mode Chase
                currentBehavior = GhostBehavior.Chase;
            }
        }
    }
    
    void ChooseNextDirection()
    {
        Vector2 bestDirection = currentDirection;
        float bestDistance = float.MaxValue;
        
        // Détermine la cible selon le comportement
        switch (currentBehavior)
        {
            case GhostBehavior.Chase:
                targetPosition = pacmanTransform != null ? (Vector2)pacmanTransform.position : rb.position;
                break;
            case GhostBehavior.Frightened:
                // Mode aléatoire
                List<Vector2> validDirections = new List<Vector2>();
                foreach (Vector2 dir in possibleDirections)
                {
                    // Pas de demi-tour et pas de mur
                    if (dir != -currentDirection && CanMove(dir))
                    {
                        validDirections.Add(dir);
                    }
                }
                if (validDirections.Count > 0)
                {
                    currentDirection = validDirections[Random.Range(0, validDirections.Count)];
                }
                return;
        }
        
        // Pathfinding simple : choisir la direction qui rapproche le plus de la cible
        foreach (Vector2 dir in possibleDirections)
        {
            // Règle : pas de demi-tour (sauf si bloqué)
            if (dir == -currentDirection)
                continue;
            
            // Vérifie si on peut aller dans cette direction
            if (!CanMove(dir))
                continue;
            
            // Calcule la distance à la cible depuis cette direction
            Vector2 nextPos = rb.position + dir * gridSize;
            float distance = Vector2.Distance(nextPos, targetPosition);
            
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestDirection = dir;
            }
        }
        
        // Si aucune direction valide trouvée (bloqué), autoriser le demi-tour
        if (bestDirection == currentDirection && !CanMove(currentDirection))
        {
            foreach (Vector2 dir in possibleDirections)
            {
                if (CanMove(dir))
                {
                    bestDirection = dir;
                    break;
                }
            }
        }
        
        // Si la direction change, préparer un virage fluide
        if (bestDirection != currentDirection)
        {
            nextDirection = bestDirection;
            isChangingDirection = true;
        }
        else
        {
            currentDirection = bestDirection;
        }
    }
    
    void AlignToGrid()
    {
        // Aligne progressivement sur la grille lors d'un changement de direction
        Vector2 currentPos = rb.position;
        Vector2 snappedPos = SnapToGrid(currentPos);
        
        // Distance à la position alignée
        float distanceToSnap = Vector2.Distance(currentPos, snappedPos);
        
        // Si on est suffisamment proche du centre de la grille, effectuer le virage
        if (distanceToSnap < 0.15f)
        {
            // Snap précisément et change de direction
            rb.position = snappedPos;
            transform.position = snappedPos;
            currentDirection = nextDirection;
            isChangingDirection = false;
        }
        else
        {
            // Continue dans la direction actuelle en s'alignant vers le centre
            Vector2 alignmentDirection = (snappedPos - currentPos).normalized;
            Vector2 movement = currentDirection * speed * Time.fixedDeltaTime;
            
            // Ajoute un mouvement vers le centre de la grille
            movement += alignmentDirection * speed * 0.5f * Time.fixedDeltaTime;
            
            rb.MovePosition(currentPos + movement);
        }
    }
    
    void MoveForward()
    {
        // Si on est en train de changer de direction, ne pas bouger ici
        if (isChangingDirection)
            return;
        
        // Vérifie AVANT de bouger
        if (!CanMove(currentDirection))
        {
            // Bloqué : arrête le mouvement et recalcule la direction
            Vector2 snappedPos = SnapToGrid(rb.position);
            float distanceToSnap = Vector2.Distance(rb.position, snappedPos);
            
            // Aligne seulement si très proche de la grille
            if (distanceToSnap < 0.2f)
            {
                rb.position = snappedPos;
                transform.position = snappedPos;
            }
            
            isChangingDirection = false;
            ChooseNextDirection();
            return;
        }
        
        // Calcule la nouvelle position
        Vector2 newPosition = rb.position + currentDirection * speed * Time.fixedDeltaTime;
        
        // Vérifie que la nouvelle position ne cause pas de collision
        if (WillCollideAtPosition(newPosition, currentDirection))
        {
            // Stop et aligne seulement si proche de la grille
            Vector2 snappedPos = SnapToGrid(rb.position);
            float distanceToSnap = Vector2.Distance(rb.position, snappedPos);
            
            if (distanceToSnap < 0.2f)
            {
                rb.position = snappedPos;
                transform.position = snappedPos;
            }
            
            isChangingDirection = false;
            ChooseNextDirection();
            return;
        }
        
        // Déplace
        rb.MovePosition(newPosition);
    }
    
    bool IsAtIntersection()
    {
        // Vérifie si on est aligné sur la grille
        Vector2 pos = rb.position;
        float x = Mathf.Abs((pos.x % gridSize + gridSize) % gridSize);
        float y = Mathf.Abs((pos.y % gridSize + gridSize) % gridSize);
        
        float threshold = 0.1f;
        bool aligned = (x < threshold || x > gridSize - threshold) &&
                       (y < threshold || y > gridSize - threshold);
        
        if (!aligned)
            return false;
        
        // Compte combien de directions sont disponibles
        int availableDirections = 0;
        foreach (Vector2 dir in possibleDirections)
        {
            if (dir != -currentDirection && CanMove(dir))
            {
                availableDirections++;
            }
        }
        
        // C'est une intersection si on a plus d'une option (sans compter le demi-tour)
        return availableDirections > 0;
    }
    
    bool CanMove(Vector2 dir)
    {
        if (dir == Vector2.zero)
            return false;
        
        // Distance de détection : au moins une case complète
        float checkDistance = gridSize * 0.55f;
        
        if (boxCollider == null)
        {
            // Fallback : multiple raycasts depuis le centre
            RaycastHit2D hitCenter = Physics2D.Raycast(rb.position, dir, checkDistance, wallLayer);
            if (hitCenter.collider != null)
            {
                if (debugCanMove) Debug.Log($"CanMove {dir}: BLOCKED by wall (raycast)");
                return false;
            }
            return true;
        }
        
        // Méthode 1 : BoxCast
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(wallLayer);
        filter.useTriggers = false;
        
        RaycastHit2D[] hits = new RaycastHit2D[5];
        int count = boxCollider.Cast(dir, filter, hits, checkDistance);
        
        if (count > 0)
        {
            if (debugCanMove) Debug.Log($"CanMove {dir}: BLOCKED - BoxCast detected {count} walls");
            return false;
        }
        
        // Méthode 2 : Vérification supplémentaire avec OverlapBox à la position cible
        Vector2 targetPos = rb.position + dir * checkDistance;
        Collider2D overlap = Physics2D.OverlapBox(targetPos, colliderSize * 0.9f, 0f, wallLayer);
        
        if (overlap != null)
        {
            if (debugCanMove) Debug.Log($"CanMove {dir}: BLOCKED by overlap at target position");
            return false;
        }
        
        if (debugCanMove) Debug.Log($"CanMove {dir}: FREE");
        return true;
    }
    
    bool WillCollideAtPosition(Vector2 position, Vector2 dir)
    {
        // Vérifie si à cette position on va entrer en collision
        Collider2D[] overlaps = new Collider2D[5];
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(wallLayer);
        filter.useTriggers = false;
        
        int count = Physics2D.OverlapBox(position, colliderSize * 0.8f, 0f, filter, overlaps);
        
        if (count > 0 && debugCanMove)
        {
            Debug.Log($"WillCollide at {position}: YES - {count} overlaps detected");
        }
        
        return count > 0;
    }
    
    Vector2 SnapToGrid(Vector2 position)
    {
        position.x = Mathf.Round(position.x / gridSize) * gridSize;
        position.y = Mathf.Round(position.y / gridSize) * gridSize;
        return position;
    }
    
    void SnapNow()
    {
        Vector2 snapped = SnapToGrid(rb.position);
        rb.position = snapped;
        transform.position = snapped;
    }
    

    
    // Méthode publique pour déclencher le mode Frightened (appelée par Power Pellet)
    public void SetFrightened()
    {
        currentBehavior = GhostBehavior.Frightened;
        behaviorTimer = frightenedDuration;
    }
    
    // Détection de collision avec Pacman
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (currentBehavior == GhostBehavior.Frightened)
            {
                // Pacman mange le fantôme : le fantôme retourne en mode Chase
                SnapNow();
                currentBehavior = GhostBehavior.Chase;
                Debug.Log("Minotaure mangé! Retour en mode Chase");
            }
            else
            {
                // Le fantôme attrape Pacman : Game Over
                Debug.Log("Pacman attrapé par le Minotaure!");
            }
        }
    }
    
    void OnDrawGizmos()
    {
        if (!showDebugGizmos)
            return;
    
        // Obtient le Rigidbody2D uniquement si disponible
        Rigidbody2D drawRb = GetComponent<Rigidbody2D>();
        if (drawRb == null)
            return;
    
        // Affiche la direction actuelle
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(drawRb.position, drawRb.position + currentDirection * gridSize);
    
        // Affiche la cible
        if (currentBehavior == GhostBehavior.Chase && pacmanTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(drawRb.position, pacmanTransform.position);
        }
    
        // Affiche les zones de détection
        BoxCollider2D drawCollider = GetComponent<BoxCollider2D>();
        if (drawCollider != null)
        {
            Gizmos.color = Color.green;
            foreach (Vector2 dir in possibleDirections)
            {
                Vector2 checkPos = drawRb.position + dir * (gridSize * 0.55f);
                Gizmos.DrawWireCube(checkPos, colliderSize * 0.9f);
            }
        }
    }
    
    // Méthode publique pour obtenir la direction actuelle
    public Vector2 GetCurrentDirection()
    {
        return currentDirection;
    }
}