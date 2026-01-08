using System.Collections.Generic;
using UnityEngine;
using XNode;

[CreateAssetMenu(menuName = "Relation/GodData")]
public class GodDataSO : ScriptableObject
{
    public string id;
    public string displayName;
    public Sprite icon;
    public bool unlocked = false;
    public int relation = 0;
    // Interaction tracking
    public float lastInteractionDay = 0;
    public int interactionsToday = 0;

    [Header("Dialogues (petits graphs)")]
    public List<DialogueGraph> badConvos = new List<DialogueGraph>();
    public List<DialogueGraph> normalConvos = new List<DialogueGraph>();
    public List<DialogueGraph> goodConvos = new List<DialogueGraph>();
    public List<DialogueGraph> specialConvos = new List<DialogueGraph>();

    [Header("Relation thresholds (optional)")]
    public int badThreshold = -20;
    public int goodThreshold = 20;

    // helper
    public enum RelationTier { Bad, Normal, Good }

    public RelationTier GetTier()
    {
        if (relation <= badThreshold) return RelationTier.Bad;
        if (relation >= goodThreshold) return RelationTier.Good;
        return RelationTier.Normal;
    }
}
