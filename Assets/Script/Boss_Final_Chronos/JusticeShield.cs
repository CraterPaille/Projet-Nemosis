using UnityEngine;

public class JusticeShield : MonoBehaviour
{
    public int blockedCount = 0;

    [Header("Block Validation")]
    [Tooltip("Angle minimum pour bloquer un projectile (en degrés). 45° = bloque dans un cône de ±45°")]
    public float blockAngleThreshold = 45f;

    // Tag hashes
    private static readonly int LaserTagHash = "Laser".GetHashCode();
    private static readonly int NeedleTagHash = "needle".GetHashCode();

    // Cache du collider du bouclier
    private Collider2D shieldCollider;

    // Référence au controller pour obtenir la direction du bouclier
    private JusticeShieldController controller;

    void Awake()
    {
        shieldCollider = GetComponent<Collider2D>();
        controller = GetComponentInParent<JusticeShieldController>();
    }

    void OnEnable()
    {
        blockedCount = 0;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null || controller == null) return;

        int otherHash = other.tag.GetHashCode();

        // Gestion Laser (comportement actuel) et Needle (nouveau) : validation par angle
        if (otherHash == LaserTagHash || otherHash == NeedleTagHash)
        {
            // Obtenir transform du projectile (si Laser on préfère le component, sinon on prend le transform du collider)
            Transform projTransform = null;
            Laser laser = other.GetComponentInParent<Laser>() ?? other.GetComponent<Laser>();
            if (laser != null)
                projTransform = laser.transform;
            else
                projTransform = other.transform;

            // Vérifier si le bouclier doit bloquer ce projectile
            if (!ShouldBlockProjectile(projTransform))
            {
                Debug.Log($"<color=orange>[Shield] Projectile TRAVERSE (mauvaise direction)</color>");
                return;
            }

            // Bloqué : comportement spécifique
            blockedCount++;
            Debug.Log($"<color=cyan>[Shield] Projectile BLOQUE - Total: {blockedCount}</color>");

            // Empêcher retriggers multiples sur ce collider
            other.enabled = false;

            if (otherHash == LaserTagHash && laser != null)
            {
                // Signaler au laser pour l'effet de coupe + SFX
                laser.OnBlockedByShield(shieldCollider);
            }
            else
            {
                // Pour needle : jouer SFX si présent et retourner à la pool / désactiver proprement
                GameObject projGO = other.gameObject;
                if (projGO != null)
                {
                    // Jouer SFX via ChronosGameManager.sfxSource si disponible
                    var gm = ChronosGameManager.Instance;
                    if (gm != null && gm.sfxSource != null)
                    {
                        // Priorité : clip configuré dans le controller, sinon fallback gm.sfxDamage
                        AudioClip clip = controller != null && controller.needleBlockedSfx != null
                            ? controller.needleBlockedSfx
                            : gm.sfxDamage;

                        if (clip != null)
                            gm.sfxSource.PlayOneShot(clip, 0.9f);
                    }

                    // Essayer de retourner à la pool si ObjectPooler gère "needle"
                    var pooler = ObjectPooler.Instance;
                    bool returned = false;
                    if (pooler != null)
                    {
                        try
                        {
                            // Preferer TryReturnToPool si existant, sinon ReturnToPool
                            var methodTry = pooler.GetType().GetMethod("TryReturnToPool");
                            if (methodTry != null)
                            {
                                methodTry.Invoke(pooler, new object[] { "needle", projGO });
                                returned = true;
                            }
                            else
                            {
                                var methodRet = pooler.GetType().GetMethod("ReturnToPool");
                                if (methodRet != null)
                                {
                                    methodRet.Invoke(pooler, new object[] { "needle", projGO });
                                    returned = true;
                                }
                            }
                        }
                        catch
                        {
                            returned = false;
                        }
                    }

                    if (!returned)
                    {
                        // fallback : désactiver l'objet proprement
                        projGO.SetActive(false);
                    }
                }
            }
        }
    }

    private bool ShouldBlockProjectile(Transform projTransform)
    {
        if (projTransform == null || controller == null || shieldCollider == null)
            return false;

        int shieldDirection = controller.GetShieldDirection();
        Vector2 shieldNormal = GetShieldNormal(shieldDirection);

        Vector2 projPos = projTransform.position;
        Vector2 shieldPos = transform.position;

        Vector2 contactPoint = shieldCollider != null
            ? shieldCollider.ClosestPoint(projPos)
            : shieldPos;

        if ((contactPoint - projPos).sqrMagnitude < 1e-6f)
            contactPoint = shieldPos;

        Vector2 outwardDir = (projPos - contactPoint).normalized;
        float dotProduct = Vector2.Dot(outwardDir, shieldNormal);
        float angleThreshold = Mathf.Cos(blockAngleThreshold * Mathf.Deg2Rad);
        bool shouldBlock = dotProduct >= angleThreshold;

        Debug.DrawRay(contactPoint, outwardDir * 1.2f, Color.yellow, 0.5f);
        Debug.DrawRay(transform.position, shieldNormal * 1.2f, shouldBlock ? Color.green : Color.red, 0.5f);

        if (!shouldBlock)
        {
            Debug.Log($"[Shield] TRAVERSE dirIndex={shieldDirection} dot={dotProduct:F3} thresh={angleThreshold:F3} outward={outwardDir} contact={contactPoint}");
        }

        return shouldBlock;
    }

    private Vector2 GetShieldNormal(int direction)
    {
        return direction switch
        {
            0 => Vector2.right,
            1 => Vector2.up,
            2 => Vector2.left,
            3 => Vector2.down,
            _ => Vector2.right
        };
    }
}