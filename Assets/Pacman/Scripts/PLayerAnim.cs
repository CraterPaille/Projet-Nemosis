using UnityEngine;

public class PLayerAnim : MonoBehaviour
{
    Animator animator;
    PacmanMovement pacmanMovement;
    
    private float lastMoveX;
    private float lastMoveY = -1f; // Direction initiale vers le bas

    void Start()
    {
        animator = GetComponent<Animator>();
        pacmanMovement = GetComponent<PacmanMovement>();
        
        // Initialiser la direction de départ
        animator.SetFloat("MoveX", lastMoveX);
        animator.SetFloat("MoveY", lastMoveY);
    }

    void LateUpdate()
    {
        if (pacmanMovement != null)
        {
            // Récupère la direction actuelle du PacmanMovement
            Vector2 direction = pacmanMovement.GetCurrentDirection();
            
            // Met à jour la direction complète (X et Y ensemble)
            if (direction != Vector2.zero)
            {
                // Normalise pour avoir des valeurs propres (-1, 0, ou 1)
                lastMoveX = Mathf.Round(direction.x);
                lastMoveY = Mathf.Round(direction.y);
            }
            
        }

        // Envoi des paramètres à l'Animator avec la dernière direction
        animator.SetFloat("MoveX", lastMoveX);
        animator.SetFloat("MoveY", lastMoveY);
    }
}
