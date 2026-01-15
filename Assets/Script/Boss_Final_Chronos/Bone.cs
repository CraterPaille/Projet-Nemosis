using UnityEngine;

public class Bone : MonoBehaviour
{
    public float speed = 4f;
    public int damage = 5;

    void Update()
    {
        transform.Translate(Vector2.down * speed * Time.deltaTime);
        if (transform.position.y < -6f)
            gameObject.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            ChronosGameManager.Instance.DamagePlayer(damage);
            gameObject.SetActive(false);
        }
    }
}
