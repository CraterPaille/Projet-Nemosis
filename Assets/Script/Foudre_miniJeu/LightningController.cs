using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningController : MonoBehaviour
{
    [Header("Références")]
    public GameObject lightningPrefab;   // Visuel logique (optionnel)
    public GameObject lightningObject;   // FX avec Animator (l’éclair visible)
    public Transform lightningParent;    // Parent hiérarchique

    [Header("Paramètres")]
    public float inputCooldown = 0.15f;  // Délai entre clics

    [Header("Options cachées")]
    [HideInInspector] public bool invertClicks = false;
    [HideInInspector] public bool bounceLightning = false;

    private float lastInputTime = 0f;

    void Awake()
    {
        // Crée le parent si nécessaire
        if (lightningParent == null)
        {
            GameObject found = GameObject.Find("LightningParent");
            if (found != null)
                lightningParent = found.transform;
            else
            {
                GameObject go = new GameObject("LightningParent");
                lightningParent = go.transform;
            }
        }
    }

    void Update()
    {
        if (Time.time - lastInputTime < inputCooldown) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = 0f; // plan 2D

            Vector2 clickPos2D = new Vector2(worldPos.x, worldPos.y);
            RaycastHit2D hit = Physics2D.Raycast(clickPos2D, Vector2.zero);

            lastInputTime = Time.time;

            if (hit.collider != null && hit.collider.CompareTag("Enemy"))
            {
                Debug.Log(" Ennemi cliqué : " + hit.collider.name);
                SpawnLightning(hit.collider.transform.position, false);
            }
            else
            {
                Debug.Log(" Foudre ratée à " + clickPos2D);
                SpawnLightning(worldPos, true);
            }
        }
    }

    void SpawnLightning(Vector3 targetPos, bool missed)
    {
        if (lightningObject == null)
        {
            Debug.LogError("[LightningController] lightningObject n'est pas assigné !");
            return;
        }

        
        float lightningLength = 4f; // ajustable selon la taille du sprite ou test visuel
        Vector3 spawnPos = targetPos + Vector3.up * lightningLength / 2f;

        // --- Instancie l’éclair ---
        GameObject impact = Instantiate(lightningObject, spawnPos, Quaternion.identity, lightningParent);

        // Ajuste la taille
        impact.transform.localScale *= missed ? 0.35f : 0.5f;

        
        if (missed)
        {
            SpriteRenderer sr = impact.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.color = new Color(1f, 0.4f, 0.4f); // rouge clair
            Animator anim = impact.GetComponent<Animator>();
            if (anim != null)
                anim.speed = 1.4f;
        }

        // --- Oriente vers le bas ---
        impact.transform.rotation = Quaternion.Euler(0, 0, 0); // garde vertical vers le bas (axe Y-)

        // --- Animation ---
        Animator impactAnim = impact.GetComponent<Animator>();
        if (impactAnim != null)
            StartCoroutine(DestroyAfterAnimation(impact, impactAnim));
        else
            Destroy(impact, missed ? 1f : 1.5f);

        // --- (optionnel) logique interne ---
        if (lightningPrefab != null)
        {
            GameObject go = Instantiate(lightningPrefab, spawnPos, Quaternion.identity, lightningParent);
            LightningVisual lv = go.GetComponent<LightningVisual>();
            if (lv != null)
            {
                lv.SetTarget(targetPos);
                lv.isBounce = bounceLightning;
            }
        }
    }

    private IEnumerator DestroyAfterAnimation(GameObject obj, Animator animator)
    {
        if (animator.runtimeAnimatorController == null)
        {
            Destroy(obj, 1.5f);
            yield break;
        }

        AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
        if (clipInfo.Length > 0)
        {
            float clipLength = clipInfo[0].clip.length / animator.speed;
            yield return new WaitForSeconds(clipLength);
        }
        else
        {
            yield return new WaitForSeconds(1.5f);
        }

        Destroy(obj);
    }
}
