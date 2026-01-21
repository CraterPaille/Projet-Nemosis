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

    public string laserPoolTag = "Laser";
    public string chargePoolTag = "Charge";

    // Cache statique
    private static Transform playerTransform;
    private static ObjectPooler pooler;
    private static readonly string PLAYER_TAG = "PlayerSoul";

    // Cache local
    private Transform cachedTransform;
    private Vector3 targetDirection;
    private WaitForSeconds chargeWait;
    private WaitForSeconds aimWait;

    public static int ActiveCount { get; private set; } = 0;

    private void Awake()
    {
        cachedTransform = transform;

        // Pré-cache les WaitForSeconds pour éviter les allocations
        chargeWait = new WaitForSeconds(chargeDuration);
        aimWait = new WaitForSeconds(aimTime + 0.1f);
    }

    private void OnEnable()
    {
        ActiveCount++;
        StopAllCoroutines();

        // Cache les références statiques si nécessaire
        if (pooler == null)
            pooler = ObjectPooler.Instance;

        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag(PLAYER_TAG);
            if (playerObj != null)
                playerTransform = playerObj.transform;
        }

        StartCoroutine(SequenceRoutine());
    }

    private void OnDisable()
    {
        ActiveCount = Mathf.Max(0, ActiveCount - 1);
    }

    private IEnumerator SequenceRoutine()
    {
        if (initialDelay > 0f)
        {
            yield return new WaitForSeconds(initialDelay);
        }

        // Calculer la direction cible
        if (forceCardinalDirection && forcedDirection != Vector3.zero)
        {
            targetDirection = forcedDirection.normalized;
        }
        else if (playerTransform != null)
        {
            targetDirection = (playerTransform.position - cachedTransform.position).normalized;
        }
        else
        {
            targetDirection = cachedTransform.right;
        }

        // Orienter le blaster
        float angle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
        cachedTransform.rotation = Quaternion.Euler(0, 0, angle);

        yield return ChargeAndFire();
    }

    private IEnumerator ChargeAndFire()
    {
        GameObject charge = null;
        bool useChargePool = !string.IsNullOrEmpty(chargePoolTag) &&
                             pooler != null &&
                             pooler.poolDictionary.ContainsKey(chargePoolTag);

        // Spawn charge effect
        if (chargeEffectPrefab != null && firePoint != null)
        {
            if (useChargePool)
            {
                charge = pooler.SpawnFromPool(chargePoolTag, firePoint.position, firePoint.rotation, firePoint, true);
            }
            else
            {
                charge = Instantiate(chargeEffectPrefab, firePoint.position, firePoint.rotation, firePoint);
            }
        }

        // Charge time (utilise le WaitForSeconds pré-caché)
        yield return chargeWait;

        // Retourne l'effet de charge
        if (charge != null)
        {
            if (useChargePool)
            {
                pooler.TryReturnToPool(chargePoolTag, charge);
            }
            else
            {
                Destroy(charge);
            }
        }

        // Vérifie les prérequis pour tirer
        if (laserPrefab == null || firePoint == null)
        {
            yield break;
        }

        // Spawn laser
        bool useLaserPool = !string.IsNullOrEmpty(laserPoolTag) &&
                            pooler != null &&
                            pooler.poolDictionary.ContainsKey(laserPoolTag);

        GameObject laser = null;

        if (useLaserPool)
        {
            laser = pooler.SpawnFromPool(laserPoolTag, firePoint.position, Quaternion.identity, null, true);
        }
        else
        {
            laser = Instantiate(laserPrefab, firePoint.position, Quaternion.identity);
        }

        if (laser != null)
        {
            // Configure le laser
            Transform laserTransform = laser.transform;
            laserTransform.right = targetDirection;

            Laser laserComp = laser.GetComponent<Laser>();
            if (laserComp != null)
            {
                laserComp.SetDamage(damage);
            }

            // Gère le retour à la pool ou destruction
            if (useLaserPool)
            {
                StartCoroutine(ReturnToPoolLater(laserPoolTag, laser, laserDuration));
            }
            else
            {
                Destroy(laser, laserDuration);
            }
        }

        // Désactive le blaster
        yield return aimWait;
        gameObject.SetActive(false);

        if (pooler != null)
            cachedTransform.SetParent(pooler.transform);
    }

    private IEnumerator ReturnToPoolLater(string tag, GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (obj != null && pooler != null)
        {
            pooler.TryReturnToPool(tag, obj);
        }
    }
}