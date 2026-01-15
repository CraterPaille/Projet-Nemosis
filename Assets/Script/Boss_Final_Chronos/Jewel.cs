using UnityEngine;

public class Jewel : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            ChronosGameManager.Instance.OnJewelCollected();

            // Ne pas détruire un objet issu du pool : désactive-le et reparent pour garder la hiérarchie propre
            gameObject.SetActive(false);
            if (ObjectPooler.Instance != null)
                transform.SetParent(ObjectPooler.Instance.transform);
        }
    }
}
