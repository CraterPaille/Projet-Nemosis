using UnityEngine;

public class BallGenerator : MonoBehaviour
{
    public float Health = 100f;
    public GameObject ballPrefab;
    public GameObject spawnPoint;
    public BallGenerator instance { get; private set; }
    private GameObject currentBall;

    // Start is called once before the first execution of Update after the MonoBehaviour is created


    void Update()
    {
        if(currentBall == null)
        {
            BallSpawned();
        }
    }
    void BallSpawned()
    {
        currentBall = Instantiate(ballPrefab, spawnPoint.transform.position, Quaternion.identity);

    }
    // Update is called once per frame
    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.CompareTag("Ball"))
        {
            Health -= col.gameObject.GetComponent<Ball>().Damage;
            Destroy(col.gameObject);

            if (Health <= 0)
            {
                Debug.Log("Game Over");
            }

        }

    }
}
