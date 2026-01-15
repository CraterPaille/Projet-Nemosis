using UnityEngine;
using System.Collections;

public class ChronosAttackController : MonoBehaviour
{
    public GameObject gasterBlasterPrefab; // Assigne ce prefab dans l'inspecteur
    public Transform arena; // Assigne la transform de l'arène
    public float spawnRadius = 6f; // Rayon autour de l'arène pour spawner les GasterBlasters
    public GameObject jewelPrefab;
    public Canvas choiceCanvas; // Canvas avec les boutons
    public GameObject bonePrefab; // à assigner dans l'inspector
    public UnityEngine.UI.Button attackButton;
    public UnityEngine.UI.Button healButton;

    void OnEnable()
    {
        StartCoroutine(AttackLoop());
    }

    IEnumerator AttackLoop()
    {
        // Attendre que le singleton soit prêt
        while (ChronosGameManager.Instance == null)
            yield return null;

        int patternCount = 0;
        while (ChronosGameManager.Instance.playerHP > 0 && ChronosGameManager.Instance.bossCurrentHearts > 0)
        {
            if (patternCount > 0 && patternCount % 3 == 0)
                yield return StartCoroutine(SpawnJewelAndWaitChoice());

            int phase = ChronosGameManager.Instance.BossPhase;
            int pattern = 0;

            // Sélectionne les patterns selon la phase
            if (phase == 1)
            {
                // Phase 1 : attaques simples
                pattern = Random.Range(0, 2); // 0 ou 1
            }
            else if (phase == 2)
            {
                // Phase 2 : attaques plus dures
                pattern = Random.Range(0, 3); // 0, 1, 2
            }
            else if (phase >= 3 && phase <= 6)
            {
                // Phases 3 à 6 : tout débloqué, patterns spéciaux, vitesse accrue
                pattern = Random.Range(0, 5); // 0 à 4
            }

            switch (pattern)
            {
                case 0: yield return StartCoroutine(PatternFourCorners()); break;
                case 1: yield return StartCoroutine(PatternCircleAroundPlayer()); break;
                case 2: yield return StartCoroutine(PatternRandomSingleBlaster()); break;
                case 3: yield return StartCoroutine(PatternBoneRain(phase >= 5 ? 35 : 20, phase >= 5 ? 2f : 3f)); break; // plus d'os, plus rapide
                case 4: yield return StartCoroutine(PatternBoneWallWithGap(
                    arena.position.y + arena.GetComponent<BoxCollider2D>().size.y / 2f, 
                    phase >= 4 ? 18 : 12, 
                    phase >= 4 ? 1.2f : 2f)); break; // mur plus dense, sortie plus petite
            }

            patternCount++;
            yield return new WaitForSeconds(phase >= 4 ? 1.2f : 2f); // moins de temps de pause aux phases avancées
        }
    }

    private Rect GetCameraRect()
    {
        Camera cam = Camera.main;
        float camHeight = 2f * cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;
        Vector2 camCenter = cam.transform.position;
        return new Rect(
            camCenter.x - camWidth / 2f,
            camCenter.y - camHeight / 2f,
            camWidth,
            camHeight
        );
    }

    private bool IsInsideCamera(Vector3 pos)
    {
        Rect camRect = GetCameraRect();
        return camRect.Contains(new Vector2(pos.x, pos.y));
    }

    private bool IsOutsideBox(Vector3 pos, BoxCollider2D box)
    {
        Vector2 boxCenter = box.transform.position;
        Vector2 boxSize = box.size * box.transform.lossyScale;
        return pos.x < boxCenter.x - boxSize.x / 2f ||
               pos.x > boxCenter.x + boxSize.x / 2f ||
               pos.y < boxCenter.y - boxSize.y / 2f ||
               pos.y > boxCenter.y + boxSize.y / 2f;
    }

    IEnumerator PatternFourCorners()
    {
        BoxCollider2D box = arena.GetComponent<BoxCollider2D>();
        Rect camRect = GetCameraRect();

        for (int i = 0; i < 4; i++)
        {
            float angle = i * Mathf.PI / 2;
            Vector3 spawnPosition;
            float currentRadius = 6f;
            int attempts = 0, maxAttempts = 20;

            do
            {
                spawnPosition = new Vector3(
                    Mathf.Cos(angle) * currentRadius,
                    Mathf.Sin(angle) * currentRadius,
                    0
                );
                attempts++;
                currentRadius += 0.5f;
            }
            while ((!IsInsideCamera(spawnPosition) || !IsOutsideBox(spawnPosition, box)) && attempts < maxAttempts);

            if (IsInsideCamera(spawnPosition) && IsOutsideBox(spawnPosition, box))
                Instantiate(gasterBlasterPrefab, spawnPosition, GetRotationTowardsPlayer(spawnPosition));
            yield return new WaitForSeconds(0.3f);
        }
    }

    IEnumerator PatternCircleAroundPlayer()
    {
        int blasterCount = 8;
        float baseRadius = 8f;
        Transform player = GameObject.FindGameObjectWithTag("Player").transform;
        BoxCollider2D box = arena.GetComponent<BoxCollider2D>();

        for (int i = 0; i < blasterCount; i++)
        {
            float angle = i * (2f * Mathf.PI / blasterCount);
            Vector3 spawnPosition;
            float currentRadius = baseRadius;
            int attempts = 0, maxAttempts = 20;

            do
            {
                spawnPosition = player.position + new Vector3(
                    Mathf.Cos(angle) * currentRadius,
                    Mathf.Sin(angle) * currentRadius,
                    0
                );
                attempts++;
                currentRadius -= 0.5f;
            }
            while ((!IsInsideCamera(spawnPosition) || !IsOutsideBox(spawnPosition, box)) && attempts < maxAttempts);

            if (IsInsideCamera(spawnPosition) && IsOutsideBox(spawnPosition, box))
                Instantiate(gasterBlasterPrefab, spawnPosition, GetRotationTowardsPlayer(spawnPosition));
            yield return new WaitForSeconds(0.2f);
        }
    }

    IEnumerator PatternRandomSingleBlaster()
    {
        BoxCollider2D box = arena.GetComponent<BoxCollider2D>();
        Rect camRect = GetCameraRect();

        Vector3 spawnPosition;
        int attempts = 0, maxAttempts = 30;

        do
        {
            float x = Random.Range(camRect.xMin, camRect.xMax);
            float y = Random.Range(camRect.yMin, camRect.yMax);
            spawnPosition = new Vector3(x, y, 0);
            attempts++;
        }
        while ((!IsInsideCamera(spawnPosition) || !IsOutsideBox(spawnPosition, box)) && attempts < maxAttempts);

        if (IsInsideCamera(spawnPosition) && IsOutsideBox(spawnPosition, box))
            Instantiate(gasterBlasterPrefab, spawnPosition, GetRotationTowardsPlayer(spawnPosition));
        yield return new WaitForSeconds(1f);
    }

    IEnumerator PatternBoneRain(int boneCount = 20, float duration = 3f)
    {
        BoxCollider2D box = arena.GetComponent<BoxCollider2D>();
        Vector2 boxCenter = box.transform.position;
        Vector2 boxSize = box.size * box.transform.lossyScale;

        float startY = boxCenter.y + boxSize.y / 2f; // Bord supérieur de la box
        float minX = boxCenter.x - boxSize.x / 2f + 0.5f;
        float maxX = boxCenter.x + boxSize.x / 2f - 0.5f;

        for (int i = 0; i < boneCount; i++)
        {
            float x = Random.Range(minX, maxX);
            Vector3 spawnPos = new Vector3(x, startY, 0);
            Instantiate(bonePrefab, spawnPos, Quaternion.identity);
            yield return new WaitForSeconds(duration / boneCount);
        }
    }

    IEnumerator PatternBoneWallWithGap(float y, int boneCount = 12, float gapWidth = 2f)
    {
        BoxCollider2D box = arena.GetComponent<BoxCollider2D>();
        Vector2 boxCenter = box.transform.position;
        Vector2 boxSize = box.size * box.transform.lossyScale;

        float minX = boxCenter.x - boxSize.x / 2f + 0.5f;
        float maxX = boxCenter.x + boxSize.x / 2f - 0.5f;

        float gapCenter = Random.Range(minX + gapWidth, maxX - gapWidth);

        for (int i = 0; i < boneCount; i++)
        {
            float t = (float)i / (boneCount - 1);
            float x = Mathf.Lerp(minX, maxX, t);

            if (Mathf.Abs(x - gapCenter) < gapWidth / 2f)
                continue;

            Vector3 spawnPos = new Vector3(x, y, 0);
            Instantiate(bonePrefab, spawnPos, Quaternion.identity);
        }
        yield return new WaitForSeconds(1.5f);
    }

    private Quaternion GetRotationTowardsPlayer(Vector3 spawnPosition)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return Quaternion.identity;
        Vector3 direction = player.transform.position - spawnPosition;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        return Quaternion.Euler(0, 0, angle);
    }

    private IEnumerator SpawnJewelAndWaitChoice()
    {
        // Récupère la box de combat
        BoxCollider2D box = arena.GetComponent<BoxCollider2D>();
        Vector2 boxCenter = box.transform.position;
        Vector2 boxSize = box.size * box.transform.lossyScale;

        // Position aléatoire dans la box
        float x = Random.Range(boxCenter.x - boxSize.x / 2f, boxCenter.x + boxSize.x / 2f);
        float y = Random.Range(boxCenter.y - boxSize.y / 2f, boxCenter.y + boxSize.y / 2f);
        Vector3 spawnPos = new Vector3(x, y, 0);

        // Instancie le joyau
        GameObject jewel = Instantiate(jewelPrefab, spawnPos, Quaternion.identity);

        // Affiche le canvas de choix
        choiceCanvas.gameObject.SetActive(true);

        bool choiceMade = false;
        attackButton.onClick.RemoveAllListeners();
        healButton.onClick.RemoveAllListeners();

        attackButton.onClick.AddListener(() => {
            choiceMade = true;
            // Appelle ici ta logique d'attaque spéciale
            ChronosGameManager.Instance.Attack();
        });
        healButton.onClick.AddListener(() => {
            choiceMade = true;
            // Appelle ici ta logique de soin
            ChronosGameManager.Instance.Heal();
        });

        // Attend le choix du joueur
        yield return new WaitUntil(() => choiceMade);

        // Nettoyage
        Destroy(jewel);
    }
}
