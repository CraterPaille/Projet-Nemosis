using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChronosAttackController : MonoBehaviour
{
    public GameObject gasterBlasterPrefab;
    public Transform arena;
    public float spawnRadius = 6f;
    public GameObject jewelPrefab;
    public Canvas choiceCanvas;
    public GameObject bonePrefab;
    public UnityEngine.UI.Button attackButton;
    public UnityEngine.UI.Button healButton;

    public float minWallGap = 2f;
    public float boneRainMinSpacing = 0.3f;
    public float boneRainSlowMultiplier = 1.6f;

    private Coroutine timeFluxCoroutine;

    void OnEnable()
    {
        StartCoroutine(AttackLoop());
    }

    void OnDisable()
    {
        // Arrêter TimeFlux si actif
        if (timeFluxCoroutine != null)
        {
            StopCoroutine(timeFluxCoroutine);
            Time.timeScale = 1f;
        }
    }

    IEnumerator AttackLoop()
    {
        while (ChronosGameManager.Instance == null)
            yield return null;

        int patternCount = 0;
        while (ChronosGameManager.Instance.playerHP > 0 && ChronosGameManager.Instance.bossCurrentHearts > 0)
        {
            if (patternCount > 0 && patternCount % 3 == 0)
                yield return StartCoroutine(SpawnJewelAndWaitChoice());

            int phase = ChronosGameManager.Instance.BossPhase;
            int pattern = 0;

            if (phase == 1)
            {
                pattern = Random.Range(0, 2);
            }
            else if (phase == 2)
            {
                pattern = Random.Range(0, 3);
            }
            else
            {
                pattern = Random.Range(0, 6);
            }

            // Phase 3+ : 50% de chance de lancer TimeFlux en parallèle
            bool useTimeFlux = phase >= 3 && Random.value < 0.5f;
            if (useTimeFlux && timeFluxCoroutine == null)
            {
                timeFluxCoroutine = StartCoroutine(PatternTimeFlux(8f));
            }

            switch (pattern)
            {
                case 0: yield return StartCoroutine(PatternFourCorners()); break;
                case 1: yield return StartCoroutine(PatternCircleAroundPlayer()); break;
                case 2: yield return StartCoroutine(PatternRandomSingleBlaster()); break;
                case 3: yield return StartCoroutine(PatternBoneRain(phase >= 5 ? 35 : 20, (phase >= 5 ? 2f : 3f) * boneRainSlowMultiplier)); break;
                case 4:
                    yield return StartCoroutine(PatternBoneWallWithGap(
                            arena.position.y + arena.GetComponent<BoxCollider2D>().size.y / 2f,
                            phase >= 4 ? 18 : 12,
                            Mathf.Max(minWallGap, (phase >= 4 ? 1.2f : 2f)))); break;
                case 5: yield return StartCoroutine(PatternSideBlastersWithFallingBones()); break;
            }

            patternCount++;
            yield return new WaitForSeconds(phase >= 4 ? 1.2f : 2f);
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
                ObjectPooler.Instance.SpawnFromPool("GasterBlaster", spawnPosition, GetRotationTowardsPlayer(spawnPosition));
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
                ObjectPooler.Instance.SpawnFromPool("GasterBlaster", spawnPosition, GetRotationTowardsPlayer(spawnPosition));
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
            ObjectPooler.Instance.SpawnFromPool("GasterBlaster", spawnPosition, GetRotationTowardsPlayer(spawnPosition));
        yield return new WaitForSeconds(1f);
    }

    IEnumerator PatternBoneRain(int boneCount = 20, float duration = 3f)
    {
        if (arena == null || bonePrefab == null) yield break;

        BoxCollider2D box = arena.GetComponent<BoxCollider2D>();
        Vector2 boxCenter = box.transform.position;
        Vector2 boxSize = box.size * box.transform.lossyScale;

        float startY = boxCenter.y + boxSize.y / 2f;
        float minX = boxCenter.x - boxSize.x / 2f + 0.5f;
        float maxX = boxCenter.x + boxSize.x / 2f - 0.5f;

        float totalTime = Mathf.Max(0.1f, duration);
        float baseDelay = totalTime / Mathf.Max(1, boneCount);
        float delay = baseDelay * 1.2f;

        float lastX = float.NegativeInfinity;

        for (int i = 0; i < boneCount; i++)
        {
            float x;
            int tries = 0;
            do
            {
                x = Random.Range(minX, maxX);
                tries++;
                if (tries > 10) break;
            } while (Mathf.Abs(x - lastX) < boneRainMinSpacing);

            lastX = x;
            Vector3 spawnPos = new Vector3(x, startY, 0);
            ObjectPooler.Instance.SpawnFromPool("Bone", spawnPos, Quaternion.identity);
            yield return new WaitForSeconds(delay);
        }
    }

    IEnumerator PatternBoneWallWithGap(float y, int boneCount = 12, float gapWidth = 2f)
    {
        if (arena == null || bonePrefab == null) yield break;

        BoxCollider2D box = arena.GetComponent<BoxCollider2D>();
        Vector2 boxCenter = box.transform.position;
        Vector2 boxSize = box.size * box.transform.lossyScale;

        float minX = boxCenter.x - boxSize.x / 2f + 0.5f;
        float maxX = boxCenter.x + boxSize.x / 2f - 0.5f;

        float actualGap = Mathf.Max(minWallGap, gapWidth);
        float gapCenter = Random.Range(minX + actualGap, maxX - actualGap);

        for (int i = 0; i < boneCount; i++)
        {
            float t = (float)i / Mathf.Max(1, boneCount - 1);
            float x = Mathf.Lerp(minX, maxX, t);

            if (Mathf.Abs(x - gapCenter) < actualGap / 2f)
                continue;

            Vector3 spawnPos = new Vector3(x, y, 0);
            ObjectPooler.Instance.SpawnFromPool("Bone", spawnPos, Quaternion.identity);
            yield return new WaitForSeconds(0.02f);
        }
        yield return new WaitForSeconds(1.5f);
    }

    // NOUVEAU PATTERN : Blasters sur les côtés GAUCHE/DROITE + os qui tombent
    IEnumerator PatternSideBlastersWithFallingBones(float duration = 7f)
    {
        if (arena == null) yield break;

        BoxCollider2D box = arena.GetComponent<BoxCollider2D>();
        Vector2 boxCenter = box.transform.position;
        Vector2 boxSize = box.size * box.transform.lossyScale;

        // Positions à gauche et à droite de la box
        float leftX = boxCenter.x - boxSize.x / 2f - 1f;
        float rightX = boxCenter.x + boxSize.x / 2f + 1f;
        float minY = boxCenter.y - boxSize.y / 2f + 0.5f;
        float maxY = boxCenter.y + boxSize.y / 2f - 0.5f;

        int blastersPerSide = 4;

        // Prépare positions des blasters sur les côtés
        List<Vector3> leftPositions = new List<Vector3>();
        List<Vector3> rightPositions = new List<Vector3>();

        for (int i = 0; i < blastersPerSide; i++)
        {
            float y = Mathf.Lerp(minY, maxY, blastersPerSide == 1 ? 0.5f : (float)i / (blastersPerSide - 1));
            leftPositions.Add(new Vector3(leftX, y, 0));
            rightPositions.Add(new Vector3(rightX, y, 0));
        }

        // Démarre la boucle de blasters en parallèle
        float blasterInterval = 1.2f;
        Coroutine blastersCoroutine = StartCoroutine(SpawnSideBlastersLoop(leftPositions, rightPositions, duration, blasterInterval));

        // Fait tomber des os pendant que les lasers tirent
        float boneStartY = boxCenter.y + boxSize.y / 2f;
        float boneMinX = boxCenter.x - boxSize.x / 2f + 0.5f;
        float boneMaxX = boxCenter.x + boxSize.x / 2f - 0.5f;

        float elapsed = 0f;
        float boneSpawnInterval = 0.35f;
        float gapSize = 1.5f; // Taille du gap pour laisser passer le joueur
        float lastGapX = float.NegativeInfinity;

        while (elapsed < duration)
        {
            // Spawn 3-5 os avec UN gap aléatoire
            int boneCount = Random.Range(3, 6);
            float gapX = Random.Range(boneMinX + gapSize, boneMaxX - gapSize);

            // Évite que le gap soit au même endroit qu'avant
            if (Mathf.Abs(gapX - lastGapX) < gapSize * 1.5f)
            {
                gapX = boneMinX + (boneMaxX - boneMinX) * Random.Range(0.3f, 0.7f);
            }
            lastGapX = gapX;

            for (int i = 0; i < boneCount; i++)
            {
                float t = (float)i / Mathf.Max(1, boneCount - 1);
                float x = Mathf.Lerp(boneMinX, boneMaxX, t);

                // Laisse le gap
                if (Mathf.Abs(x - gapX) < gapSize / 2f)
                    continue;

                Vector3 bonePos = new Vector3(x, boneStartY, 0);
                ObjectPooler.Instance.SpawnFromPool("Bone", bonePos, Quaternion.identity);
            }

            yield return new WaitForSeconds(boneSpawnInterval);
            elapsed += boneSpawnInterval;
        }

        // Arrête les blasters
        if (blastersCoroutine != null)
            StopCoroutine(blastersCoroutine);

        yield return new WaitForSeconds(0.3f);
    }

    // Spawn les blasters sur les côtés (gauche et droite) qui tirent horizontalement
    IEnumerator SpawnSideBlastersLoop(List<Vector3> leftPositions, List<Vector3> rightPositions, float duration, float interval)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // Alterne entre gauche et droite ou spawn les deux
            bool spawnLeft = Random.value > 0.3f;
            bool spawnRight = Random.value > 0.3f;

            if (spawnLeft)
            {
                // Choisit une position aléatoire à gauche
                int index = Random.Range(0, leftPositions.Count);
                Vector3 pos = leftPositions[index];
                // Rotation pour tirer vers la droite (0°)
                ObjectPooler.Instance.SpawnFromPool("GasterBlaster", pos, Quaternion.Euler(0, 0, 0));
            }

            if (spawnRight)
            {
                // Choisit une position aléatoire à droite
                int index = Random.Range(0, rightPositions.Count);
                Vector3 pos = rightPositions[index];
                // Rotation pour tirer vers la gauche (180°)
                ObjectPooler.Instance.SpawnFromPool("GasterBlaster", pos, Quaternion.Euler(0, 0, 180));
            }

            float waited = 0f;
            while (waited < interval && elapsed < duration)
            {
                waited += Time.deltaTime;
                elapsed += Time.deltaTime;
                yield return null;
            }
        }
    }

    // CORRIGÉ : TimeFlux en parallèle avec affichage du statut
    IEnumerator PatternTimeFlux(float totalDuration = 8f)
    {
        float elapsed = 0f;

        while (elapsed < totalDuration)
        {
            float targetScale = Random.Range(0.45f, 1.6f);
            float transTime = Random.Range(0.12f, 0.35f);
            float holdTime = Random.Range(0.6f, 1.2f);

            // Afficher le statut dans le dialogue
            string status = targetScale < 0.8f ? "* Le temps ralentit..." :
                           targetScale > 1.2f ? "* Le temps accélère !" :
                           "* Le temps se stabilise.";
            ChronosGameManager.Instance.dialogueText.text = status;

            // Lerp vers target
            float t = 0f;
            float startScale = Time.timeScale;
            while (t < transTime)
            {
                t += Time.unscaledDeltaTime;
                Time.timeScale = Mathf.Lerp(startScale, targetScale, t / transTime);
                yield return null;
            }
            Time.timeScale = targetScale;

            // Hold stable
            float rt = 0f;
            while (rt < holdTime)
            {
                rt += Time.unscaledDeltaTime;
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        // Restaurer timescale
        ChronosGameManager.Instance.dialogueText.text = "* Le temps revient à la normale.";
        float restoreTime = 0.3f;
        float tr = 0f;
        float from = Time.timeScale;
        while (tr < restoreTime)
        {
            tr += Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Lerp(from, 1f, tr / restoreTime);
            yield return null;
        }
        Time.timeScale = 1f;

        timeFluxCoroutine = null;
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
        BoxCollider2D box = arena.GetComponent<BoxCollider2D>();
        Vector2 boxCenter = box.transform.position;
        Vector2 boxSize = box.size * box.transform.lossyScale;

        float x = Random.Range(boxCenter.x - boxSize.x / 2f, boxCenter.x + boxSize.x / 2f);
        float y = Random.Range(boxCenter.y - boxSize.y / 2f, boxCenter.y + boxSize.y / 2f);
        Vector3 spawnPos = new Vector3(x, y, 0);

        GameObject jewel = ObjectPooler.Instance.SpawnFromPool("Jewel", spawnPos, Quaternion.identity);

        choiceCanvas.gameObject.SetActive(true);

        bool choiceMade = false;
        attackButton.onClick.RemoveAllListeners();
        healButton.onClick.RemoveAllListeners();

        attackButton.onClick.AddListener(() => {
            choiceMade = true;
            ChronosGameManager.Instance.Attack();
        });
        healButton.onClick.AddListener(() => {
            choiceMade = true;
            ChronosGameManager.Instance.Heal();
        });

        yield return new WaitUntil(() => choiceMade);

        jewel.SetActive(false);
    }
}