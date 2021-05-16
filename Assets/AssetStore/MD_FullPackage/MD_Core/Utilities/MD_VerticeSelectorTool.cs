#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MD_Plugin;

namespace MD_PluginEditor
{
    public class MD_VerticeSelectorTool : EditorWindow
    {
        public MDM_MeshFit sender;
        public List<GameObject> selectedVertices = new List<GameObject>();

        private void OnGUI()
        {
            if (GUILayout.Button("Assign selected vertices [" + Selection.gameObjects.Length + "]"))
            {
                selectedVertices.Clear();
                if (Selection.activeGameObject != null && Selection.gameObjects.Length > 1)
                {
                    foreach (GameObject gm in Selection.gameObjects)
                    {
                        if (gm.transform.root == sender.transform)
                            selectedVertices.Add(gm);
                    }

                    sender.ppMODIF_MeshFitter_SelectedVertexes = selectedVertices.ToArray();
                    sender.MeshFit_RefreshVerticesActiveState();
                    Selection.activeObject = sender.gameObject;

                    this.Close();
                }
            }
        }
    }
}
#endif