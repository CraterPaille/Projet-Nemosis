using UnityEngine;

public class Laser : MonoBehaviour
{
    private int damage;

    public void SetDamage(int dmg)
    {
        damage = dmg;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            ChronosGameManager.Instance.DamagePlayer(damage);
        }
    }
}
