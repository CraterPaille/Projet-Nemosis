using System.Runtime.ExceptionServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Tooltip")]
    [SerializeField] public GameObject tooltipPanel;
    [SerializeField] private Canvas tooltipCanvas; // Canvas contenant le tooltip
    [SerializeField] private TextMeshProUGUI tooltipTitle;
    [SerializeField] private TextMeshProUGUI tooltipDescription;

    private BuildingData currentTooltipData = null;

    [Header("Interaction Menu")]
    [SerializeField] private GameObject interactionPanel;
    [SerializeField] private GameObject interactionHeader;
    [SerializeField] private Transform interactionContent;
    [SerializeField] private GameObject interactionButtonPrefab;

    [SerializeField] private TextMeshProUGUI RerollText;

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



    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        // Résout automatiquement le Canvas du tooltip si non assigné
        if (tooltipCanvas == null && tooltipPanel != null)
        {
            tooltipCanvas = tooltipPanel.GetComponentInParent<Canvas>(true); // true = inclut les parents inactifs
        }
        tooltipPanel.SetActive(false);
        interactionPanel.SetActive(false);
        villagePanel.SetActive(false);
        dayModeChoicePanel.SetActive(false);
        PanelStats.SetActive(true);
        if (eventPanel != null) eventPanel.SetActive(false);
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
        // Positionne le tooltip à côté de la souris
        if (tooltipPanel.activeSelf && tooltipPanel.activeInHierarchy)
        {
            // Assure que le tooltip est sous un Canvas
            if (tooltipCanvas == null && tooltipPanel != null)
            {
                tooltipCanvas = tooltipPanel.GetComponentInParent<Canvas>(true); // true = inclut les parents inactifs
            }

            // Détermine la caméra à utiliser selon le mode du Canvas
            Camera cam = null;
            if (tooltipCanvas != null && tooltipCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                cam = tooltipCanvas.worldCamera;
            }

            RectTransform tooltipRect = tooltipPanel.GetComponent<RectTransform>();
            RectTransform canvasRect = tooltipCanvas != null ? tooltipCanvas.GetComponent<RectTransform>() : tooltipPanel.transform.parent.GetComponent<RectTransform>();

            if (canvasRect != null && tooltipRect != null)
            {
                Vector2 offset = offsetTooltip;

                if (tooltipCanvas != null && tooltipCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    // Position écran directe pour Screen Space Overlay
                    tooltipPanel.transform.position = Input.mousePosition + new Vector3(offset.x, offset.y, 0f);
                }
                else
                {
                    // Pour Screen Space Camera : convertit la position écran en position locale dans le Canvas
                    Vector2 localPoint;
                    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition, cam, out localPoint))
                    {
                        // Applique l'offset et positionne via anchoredPosition
                        tooltipRect.anchoredPosition = localPoint + offset;
                    }
                }
            }
        }
    }

    public void GameModeChoice()
    {
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
        tooltipPanel.SetActive(true);
        Debug.Log($"[UIManager] ShowBuildingTooltip called for {data.buildingName}");
        
        // Évite de rafraîchir si c'est le même bâtiment
        if (currentTooltipData == data && tooltipPanel != null && tooltipPanel.activeSelf)
            return;

        currentTooltipData = data;
        if (tooltipPanel != null)
        {
            // Récupère le Canvas parent si nécessaire
            if (tooltipCanvas == null)
            {
                tooltipCanvas = tooltipPanel.GetComponentInParent<Canvas>(true); // true = inclut les inactifs
            }
            
            // Active TOUS les parents jusqu'au Canvas (inclus)
            Transform current = tooltipPanel.transform;
            while (current != null)
            {
                if (!current.gameObject.activeSelf)
                {
                    Debug.Log($"[UIManager] Activating parent: {current.name}");
                    current.gameObject.SetActive(true);
                }
                // S'arrête au Canvas
                if (tooltipCanvas != null && current == tooltipCanvas.transform)
                    break;
                current = current.parent;
            }
            
            // S'assure que le Canvas lui-même est actif
            if (tooltipCanvas != null)
            {
                if (!tooltipCanvas.gameObject.activeSelf)
                {
                    Debug.Log($"[UIManager] Activating Canvas: {tooltipCanvas.name}");
                    tooltipCanvas.gameObject.SetActive(true);
                }
                // S'assure que le Canvas component est enabled
                if (!tooltipCanvas.enabled)
                {
                    Debug.Log($"[UIManager] Enabling Canvas component: {tooltipCanvas.name}");
                    tooltipCanvas.enabled = true;
                }
            }
            
            // S'assure que le tooltip est sous le Canvas et au-dessus des autres éléments
            if (tooltipCanvas != null && tooltipPanel.transform.parent != tooltipCanvas.transform)
            {
                tooltipPanel.transform.SetParent(tooltipCanvas.transform, true);
            }
            tooltipPanel.transform.SetAsLastSibling();
            
            // Force les anchors au centre pour que anchoredPosition fonctionne correctement avec ScreenPointToLocalPointInRectangle
            RectTransform tooltipRect = tooltipPanel.GetComponent<RectTransform>();
            if (tooltipRect != null)
            {
                tooltipRect.anchorMin = new Vector2(0.5f, 0.5f);
                tooltipRect.anchorMax = new Vector2(0.5f, 0.5f);
                tooltipRect.pivot = new Vector2(0f, 1f); // Pivot en haut à gauche pour que le tooltip s'affiche en bas à droite de la souris
            }
            
            tooltipPanel.SetActive(true);
            
            Debug.Log($"[UIManager] Tooltip panel activeSelf: {tooltipPanel.activeSelf}, activeInHierarchy: {tooltipPanel.activeInHierarchy}");
        }
        else
        {
            Debug.LogError("[UIManager] tooltipPanel is NULL! Cannot show tooltip!");
            return;
        }
        
        if (tooltipTitle != null)
            tooltipTitle.text = data.buildingName ?? string.Empty;
        else
            Debug.LogError("[UIManager] tooltipTitle is NULL!");
            
        if (tooltipDescription != null)
            tooltipDescription.text = data.description ?? string.Empty;
        else
            Debug.LogError("[UIManager] tooltipDescription is NULL!");
    }

    public void HideTooltip()
    {
        currentTooltipData = null;
        tooltipPanel.SetActive(false);
    }

    public BuildingData GetCurrentTooltipData()
    {
        return currentTooltipData;
    }

    // --- INTERACTIONS ---
    public void ShowInteractionMenu(BuildingData data)
    {
        HideAllUI();
        interactionPanel.SetActive(true);

        interactionHeader.GetComponentInChildren<TextMeshProUGUI>().text = data.buildingName;
        interactionHeader.GetComponentInChildren<Image>().sprite = data.icon;

        // Clear previous buttons
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

    public void HideInteractionMenu()
    {
        interactionPanel.SetActive(false);
    }

    public bool IsInteractionMenuOpen()
    {
        return interactionPanel != null && interactionPanel.activeSelf;
    }

    public void closeInteractionMenu()
    {
        if (GameManager.Instance.currentGameMode == GameManager.GameMode.village) 
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
        villagePanel.SetActive(false);
        //GameManager.Instance.EndHalfDay();
    }

    public void closeVillageUI()
    {
        HideVillageUI();
        GameManager.Instance.EndHalfDay();
    }

    // Les pilier du jeu 

    public void ChangeStatUI(StatType stat, float value)
    {
        switch (stat)
        {
            case StatType.Foi:
                StatsFoi.GetComponentInChildren<TextMeshProUGUI>().text = $" {value}";
                break;
            case StatType.Nemosis:
                StatsNemosis.GetComponentInChildren<TextMeshProUGUI>().text = $" {value}";
                break;
            case StatType.Human:
                StatsHumain.GetComponentInChildren<TextMeshProUGUI>().text = $" {value}";
                break;
            case StatType.Or:
                StatsArgent.GetComponentInChildren<TextMeshProUGUI>().text = $" {value}";
                break;
            case StatType.Food:
                StatsFood.GetComponentInChildren<TextMeshProUGUI>().text = $" {value}";
                break;
            default:
                Debug.LogWarning($"Unknown stat type: {stat}");
                break;
        }
    }

    public void changeDateUI()
    {
        if (GameManager.Instance != null)
        {
            Date.text = $"{GameManager.Instance.currentTime}  {GameManager.Instance.currentWeekDay}  {GameManager.Instance.currentDay}";
        }
    }


    //// Day Mode Choice Menu
    public void DayModeChoice(bool active)
    {
        dayModeChoicePanel.SetActive(active);
    }

    public void VillageCardChoice(VillageCardCollectionSO cardCollection, int cardsToDraw)
    {
        HideAllUI();
        Debug.Log("UIManager: Affichage du choix de cartes de village.");
        PanelStats.SetActive(true);

        interactionPanel.SetActive(true);

        interactionHeader.GetComponentInChildren<TextMeshProUGUI>().text = "Quelle carte jouer ?";
        //interactionHeader.GetComponentInChildren<Button>().interactable = false;
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

    /// <summary>
    /// Cache TOUTE l'UI (utilisé lors du chargement d'un mini-jeu)
    /// </summary>
    public void HideAllUIForMiniGame()
    {
        HideAllUI();
        dayModeChoicePanel.SetActive(false);
        PanelStats.SetActive(false); // Cache aussi les stats
        Date.gameObject.SetActive(false);
        
        // Cache le Canvas entier si besoin
        // GetComponent<Canvas>().enabled = false;
    }

    /// <summary>
    /// Réaffiche l'UI principale (utilisé au retour du mini-jeu)
    /// </summary>
    public void ShowMainUI()
    {
        PanelStats.SetActive(true);
        GameModeChoice();
        dayModeChoicePanel.SetActive(true);
        Date.gameObject.SetActive(true);
        // Réactive le Canvas si vous l'aviez désactivé
        // GetComponent<Canvas>().enabled = true;
    }
}
