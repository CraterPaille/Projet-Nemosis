using System.Runtime.ExceptionServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

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
    [SerializeField] private GameObject StatsFoi;
    [SerializeField] private GameObject StatsNemosis;
    [SerializeField] private GameObject StatsHumain;
    [SerializeField] private GameObject StatsArgent;
    [SerializeField] private GameObject StatsFood;
    [SerializeField] private TextMeshProUGUI Date;

    [Header("Day Mode Choice UI")]

    [SerializeField] private GameObject dayModeChoicePanel;

    [Header("Village Card Choice UI")]
    //[SerializeField] private Transform cardContainer;
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
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        InitializePanels();

        // S'abonner aux changements de scène
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
    }

    private void EnsureEventSystem()
    {
        // Vérifie qu'un EventSystem existe dans la scène
        if (UnityEngine.EventSystems.EventSystem.current == null)
        {
            Debug.LogWarning("[UIManager] Pas d'EventSystem trouvé, les interactions UI peuvent ne pas fonctionner.");
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Réactiver l'UI uniquement sur la scène principale
        if (scene.name == MAIN_SCENE_NAME)
        {
            SetUIActive(true);
            EnsureEventSystem();
            
            // Si on revient d'un mini-jeu, avancer le temps
            if (returningFromMiniGame)
            {
                returningFromMiniGame = false;
                
                // Avancer le temps d'une demi-journée
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.EndHalfDay();
                }
            }
        }
    }

    /// <summary>
    /// Marque qu'on va lancer un mini-jeu (pour avancer le temps au retour).
    /// </summary>
    public void MarkMiniGameLaunch()
    {
        returningFromMiniGame = true;
    }

    /// <summary>
    /// Active ou désactive le GameObject racine de l'UIManager (et tout son contenu).
    /// </summary>
    public void SetUIActive(bool active)
    {
        gameObject.SetActive(active);
    }

    /// <summary>
    /// Vérifie si les références UI sont valides.
    /// </summary>
    private bool AreReferencesValid()
    {
        return tooltipPanel != null && interactionPanel != null && villagePanel != null && dayModeChoicePanel != null;
    }


    private void Start()
    {
        // Subscribe to events (try common manager singletons)
    
        
        GameEvents.OnDayEnd += changeDateUI;
        GameEvents.OnStatChanged += ChangeStatUI;
        GameEvents.OnMorningEnd += changeDateUI;

    }
    

    private void Update()
    {
        // Vérifier que les références sont valides avant d'accéder
        if (tooltipPanel == null) return;

        // Positionne le tooltip à côté de la souris
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
                
                // Offset fixe en pixels : décale à droite et légèrement en bas
                Vector2 offset = offsetTooltip;
                tooltipRect.anchoredPosition = localPoint + offset;
            }
        }
    }

    public void GameModeChoice()
    {
        if (dayModeChoicePanel != null)
            dayModeChoicePanel.SetActive(true);
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

        // Évite de rafraîchir si c'est le même bâtiment
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

        // Clear previous buttons
        if (interactionContent != null)
        {
            foreach (Transform child in interactionContent)
                Destroy(child.gameObject);

            // Create one button per interaction effect
            foreach (var effect in data.interactionEffects)
            {
                var interactionGO = Instantiate(interactionButtonPrefab, interactionContent);
                var Ui = interactionGO.GetComponent<InteractionEntryUI>();
                Ui.Setup(effect);
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
            // Retour au mode village (UI ou 2D selon la config)
            ShowVillage2DView(); 
        }
        else { HideAllUI(); }
    }

    // --- VILLAGE UI ---
    public void SHowVillageUI()
    {
        HideAllUI();
        if (villagePanel != null)
            villagePanel.SetActive(true);
    }

    // Mode 2D isométrique - cache tout l'UI pour voir le monde
    public void ShowVillage2DView()
    {
        HideAllUI();
        // S'assure que le panel village UI est bien désactivé
        if (villagePanel != null)
            villagePanel.SetActive(false);
    }

    public void HideVillageUI()
    {
        if (villagePanel != null)
            villagePanel.SetActive(false);
        //GameManager.Instance.EndHalfDay();
    }

    public void closeVillageUI()
    {
        HideVillageUI();
        if (GameManager.Instance != null)
            GameManager.Instance.EndHalfDay();
    }

    // Les pilier du jeu 

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


    //// Day Mode Choice Menu
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
        
        // Clear previous buttons
        foreach (Transform child in interactionContent)
            Destroy(child.gameObject);

        // Create one button per interaction effect

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
            var entry = ui.GetComponent<CardUI>(); // Ton script sur le prefab
            entry.Setup(card);
        }
    }

    /// <summary>
    /// Affiche le panel d'événement avec image, titre et description.
    /// </summary>
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

        // Le bouton "Commencer" fermera le panel (le mini-jeu se charge déjà)
        if (eventStartButton != null)
        {
            eventStartButton.onClick.RemoveAllListeners();
            eventStartButton.onClick.AddListener(HideEventPanel);
        }

        Debug.Log($"Panel d'événement affiché : {title}");
    }

    /// <summary>
    /// Masque le panel d'événement.
    /// </summary>
    public void HideEventPanel()
    {
        if (eventPanel != null)
        {
            eventPanel.SetActive(false);
        }
    }


}