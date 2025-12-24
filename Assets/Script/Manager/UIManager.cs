using System.Runtime.ExceptionServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Tooltip")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TextMeshProUGUI tooltipTitle;
    [SerializeField] private TextMeshProUGUI tooltipDescription;

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



    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        tooltipPanel.SetActive(false);
        interactionPanel.SetActive(false);
        villagePanel.SetActive(false);
        dayModeChoicePanel.SetActive(false);
    }


    private void Start()
    {
        // Subscribe to events (try common manager singletons)
    
        
        GameEvents.OnDayEnd += changeDateUI;
        GameEvents.OnStatChanged += ChangeStatUI;
        GameEvents.OnMorningEnd += changeDateUI;

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
        tooltipTitle.text = data.buildingName;
        tooltipDescription.text = data.description;
    }

    public void HideTooltip()
    {
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

    public void closeInteractionMenu()
    {
        if (GameManager.Instance.currentGameMode == GameManager.GameMode.village) { SHowVillageUI(); }
        else { HideAllUI(); }
    }

    // --- VILLAGE UI ---
    public void SHowVillageUI()
    {
        HideAllUI();
        villagePanel.SetActive(true);
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


}
