using System.Collections.Generic;
using UnityEngine;
using XNode;

[CreateNodeMenu("Dialogue/Node")]
public class DialogueNode : Node
{
    // Port d'entrée (visible)
    [Input] public DialogueNode previousNode;

    [TextArea(2, 6)]
    public string godText;

    [System.Serializable]
    public class PlayerResponse
    {
        public string responseText;
        public ConditionSO[] conditions = new ConditionSO[0];  // Multiple conditions (ALL must be true to make response available)
        public EffectSO[] effects = new EffectSO[0];           // Multiple effects to apply when response is chosen
        public int relationDelta = 0;                          // Impact direct sur la relation du dieu
        // NOTE: le lien suivant n'est pas sérialisé comme champ, les ports sont gérés dynamiquement
        // NOTE: on stocke désormais la référence sérialisée vers le node suivant
        // pour faciliter le runtime (synchronisée avec les ports dynamiques dans l'éditeur).
        public DialogueNode nextNode;
    }

    [SerializeField]
    public PlayerResponse[] responses = new PlayerResponse[0];

    public bool endOfPhase = false;

    // --- DYNAMIQUE : création/suppression de ports ---
    public override object GetValue(NodePort port) => null;

    protected override void Init()
    {
        base.Init();
        UpdateDynamicPorts();
    }

    // Appelé quand une connexion est créée (XNode)
    public override void OnCreateConnection(NodePort from, NodePort to)
    {
        base.OnCreateConnection(from, to);
        UpdateDynamicPorts();
#if UNITY_EDITOR
        // When a connection is created on a dynamic response port (response_X),
        // synchronize the serialized responses[X].nextNode so the runtime can use it.
        try
        {
            NodePort localPort = null;
            NodePort otherPort = null;
            if (from.node == this) { localPort = from; otherPort = to; }
            else if (to.node == this) { localPort = to; otherPort = from; }

            if (localPort != null && localPort.fieldName != null && localPort.fieldName.StartsWith("response_"))
            {
                if (int.TryParse(localPort.fieldName.Replace("response_", ""), out int idx))
                {
                    if (idx >= 0 && idx < responses.Length)
                    {
                        responses[idx].nextNode = otherPort?.node as DialogueNode;
                        UnityEditor.EditorUtility.SetDirty(this);
                    }
                }
            }
        }
        catch { }
#endif
    }

    // Appelé quand une connexion est supprimée (XNode)
    public override void OnRemoveConnection(NodePort port)
    {
        base.OnRemoveConnection(port);
        UpdateDynamicPorts();
#if UNITY_EDITOR
        // When a connection is removed from a dynamic response port, clear the nextNode ref
        try
        {
            if (port != null && port.fieldName != null && port.fieldName.StartsWith("response_"))
            {
                if (int.TryParse(port.fieldName.Replace("response_", ""), out int idx))
                {
                    if (idx >= 0 && idx < responses.Length)
                    {
                        responses[idx].nextNode = null;
                        UnityEditor.EditorUtility.SetDirty(this);
                    }
                }
            }
        }
        catch { }
#endif
    }

    // Met à jour les ports dynamiques "responses 0", "responses 1", ...
    public void UpdateDynamicPorts()
    {
        // Supprime les ports "response_X" qui dépassent le nombre de réponses
        foreach (var p in new List<NodePort>(DynamicPorts))
        {
            if (!p.fieldName.StartsWith("response_")) continue;
            if (!int.TryParse(p.fieldName.Replace("response_", ""), out int idx)) continue;
            if (idx >= responses.Length) RemoveDynamicPort(p);
        }

        // Crée les ports manquants
        for (int i = 0; i < responses.Length; i++)
        {
            string portName = $"response_{i}";
            if (!HasPort(portName))
            {
                // AddDynamicOutput(type, connectionType, typeConstraint, fieldName)
                AddDynamicOutput(typeof(DialogueNode), ConnectionType.Multiple, TypeConstraint.Inherited, portName);
            }
        }
    }

    // helpers pour l'éditeur / runtime
    public void AddResponse()
    {
        var list = new List<PlayerResponse>(responses);
        list.Add(new PlayerResponse { responseText = "Nouvelle réponse" });
        responses = list.ToArray();
        UpdateDynamicPorts();
    }

    public void RemoveLastResponse()
    {
        if (responses.Length == 0) return;
        var list = new List<PlayerResponse>(responses);
        list.RemoveAt(list.Count - 1);
        responses = list.ToArray();
        UpdateDynamicPorts();
    }

    // Remove response at arbitrary index
    public void RemoveResponseAt(int index)
    {
        if (index < 0 || index >= responses.Length) return;
        var list = new List<PlayerResponse>(responses);
        list.RemoveAt(index);
        responses = list.ToArray();
        UpdateDynamicPorts();
    }
}
