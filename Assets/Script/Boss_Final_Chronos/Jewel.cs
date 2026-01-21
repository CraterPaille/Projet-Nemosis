using UnityEngine;

public class Jewel : MonoBehaviour
{
    [SerializeField] private string poolTag = "Jewel"; // Assure-toi que ce tag existe dans ObjectPooler.pools

    // Cache du tag pour éviter les allocations
    private static readonly int PlayerTagHash = "PlayerSoul".GetHashCode();

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Comparaison optimisée avec hashcode
        if (other.tag.GetHashCode() == PlayerTagHash)
        {
            ChronosGameManager.Instance.OnJewelCollected();

            // Désactive
            gameObject.SetActive(false);

            // Si la pool existe, retourne l'objet dedans, sinon fallback : reparenter
            if (ObjectPooler.Instance != null)
            {
                if (ObjectPooler.Instance.poolDictionary != null && ObjectPooler.Instance.poolDictionary.ContainsKey(poolTag))
                {
                    ObjectPooler.Instance.ReturnToPool(poolTag, gameObject);
                }
                else
                {
                    transform.SetParent(ObjectPooler.Instance.transform);
                }
            }
        }
    }
}