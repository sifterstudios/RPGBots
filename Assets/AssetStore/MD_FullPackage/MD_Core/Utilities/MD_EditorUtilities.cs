#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using MD_Plugin;

namespace MD_PluginEditor
{
    /// <summary>
    /// Essential editor utilities for internal purpose
    /// </summary>
    public class MD_EditorUtilities : Editor
    {
        protected void ppDrawProperty(SerializedProperty p, string Text, string ToolTip = "", bool includeChilds = false)
        {
            try
            {
                EditorGUILayout.PropertyField(p, new GUIContent(Text, ToolTip), includeChilds, null);
                serializedObject.ApplyModifiedProperties();
            }
            catch
            {  }
        }
        protected void ppDrawProperty(string p, string Text, string ToolTip = "", bool includeChilds = false, bool identOffset = false)
        {
            try
            {
                if (identOffset) EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty(p), new GUIContent(Text, ToolTip), includeChilds, null);
                if (identOffset) EditorGUI.indentLevel--;
                serializedObject.ApplyModifiedProperties();
            }
            catch
            {  }
        }

        protected void ppAddMeshColliderRefresher(GameObject sender)
        {
            if (sender.GetComponent<MD_MeshColliderRefresher>()) return;
            Color c;
            ColorUtility.TryParseHtmlString("#49de71", out c);
            GUI.color = c;
            if (pb("Add Mesh Collider Refresher")) sender.AddComponent<MD_MeshColliderRefresher>();
        }
        protected void ppBackToMeshEditor(MonoBehaviour Sender)
        {
            Color c;
            ColorUtility.TryParseHtmlString("#f2d0d0", out c);
            GUI.color = c;
            if (pb("Back To Mesh Editor"))
            {
                GameObject gm = Sender.gameObject;
                DestroyImmediate(Sender);
                gm.AddComponent<MD_MeshProEditor>();
            }
        }

        protected void phb(string msg, MessageType msgt = MessageType.None)
        {
            EditorGUILayout.HelpBox(msg, msgt);
        }
        protected void pl(string s, bool bold = false)
        {
            if(bold)
                GUILayout.Label(s, EditorStyles.boldLabel);
            else
                GUILayout.Label(s);
        }
        protected void pl(Texture2D s)
        {
            GUILayout.Label(s);
        }
        protected void pl(Texture s)
        {
            GUILayout.Label(s);
        }
        protected void pv(bool box = true)
        {
            if (!box) GUILayout.BeginVertical();
            else GUILayout.BeginVertical("Box");
        }
        protected void pve()
        {
            GUILayout.EndVertical();
        }
        protected void ph(bool box = true)
        {
            if (!box) GUILayout.BeginHorizontal();
            else GUILayout.BeginHorizontal("Box");
        }
        protected void phe()
        {
            GUILayout.EndHorizontal();
        }
        protected bool pb(string s)
        {
            return GUILayout.Button(s);
        }
        protected bool pb(GUIContent gui)
        {
            return GUILayout.Button(gui);
        }
        protected void ps(int s = 10)
        {
            GUILayout.Space(s);
        }
    }

    /// <summary>
    /// Essential editor Window utilities for internal purpose
    /// </summary>
    public class MD_EditorWindowUtilities : EditorWindow
    {
        protected void phb(string msg, MessageType msgt = MessageType.None)
        {
            EditorGUILayout.HelpBox(msg, msgt);
        }
        protected void pl(string s, bool bold = false)
        {
            if (bold)
                GUILayout.Label(s, EditorStyles.boldLabel);
            else
                GUILayout.Label(s);
        }
        protected void pl(Texture2D s)
        {
            GUILayout.Label(s);
        }
        protected void pv(bool box = true)
        {
            if (!box) GUILayout.BeginVertical();
            else GUILayout.BeginVertical("Box");
        }
        protected void pve()
        {
            GUILayout.EndVertical();
        }
        protected void ph(bool box = true)
        {
            if (!box) GUILayout.BeginHorizontal();
            else GUILayout.BeginHorizontal("Box");
        }
        protected void phe()
        {
            GUILayout.EndHorizontal();
        }
        protected bool pb(string s)
        {
            return GUILayout.Button(s);
        }
        protected bool pb(GUIContent gui)
        {
            return GUILayout.Button(gui);
        }
        protected void ps(int s = 10)
        {
            GUILayout.Space(s);
        }
    }

    /// <summary>
    /// Essential editor Material utilities for internal purpose
    /// </summary>
    public class MD_MaterialEditorUtilities : ShaderGUI
    {
        protected bool ppDrawProperty(MaterialEditor matSrc, MaterialProperty[] props, string p, bool texture = false, string tooltip = "")
        {
            bool found = false;
            foreach (MaterialProperty prop in props)
            {
                if (prop.name == p)
                {
                    found = true;
                    break;
                }
            }
            if (!found) return false;
            MaterialProperty property = FindProperty(p, props);
            if (!texture)  matSrc.ShaderProperty(property, new GUIContent(property.displayName, tooltip));
            else
            {
                Rect last = EditorGUILayout.GetControlRect();
                matSrc.TexturePropertyMiniThumbnail(last, property, property.displayName, "");
            }

            return true;
        }
        protected bool ppDrawProperty(MaterialEditor matSrc, MaterialProperty[] props, string p, string s, string tooltip = "")
        {
            bool found = false;
            foreach (MaterialProperty prop in props)
            {
                if (prop.name == p)
                {
                    found = true;
                    break;
                }
            }
            if (!found) return false;
            MaterialProperty property = FindProperty(p, props);
            matSrc.ShaderProperty(property, new GUIContent(s, tooltip));

            return true;
        }

        protected bool ppCompareProperty(MaterialEditor matSrc, string a, float b)
        {
            return MaterialEditor.GetMaterialProperty(matSrc.serializedObject.targetObjects, a).floatValue == b;
        }

        protected void phb(string msg)
        {
            EditorGUILayout.HelpBox(msg, MessageType.None);
        }
        protected void pl(string s, bool bold = true)
        {
            if (bold)
                GUILayout.Label(s, EditorStyles.boldLabel);
            else
                GUILayout.Label(s);
        }
        protected void pl(Texture2D s)
        {
            GUILayout.Label(s);
        }
        protected void pv(bool box = true)
        {
            if (!box) GUILayout.BeginVertical();
            else GUILayout.BeginVertical("Box");
        }
        protected void pve()
        {
            GUILayout.EndVertical();
        }
        protected void ph(bool box = true)
        {
            if (!box) GUILayout.BeginHorizontal();
            else GUILayout.BeginHorizontal("Box");
        }
        protected void phe()
        {
            GUILayout.EndHorizontal();
        }
        protected bool pb(string s)
        {
            return GUILayout.Button(s);
        }
        protected void ps(int s = 10)
        {
            GUILayout.Space(s);
        }
    }
}
#endif