using UnityEngine;
using UnityEngine.InputSystem;

public class ButtonController : MonoBehaviour
{
    public Sprite buttonNormal;
    public Sprite buttonPressed;

    [Tooltip("0 à 3")]
    public int lane; // correspond à la lane dans InputManager

    // public bool useGamepad = false; // plus besoin

    private SpriteRenderer spriteRenderer;
    private PlayerControls keyboardControls;
    private PlayerControls gamepadControls;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

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

    private void Press(InputAction.CallbackContext ctx)
    {
        spriteRenderer.sprite = buttonPressed;
    }

    private void Release(InputAction.CallbackContext ctx)
    {
        spriteRenderer.sprite = buttonNormal;
    }

    // Feedback déclenché depuis InputManager
    public void PressFeedback()
    {
        spriteRenderer.sprite = buttonPressed;
        CancelInvoke(nameof(ResetSprite));
        Invoke(nameof(ResetSprite), 0.1f);
    }

    private void ResetSprite()
    {
        spriteRenderer.sprite = buttonNormal;
    }
}