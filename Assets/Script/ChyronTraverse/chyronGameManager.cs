using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class chyronGameManager : MonoBehaviour
{
    public float scrollSpeed = 3f;
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

    void Start()
    {
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
        coinText.text = "Pièces : " + coinScore;
    }

    public void HitObstacle()
    {
        if (!isInvincible)
        {
            Vibrate(0.3f, 0.3f, 0.5f);

            // >>> jouer le son de bateau qui craque <<<
            PlayBoatHitSfx();
        }

        if (isInvincible || isGameOver) return;

        currentLives--;

        if (currentLives <= 0)
        {
            GameOver();
            return;
        }

        scrollSpeed -= obstaclePenalty;
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


        // Conversion score -> Or + Foi
        if (GameManager.Instance != null)
        {
       
            float foiGain = finalScore / 700; // à ajuster

            if (coinScore != 0)
                GameManager.Instance.changeStat(StatType.Or, coinScore);
            if (foiGain != 0)
                GameManager.Instance.changeStat(StatType.Foi, foiGain);

            Debug.Log($"[Chyron] Score={finalScore} -> Or +{coinScore}, Foi +{foiGain}");
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
}
