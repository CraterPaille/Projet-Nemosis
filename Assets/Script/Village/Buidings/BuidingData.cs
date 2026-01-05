using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BuidingData", menuName = "Village/Building")]
public class BuildingData : ScriptableObject
{
    public string buildingName;
    public Sprite icon;
    public string description;

    [Header("Dimensions pour placement automatique")]
    public int gridSize = 2;   // Taille du carré (gridSize x gridSize)

    public List<EffectSO> passiveEffects;      // Appliqués automatiquement
    public List<EffectSO> interactionEffects;  // Lorsqu’on clique dessus
}