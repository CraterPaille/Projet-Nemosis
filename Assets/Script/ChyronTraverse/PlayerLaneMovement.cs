using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLaneMovement : MonoBehaviour
{
    public float laneOffset = 2f;
    public float moveSpeed = 10f;

    public int[] laneIndices = new int[] { -1, 0, 1 }; // tes lanes
    private int laneArrayIndex = 1; // commence au centre
    private int laneIndex => laneIndices[laneArrayIndex]; // valeur réelle

    private Vector3 targetPosition;

    public SpriteRenderer spriteRenderer;
    public Sprite idleSprite;
    public Sprite turnLeftSprite;
    public Sprite turnRightSprite;
    public float turnSpriteDuration = 0.25f;

    private float spriteTimer = 0f;

    private PlayerControls keyboardControls;
    private PlayerControls gamepadControls;

    private void Awake()
    {
        keyboardControls = InputManager.Instance.keyboardControls;
        gamepadControls = InputManager.Instance.gamepadControls;

        // Clavier
        keyboardControls.Gameplay.MoveLeft.performed += ctx => MoveLeft();
        keyboardControls.Gameplay.MoveRight.performed += ctx => MoveRight();

        // Manette
        gamepadControls.Gameplay.MoveLeft.performed += ctx => MoveLeft();
        gamepadControls.Gameplay.MoveRight.performed += ctx => MoveRight();
    }

    private void OnEnable()
    {
        keyboardControls.Gameplay.Enable();
        gamepadControls.Gameplay.Enable();
    }

    private void OnDisable()
    {
        keyboardControls.Gameplay.Disable();
        gamepadControls.Gameplay.Disable();
    }

    private void Start()
    {
        targetPosition = transform.position;
        laneArrayIndex = laneIndices.Length / 2; // commence au centre
    }

    private void MoveLeft()
    {
        laneArrayIndex = Mathf.Max(laneArrayIndex - 1, 0); // ne pas sortir du tableau
        spriteRenderer.sprite = turnLeftSprite;
        spriteTimer = turnSpriteDuration;
    }

    private void MoveRight()
    {
        laneArrayIndex = Mathf.Min(laneArrayIndex + 1, laneIndices.Length - 1);
        spriteRenderer.sprite = turnRightSprite;
        spriteTimer = turnSpriteDuration;
    }

    private void Update()
    {
        // Timer pour revenir au sprite normal
        if (spriteTimer > 0)
        {
            spriteTimer -= Time.deltaTime;
            if (spriteTimer <= 0)
                spriteRenderer.sprite = idleSprite;
        }

        // Smooth movement latéral
        targetPosition = new Vector3(laneIndex * laneOffset, transform.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);
    }
}
