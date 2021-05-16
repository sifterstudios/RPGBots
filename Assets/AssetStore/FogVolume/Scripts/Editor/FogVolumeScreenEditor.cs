using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(FogVolumeScreen))]
[ExecuteInEditMode]
public class FogVolumeScreenEditor : Editor
{
    string[] layerMaskName;
    int layerMaskNameIndex = 0;

    void OnEnable()
    {
        FogVolumeScreen _target = (FogVolumeScreen)target;
        List<string> layerMaskList = new List<string>();
        for (int i = 0; i < 32; i++)
        {
            string layerName = LayerMask.LayerToName(i);
            if (layerName != "")
            {
                if (layerName == _target.FogVolumeLayerName)
                    layerMaskNameIndex = layerMaskList.Count;
                layerMaskList.Add(layerName);
            }
        }
        layerMaskName = layerMaskList.ToArray();
    }

    public override void OnInspectorGUI()
    {
        FogVolumeScreen _target = (FogVolumeScreen)target;
        

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Density layer");
        int newLayerMaskNameIndex = EditorGUILayout.Popup(layerMaskNameIndex, layerMaskName);
        if (newLayerMaskNameIndex != layerMaskNameIndex)
        {
            layerMaskNameIndex = newLayerMaskNameIndex;
            _target.FogVolumeLayerName = layerMaskName[layerMaskNameIndex];
        }
        GUILayout.EndHorizontal();
        // Draw the default inspector
        DrawDefaultInspector();
        EditorGUILayout.HelpBox("Work in progress!", MessageType.None);
        EditorUtility.SetDirty(target);
    }
}