using UnityEngine;
using System.Collections;

public class GasterBlaster : MonoBehaviour
{
    public GameObject laserPrefab;
    public Transform firePoint;
    public GameObject chargeEffectPrefab;
    public float aimTime = 0.5f;
    public float laserDuration = 1.5f;
    public int damage = 15;
    public float laserSpeed = 8f;
    public float chargeDuration = 1f;

    [HideInInspector] public float initialDelay = 0f;
    [HideInInspector] public bool forceCardinalDirection = false;
    [HideInInspector] public Vector3 forcedDirection = Vector3.zero;

    // Tags optionnels pour utiliser la pool
    public string laserPoolTag = "";  // ex: "Laser"
    public string chargePoolTag = ""; // ex: "Charge"

    private Transform player;
    private Vector3 targetDirection;
    public static int ActiveCount { get; private set; } = 0;

    private void OnEnable()
    {
        ActiveCount++;
        StopAllCoroutines();

        Debug.Log($"GasterBlaster OnEnable: charge={chargeDuration}s speed={laserSpeed}");
        StartCoroutine(SequenceRoutine());
    }

    private void OnDisable()
    {
        ActiveCount = Mathf.Max(0, ActiveCount - 1);
    }

    private IEnumerator SequenceRoutine()
    {
        // Attendre initialDelay si nécessaire (temps normal)
        if (initialDelay > 0f)
        {
            yield return new WaitForSeconds(initialDelay);
        }

        // Calculer la direction cible
        player = GameObject.FindGameObjectWithTag("PlayerSoul")?.transform;

        if (forceCardinalDirection && forcedDirection != Vector3.zero)
        {
            targetDirection = forcedDirection.normalized;
            Debug.Log($"GasterBlaster: Using forced direction {targetDirection}");
        }
        else if (player != null)
        {
            targetDirection = (player.position - transform.position).normalized;
            Debug.Log($"GasterBlaster: Targeting player at {player.position}");
        }
        else
        {
            targetDirection = transform.right;
            Debug.LogWarning("GasterBlaster: No target, using default direction");
        }

        // Orienter le blaster
        float angle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Lancer charge et tir
        yield return StartCoroutine(ChargeAndFire());
    }

    private IEnumerator ChargeAndFire()
    {
        // Effet de chargement
        GameObject charge = null;
        if (chargeEffectPrefab != null && firePoint != null)
        {
            charge = Instantiate(chargeEffectPrefab, firePoint.position, firePoint.rotation, firePoint);
        }

        // Temps de charge (temps normal)
        yield return new WaitForSeconds(chargeDuration);

        // Retire l'effet de charge
        if (charge != null)
            Destroy(charge);

        // Tire le laser
        if (laserPrefab == null)
        {
            Debug.LogError("GasterBlaster: laserPrefab is NULL!");
            yield break;
        }
        if (firePoint == null)
        {
            Debug.LogError("GasterBlaster: firePoint is NULL!");
            yield break;
        }

        Debug.Log($"GasterBlaster FIRING: pos={firePoint.position} dir={targetDirection} speed={laserSpeed}");

        GameObject laser = Instantiate(laserPrefab, firePoint.position, Quaternion.identity);
        laser.transform.right = targetDirection;

        // Configure le laser
        var laserComp = laser.GetComponent<Laser>();
        if (laserComp != null)
            laserComp.SetDamage(damage);

        var laserMove = laser.GetComponent<LaserMovement>();
        if (laserMove != null)
        {
            laserMove.speed = laserSpeed;
            laserMove.useRealtime = false; // Toujours temps normal maintenant
            Debug.Log($"LaserMovement configured: speed={laserSpeed}");
        }
        else
        {
            Debug.LogError("GasterBlaster: LaserMovement component MISSING on laser prefab!");
        }

        Destroy(laser, laserDuration);

        // Désactive le blaster après un délai
        yield return new WaitForSeconds(aimTime + 0.1f);
        gameObject.SetActive(false);

        if (ObjectPooler.Instance != null)
            transform.SetParent(ObjectPooler.Instance.transform);
    }
}