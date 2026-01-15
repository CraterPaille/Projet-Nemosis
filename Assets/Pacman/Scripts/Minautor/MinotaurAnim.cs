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
