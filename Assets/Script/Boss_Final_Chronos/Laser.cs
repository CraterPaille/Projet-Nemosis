using UnityEngine;

public class Laser : MonoBehaviour
{
    private int damage;
    private bool hasHitShield = false;
    private bool hasHitPlayer = false;

    public int lifeTime = 3;

    public void SetDamage(int dmg)
    {
        damage = dmg;
        Debug.Log($"Laser damage set to {dmg}");
    }

    void OnEnable()
    {
        hasHitShield = false;
        hasHitPlayer = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[LASER] Collision with: {other.gameObject.name} (Tag: {other.tag})");

        // PRIORITÉ 1 : Bouclier (tag exact "Shield")
        if (other.CompareTag("Shield") && !hasHitShield)
        {
            hasHitShield = true;
            Debug.Log($" [LASER] BLOCKED by shield! Destroying laser.");
            Destroy(gameObject);
            return;
        }

        // PRIORITÉ 2 : Joueur (tag exact "PlayerSoul")
        if (other.CompareTag("PlayerSoul") && !hasHitPlayer && !hasHitShield)
        {
            hasHitPlayer = true;
            Debug.Log($" [LASER] HIT PLAYER for {damage} damage!");
            ChronosGameManager.Instance.DamagePlayer(damage);
            Destroy(gameObject);
            return;
        }
    }
}