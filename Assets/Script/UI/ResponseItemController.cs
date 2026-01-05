using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResponseItemController : MonoBehaviour
{
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public Button selectButton; // the button on the prefab that triggers selection

    private int responseIndex;

    public void Setup(DialogueNode.PlayerResponse response, int index, bool interactable = true)
    {
        responseIndex = index;
        // Responses no longer use a separate title; show the player's choice in the description area instead.
        if (titleText != null) titleText.text = string.Empty;
        // Show responseText or a list of effects if responseText is empty
        string displayText = response.responseText;
        if (string.IsNullOrEmpty(displayText) && response.effects.Length > 0)
        {
            displayText = "Effets: " + string.Join(", ", System.Array.ConvertAll(response.effects, e => e != null ? e.effectName : "(vide)"));
        }
        if (descriptionText != null) descriptionText.text = string.IsNullOrEmpty(displayText) ? "(vide)" : displayText;
        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(() => OnClicked());
            selectButton.interactable = interactable;
        }
    }

    private void OnClicked()
    {
        DialogueRunner.Instance.ChooseResponse(responseIndex);
    }
}
