using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FogVolumeDirectionalLight))]
public class FogVolumeDirectionalLightEditor : Editor
{

    FogVolumeDirectionalLight _target;
    void OnEnable()
    {
        _target = (FogVolumeDirectionalLight)target;
    }

    GUIContent VariableField(string VariableName, string Tooltip)
    {
        return new GUIContent(VariableName, Tooltip);
    }
    private static bool SHOW_DEBUG_Options
    {
        get { return EditorPrefs.GetBool("SHOW_FV_DIR_LIGHT_DEBUG_OptionsTab", false); }
        set { EditorPrefs.SetBool("SHOW_FV_DIR_LIGHT_DEBUG_OptionsTab", value); }
    }

    public override void OnInspectorGUI()
    {

        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        Undo.RecordObject(_target, "Fog volume directional light modified");
        GUILayout.Space(10);
        GUILayout.BeginVertical("box");
        if (_target._ProminentFogVolume == null)
            GUI.color = Color.red;
        // _target._TargetFogVolume = (FogVolume)EditorGUILayout.ObjectField("Target Fog Volume", _target._TargetFogVolume, typeof(FogVolume), true);

        var FogVolumes = serializedObject.FindProperty("_TargetFogVolumes");
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(FogVolumes, new GUIContent("Target Fog Volume"), true);
        EditorGUI.indentLevel--;
        GUI.color = Color.white;
        GUI.color = new Color(.9f, 1, .9f);
        if (GUILayout.Button("Add all Fog Volumes" /*GUILayout.Width(100)*/))
        {
            _target.AddAllFogVolumesToThisLight();
            _target.Refresh();
            _target.Render();
        }
        if (GUILayout.Button("Remove all Fog Volumes" /*GUILayout.Width(100)*/))
        {
            _target.RemoveAllFogVolumesFromThisLight();
            _target.Refresh();
            _target.Render();
        }
        GUI.color = Color.white;
        _target._CameraVerticalPosition = EditorGUILayout.FloatField(VariableField
            ("Shadow camera distance", "This is the distance from the shadow camera to the focus point. Increase it if the scene area you want to shade is not completely visible in the shadowmap"), _target._CameraVerticalPosition);
        _target.Size = (FogVolumeDirectionalLight.Resolution)EditorGUILayout.EnumPopup("Resolution", _target.Size);
        _target._Antialiasing = (FogVolumeDirectionalLight.Antialiasing)EditorGUILayout.EnumPopup("Antialiasing", _target._Antialiasing);
        _target._UpdateMode = (FogVolumeDirectionalLight.UpdateMode)EditorGUILayout.EnumPopup(VariableField("Update mode", "OnStart: bake shadowmap on start\nInterleaved: skip frames"), _target._UpdateMode);
        if (_target._UpdateMode == FogVolumeDirectionalLight.UpdateMode.Interleaved)
            _target.SkipFrames = EditorGUILayout.IntSlider(VariableField("Skip frames", "Instead updating per-frame, we can skip frames before updating the shadow"), _target.SkipFrames, 0, 10);
        else
        {
            GUI.color = new Color(.9f, 1, .9f);
            if (GUILayout.Button("Refresh" /*GUILayout.Width(100)*/))
            {
                _target.Refresh();
                _target.Render();
                _target.Render();
            }
            GUI.color = Color.white;
        }

        Rect LayerFieldRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(20));

        if (_target.ShadowCamera != null && _target.ShadowCamera.cullingMask == 0)
            GUI.color = Color.red;
        _target.LayersToRender = FogVolumeUtilities.EditorExtension.DrawLayerMaskField(LayerFieldRect, _target.LayersToRender, new GUIContent("Layers to render"));
        GUI.color = Color.white;
        _target._ScaleMode = (FogVolumeDirectionalLight.ScaleMode)EditorGUILayout.EnumPopup(VariableField("Scale mode",
            "With VolumeX, the camera will take the volume scale.x to adjust its size\nSet it to manual to adjust this size yourself. Useful when you have a large volume and only want to have good shadows at the focus point"), _target._ScaleMode);
        if (_target._ScaleMode == FogVolumeDirectionalLight.ScaleMode.Manual && _target._ProminentFogVolume != null)
            _target.Scale = EditorGUILayout.Slider(VariableField("Zoom", "Scale the view range of the camera.\n It will use the volume size from position 0"), _target.Scale, 10, _target._ProminentFogVolume.fogVolumeScale.x);
        _target._FocusMode = (FogVolumeDirectionalLight.FocusMode)EditorGUILayout.EnumPopup(VariableField("Focus mode", "The camera will focus on the given position. If the volume is not too large the best option would be Volume center"), _target._FocusMode);
        if (_target._FocusMode == FogVolumeDirectionalLight.FocusMode.GameObject)
            _target._GameObjectFocus = (Transform)EditorGUILayout.ObjectField(VariableField("Focus point", "Gameobject used to focus the shadow camera"), _target._GameObjectFocus, typeof(Transform), true);
        _target._FogVolumeShadowMapEdgeSoftness = EditorGUILayout.Slider(VariableField("Edge softness", "Fading range for the edges of the shadowmap"),
            _target._FogVolumeShadowMapEdgeSoftness, .001f, 1);
        GUILayout.EndVertical();//end box

        #region Debug
        GUILayout.BeginVertical("box");
        if (GUILayout.Button("Debug options", EditorStyles.toolbarButton))
            SHOW_DEBUG_Options = !SHOW_DEBUG_Options;

        if (SHOW_DEBUG_Options)
        {
            _target.CameraVisible = EditorGUILayout.Toggle(VariableField("Show shadow map camera", "Not updating correctly until we click on the hierarchy window :/"), _target.CameraVisible);


            if (_target.GOShadowCamera && _target.GOShadowCamera.hideFlags == HideFlags.HideInHierarchy && _target.CameraVisible)
            {

                _target.GOShadowCamera.hideFlags = HideFlags.None;
                EditorApplication.RepaintHierarchyWindow();
                EditorApplication.DirtyHierarchyWindowSorting();
            }
            if (_target.GOShadowCamera && _target.GOShadowCamera.hideFlags == HideFlags.None && !_target.CameraVisible)
            {
                _target.GOShadowCamera.hideFlags = HideFlags.HideInHierarchy;
                EditorApplication.RepaintHierarchyWindow();
                EditorApplication.DirtyHierarchyWindowSorting();
            }
            _target.ShowMiniature = EditorGUILayout.Toggle("View depth map", _target.ShowMiniature);

            EditorGUI.indentLevel++;
            if (_target.ShowMiniature)
                _target.MiniaturePosition = EditorGUILayout.Vector2Field("Position", _target.MiniaturePosition);
            EditorGUI.indentLevel--;
        }
        EditorGUI.EndChangeCheck();
        GUI.color = Color.green;
        if (_target._ProminentFogVolume == null)
            EditorGUILayout.HelpBox("Add a Fog Volume now", MessageType.Info);
        if (_target._ProminentFogVolume != null && _target.ShadowCamera != null && _target.ShadowCamera.cullingMask == 0)
            EditorGUILayout.HelpBox("Set the layers used to cast shadows", MessageType.Info);
        GUI.color = Color.white;

        GUI.color = Color.red;
        if (_target._TargetFogVolumes != null)
        {
            foreach (FogVolume fv in _target._TargetFogVolumes)
            {
                if (fv != null)
                    if (fv._FogType == FogVolume.FogType.Uniform)
                    {
                        EditorGUILayout.HelpBox(fv.name +
                                                " Is not a valid Volume. It has to be textured (must use gradient or noise)",
                                                MessageType.Info);
                    }
            }
        }

        GUI.color = Color.white;

        GUI.color = Color.red;
        if (_target._FocusMode == FogVolumeDirectionalLight.FocusMode.GameObject && _target._GameObjectFocus == null)
            EditorGUILayout.HelpBox("Set the focus point object", MessageType.Info);
        GUI.color = Color.white;

        EditorGUILayout.HelpBox("Initial version, experimental", MessageType.Info);
        GUILayout.EndVertical();//end box
        #endregion

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }

        serializedObject.ApplyModifiedProperties();
    }
}