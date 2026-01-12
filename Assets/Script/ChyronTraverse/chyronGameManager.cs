using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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

    PlayerLaneMovement player;

    private Coroutine vibrationCoroutine;

    // dérivé de la carte
    private float _chaosLevel = 0f;
    private float _rewardMult = 1f;
    private float _rewardFlat = 0f;
    private bool _oneMistakeFail = false;

    void Start()
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
    }

    void Update()
    {
        if (isGameOver) return;

        score += scrollSpeed * Time.deltaTime;
        scoreText.text = "Score : " + Mathf.FloorToInt(score);
        lifeText.text = "Vies : " + currentLives + "/" + maxLives;
        if (coinText != null)
            coinText.text = "Pièces : " + coinScore;
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


        // Conversion score -> Or + Foi avec rewardMultiplier / rewardFlatBonus
        if (GameManager.Instance != null)
        {
            float baseFoi = finalScore / 700f;
            float foiGain = baseFoi * _rewardMult + _rewardFlat;

            float orGain = coinScore * _rewardMult + _rewardFlat;

            if (orGain != 0)
                GameManager.Instance.changeStat(StatType.Or, orGain);
            if (foiGain != 0)
                GameManager.Instance.changeStat(StatType.Foi, foiGain);

            Debug.Log($"[Chyron] Score={finalScore} -> Or +{orGain}, Foi +{foiGain} (mult x{_rewardMult}, flat +{_rewardFlat})");
        }

        SceneManager.LoadScene("SampleScene");

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
