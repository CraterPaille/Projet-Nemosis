using UnityEngine;

public class Bone : MonoBehaviour
{
    public float speed = 4f;
    public int damage = 5;

    // Constantes pré-calculées
    private const float DESPAWN_Y = -6f;
    private static readonly int PlayerTagHash = "PlayerSoul".GetHashCode();

    // Cache pour éviter les allocations
    private Vector3 movement;

    void OnEnable()
    {
        // Pré-calcule le vecteur de mouvement
        movement = Vector2.down * speed;
    }

    void Update()
    {
        // Utilise le vecteur pré-calculé et Time.deltaTime
        transform.position += movement * Time.deltaTime;

        // Vérification optimisée de la position Y
        if (transform.position.y < DESPAWN_Y)
        {
            gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Comparaison optimisée avec hashcode
        if (other.tag.GetHashCode() == PlayerTagHash)
        {
            ChronosGameManager.Instance.DamagePlayer(damage);
            gameObject.SetActive(false);
        }
    }
}