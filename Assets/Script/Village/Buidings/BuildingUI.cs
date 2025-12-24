using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class BuildingUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [HideInInspector] public BuildingData Data;
    private VillageManager manager;

    [SerializeField] private Sprite icon;
    [SerializeField] private Text nameText;

    // Initialize the Building UI with data
    [SerializeField] private Image iconImage;
    [SerializeField]private TextMeshProUGUI buildingNameText;

    public void Init(BuildingData data, VillageManager manager)
    {
        this.Data = data;
        this.manager = manager;
        Debug.Log($"[BuildingUI] Initializing UI for building: {data.buildingName} he has icon: {data.icon}");
        this.icon = data.icon;
        if (iconImage != null)
        {
            iconImage.sprite = icon;
        }
        if (buildingNameText != null)
        {
            buildingNameText.text = data.buildingName;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log($"Building {Data.buildingName} hovered.");
        manager.OnBuildingHovered(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UIManager.Instance.HideTooltip();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        manager.OnBuildingClicked(this);
    }

    public void OnClick()
    {
        manager.OnBuildingClicked(this);
    }
}
