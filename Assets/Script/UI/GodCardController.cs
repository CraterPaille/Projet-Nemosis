using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;

public class GodCardController : MonoBehaviour, IPointerClickHandler
{
    public Image iconImage;
    public TMP_Text nameText;
    public TMP_Text relationText;
    public Button previewButton;

    private GodDataSO god;
    private Action<GodDataSO> onSelected;

    // Setup without requiring a talkButton. The card itself is clickable (IPointerClickHandler)
    public void Setup(GodDataSO data, Action<GodDataSO> onSelectedCallback)
    {
        god = data;
        onSelected = onSelectedCallback;
        if (nameText != null) nameText.text = god.displayName;
        if (iconImage != null) iconImage.sprite = god.icon;
        if (relationText != null) relationText.text = $"{god.relation}/100";

        if (previewButton != null)
        {
            previewButton.onClick.RemoveAllListeners();
            previewButton.onClick.AddListener(() => ShowPreview());
        }
    }

    private void ShowPreview()
    {
        Debug.Log($"Preview passive for {god.displayName}");
        // TODO: hook with GlobalEffectsPopup UI
    }

    // Called when the user clicks the card (works without an explicit talk button)
    public void OnPointerClick(PointerEventData eventData)
    {
        onSelected?.Invoke(god);
    }
}
