using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FogVolumePrimitive))]
public class FogVolumePrimitiveEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GUILayout.Space(10.0f);
        serializedObject.Update();

        GUILayout.BeginVertical("box");


        var enabled = serializedObject.FindProperty("IsSubtractive");
        int selectedActionType = enabled.boolValue ? 0 : 1;

        selectedActionType =
                EditorGUILayout.Popup("Action Type: ",
                                      selectedActionType,
                                      m_actionTypes,
                                      EditorStyles.toolbarButton);

        if (selectedActionType == 0) { enabled.boolValue = true; }
        else { enabled.boolValue = false; }


        var persistent = serializedObject.FindProperty("IsPersistent");
        int selectedPersistenceType = persistent.boolValue ? 0 : 1;

        selectedPersistenceType =
                EditorGUILayout.Popup("Persistence Type:",
                                      selectedPersistenceType,
                                      m_persistenceType,
                                      EditorStyles.toolbarButton);
        if (selectedPersistenceType == 0) { persistent.boolValue = true; }
        else { persistent.boolValue = false; }

        GUILayout.Space(2.0f);





        GUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }

    private readonly string[] m_actionTypes = new[]
    {
        "Subtractive",
        "Additive"  
    };

    private readonly string[] m_persistenceType = new[]
    {
        "Persistent",
        "Cullable"
    };
}
