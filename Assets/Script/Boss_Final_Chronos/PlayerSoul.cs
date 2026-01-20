using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSoul : MonoBehaviour
{
    public float speed = 6f;
    private Rigidbody2D rb;
    private Vector2 input;
    public BoxCollider2D combatBox;

    [Header("Input System")]
    [SerializeField] private InputActionAsset inputActions; // Assigne ton .inputactions ici

    private InputAction moveAction;
    private InputAction leftShieldAction;
    private InputAction rightShieldAction;

    // Layout clavier (modifiable dans l'inspecteur ou via un menu d'options)
    [Header("Options clavier")]
    public bool isAzerty = true; // true = ZQSD, false = WASD

    // Cache des bounds pour éviter les recalculs
    private Bounds cachedBounds;
    private bool boundsNeedUpdate = true;

    // Mode Justice
    private bool justiceMode = false;
    private bool canMove = true;

    // Collider du joueur pour clamp précis
    private BoxCollider2D playerBox;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerBox = GetComponent<BoxCollider2D>();

        // Récupère les actions par leur nom (doivent exister dans l'asset)
        moveAction = inputActions.FindAction("Move");
        leftShieldAction = inputActions.FindAction("LeftShield");
        rightShieldAction = inputActions.FindAction("RightShield");
    }

    void OnEnable()
    {
        moveAction?.Enable();
        leftShieldAction?.Enable();
        rightShieldAction?.Enable();
    }

    void OnDisable()
    {
        moveAction?.Disable();
        leftShieldAction?.Disable();
        rightShieldAction?.Disable();

        input = Vector2.zero;
        boundsNeedUpdate = true;
    }

    void Update()
    {
        if (!canMove)
        {
            input = Vector2.zero;
            return;
        }

        Vector2 moveInput = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;

        if (justiceMode)
        {
            if (moveInput.x != 0 && moveInput.y != 0)
            {
                if (Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y))
                    moveInput.y = 0;
                else
                    moveInput.x = 0;
            }
        }

        input = moveInput.sqrMagnitude > 0f ? moveInput.normalized : Vector2.zero;

        // Pour les boucliers
        Vector2 leftShieldInput = leftShieldAction != null ? leftShieldAction.ReadValue<Vector2>() : Vector2.zero;
        Vector2 rightShieldInput = rightShieldAction != null ? rightShieldAction.ReadValue<Vector2>() : Vector2.zero;
        // Utilise ces valeurs pour tes boucliers
    }

    void FixedUpdate()
    {
        if (!canMove) return;

        Vector2 targetPos;
        targetPos.x = rb.position.x + input.x * speed * Time.fixedDeltaTime;
        targetPos.y = rb.position.y + input.y * speed * Time.fixedDeltaTime;

        if (combatBox != null)
        {
            if (boundsNeedUpdate)
            {
                cachedBounds = combatBox.bounds;
                boundsNeedUpdate = false;
            }

            // Clamp le centre du joueur dans les bounds de la box
            targetPos.x = Mathf.Clamp(targetPos.x, cachedBounds.min.x, cachedBounds.max.x);
            targetPos.y = Mathf.Clamp(targetPos.y, cachedBounds.min.y, cachedBounds.max.y);
        }

        rb.MovePosition(targetPos);
    }

    public void EnterJusticeMode(BoxCollider2D box, Vector2 centerPosition)
    {
        justiceMode = true;
        canMove = false;
        combatBox = box;
        boundsNeedUpdate = true;

        rb.position = centerPosition;
        rb.linearVelocity = Vector2.zero;

        Vector3 pos = transform.position;
        pos.x = centerPosition.x;
        pos.y = centerPosition.y;
        transform.position = pos;
    }

    public void ExitJusticeMode()
    {
        justiceMode = false;
        canMove = true;
    }

    public static PlayerSoul Spawn(Vector3 position, Quaternion rotation)
    {
        var soulObj = ObjectPooler.Instance.SpawnFromPool("PlayerSoul", position, rotation);
        return soulObj.GetComponent<PlayerSoul>();
    }

    public void Despawn()
    {
        gameObject.SetActive(false);
    }

    public void SetMovementEnabled(bool enabled)
    {
        canMove = enabled;
    }
}