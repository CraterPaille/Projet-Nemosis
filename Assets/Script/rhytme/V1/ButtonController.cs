using UnityEngine;
using UnityEngine.InputSystem;

public class ButtonController : MonoBehaviour
{
    public Sprite buttonNormal;
    public Sprite buttonPressed;

    [Tooltip("0 à 3")]
    public int lane; // correspond à la lane dans InputManager

    [Header("Animation")]
    public float punchScale = 1.15f;
    public float punchDuration = 0.1f;
    public Color pressedColor = Color.white;     // teinte quand pressé
    public Color normalColor = Color.white;      // couleur par défaut

    private SpriteRenderer spriteRenderer;
    private PlayerControls keyboardControls;
    private PlayerControls gamepadControls;

    private Vector3 _baseScale;
    private float _punchTimer = 0f;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        _baseScale = transform.localScale;

        if (InputManager.Instance == null)
        {
            Debug.LogError("ButtonController : InputManager non trouvé !");
            return;
        }

        keyboardControls = InputManager.Instance.keyboardControls;
        gamepadControls = InputManager.Instance.gamepadControls;
    }

    private void OnEnable()
    {
        if (keyboardControls == null || gamepadControls == null) return;

        switch (lane)
        {
            case 0:
                keyboardControls.Rhytm.Lane0.started += Press;
                keyboardControls.Rhytm.Lane0.canceled += Release;
                gamepadControls.Rhytm.Lane0.started += Press;
                gamepadControls.Rhytm.Lane0.canceled += Release;
                break;
            case 1:
                keyboardControls.Rhytm.Lane1.started += Press;
                keyboardControls.Rhytm.Lane1.canceled += Release;
                gamepadControls.Rhytm.Lane1.started += Press;
                gamepadControls.Rhytm.Lane1.canceled += Release;
                break;
            case 2:
                keyboardControls.Rhytm.Lane2.started += Press;
                keyboardControls.Rhytm.Lane2.canceled += Release;
                gamepadControls.Rhytm.Lane2.started += Press;
                gamepadControls.Rhytm.Lane2.canceled += Release;
                break;
            case 3:
                keyboardControls.Rhytm.Lane3.started += Press;
                keyboardControls.Rhytm.Lane3.canceled += Release;
                gamepadControls.Rhytm.Lane3.started += Press;
                gamepadControls.Rhytm.Lane3.canceled += Release;
                break;
        }
    }

    private void OnDisable()
    {
        if (keyboardControls == null || gamepadControls == null) return;

        switch (lane)
        {
            case 0:
                keyboardControls.Rhytm.Lane0.started -= Press;
                keyboardControls.Rhytm.Lane0.canceled -= Release;
                gamepadControls.Rhytm.Lane0.started -= Press;
                gamepadControls.Rhytm.Lane0.canceled -= Release;
                break;
            case 1:
                keyboardControls.Rhytm.Lane1.started -= Press;
                keyboardControls.Rhytm.Lane1.canceled -= Release;
                gamepadControls.Rhytm.Lane1.started -= Press;
                gamepadControls.Rhytm.Lane1.canceled -= Release;
                break;
            case 2:
                keyboardControls.Rhytm.Lane2.started -= Press;
                keyboardControls.Rhytm.Lane2.canceled -= Release;
                gamepadControls.Rhytm.Lane2.started -= Press;
                gamepadControls.Rhytm.Lane2.canceled -= Release;
                break;
            case 3:
                keyboardControls.Rhytm.Lane3.started -= Press;
                keyboardControls.Rhytm.Lane3.canceled -= Release;
                gamepadControls.Rhytm.Lane3.started -= Press;
                gamepadControls.Rhytm.Lane3.canceled -= Release;
                break;
        }
    }

    private void Update()
    {
        if (_punchTimer > 0f)
        {
            _punchTimer -= Time.deltaTime;
            float t = 1f - Mathf.Clamp01(_punchTimer / punchDuration);
            // petit overshoot
            float s = Mathf.Lerp(punchScale, 1f, t);
            transform.localScale = _baseScale * s;

            if (_punchTimer <= 0f)
                transform.localScale = _baseScale;
        }
    }

    private void Press(InputAction.CallbackContext ctx)
    {
        spriteRenderer.sprite = buttonPressed;
        spriteRenderer.color = pressedColor;
        _punchTimer = punchDuration;
    }

    private void Release(InputAction.CallbackContext ctx)
    {
        spriteRenderer.sprite = buttonNormal;
        spriteRenderer.color = normalColor;
    }

    // Feedback déclenché depuis InputManager (optionnel)
    public void PressFeedback()
    {
        Press(default);
        CancelInvoke(nameof(ResetSprite));
        Invoke(nameof(ResetSprite), 0.1f);
    }

    private void ResetSprite()
    {
        Release(default);
    }
}