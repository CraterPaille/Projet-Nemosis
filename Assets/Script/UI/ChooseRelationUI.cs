using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
   

    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        if (ChooseGodPanel != null) ChooseGodPanel.SetActive(false);
        if (godCardPrefab == null) Debug.LogWarning("ChooseRelationUI: godCardPrefab not assigned");
        PopulateGods();
        if (toTalkButton != null) toTalkButton.onClick.AddListener(OnTalkButtonPressed);
    }

    // Open the ChooseRelation panel (make sure the GameObject containing this script is active)
    public void Open()
    {
        if (ChooseGodPanel != null) ChooseGodPanel.SetActive(true);
        if (UIManager.Instance != null) UIManager.Instance.DayModeChoice(false);
        PopulateGods();
        if (talkButtonText != null) talkButtonText.text = $"Lui parler ({NombreDialogues+1}/3)";
    }

    // Close the ChooseRelation panel
    public void CloseConversation()
    {
        if(NombreDialogues>= 3){OnClosePressed_EndHalfDay(); return;}
        if (ChooseGodPanel != null) ChooseGodPanel.SetActive(true);
        if (talkButtonText != null) talkButtonText.text = $"Lui parler ({NombreDialogues+1}/3)";

        //UIManager.Instance.DayModeChoice(true);
    }

    // Called by the UI Close button when the player wants to finish the half-day from the selection screen
    public void OnClosePressed_EndHalfDay()
    {
        // Notify GameManager that the half-day is finished
        if (GameManager.Instance != null) GameManager.Instance.EndHalfDay();
        // Close the panel
        if (ChooseGodPanel != null) ChooseGodPanel.SetActive(false);
        NombreDialogues = 0;
    }

    public void PopulateGods()
    {
        if (GodManager.Instance == null) { Debug.LogWarning("No GodManager found"); return; }
        if (godsContainer == null) return;
        foreach (Transform t in godsContainer) Destroy(t.gameObject);
        foreach (var god in GodManager.Instance.gods)
        {
            var go = Instantiate(godCardPrefab, godsContainer);
            var ctrl = go.GetComponent<GodCardController>();
            if (ctrl != null) ctrl.Setup(god, OnGodSelected);
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
        if (informationText != null) informationText.text = "Information et passif"; // TODO: show real passive/effect info
        if (niveauRelationText != null) niveauRelationText.text = $"{selectedGod.relation}/100\nNiveau relation";
    }

    public void OnTalkButtonPressed()
    {
        NombreDialogues++;
        if (selectedGod == null) return;
        if (ChooseGodPanel != null) ChooseGodPanel.SetActive(false);
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
        if (DialogueRunner.Instance != null)
            DialogueRunner.Instance.StartConversation(selectedGod, graph);
    }
}
