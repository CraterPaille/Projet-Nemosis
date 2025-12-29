using System.Collections.Generic;
using UnityEngine;

public class VerticalRepeatingWorld : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject waterTilePrefab;
    public GameObject obstaclePrefab;   // obstacle standard
    public GameObject barrelPrefab;     // tonneau
    public GameObject repairPrefab;     // bonus réparation
    public GameObject shieldPrefab;     // bonus bouclier
    public GameObject coinPrefab;       // pièce collectible

    [Header("References")]
    public Transform player;
    public Camera mainCamera;
    public chyronGameManager gameManager;

    [Header("Grid")]
    public int extraRows = 2;

    [Header("Lanes")]
    public float laneOffset = 2f;
    public int[] laneIndices = new int[] { -1, 0, 1 };

    [Header("Obstacles")]
    [Range(0f, 1f)] public float spawnChance = 0.12f;
    [Range(0f, 1f)] public float bonusSpawnChance = 0.05f;
    public int initialPoolSize = 30;
    public int maxObstaclesPerLine = 2;
    public int minEmptyLines = 3;

    [Header("Scrolling")]
    public float minScrollSpeed = 3f;
    public float maxScrollSpeed = 5f;
    public float scrollLerpSpeed = 2f;

    private int tilesY;
    private GameObject[] tiles;   // une seule colonne
    private float[] rowYs;

    private float tileWidth;
    private float tileHeight;

    private Queue<GameObject> obstaclePool = new Queue<GameObject>();
    private List<GameObject> activeObstacles = new List<GameObject>();
    private int linesSinceLastObstacle = 0;

    private float camHeight;
    private float camWidth;

    [HideInInspector] public float targetScrollSpeed;

    void Awake()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        var sr = waterTilePrefab.GetComponent<SpriteRenderer>();
        tileWidth = (sr != null) ? sr.bounds.size.x : 1f;
        tileHeight = (sr != null) ? sr.bounds.size.y : 1f;
    }

    void Start()
    {
        camHeight = mainCamera.orthographicSize * 2f;
        camWidth = camHeight * mainCamera.aspect;

        tilesY = Mathf.CeilToInt(camHeight / tileHeight) + 2 + extraRows;

        tiles = new GameObject[tilesY];
        rowYs = new float[tilesY];

        targetScrollSpeed = minScrollSpeed;
        gameManager.scrollSpeed = minScrollSpeed;

        InitObstaclePool();
        InitWaterColumn();
    }

    void Update()
    {
        if (gameManager.isGameOver) return;

        float dt = Time.deltaTime;

        targetScrollSpeed += 0.5f * dt;
        targetScrollSpeed = Mathf.Clamp(targetScrollSpeed, minScrollSpeed, maxScrollSpeed);
        gameManager.scrollSpeed = Mathf.Lerp(gameManager.scrollSpeed, targetScrollSpeed, dt * scrollLerpSpeed);

        ScrollTiles(dt);
        ScrollObstacles(dt);
        RecycleRows();
    }

    // -----------------------------
    // INIT
    // -----------------------------
    void InitObstaclePool()
    {
        // Obstacles standard + tonneaux + bonus
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject o = Instantiate(obstaclePrefab);
            o.SetActive(false);
            o.GetComponent<ObstacleCollision>().type = ObstacleType.Standard;
            obstaclePool.Enqueue(o);

            GameObject b = Instantiate(barrelPrefab);
            b.SetActive(false);
            b.GetComponent<ObstacleCollision>().type = ObstacleType.Barrel;
            obstaclePool.Enqueue(b);

            GameObject r = Instantiate(repairPrefab);
            r.SetActive(false);
            r.GetComponent<ObstacleCollision>().type = ObstacleType.Repair;
            obstaclePool.Enqueue(r);

            GameObject s = Instantiate(shieldPrefab);
            s.SetActive(false);
            s.GetComponent<ObstacleCollision>().type = ObstacleType.Shield;
            obstaclePool.Enqueue(s);

            GameObject c = Instantiate(coinPrefab);
            c.SetActive(false);
            c.GetComponent<ObstacleCollision>().type = ObstacleType.Coin;
            obstaclePool.Enqueue(c);
        }
    }

    // une seule "colonne" de tiles, centrée en X
    void InitWaterColumn()
    {
        Vector3 camPos = mainCamera.transform.position;
        float firstY = camPos.y - (tilesY / 2f - 0.5f) * tileHeight;
        float centerX = camPos.x;

        for (int r = 0; r < tilesY; r++)
        {
            rowYs[r] = firstY + r * tileHeight;
            tiles[r] = Instantiate(waterTilePrefab,
                                   new Vector3(centerX, rowYs[r], 0f),
                                   Quaternion.identity,
                                   transform);
        }

        for (int r = 0; r < tilesY; r++)
            SpawnObstaclesOnRow(rowYs[r]);
    }

    // -----------------------------
    // SCROLLING
    // -----------------------------
    void ScrollTiles(float dt)
    {
        float dy = -gameManager.scrollSpeed * dt;

        for (int r = 0; r < tilesY; r++)
        {
            rowYs[r] += dy;
            Vector3 p = tiles[r].transform.position;
            p.y = rowYs[r];
            tiles[r].transform.position = p;
        }
    }

    void ScrollObstacles(float dt)
    {
        float dy = -gameManager.scrollSpeed * dt;

        for (int i = activeObstacles.Count - 1; i >= 0; i--)
        {
            var o = activeObstacles[i];
            o.transform.position += new Vector3(0, dy, 0);

            if (o.transform.position.y < mainCamera.transform.position.y - camHeight / 2 - tileHeight / 2)
            {
                o.SetActive(false);
                obstaclePool.Enqueue(o);
                activeObstacles.RemoveAt(i);
            }
        }
    }

    // -----------------------------
    // RECYCLAGE DES LIGNES (Y UNIQUEMENT)
    // -----------------------------
    void RecycleRows()
    {
        float camBottom = mainCamera.transform.position.y - camHeight / 2 - tileHeight / 2;

        for (int r = 0; r < tilesY; r++)
        {
            if (rowYs[r] < camBottom)
            {
                float oldY = rowYs[r];
                rowYs[r] += tilesY * tileHeight;
                float newY = rowYs[r];

                Vector3 p = tiles[r].transform.position;
                p.y = newY;
                tiles[r].transform.position = p;

                ReclaimObstaclesForLine(oldY);
                SpawnObstaclesOnRow(newY);
            }
        }
    }

    void ReclaimObstaclesForLine(float oldY)
    {
        for (int i = activeObstacles.Count - 1; i >= 0; i--)
        {
            var o = activeObstacles[i];
            if (Mathf.Abs(o.transform.position.y - oldY) < tileHeight * 0.7f)
            {
                o.SetActive(false);
                obstaclePool.Enqueue(o);
                activeObstacles.RemoveAt(i);
            }
        }
    }

    void SpawnObstaclesOnRow(float rowY)
    {
        float safeDistance = tileHeight * 3f;
        if (Mathf.Abs(rowY - player.position.y) < safeDistance || linesSinceLastObstacle < minEmptyLines)
        {
            linesSinceLastObstacle++;
            return;
        }

        int maxObjectsThisLine = Mathf.Min(maxObstaclesPerLine, laneIndices.Length - 1);
        int obstaclesSpawned = 0;

        // Mélange des lanes
        List<int> availableLanes = new List<int>(laneIndices);
        for (int i = 0; i < availableLanes.Count; i++)
        {
            int rnd = Random.Range(i, availableLanes.Count);
            (availableLanes[i], availableLanes[rnd]) = (availableLanes[rnd], availableLanes[i]);
        }

        // 20% de chance ligne bonus, 15% de chance ligne de pièces
        bool spawnBonusLine = Random.value < 0.20f;
        bool spawnCoinLine  = !spawnBonusLine && Random.value < 0.15f;

        foreach (int lane in availableLanes)
        {
            if (obstaclesSpawned >= maxObjectsThisLine || obstaclePool.Count == 0) break;

            GameObject prefabChoice;

            if (spawnCoinLine)
            {
                prefabChoice = coinPrefab;
            }
            else if (spawnBonusLine)
            {
                prefabChoice = (Random.value < 0.5f) ? repairPrefab : shieldPrefab;
            }
            else
            {
                prefabChoice = (Random.value < 0.5f) ? barrelPrefab : obstaclePrefab;
            }

            // récupérer bon type dans le pool
            GameObject obj = null;
            foreach (var o in obstaclePool)
            {
                var type = o.GetComponent<ObstacleCollision>().type;

                if (spawnCoinLine)
                {
                    if (type == ObstacleType.Coin)
                    {
                        obj = o; break;
                    }
                }
                else if (spawnBonusLine)
                {
                    if ((prefabChoice == repairPrefab && type == ObstacleType.Repair) ||
                        (prefabChoice == shieldPrefab && type == ObstacleType.Shield))
                    {
                        obj = o; break;
                    }
                }
                else
                {
                    if ((prefabChoice == obstaclePrefab && type == ObstacleType.Standard) ||
                        (prefabChoice == barrelPrefab && type == ObstacleType.Barrel))
                    {
                        obj = o; break;
                    }
                }
            }

            if (obj != null)
            {
                // retirer du pool
                var newQueue = new Queue<GameObject>();
                while (obstaclePool.Count > 0)
                {
                    var q = obstaclePool.Dequeue();
                    if (q != obj) newQueue.Enqueue(q);
                }
                obstaclePool = newQueue;
            }
            else
            {
                obj = Instantiate(prefabChoice);
                var typeScript = obj.GetComponent<ObstacleCollision>();

                if (prefabChoice == obstaclePrefab)      typeScript.type = ObstacleType.Standard;
                else if (prefabChoice == barrelPrefab)   typeScript.type = ObstacleType.Barrel;
                else if (prefabChoice == repairPrefab)   typeScript.type = ObstacleType.Repair;
                else if (prefabChoice == shieldPrefab)   typeScript.type = ObstacleType.Shield;
                else if (prefabChoice == coinPrefab)     typeScript.type = ObstacleType.Coin;
            }

            obj.transform.position = new Vector3(lane * laneOffset, rowY, 0f);
            obj.SetActive(true);
            activeObstacles.Add(obj);

            obstaclesSpawned++;
        }

        linesSinceLastObstacle = (obstaclesSpawned > 0) ? 0 : linesSinceLastObstacle + 1;
    }

    public void HitObstacle()
    {
        targetScrollSpeed = minScrollSpeed;
    }
}