using UnityEngine;

public class Village : MonoBehaviour
{
    public float Health = 140;
    void Start()
    {
        
    }
    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.CompareTag("Ball"))
        {
            Health -= col.gameObject.GetComponent<Ball>().Damage;
            Destroy(col.gameObject);
            if (Health <= 0)
            {
                Destroy(gameObject);
                Debug.Log("Game Over");
            }

        }
    }
}