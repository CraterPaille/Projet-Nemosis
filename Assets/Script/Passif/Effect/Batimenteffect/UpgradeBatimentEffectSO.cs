using UnityEngine;

[CreateAssetMenu(menuName = "Effects/Batiments/UpgradeBatiment")]
public class UpgradeBatimentEffectSO : EffectSO
{
    public BuildingData UpgradedBuilding;

    public override Effect CreateInstance()
    {
        return new Effect_UpgradeBatiment(this);
    }
}

public class Effect_UpgradeBatiment : Effect
{
    private UpgradeBatimentEffectSO soData;

    public Effect_UpgradeBatiment(UpgradeBatimentEffectSO soData) : base(soData)
    {
        this.soData = soData;
    }
    public override void Activate()
    {
        if (IsActive) return;
        IsActive = true;
        if (VillageManager.Instance == null) return;
        if (VillageManager.Instance.buildingClicked == null) return;
        BuildingData buildingToUpgrade = VillageManager.Instance.buildingClicked;
        VillageManager.Instance.buildingClicked = null;
        UIManager.Instance.HideInteractionMenu();
        VillageManager.Instance.RemoveBuilding(buildingToUpgrade);
        VillageManager.Instance.AddBuilding(soData.UpgradedBuilding);
        VillageManager.Instance.AfficheBuildings2D();
        DestroySelf();
    }
    public override void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
        return;
    }

}
