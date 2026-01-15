using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections; // Ajoutez en haut

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

    [Header("Boss Hearts UI")]
    public Image[] bossHeartImages; // 6 images à assigner dans l’Inspector
    public Sprite heartEmpty;
    public Sprite heartQuarter;
    public Sprite heartHalf;
    public Sprite heartThreeQuarters;
    public Sprite heartFull;

    [Header("UI")]
    public TMP_Text dialogueText;

    public bool isPausedForJewel = false;

    private Coroutine hpAnimCoroutine;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Les références sont assignées dans l’Inspector, pas via le pool
        playerHP = playerMaxHP;
        bossCurrentHearts = bossMaxHearts;
        bossCurrentHP = bossHeartHP;
        UpdateUI();
        dialogueText.text = "* sans t'observe avec un sourire.";
    }

    public void DamagePlayer(int dmg)
    {
        int oldHP = playerHP;
        playerHP -= dmg;
        if (playerHP < 0) playerHP = 0;

        if (hpAnimCoroutine != null)
            StopCoroutine(hpAnimCoroutine);

        hpAnimCoroutine = StartCoroutine(AnimateHPBarAndText(oldHP, playerHP));

        if (playerHP <= 0)
        {
            dialogueText.text = "* sans gagne. tu es mort.";
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
            if (playerHPText != null)
                playerHPText.text = $"{currentHP} / {playerMaxHP}";

            yield return null;
        }
        // Valeur finale
        playerHPBar.fillAmount = endFill;
        if (playerHPText != null)
            playerHPText.text = $"{toHP} / {playerMaxHP}";

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

        bossCurrentHP--;
        if (bossCurrentHP <= 0)
        {
            bossCurrentHearts--;
            bossCurrentHP = bossHeartHP;
            // Changement de phase
            dialogueText.text = $"* Phase {BossPhase} !";
            // Joue l’animation de cœur perdu ici
        }
        else
        {
            // Joue l’animation de quart de cœur perdu ici
            dialogueText.text = "* Tu enlèves 1/4 de cœur !";
        }

        if (bossCurrentHearts <= 0)
        {
            dialogueText.text = "* Le boss est vaincu !";
            // Animation de victoire
        }

        UpdateUI();
    }

    public void Heal()
    {
        // Exemple : soigne le joueur d’une valeur fixe (ex : 20 HP)
        int healAmount = 20;
        playerHP = Mathf.Min(playerHP + healAmount, playerMaxHP);
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
        switch (value)
        {
            case 4: return heartFull;
            case 3: return heartThreeQuarters;
            case 2: return heartHalf;
            case 1: return heartQuarter;
            default: return heartEmpty;
        }
    }

    void UpdateUI()
    {
        playerHPBar.fillAmount = (float)playerHP / playerMaxHP;

        // Affiche le texte HP en gros
        if (playerHPText != null)
            playerHPText.text = $"{playerHP} / {playerMaxHP}";


        // Mise à jour des cœurs du boss
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
}
