using UnityEngine;

public class StarCollectible : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        // On vérifie si c'est le joueur
        if (other.CompareTag("Player"))
        {
            // On cherche le ScoreManager pour ajouter le point
            ScoreManager manager = Object.FindFirstObjectByType<ScoreManager>();
            if (manager != null)
            {
                manager.AddStar();
            }

            // L'étoile disparaît
            Destroy(gameObject);
        }
    }
}