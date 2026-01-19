using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LightningController : MonoBehaviour
{
    [Header("Références")]
    public GameObject lightningPrefab;
    public GameObject lightningObject;
    public Transform lightningParent;

    [Header("Paramètres")]
    public float inputCooldown = 0.15f;  // Délai entre clics
    public float stickDeadzone = 0.2f;   // seuil pour considérer le stick actif
    public float gamepadCursorSpeed = 1200f; // vitesse du curseur manette en pixels/s

    [Header("Curseur UI (optionnel)")]
    public RectTransform uiCursor; // assigner une Image (pivot centre) dans un Canvas Screen Space - Overlay

    [Header("Options cachées")]
    [HideInInspector] public bool invertClicks = false;
    [HideInInspector] public bool bounceLightning = false;

    private float lastInputTime = 0f;

    // Input System
    private InputAction pointAction;
    private InputAction clickAction;
    private InputAction stickAction;

    // état du curseur virtuel
    private Vector2 virtualCursorScreenPos;
    private bool usingGamepadCursor = false;

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

        // Actions : position souris/pointer, clic (souris + manette), stick
        pointAction = new InputAction("Point", InputActionType.Value, "<Pointer>/position");
        clickAction = new InputAction("Click", InputActionType.Button);
        clickAction.AddBinding("<Mouse>/leftButton");
        clickAction.AddBinding("<Gamepad>/buttonSouth");
        clickAction.AddBinding("<Gamepad>/rightTrigger");
        stickAction = new InputAction("Stick", InputActionType.Value, "<Gamepad>/leftStick");

        // position initiale du curseur = centre écran
        virtualCursorScreenPos = new Vector2(Screen.width / 2f, Screen.height / 2f);
    }

    void OnEnable()
    {
        pointAction.Enable();
        clickAction.Enable();
        stickAction.Enable();
    }

    void OnDisable()
    {
        pointAction.Disable();
        clickAction.Disable();
        stickAction.Disable();
    }

    void Update()
    {
        if (Time.time - lastInputTime < inputCooldown) return;

        // --- MISE A JOUR DU CURSEUR ---
        UpdateCursorState();

        // --- CLIC ---
        if (clickAction.WasPressedThisFrame())
        {
            lastInputTime = Time.time;

            Vector2 usedScreenPos = virtualCursorScreenPos;

            // Si uiCursor n'est pas assigné, on tente d'utiliser la position pointer (souris)
            if (uiCursor == null && Mouse.current != null)
            {
                usedScreenPos = pointAction.ReadValue<Vector2>();
            }

            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(usedScreenPos.x, usedScreenPos.y, Camera.main.nearClipPlane));
            worldPos.z = 0f; // plan 2D

            Vector2 clickPos2D = new Vector2(worldPos.x, worldPos.y);
            RaycastHit2D hit = Physics2D.Raycast(clickPos2D, Vector2.zero);

            if (hit.collider != null && hit.collider.CompareTag("Enemy"))
            {
                SpawnLightning(hit.collider.transform.position, false);
            }
            else
            {
                SpawnLightning(worldPos, true);
            }
        }
    }

    void UpdateCursorState()
    {
        bool mousePresent = Mouse.current != null;
        Vector2 mousePos = Vector2.zero;
        Vector2 stick = stickAction.ReadValue<Vector2>();

        // lecture position souris si disponible
        if (mousePresent)
            mousePos = pointAction.ReadValue<Vector2>();

        // Détecter activité récente : préférence souris si elle bouge, sinon manette si stick bouge
        bool mouseMoved = mousePresent && (Vector2.Distance(mousePos, virtualCursorScreenPos) > 0.001f || Mouse.current.delta.ReadValue().sqrMagnitude > 0.0f);
        bool gamepadActive = stick.sqrMagnitude >= stickDeadzone * stickDeadzone;

        if (mouseMoved)
        {
            usingGamepadCursor = false;
            virtualCursorScreenPos = mousePos;
        }
        else if (gamepadActive)
        {
            usingGamepadCursor = true;
            virtualCursorScreenPos += stick * gamepadCursorSpeed * Time.deltaTime;
            virtualCursorScreenPos.x = Mathf.Clamp(virtualCursorScreenPos.x, 0f, Screen.width);
            virtualCursorScreenPos.y = Mathf.Clamp(virtualCursorScreenPos.y, 0f, Screen.height);
        }
        // sinon on conserve virtualCursorScreenPos (stays where it was)

        // Mettre à jour UI cursor si assigné
        if (uiCursor != null)
        {
            uiCursor.gameObject.SetActive(true);
            RectTransform parentRect = uiCursor.parent as RectTransform;
            if (parentRect != null)
            {
                Vector2 anchored;
                // Canvas en Screen Space - Overlay -> camera = null
                RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, virtualCursorScreenPos, null, out anchored);
                uiCursor.anchoredPosition = anchored;
            }
        }

        // Optionnel : cacher le curseur OS si on utilise le curseur manette
        Cursor.visible = !usingGamepadCursor;
    }

    void SpawnLightning(Vector3 targetPos, bool missed)
    {
        if (lightningObject == null)
        {
            Debug.LogError("[LightningController] lightningObject n'est pas assigné !");
            return;
        }

        float lightningLength = 4f;
        Vector3 spawnPos = targetPos + Vector3.up * lightningLength / 2f;
        GameObject impact = Instantiate(lightningObject, spawnPos, Quaternion.identity, lightningParent);
        impact.transform.localScale *= missed ? 0.35f : 0.5f;

        if (missed)
        {
            SpriteRenderer sr = impact.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.color = new Color(1f, 0.4f, 0.4f);
            Animator anim = impact.GetComponent<Animator>();
            if (anim != null)
                anim.speed = 1.4f;
        }

        impact.transform.rotation = Quaternion.Euler(0, 0, 0);

        Animator impactAnim = impact.GetComponent<Animator>();
        if (impactAnim != null)
            StartCoroutine(DestroyAfterAnimation(impact, impactAnim));
        else
            Destroy(impact, missed ? 1f : 1.5f);

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
