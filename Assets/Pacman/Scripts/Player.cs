using UnityEngine;
using System.Collections;

public class PacmanMovement : MonoBehaviour
{
    
    public float speed = 4f;
    public float gridSize = 1f;
    public float snapThreshold = 0.1f;
    public LayerMask wallLayer;
    public GameObject minautor;

    Vector2 currentDirection = Vector2.zero;
    Vector2 desiredDirection = Vector2.zero;

    Rigidbody2D rb;
    BoxCollider2D box;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        box = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        ReadInput();
    }

    void FixedUpdate()
    {
        TryTurn();
        Move();
    }

    void ReadInput()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow)) desiredDirection = Vector2.up;
        else if (Input.GetKeyDown(KeyCode.DownArrow)) desiredDirection = Vector2.down;
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) desiredDirection = Vector2.left;
        else if (Input.GetKeyDown(KeyCode.RightArrow)) desiredDirection = Vector2.right;
    }

    void TryTurn()
    {
        if (desiredDirection == currentDirection)
            return;

        // Prevent 180° turns (going backwards)
        bool isUTurn = Vector2.Dot(desiredDirection, currentDirection) < -0.9f;
        if (isUTurn)
            return;
        
        if (!IsAlignedForTurn(desiredDirection))
            return;

        if (CanMove(desiredDirection))
        {
            SnapToGrid();
            currentDirection = desiredDirection;
        }
    }

    void Move()
    {
        if (currentDirection != Vector2.zero && CanMove(currentDirection))
        {
            Vector2 newPos = rb.position + currentDirection * speed * Time.fixedDeltaTime;
            rb.MovePosition(newPos);
        }
    }

    bool CanMove(Vector2 dir)
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(wallLayer);
        filter.useTriggers = false;

        RaycastHit2D[] hits = new RaycastHit2D[1];

        int count = box.Cast(
            dir,
            filter,
            hits,
            0.05f
        );

        return count == 0;
    }

    bool IsAlignedForTurn(Vector2 dir)
    {
        Vector2 pos = rb.position;

        if (dir.x != 0)
        {
            float y = Mathf.Abs(pos.y % gridSize);
            return y < snapThreshold || y > gridSize - snapThreshold;
        }
        else
        {
            float x = Mathf.Abs(pos.x % gridSize);
            return x < snapThreshold || x > gridSize - snapThreshold;
        }
    }

    void SnapToGrid()
    {
        Vector2 pos = rb.position;
        pos.x = Mathf.Round(pos.x / gridSize) * gridSize;
        pos.y = Mathf.Round(pos.y / gridSize) * gridSize;
        rb.position = pos;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == minautor)
        {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject == minautor)
        {
            Destroy(gameObject);
        }
    }
}
