using UnityEngine;

public class PlayerSoul : MonoBehaviour
{
    public float speed = 6f;
    private Rigidbody2D rb;
    private Vector2 input;
    public BoxCollider2D combatBox;

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
    }

    void Update()
    {
        if (!canMove)
        {
            input = Vector2.zero;
            return;
        }

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        if (justiceMode)
        {
            if (h != 0 && v != 0)
            {
                if (Mathf.Abs(h) > Mathf.Abs(v))
                    v = 0;
                else
                    h = 0;
            }
        }

        float magnitude = Mathf.Sqrt(h * h + v * v);
        if (magnitude > 0f)
        {
            input.x = h / magnitude;
            input.y = v / magnitude;
        }
        else
        {
            input = Vector2.zero;
        }
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

    void OnDisable()
    {
        input = Vector2.zero;
        boundsNeedUpdate = true;
    }
}