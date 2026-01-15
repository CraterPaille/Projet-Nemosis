using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NUnit.Framework;

public class ChooseRelationUI : MonoBehaviour
{
    public static ChooseRelationUI Instance;
    [Header("Panel")]
    public GameObject ChooseGodPanel; // root panel to show/hide
    [Header("God list")]
    public Transform godsContainer; // where to instantiate GodCard prefabs
    public GameObject godCardPrefab; // prefab that contains GodCardController

    [Header("God info panel (right side)")]
    public Image godImage;
    public TMP_Text informationText;
    public TMP_Text niveauRelationText;
    public Button toTalkButton;
    public TMP_Text talkButtonText;

    public GodDataSO selectedGod;
    private int NombreDialogues = 0;

    public TMP_Text GodNameText;

    private bool firstOpen = true;
   

    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        ChooseGodPanel.SetActive(false);
        if (godCardPrefab == null) Debug.LogWarning("ChooseRelationUI: godCardPrefab not assigned");
        PopulateGods();
        if (toTalkButton != null) toTalkButton.onClick.AddListener(OnTalkButtonPressed);
    }

    // Open the ChooseRelation panel (make sure the GameObject containing this script is active)
    public void Open()
    {
        ChooseGodPanel.SetActive(true);
        UIManager.Instance.DayModeChoice(false);
        PopulateGods();
        talkButtonText.text = $"Lui parler ({NombreDialogues+1}/3)";
    }

    // Close the ChooseRelation panel
    public void CloseConversation()
    {
        if(NombreDialogues>= 3){OnClosePressed_EndHalfDay(); return;}
        ChooseGodPanel.SetActive(true);
        talkButtonText.text = $"Lui parler ({NombreDialogues+1}/3)";

        //UIManager.Instance.DayModeChoice(true);
    }

    // Called by the UI Close button when the player wants to finish the half-day from the selection screen
    public void OnClosePressed_EndHalfDay()
    {
        // Notify GameManager that the half-day is finished
        if (GameManager.Instance != null) GameManager.Instance.EndHalfDay();
        // Close the panel
        ChooseGodPanel.SetActive(false);
        NombreDialogues = 0;
    }

    public void PopulateGods()
    {   
        if (firstOpen)
        {
            FirstOpen();
        }
        if (GodManager.Instance == null) { Debug.LogWarning("No GodManager found"); return; }
        foreach (Transform t in godsContainer) Destroy(t.gameObject);
        var orderedGods = GodManager.Instance.gods
            .OrderByDescending(g => g.unlocked) // unlocked en premier cela les tire
            .ToList();

        foreach (var god in orderedGods)
        {
            var go = Instantiate(godCardPrefab, godsContainer);
            var ctrl = go.GetComponent<GodCardController>();
            if (ctrl != null) ctrl.Setup(god, OnGodSelected);
        }
    }

    public void FirstOpen()
    {
        firstOpen = false;
        var unlockedGods = new List<GodDataSO>();
        foreach (var god in GodManager.Instance.gods)
        {
            if (god.Is_Unlockable)
                unlockedGods.Add(god);
        }

        for (int i = 0; i < 3 && unlockedGods.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, unlockedGods.Count);
            unlockedGods[randomIndex].unlocked = true;
            unlockedGods.RemoveAt(randomIndex);
        }
    }
    public void OnGodSelected(GodDataSO god)
    {
        selectedGod = god;
        UpdateInfoPanel();
    }

    private void UpdateInfoPanel()
    {
        if (selectedGod == null) return;
        if (godImage != null) godImage.sprite = selectedGod.icon;
        if (informationText != null)
        {
            if (selectedGod.unlocked)
                informationText.text = selectedGod.description;
            else
                informationText.text = selectedGod.unlockDescription;
        } 
        if (niveauRelationText != null) niveauRelationText.text = $"{selectedGod.relation}/100\nNiveau relation";
    }

    public void OnTalkButtonPressed()
    {
        if (selectedGod == null) return;
        if (!selectedGod.unlocked) return;
        GodNameText.text = selectedGod.displayName;
        NombreDialogues++;
        if (selectedGod == null) return;
        ChooseGodPanel.SetActive(false);
        // choose a dialogue graph from tier
        var tier = selectedGod.GetTier();
        List<DialogueGraph> pool = (tier == GodDataSO.RelationTier.Bad) ? selectedGod.badConvos :
                                   (tier == GodDataSO.RelationTier.Good) ? selectedGod.goodConvos : selectedGod.normalConvos;
        if (pool == null || pool.Count == 0)
        {
            Debug.LogWarning("No conversation available for this god in tier");
            return;
        }
        var graph = pool[Random.Range(0, pool.Count)];
        DialogueRunner.Instance.StartConversation(selectedGod, graph);
    }
}
