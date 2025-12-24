using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BuidingData", menuName = "Village/Building")]
public class BuildingData : ScriptableObject
{
    public string buildingName;
    public Sprite icon;
    public string description;

    public List<EffectSO> passiveEffects;      // Appliqués automatiquement
    public List<EffectSO> interactionEffects;  // Lorsqu’on clique dessus
    public List<EffectSO> upgradeEffects;      // Améliorations
}