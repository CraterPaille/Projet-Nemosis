using UnityEngine;

public class LaserMovement : MonoBehaviour
{
    public float speed = 8f;
    public bool useRealtime = false;
    private Vector3 direction;

    void Start()
    {
        direction = transform.right;
        // debug logging removed pour réduire allocations/overhead en runtime
    }

    void Update()
    {
        float dt = useRealtime ? Time.unscaledDeltaTime : Time.deltaTime;
        transform.position += direction * speed * dt;
    }
}