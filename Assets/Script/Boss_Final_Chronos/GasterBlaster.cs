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

    // Compteur d'instances actives (utilisé pour limiter le spawn)
    public static int ActiveCount { get; private set; } = 0;

    private void OnEnable()
    {
        ActiveCount++;

        // S'assurer qu'aucune coroutine précédente ne tourne
        StopAllCoroutines();

        // Initialisation et démarrage du tir : utilisable à chaque activation (pool)
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player != null)
        {
            targetDirection = (player.position - transform.position).normalized;
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

    private void OnDisable()
    {
        ActiveCount = Mathf.Max(0, ActiveCount - 1);
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
            var laserComp = laser.GetComponent<Laser>();
            if (laserComp != null)
                laserComp.SetDamage(damage);
            Destroy(laser, laserDuration);
        }

        // Ne pas détruire un objet géré par le pool : désactiver après délai
        StartCoroutine(DisableAfter(aimTime + 0.1f));
    }

    private IEnumerator DisableAfter(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Plutôt que Destroy(gameObject), on désactive l'objet pour qu'il retourne au pool
        gameObject.SetActive(false);

        // Reparent optionnel pour garder la hiérarchie propre
        if (ObjectPooler.Instance != null)
            transform.SetParent(ObjectPooler.Instance.transform);
    }
}
