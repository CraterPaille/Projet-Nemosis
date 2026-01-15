using UnityEngine;

public class PlayerSoul : MonoBehaviour
{
    public float speed = 6f;
    private Rigidbody2D rb;
    private Vector2 input;
    public BoxCollider2D combatBox;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        input = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;
    }

    void FixedUpdate()
    {
        Vector2 targetPos = rb.position + input * speed * Time.fixedDeltaTime;
        if (combatBox != null)
        {
            Bounds b = combatBox.bounds;
            targetPos.x = Mathf.Clamp(targetPos.x, b.min.x, b.max.x);
            targetPos.y = Mathf.Clamp(targetPos.y, b.min.y, b.max.y);
        }
        rb.MovePosition(targetPos);
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
}
