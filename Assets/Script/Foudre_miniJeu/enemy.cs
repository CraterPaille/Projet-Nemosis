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
    [Header("Movement")]
    public float speed = 2f;
    [HideInInspector] public bool isFast = false;
    public EnemyType type = EnemyType.Normal;

    [Header("Animation")]
    public Sprite[] runSprites;         // frames du run (assigne les sprites découpés du sprite sheet)
    public float runFrameRate = 10f;    // fps de l'animation
    [Tooltip("Si vrai, on utilise la sprite actuelle comme 'idle' lorsque l'ennemi ne bouge pas")]
    public bool useInitialSpriteAsIdle = true;

    private Transform city;
    private bool isDead = false;
    private SpriteRenderer sr;

    // animation runtime
    private int runFrameIndex = 0;
    private float runFrameTimer = 0f;
    private Sprite idleSprite;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();

        // sauvegarde de la sprite initiale comme 'idle' si demandé
        if (sr != null && useInitialSpriteAsIdle)
            idleSprite = sr.sprite;
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

        // Mouvement
        Vector3 prevPos = transform.position;
        transform.position = Vector3.MoveTowards(transform.position, city.position, speed * Time.deltaTime);
        Vector3 movement = transform.position - prevPos;

        // Détection du fait que l'ennemi avance (se base sur le mouvement réel)
        bool isMoving = movement.sqrMagnitude > 0.0001f;

        // Orientation (flip selon la direction X)
        if (isMoving)
        {
            if (movement.x < 0) sr.flipX = true;
            else if (movement.x > 0) sr.flipX = false;
        }
        else
        {
            // si on n'a pas de mouvement X (ex: vertical), on peut orienter selon la cible
            float dirX = city.position.x - transform.position.x;
            if (Mathf.Abs(dirX) > 0.01f)
                sr.flipX = dirX < 0;
        }

        // Animation cadre par cadre simple
        if (runSprites != null && runSprites.Length > 0 && isMoving)
        {
            // avancer le timer
            runFrameTimer += Time.deltaTime;
            float frameDuration = 1f / Mathf.Max(0.0001f, runFrameRate);
            if (runFrameTimer >= frameDuration)
            {
                runFrameTimer -= frameDuration;
                runFrameIndex = (runFrameIndex + 1) % runSprites.Length;
                sr.sprite = runSprites[runFrameIndex];
            }
        }
        else
        {
            // When not moving, show idle sprite (either saved initial sprite or first run frame)
            if (idleSprite != null)
                sr.sprite = idleSprite;
            else if (runSprites != null && runSprites.Length > 0)
                sr.sprite = runSprites[0];
        }

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
