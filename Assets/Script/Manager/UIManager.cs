using System.Runtime.ExceptionServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using DG.Tweening;
using Math = System.Math;
using UnityEditor.Localization.Plugins.XLIFF.V20;
using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Tooltip")]
    // Rendu public pour maintenir la compatibilité avec le code existant (VillageManager/Building2D)
    public GameObject tooltipPanel;
    [SerializeField] private TextMeshProUGUI tooltipTitle;
    [SerializeField] private TextMeshProUGUI tooltipDescription;

    private BuildingData currentTooltipData = null;

    [Header("Interaction Menu")]
    [SerializeField] private GameObject interactionPanel;
    [SerializeField] private GameObject interactionHeader;
    [SerializeField] private Transform interactionContent;
    [SerializeField] private GameObject interactionButtonPrefab;
    [SerializeField] public TextMeshProUGUI RerollTxt;
    [SerializeField] private Button CloseInteractionButton;

    [Header("Village UI")]
    [SerializeField] private GameObject villagePanel;
    [SerializeField] private GameObject VillageContent;

    [Header("Stats UI")]
    [SerializeField] private GameObject PanelStats;

    [SerializeField] private GameObject StatFoi;
    [SerializeField] private Sprite[] SpritesFoi;

    [SerializeField] private GameObject StatNemosis;
    [SerializeField] private Sprite[] SpritesNemosis;

    [SerializeField] private GameObject StatHumain;
    [SerializeField] private Sprite[] SpritesHumain;
    [SerializeField] private GameObject StatArgent;
    [SerializeField] private Sprite[] SpritesArgent;

    [SerializeField] private GameObject StatFood;
    [SerializeField] private Sprite[] SpritesFood;
    [SerializeField] private TextMeshProUGUI Date;

    [Header("Day Mode Choice UI")]
    [SerializeField] private GameObject dayModeChoicePanel;
    [SerializeField] private GameObject dayModeFirstSelected; // bouton par défaut à assigner dans l’Inspector

    [Header("Mini-jeu - Cartes")]
    [SerializeField] private GameObject miniJeuCardPanel;
    [SerializeField] private GameObject miniJeuCardFirstSelected;

    [Header("Village Card Choice UI")]
    [SerializeField] private GameObject cardUIPrefab;

    [Header("Event Panel UI")]
    [SerializeField] private GameObject eventPanel;
    [SerializeField] private Image eventImage;
    [SerializeField] private TextMeshProUGUI eventTitle;
    [SerializeField] private TextMeshProUGUI eventDescription;
    [SerializeField] private Button eventStartButton;

    public Vector2 offsetTooltip;

    // Nom de la scène principale où l'UI doit être active
    private const string MAIN_SCENE_NAME = "SampleScene";

    // Flag pour savoir si on revient d'un mini-jeu
    private bool returningFromMiniGame = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializePanels();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void InitializePanels()
    {
        if (tooltipPanel != null) tooltipPanel.SetActive(false);
        if (interactionPanel != null) interactionPanel.SetActive(false);
        if (villagePanel != null) villagePanel.SetActive(false);
        if (dayModeChoicePanel != null) dayModeChoicePanel.SetActive(false);
        if (eventPanel != null) eventPanel.SetActive(false);
        if (miniJeuCardPanel != null) miniJeuCardPanel.SetActive(false);
        if (PanelStats != null) PanelStats.SetActive(true);
    }

    private void EnsureEventSystem()
    {
        if (EventSystem.current == null)
        {
            Debug.LogWarning("[UIManager] Pas d'EventSystem trouvé, les interactions UI peuvent ne pas fonctionner.");
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == MAIN_SCENE_NAME)
        {
            SetUIActive(true);
            EnsureEventSystem();

            if (returningFromMiniGame)
            {
                returningFromMiniGame = false;

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.EndHalfDay();
                }
            }
        }
    }

    /// <summary>Marque qu'on va lancer un mini-jeu (pour avancer le temps au retour).</summary>
    public void MarkMiniGameLaunch()
    {
        returningFromMiniGame = true;
    }

    /// <summary>Active ou désactive le GameObject racine de l'UIManager (et tout son contenu).</summary>
    public void SetUIActive(bool active)
    {
        gameObject.SetActive(active);
        // Ne désactive pas le GameObject principal (le Canvas), mais seulement les panels
        if (dayModeChoicePanel != null) dayModeChoicePanel.SetActive(active);

    }

    private bool AreReferencesValid()
    {
        return tooltipPanel != null && interactionPanel != null && villagePanel != null && dayModeChoicePanel != null;
    }

    private void Start()
    {
        GameEvents.OnDayEnd += changeDateUI;
        GameEvents.OnStatChanged += ChangeStatUI;
        GameEvents.OnMorningEnd += changeDateUI;
    }

    private void Update()
    {
        if (tooltipPanel == null) return;

        if (tooltipPanel.activeSelf)
        {
            RectTransform canvasRect = tooltipPanel.transform.parent.GetComponent<RectTransform>();
            RectTransform tooltipRect = tooltipPanel.GetComponent<RectTransform>();

            if (canvasRect != null && tooltipRect != null)
            {
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect,
                    Input.mousePosition,
                    Camera.main,
                    out localPoint
                );

                Vector2 offset = offsetTooltip;
                tooltipRect.anchoredPosition = localPoint + offset;
            }
        }
    }

    /// <summary>
    /// Retourne les données actuellement affichées par le tooltip (compatibilité)
    /// </summary>
    public BuildingData GetCurrentTooltipData()
    {
        return currentTooltipData;
    }

    /// <summary>Force la sélection d’un élément pour la navigation manette.</summary>
    public void SetDefaultSelected(GameObject toSelect)
    {
        if (EventSystem.current == null || toSelect == null)
            return;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(toSelect);
    }

    public void GameModeChoice()
    {
        if (dayModeChoicePanel != null)
        {
            dayModeChoicePanel.SetActive(true);
            SetDefaultSelected(dayModeFirstSelected);
        }
    }

    public void ShowMiniJeuCardPanel()
    {
        if (miniJeuCardPanel == null) return;

        HideAllUI();
        miniJeuCardPanel.SetActive(true);
        SetDefaultSelected(miniJeuCardFirstSelected);
    }

    public void CloseMiniJeuCardPanelAndBackToModeChoice()
    {
        if (miniJeuCardPanel != null)
            miniJeuCardPanel.SetActive(false);

        GameModeChoice();
    }

    public void HideAllUI()
    {
        HideTooltip();
        HideInteractionMenu();
        HideVillageUI();
        DayModeChoice(false);
    }

    // --- TOOLTIP ---
    public void ShowBuildingTooltip(BuildingData data)
    {
        if (tooltipPanel == null) return;

        if (currentTooltipData == data && tooltipPanel.activeSelf)
            return;

        currentTooltipData = data;
        tooltipPanel.SetActive(true);
        if (tooltipTitle != null) tooltipTitle.text = data.buildingName;
        if (tooltipDescription != null) tooltipDescription.text = data.description;
    }

    public void HideTooltip()
    {
        currentTooltipData = null;
        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);
    }

    // --- INTERACTIONS ---
    public void ShowInteractionMenu(BuildingData data)
    {
        if (interactionPanel == null) return;

        HideAllUI();
        interactionPanel.SetActive(true);

        if (interactionHeader != null)
        {
            var headerText = interactionHeader.GetComponentInChildren<TextMeshProUGUI>();
            if (headerText != null) headerText.text = data.buildingName;

            var headerImage = interactionHeader.GetComponentInChildren<Image>();
            if (headerImage != null) headerImage.sprite = data.icon;
        }

        if (interactionContent != null)
        {
            foreach (Transform child in interactionContent)
                Destroy(child.gameObject);

            foreach (var effect in data.interactionEffects)
            {
                var interactionGO = Instantiate(interactionButtonPrefab, interactionContent);
                var ui = interactionGO.GetComponent<InteractionEntryUI>();
                ui.Setup(effect);
            }
        }
    }

    public void HideInteractionMenu()
    {
        if (interactionPanel != null)
            interactionPanel.SetActive(false);
    }

    public bool IsInteractionMenuOpen()
    {
        return interactionPanel != null && interactionPanel.activeSelf;
    }

    public void closeInteractionMenu()
    {
        if (GameManager.Instance != null && GameManager.Instance.currentGameMode == GameManager.GameMode.village)
        {
            ShowVillage2DView();
        }
        else
        {
            HideAllUI();
        }
    }

    // --- VILLAGE UI ---
    public void SHowVillageUI()
    {
        Debug.Log("UIManager: Affichage de l'UI du village.");
        CloseInteractionButton.onClick.RemoveAllListeners();
        CloseInteractionButton.onClick.AddListener(closeInteractionMenu);
        HideAllUI();
        if (villagePanel != null)
            villagePanel.SetActive(true);
    }

    public void ShowVillage2DView()
    {
        CloseInteractionButton.onClick.RemoveAllListeners();
        CloseInteractionButton.onClick.AddListener(closeInteractionMenu);
        HideAllUI();
        if (villagePanel != null)
            villagePanel.SetActive(false);
    }

    public void HideVillageUI()
    {
        if (villagePanel != null)
            villagePanel.SetActive(false);
    }

    public void closeVillageUI()
    {
        HideVillageUI();
        if (GameManager.Instance != null)
            GameManager.Instance.EndHalfDay();
    }
    #region CHANGE STAT UI
    // --- STATS UI ---
    // Empêche les coroutines de se chevaucher par panel
    private readonly Dictionary<GameObject, Coroutine> activeStatCoroutines = new Dictionary<GameObject, Coroutine>();

    public void ChangeStatUI(StatType stat, float value)
    {
        try
        {
            switch (stat)
            {
                case StatType.Foi:
                    if (StatFoi != null)
                        StartStatCoroutine(StatFoi, value);
                    break;
                case StatType.Nemosis:
                    if (StatNemosis != null)
                        StartStatCoroutine(StatNemosis, value);
                    break;
                case StatType.Human:
                    if (StatHumain != null)
                        StartStatCoroutine(StatHumain, value);
                    break;
                case StatType.Or:
                    if (StatArgent != null)
                        StartStatCoroutine(StatArgent, value);
                    break;
                case StatType.Food:
                    if (StatFood != null)
                        StartStatCoroutine(StatFood, value);
                    break;
            }
        }
        catch (MissingReferenceException)
        {
            Debug.LogWarning("[UIManager] ChangeStatUI appelé mais un objet UI a été détruit.");
        }
    }

    private void StartStatCoroutine(GameObject panel, float targetValue)
    {
        if (panel == null) return;

        var txt = panel.GetComponentInChildren<TextMeshProUGUI>();
        if (txt == null) return;

        int currentVal;
        if (!int.TryParse(txt.text, out currentVal))
        {
            currentVal = 0;
        }

        int difference = (int)targetValue - currentVal;

        // Si aucune variation, on met à jour direct
        if (difference == 0)
        {
            txt.text = $"{(int)targetValue}";
            return;
        }

        // Stop l'ancienne coroutine pour ce panel
        Coroutine existing;
        if (activeStatCoroutines.TryGetValue(panel, out existing) && existing != null)
        {
            StopCoroutine(existing);
        }

        activeStatCoroutines[panel] = StartCoroutine(ChangeStatUICoroutine(panel, difference, (int)targetValue));
    }


    IEnumerator ChangeStatUICoroutine(GameObject PanelStat, int difference, int valueObjectif)
    {
        if (PanelStat != null)
        {
            var txt = PanelStat.GetComponentInChildren<TextMeshProUGUI>();
            RectTransform panelRect = PanelStat.GetComponent<RectTransform>();
            int absDiff = Math.Abs(difference);

            if (txt == null || panelRect == null)
                yield break;

            // Durée totale qui augmente de façon asymptotique (de moins en moins vite)
            float baseDuration = 0.5f;
            float maxAdditionalDuration = 2.5f;
            float scaleFactor = 0.02f;
            float totalDuration = baseDuration + maxAdditionalDuration * (1f - 1f / (1f + absDiff * scaleFactor));

            // Scale qui augmente de façon asymptotique selon la différence
            float baseScale = 0.65f;
            float maxAdditionalScale = 0.2f; // Max +20% (donc 0.65 -> 0.85 max)
            float targetScale = baseScale + maxAdditionalScale * (1f - 1f / (1f + absDiff * scaleFactor));

            // Couleur selon la différence (rouge négatif, vert positif)
            Color originalColor = txt.color;
            Color targetColor = difference < 0 ? new Color(1f, 0.3f, 0.3f) : new Color(0.3f, 1f, 0.3f);

            // Kill les tweens existants sur ce panel
            DOTween.Kill(panelRect);
            DOTween.Kill(txt);

            // Animation de couleur du texte
            txt.DOColor(targetColor, totalDuration * 0.3f).SetEase(Ease.OutQuad);

            // Animation de scale du panel
            panelRect.DOScale(targetScale, totalDuration).SetEase(Ease.InOutQuad);

            // Animation de rotation gauche-droite (shake léger)
            float rotationIntensity = 3f + 5f * (1f - 1f / (1f + absDiff * scaleFactor)); // 3° à 8° selon diff
            panelRect.DORotate(new Vector3(0, 0, rotationIntensity), totalDuration * 0.1f, RotateMode.Fast)
                .SetEase(Ease.InOutQuad)
                .SetLoops(-1, LoopType.Yoyo);

            int startValue = valueObjectif - difference;
            float elapsed = 0f;

            while (elapsed < totalDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / totalDuration);

                // EaseInOutQuad
                float easedT = t < 0.5f
                    ? 2f * t * t
                    : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;

                int currentValue = Mathf.RoundToInt(Mathf.Lerp(startValue, valueObjectif, easedT));
                txt.text = $"{currentValue}";

                yield return null;
            }

            // S'assurer que la valeur finale est exacte
            txt.text = $"{valueObjectif}";

            // Reset des animations
            DOTween.Kill(panelRect);
            txt.DOColor(originalColor, 0.3f).SetEase(Ease.OutQuad);
            panelRect.DOScale(0.65f, 0.3f).SetEase(Ease.OutQuad);
            panelRect.DORotate(Vector3.zero, 0.2f).SetEase(Ease.OutQuad);
        }
    }

    #endregion

    public void changeDateUI()
    {
        if (GameManager.Instance != null && Date != null)
        {
            Date.text = $"{GameManager.Instance.currentTime}  {GameManager.Instance.currentWeekDay}  {GameManager.Instance.currentDay}";
        }
    }

    // --- Day Mode Choice Menu ---
    public void DayModeChoice(bool active)
    {
        if (dayModeChoicePanel != null)
            dayModeChoicePanel.SetActive(active);
    }

    public void VillageCardChoice(VillageCardCollectionSO cardCollection, int cardsToDraw)
    {
        CloseInteractionButton.onClick.RemoveAllListeners();
        CloseInteractionButton.onClick.AddListener(RerollVillageCards);
        if (interactionPanel == null || interactionContent == null) return;
        RerollTxt.text = $"Rerolls : {GameManager.Instance.RerollsRemaining}";
        
        HideAllUI();
        interactionPanel.SetActive(true);

        if (interactionHeader != null)
        {
            var headerText = interactionHeader.GetComponentInChildren<TextMeshProUGUI>();
            if (headerText != null) headerText.text = "Quelle carte jouer ?";
        }

        foreach (Transform child in interactionContent)
            Destroy(child.gameObject);

        List<VillageCard> pool = new List<VillageCard>(cardCollection.allVillageCards);
        List<VillageCard> currentChoices = new List<VillageCard>();
        for (int i = 0; i < cardsToDraw && pool.Count > 0; i++)
        {
            int index = Random.Range(0, pool.Count);
            var card = pool[index];
            pool.RemoveAt(index);
            currentChoices.Add(card);
        }

        GameObject firstButton = null;

        foreach (var card in currentChoices)
        {
            var ui = Instantiate(cardUIPrefab, interactionContent);
            var entry = ui.GetComponent<CardUI>();
            entry.Setup(card);

            if (firstButton == null)
                firstButton = ui;
        }

        // Focus manette sur la première carte
        if (firstButton != null)
            SetDefaultSelected(firstButton);
    }

    public void RerollVillageCards()
    {
        if(GameManager.Instance.RerollsRemaining <= 0)
        {
            Debug.LogWarning("UIManager: Pas de rerolls restants !");
            return;
        }
        GameManager.Instance.RerollsRemaining--;
        VillageCardChoice(GameManager.Instance.villageCardCollection, GameManager.Instance.cardsToDraw);
        RerollTxt.text = $"Rerolls : {GameManager.Instance.RerollsRemaining}";
    }

    // --- PANEL D'ÉVÉNEMENT ---
    public void ShowEventPanel(Sprite image, string title, string description)
    {
        if (eventPanel == null)
        {
            Debug.LogWarning("UIManager: eventPanel n'est pas assigné!");
            return;
        }

        eventPanel.SetActive(true);

        if (eventImage != null) eventImage.sprite = image;
        if (eventTitle != null) eventTitle.text = title;
        if (eventDescription != null) eventDescription.text = description;

        if (eventStartButton != null)
        {
            eventStartButton.onClick.RemoveAllListeners();
            eventStartButton.onClick.AddListener(HideEventPanel);
        }

        Debug.Log($"Panel d'événement affiché : {title}");
    }

    public void HideEventPanel()
    {
        if (eventPanel != null)
        {
            eventPanel.SetActive(false);
        }
    }

    // --- Utilitaires pour mini-jeux (ancienne version conservée) ---
    /// <summary>Cache toute l'UI (utilisé lors du chargement d'un mini-jeu manuel).</summary>
    public void HideAllUIForMiniGame()
    {
        HideAllUI();
        if (dayModeChoicePanel != null) dayModeChoicePanel.SetActive(false);
        if (PanelStats != null) PanelStats.SetActive(false);
        if (Date != null) Date.gameObject.SetActive(false);
    }

    /// <summary>Réaffiche l'UI principale (utilisé au retour d'un mini-jeu manuel).</summary>
    public void ShowMainUI()
    {
        if (PanelStats != null) PanelStats.SetActive(true);
        if (Date != null) Date.gameObject.SetActive(true);
        GameModeChoice();  // -> SetDefaultSelected(dayModeFirstSelected)
    }

    // --- BOUTONS DE SAUVEGARDE / CHARGEMENT ---
    public void OnSaveButtonClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SaveGame();
        }
        else
        {
            Debug.LogWarning("[UIManager] Impossible de sauvegarder : GameManager.Instance est null.");
        }
    }

    public void OnLoadButtonClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadGame();
        }
        else
        {
            Debug.LogWarning("[UIManager] Impossible de charger : GameManager.Instance est null.");
        }
    }
}