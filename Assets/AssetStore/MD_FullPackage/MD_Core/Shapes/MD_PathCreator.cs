using System.Collections.Generic;
using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using MD_Plugin;
#endif

namespace MD_Plugin
{
    [AddComponentMenu(MD_Debug.ORGANISATION + MD_Debug.PACKAGENAME + "Path Creator")]
    [ExecuteInEditMode]
    public class MD_PathCreator : MonoBehaviour
    {
        public float pSize = 1.0f;
        public float pNodeSize = 0.2f;

        public bool pRevertNormals = false;
        public bool pApplyNodeLocalScale = false;

        public bool pUpdateEveryFrame = true;
        public bool pEnableSmartRotation = true;

        public bool pEnableDebug = true;

        public List<Transform> pNodes = new List<Transform>();

        public List<Vector3> pVertices = new List<Vector3>();
        public List<int> pTriangles = new List<int>();
        public List<Vector2> pUV = new List<Vector2>();

        public Mesh pCurrentMesh;

#if UNITY_EDITOR
        [MenuItem("GameObject/3D Object" + MD_Debug.PACKAGENAME + "Advanced/Path Creator")]
        public static void GeneratePathObj()
        {
            GameObject newPath = new GameObject("PathCreator");
            newPath.AddComponent<MD_PathCreator>();
            Selection.activeGameObject = newPath;
            newPath.transform.position = Vector3.zero;

            newPath.AddComponent<MeshFilter>();
            newPath.AddComponent<MeshRenderer>();

            Material mat = new Material(Shader.Find("Diffuse"));
            newPath.GetComponent<Renderer>().sharedMaterial = mat;
        }
#endif

        private void OnDrawGizmos()
        {
            if (!pEnableDebug)
                return;
            Gizmos.color = Color.cyan;
            if (pNodes.Count == 0)
                return;
            for (int i = 0; i < pNodes.Count; i++)
            {
                if (pNodes[i] == null)
                    continue;
                if (i != 0)
                    Gizmos.DrawLine(pNodes[i].position, pNodes[i - 1].position);
            }
        }

        private void Awake()
        {
            if (pCurrentMesh == null)
                pCurrentMesh = new Mesh();
        }

        private void Update()
        {
            if (!pUpdateEveryFrame)
                return;
            PUBLICpFunct_RefreshNodes();
        }

        //----INTERNAL/PUBLIC - Nodes Manipulations
        #region Accessible functions - Nodes Manipulation

        /// <summary>
        /// Add node on specific position
        /// </summary>
        public void PUBLICpFunct_AddNode(Vector3 toPos, bool GroupOnAdd = true)
        {
            if (Application.isPlaying)
                return;
            Awake();

            if (pNodes.Count == 0)
            {
                pFunctCreateNodeBlock(toPos, GroupOnAdd);
                pFunctCreateNodeBlock(toPos == Vector3.zero ? toPos + Vector3.forward : toPos, GroupOnAdd);
            }
            else
                pFunctCreateNodeBlock((toPos == Vector3.zero) ? pNodes[pNodes.Count - 1].position + pNodes[pNodes.Count - 1].forward * 2 : toPos, GroupOnAdd);
        }
        /// <summary>
        /// Remove last node
        /// </summary>
        public void PUBLICpFunct_RemoveNode()
        {
            if (Application.isPlaying)
                return;
            Awake();

            pFunctRemoveNodeBlock();
        }
        /// <summary>
        /// Clear all nodes
        /// </summary>
        public void PUBLICpFunct_ClearAll()
        {
            if (Application.isPlaying)
                return;

            pVertices.Clear();
            pTriangles.Clear();
            pUV.Clear();

            int c = pNodes.Count;
            for (int i = 0; i < c; i++)
            {
                if (pNodes[i] == null)
                    continue;
                DestroyImmediate(pNodes[i].gameObject);
            }

            pNodes.Clear();

            if(GetComponent<MeshFilter>())
            GetComponent<MeshFilter>().sharedMesh = null;
            pCurrentMesh = null;

            Awake();
        }
        /// <summary>
        /// Refresh current tunnel mesh
        /// </summary>
        public void PUBLICpFunct_RefreshNodes()
        {
            if (pNodes.Count == 0)
                return;
            if (Application.isPlaying)
                return;

            int iiindex = 0;
            for (int i = 0; i < pNodes.Count; i++)
            {
                if (pNodes.Count > 1 && i > 0 && pEnableSmartRotation)
                    pNodes[i].rotation = Quaternion.LookRotation(pNodes[i].position - pNodes[i - 1].position);
                pFunctRefreshMesh(iiindex, i);
                iiindex += 2;
            }

            if (pRevertNormals)
            {
                pCurrentMesh.triangles = pCurrentMesh.triangles.Reverse().ToArray();
                pCurrentMesh.normals = pCurrentMesh.normals.Reverse().ToArray();
            }
            else
                pCurrentMesh.triangles = pCurrentMesh.triangles.ToArray();

            pCurrentMesh.RecalculateBounds();
            GetComponent<MeshFilter>().mesh = pCurrentMesh;
        }

        /// <summary>
        /// Group all nodes together in hierarchy
        /// </summary>
        public void PUBLICpFunct_GroupNodesTogether()
        {
            for (int i = 0; i < pNodes.Count; i++)
            {
                if (i <= 0)
                    continue;
                pNodes[i].parent = pNodes[i - 1];
            }
        }
        /// <summary>
        /// Ungroup all nodes to 'empty' or to 'some object'
        /// </summary>
        public void PUBLICpFunct_UngroupNodes(Transform Detachto)
        {
            for (int i = 0; i < pNodes.Count; i++)
            {
                if (i <= 0)
                    continue;
                pNodes[i].parent = (Detachto) ? Detachto : null;
            }
        }

        #endregion

        //----INTERNAL - Creating path completed blocks
        private void pFunctCreateNodeBlock(Vector3 OriginPosition, bool GroupOnAdd = true)
        {
            Transform newOrigin = MD_Octahedron.Generate().transform;
            newOrigin.name = "Node" + pNodes.Count.ToString();
            newOrigin.localScale = Vector3.one * pNodeSize;
            DestroyImmediate(newOrigin.GetComponent<SphereCollider>());
            newOrigin.position = OriginPosition;

            if (GroupOnAdd && pNodes.Count >= 1)
                newOrigin.transform.parent = pNodes[pNodes.Count - 1].transform;

            pNodes.Add(newOrigin.transform);

            pProcess_CreateVerticePanel(newOrigin);
            pProcess_CreateUV();

            if (pNodes.Count <= 1)
                return;

            pProcess_CreateTrianglePanel();

            pFunctUpdateMeshParams();
        }
        private void pFunctRemoveNodeBlock()
        {
            if (pNodes.Count == 0)
                return;
            else if (pNodes.Count == 1)
            {
                DestroyImmediate(pNodes[pNodes.Count - 1].gameObject);
                pNodes.RemoveAt(pNodes.Count - 1);
                pVertices.RemoveRange(pVertices.Count - 2, 2);
                pUV.RemoveRange(pUV.Count - 2, 2);
                return;
            }

            DestroyImmediate(pNodes[pNodes.Count - 1].gameObject);
            pNodes.RemoveAt(pNodes.Count - 1);

            pTriangles.RemoveRange(pTriangles.Count - 6, 6);
            pUV.RemoveRange(pUV.Count - 2, 2);
            pVertices.RemoveRange(pVertices.Count - 2, 2);

            pFunctUpdateMeshParams(true);
        }

        private void pFunctRefreshMesh(int VertexQueue, int OriginQueue)
        {
            Transform OriginPosition = pNodes[OriginQueue];

            for (int i = VertexQueue; i < VertexQueue + 2; i++)
            {
                Matrix4x4 m = Matrix4x4.TRS(OriginPosition.position, OriginPosition.rotation, pApplyNodeLocalScale ? OriginPosition.localScale : Vector3.one);
                Vector3 pos = new Vector3(i % 2 == 0 ? -pSize : pSize, 0, 0);

                pos = m.MultiplyPoint3x4(pos);
                pVertices[i] = pos;
            }

            PUBLICpFunct_UpdateUVs();
            pFunctUpdateMeshParams();
        }

        private void pFunctUpdateMeshParams(bool deletingMesh = false)
        {
            if (!deletingMesh)
                pCurrentMesh.vertices = pVertices.ToArray();
            pCurrentMesh.triangles = pTriangles.ToArray();
            if (deletingMesh)
                pCurrentMesh.vertices = pVertices.ToArray();
            pCurrentMesh.RecalculateNormals();
            pCurrentMesh.RecalculateTangents();
            pCurrentMesh.RecalculateBounds();
            if (!GetComponent<MeshFilter>())
                gameObject.AddComponent<MeshFilter>();
            if(!GetComponent<MeshRenderer>())
                gameObject.AddComponent<MeshRenderer>();
            GetComponent<MeshFilter>().sharedMesh = pCurrentMesh;
        }

        //----INTERNAL/PUBLIC - UV Managing
        #region Accessible functions - UV Manipulation

        /// <summary>
        /// Update UV sets with specific UV mode
        /// </summary>
        public void PUBLICpFunct_UpdateUVs()
        {
            for (int v = 0; v < pVertices.Count; v++)
                pUV[v] = new Vector2(pVertices[v].x, pVertices[v].z);

            if(pUV.Count == pCurrentMesh.vertexCount)
                pCurrentMesh.uv = pUV.ToArray();
        }

        #endregion

        //----INTERNAL - Creating vertice connectors
        private void pProcess_CreateVerticePanel(Transform newOrigin)
        {
            pVertices.Add(new Vector3(-pSize, 0, newOrigin.position.z));
            pVertices.Add(new Vector3(pSize, 0, newOrigin.position.z));
        }
        private void pProcess_CreateTrianglePanel()
        {
            pProcess_CreateFace(pVertices.Count - 1);
        }
        private void pProcess_CreateFace(int i)
        {
            int[] Faces = new int[]
            {
                i - 3, i - 1, i,
                i - 3, i, i - 2,
            };

            pTriangles.AddRange(Faces);
        }
        private void pProcess_CreateUV()
        {
            pUV.Add(new Vector2(1, 0));
            pUV.Add(new Vector2(1, 0));
        }
    }
}

#if UNITY_EDITOR

namespace MD_PluginEditor
{
    [CustomEditor(typeof(MD_PathCreator))]
    public class EDITOR_MD_PathCreatorEditor : MD_EditorUtilities
    {
        private MD_PathCreator tc;

        private void OnEnable()
        {
            tc = (MD_PathCreator)target;
        }

        private GameObject obj;

        public override void OnInspectorGUI()
        {
            s(10);
            l("Mesh Settings");
            bv();
            DrawProperty("pSize", "Path Size");
            DrawProperty("pNodeSize", "Node Size");
            s();
            DrawProperty("pRevertNormals", "Revert Normals");
            DrawProperty("pApplyNodeLocalScale", "Apply Node Local Scale", "If enabled, the path size will change according to nodes local size");
            ev();

            s(10);
            l("Node Settings");
            bv();
            l("Current Node Count: " + tc.pNodes.Count.ToString());
            DrawProperty("pNodes", "Nodes", "Array of available path nodes", true);
            s();
            obj = EditorGUILayout.ObjectField(new GUIContent("Root Node"), (Object)obj, typeof(GameObject), true) as GameObject;
            if(GUILayout.Button("Load Nodes From Root Node To Path") && obj!=null)
            {
                tc.pNodes.Clear();
                foreach (Transform node in obj.GetComponentsInChildren<Transform>())
                    tc.pNodes.Add(node);
                tc.PUBLICpFunct_RefreshNodes();
            }
            s();
            l("Node Editor Manipulation");
            bv();
            bh();
            if (b("Add Node"))
                tc.PUBLICpFunct_AddNode(Vector3.zero);
            if (b("Remove Node"))
                tc.PUBLICpFunct_RemoveNode();
            eh();
            bh();
            if (b("Ungroup Nodes"))
                tc.PUBLICpFunct_UngroupNodes(null);
            if (b("Group All Nodes Together"))
                tc.PUBLICpFunct_GroupNodesTogether();
            eh();
            ev();
            ev();
            s(10);
            l("Logic Settings");
            bv();
            DrawProperty("pUpdateEveryFrame", "Update Every Frame");
            if (!tc.pUpdateEveryFrame)
            {
                if (GUILayout.Button("Refresh Nodes"))
                    tc.PUBLICpFunct_RefreshNodes();
            }
            DrawProperty("pEnableSmartRotation", "Enable Smart Rotation", "If enabled, the nodes will rotate naturally towards the direction");
            ev();

            s();

            DrawProperty("pEnableDebug", "Enable Debug");

            s(10);

            bh();
            if (GUILayout.Button("Show All Nodes"))
            {
                foreach (Transform p in tc.pNodes)
                    p.GetComponent<MeshRenderer>().enabled = true;
            }
            if (GUILayout.Button("Hide All Nodes"))
            {
                foreach (Transform p in tc.pNodes)
                    p.GetComponent<MeshRenderer>().enabled = false;
            }
            eh();
            if (GUILayout.Button("Clear All Nodes"))
            {
                if (EditorUtility.DisplayDialog("Warning", "You are about to clear all nodes and whole path mesh. Are you sure? There is no way back.", "OK", "Cancel"))
                    tc.PUBLICpFunct_ClearAll();
            }

            s();
            Color c;
            ColorUtility.TryParseHtmlString("#f2d0d0", out c);
            GUI.color = c;
            GUILayout.Space(5);
            if (GUILayout.Button("Back To Mesh Editor"))
            {
                GameObject gm = tc.gameObject;
                DestroyImmediate(tc);
                gm.AddComponent<MD_MeshProEditor>();
            }
        }

        private void l(string label)
        {
            GUILayout.Label(label);
        }
        private void bh()
        {
            GUILayout.BeginHorizontal("Box");
        }
        private void eh()
        {
            GUILayout.EndHorizontal();
        }
        private void bv()
        {
            GUILayout.BeginVertical("Box");
        }
        private void ev()
        {
            GUILayout.EndVertical();
        }
        private bool b(string txt, int size = 0, Texture2D icon = null)
        {
            if (size == 0)
                return GUILayout.Button(new GUIContent(txt, icon));
            else
                return GUILayout.Button(txt, GUILayout.Width(size));
        }

        private void s(float sp = 5)
        {
            GUILayout.Space(sp);
        }
        private void DrawProperty(string PropertyName, string Text = "", string ToolTip = "", bool includeChilds = false, Texture img = null)
        {
            try
            {
                if (string.IsNullOrEmpty(Text))
                    Text = PropertyName;
                if (img == null)
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(PropertyName), new GUIContent(Text, ToolTip), includeChilds, null);
                else
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(PropertyName), new GUIContent(Text, img), includeChilds, null);
            }
            catch { Debug.Log("Error with " + PropertyName); }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
