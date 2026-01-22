using UnityEngine;

public class PlatformJump : MonoBehaviour
{
    public float jumpForce = 11f;

    void OnCollisionEnter2D(Collision2D collision)
    {
        // On vérifie qu'on tombe (on ne saute pas si on tape par en dessous)
        if (collision.relativeVelocity.y <= 0f)
        {
            Rigidbody2D rb = collision.collider.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                // 1. On fait sauter
                Vector2 velocity = rb.linearVelocity;
                velocity.y = jumpForce;
                rb.linearVelocity = velocity;

                // 2. On joue le son (si le joueur a un AudioSource)
                AudioSource audio = collision.collider.GetComponent<AudioSource>();
                if (audio != null)
                {
                    audio.Play();
                }
            }
        }
    }
}