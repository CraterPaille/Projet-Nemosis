using System;
using System.Collections.Generic;
using UnityEngine;
using XNode;

public class DialogueRunner : MonoBehaviour
{
    public static DialogueRunner Instance { get; private set; }

    public event Action<DialogueNode, List<int>> OnNodeEnter;
    public event Action OnConversationEnd;

    private DialogueGraph currentGraph;
    private DialogueNode currentNode;
    private GodDataSO currentGod;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartConversation(GodDataSO god, DialogueGraph graph)
    {
        currentGod = god;
        currentGraph = graph;
        if (currentGraph == null)
        {
            Debug.LogError("DialogueRunner: graph is null");
            return;
        }

        // prefer explicit start node if set
        currentNode = currentGraph.startNode;
        if (currentNode == null)
        {
            // fallback: try to find root DialogueNode(s) in the graph via reflection (robust across XNode versions)
            var nodes = GetAllNodesFromGraph(currentGraph);
            // prefer nodes that have no 'previousNode' input connected
            foreach (var n in nodes)
            {
                try
                {
                    var inPort = n.GetInputPort("previousNode");
                    if (inPort == null)
                    {
                        currentNode = n;
                        break;
                    }
                    // try common properties on NodePort
                    bool isConnected = false;
                    try { isConnected = inPort.IsConnected; } catch { /* ignore */ }
                    try
                    {
                        // fallback to ConnectionCount if IsConnected not available
                        var cnt = inPort.ConnectionCount;
                        isConnected = cnt > 0;
                    }
                    catch { }

                    if (!isConnected)
                    {
                        currentNode = n;
                        break;
                    }
                }
                catch { /* ignore and continue */ }
            }
            // if still not found, pick the first node as ultimate fallback
            if (currentNode == null && nodes.Count > 0) currentNode = nodes[0];
        }

        if (currentNode == null)
        {
            Debug.LogError("DialogueRunner: no start node found in graph");
            return;
        }

        EnterNode(currentNode);
    }

    private void EnterNode(DialogueNode node)
    {
        currentNode = node;
        // Determine available responses (simple: condition == null -> available)
        var availableIndices = new List<int>();
        for (int i = 0; i < node.responses.Length; i++)
        {
            var r = node.responses[i];
            if (r == null) continue;
            bool ok = true;
            if (r.condition != null)
            {
                try { ok = r.condition.EvaluateStandalone(); } catch { ok = true; }
            }
            if (ok) availableIndices.Add(i);
        }

        OnNodeEnter?.Invoke(node, availableIndices);
    }

    // Called by UI when player chooses response index i (index into currentNode.responses)
    public void ChooseResponse(int responseIndex)
    {
        if (currentNode == null) return;
        if (responseIndex < 0 || responseIndex >= currentNode.responses.Length) return;
        var response = currentNode.responses[responseIndex];

        // apply effect if any
        if (response.effect != null)
        {
            var eff = response.effect.CreateInstance();
            // many Effect implementations apply themselves immediately in constructor/Activate
        }

        // persist immediate effect on relation if any effect code changes god.relation
        // here we simply mark that the god has been interacted with this day
        if (currentGod != null)
        {
            if (GameManager.Instance != null)
            {
                if (currentGod.lastInteractionDay != GameManager.Instance.currentDay)
                {
                    currentGod.interactionsToday = 0;
                    currentGod.lastInteractionDay = GameManager.Instance.currentDay;
                }
                currentGod.interactionsToday++;
            }
        }

        // If current node is flagged endOfPhase, call EndHalfDay AFTER the choice (as requested)
        if (currentNode.endOfPhase)
        {
            if (GameManager.Instance != null) GameManager.Instance.EndHalfDay();
        }

        // Navigate to next node if available (response.nextNode)
        if (response.nextNode != null)
        {
            EnterNode(response.nextNode);
            return;
        }

        // If there is no next node, do NOT immediately end the conversation.
        // Instead present the player with the current node text and no responses so they
        // must explicitly close the dialogue (via the close button) to finish.
        // This matches the design: "tant que il y a des réponses on continue; sinon on part en appuyant".
        OnNodeEnter?.Invoke(currentNode, new List<int>());
        return;
    }

    public void EndConversation()
    {
        OnConversationEnd?.Invoke();
        currentGraph = null;
        currentNode = null;
        currentGod = null;
    }

    // Use reflection to gather DialogueNode instances stored inside the DialogueGraph asset.
    private List<DialogueNode> GetAllNodesFromGraph(DialogueGraph graph)
    {
        var result = new List<DialogueNode>();
        if (graph == null) return result;

        // Try common property/field names and methods that may expose nodes
        var gType = graph.GetType();

        // 1) try property 'nodes'
        var prop = gType.GetProperty("nodes", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (prop != null)
        {
            var val = prop.GetValue(graph, null) as System.Collections.IEnumerable;
            if (val != null)
            {
                foreach (var o in val)
                    if (o is DialogueNode dn) result.Add(dn);
                if (result.Count > 0) return result;
            }
        }

        // 2) try field 'nodes'
        var field = gType.GetField("nodes", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            var val = field.GetValue(graph) as System.Collections.IEnumerable;
            if (val != null)
            {
                foreach (var o in val)
                    if (o is DialogueNode dn) result.Add(dn);
                if (result.Count > 0) return result;
            }
        }

        // 3) try method GetNodes() or GetAllNodes()
        var getNodesMethod = gType.GetMethod("GetNodes", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                          ?? gType.GetMethod("GetAllNodes", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (getNodesMethod != null)
        {
            var val = getNodesMethod.Invoke(graph, null) as System.Collections.IEnumerable;
            if (val != null)
            {
                foreach (var o in val)
                    if (o is DialogueNode dn) result.Add(dn);
                if (result.Count > 0) return result;
            }
        }

        // 4) last resort: reflect all fields and properties, pick any IEnumerable that contains DialogueNode
        var members = new System.Collections.Generic.List<System.Reflection.MemberInfo>();
        members.AddRange(gType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance));
        members.AddRange(gType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance));
        foreach (var m in members)
        {
            object val = null;
            try
            {
                if (m is System.Reflection.FieldInfo f) val = f.GetValue(graph);
                else if (m is System.Reflection.PropertyInfo p) val = p.GetValue(graph, null);
            }
            catch { continue; }
            if (val is System.Collections.IEnumerable en)
            {
                foreach (var o in en)
                    if (o is DialogueNode dn) result.Add(dn);
                if (result.Count > 0) return result;
            }
        }

        return result;
    }
}
