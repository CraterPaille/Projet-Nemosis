using UnityEngine;
using System.Collections.Generic;

public class GhostMovement : MonoBehaviour
{
    public float speed = 5f;
    public float gridSize = 1f;
    public float snapThreshold = 0.1f;
    public LayerMask wallLayer;
    public Transform target; // Pacman

    Vector2 currentDirection;
    Rigidbody2D rb;
    BoxCollider2D box;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        box = GetComponent<BoxCollider2D>();

        // direction de départ
        currentDirection = Vector2.left;
    }

    void FixedUpdate()
    {
        TryChooseDirection();
        Move();
    }

    void Move()
    {
        if (currentDirection == Vector2.zero)
            return;

        if (CanMove(currentDirection))
        {
            rb.MovePosition(rb.position + currentDirection * speed * Time.fixedDeltaTime);
        }
        else
        {
            currentDirection = Vector2.zero;
        }
    }

    void TryChooseDirection()
    {
        if (!IsAlignedForTurn())
            return;

        List<Vector2> possibleDirs = GetAvailableDirections();
        if (possibleDirs.Count == 0)
            return;

        // Évite le demi-tour sauf si c'est la seule option
        Vector2 oppositeDir = -currentDirection;
        if (possibleDirs.Count > 1 && possibleDirs.Contains(oppositeDir))
            possibleDirs.Remove(oppositeDir);

        Vector2 newDirection = ChooseDirection(possibleDirs);

        if (newDirection != currentDirection)
        {
            SnapToGrid();
            currentDirection = newDirection;
        }
    }

    Vector2 ChooseDirection(List<Vector2> dirs)
    {
        if (target == null || dirs.Count == 0)
            return dirs[Random.Range(0, dirs.Count)];

        // Simple et efficace : choisi la direction qui rapproche le plus de Pacman
        Vector2 currentPos = rb.position;
        Vector2 targetPos = target.position;

        Vector2 bestDir = dirs[0];
        float shortestDist = float.MaxValue;

        foreach (Vector2 dir in dirs)
        {
            // Calcule où on sera après avoir pris cette direction
            Vector2 nextPos = currentPos + dir * gridSize;
            
            // Distance entre la prochaine position et Pacman
            float dist = Vector2.Distance(nextPos, targetPos);
            
            if (dist < shortestDist)
            {
                shortestDist = dist;
                bestDir = dir;
            }
        }

        return bestDir;
    }

    bool IsAlignedForTurn()
    {
        Vector2 pos = rb.position;
        float x = Mathf.Abs(pos.x % gridSize);
        float y = Mathf.Abs(pos.y % gridSize);

        return (x < snapThreshold || x > gridSize - snapThreshold) &&
               (y < snapThreshold || y > gridSize - snapThreshold);
    }

    List<Vector2> GetAvailableDirections()
    {
        List<Vector2> dirs = new List<Vector2>();

        if (CanMove(Vector2.up)) dirs.Add(Vector2.up);
        if (CanMove(Vector2.down)) dirs.Add(Vector2.down);
        if (CanMove(Vector2.left)) dirs.Add(Vector2.left);
        if (CanMove(Vector2.right)) dirs.Add(Vector2.right);

        return dirs;
    }

    bool CanMove(Vector2 dir)
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(wallLayer);
        filter.useTriggers = false;

        RaycastHit2D[] hits = new RaycastHit2D[1];
        return box.Cast(dir, filter, hits, 0.05f) == 0;
    }

    void SnapToGrid()
    {
        Vector2 pos = rb.position;
        pos.x = Mathf.Round(pos.x / gridSize) * gridSize;
        pos.y = Mathf.Round(pos.y / gridSize) * gridSize;
        rb.position = pos;
    }
}
