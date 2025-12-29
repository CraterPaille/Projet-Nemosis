using UnityEngine;
using System.Collections.Generic;

public class LightningVisual : MonoBehaviour
{
    public float fallSpeed = 20f;
    public float hitRadius = 1f;     // zone d’impact
    public bool isBounce = false;
    public int maxBounces = 1;
    public float bounceRadius = 1.5f;

    private Vector3 targetPos;

    public void SetTarget(Vector3 target)
    {
        targetPos = target;
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPos, fallSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPos) < 0.05f)
        {
            // Détection des ennemis autour
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, hitRadius);
            HashSet<GameObject> alreadyKilled = new HashSet<GameObject>();

            foreach (var h in hits)
            {
                if (h.CompareTag("Enemy"))
                {
                    h.GetComponent<Enemy>()?.Kill();
                    alreadyKilled.Add(h.gameObject);
                }
            }

            // Bounce si activé
            if (isBounce)
            {
                int b = 0;
                Collider2D[] bounceHits = Physics2D.OverlapCircleAll(transform.position, bounceRadius);
                foreach (var h in bounceHits)
                {
                    if (b >= maxBounces) break;
                    if (h.CompareTag("Enemy") && !alreadyKilled.Contains(h.gameObject))
                    {
                        h.GetComponent<Enemy>()?.Kill();
                        alreadyKilled.Add(h.gameObject);
                        b++;
                    }
                }
            }

            Destroy(gameObject);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(targetPos, hitRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(targetPos, bounceRadius);
    }
}
