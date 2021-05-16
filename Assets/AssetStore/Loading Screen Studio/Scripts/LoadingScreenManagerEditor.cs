using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;

namespace Michsky.LSS
{
    [CustomEditor(typeof(LoadingScreenManager))]
    [System.Serializable]
    public class LoadingScreenManagerEditor : Editor
    {
        private LoadingScreenManager lsmTarget;
        List<string> lsList = new List<string>();

        private void OnEnable()
        {
            lsmTarget = (LoadingScreenManager)target;
            lsmTarget.loadingScreens = Resources.LoadAll("Loading Screens", typeof(GameObject));

            foreach (var t in lsmTarget.loadingScreens)
            {
                lsList.Add(t.name);
            }
        }

        public override void OnInspectorGUI()
        {
            GUISkin customSkin;
            Color defaultColor = GUI.color;

            if (EditorGUIUtility.isProSkin == true)
                customSkin = (GUISkin)Resources.Load("Editor\\LSS Skin Dark");
            else
                customSkin = (GUISkin)Resources.Load("Editor\\LSS Skin Light");

            GUILayout.BeginHorizontal();
            GUI.backgroundColor = defaultColor;
            GUILayout.FlexibleSpace();

            GUILayout.Box(new GUIContent(""), customSkin.FindStyle("LSM Top Header"));

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            var managerTag = serializedObject.FindProperty("managerTag");
            var enableTrigger = serializedObject.FindProperty("enableTrigger");
            var onTriggerExit = serializedObject.FindProperty("onTriggerExit");
            var onlyLoadWithTag = serializedObject.FindProperty("onlyLoadWithTag");
            var dontDestroyOnLoad = serializedObject.FindProperty("dontDestroyOnLoad");
            var startLoadingAtStart = serializedObject.FindProperty("startLoadingAtStart");
            var sceneName = serializedObject.FindProperty("sceneName");
            var objectTag = serializedObject.FindProperty("objectTag");
            var prefabHelper = serializedObject.FindProperty("prefabHelper");
            var selectedLoadingIndex = serializedObject.FindProperty("selectedLoadingIndex");
            var selectedTagIndex = serializedObject.FindProperty("selectedTagIndex");

            GUILayout.Label("LOADING SCREEN", customSkin.FindStyle("Header"));

            if (lsList.Count == 1 || lsList.Count >= 1)
            {
                GUILayout.BeginHorizontal(EditorStyles.helpBox);

                EditorGUILayout.LabelField(new GUIContent("Selected Screen"), customSkin.FindStyle("Text"), GUILayout.Width(120));
                selectedLoadingIndex.intValue = EditorGUILayout.Popup(selectedLoadingIndex.intValue, lsList.ToArray());
                prefabHelper.stringValue = lsmTarget.loadingScreens.GetValue(selectedLoadingIndex.intValue).ToString().Replace(" (UnityEngine.GameObject)", "").Trim();

                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal(EditorStyles.helpBox);
                EditorGUI.indentLevel = 1;

                EditorGUILayout.PropertyField(dontDestroyOnLoad, new GUIContent("Don't Destroy On Load"), true);

                EditorGUI.indentLevel = 0;
                GUILayout.EndHorizontal();

                if (GUILayout.Button("Show Selected Screen", customSkin.button))
                    Selection.activeObject = Resources.Load("Loading Screens/" + lsList[lsmTarget.selectedLoadingIndex]);

                GUILayout.Space(16);
                GUILayout.Label("SETTINGS", customSkin.FindStyle("Header"));
                GUILayout.BeginHorizontal(EditorStyles.helpBox);

                startLoadingAtStart.boolValue = GUILayout.Toggle(startLoadingAtStart.boolValue, new GUIContent("Start Loading At Start"), customSkin.FindStyle("Toggle"));
                startLoadingAtStart.boolValue = GUILayout.Toggle(startLoadingAtStart.boolValue, new GUIContent(""), customSkin.FindStyle("Toggle Helper"));

                GUILayout.EndHorizontal();

                if (startLoadingAtStart.boolValue == true)
                {
                    GUILayout.BeginHorizontal(EditorStyles.helpBox);

                    EditorGUILayout.LabelField(new GUIContent("Load Scene"), customSkin.FindStyle("Text"), GUILayout.Width(120));
                    sceneName.stringValue = EditorGUILayout.TextField(sceneName.stringValue);

                    GUILayout.EndHorizontal();
                }

                GUILayout.BeginHorizontal(EditorStyles.helpBox);

                enableTrigger.boolValue = GUILayout.Toggle(enableTrigger.boolValue, new GUIContent("Load With Trigger"), customSkin.FindStyle("Toggle"));
                enableTrigger.boolValue = GUILayout.Toggle(enableTrigger.boolValue, new GUIContent(""), customSkin.FindStyle("Toggle Helper"));

                GUILayout.EndHorizontal();

                if (enableTrigger.boolValue == true)
                {
                    GUILayout.BeginHorizontal(EditorStyles.helpBox);

                    onTriggerExit.boolValue = GUILayout.Toggle(onTriggerExit.boolValue, new GUIContent("On Trigger Exit"), customSkin.FindStyle("Toggle"));
                    onTriggerExit.boolValue = GUILayout.Toggle(onTriggerExit.boolValue, new GUIContent(""), customSkin.FindStyle("Toggle Helper"));

                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal(EditorStyles.helpBox);

                    onlyLoadWithTag.boolValue = GUILayout.Toggle(onlyLoadWithTag.boolValue, new GUIContent("Only Load With Tag"), customSkin.FindStyle("Toggle"));
                    onlyLoadWithTag.boolValue = GUILayout.Toggle(onlyLoadWithTag.boolValue, new GUIContent(""), customSkin.FindStyle("Toggle Helper"));

                    GUILayout.EndHorizontal();

                    if (lsmTarget.onlyLoadWithTag == true)
                    {
                        GUILayout.BeginHorizontal(EditorStyles.helpBox);

                        EditorGUILayout.LabelField(new GUIContent("Object Tag"), customSkin.FindStyle("Text"), GUILayout.Width(120));
                        selectedTagIndex.intValue = EditorGUILayout.Popup(selectedTagIndex.intValue, UnityEditorInternal.InternalEditorUtility.tags);
                        objectTag.stringValue = UnityEditorInternal.InternalEditorUtility.tags[selectedTagIndex.intValue].ToString();

                        GUILayout.EndHorizontal();
                    }

                    GUILayout.BeginHorizontal(EditorStyles.helpBox);

                    EditorGUILayout.LabelField(new GUIContent("Load Scene"), customSkin.FindStyle("Text"), GUILayout.Width(120));
                    sceneName.stringValue = EditorGUILayout.TextField(sceneName.stringValue);

                    GUILayout.EndHorizontal();
                }
            }

            else
                EditorGUILayout.HelpBox("There isn't any loading screen prefab in Resoures > Loading Screens folder." +
                    "You have to create a prefab in order to use loading screen system.", MessageType.Warning);

            GUILayout.Space(6);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif