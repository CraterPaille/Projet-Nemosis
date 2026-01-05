using UnityEngine;

public class Ball : MonoBehaviour
{
    public float speed = 10f;
    public Vector2 direction = Vector2.down;
    public float maxBounceAngleDeg = 75f;
    public float Hit = 1f;
    public float Damage = 1f;
    public float randomAngleRange = 30f; // Plage d'angle aléatoire en degrés
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (direction.sqrMagnitude < 0.01f)
            direction = Vector2.down;
        
        // Ajoute un angle aléatoire à la direction initiale
        float randomAngle = Random.Range(-randomAngleRange, randomAngleRange);
        direction = Quaternion.Euler(0, 0, randomAngle) * direction;
        direction = direction.normalized;
    }

    void FixedUpdate()
    {
rb.linearVelocity = direction * speed * Hit;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
            Debug.Log("Collision avec " + collision.gameObject.name);

        Vector2 normal = collision.contacts.Length > 0 ? collision.contacts[0].normal : Vector2.up;

        // Si on touche un mur vertical, inverser la composante X de la normale
        // pour s'assurer que la balle parte vers l'extérieur du mur
        // (ex. toucher le bord droit -> aller à gauche)


        if (collision.gameObject.CompareTag("Player"))
        {
            Damage += 1f;
            Hit += 0.2f;
            // calcul simple d'angle en fonction du point d'impact (hitFactor ∈ [-1,1])
            float hitPoint = (transform.position.x - collision.transform.position.x);
            float halfWidth = collision.collider.bounds.size.x / 2f;
            float hitFactor = Mathf.Clamp(hitPoint / halfWidth, -1f, 1f);

            // angle en radians mesuré depuis la verticale (vers le haut)
            float angleRad = hitFactor * maxBounceAngleDeg * Mathf.Deg2Rad;

            // direction = (sin(angle), cos(angle)) pour que y soit positif (vers le haut)
            direction = new Vector2(Mathf.Sin(angleRad), Mathf.Cos(angleRad)).normalized;

        }
        else if (collision.gameObject.CompareTag("Wall"))
        {
            Damage += 1f;
            Hit += 0.2f;
            direction = Vector2.Reflect(direction, normal).normalized;
        }
    }
}