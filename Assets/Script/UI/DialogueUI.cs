using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject panel; // root panel to show/hide
    public TMP_Text godText;
    public Transform responsesContainer; // parent for response prefabs
    public GameObject responseItemPrefab; // prefab that contains ResponseItemController
    public Button closeButton;
    public Image GodIcon;

    private DialogueNode currentNode;

    void Start()
    {
        panel.SetActive(false);
        
        // Se désabonner d'abord au cas où on se réabonnerait (redémarrage de scène)
        if (DialogueRunner.Instance != null)
        {
            DialogueRunner.Instance.OnNodeEnter -= OnNodeEnter;
            DialogueRunner.Instance.OnConversationEnd -= OnConversationEnd;
        }
        
        // Puis se réabonner
        DialogueRunner.Instance.OnNodeEnter += OnNodeEnter;
        DialogueRunner.Instance.OnConversationEnd += OnConversationEnd;
        
        if (closeButton != null) closeButton.onClick.AddListener(() => DialogueRunner.Instance.EndConversation());
    }

    void OnDestroy()
    {
        if (DialogueRunner.Instance != null)
        {
            DialogueRunner.Instance.OnNodeEnter -= OnNodeEnter;
            DialogueRunner.Instance.OnConversationEnd -= OnConversationEnd;
        }
    }

    private void OnNodeEnter(DialogueNode node, List<int> availableResponseIndices)
    {
        panel.SetActive(true);
        currentNode = node;
        godText.text = node.godText;
        
        // Assigner l'icône du dieu si disponible
        if (ChooseRelationUI.Instance != null && ChooseRelationUI.Instance.selectedGod != null)
        {
            GodIcon.sprite = ChooseRelationUI.Instance.selectedGod.icon;
        }

        // clear old items
        foreach (Transform t in responsesContainer) 
        {
            Destroy(t.gameObject);
        }

        // create response prefab instances
        for (int i = 0; i < availableResponseIndices.Count; i++)
        {
            int responseIndex = availableResponseIndices[i];
            if (responseItemPrefab == null)
            {
                Debug.LogError("DialogueUI: responseItemPrefab is not assigned.");
                break;
            }
            
            var go = Instantiate(responseItemPrefab, responsesContainer);
            go.SetActive(true);
            
            var ctrl = go.GetComponent<ResponseItemController>();
            if (ctrl != null)
            {
                ctrl.Setup(node.responses[responseIndex], responseIndex, true);
            }
            else
            {
                // fallback: try to find a TMP child and set text
                var t = go.GetComponentInChildren<TMP_Text>();
                if (t != null) t.text = node.responses[responseIndex].responseText;
            }
        }
        
        // Force le layout du Canvas à se recalculer
        Canvas.ForceUpdateCanvases();
        var layoutGroup = responsesContainer.GetComponent<LayoutGroup>();
        if (layoutGroup != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(responsesContainer as RectTransform);
        }
    }

    private void OnResponseClicked(int responseIndex)
    {
        DialogueRunner.Instance.ChooseResponse(responseIndex);
    }

    private void OnConversationEnd()
    {
        panel.SetActive(false);
    }
}
