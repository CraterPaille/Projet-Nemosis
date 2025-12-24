#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using XNodeEditor;
using XNode;

[CustomNodeEditor(typeof(DialogueNode))]
public class DialogueNodeEditor : NodeEditor
{
    SerializedProperty godTextProp;
    SerializedProperty responsesProp;
    SerializedProperty endOfPhaseProp;

    // Note: Some versions of the XNode NodeEditor do not expose OnEnable as overrideable.
    // Initialize SerializedProperty fields lazily in OnBodyGUI to avoid override issues.

    public override void OnBodyGUI()
    {
        // IMPORTANT : update serializedObject (layout safe)
        serializedObject.Update();

        // lazy init SerializedProperty (avoid overriding OnEnable which may not exist)
        if (godTextProp == null)
        {
            godTextProp = serializedObject.FindProperty("godText");
            responsesProp = serializedObject.FindProperty("responses");
            endOfPhaseProp = serializedObject.FindProperty("endOfPhase");
        }

        DialogueNode node = target as DialogueNode;

        // --- PORT d'entrée (previousNode) visible en haut ---
        NodePort inPort = node.GetInputPort("previousNode");
        if (inPort != null) NodeEditorGUILayout.PortField(new GUIContent("Entrée"), inPort);

        EditorGUILayout.Space(6);

        // Texte du dieu
        EditorGUILayout.PropertyField(godTextProp, new GUIContent("Texte du dieu"));

        EditorGUILayout.Space(8);

        // Responses array : affichage custom + port dynamique par élément
        EditorGUILayout.LabelField("Réponses", EditorStyles.boldLabel);

        if (responsesProp != null)
        {
            EditorGUI.indentLevel++;
            // defer deletion to avoid breaking layout (Begin/End mismatch)
            int removeIndex = -1;
            int count = responsesProp.arraySize;
            for (int i = 0; i < count; i++)
            {
                SerializedProperty responseProp = responsesProp.GetArrayElementAtIndex(i);
                SerializedProperty responseTextProp = responseProp.FindPropertyRelative("responseText");
                SerializedProperty conditionProp = responseProp.FindPropertyRelative("condition");
                SerializedProperty effectProp = responseProp.FindPropertyRelative("effect");

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Réponse #{i + 1}", GUILayout.MaxWidth(120));
                if (GUILayout.Button("–", GUILayout.Width(24)))
                {
                    // mark for removal after layout is finished
                    removeIndex = i;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.PropertyField(responseTextProp, new GUIContent("Texte"));
                EditorGUILayout.PropertyField(conditionProp, new GUIContent("Condition (SO)"));
                EditorGUILayout.PropertyField(effectProp, new GUIContent("Effet (SO)"));

                // Affiche le port dynamique correspondant (response_i)
                string portName = $"response_{i}";
                // ...CHANGEMENT: utiliser GetPort() (nom du port exact) plutôt que GetOutputPort (potentiellement absent)
                NodePort outPort = node.GetPort(portName);
                if (outPort != null)
                {
                    NodeEditorGUILayout.PortField(new GUIContent("Vers"), outPort);
                }
                else
                {
                    EditorGUILayout.LabelField("Port non créé (sauvegarder pour générer)");
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(4);
            }

            // actually remove the element outside of the layout loop
            if (removeIndex >= 0 && removeIndex < responsesProp.arraySize)
            {
                responsesProp.DeleteArrayElementAtIndex(removeIndex);
            }

            EditorGUI.indentLevel--;
        }

        // Boutons d'ajout / suppression
    EditorGUILayout.BeginHorizontal();
    if (GUILayout.Button("+ Ajouter une réponse", GUILayout.ExpandWidth(true)))
        {
            // utilise l'API serialized pour ajouter proprement (plus sûr que InsertArrayElementAtIndex)
            int newIndex = responsesProp.arraySize;
            responsesProp.arraySize = responsesProp.arraySize + 1;
            SerializedProperty newElem = responsesProp.GetArrayElementAtIndex(newIndex);
            newElem.FindPropertyRelative("responseText").stringValue = "Nouvelle réponse";
            newElem.FindPropertyRelative("condition").objectReferenceValue = null;
            newElem.FindPropertyRelative("effect").objectReferenceValue = null;
            // les ports seront mis à jour après ApplyModifiedProperties
        }
        if (GUILayout.Button("Suppr. dernière", GUILayout.ExpandWidth(true)))
        {
            if (responsesProp.arraySize > 0)
                responsesProp.DeleteArrayElementAtIndex(responsesProp.arraySize - 1);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(6);
        EditorGUILayout.PropertyField(endOfPhaseProp, new GUIContent("Fin de phase"));

        // Applique les modifications - récupère si des changements ont été effectivement appliqués
        bool applied = serializedObject.ApplyModifiedProperties();

        // Après avoir modifié l'array via serializedObject, force mise à jour des ports si nécessaire
        if (applied)
        {
            DialogueNode nodeRef = (DialogueNode)target;
            // Update dynamic ports sur le node (méthode prévue côté DialogueNode)
            nodeRef.UpdateDynamicPorts();
            EditorUtility.SetDirty(nodeRef);
            // Forcer le repaint de l'éditeur de nodes pour refléter les changements de ports
            NodeEditorWindow.RepaintAll();

            // --- Synchronise les nextNode dans responses[] d'après les connexions visuelles ---
            // Utilise reflection pour être compatible avec différentes versions de XNode.
            bool anyChange = false;
            serializedObject.Update();
            for (int i = 0; i < responsesProp.arraySize; i++)
            {
                SerializedProperty responseProp = responsesProp.GetArrayElementAtIndex(i);
                SerializedProperty nextNodeProp = responseProp.FindPropertyRelative("nextNode");
                string portName = $"response_{i}";
                NodePort outPort = nodeRef.GetPort(portName);
                DialogueNode connected = null;
                if (outPort != null)
                {
                    // reflection-based helpers to find the connected node
                    object connNode = null;
                    var portType = outPort.GetType();
                    // 1) try IsConnected + Connection
                    var isConnectedProp = portType.GetProperty("IsConnected");
                    if (isConnectedProp != null)
                    {
                        bool isConn = (bool)isConnectedProp.GetValue(outPort, null);
                        if (isConn)
                        {
                            var connProp = portType.GetProperty("Connection") ?? portType.GetProperty("connection");
                            if (connProp != null)
                            {
                                var conn = connProp.GetValue(outPort, null);
                                if (conn != null)
                                {
                                    var nodeProp = conn.GetType().GetProperty("node");
                                    if (nodeProp != null) connNode = nodeProp.GetValue(conn, null);
                                }
                            }
                        }
                    }

                    // 2) try ConnectionCount / GetConnection(i) pattern
                    if (connNode == null)
                    {
                        var connCountProp = portType.GetProperty("ConnectionCount") ?? portType.GetProperty("connectionCount");
                        if (connCountProp != null)
                        {
                            int cnt = (int)connCountProp.GetValue(outPort, null);
                            if (cnt > 0)
                            {
                                var getConn = portType.GetMethod("GetConnection") ?? portType.GetMethod("GetConnections");
                                if (getConn != null)
                                {
                                    var res = getConn.Invoke(outPort, new object[] { 0 });
                                    if (res != null)
                                    {
                                        var nodeProp = res.GetType().GetProperty("node");
                                        if (nodeProp != null) connNode = nodeProp.GetValue(res, null);
                                    }
                                }
                            }
                        }
                    }

                    // 3) try Connections property (array)
                    if (connNode == null)
                    {
                        var connsProp = portType.GetProperty("Connections") ?? portType.GetProperty("connections");
                        var conns = connsProp != null ? connsProp.GetValue(outPort, null) as System.Collections.IEnumerable : null;
                        if (conns != null)
                        {
                            var enumerator = conns.GetEnumerator();
                            if (enumerator.MoveNext())
                            {
                                var first = enumerator.Current;
                                var nodeProp = first.GetType().GetProperty("node");
                                if (nodeProp != null) connNode = nodeProp.GetValue(first, null);
                            }
                        }
                    }

                    connected = connNode as DialogueNode;
                }

                if (nextNodeProp != null)
                {
                    if (nextNodeProp.objectReferenceValue != connected)
                    {
                        nextNodeProp.objectReferenceValue = connected;
                        anyChange = true;
                    }
                }
            }
            if (anyChange) serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
