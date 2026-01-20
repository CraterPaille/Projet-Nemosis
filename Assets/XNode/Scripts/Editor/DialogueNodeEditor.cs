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

    // Styles reutilisables pour le texte multi-ligne et a retour automatique
    private static GUIStyle _textAreaStyle;

    // Note: Some versions of the XNode NodeEditor do not expose OnEnable as overrideable.
    // Initialize SerializedProperty fields lazily in OnBodyGUI to avoid override issues.

    public override int GetWidth()
    {
        // Largeur augmentee pour faciliter la lecture
        return 360;
    }

    public override void OnBodyGUI()
    {
        serializedObject.Update();

        if (godTextProp == null)
        {
            godTextProp = serializedObject.FindProperty("godText");
            responsesProp = serializedObject.FindProperty("responses");
            endOfPhaseProp = serializedObject.FindProperty("endOfPhase");
        }

        DialogueNode node = target as DialogueNode;

        NodePort inPort = node.GetInputPort("previousNode");
        if (inPort != null) NodeEditorGUILayout.PortField(new GUIContent("Entree"), inPort);

        EditorGUILayout.Space(6);

        // Texte du dieu (multiligne, wrap)
        if (_textAreaStyle == null)
        {
            _textAreaStyle = new GUIStyle(EditorStyles.textArea) { wordWrap = true };
        }
        EditorGUILayout.LabelField("Texte du dieu", EditorStyles.boldLabel);
        godTextProp.stringValue = EditorGUILayout.TextArea(godTextProp.stringValue, _textAreaStyle, GUILayout.MinHeight(48));

        EditorGUILayout.Space(8);

        EditorGUILayout.LabelField("Reponses", EditorStyles.boldLabel);

        if (responsesProp != null)
        {
            EditorGUI.indentLevel++;
            int removeIndex = -1;
            int count = responsesProp.arraySize;
            for (int i = 0; i < count; i++)
            {
                SerializedProperty responseProp = responsesProp.GetArrayElementAtIndex(i);
                SerializedProperty responseTextProp = responseProp.FindPropertyRelative("responseText");
                SerializedProperty conditionsProp = responseProp.FindPropertyRelative("conditions");
                SerializedProperty effectsProp = responseProp.FindPropertyRelative("effects");

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Reponse #{i + 1}", GUILayout.MaxWidth(120));
                if (GUILayout.Button("-", GUILayout.Width(24))) removeIndex = i;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.LabelField("Texte", EditorStyles.miniBoldLabel);
                responseTextProp.stringValue = EditorGUILayout.TextArea(responseTextProp.stringValue, _textAreaStyle, GUILayout.MinHeight(40));
                EditorGUILayout.PropertyField(responseProp.FindPropertyRelative("relationDelta"), new GUIContent("Delta relation"));
                EditorGUILayout.PropertyField(conditionsProp, new GUIContent("Conditions (SO)"));
                EditorGUILayout.PropertyField(effectsProp, new GUIContent("Effets (SO)"));

                string portName = $"response_{i}";
                NodePort outPort = node.GetPort(portName);
                if (outPort != null)
                {
                    NodeEditorGUILayout.PortField(new GUIContent("Vers"), outPort);
                }
                else
                {
                    EditorGUILayout.LabelField("Port non cree (sauvegarder pour generer)");
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(4);
            }

            if (removeIndex >= 0 && removeIndex < responsesProp.arraySize)
            {
                responsesProp.DeleteArrayElementAtIndex(removeIndex);
            }

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("+ Ajouter une reponse", GUILayout.ExpandWidth(true)))
        {
            int newIndex = responsesProp.arraySize;
            responsesProp.arraySize = responsesProp.arraySize + 1;
            SerializedProperty newElem = responsesProp.GetArrayElementAtIndex(newIndex);
            newElem.FindPropertyRelative("responseText").stringValue = "Nouvelle reponse";
            newElem.FindPropertyRelative("conditions").arraySize = 0;
            newElem.FindPropertyRelative("effects").arraySize = 0;
        }
        if (GUILayout.Button("Suppr. derniere", GUILayout.ExpandWidth(true)))
        {
            if (responsesProp.arraySize > 0)
                responsesProp.DeleteArrayElementAtIndex(responsesProp.arraySize - 1);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(6);
        EditorGUILayout.PropertyField(endOfPhaseProp, new GUIContent("Fin de phase"));

        bool applied = serializedObject.ApplyModifiedProperties();

        if (applied)
        {
            DialogueNode nodeRef = (DialogueNode)target;
            nodeRef.UpdateDynamicPorts();
            EditorUtility.SetDirty(nodeRef);
            NodeEditorWindow.RepaintAll();

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
                    object connNode = null;
                    var portType = outPort.GetType();
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
