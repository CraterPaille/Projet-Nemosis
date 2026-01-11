using System.Runtime.ExceptionServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Tooltip")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TextMeshProUGUI tooltipTitle;
    [SerializeField] private TextMeshProUGUI tooltipDescription;

    private BuildingData currentTooltipData = null;

    [Header("Interaction Menu")]
    [SerializeField] private GameObject interactionPanel;
    [SerializeField] private GameObject interactionHeader;
    [SerializeField] private Transform interactionContent;
    [SerializeField] private GameObject interactionButtonPrefab;

    [Header("Village UI")]
    [SerializeField] private GameObject villagePanel;
    [SerializeField] private GameObject VillageContent;

    [Header("Stats UI")]
    [SerializeField] private GameObject PanelStats;
    [SerializeField] private GameObject StatsFoi;
    [SerializeField] private GameObject StatsNemosis;
    [SerializeField] private GameObject StatsHumain;
    [SerializeField] private GameObject StatsArgent;
    [SerializeField] private GameObject StatsFood;
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

    /// <summary>Force la sélection d’un élément pour la navigation manette.</summary>
    private void SetDefaultSelected(GameObject toSelect)
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
        HideAllUI();
        if (villagePanel != null)
            villagePanel.SetActive(true);
    }

    public void ShowVillage2DView()
    {
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

    // --- STATS UI ---
    public void ChangeStatUI(StatType stat, float value)
    {
        try
        {
            switch (stat)
            {
                case StatType.Foi:
                    if (StatsFoi != null)
                    {
                        var txt = StatsFoi.GetComponentInChildren<TextMeshProUGUI>();
                        if (txt != null) txt.text = $" {value}";
                    }
                    break;
                case StatType.Nemosis:
                    if (StatsNemosis != null)
                    {
                        var txt = StatsNemosis.GetComponentInChildren<TextMeshProUGUI>();
                        if (txt != null) txt.text = $" {value}";
                    }
                    break;
                case StatType.Human:
                    if (StatsHumain != null)
                    {
                        var txt = StatsHumain.GetComponentInChildren<TextMeshProUGUI>();
                        if (txt != null) txt.text = $" {value}";
                    }
                    break;
                case StatType.Or:
                    if (StatsArgent != null)
                    {
                        var txt = StatsArgent.GetComponentInChildren<TextMeshProUGUI>();
                        if (txt != null) txt.text = $" {value}";
                    }
                    break;
                case StatType.Food:
                    if (StatsFood != null)
                    {
                        var txt = StatsFood.GetComponentInChildren<TextMeshProUGUI>();
                        if (txt != null) txt.text = $" {value}";
                    }
                    break;
            }
        }
        catch (MissingReferenceException)
        {
            Debug.LogWarning("[UIManager] ChangeStatUI appelé mais un objet UI a été détruit.");
        }
    }

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
        if (interactionPanel == null || interactionContent == null) return;

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

        foreach (var card in currentChoices)
        {
            var ui = Instantiate(cardUIPrefab, interactionContent);
            var entry = ui.GetComponent<CardUI>();
            entry.Setup(card);
        }
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
        GameModeChoice();
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