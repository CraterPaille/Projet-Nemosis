using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class BuildingPlacement
{
    public BuildingData buildingData;
    public Vector3 worldPosition; // Position dans le monde
}

public class VillageManager : MonoBehaviour
{
    public static VillageManager Instance { get; private set; }
    
    [Header("UI Mode (ancien système)")]
    public GameObject buildingUIPrefab;
    public Transform buildingContainer;
    
    [Header("2D Iso Mode (nouveau système)")]
    public GameObject building2DPrefab; // Le prefab avec Building2D script
    public Transform buildingsParent; // Parent pour organiser la hiérarchie
    public GameObject villageGrid; // La grille à activer/désactiver
    public List<BuildingPlacement> buildingPlacements; // Liste des bâtiments avec positions

    public GameObject CloseButton;

    [Header("Fallback (si buildingPlacements vide)")]
    public List<BuildingData> currentBuildings;

    private List<BuildingUI> instantiatedBuildingsUI = new List<BuildingUI>();
    private List<Building2D> instantiatedBuildings2D = new List<Building2D>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        CloseButton.SetActive(false);
        villageGrid.SetActive(false);
    }
    public void AfficheBuildings()
    {
        // Mode 2D Isométrique
        if (building2DPrefab != null && buildingPlacements != null && buildingPlacements.Count > 0)
        {
            AfficheBuildings2D();
        }
        // Mode UI (ancien système, fallback)
        else if (buildingUIPrefab != null && currentBuildings != null && currentBuildings.Count > 0)
        {
            AfficheBuildingsUI();
        }
        else
        {
            Debug.LogWarning("[VillageManager] Aucun b�timent configur� !");
        }
    }

    private void AfficheBuildingsUI()
    {
        // Nettoie l'ancien UI
        for (int i = instantiatedBuildingsUI.Count - 1; i >= 0; i--)
        {
            var ui = instantiatedBuildingsUI[i];
            if (ui != null && ui.gameObject != null)
                Destroy(ui.gameObject);
        }
        instantiatedBuildingsUI.Clear();
        // Désactive la grille et le contrôle caméra
        if (villageGrid != null)
            villageGrid.SetActive(false);

        // Crée les nouveaux UI
        UIManager.Instance.SHowVillageUI();
        foreach (var building in currentBuildings)
        {
            var go = Instantiate(buildingUIPrefab, buildingContainer);
            var ui = go.GetComponent<BuildingUI>();
            ui.Init(building, this);
            instantiatedBuildingsUI.Add(ui);
        }
    }

    private void AfficheBuildings2D()
    {
        // Nettoie les anciens bâtiments 2D
        CloseButton.SetActive(true);
        villageGrid.SetActive(true);
        for (int i = instantiatedBuildings2D.Count - 1; i >= 0; i--)
        {
            var building = instantiatedBuildings2D[i];
            if (building != null && building.gameObject != null)
                Destroy(building.gameObject);
        }
        instantiatedBuildings2D.Clear();

        // Active la vue 2D (cache les panels UI)
        UIManager.Instance.ShowVillage2DView();

        // Active la grille et le contrôle caméra
        // Active la grille
        if (villageGrid != null)
            villageGrid.SetActive(true);
        // Instancie chaque b�timent � sa position
        foreach (var placement in buildingPlacements)
        {
            if (placement.buildingData == null)
            {
                Debug.LogWarning("[VillageManager] BuildingPlacement avec buildingData null !");
                continue;
            }

            // Instancie le prefab
            GameObject go = Instantiate(building2DPrefab, placement.worldPosition, Quaternion.identity, buildingsParent);
            go.name = $"Building_{placement.buildingData.buildingName}";

            // Init le script
            var building2D = go.GetComponent<Building2D>();
            if (building2D != null)
            {
                building2D.Init(placement.buildingData, this);
                instantiatedBuildings2D.Add(building2D);
            }
            else
            {
                Debug.LogError($"[VillageManager] Le prefab {building2DPrefab.name} n'a pas de composant Building2D !");
            }
        }

        Debug.Log($"[VillageManager] {instantiatedBuildings2D.Count} bâtiments 2D instanciés.");
    }

    public void CloseVillage()
    {
        // Nettoie les bâtiments 2D
        for (int i = instantiatedBuildings2D.Count - 1; i >= 0; i--)
        {
            var building = instantiatedBuildings2D[i];
            if (building != null && building.gameObject != null)
                Destroy(building.gameObject);
        }
        instantiatedBuildings2D.Clear();

        // Désactive la grille et le bouton
        if (villageGrid != null)
            villageGrid.SetActive(false);
        if (CloseButton != null)
            CloseButton.SetActive(false);

        // Retour au choix de mode
        UIManager.Instance.HideAllUI();
        GameManager.Instance.EndHalfDay();
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
    // Overload pour Building2D
    public void OnBuildingHovered(Building2D building)
    {
        UIManager.Instance.ShowBuildingTooltip(building.GetData());
    }

    public void OnBuildingClicked(Building2D building)
    {
        UIManager.Instance.ShowInteractionMenu(building.GetData());
        Debug.Log($"Building {building.GetData().buildingName} clicked.");
    }
}

