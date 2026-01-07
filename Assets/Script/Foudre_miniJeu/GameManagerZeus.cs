using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    // Runtime
    private float spawnRate;
    private float currentTime;
    private int enemiesKilled = 0;
    private int enemiesPassed = 0;
    private int score = 0;          // score selon ta règle
    private bool isRunning = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Lance automatiquement le mini-jeu si demandé (utile lorsqu'on charge la scène via SceneManager.LoadScene)
        if (autoStartOnLoad && Application.isPlaying)
        {
            StartGame();
        }
    }

    public void StartGame()
    {
        if (enemyPrefab == null || spawnPoints == null || spawnPoints.Length == 0 || lightningController == null)
        {
            Debug.LogError("GameManagerZeus: Prefabs or references missing! Vérifie enemyPrefab, spawnPoints et lightningController dans l'inspector.");
            return;
        }

        // Choix aléatoire d’un mode météo
        currentWeather = (WeatherMode)Random.Range(0, 4);

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
            yield return new WaitForSeconds(spawnRate);
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
    }

    // Appelé par Enemy.ReachCity()
    public void EnemyReachedCity(EnemyType type)
    {
        enemiesPassed++;

        // Score négatif selon le type
        switch (type)
        {
            case EnemyType.Normal:
            case EnemyType.Blue:
                score -= 1;  // passé -1
                break;
            case EnemyType.Gold:
                score -= 1;
                break;
            case EnemyType.Red:
                score -= 5;  // rouge -5
                break;
        }

        UpdateUI();
    }

    void Update()
    {
        if (!isRunning) return;

        currentTime -= Time.deltaTime;
        if (timerText) timerText.text = "Time: " + Mathf.CeilToInt(currentTime);

        if (currentTime <= 0) EndGame();
    }

    void UpdateUI()
    {
        if (scoreText)
            scoreText.text = $"Tués: {enemiesKilled}  Passés: {enemiesPassed}  Score: {score}";
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
            float humanGain = finalScore / 2; // à ajuster
            if (humanGain != 0)
                GameManager.Instance.changeStat(StatType.Human, humanGain);

            Debug.Log($"[Zeus] Score={finalScore} -> Human +{humanGain}");
        }
        SceneManager.LoadScene("SampleScene");

    }

    public void OnQuitMiniGame()
    {
        SceneManager.LoadScene("SampleScene");
    }
}
