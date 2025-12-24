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
        if (titleText != null) titleText.text = string.IsNullOrEmpty(response.responseText) ? "(vide)" : response.responseText;
        if (descriptionText != null) descriptionText.text = response.effect != null ? response.effect.effectName : string.Empty;
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
