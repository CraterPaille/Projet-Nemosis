using UnityEngine;

public class Jewel : MonoBehaviour
{
    // Cache du tag pour éviter les allocations
    private static readonly int PlayerTagHash = "PlayerSoul".GetHashCode();

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Comparaison optimisée avec hashcode
        if (other.tag.GetHashCode() == PlayerTagHash)
        {
            ChronosGameManager.Instance.OnJewelCollected();

            // Désactive et reparent pour le pooling
            gameObject.SetActive(false);

            // Vérification null optimisée
            if (ObjectPooler.Instance != null)
            {
                transform.SetParent(ObjectPooler.Instance.transform);
            }
        }
    }
}