using UnityEngine;
using System.Collections;

public class GasterBlaster : MonoBehaviour
{
    public GameObject laserPrefab; // Assigne ce prefab dans l'inspecteur
    public Transform firePoint; // Le point de départ du laser
    public GameObject chargeEffectPrefab; // Un prefab de cube blanc ou effet de chargement
    public float aimTime = 0.5f;
    public float laserDuration = 1.5f;
    public int damage = 15;
    public float laserSpeed = 8f;
    public float chargeDuration = 1f;

    private Transform player;
    private Vector3 targetDirection;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player != null)
        {
            targetDirection = (player.transform.position - transform.position).normalized;
            float angle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
            StartCoroutine(ChargeAndFire());
        }
        else
        {
            targetDirection = transform.right; // fallback
            Debug.LogError("GasterBlaster: Player not found!");
        }
    }

    private IEnumerator ChargeAndFire()
    {
        // Affiche l’effet de chargement
        GameObject charge = null;
        if (chargeEffectPrefab != null && firePoint != null)
        {
            charge = Instantiate(chargeEffectPrefab, firePoint.position, firePoint.rotation, firePoint);
        }

        yield return new WaitForSeconds(chargeDuration);

        // Retire l’effet de chargement
        if (charge != null)
            Destroy(charge);

        // Tire le laser dans la direction mémorisée
        if (laserPrefab != null && firePoint != null)
        {
            GameObject laser = Instantiate(laserPrefab, firePoint.position, Quaternion.identity);
            // Oriente le laser
            laser.transform.right = targetDirection;
            laser.GetComponent<Laser>().SetDamage(damage);
            Destroy(laser, laserDuration);
        }

        Destroy(gameObject, aimTime + 0.1f); // Détruit le GasterBlaster après avoir tiré
    }
}
