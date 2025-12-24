using UnityEngine;
using XNode;

[CreateAssetMenu(menuName = "Dialogue Graph")]
public class DialogueGraph : NodeGraph
{
    public DialogueNode startNode; // Optionnel : pour désigner le premier nœud du dialogue
}
