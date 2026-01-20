using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using DG.Tweening;

public enum WeatherMode
{
    Normal,
    Pluie,      // moins d’ennemis, un peu de brouillard
    CielClair,  // plus d’ennemis
    Orage       // gros pic de difficulté
}

public class GameManagerZeus : MonoBehaviour
{
    public static GameManagerZeus Instance;

    [Header("Prefabs & Scene refs")]
    public GameObject enemyPrefab;
    public Transform[] spawnPoints;
    public Transform enemyParent;
    public LightningController lightningController;
    public GameObject fogOverlay; // UI overlay for fog
    public Camera mainCamera;

    [Header("Gameplay")]
    public float baseSpawnRate = 1.2f; // seconds between spawns
    public float gameDuration = 20f;

    [Header("UI")]
    public TMP_Text timerText;
    public TMP_Text scoreText;
    public GameObject endPanel;
    public TMP_Text endPanelText;

    [Header("Météo")]
    public WeatherMode currentWeather = WeatherMode.Normal;

    [Header("Options")]
    public bool autoStartOnLoad = true; // si vrai, StartGame() est appelé automatiquement à l'ouverture de la scène

    [Header("Paliers étoiles")]
    public int[] starThresholds = new int[3] { 5, 10, 20 }; // ajuster selon le gameplay
    private bool[] starGiven = new bool[3];

    [Header("UI Étoiles")]
    public UnityEngine.UI.Image[] starImages;
    public Sprite starOnSprite;
    public Sprite starOffSprite;

    // Runtime
    private float spawnRate;
    private float currentTime;
    private int enemiesKilled = 0;
    private int enemiesPassed = 0;
    private int score = 0;          // score selon ta règle
    private bool isRunning = false;
    private float _baseSpawnRateCached;

    // dérivé de la carte
    private float _chaosLevel = 0f;
    private float _rewardMult = 1f;
    private float _rewardFlat = 0f;
    private bool _oneMistakeFail = false;

    public MiniGameTutorialPanel tutorialPanel; // à assigner dans l'inspector
    public VideoClip tutorialClip; // à assigner dans l'inspector
    private bool tutorialValidated = false;


    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        ShowTutorialAndStart();
        _baseSpawnRateCached = baseSpawnRate;
        ApplyMiniGameCardIfAny();

        // Initialisation étoiles
        if (starThresholds == null || starThresholds.Length == 0)
            starThresholds = new int[3] { 5, 10, 20 };

        starGiven = new bool[starThresholds.Length];
        // Préparer l'affichage des étoiles si assignées — OFF doit être visible (scale = 1, couleur dim)
        if (starImages != null)
        {
            for (int i = 0; i < starImages.Length; i++)
            {
                if (starImages[i] != null)
                {
                    starImages[i].sprite = starOffSprite;
                    starImages[i].transform.localScale = Vector3.one;
                    starImages[i].color = new Color(1f, 1f, 1f, 0.45f); // dimmer pour 'off'
                }
            }
        }
        UpdateStarsUI();
    }

    private void ShowTutorialAndStart()
    {
        tutorialPanel.ShowClick(
            "Zeus",
            tutorialClip
        );
        tutorialPanel.continueButton.onClick.RemoveAllListeners();
        tutorialPanel.continueButton.onClick.AddListener(() => {
            tutorialPanel.Hide();
            tutorialValidated = true;
        });
    }

    private void ApplyMiniGameCardIfAny()
    {
        var runtime = MiniGameCardRuntime.Instance;
        if (runtime == null || runtime.SelectedCard == null)
            return;

        var card = runtime.SelectedCard;
        if (card.targetMiniGame != MiniGameType.Any && card.targetMiniGame != MiniGameType.Zeus)
            return;

        float speedMult = Mathf.Max(0.1f, card.speedMultiplier);
        float spawnMult = Mathf.Max(0.1f, card.spawnRateMultiplier);

        baseSpawnRate = _baseSpawnRateCached / speedMult;
        baseSpawnRate /= spawnMult;

        if (card.moreEnemies) currentWeather = WeatherMode.Orage;
        if (card.lessEnemies) currentWeather = WeatherMode.Pluie;

        _chaosLevel = Mathf.Clamp01(card.chaosLevel);
        _rewardMult = Mathf.Max(0.1f, card.rewardMultiplier);
        _rewardFlat = card.rewardFlatBonus;
        _oneMistakeFail = card.oneMistakeFail;   // <--- AJOUT

        Debug.Log($"[Zeus] Carte appliquée : {card.cardName}, baseSpawnRate={baseSpawnRate}, chaos={_chaosLevel}, rewardMult={_rewardMult}, rewardFlat={_rewardFlat}, oneMistakeFail={_oneMistakeFail}, météo={currentWeather}");

        runtime.Clear();
    }

    public void StartGame()
    {
        if (enemyPrefab == null || spawnPoints == null || spawnPoints.Length == 0 || lightningController == null)
        {
            Debug.LogError("GameManagerZeus: Prefabs or references missing! Vérifie enemyPrefab, spawnPoints et lightningcontroller dans l'inspector.");
            return;
        }

        // météo aléatoire seulement si la carte ne l’a pas forcée
        if (currentWeather == WeatherMode.Normal)
        {
            currentWeather = (WeatherMode)Random.Range(0, 4);
        }

        ResetGame();
        ApplyWeatherModifiers();

        isRunning = true;
        StartCoroutine(SpawnLoop());
        UpdateUI();
    }

    void ResetGame()
    {
        enemiesKilled = 0;
        enemiesPassed = 0;
        score = 0;
        currentTime = gameDuration;
        spawnRate = baseSpawnRate;

        // Reset lightning controller modifiers
        lightningController.invertClicks = false;
        lightningController.bounceLightning = false;

        if (fogOverlay) fogOverlay.SetActive(false);

        // Reset étoiles — OFF visible
        for (int i = 0; i < starGiven.Length; i++) starGiven[i] = false;
        if (starImages != null)
        {
            for (int i = 0; i < starImages.Length; i++)
            {
                if (starImages[i] != null)
                {
                    starImages[i].sprite = starOffSprite;
                    starImages[i].transform.localScale = Vector3.one;
                    starImages[i].color = new Color(1f, 1f, 1f, 0.45f);
                }
            }
        }
        UpdateStarsUI();
    }

    void ApplyWeatherModifiers()
    {
        // On part de la valeur de base
        spawnRate = baseSpawnRate;

        switch (currentWeather)
        {
            case WeatherMode.Normal:
                break;

            case WeatherMode.Pluie:
                // Moins d’ennemis, brouillard léger
                spawnRate *= 1.4f;
                if (fogOverlay) fogOverlay.SetActive(true);
                break;

            case WeatherMode.CielClair:
                // Plus d’ennemis
                spawnRate *= 0.8f;
                if (fogOverlay) fogOverlay.SetActive(false);
                break;

            case WeatherMode.Orage:
                // Beaucoup d’ennemis, gros brouillard
                spawnRate *= 0.6f;
                if (fogOverlay) fogOverlay.SetActive(true);
                break;
        }
    }

    IEnumerator SpawnLoop()
    {
        while (isRunning)
        {
            // temps d’attente chaotique autour de spawnRate
            float chaosFactor = 1f + Random.Range(-_chaosLevel, _chaosLevel);
            float wait = Mathf.Max(0.1f, spawnRate * chaosFactor);

            yield return new WaitForSeconds(wait);
            SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        if (spawnPoints.Length == 0 || enemyPrefab == null) return;

        int idx = Random.Range(0, spawnPoints.Length);
        Vector3 pos = spawnPoints[idx].position;

        GameObject e = Instantiate(enemyPrefab, pos, Quaternion.identity, enemyParent);

        // Type d’ennemi spécial
        Enemy enemy = e.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.RandomizeType();
        }
    }

    // Appelé par Enemy.Kill()
    public void EnemyKilled(Vector3 killPosition, EnemyType type)
    {
        enemiesKilled++;

        // Score selon le type
        switch (type)
        {
            case EnemyType.Normal:
                score += 1;  // tué +1
                break;
            case EnemyType.Gold:
                score += 5;  // doré +5
                break;
            case EnemyType.Red:
                score += 1;  // rouge tué = comme un normal
                break;
            case EnemyType.Blue:
                score += 1;  // bleu tué = +1 aussi
                break;
        }

        if (mainCamera != null)
        {
            var shake = mainCamera.GetComponent<CameraShake>();
            if (shake != null) shake.Shake(0.05f, 0.05f);
        }

        UpdateUI();
        CheckStarThresholds();

        // animation score selon le type (positive)
        PlayScoreAnimation(type, true);
    }

    // Appelé par Enemy.ReachCity()
    public void EnemyReachedCity(EnemyType type)
    {
        if (!isRunning) return;

        enemiesPassed++;

        // --- ONE MISTAKE FAIL ---
        if (_oneMistakeFail)
        {
            Debug.Log("[Zeus] Mode oneMistakeFail : un ennemi a atteint la ville -> fin immédiate.");
            EndGame();
            return;
        }

        // Score négatif selon le type (logique existante)
        switch (type)
        {
            case EnemyType.Normal:
            case EnemyType.Blue:
                score -= 1;
                break;
            case EnemyType.Gold:
                score -= 1;
                break;
            case EnemyType.Red:
                score -= 5;
                break;
        }

        UpdateUI();
        CheckStarThresholds();

        // animation score négative
        PlayScoreAnimation(type, false);
    }

    void Update()
    {
        // Ne démarre le jeu que si le tutoriel a été validé
        if (!tutorialValidated)
            return;
        if (!isRunning) return;

        currentTime -= Time.deltaTime;
        if (timerText) timerText.text = "Time: " + Mathf.CeilToInt(currentTime);

        if (currentTime <= 0) EndGame();
    }

    void UpdateUI()
    {
        if (scoreText)
            scoreText.text = $"Score: {score} Tués: {enemiesKilled}  Passés: {enemiesPassed}  ";
    }

    void EndGame()
    {
        isRunning = false;
        StopAllCoroutines();

        int finalScore = Mathf.Max(0, score);

        if (endPanel) endPanel.SetActive(true);
        if (endPanelText)
        {
            endPanelText.text =
                $"Météo : {currentWeather}\n" +
                $"Tués : {enemiesKilled}\n" +
                $"Passés : {enemiesPassed}\n" +
                $"Score : {finalScore}";
        }

        // Conversion score -> Population (Human)
        if (GameManager.Instance != null)
        {
            float baseHumanGain = finalScore / 2f;
            float humanGain = baseHumanGain * _rewardMult + _rewardFlat;

            if (humanGain != 0)
                GameManager.Instance.changeStat(StatType.Human, humanGain);

            Debug.Log($"[Zeus] Score={finalScore} -> Human +{humanGain} (mult x{_rewardMult}, flat +{_rewardFlat})");
        }
        SceneManager.LoadScene("SampleScene");

    }

    public void OnQuitMiniGame()
    {
        SceneManager.LoadScene("SampleScene");
    }

    // --- ÉTOILES / UI ---
    void CheckStarThresholds()
    {
        if (starThresholds == null || starThresholds.Length == 0) return;

        for (int i = 0; i < starThresholds.Length; i++)
        {
            if (i >= starGiven.Length) break;
            if (!starGiven[i] && score >= starThresholds[i])
            {
                starGiven[i] = true;
                // Donne la stat (même comportement que les autres mini-jeux)
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.changeStat(StatType.Foi, 5f);
                }
                PlayStarPop(i);
                UpdateStarsUI();
            }
        }
    }

    public void UpdateStarsUI()
    {
        if (starImages == null) return;
        for (int i = 0; i < starImages.Length; i++)
        {
            if (starImages[i] == null) continue;
            bool on = (i < starGiven.Length && starGiven[i]);
            starImages[i].sprite = on ? starOnSprite : starOffSprite;
            starImages[i].color = on ? Color.white : new Color(1f, 1f, 1f, 0.45f);
            // si on veut un "pop" visible pour l'acquisition, garder scale dynamique ; sinon scale = 1
            if (on)
                starImages[i].transform.localScale = Vector3.one;
            else
                starImages[i].transform.localScale = Vector3.one;
        }
    }

    void PlayStarPop(int index)
    {
        if (starImages == null || index < 0 || index >= starImages.Length) return;
        var img = starImages[index];
        if (img == null) return;

        img.DOKill();
        Sequence s = DOTween.Sequence();
        img.sprite = starOnSprite;
        img.transform.localScale = Vector3.zero;
        img.color = Color.white;
        s.Append(img.transform.DOScale(1.4f, 0.28f).SetEase(Ease.OutBack));
        s.Append(img.transform.DOScale(1f, 0.12f).SetEase(Ease.OutBack));
        // petite rotation dynamique
        img.transform.DORotate(new Vector3(0, 0, 20f), 0.35f, RotateMode.Fast).SetLoops(2, LoopType.Yoyo);
        s.Play();
    }

    // --- ANIMATIONS SCORE selon type ---
    void PlayScoreAnimation(EnemyType type, bool positive)
    {
        if (scoreText == null) return;

        scoreText.transform.DOKill();
        scoreText.DOKill();

        Color original = scoreText.color;

        if (positive)
        {
            // couleur selon le type
            Color col = type switch
            {
                EnemyType.Gold => new Color(1f, 0.85f, 0.2f),
                EnemyType.Red => new Color(1f, 0.7f, 0.7f),
                EnemyType.Blue => new Color(0.6f, 0.8f, 1f),
                _ => Color.white
            };

            // punch scale + color flash
            scoreText.transform.localScale = Vector3.one;
            scoreText.transform.DOPunchScale(Vector3.one * 0.22f, 0.35f, 10, 0.6f);
            scoreText.DOColor(col, 0.12f).OnComplete(() => scoreText.DOColor(original, 0.35f));
        }
        else
        {
            // negative feedback: red flash + shake
            scoreText.DOColor(new Color(1f, 0.45f, 0.45f), 0.08f).OnComplete(() => scoreText.DOColor(original, 0.3f));
            scoreText.transform.DOShakePosition(0.35f, new Vector3(10f, 0, 0), 12, 90, false, true);
        }
    }

    public int GetStarCount()
    {
        int count = 0;
        for (int i = 0; i < starGiven.Length; i++)
            if (starGiven[i]) count++;
        return count;
    }
}