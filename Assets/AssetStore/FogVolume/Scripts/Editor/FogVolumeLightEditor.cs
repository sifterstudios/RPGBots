using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using UnityEditor;


[CustomEditor(typeof(FogVolumeLight))]
public class FogVolumeLightEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GUILayout.Space(10.0f);
        serializedObject.Update();

        GUILayout.BeginVertical("box");

        var usesNormalLight = serializedObject.FindProperty("IsAddedToNormalLight");
        if (usesNormalLight.boolValue)
        {
               EditorGUILayout.HelpBox("This light will use the settings of the light on this GameObject.\n\nRemove this component if the light should not be recognized by Fog Volume 3", MessageType.Info); 

        }
        else
        {
            var enabled = serializedObject.FindProperty("Enabled");
            if (GUILayout.Button(enabled.boolValue ? "Disable the light" : "Enable the light"))
            {
                enabled.boolValue = !enabled.boolValue;
            }

            if (enabled.boolValue == true)
            {
                GUILayout.Space(10.0f);

                var isPointLight = serializedObject.FindProperty("IsPointLight");
                int selectedLightType = isPointLight.boolValue ? 0 : 1;

                selectedLightType =
                        EditorGUILayout.Popup("Light Type: ",
                                              selectedLightType,
                                              m_lightTypes,
                                              EditorStyles.toolbarButton);

                if (selectedLightType == 0) { isPointLight.boolValue = true; }
                else { isPointLight.boolValue = false; }

                GUILayout.Space(10.0f);

                var color = serializedObject.FindProperty("Color");
                EditorGUILayout.PropertyField(color, new GUIContent("Color:"));

                GUILayout.Space(10.0f);

                var intensity = serializedObject.FindProperty("Intensity");
                EditorGUILayout.Slider(intensity,
                                       MinIntensity,
                                       MaxIntenstity,
                                       new GUIContent("Intensity:"));

                GUILayout.Space(10.0f);

                var range = serializedObject.FindProperty("Range");
                EditorGUILayout.Slider(range, MinRange, MaxRange, new GUIContent("Range:"));



                if (selectedLightType == 1)
                {
                    GUILayout.Space(10.0f);
                    var angle = serializedObject.FindProperty("Angle");
                    EditorGUILayout.Slider(angle,
                                           MinSpotAngle,
                                           MaxSpotAngle,
                                           new GUIContent("SpotAngle:"));
                }
            }
        }
        GUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }

    private const float MinIntensity = 0.0f;
    private const float MaxIntenstity = 50.0f;
    private const float MinRange = 0.0f;
    private const float MaxRange = 500.0f;
    private const float MinSpotAngle = 0.0f;
    private const float MaxSpotAngle = 180.0f;

    private readonly string[] m_lightTypes = new[]
    {
        "Point Light",
        "Spot Light"
    };
}
