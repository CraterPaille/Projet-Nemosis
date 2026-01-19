using UnityEngine;

public class JusticeShield : MonoBehaviour
{
    public int blockedCount = 0;

    // Cache du tag pour éviter les allocations
    private static readonly int LaserTagHash = "Laser".GetHashCode();

    void OnEnable()
    {
        blockedCount = 0; // Réinitialise lors de l'activation
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Comparaison optimisée avec hashcode
        if (other.tag.GetHashCode() == LaserTagHash)
        {
            blockedCount++;
            Destroy(other.gameObject);
            // TODO: Ajouter effet visuel/sonore de blocage
        }
    }

    // Note: OnCollisionEnter2D n'est pas nécessaire si vous utilisez des triggers
    // Supprimé pour éviter les vérifications redondantes
}