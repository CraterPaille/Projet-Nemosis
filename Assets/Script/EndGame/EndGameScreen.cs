using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

public class EndGameScreen : MonoBehaviour
{
    [Header("UI Elements")]
    public CanvasGroup canvasGroup;
    public TMP_Text titleText;
    public TMP_Text completionText;
    public TMP_Text statsText;
    public Button menuButton;
    public Button quitButton;

    [Header("Optional Decoration")]
    public Image decorativeImage;
    public ParticleSystem celebrationParticles;

    [Header("Animation Settings")]
    [Range(0.5f, 3f)]
    public float fadeInDuration = 1f;
    [Range(0.5f, 3f)]
    public float textAnimationDuration = 1.2f;
    [Range(0.5f, 2f)]
    public float buttonPulseScale = 1.1f;
    [Range(0.5f, 3f)]
    public float buttonPulseDuration = 1.5f;

    [Header("Sounds (Optional)")]
    public AudioClip endGameMusic;
    public AudioClip buttonHoverSound;

    private AudioSource audioSource;

    void Start()
    {
        SetupAudio();
        StartCoroutine(PlayEndGameSequence());
    }

    private void SetupAudio()
    {
        if (endGameMusic != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = endGameMusic;
            audioSource.loop = true;
            audioSource.volume = 0f;
            audioSource.Play();
            audioSource.DOFade(0.5f, 2f);
        }
    }

    private IEnumerator PlayEndGameSequence()
    {
        // Initialisation : tout invisible
        canvasGroup.alpha = 0;
        if (titleText != null) titleText.alpha = 0;
        if (completionText != null) completionText.alpha = 0;
        if (statsText != null) statsText.alpha = 0;

        // Initialiser les CanvasGroup des boutons
        if (menuButton != null)
        {
            CanvasGroup menuCG = menuButton.GetComponent<CanvasGroup>();
            if (menuCG == null) menuCG = menuButton.gameObject.AddComponent<CanvasGroup>();
            menuCG.alpha = 0;
        }
        if (quitButton != null)
        {
            CanvasGroup quitCG = quitButton.GetComponent<CanvasGroup>();
            if (quitCG == null) quitCG = quitButton.gameObject.AddComponent<CanvasGroup>();
            quitCG.alpha = 0;
        }

        // 1. Fade in du fond
        canvasGroup.DOFade(1f, fadeInDuration).SetEase(Ease.OutQuad);
        yield return new WaitForSeconds(fadeInDuration * 0.5f);

        // Déterminer si c'est une défaite due à Nemosis >= 100
        bool isDefeatByNemosis = false;
        if (GameManager.Instance != null && GameManager.Instance.Valeurs != null)
        {
            if (GameManager.Instance.Valeurs.TryGetValue(StatType.Nemosis, out float nemosisValue))
            {
                isDefeatByNemosis = nemosisValue >= 100f;
            }
        }

        // 2. Animation du titre principal (cible scale dépend si défaite)
        if (titleText != null)
        {
            if (isDefeatByNemosis)
            {
                titleText.text = "DÉFAITE";
                titleText.color = Color.red;
                titleText.transform.localScale = Vector3.zero;
                titleText.DOFade(1f, 1.0f);
                // plus grand pour bien marquer la défaite
                titleText.transform.DOScale(Vector3.one * 2.5f, textAnimationDuration)
                    .SetEase(Ease.OutElastic, 2f, 1.6f);
                // secousse un peu plus prononcée
                titleText.transform.DOShakeRotation(0.7f, 8f, 12, 90f)
                    .SetDelay(textAnimationDuration);
            }
            else
            {
                titleText.text = "FÉLICITATIONS !";
                titleText.transform.localScale = Vector3.zero;
                titleText.DOFade(1f, 1.3f);
                titleText.transform.DOScale(Vector3.one * 2f, textAnimationDuration)
                    .SetEase(Ease.OutElastic, 2f, 1.6f);

                // Effet de shake subtil
                titleText.transform.DOShakeRotation(0.5f, 5f, 10, 90f)
                    .SetDelay(textAnimationDuration);
            }
        }
        yield return new WaitForSeconds(textAnimationDuration);

        // 3. Texte de complétion (cible scale = 2) — si défaite, expliquer pourquoi
        if (completionText != null)
        {
            if (isDefeatByNemosis)
            {
                completionText.text = "La Nemosis a atteint son maximum — vous avez succombé.";
                completionText.color = Color.red;
            }
            else
            {
                DisplayCompletionPercentage();
            }

            // démarrer légèrement plus petit que la cible (par ex. 1.6) pour un effet d'entrée
            completionText.transform.localScale = Vector3.one * 1.6f;
            completionText.DOFade(1f, 0.5f);
            // scale légèrement plus grand si défaite (pour emphase)
            float targetScale = isDefeatByNemosis ? 2.2f : 2f;
            completionText.transform.DOScale(Vector3.one * targetScale, 0.8f).SetEase(Ease.OutBack);
        }
        yield return new WaitForSeconds(0.6f);

        // 4. Statistiques du jeu (ajout d'une entrée en scale jusqu'à 2)
        if (statsText != null)
        {
            DisplayGameStats();
            statsText.transform.localScale = Vector3.zero;
            statsText.DOFade(1f, 1.8f);
            statsText.transform.DOScale(Vector3.one * 2f, textAnimationDuration).SetEase(Ease.OutBack);
        }
        yield return new WaitForSeconds(0.8f);

        // 5. Particules de célébration — ne pas jouer en cas de défaite
        if (!isDefeatByNemosis && celebrationParticles != null)
        {
            celebrationParticles.Play();
        }

        // 6. Animation des boutons (maintenant identique aux textes: target scale = 2)
        AnimateButtons();

        // 7. Focus accessibilité
        SetAccessibilityFocus();
    }

    private void DisplayCompletionPercentage()
    {
        if (GameManager.Instance == null)
        {
            // Données de test pour l'éditeur
            completionText.text = "Partie complétée à 100%";
            completionText.color = new Color(1f, 0.84f, 0f); // Or
            return;
        }

        int daysCompleted = GameManager.Instance.currentDay;
        int totalDays = GameManager.Instance.totalDays;
        float percentage = (float)daysCompleted / totalDays * 100f;

        completionText.text = $"Partie complétée à {Mathf.RoundToInt(percentage)}%";

        // Couleur basée sur le pourcentage
        if (percentage >= 100f)
            completionText.color = new Color(1f, 0.84f, 0f); // Or
        else if (percentage >= 75f)
            completionText.color = new Color(0.75f, 0.75f, 0.75f); // Argent
        else
            completionText.color = new Color(0.8f, 0.5f, 0.2f); // Bronze
    }

    private void DisplayGameStats()
    {
        string stats = $"<b>Récapitulatif de votre aventure :</b>\n\n";

        if (GameManager.Instance == null)
        {
            // Données de test pour prévisualiser dans l'éditeur
            Debug.LogWarning("[EndGameScreen] GameManager non trouvé, affichage de données de test.");
            stats += "• Jours survécus : 28/28\n";
            stats += "• Population finale : 85\n";
            stats += "• Nourriture restante : 42\n";
            statsText.text = stats;
            return;
        }

        var gm = GameManager.Instance;
        stats += $"• Jours survécus : {gm.currentDay}/{gm.totalDays}\n";

        // Affichage des stats principales
        if (gm.Valeurs.ContainsKey(StatType.Human))
            stats += $"• Population finale : {Mathf.RoundToInt(gm.Valeurs[StatType.Human])}\n";

        if (gm.Valeurs.ContainsKey(StatType.Food))
            stats += $"• Nourriture restante : {Mathf.RoundToInt(gm.Valeurs[StatType.Food])}\n";

        statsText.text = stats;
    }

    private void AnimateButtons()
    {
        Vector3 targetTextScale = Vector3.one * 2f;

        // Animation du bouton menu
        if (menuButton != null)
        {
            CanvasGroup menuCG = menuButton.GetComponent<CanvasGroup>();
            if (menuCG != null)
            {
                // fade in canvas group (alpha 0->1)
                menuCG.DOFade(1f, fadeInDuration).SetEase(Ease.OutQuad);

                // mettre l'échelle initiale à zéro pour reproduire l'entrée des textes
                menuButton.transform.localScale = Vector3.zero;

                // animer vers la même échelle cible que les textes
                menuButton.transform.DOScale(targetTextScale, textAnimationDuration).SetEase(Ease.OutBack);

                // Pulsation continue autour de la scale cible
                menuButton.transform.DOScale(targetTextScale * buttonPulseScale, buttonPulseDuration)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine)
                    .SetDelay(0.5f);

                // Son au survol
                AddHoverSound(menuButton);
            }
        }

        // Animation du bouton quitter
        if (quitButton != null)
        {
            CanvasGroup quitCG = quitButton.GetComponent<CanvasGroup>();
            if (quitCG != null)
            {
                // fade in canvas group (alpha 0->1), léger décalage
                quitCG.DOFade(1f, fadeInDuration).SetEase(Ease.OutQuad).SetDelay(0.15f);

                // mettre l'échelle initiale à zéro pour reproduire l'entrée des textes
                quitButton.transform.localScale = Vector3.zero;

                // animer vers la même échelle cible que les textes (légère delay pour le stagger)
                quitButton.transform.DOScale(targetTextScale, textAnimationDuration).SetEase(Ease.OutBack).SetDelay(0.15f);

                // Pulsation continue autour de la scale cible
                quitButton.transform.DOScale(targetTextScale * buttonPulseScale, buttonPulseDuration)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine)
                    .SetDelay(0.65f);

                AddHoverSound(quitButton);
            }
        }
    }

    private void AddHoverSound(Button button)
    {
        if (buttonHoverSound == null || button == null) return;

        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null) trigger = button.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerEnter;
        entry.callback.AddListener((data) => {
            if (audioSource != null)
                audioSource.PlayOneShot(buttonHoverSound, 0.3f);
        });
        trigger.triggers.Add(entry);
    }

    private void SetAccessibilityFocus()
    {
        if (EventSystem.current != null && menuButton != null)
        {
            EventSystem.current.SetSelectedGameObject(menuButton.gameObject);

            // Animation de focus visuel
            StartCoroutine(HighlightSelectedButton());
        }
    }

    private IEnumerator HighlightSelectedButton()
    {
        while (true)
        {
            GameObject selected = EventSystem.current?.currentSelectedGameObject;
            if (selected != null)
            {
                Image buttonImage = selected.GetComponent<Image>();
                if (buttonImage != null)
                {
                    // Pulse de couleur pour indiquer le focus
                    Color originalColor = buttonImage.color;
                    Color highlightColor = new Color(originalColor.r * 1.2f, originalColor.g * 1.2f, originalColor.b * 1.2f);
                    buttonImage.DOColor(highlightColor, 0.3f).SetLoops(2, LoopType.Yoyo);
                }
            }
            yield return new WaitForSeconds(2f);
        }
    }

    public void ReturnToMainMenu()
    {
        // Animation de sortie
        canvasGroup.DOFade(0f, 0.5f).OnComplete(() => {
            if (audioSource != null)
                audioSource.DOFade(0f, 0.5f);

            UnityEngine.SceneManagement.SceneManager.LoadScene("Menu_principal");
        });
    }

    public void QuitGame()
    {
        canvasGroup.DOFade(0f, 0.5f).OnComplete(() => {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        });
    }

    void OnDestroy()
    {
        // Nettoyage des tweens
        DOTween.Kill(canvasGroup);
        if (titleText != null) DOTween.Kill(titleText.transform);
        if (completionText != null) DOTween.Kill(completionText.transform);
        if (menuButton != null) DOTween.Kill(menuButton.transform);
        if (quitButton != null) DOTween.Kill(quitButton.transform);
    }
}