using UnityEngine;

public class Jewel : MonoBehaviour
{


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            ChronosGameManager.Instance.OnJewelCollected();
            Destroy(gameObject);
        }
    }
}
