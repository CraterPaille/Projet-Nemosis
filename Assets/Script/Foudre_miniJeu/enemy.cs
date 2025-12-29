using UnityEngine;

public enum EnemyType
{
    Normal,
    Gold,   // plus de points quand tué
    Red,    // plus dangereux (malus plus fort si passe)
    Blue    // peu de points, peu de danger
}

public class Enemy : MonoBehaviour
{
    public float speed = 2f;
    [HideInInspector] public bool isFast = false;
    public EnemyType type = EnemyType.Normal;

    private Transform city;
    private bool isDead = false;
    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        if (isFast) speed *= 2f;

        GameObject cityGO = GameObject.FindGameObjectWithTag("City");
        if (cityGO != null) city = cityGO.transform;

        ApplyVisual();
    }

    public void RandomizeType()
    {
        float r = Random.value;

        if (r < 0.70f)      type = EnemyType.Normal; // 70%
        else if (r < 0.85f) type = EnemyType.Gold;   // 15%
        else if (r < 0.95f) type = EnemyType.Red;    // 10%
        else                type = EnemyType.Blue;   // 5%

        ApplyVisual();
    }

    void ApplyVisual()
    {
        if (sr == null) return;

        switch (type)
        {
            case EnemyType.Normal:
                sr.color = Color.white;
                break;
            case EnemyType.Gold:
                sr.color = new Color(1f, 0.85f, 0.2f); // doré
                break;
            case EnemyType.Red:
                sr.color = new Color(1f, 0.4f, 0.4f);  // rouge
                break;
            case EnemyType.Blue:
                sr.color = new Color(0.6f, 0.8f, 1f);  // bleu
                break;
        }
    }

    void Update()
    {
        if (isDead || city == null) return;

        transform.position = Vector3.MoveTowards(transform.position, city.position, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, city.position) < 0.2f)
        {
            isDead = true;
            ReachCity();
        }
    }

    public void Kill()
    {
        if (isDead) return;
        isDead = true;
        GameManagerZeus.Instance.EnemyKilled(transform.position, type);
        Destroy(gameObject);
    }

    public void ReachCity()
    {
        GameManagerZeus.Instance.EnemyReachedCity(type);
        Destroy(gameObject);
    }
}
