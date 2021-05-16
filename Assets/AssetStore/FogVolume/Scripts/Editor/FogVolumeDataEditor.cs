using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditorInternal;
using UnityEditor;

[CustomEditor(typeof(FogVolumeData))]
public class FogVolumeDataEditor : Editor
{

    FogVolumeData _target;
    void OnEnable()
    {
        _target = (FogVolumeData)target;
    }
    private static bool SHOW_DEBUG_DATA
    {
        get { return EditorPrefs.GetBool("SHOW_DEBUG_DATATab", false); }
        set { EditorPrefs.SetBool("SHOW_DEBUG_DATATab", value); }
    }
    public override void OnInspectorGUI()
    {

        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        GUILayout.Space(10);
        _target.GameCamera = (Camera)EditorGUILayout.ObjectField("Game Camera", _target.GameCamera, typeof(Camera), true);
        _target.ForceNoRenderer = EditorGUILayout.Toggle("Disable Camera script", _target.ForceNoRenderer);
       
        GUILayout.Space(10);

        if (GUILayout.Button("DEBUG DATA", EditorStyles.toolbarButton))
            SHOW_DEBUG_DATA = !SHOW_DEBUG_DATA;
        if (SHOW_DEBUG_DATA)
        {
            var FoundCameras = serializedObject.FindProperty("FoundCameras");
            GUILayout.BeginVertical("box");
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(FoundCameras, new GUIContent("Available Cameras:"), true);
            EditorGUI.indentLevel--;
            GUILayout.EndVertical();

            var SceneFogVolumes = serializedObject.FindProperty("SceneFogVolumes");
            GUILayout.BeginVertical("box");
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(SceneFogVolumes, new GUIContent("Available Fog Volumes:"), true);
            EditorGUI.indentLevel--;
            GUILayout.EndVertical();

        }
        EditorGUI.EndChangeCheck();

    
        if (GUI.changed)
        {
            Undo.RecordObject(target, "Fog volume Data modified");
            EditorUtility.SetDirty(target);
        }

        serializedObject.ApplyModifiedProperties();
    }
}