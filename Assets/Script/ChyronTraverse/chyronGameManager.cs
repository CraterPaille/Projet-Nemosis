using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class chyronGameManager : MonoBehaviour
{
    public float scrollSpeed = 3f;
    private float _baseScrollSpeed;
    public float score = 0f;

    public int maxLives = 3;
    public int currentLives;

    public float invincibilityDuration = 3f;
    public bool isInvincible = false;

    public float obstaclePenalty = 5f;

    public TMP_Text scoreText;
    public TMP_Text lifeText;

    [Header("Pièces")]
    public int coinScore = 0;              // nombre de pièces collectées
    public TMP_Text coinText;              // optionnel : UI pour afficher les pièces

    [Header("SFX")]
    [SerializeField] private AudioSource sfxSource;      // AudioSource pour les SFX du game
    [SerializeField] private AudioClip boatHitClip;      // son de bateau qui craque

    public bool isGameOver = false;

    public SpriteRenderer spriteRenderer;
    PlayerLaneMovement player;

    private Coroutine vibrationCoroutine;

    // dérivé de la carte
    private float _chaosLevel = 0f;
    private float _rewardMult = 1f;
    private float _rewardFlat = 0f;
    private bool _oneMistakeFail = false;

    [Header("Tutorial")]
    public MiniGameTutorialPanel tutorialPanel;
    public VideoClip tutorialClip;
    private bool tutorialValidated = false;

    [Header("Paliers étoiles")]
    public int[] starThresholds = new int[3] { 100, 200, 300 };
    private bool[] starGiven = new bool[3];
    [Header("UI Étoiles")]
    public UnityEngine.UI.Image[] starImages;
    public Sprite starOnSprite;
    public Sprite starOffSprite;

    public bool IsPlaying => tutorialValidated && !isGameOver;

    void Start()
    {
        ShowTutorialAndStart();
        starGiven = new bool[3];
        UpdateStarsUI();
    }

    public void ShowTutorialAndStart()
    {
        tutorialPanel.ShowClick(
            "Chyron",
            tutorialClip
        );
        tutorialPanel.continueButton.onClick.RemoveAllListeners();
        tutorialPanel.continueButton.onClick.AddListener(() => {
            tutorialPanel.Hide();
            tutorialValidated = true;
            StartGameAfterTutorial();
        });
    }

    private void StartGameAfterTutorial()
    {
        _baseScrollSpeed = scrollSpeed;
        ApplyMiniGameCardIfAny();

        currentLives = maxLives;
        player = FindFirstObjectByType<PlayerLaneMovement>();

        // sécurités si jamais l'AudioSource n'est pas assigné
        if (sfxSource == null)
        {
            sfxSource = GetComponent<AudioSource>();
        }
        if (sfxSource != null)
        {
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
        }
        starGiven = new bool[3];
        UpdateStarsUI();

    }

    void Update()
    {
        if (!tutorialValidated) return;
        if (isGameOver) return;

        score += scrollSpeed * Time.deltaTime;
        scoreText.text = "Score : " + Mathf.FloorToInt(score);
        lifeText.text = "Vies : " + currentLives + "/" + maxLives;
        if (coinText != null)
            coinText.text = "Pièces : " + coinScore;
        for (int i = 0; i < starThresholds.Length; i++)
        {
            if (!starGiven[i] && score >= starThresholds[i])
            {
                starGiven[i] = true;
                if (GameManager.Instance != null)
                    GameManager.Instance.changeStat(StatType.Foi, 5f);
            }
        }
        UpdateStarsUI();

    }

    public void HitObstacle()
    {
        if (!isInvincible)
        {
            Vibrate(0.3f, 0.3f, 0.5f);
            PlayBoatHitSfx();
        }

        if (isInvincible || isGameOver) return;

        // --- ONE MISTAKE FAIL ---
        if (_oneMistakeFail)
        {
            Debug.Log("[Chyron] Mode oneMistakeFail : obstacle touché -> GameOver immédiat.");
            GameOver();
            return;
        }

        currentLives--;

        if (currentLives <= 0)
        {
            GameOver();
            return;
        }

        float chaosFactor = 1f + Random.Range(-_chaosLevel, _chaosLevel);
        float actualPenalty = obstaclePenalty * chaosFactor;

        scrollSpeed -= actualPenalty;
        if (scrollSpeed < 0) scrollSpeed = 0;

        StartCoroutine(InvincibilityRoutine());
    }

    private void PlayBoatHitSfx()
    {
        if (sfxSource == null || boatHitClip == null) return;

        // volume actuel de l'AudioSource (tu pourras le lier à un slider plus tard)
        sfxSource.PlayOneShot(boatHitClip);
    }

    void GameOver()
    {
        isGameOver = true;
        scrollSpeed = 0;
        Debug.Log("GAME OVER!");

        int finalScore = Mathf.FloorToInt(score);
        SceneManager.LoadScene("SampleScene");

    }


    public void UpdateStarsUI()
    {
        for (int i = 0; i < starImages.Length; i++)
            starImages[i].sprite = starGiven[i] ? starOnSprite : starOffSprite;
    }

    // invincibilité et clignotement
    System.Collections.IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;

        SpriteRenderer sr = player.spriteRenderer;

        float elapsed = 0f;
        float blinkInterval = 0.15f;

        while (elapsed < invincibilityDuration)
        {
            sr.enabled = !sr.enabled;
            elapsed += blinkInterval;
            yield return new WaitForSeconds(blinkInterval);
        }

        sr.enabled = true;
        isInvincible = false;
    }

    // système de vibration
    public void Vibrate(float left, float right, float duration)
    {
        if (Gamepad.current == null)
            return;

        if (vibrationCoroutine != null)
            StopCoroutine(vibrationCoroutine);

        vibrationCoroutine = StartCoroutine(VibrationRoutine(left, right, duration));
    }

    private System.Collections.IEnumerator VibrationRoutine(float left, float right, float duration)
    {
        Gamepad.current.SetMotorSpeeds(left, right);
        yield return new WaitForSeconds(duration);
        StopVibration();
    }

    private void StopVibration()
    {
        if (Gamepad.current != null)
            Gamepad.current.SetMotorSpeeds(0f, 0f);

        vibrationCoroutine = null;
    }

    public void HealPlayer(int amount)
    {
        if (currentLives >= maxLives) return;

        currentLives += amount;
        if (currentLives > maxLives)
            currentLives = maxLives;

        Debug.Log("Soin +" + amount + " PV, Vies : " + currentLives + "/" + maxLives);
    }

    public void IncreaseMaxHealth(int amount)
    {
        maxLives += amount;
        currentLives += 1;
        if (currentLives > maxLives)
            currentLives = maxLives;

        Debug.Log("Bouclier +" + amount + " PV max, Vies : " + currentLives + "/" + maxLives);
    }

    // --- SCORE PIÈCES ---
    public void AddCoin(int amount)
    {
        coinScore += amount;
    }

    private void ApplyMiniGameCardIfAny()
    {
        var runtime = MiniGameCardRuntime.Instance;
        if (runtime == null || runtime.SelectedCard == null)
            return;

        var card = runtime.SelectedCard;
        if (card.targetMiniGame != MiniGameType.Any && card.targetMiniGame != MiniGameType.Chyron)
            return;

        float speedMult = Mathf.Max(0.1f, card.speedMultiplier);
        scrollSpeed = _baseScrollSpeed * speedMult;

        float diffMult = Mathf.Max(0.5f, card.difficultyMultiplier);

        if (diffMult > 1f)
        {
            maxLives = Mathf.Max(1, Mathf.RoundToInt(maxLives / diffMult));
        }
        else if (diffMult < 1f)
        {
            maxLives = Mathf.RoundToInt(maxLives / diffMult);
        }
        currentLives = maxLives;

        _chaosLevel = Mathf.Clamp01(card.chaosLevel);
        _rewardMult = Mathf.Max(0.1f, card.rewardMultiplier);
        _rewardFlat = card.rewardFlatBonus;
        _oneMistakeFail = card.oneMistakeFail;   // <--- AJOUT

        Debug.Log($"[Chyron] Carte appliquée : {card.cardName}, scroll x{speedMult}, maxLives={maxLives}, chaos={_chaosLevel}, rewardMult={_rewardMult}, rewardFlat={_rewardFlat}, oneMistakeFail={_oneMistakeFail}");

        runtime.Clear();
    }
}
