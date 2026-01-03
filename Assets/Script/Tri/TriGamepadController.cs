using UnityEngine;
using UnityEngine.InputSystem;

public class TriGamepadController : MonoBehaviour
{
    [Header("Curseur")]
    public RectTransform uiCursor; // Image UI (pivot centre) dans un Canvas (Screen Space - Overlay ou Screen Space - Camera)

    public float gamepadCursorSpeed = 1200f; // px/s
    public float stickDeadzone = 0.2f;

    // Seuils pour la bascule entre manette / souris
    public float switchBackToMouseDelay = 0.25f; // seconde avant d'accepter automatiquement la souris après usage du pad
    public float mouseMoveThreshold = 2f; // nb de pixels/min mouvement considéré comme volontaire

    // Input System
    private InputAction pointAction;
    private InputAction clickAction;
    private InputAction stickAction;

    private Vector2 virtualCursorScreenPos;
    private bool usingGamepadCursor = false;

    // Drag state
    private Soul selectedSoul;
    private Vector3 grabOffset;

    [Header("Debug")]
    public bool logStickValues = false;

    // temps du dernier usage du gamepad
    private float lastGamepadUseTime = -10f;

    void Awake()
    {
        // Actions
        pointAction = new InputAction("Point", InputActionType.Value, "<Pointer>/position");
        clickAction = new InputAction("Click", InputActionType.Button);
        clickAction.AddBinding("<Mouse>/leftButton");
        clickAction.AddBinding("<Gamepad>/buttonSouth");
        clickAction.AddBinding("<Gamepad>/rightTrigger");
        stickAction = new InputAction("Stick", InputActionType.Value, "<Gamepad>/leftStick");

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
        if (!TriGameManager.Instance.IsPlaying)
        {
            if (uiCursor != null) uiCursor.gameObject.SetActive(false);
            return;
        }

        UpdateCursorState();

        // Press
        if (clickAction.WasPressedThisFrame())
        {
            Vector3 worldPos = ScreenToWorld(virtualCursorScreenPos);
            TryPickSoulAt(worldPos);
        }

        // While held -> move selected
        if (clickAction.IsPressed() && selectedSoul != null)
        {
            Vector3 worldPos = ScreenToWorld(virtualCursorScreenPos);
            selectedSoul.transform.position = worldPos + grabOffset;
        }

        // Release
        if (clickAction.WasReleasedThisFrame())
        {
            if (selectedSoul != null)
            {
                selectedSoul.isBeingDragged = false;
                selectedSoul = null;
            }
        }
    }

    void UpdateCursorState()
    {
        bool mousePresent = Mouse.current != null;
        Vector2 mousePos = Vector2.zero;

        if (mousePresent)
            mousePos = pointAction.ReadValue<Vector2>();

        // Lire stick depuis l'InputAction
        Vector2 stick = stickAction.ReadValue<Vector2>();

        // Fallback : si action ne donne rien, lire directement depuis Gamepad.current
        if (stick.sqrMagnitude < 0.0001f && Gamepad.current != null)
        {
            stick = Gamepad.current.leftStick.ReadValue();
        }

        if (logStickValues)
            Debug.Log($"Stick raw: {stick}");

        // Appliquer deadzone manuelle (pour éviter micro-mouvements)
        if (stick.magnitude < stickDeadzone)
            stick = Vector2.zero;
        else
            stick = stick.normalized * ((stick.magnitude - stickDeadzone) / (1f - stickDeadzone));

        bool mouseMoved = mousePresent && (Vector2.Distance(mousePos, virtualCursorScreenPos) > 0.001f || Mouse.current.delta.ReadValue().sqrMagnitude > 0.5f);
        bool gamepadActive = stick.sqrMagnitude > 0.0001f;

        // Priorité au gamepad : si stick actif, utiliser le curseur manette même si une souris est présente
        if (gamepadActive)
        {
            // mémoriser la dernière utilisation du gamepad
            lastGamepadUseTime = Time.time;

            // Si on passe de la souris au gamepad, initialiser le curseur virtuel sur la position actuelle de la souris
            if (!usingGamepadCursor && mousePresent)
                virtualCursorScreenPos = mousePos;

            usingGamepadCursor = true;
            virtualCursorScreenPos += stick * gamepadCursorSpeed * Time.deltaTime;
        }
        else
        {
            // Déterminer si le mouvement souris est volontaire (clic / grand mouvement) OU si le délai depuis le pad est écoulé
            bool mouseIntentional = false;
            if (mousePresent)
            {
                var mouseDelta = Mouse.current.delta.ReadValue();
                bool mouseMovedEnough = mouseDelta.magnitude > mouseMoveThreshold;
                bool mouseClicked = Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.leftButton.isPressed;
                bool cooldownExpired = (Time.time - lastGamepadUseTime) > switchBackToMouseDelay;

                mouseIntentional = mouseMovedEnough || mouseClicked || cooldownExpired;
            }

            if (mouseMoved && mouseIntentional)
            {
                usingGamepadCursor = false;
                virtualCursorScreenPos = mousePos;
            }
            // sinon : on reste sur virtualCursorScreenPos (ne pas "coller" au pointeur OS)
        }

        // Clamp sur le rect du Canvas si disponible, sinon sur l'écran
        Rect canvasPixelRect = new Rect(0, 0, Screen.width, Screen.height);
        Camera uiCamera = null;
        if (uiCursor != null)
        {
            var parentRect = uiCursor.parent as RectTransform;
            if (parentRect != null)
            {
                var canvas = parentRect.GetComponentInParent<UnityEngine.Canvas>();
                if (canvas != null)
                {
                    // pixelRect est en coordonnées écran pour ScreenSpace Overlay & ScreenSpace Camera
                    canvasPixelRect = canvas.pixelRect;
                    if (canvas.renderMode == UnityEngine.RenderMode.ScreenSpaceCamera)
                        uiCamera = canvas.worldCamera != null ? canvas.worldCamera : Camera.main;
                }
            }
        }

        virtualCursorScreenPos.x = Mathf.Clamp(virtualCursorScreenPos.x, canvasPixelRect.xMin, canvasPixelRect.xMax);
        virtualCursorScreenPos.y = Mathf.Clamp(virtualCursorScreenPos.y, canvasPixelRect.yMin, canvasPixelRect.yMax);

        // UI Cursor update
        if (uiCursor != null)
        {
            uiCursor.gameObject.SetActive(true);
            RectTransform parentRect = uiCursor.parent as RectTransform;
            if (parentRect != null)
            {
                Vector2 anchored;
                // Utiliser la camera du Canvas si Canvas en Screen Space - Camera, sinon null
                RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, virtualCursorScreenPos, uiCamera, out anchored);
                uiCursor.anchoredPosition = anchored;
            }
        }

        // Optionnel : cacher le curseur OS quand manette active
        Cursor.visible = !usingGamepadCursor;
    }

    Vector3 ScreenToWorld(Vector2 screenPos)
    {
        // Si le Canvas est en Screen Space - Camera, il est préférable d'utiliser la même camera.
        Camera cam = Camera.main;
        if (uiCursor != null)
        {
            var canvas = uiCursor.GetComponentInParent<UnityEngine.Canvas>();
            if (canvas != null && canvas.renderMode == UnityEngine.RenderMode.ScreenSpaceCamera)
                cam = canvas.worldCamera != null ? canvas.worldCamera : cam;
        }

        Vector3 p = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, cam.nearClipPlane + 1f));
        p.z = 0f;
        return p;
    }

    void TryPickSoulAt(Vector3 worldPos)
    {
        Collider2D hit = Physics2D.OverlapPoint(worldPos);
        if (hit == null) return;

        Soul s = hit.GetComponent<Soul>();
        if (s == null) return;

        // Commencer le drag
        selectedSoul = s;
        selectedSoul.isBeingDragged = true;
        grabOffset = selectedSoul.transform.position - worldPos;
    }
}