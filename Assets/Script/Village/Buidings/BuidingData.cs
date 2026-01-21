using UnityEngine;
using System.Collections.Generic;

public enum BuildingVisualType
{
    Sprite,
    Animation
}

[CreateAssetMenu(fileName = "BuidingData", menuName = "Village/Building")]
public class BuildingData : ScriptableObject
{
    public string buildingName;
    public string description;

    [Header("Visuel du bâtiment")]
    public BuildingVisualType visualType = BuildingVisualType.Sprite;
    
    [Tooltip("Sprite statique (utilisé si visualType = Sprite)")]
    public Sprite icon;
    
    [Tooltip("Sprites de l'animation (utilisé si visualType = Animation)")]
    public Sprite[] animationSprites;
    
    [Tooltip("Vitesse de l'animation en frames par seconde")]
    public float animationFPS = 12f;

    [Header("Dimensions pour placement automatique")]
    public int gridSize = 2;   // Taille du carré (gridSize x gridSize)

    public List<EffectSO> passiveEffects;      // Appliqués automatiquement
    public List<EffectSO> interactionEffects;  // Lorsqu’on clique dessus
}