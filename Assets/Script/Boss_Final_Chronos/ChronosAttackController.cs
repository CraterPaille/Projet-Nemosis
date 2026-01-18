using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChronosAttackController : MonoBehaviour
{
    [Header("Prefabs & UI")]
    public GameObject gasterBlasterPrefab;
    public Transform arena;
    public GameObject jewelPrefab;
    public Canvas choiceCanvas;
    public GameObject bonePrefab;
    public UnityEngine.UI.Button attackButton;
    public UnityEngine.UI.Button healButton;
    public JusticeShieldController justiceController;

    [Header("Settings")]
    public float spawnRadius = 6f;
    public float minWallGap = 2f;
    public float boneRainMinSpacing = 0.3f;
    public float boneRainSlowMultiplier = 1.6f;

    // Cache
    private Transform player;
    private BoxCollider2D arenaBox;
    private ChronosGameManager gm;
    private Rect camRect;
    private bool camDirty = true;
    private Bounds arenaBounds;
    private Vector2 arenaCenter, arenaSize;

    private bool isJusticeMode;
    private bool hasPhase5Pattern;
    private Coroutine timeFluxCo;

    void OnEnable()
    {
        hasPhase5Pattern = false;
        StartCoroutine(Init());
    }

    IEnumerator Init()
    {
        while (!ChronosGameManager.Instance) yield return null;
        gm = ChronosGameManager.Instance;
        player = GameObject.FindGameObjectWithTag("PlayerSoul").transform;
        arenaBox = arena.GetComponent<BoxCollider2D>();
        UpdateArenaCache();
        StartCoroutine(AttackLoop());
    }

    void UpdateArenaCache()
    {
        arenaBounds = arenaBox.bounds;
        arenaCenter = arenaBounds.center;
        arenaSize = arenaBounds.size;
    }

    void LateUpdate() => camDirty = true;

    void OnDisable()
    {
        if (timeFluxCo != null) { StopCoroutine(timeFluxCo); Time.timeScale = 1f; timeFluxCo = null; }
    }

    IEnumerator AttackLoop()
    {
        int count = 0;
        while (gm.playerHP > 0 && gm.bossCurrentHearts > 0)
        {
            int phase = gm.BossPhase;

            // Phase 5: Justice Mode
            if (phase == 5 && !hasPhase5Pattern)
            {
                hasPhase5Pattern = true;
                gm.dialogueText.text = "* Chronos devient sérieux...";
                yield return new WaitForSeconds(2f);
                yield return PatternTimeStopJustice();
                gm.bossCurrentHearts = 1;
                gm.bossCurrentHP = gm.bossHeartHP;
                gm.UpdateUI();
                gm.dialogueText.text = "* Phase 6 ! Le jugement commence...";
                yield return new WaitForSeconds(3f);
                count = 0;
                continue;
            }

            // Phase 6: Final
            if (phase == 6) { yield return Phase6_FinalJudgment(); break; }

            // Jewel every 3 patterns
            if (count > 0 && count % 3 == 0 && phase < 5)
                yield return SpawnJewelAndWaitChoice();

            // Pattern selection
            int p = phase == 1 ? Random.Range(0, 2) : phase == 2 ? Random.Range(0, 3) : Random.Range(0, 6);

            // Time flux
            bool flux = phase >= 3 && phase < 6 && Random.value < 0.5f;
            if (flux && timeFluxCo == null) timeFluxCo = StartCoroutine(PatternTimeFlux(8f));

            // Execute pattern
            switch (p)
            {
                case 0: yield return PatternFourCorners(); break;
                case 1: yield return PatternCircleAroundPlayer(); break;
                case 2: yield return PatternRandomSingleBlaster(); break;
                case 3: yield return PatternBoneRain(phase >= 5 ? 35 : 20, (phase >= 5 ? 2f : 3f) * boneRainSlowMultiplier); break;
                case 4: yield return PatternBoneWallWithGap(arenaCenter.y + arenaSize.y / 2f, phase >= 4 ? 18 : 12, Mathf.Max(minWallGap, phase >= 4 ? 1.2f : 2f)); break;
                case 5: yield return PatternSideBlastersWithFallingBones(); break;
            }

            count++;
            yield return new WaitForSeconds(phase >= 4 ? 1.2f : 2f);
        }
    }

    // === PHASE 6 ===
    IEnumerator Phase6_FinalJudgment()
    {
        yield return Phase6_Intro();
        yield return Phase6B_SpiralBarrage();
        yield return Phase6C_FinalAssault();
        yield return Phase6_Ending();
    }

    IEnumerator Phase6_Intro()
    {
        string[] msgs = { "* ...", "* Tu as osé aller aussi loin.", "* Très bien. Montre-moi ce que tu vaux vraiment.", "* EN GARDE !" };
        float[] delays = { 1.5f, 2f, 2f, 1.5f };
        for (int i = 0; i < msgs.Length; i++) { gm.dialogueText.text = msgs[i]; yield return new WaitForSeconds(delays[i]); }
    }

    IEnumerator Phase6B_SpiralBarrage()
    {
        gm.dialogueText.text = "* Prépare-toi !";
        yield return new WaitForSeconds(2f);

        for (int s = 0; s < 4; s++)
        {
            gm.dialogueText.text = $"* Spirale {s + 1} !";
            for (int i = 0; i < 25; i++)
            {
                float t = i / 25f;
                float angle = (t * 720f + s * 90f) * Mathf.Deg2Rad;
                float r = 10f * (1f - t * 0.3f);
                Vector3 pos = player.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * r;
                if (InCam(pos) && OutBox(pos)) Spawn("GasterBlaster", pos, RotToPlayer(pos));
                yield return new WaitForSeconds(0.12f);
            }
            yield return new WaitForSeconds(3f);
        }
        gm.dialogueText.text = "* Pas mal... Mais ce n'est pas fini !";
        yield return new WaitForSeconds(2f);
    }

    IEnumerator Phase6C_FinalAssault()
    {
        gm.dialogueText.text = "* C'est l'heure du jugement !";
        yield return new WaitForSeconds(2f);

        float e = 0;
        int w = 0;
        while (e < 20f)
        {
            switch (w++ % 4)
            {
                case 0: StartCoroutine(PatternCircleAroundPlayer()); yield return new WaitForSeconds(3f); e += 3f; break;
                case 1: StartCoroutine(PatternFourCorners()); yield return new WaitForSeconds(2.5f); e += 2.5f; break;
                case 2: StartCoroutine(PatternBoneRain(30, 3f)); yield return new WaitForSeconds(3.5f); e += 3.5f; break;
                case 3: StartCoroutine(PatternSideBlastersWithFallingBones(5f)); yield return new WaitForSeconds(5.5f); e += 5.5f; break;
            }
        }

        gm.dialogueText.text = "* DERNIER COUP !";
        yield return FinalWave(); // FIX: Appel correct de FinalWave
        yield return new WaitForSeconds(3f);
    }

    IEnumerator FinalWave()
    {
        for (int c = 0; c < 3; c++)
        {
            float r = 8f + c * 2.5f;
            for (int i = 0; i < 10; i++)
            {
                float angle = i * 36f * Mathf.Deg2Rad;
                Vector3 pos = player.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * r;
                if (InCam(pos) && OutBox(pos)) Spawn("GasterBlaster", pos, RotToPlayer(pos));
            }
            yield return new WaitForSeconds(1f);
        }
    }

    IEnumerator Phase6_Ending()
    {
        // FIX: Enlève le dernier cœur
        gm.bossCurrentHearts = 0;
        gm.bossCurrentHP = 0;
        gm.UpdateUI();

        string[] msgs = { "* ...", "* Incroyable...", "* Tu as vraiment... gagné.", "* Félicitations, humain." };
        foreach (var m in msgs) { gm.dialogueText.text = m; yield return new WaitForSeconds(2f); }

        // Lance la scène de victoire stylée
        yield return VictorySequence();
    }

    IEnumerator VictorySequence()
    {
        // Trouve ou crée le VictoryScreen
        var victoryScreen = FindFirstObjectByType<VictoryScreen>();
        if (victoryScreen == null)
        {
            Debug.LogError("VictoryScreen not found! Create an empty GameObject with VictoryScreen.cs");
            gm.dialogueText.text = "* ★ VICTOIRE ! ★";
            yield break;
        }

        victoryScreen.PlayVictoryAnimation();
        StopAllCoroutines();
    }

    // === PATTERNS ===
    IEnumerator PatternFourCorners()
    {
        for (int i = 0; i < 4; i++)
        {
            float r = 6f;
            int tries = 0;
            Vector3 pos;
            do
            {
                float a = i * Mathf.PI / 2;
                pos = new Vector3(Mathf.Cos(a), Mathf.Sin(a)) * r;
                r += 0.5f;
            } while ((!InCam(pos) || !OutBox(pos)) && ++tries < 20);
            if (InCam(pos) && OutBox(pos)) Spawn("GasterBlaster", pos, RotToPlayer(pos));
            yield return new WaitForSeconds(0.3f);
        }
    }

    IEnumerator PatternCircleAroundPlayer()
    {
        Vector3[] oct = {
            new Vector3(7,0), new Vector3(5,5), new Vector3(0,7), new Vector3(-5,5),
            new Vector3(-7,0), new Vector3(-5,-5), new Vector3(0,-7), new Vector3(5,-5)
        };
        foreach (var off in oct)
        {
            Vector3 pos = player.position + off;
            if (!InCam(pos) || !OutBox(pos)) continue;
            float a = Mathf.Atan2(off.y, off.x) * Mathf.Rad2Deg;
            Spawn("GasterBlaster", pos, Quaternion.Euler(0, 0, a));
            yield return new WaitForSeconds(0.2f);
        }
    }

    IEnumerator PatternRandomSingleBlaster()
    {
        Vector3 pos;
        int tries = 0;
        do
        {
            pos = new Vector3(Random.Range(camRect.xMin, camRect.xMax), Random.Range(camRect.yMin, camRect.yMax));
        } while ((!InCam(pos) || !OutBox(pos)) && ++tries < 30);
        if (InCam(pos) && OutBox(pos)) Spawn("GasterBlaster", pos, RotToPlayer(pos));
        yield return new WaitForSeconds(1f);
    }

    IEnumerator PatternBoneRain(int count, float dur)
    {
        float startY = arenaCenter.y + arenaSize.y / 2f;
        float minX = arenaCenter.x - arenaSize.x / 2f + 0.5f;
        float maxX = arenaCenter.x + arenaSize.x / 2f - 0.5f;
        float delay = Mathf.Max(0.1f, dur / count) * 1.2f;
        float lastX = float.NegativeInfinity;

        for (int i = 0; i < count; i++)
        {
            float x;
            int t = 0;
            do { x = Random.Range(minX, maxX); } while (Mathf.Abs(x - lastX) < boneRainMinSpacing && ++t < 10);
            lastX = x;
            Spawn("needle", new Vector3(x, startY, 0), Quaternion.identity);
            yield return new WaitForSeconds(delay);
        }
    }

    IEnumerator PatternBoneWallWithGap(float y, int count, float gap)
    {
        float minX = arenaCenter.x - arenaSize.x / 2f + 0.5f;
        float maxX = arenaCenter.x + arenaSize.x / 2f - 0.5f;
        float gapX = Random.Range(minX + gap, maxX - gap);

        for (int i = 0; i < count; i++)
        {
            float x = Mathf.Lerp(minX, maxX, i / (float)(count - 1));
            if (Mathf.Abs(x - gapX) < gap / 2f) continue;
            Spawn("needle", new Vector3(x, y, 0), Quaternion.identity);
            yield return new WaitForSeconds(0.02f);
        }
        yield return new WaitForSeconds(1.5f);
    }

    IEnumerator PatternSideBlastersWithFallingBones(float dur = 7f)
    {
        float lx = arenaCenter.x - arenaSize.x / 2f - 1f;
        float rx = arenaCenter.x + arenaSize.x / 2f + 1f;
        float minY = arenaCenter.y - arenaSize.y / 2f + 0.5f;
        float maxY = arenaCenter.y + arenaSize.y / 2f - 0.5f;

        List<Vector3> lPos = new(), rPos = new();
        for (int i = 0; i < 4; i++)
        {
            float y = Mathf.Lerp(minY, maxY, i / 3f);
            lPos.Add(new Vector3(lx, y, 0));
            rPos.Add(new Vector3(rx, y, 0));
        }

        StartCoroutine(SpawnSideBlasters(lPos, rPos, dur));

        float e = 0, startY = arenaCenter.y + arenaSize.y / 2f;
        float minX = arenaCenter.x - arenaSize.x / 2f + 0.5f;
        float maxX = arenaCenter.x + arenaSize.x / 2f - 0.5f;
        float lastGap = float.NegativeInfinity;

        while (e < dur)
        {
            int bc = Random.Range(3, 6);
            float gx = Random.Range(minX + 1.5f, maxX - 1.5f);
            if (Mathf.Abs(gx - lastGap) < 2.25f) gx = minX + (maxX - minX) * Random.Range(0.3f, 0.7f);
            lastGap = gx;

            for (int i = 0; i < bc; i++)
            {
                float x = Mathf.Lerp(minX, maxX, i / (float)(bc - 1));
                if (Mathf.Abs(x - gx) < 0.75f) continue;
                Spawn("needle", new Vector3(x, startY, 0), Quaternion.identity);
            }
            yield return new WaitForSeconds(0.35f);
            e += 0.35f;
        }
        yield return new WaitForSeconds(0.3f);
    }

    IEnumerator SpawnSideBlasters(List<Vector3> l, List<Vector3> r, float dur)
    {
        float e = 0;
        while (e < dur)
        {
            if (Random.value > 0.3f) Spawn("GasterBlaster", l[Random.Range(0, l.Count)], Quaternion.identity);
            if (Random.value > 0.3f) Spawn("GasterBlaster", r[Random.Range(0, r.Count)], Quaternion.Euler(0, 0, 180));
            float w = 0;
            while (w < 1.2f && e < dur) { w += Time.deltaTime; e += Time.deltaTime; yield return null; }
        }
    }

    IEnumerator PatternTimeFlux(float dur = 8f)
    {
        float e = 0;
        while (e < dur)
        {
            float scale = Random.Range(0.45f, 1.6f);
            float trans = Random.Range(0.12f, 0.35f);
            float hold = Random.Range(0.6f, 1.2f);
            gm.dialogueText.text = scale < 0.8f ? "* Le temps ralentit..." : scale > 1.2f ? "* Le temps accélère !" : "* Le temps se stabilise.";

            float t = 0, start = Time.timeScale;
            while (t < trans) { t += Time.unscaledDeltaTime; Time.timeScale = Mathf.Lerp(start, scale, t / trans); yield return null; }
            Time.timeScale = scale;

            float rt = 0;
            while (rt < hold) { rt += Time.unscaledDeltaTime; e += Time.unscaledDeltaTime; yield return null; }
        }

        gm.dialogueText.text = "* Le temps revient à la normale.";
        float tr = 0, from = Time.timeScale;
        while (tr < 0.3f) { tr += Time.unscaledDeltaTime; Time.timeScale = Mathf.Lerp(from, 1f, tr / 0.3f); yield return null; }
        Time.timeScale = 1f;
        timeFluxCo = null;
    }

    IEnumerator PatternTimeStopJustice()
    {
        if (!justiceController) yield break;

        gm.dialogueText.text = "* Le temps s'arrête...";
        yield return new WaitForSeconds(1.5f);
        isJusticeMode = true;

        if (timeFluxCo != null) { StopCoroutine(timeFluxCo); timeFluxCo = null; Time.timeScale = 1f; }

        var ps = player.GetComponent<PlayerSoul>();
        if (ps) ps.EnterJusticeMode(arenaBox, arenaCenter);
        justiceController.ActivateShields();

        gm.dialogueText.text = "* La justice ne s'arrête pour PERSONNE !";
        yield return new WaitForSeconds(1.5f);

        // FIX: RADIUS FIXE À 8f pour TOUTES les vagues
        for (int w = 1; w <= 5; w++)
        {
            gm.dialogueText.text = $"* Vague {w} !";
            float charge = Mathf.Max(0.8f, 1.5f - w * 0.15f);
            float speed = 3f + w * 0.3f;
            yield return SpawnCardinalBlasters(player.position, 8f, charge, speed); // TOUJOURS 8f !
            yield return new WaitForSeconds(Mathf.Max(2.5f, 4f - w * 0.3f));
        }

        gm.dialogueText.text = "* Tu as survécu... pour l'instant.";
        yield return new WaitForSeconds(2f);
        justiceController.DeactivateShields();
        isJusticeMode = false;
        if (ps) ps.ExitJusticeMode();
        gm.dialogueText.text = "* Le temps reprend son cours.";
        yield return new WaitForSeconds(1f);
    }

    IEnumerator SpawnCardinalBlasters(Vector3 center, float r, float charge, float speed)
    {
        Vector3[] dirs = { Vector3.up, Vector3.right, Vector3.down, Vector3.left };
        foreach (var d in dirs)
        {
            Vector3 pos = center + d * r;
            if (!InCam(pos) || !OutBox(pos)) continue;
            float angle = Mathf.Atan2(-d.y, -d.x) * Mathf.Rad2Deg;
            var gb = Spawn("GasterBlaster", pos, Quaternion.Euler(0, 0, angle), false);
            if (gb)
            {
                var gbc = gb.GetComponent<GasterBlaster>();
                if (gbc) { gbc.chargeDuration = charge; gbc.laserSpeed = speed; gbc.forceCardinalDirection = true; gbc.forcedDirection = -d; gbc.initialDelay = 0; }
                gb.SetActive(true);
            }
            yield return new WaitForSeconds(0.3f);
        }
    }

    IEnumerator SpawnJewelAndWaitChoice()
    {
        float x = Random.Range(arenaCenter.x - arenaSize.x / 2f, arenaCenter.x + arenaSize.x / 2f);
        float y = Random.Range(arenaCenter.y - arenaSize.y / 2f, arenaCenter.y + arenaSize.y / 2f);
        var jewel = Spawn("Jewel", new Vector3(x, y, 0), Quaternion.identity);
        // Animation DOTween
        Tween floatTween = jewel.transform
       .DOMoveY(y + 0.5f, 1f)
       .SetLoops(-1, LoopType.Yoyo)
       .SetEase(Ease.InOutSine);

        Tween rotateTween = jewel.transform
       .DORotate(new Vector3(0, 0, 360), 2f, RotateMode.FastBeyond360)
       .SetLoops(-1)
       .SetEase(Ease.Linear);

        jewel.transform.localScale = Vector3.zero;
        jewel.transform.DOScale(0.3f, 0.3f).SetEase(Ease.OutBack);

        choiceCanvas.gameObject.SetActive(true);
        bool done = false;
        attackButton.onClick.RemoveAllListeners();
        healButton.onClick.RemoveAllListeners();
        attackButton.onClick.AddListener(() => { done = true; gm.Attack(); });
        healButton.onClick.AddListener(() => { done = true; gm.Heal(); });
        yield return new WaitUntil(() => done);

        floatTween.Kill();
        rotateTween.Kill();

        jewel.transform.DOScale(0f, 0.2f).OnComplete(() =>
        {
            jewel.SetActive(false);
        });
    }

    // === HELPERS ===
    Rect GetCamRect()
    {
        if (camDirty)
        {
            Camera c = Camera.main;
            float h = 2f * c.orthographicSize, w = h * c.aspect;
            Vector2 p = c.transform.position;
            camRect = new Rect(p.x - w / 2f, p.y - h / 2f, w, h);
            camDirty = false;
        }
        return camRect;
    }

    bool InCam(Vector3 p) { GetCamRect(); return camRect.Contains(p); }
    bool OutBox(Vector3 p) => p.x < arenaBounds.min.x || p.x > arenaBounds.max.x || p.y < arenaBounds.min.y || p.y > arenaBounds.max.y;
    Quaternion RotToPlayer(Vector3 pos) { Vector3 d = player.position - pos; return Quaternion.Euler(0, 0, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg); }
    GameObject Spawn(string tag, Vector3 pos, Quaternion rot, bool active = true) => ObjectPooler.Instance.SpawnFromPool(tag, pos, rot, null, active);
}