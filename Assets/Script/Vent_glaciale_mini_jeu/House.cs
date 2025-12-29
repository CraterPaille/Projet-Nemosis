using UnityEngine;
using UnityEngine.InputSystem;

public class House : MonoBehaviour
{
    public bool isOn = true;
    private SpriteRenderer sr;

    public Sprite onSprite;
    public Sprite offSprite;

    // Nouvelle Input System
    private InputAction clickAction;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        UpdateVisual();

        // Initialise l'action souris
        clickAction = new InputAction(type: InputActionType.Button, binding: "<Mouse>/leftButton");
        clickAction.performed += ctx => OnClick(); // callback sur clic
    }

    void OnEnable()
    {
        clickAction?.Enable();
    }

    void OnDisable()
    {
        clickAction?.Disable();
    }

    public void SetState(bool on)
    {
        isOn = on;
        UpdateVisual();
    }

    void UpdateVisual()
    {
        if (sr != null)
            sr.sprite = isOn ? onSprite : offSprite;
    }

    private void OnClick()
    {
        if (!NuitGlacialeGameManager.Instance.isRunning)
            return;

        // On vérifie si le clic touche cette maison
        Vector3 mouseWorldPos = Mouse.current.position.ReadValue();
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mouseWorldPos);
        Vector2 clickPos2D = new Vector2(worldPos.x, worldPos.y);

        RaycastHit2D hit = Physics2D.Raycast(clickPos2D, Vector2.zero);
        if (hit.collider != null && hit.collider.gameObject == this.gameObject)
        {
            if (!isOn)
                SetState(true);
        }
    }
}
