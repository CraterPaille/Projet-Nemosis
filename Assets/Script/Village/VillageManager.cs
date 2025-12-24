using UnityEngine;
using System.Collections.Generic;

public class VillageManager : MonoBehaviour
{
    public static VillageManager Instance { get; private set; }
    public GameObject buildingUIPrefab; // Unique pour tous
    public Transform buildingContainer;
    public List<BuildingData> currentBuildings;

    private List<BuildingUI> instantiatedBuildings = new List<BuildingUI>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    public void AfficheBuildings()
    {
        // Nettoie l’ancien UI en parcourant la liste en sens inverse et en évitant les références nulles
        for (int i = instantiatedBuildings.Count - 1; i >= 0; i--)
        {
            var ui = instantiatedBuildings[i];
            if (ui != null && ui.gameObject != null)
                Destroy(ui.gameObject);
        }
        instantiatedBuildings.Clear();

        // Crée les nouveaux UI
        UIManager.Instance.SHowVillageUI();
        foreach (var building in currentBuildings)
        {
            var go = Instantiate(buildingUIPrefab, buildingContainer);
            var ui = go.GetComponent<BuildingUI>();
            ui.Init(building, this);
            instantiatedBuildings.Add(ui);
        }
    }

    public void OnBuildingHovered(BuildingUI building)
    {
        // Affiche info tooltip
        UIManager.Instance.ShowBuildingTooltip(building.Data);
    }

    public void OnBuildingClicked(BuildingUI building)
    {
        // Si interactions : ouvre un panneau d’interaction
        UIManager.Instance.ShowInteractionMenu(building.Data);
        Debug.Log($"Building {building.Data.buildingName} clicked.");
    }
}
