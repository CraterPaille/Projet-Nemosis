using UnityEngine;
using UnityEngine.InputSystem;

public enum SoulType { Good, Neutral, Bad }

[RequireComponent(typeof(Collider2D))]
public class Soul : MonoBehaviour
{
    public enum MovementType { Vertical, Horizontal }
    public MovementType movementType = MovementType.Vertical;

    public SoulType type;
    public float fallSpeed = 2f;
    private Vector3 pointerOffset;

    // Input System
    private InputAction pointAction;
    private InputAction clickAction;
    private InputAction stickAction;

    // Indique si l'objet est actuellement déplacé (souris ou manette)
    [HideInInspector] public bool isBeingDragged = false;

    // Curseur virtuel partagé (permet de piloter avec manette)
    private static Vector2 s_virtualCursorScreenPos;
    private static bool s_usingGamepadCursor;
    private static bool s_virtualCursorInitialized = false;
    private float stickDeadzone = 0.2f;
    private float gamepadCursorSpeed = 1200f;

    private void Awake()
    {
        // Actions : position souris/pointer, clic (souris + manette), stick
        pointAction = new InputAction("Point", InputActionType.Value, "<Pointer>/position");
        clickAction = new InputAction("Click", InputActionType.Button);
        clickAction.AddBinding("<Mouse>/leftButton");
        clickAction.AddBinding("<Gamepad>/buttonSouth");
        clickAction.AddBinding("<Gamepad>/rightTrigger");
        stickAction = new InputAction("Stick", InputActionType.Value, "<Gamepad>/leftStick");

        // Initialise le curseur virtuel au centre de l'écran une seule fois
        if (!s_virtualCursorInitialized)
        {
            s_virtualCursorScreenPos = new Vector2(Screen.width / 2f, Screen.height / 2f);
            s_usingGamepadCursor = false;
            s_virtualCursorInitialized = true;
        }
    }

    private void OnEnable()
    {
        pointAction.Enable();
        clickAction.Enable();
        stickAction.Enable();
    }

    private void OnDisable()
    {
        pointAction.Disable();
        clickAction.Disable();
        stickAction.Disable();
    }

    private void Update()
    {
        if (!TriGameManager.Instance.IsPlaying) return;

        // Mise à jour de l'état du curseur (souris vs manette)
        UpdateCursorState();

        // Gestion du drag via Input System (compatible souris & virtual mouse)
        // Si on n'est pas en train de draguer, regarder si on commence un drag sur cet objet
        if (!isBeingDragged)
        {
            if (clickAction.WasPressedThisFrame())
            {
                Vector2 usedScreenPos = s_virtualCursorScreenPos;
                if (Mouse.current != null)
                    usedScreenPos = pointAction.ReadValue<Vector2>();

                Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(usedScreenPos.x, usedScreenPos.y, Camera.main.nearClipPlane));
                worldPos.z = transform.position.z;
                RaycastHit2D hit = Physics2D.Raycast(new Vector2(worldPos.x, worldPos.y), Vector2.zero);

                if (hit.collider != null && hit.collider == GetComponent<Collider2D>())
                {
                    // Commence le drag
                    isBeingDragged = true;
                    pointerOffset = transform.position - new Vector3(worldPos.x, worldPos.y, transform.position.z);
                }
            }
        }
        else
        {
            // Si on est en train de draguer, déplacer tant que le bouton est enfoncé
            float clickVal = clickAction.ReadValue<float>();
            if (clickVal > 0.5f)
            {
                Vector2 usedScreenPos = s_virtualCursorScreenPos;
                if (Mouse.current != null)
                    usedScreenPos = pointAction.ReadValue<Vector2>();

                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(usedScreenPos.x, usedScreenPos.y, Camera.main.nearClipPlane));
                transform.position = new Vector3(mouseWorldPos.x + pointerOffset.x, mouseWorldPos.y + pointerOffset.y, transform.position.z);
            }
            else if (clickAction.WasReleasedThisFrame())
            {
                // Relâchement
                isBeingDragged = false;
            }
        }

        // Ne pas appliquer le mouvement automatique si on est en train de draguer
        if (isBeingDragged) return;

        if (movementType == MovementType.Vertical)
        {
            transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);
        }
        else if (movementType == MovementType.Horizontal)
        {
            transform.Translate(Vector3.right * fallSpeed * Time.deltaTime);
        }

        if (transform.position.y < -6f || transform.position.x < -10f || transform.position.x > 10f)
        {
            TriGameManager.Instance.AddScore(-2);
            Destroy(gameObject);
        }
    }

    private void UpdateCursorState()
    {
        bool mousePresent = Mouse.current != null;
        Vector2 mousePos = Vector2.zero;
        Vector2 stick = stickAction.ReadValue<Vector2>();

        if (mousePresent)
            mousePos = pointAction.ReadValue<Vector2>();

        bool mouseMoved = mousePresent && (Vector2.Distance(mousePos, s_virtualCursorScreenPos) > 0.001f || (Mouse.current != null && Mouse.current.delta.ReadValue().sqrMagnitude > 0f));
        bool gamepadActive = stick.sqrMagnitude >= stickDeadzone * stickDeadzone;

        if (mouseMoved)
        {
            s_usingGamepadCursor = false;
            s_virtualCursorScreenPos = mousePos;
        }
        else if (gamepadActive)
        {
            s_usingGamepadCursor = true;
            s_virtualCursorScreenPos += stick * gamepadCursorSpeed * Time.deltaTime;
            s_virtualCursorScreenPos.x = Mathf.Clamp(s_virtualCursorScreenPos.x, 0f, Screen.width);
            s_virtualCursorScreenPos.y = Mathf.Clamp(s_virtualCursorScreenPos.y, 0f, Screen.height);
        }

        // Optionnel : cacher le curseur OS si on utilise le curseur manette
        Cursor.visible = !s_usingGamepadCursor;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        SortingZone zone = other.GetComponent<SortingZone>();
        if (zone != null)
        {
            bool correct = zone.AcceptsSoul(type);

            if (correct)
                TriGameManager.Instance.AddScore(1);
            else
                TriGameManager.Instance.AddScore(-1);

            Destroy(gameObject);
        }
    }
}