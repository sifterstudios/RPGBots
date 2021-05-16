using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using MD_Plugin;

namespace MD_Plugin
{
    public class MD_GlobalPreferences
    {
        [SerializeField] public static bool createNewReference; //Create new mesh reference if any modifier/ mesh deformator is applied to an object. Recommended to True
        [SerializeField] public static bool popupEditorWindow;  //Popup editor windows if any important notification occurs. Recommended to True
        [SerializeField] public static bool autoRecalcNormals;  //Auto recalculate normals as default
        [SerializeField] public static bool autoRecalcBounds;   //Auto recalculate bounds as default
        [SerializeField] public static int vertexLimit;         //Maximum vertex count limit to edit. Recommended value is 2000
    }
}

#if UNITY_EDITOR
namespace MD_PluginEditor
{
    [InitializeOnLoad]
    public class MD_Preferences : MD_EditorWindowUtilities
    {
        static MD_Preferences()
        {
            RefreshValues();
        }

        private static void RefreshValues()
        {
            MD_GlobalPreferences.createNewReference = mdPreference_CreateNewMeshReference;
            MD_GlobalPreferences.popupEditorWindow = mdPreference_PopupEditorWindows;
            MD_GlobalPreferences.autoRecalcNormals = mdPreference_AutoRecalculateNormals;
            MD_GlobalPreferences.autoRecalcBounds = mdPreference_AutoRecalculateBounds;
            MD_GlobalPreferences.vertexLimit = mdPreference_VertexLimit;
        }

        [MenuItem("Window/MD_Package/Preferences")]
        private static void Init()
        {
            MD_Preferences vt = (MD_Preferences)GetWindow(typeof(MD_Preferences));
            vt.maxSize = new Vector2(400, 350);
            vt.minSize = new Vector2(350, 300);
            vt.titleContent = new GUIContent("MD Package Preferences");
            vt.Show();
            RefreshValues();
        }

        #region Preferences

        private static bool mdpCreateNewReference = true;
        public static bool mdPreference_CreateNewMeshReference
        {
            get
            {
                if (EditorPrefs.HasKey("mdpCreateNewReference"))
                    return EditorPrefs.GetBool("mdpCreateNewReference");
                else
                    return mdpCreateNewReference;
            }
            set
            {
                mdpCreateNewReference = value;
                MD_GlobalPreferences.createNewReference = mdpCreateNewReference;
                EditorPrefs.SetBool("mdpCreateNewReference", mdpCreateNewReference);
            }
        }


        private static int mdpVertexLimit = 2000;
        public static int mdPreference_VertexLimit
        {
            get
            {
                if (EditorPrefs.HasKey("mdpVertexLimit"))
                    return EditorPrefs.GetInt("mdpVertexLimit");
                else
                    return mdpVertexLimit;
            }
            set
            {
                mdpVertexLimit = value;
                MD_GlobalPreferences.vertexLimit = mdpVertexLimit;
                EditorPrefs.SetInt("mdpVertexLimit", mdpVertexLimit);
            }
        }


        private static bool mdpPopupEditorWindows = true;
        public static bool mdPreference_PopupEditorWindows
        {
            get
            {
                if (EditorPrefs.HasKey("mdpPopupEditorWindows"))
                    return EditorPrefs.GetBool("mdpPopupEditorWindows");
                else
                    return mdpPopupEditorWindows;
            }
            set
            {
                mdpPopupEditorWindows = value;
                MD_GlobalPreferences.popupEditorWindow = mdpPopupEditorWindows;
                EditorPrefs.SetBool("mdpPopupEditorWindows", mdpPopupEditorWindows);
            }
        }


        private static bool mdpAutoRecalculateNormals = true;
        public static bool mdPreference_AutoRecalculateNormals
        {
            get
            {
                if (EditorPrefs.HasKey("mdpAutoRecalculateNormals"))
                    return EditorPrefs.GetBool("mdpAutoRecalculateNormals");
                else
                    return mdpAutoRecalculateNormals;
            }
            set
            {
                mdpAutoRecalculateNormals = value;
                MD_GlobalPreferences.autoRecalcNormals = mdpAutoRecalculateNormals;
                EditorPrefs.SetBool("mdpAutoRecalculateNormals", mdpAutoRecalculateNormals);
            }
        }

        private static bool mdpAutoRecalculateBounds = true;
        public static bool mdPreference_AutoRecalculateBounds
        {
            get
            {
                if (EditorPrefs.HasKey("mdpAutoRecalculateBounds"))
                    return EditorPrefs.GetBool("mdpAutoRecalculateBounds");
                else
                    return mdpAutoRecalculateBounds;
            }
            set
            {
                mdpAutoRecalculateBounds = value;
                MD_GlobalPreferences.autoRecalcBounds = mdpAutoRecalculateBounds;
                EditorPrefs.SetBool("mdpAutoRecalculateBounds", mdpAutoRecalculateBounds);
            }
        }

        #endregion

        private void OnGUI()
        {
            ps(20);
            pl("MD Package - Preferences", true);
            ps(15);
            pv();
            pv();
            mdPreference_PopupEditorWindows = GUILayout.Toggle(mdPreference_PopupEditorWindows, "Popup Editor Windows if any notification");
            ps();
            mdPreference_CreateNewMeshReference = GUILayout.Toggle(mdPreference_CreateNewMeshReference, "Create New Mesh Reference as Default");
            pve();
            ps(20);
            pv();
            mdPreference_AutoRecalculateNormals = GUILayout.Toggle(mdPreference_AutoRecalculateNormals, "Auto Recalculate Normals as Default");
            ps();
            mdPreference_AutoRecalculateBounds = GUILayout.Toggle(mdPreference_AutoRecalculateBounds, "Auto Recalculate Bounds as Default");
            pve();
            ps(20);
            pv();
            mdPreference_VertexLimit = EditorGUILayout.IntField("Max Vertex Limit", mdPreference_VertexLimit);
            if (mdPreference_VertexLimit != 2000)
                phb("It's very recommended to keep the original value [2000]. The higher value is, the higher risk of application damage may be caused.", MessageType.Warning);
            pve();
            pve();
        }
    }
}
#endif