using UnityEngine;

public class MinotaurAnim : MonoBehaviour
{
    Animator animator;
    MinotaurAI minotaurAI;
    
    private float lastMoveX;
    private float lastMoveY = -1f; // Direction initiale vers le bas

    void Start()
    {
        animator = GetComponent<Animator>();
        minotaurAI = GetComponent<MinotaurAI>();
        
        // Initialiser la direction de départ
        animator.SetFloat("MoveX", lastMoveX);
        animator.SetFloat("MoveY", lastMoveY);
    }

    void LateUpdate()
    {
        if (minotaurAI != null)
        {
            // Récupère la direction actuelle du MinotaurAI
            Vector2 direction = minotaurAI.GetCurrentDirection();
            
            // Met à jour immédiatement la direction si elle est non-nulle
            if (direction != Vector2.zero)
            {
                lastMoveX = direction.x;
                lastMoveY = direction.y;
            }
        }

        // Envoi des paramètres à l'Animator avec la dernière direction
        animator.SetFloat("MoveX", lastMoveX);
        animator.SetFloat("MoveY", lastMoveY);
    }
}
