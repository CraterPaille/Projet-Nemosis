using System.Runtime.ExceptionServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
<<<<<<< Updated upstream

=======
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using DG.Tweening;
using Math = System.Math;
using UnityEditor.Localization.Plugins.XLIFF.V20;
using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
>>>>>>> Stashed changes
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
    [SerializeField] private TextMeshProUGUI RerollTxt;
    [SerializeField] private Button CloseInteractionButton;

    [Header("Village UI")]
    [SerializeField] private GameObject villagePanel;
    [SerializeField] private GameObject VillageContent;

    [Header("Stats UI")]
    [SerializeField] private GameObject PanelStats;

<<<<<<< Updated upstream
    [SerializeField] private GameObject StatsFoi;
    [SerializeField] private GameObject StatsNemosis;
    [SerializeField] private GameObject StatsHumain;
    [SerializeField] private GameObject StatsArgent;
    [SerializeField] private GameObject StatsFood;
=======
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
>>>>>>> Stashed changes
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
        // Évite de rafraîchir si c'est le même bâtiment
        if (currentTooltipData == data && tooltipPanel.activeSelf)
            return;

        currentTooltipData = data;
        tooltipPanel.SetActive(true);
        tooltipTitle.text = data.buildingName;
        tooltipDescription.text = data.description;
    }

    public void HideTooltip()
    {
        currentTooltipData = null;
        tooltipPanel.SetActive(false);
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
        Debug.Log("UIManager: Affichage de l'UI du village.");
        CloseInteractionButton.onClick.RemoveAllListeners();
        CloseInteractionButton.onClick.AddListener(closeInteractionMenu);
        HideAllUI();
        villagePanel.SetActive(true);
    }

    // Mode 2D isométrique - cache tout l'UI pour voir le monde
    public void ShowVillage2DView()
    {
        CloseInteractionButton.onClick.RemoveAllListeners();
        CloseInteractionButton.onClick.AddListener(closeInteractionMenu);
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
<<<<<<< Updated upstream

    // Les pilier du jeu 
=======
    #region CHANGE STAT UI
    // --- STATS UI ---
    // Empêche les coroutines de se chevaucher par panel
    private readonly Dictionary<GameObject, Coroutine> activeStatCoroutines = new Dictionary<GameObject, Coroutine>();
>>>>>>> Stashed changes

    public void ChangeStatUI(StatType stat, float value)
    {
        switch (stat)
        {
<<<<<<< Updated upstream
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
=======
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
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
=======
        CloseInteractionButton.onClick.RemoveAllListeners();
        CloseInteractionButton.onClick.AddListener(RerollVillageCards);
        if (interactionPanel == null || interactionContent == null) return;
        RerollTxt.text = $"Rerolls : {GameManager.Instance.RerollsRemaining}";
        
>>>>>>> Stashed changes
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

<<<<<<< Updated upstream
    /// <summary>
    /// Affiche le panel d'événement avec image, titre et description.
    /// </summary>
=======
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
>>>>>>> Stashed changes
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
