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
    public int bossCurrentHP; // PV du cœur actuel

    public int BossPhase => bossMaxHearts - bossCurrentHearts + 1;
    [Header("HP Bar Sprites")]
    public Sprite hpFull;
    public Sprite hpHalf;
    public Sprite hpLow;
    public Sprite hpEmpty;


    [Header("Boss Hearts UI")]
    public Image[] bossHeartImages; // 6 images à assigner dans l’Inspector
    private Tween[] heartRotateTweens;
    public Sprite heartEmpty;
    public Sprite heartQuarter;
    public Sprite heartHalf;
    public Sprite heartThreeQuarters;
    public Sprite heartFull;

    [Header("UI")]
    public TMP_Text dialogueText;

    public bool isPausedForJewel = false;

    private Coroutine hpAnimCoroutine;

    [Header("SFX")]
    public AudioSource sfxSource;
    public AudioClip sfxDamage;
    public AudioClip sfxHeal;
    public AudioClip sfxAttack;


    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        heartRotateTweens = new Tween[bossMaxHearts];
        playerHP = playerMaxHP;
        bossCurrentHearts = bossMaxHearts;
        bossCurrentHP = bossHeartHP;
        UpdateUI();
        dialogueText.text = "* Chronos t'observe avec un sourire.";
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
        float duration = 0.5f;
        float elapsed = 0f;
        float startFill = (float)fromHP / playerMaxHP;
        float endFill = (float)toHP / playerMaxHP;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Interpolation linéaire
            float currentFill = Mathf.Lerp(startFill, endFill, t);
            int currentHP = Mathf.RoundToInt(Mathf.Lerp(fromHP, toHP, t));

            playerHPBar.fillAmount = currentFill;
            UpdatePlayerHPText(currentHP);

            yield return null;
        }
        // Valeur finale
        playerHPBar.fillAmount = endFill;
        UpdatePlayerHPText(toHP);
        if (playerHP <= playerMaxHP * 0.2f)
        {
            playerHPBar.DOColor(Color.red, 0.2f).SetLoops(-1, LoopType.Yoyo);
        }
        else
        {
            playerHPBar.DOKill();
            playerHPBar.color = Color.white;
        }

        UpdateUIBossHearts(); // Pour garder la logique des cœurs du boss

    }

    // Séparez la mise à jour des cœurs du boss pour éviter de toucher à la barre/text pendant l'animation
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
            sfxSource.pitch = Random.Range(0.85f, 1.25f); // plage de pitch à ajuster selon ton goût
            sfxSource.PlayOneShot(sfxAttack);
            sfxSource.pitch = 1f; // reset pour les autres sons
        }

        bossCurrentHP--;
        if (bossCurrentHP <= 0)
        {
            bossCurrentHearts--;
            bossCurrentHP = bossHeartHP;
            // Changement de phase
            dialogueText.text = $"* Phase {BossPhase} !";
            PlayDialogueEffect();

            // Impact visuel fort
            Camera.main.transform.DOShakePosition(0.3f, 0.3f);
        }
        else
        {
            dialogueText.text = "* Tu enlèves 1/4 de cœur !";
            PlayDialogueEffect();

        }

        if (bossCurrentHearts <= 0)
        {
            dialogueText.text = "*Chronos est vaincu !";
        }

        UpdateUI();
    }

    public void Heal()
    {
        // Exemple : soigne le joueur d’une valeur fixe (ex : 20 HP)
        int healAmount = 20;
        int oldHP = playerHP;

        playerHP = Mathf.Min(playerHP + healAmount, playerMaxHP);

        // Effet visuel
        playerHPBar.DOColor(Color.green, 0.15f)
            .OnComplete(() =>
            {
                playerHPBar.DOColor(Color.white, 0.2f);
            });

        playerHPBar.rectTransform
            .DOScale(1.15f, 0.2f)
            .SetEase(Ease.OutBack)
            .OnPlay(() =>
            {
                sfxSource.PlayOneShot(sfxHeal);
            })
            .OnComplete(() =>
            {
                playerHPBar.rectTransform.DOScale(1f, 0.1f);
            });

        if (hpAnimCoroutine != null)
            StopCoroutine(hpAnimCoroutine);

        hpAnimCoroutine = StartCoroutine(AnimateHPBarAndText(oldHP, playerHP));

        if (playerHPText != null)
        {
            playerHPText.rectTransform
                .DOScale(1.3f, 0.2f)
                .SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    playerHPText.rectTransform.DOScale(1f, 0.1f);
                });
        }

        UpdateHPBar();
        UpdatePlayerHPText();

        UpdateUI();
        dialogueText.text = $"* Tu te soignes de {healAmount} PV !";
        // Ajoute ici ta logique de soin spéciale
    }

    // Exemple pour un effet de heal (retour à Instantiate/Destroy)


    // Appelée quand le joyau est récupéré
    public void OnJewelCollected()
    {
        isPausedForJewel = true;
        // Désactive le contrôleur d'attaque
        var attackController = FindFirstObjectByType<ChronosAttackController>();
        if (attackController != null)
            attackController.enabled = false;

        dialogueText.text = "* Un joyau ! Choisis : Attaquer ou Te soigner.";
        // Ici, affiche les boutons ou options pour le choix (à relier dans l'UI)
    }

    public void ChooseAttack()
    {
        Attack();
        ResumeAttacks();
    }

    public void ChooseHeal()
    {
        Heal();
        ResumeAttacks();
    }

    private void ResumeAttacks()
    {
        isPausedForJewel = false;
        var attackController = FindFirstObjectByType<ChronosAttackController>();
        if (attackController != null)
            attackController.enabled = true; // c'est tout !
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

        // Affiche le texte HP en gros
        UpdatePlayerHPText();


        // Mise à jour des cœurs du boss
        for (int i = 0; i < bossMaxHearts; i++)
        {
            if (i < bossCurrentHearts - 1)
            {
                bossHeartImages[i].sprite = heartFull;
                StartHeartRotation(i);// Cœur plein, démarre la rotation
            }
            else if (i == bossCurrentHearts - 1)
            {
                // Animation DOTween
                bossHeartImages[i].sprite = GetHeartSprite(bossCurrentHP);
                bossHeartImages[i].rectTransform
                .DOScale(1.3f, 0.1f)
                .SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                bossHeartImages[i].rectTransform.DOScale(1f, 0.1f);
                });
                bossHeartImages[i].rectTransform.DOShakeRotation(0.2f, 15f);
                StopHeartRotation(i);// Cœur actuel, arrête la rotation
            }
            else
            {
                bossHeartImages[i].sprite = heartEmpty;
                StopHeartRotation(i);// Cœur vide, arrête la rotation
            }
        }
    }

    void StartHeartRotation(int index)
    {
        // Sécurité : éviter doublons
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

        // Reset rotation
        bossHeartImages[index].rectTransform.rotation = Quaternion.identity;
    }

    void PlayPlayerDamageEffects()
    {
        // Shake léger
        playerHPBar.rectTransform.DOShakePosition( 0.2f, strength: new Vector3(10f, 0, 0), vibrato: 15)
    .OnPlay(() =>
    {
        sfxSource.PlayOneShot(sfxDamage);
    });

        // Flash rouge
        playerHPBar.DOColor(Color.red, 0.1f)
            .OnComplete(() =>
            {
                playerHPBar.DOColor(Color.white, 0.15f);
            });

        if (playerHPText != null)
        {
            playerHPText.rectTransform.DOShakePosition(
                0.3f,
                strength: 8f,
                vibrato: 20
            );
        }
    }

    void PlayDialogueEffect()
    {
        dialogueText.rectTransform.DOKill();

        dialogueText.rectTransform.localScale = Vector3.one * 0.95f;
        dialogueText.rectTransform
            .DOScale(1f, 0.15f)
            .SetEase(Ease.OutBack)
            .OnPlay(() => {sfxSource.PlayOneShot(sfxDamage, 0.3f);});
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
        playerHPBar.rectTransform.DOKill(); // Stop tweens en cours

        if (isDamage)
        {
            // Shake + son
            playerHPBar.rectTransform.DOShakePosition(0.2f, new Vector3(10f, 0, 0), 15);
            sfxSource.PlayOneShot(sfxDamage);
        }
        else
        {
            // Heal avec un pop + son
            playerHPBar.rectTransform
                .DOScale(1.15f, 0.2f)
                .SetEase(Ease.OutBack)
                .OnPlay(() => sfxSource.PlayOneShot(sfxHeal))
                .OnComplete(() => playerHPBar.rectTransform.DOScale(1f, 0.1f));
        }
    }

    private void UpdateHPBar()
    {
        playerHPBar.sprite = GetHPBarSprite(playerHP);

        // Optionnel : si tu veux un fillAmount animé en plus
        float targetFill = (float)playerHP / playerMaxHP;
        playerHPBar.DOFillAmount(targetFill, 0.4f); // animation douce
    }

    private void UpdatePlayerHPText(int hp = -1)
    {
        if (playerHPText != null)
            playerHPText.text = $"{(hp < 0 ? playerHP : hp)} / {playerMaxHP}";
    }
}
