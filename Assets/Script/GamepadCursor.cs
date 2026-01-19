using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class GamepadCursor : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveCursor;
    [SerializeField] private InputActionReference click;

    [Header("Settings")]
    [SerializeField] private float cursorSpeed = 1000f;
    [SerializeField] private float stickDeadzone = 0.15f; // Réduit pour plus de réactivité
    [SerializeField] private Texture2D cursorTexture;
    [SerializeField] private Texture2D cursorClickTexture; // Ajouté pour le sprite de clic
    [SerializeField] private CursorMode cursorMode = CursorMode.Auto;
    [SerializeField] private Vector2 hotSpot = Vector2.zero;

    private bool usingGamepad;
    private Vector2 lastMousePosition;
    private Vector2 virtualPosition; // Position virtuelle du curseur

    void OnEnable()
    {
        moveCursor.action.Enable();
        click.action.Enable();

        if (Mouse.current != null)
        {
            lastMousePosition = Mouse.current.position.ReadValue();
            virtualPosition = lastMousePosition;
            Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);
        }
    }

    void OnDisable()
    {
        moveCursor.action.Disable();
        click.action.Disable();
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    // Utiliser FixedUpdate ou LateUpdate peut causer du lag - Update est correct
    void Update()
    {
        HandleMouseDetection();
        HandleGamepadMovement();
        HandleGamepadClick();
    }

    void HandleGamepadMovement()
    {
        Vector2 input = moveCursor.action.ReadValue<Vector2>();

        if (input.magnitude < stickDeadzone)
            return;

        if (!usingGamepad)
        {
            usingGamepad = true;
            // Synchroniser la position virtuelle avec la vraie position
            if (Mouse.current != null)
                virtualPosition = Mouse.current.position.ReadValue();
        }

        // Calculer le delta avec une courbe d'accélération pour plus de précision
        float speedMultiplier = Mathf.Pow(input.magnitude, 1.5f); // Accélération progressive
        Vector2 delta = input.normalized * speedMultiplier * cursorSpeed * Time.unscaledDeltaTime;

        // Mettre à jour la position virtuelle (sans lag)
        virtualPosition += delta;
        virtualPosition.x = Mathf.Clamp(virtualPosition.x, 0, Screen.width);
        virtualPosition.y = Mathf.Clamp(virtualPosition.y, 0, Screen.height);

        // Déplacer le curseur système
        if (Mouse.current != null)
        {
            Mouse.current.WarpCursorPosition(virtualPosition);
            InputState.Change(Mouse.current.position, virtualPosition); // Forcer la mise à jour immédiate
        }
    }

    void HandleMouseDetection()
    {
        if (Mouse.current == null)
            return;

        Vector2 currentMousePos = Mouse.current.position.ReadValue();

        if (usingGamepad)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            if (mouseDelta.sqrMagnitude > 0.5f)
            {
                Vector2 input = moveCursor.action.ReadValue<Vector2>();
                if (input.magnitude < stickDeadzone)
                {
                    usingGamepad = false;
                    virtualPosition = currentMousePos;
                }
            }
        }
        else
        {
            // Garder la position virtuelle synchronisée avec la souris
            virtualPosition = currentMousePos;
        }

        lastMousePosition = currentMousePos;
    }

    void HandleGamepadClick()
    {
        if (!usingGamepad)
            return;

        if (click.action.WasPressedThisFrame())
        {
            SimulateMouseClick(true);
            Cursor.SetCursor(cursorClickTexture != null ? cursorClickTexture : cursorTexture, hotSpot, cursorMode);
        }

        if (click.action.WasReleasedThisFrame())
        {
            SimulateMouseClick(false);
            Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);   
        }
    }

    void SimulateMouseClick(bool pressed)
    {
        if (Mouse.current == null)
            return;

        using (StateEvent.From(Mouse.current, out var eventPtr))
        {
            Mouse.current.CopyState<MouseState>(out var state);
            state.WithButton(MouseButton.Left, pressed);
            InputState.Change(Mouse.current, state);
        }
    }
}