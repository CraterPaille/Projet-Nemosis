using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using DG.Tweening;

public class ChronosGameManager : MonoBehaviour
{
    public static ChronosGameManager Instance;

    [Header("Player")]
    public int playerMaxHP = 92;
    public int playerHP;
    public Image playerHPBar;
    public TMP_Text playerHPText;

    [Header("Boss HP")]
    public int bossMaxHearts = 6;
    public int bossHeartHP = 4;
    public int bossCurrentHearts;
    public int bossCurrentHP;

    public int BossPhase => bossMaxHearts - bossCurrentHearts + 1;

    [Header("HP Bar Sprites")]
    public Sprite hpFull;
    public Sprite hpHalf;
    public Sprite hpLow;
    public Sprite hpEmpty;

    [Header("Boss Hearts UI")]
    public Image[] bossHeartImages;
    private Tween[] heartRotateTweens;
    public Sprite heartEmpty;
    public Sprite heartQuarter;
    public Sprite heartHalf;
    public Sprite heartThreeQuarters;
    public Sprite heartFull;

    [Header("Boss Image (fade)")]
    public Image bossImage; // <-- Référence à l'image du boss à placer dans l'inspecteur

    [Header("UI")]
    public TMP_Text dialogueText;

    public bool isPausedForJewel = false;
    public GameObject gamepadCursor;

    private Coroutine hpAnimCoroutine;

    [Header("SFX")]
    public AudioSource sfxSource;
    public AudioClip sfxDamage;
    public AudioClip sfxHeal;
    public AudioClip sfxAttack;

    // Cache
    private RectTransform playerHPBarRect;
    private RectTransform playerHPTextRect;
    private RectTransform dialogueTextRect;
    private Camera mainCamera;

    // Constantes pré-calculées
    private const float HP_ANIM_DURATION = 0.5f;
    private const float SHAKE_DURATION = 0.2f;
    private const float SCALE_DURATION = 0.2f;
    private const float LOW_HP_THRESHOLD = 0.2f;

    // Colors cache
    private static readonly Color colorRed = Color.red;
    private static readonly Color colorGreen = Color.green;
    private static readonly Color colorWhite = Color.white;

    void Awake()
    {
        Instance = this;

        // Cache RectTransforms
        if (playerHPBar != null)
            playerHPBarRect = playerHPBar.rectTransform;

        if (playerHPText != null)
            playerHPTextRect = playerHPText.rectTransform;

        if (dialogueText != null)
            dialogueTextRect = dialogueText.rectTransform;

        mainCamera = Camera.main;
    }

    void Start()
    {
        heartRotateTweens = new Tween[bossMaxHearts];
        playerHP = playerMaxHP;
        bossCurrentHearts = bossMaxHearts;
        bossCurrentHP = bossHeartHP;
        UpdateUI();
        // Initialiser l'alpha de l'image du boss à pleine opacité
        UpdateBossImageAlpha(instant: true);

        dialogueText.text = "* Chronos t'observe avec un sourire.";
    }

    void OnDestroy()
    {
        // Cleanup tweens
        if (heartRotateTweens != null)
        {
            for (int i = 0; i < heartRotateTweens.Length; i++)
            {
                if (heartRotateTweens[i] != null && heartRotateTweens[i].IsActive())
                    heartRotateTweens[i].Kill();
            }
        }

        if (Instance == this)
            Instance = null;
    }

    public void DamagePlayer(int dmg)
    {
        int oldHP = playerHP;
        playerHP = Mathf.Max(playerHP - dmg, 0);

        PlayHPBarEffect(true);
        PlayPlayerDamageEffects();

        UpdateHPBar();
        UpdatePlayerHPText();

        if (playerHP <= 0)
        {
            dialogueText.text = "* Chronos à gagné. tu es mort.";
            StopAllCoroutines();
        }
    }

    private IEnumerator AnimateHPBarAndText(int fromHP, int toHP)
    {
        float elapsed = 0f;
        float startFill = (float)fromHP / playerMaxHP;
        float endFill = (float)toHP / playerMaxHP;

        while (elapsed < HP_ANIM_DURATION)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / HP_ANIM_DURATION;

            float currentFill = Mathf.Lerp(startFill, endFill, t);
            int currentHP = Mathf.RoundToInt(Mathf.Lerp(fromHP, toHP, t));

            playerHPBar.fillAmount = currentFill;
            UpdatePlayerHPText(currentHP);

            yield return null;
        }

        // Valeur finale
        playerHPBar.fillAmount = endFill;
        UpdatePlayerHPText(toHP);

        // Low HP effect
        if (playerHP <= playerMaxHP * LOW_HP_THRESHOLD)
        {
            playerHPBar.DOColor(colorRed, SHAKE_DURATION).SetLoops(-1, LoopType.Yoyo);
        }
        else
        {
            playerHPBar.DOKill();
            playerHPBar.color = colorWhite;
        }

        UpdateUIBossHearts();
    }

    void UpdateUIBossHearts()
    {
        for (int i = 0; i < bossMaxHearts; i++)
        {
            if (i < bossCurrentHearts - 1)
            {
                bossHeartImages[i].sprite = heartFull;
            }
            else if (i == bossCurrentHearts - 1)
            {
                bossHeartImages[i].sprite = GetHeartSprite(bossCurrentHP);
            }
            else
            {
                bossHeartImages[i].sprite = heartEmpty;
            }
        }
    }

    public void Attack()
    {
        if (bossCurrentHearts <= 0) return;

        if (sfxAttack != null && sfxSource != null)
        {
            sfxSource.pitch = Random.Range(0.85f, 1.25f);
            sfxSource.PlayOneShot(sfxAttack);
            sfxSource.pitch = 1f;
        }

        bossCurrentHP--;

        if (bossCurrentHP <= 0)
        {
            bossCurrentHearts--;
            bossCurrentHP = bossHeartHP;

            dialogueText.text = $"* Phase {BossPhase} !";
            PlayDialogueEffect();

            if (mainCamera != null)
                mainCamera.transform.DOShakePosition(0.3f, 0.3f);
        }
        else
        {
            dialogueText.text = "* Tu enlèves 1/4 de cœur !";
            PlayDialogueEffect();
        }

        // Mise à jour de l'alpha de l'image du boss à chaque coup
        UpdateBossImageAlpha();

        if (bossCurrentHearts <= 0)
        {
            dialogueText.text = "*Chronos est vaincu !";
            // Optionnel : désactiver le raycast ou l'objet une fois invisible
            if (bossImage != null)
                bossImage.raycastTarget = false;
        }

        UpdateUI();
    }

    public void Heal()
    {
        int oldHP = playerHP;
        int healAmount = playerMaxHP - playerHP;

        if (healAmount <= 0)
        {
            dialogueText.text = "* Tu es déjà à pleine santé.";
            PlayDialogueEffect();
            return;
        }

        playerHP = playerMaxHP;

        // Effet visuel optimisé
        playerHPBar.DOColor(colorGreen, 0.15f)
            .OnComplete(() => playerHPBar.DOColor(colorWhite, SHAKE_DURATION));

        if (playerHPBarRect != null)
        {
            playerHPBarRect
                .DOScale(1.15f, SCALE_DURATION)
                .SetEase(Ease.OutBack)
                .OnPlay(() =>
                {
                    if (sfxSource != null && sfxHeal != null)
                        sfxSource.PlayOneShot(sfxHeal);
                })
                .OnComplete(() => playerHPBarRect.DOScale(1f, 0.1f));
        }

        if (hpAnimCoroutine != null)
            StopCoroutine(hpAnimCoroutine);

        hpAnimCoroutine = StartCoroutine(AnimateHPBarAndText(oldHP, playerHP));

        if (playerHPTextRect != null)
        {
            playerHPTextRect
                .DOScale(1.3f, SCALE_DURATION)
                .SetEase(Ease.OutBack)
                .OnComplete(() => playerHPTextRect.DOScale(1f, 0.1f));
        }

        UpdateHPBar();
        UpdatePlayerHPText();
        UpdateUI();

        dialogueText.text = $"* Tu te soignes de {healAmount} PV !";
    }

    public void OnJewelCollected()
    {
        isPausedForJewel = true;

        ChronosAttackController attackController = FindFirstObjectByType<ChronosAttackController>();
        if (attackController != null)
            attackController.enabled = false;

        GameObject playerObj = GameObject.FindGameObjectWithTag("PlayerSoul");
        if (playerObj != null)
        {
            PlayerSoul playerSoul = playerObj.GetComponent<PlayerSoul>();
            if (playerSoul != null)
            {
                playerSoul.ExitJusticeMode();
                playerSoul.SetMovementEnabled(true);
            }
        }

        dialogueText.text = "* Un joyau ! Choisis : Attaquer ou Te soigner.";

        if (gamepadCursor != null)
            gamepadCursor.SetActive(true);
    }

    public void ChooseAttack()
    {
        Attack();
        UnlockPlayerMovement();

        if (gamepadCursor != null)
            gamepadCursor.SetActive(false);

        ResumeAttacks();
    }

    public void ChooseHeal()
    {
        Heal();
        UnlockPlayerMovement();

        if (gamepadCursor != null)
            gamepadCursor.SetActive(false);

        ResumeAttacks();
    }

    private void UnlockPlayerMovement()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("PlayerSoul");
        if (playerObj != null)
        {
            PlayerSoul playerSoul = playerObj.GetComponent<PlayerSoul>();
            if (playerSoul != null)
            {
                playerSoul.ExitJusticeMode();
                playerSoul.SetMovementEnabled(true);
            }

            JusticeShieldController shieldCtrl = playerObj.GetComponent<JusticeShieldController>();
            if (shieldCtrl != null)
                shieldCtrl.DeactivateShields();
        }

        if (gamepadCursor != null)
            gamepadCursor.SetActive(false);
    }

    private void ResumeAttacks()
    {
        isPausedForJewel = false;

        ChronosAttackController attackController = FindFirstObjectByType<ChronosAttackController>();
        if (attackController != null)
            attackController.enabled = true;

        dialogueText.text = "* Les attaques reprennent !";
    }

    private Sprite GetHeartSprite(int value)
    {
        return value switch
        {
            4 => heartFull,
            3 => heartThreeQuarters,
            2 => heartHalf,
            1 => heartQuarter,
            _ => heartEmpty,
        };
    }

    public void UpdateUI()
    {
        playerHPBar.fillAmount = (float)playerHP / playerMaxHP;
        UpdatePlayerHPText();

        // Mise à jour des cœurs du boss
        for (int i = 0; i < bossMaxHearts; i++)
        {
            if (i < bossCurrentHearts - 1)
            {
                bossHeartImages[i].sprite = heartFull;
                StartHeartRotation(i);
            }
            else if (i == bossCurrentHearts - 1)
            {
                bossHeartImages[i].sprite = GetHeartSprite(bossCurrentHP);

                RectTransform heartRect = bossHeartImages[i].rectTransform;
                heartRect
                    .DOScale(1.3f, 0.1f)
                    .SetEase(Ease.OutBack)
                    .OnComplete(() => heartRect.DOScale(1f, 0.1f));

                heartRect.DOShakeRotation(SHAKE_DURATION, 15f);
                StopHeartRotation(i);
            }
            else
            {
                bossHeartImages[i].sprite = heartEmpty;
                StopHeartRotation(i);
            }
        }
    }

    void StartHeartRotation(int index)
    {
        if (heartRotateTweens[index] != null && heartRotateTweens[index].IsActive())
            return;

        RectTransform rt = bossHeartImages[index].rectTransform;

        heartRotateTweens[index] = rt
            .DORotate(new Vector3(0, 0, 180f), 5f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetDelay(2f);
    }

    void StopHeartRotation(int index)
    {
        if (heartRotateTweens[index] != null)
        {
            heartRotateTweens[index].Kill();
            heartRotateTweens[index] = null;
        }

        bossHeartImages[index].rectTransform.rotation = Quaternion.identity;
    }

    void PlayPlayerDamageEffects()
    {
        if (playerHPBarRect != null)
        {
            playerHPBarRect
                .DOShakePosition(SHAKE_DURATION, new Vector3(10f, 0, 0), 15)
                .OnPlay(() =>
                {
                    if (sfxSource != null && sfxDamage != null)
                        sfxSource.PlayOneShot(sfxDamage);
                });
        }

        playerHPBar.DOColor(colorRed, 0.1f)
            .OnComplete(() => playerHPBar.DOColor(colorWhite, 0.15f));

        if (playerHPTextRect != null)
        {
            playerHPTextRect.DOShakePosition(0.3f, 8f, 20);
        }
    }

    void PlayDialogueEffect()
    {
        if (dialogueTextRect == null) return;

        dialogueTextRect.DOKill();
        dialogueTextRect.localScale = Vector3.one * 0.95f;

        dialogueTextRect
            .DOScale(1f, 0.15f)
            .SetEase(Ease.OutBack)
            .OnPlay(() =>
            {
                if (sfxSource != null && sfxDamage != null)
                    sfxSource.PlayOneShot(sfxDamage, 0.3f);
            });
    }

    private Sprite GetHPBarSprite(int currentHP)
    {
        float ratio = (float)currentHP / playerMaxHP;

        if (ratio >= 0.75f)
            return hpFull;
        else if (ratio >= 0.5f)
            return hpHalf;
        else if (ratio > 0f)
            return hpLow;
        else
            return hpEmpty;
    }

    private void PlayHPBarEffect(bool isDamage)
    {
        if (playerHPBarRect == null) return;

        playerHPBarRect.DOKill();

        if (isDamage)
        {
            playerHPBarRect.DOShakePosition(SHAKE_DURATION, new Vector3(10f, 0, 0), 15);

            if (sfxSource != null && sfxDamage != null)
                sfxSource.PlayOneShot(sfxDamage);
        }
        else
        {
            playerHPBarRect
                .DOScale(1.15f, SCALE_DURATION)
                .SetEase(Ease.OutBack)
                .OnPlay(() =>
                {
                    if (sfxSource != null && sfxHeal != null)
                        sfxSource.PlayOneShot(sfxHeal);
                })
                .OnComplete(() => playerHPBarRect.DOScale(1f, 0.1f));
        }
    }

    private void UpdateHPBar()
    {
        playerHPBar.sprite = GetHPBarSprite(playerHP);

        float targetFill = (float)playerHP / playerMaxHP;
        playerHPBar.DOFillAmount(targetFill, 0.4f);
    }

    private void UpdatePlayerHPText(int hp = -1)
    {
        if (playerHPText != null)
            playerHPText.text = $"{(hp < 0 ? playerHP : hp)} / {playerMaxHP}";
    }

    // -----------------------------
    // Nouvelle logique pour l'alpha du boss
    // -----------------------------
    private int TotalBossHP => bossMaxHearts * bossHeartHP;

    private int GetRemainingBossHP()
    {
        if (bossCurrentHearts <= 0) return 0;
        return (bossCurrentHearts - 1) * bossHeartHP + bossCurrentHP;
    }

    private void UpdateBossImageAlpha(bool instant = false)
    {
        if (bossImage == null) return;

        float alpha = (float)GetRemainingBossHP() / Mathf.Max(1, TotalBossHP);
        alpha = Mathf.Clamp01(alpha);

        if (instant)
        {
            Color c = bossImage.color;
            bossImage.color = new Color(c.r, c.g, c.b, alpha);
        }
        else
        {
            bossImage.DOFade(alpha, 0.25f);
        }

        // Si plus de PV, s'assurer que l'image est désactivée/complètement transparente
        if (GetRemainingBossHP() <= 0 && bossImage != null)
        {
            // Garder l'objet actif si vous voulez d'autres animations, sinon désactivez-le:
            // bossImage.gameObject.SetActive(false);
            bossImage.raycastTarget = false;
        }
    }
}