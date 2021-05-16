using System.Collections.Generic;
using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using MD_Plugin;
#endif

namespace MD_Plugin
{
    [AddComponentMenu(MD_Debug.ORGANISATION + MD_Debug.PACKAGENAME + "Tunnel Creator/Tunnel Creator Source")]
    [ExecuteInEditMode]
    public class MD_TunnelCreator : MonoBehaviour
    {
        [Range(4, 64)]
        public int tVertexCount = 16;
        public int tVertexCountInternal = 0;
        public float tRadius = 1.0f;
        public float tNodeSize = 0.2f;

        public bool tHemiCylinder = false;
        public bool tHemiCylinderVertical = false;
        public bool tRevertNormals = false;
        public bool tApplyNodeLocalScale = false;

        public bool tUpdateEveryFrame = true;
        public bool tEnableSmartRotation = true;

        public bool tUseCustomUVData = false;
        public MD_TunnelNodeUVData._UVMode tUVMode = MD_TunnelNodeUVData._UVMode.uvZX;

        public bool tEnableDebug = true;

        public List<Transform> tNodes = new List<Transform>();

        public List<Vector3> tVertices = new List<Vector3>();
        public List<int> tTriangles = new List<int>();
        public List<Vector2> tUV = new List<Vector2>();

        public Mesh tCurrentMesh;
        public Texture2D tc_ICON_ShowW;

#if UNITY_EDITOR
        [MenuItem("GameObject/3D Object" + MD_Debug.PACKAGENAME + "Advanced/Tunnel Creator")]
        public static void GenerateTunnelObj()
        {
            GameObject newTunnel = new GameObject("TunnelCreator");
            newTunnel.AddComponent<MD_TunnelCreator>();
            Selection.activeGameObject = newTunnel;
            newTunnel.transform.position = Vector3.zero;

            newTunnel.AddComponent<MeshFilter>();
            newTunnel.AddComponent<MeshRenderer>();

            Material mat = new Material(Shader.Find("Diffuse"));
            newTunnel.GetComponent<Renderer>().sharedMaterial = mat;
        }
#endif

        private void OnDrawGizmos()
        {
            if (!tEnableDebug)
                return;
            Gizmos.color = Color.cyan;
            if (tNodes.Count == 0)
                return;
            for (int i = 0; i < tNodes.Count; i++)
            {
                if (tNodes[i] == null)
                    continue;
                if (i != 0)
                    Gizmos.DrawLine(tNodes[i].position, tNodes[i - 1].position);
            }
        }

        private void Awake()
        {
            if (tCurrentMesh == null)
            {
                tCurrentMesh = new Mesh();
                tVertexCountInternal = tVertexCount;
            }
        }

        private void Update()
        {
            if (!tUpdateEveryFrame)
                return;
            PUBLICtFunct_RefreshNodes();
        }

        //----INTERNAL/PUBLIC - Nodes Manipulations
        #region Accessible functions - Nodes Manipulation

        /// <summary>
        /// Add node on specific position
        /// </summary>
        public void PUBLICtFunct_AddNode(Vector3 toPos, bool GroupOnAdd = true)
        {
            if (Application.isPlaying)
                return;
            Awake();

            if (tNodes.Count == 0)
                tFunctCreateNodeBlock((toPos == Vector3.zero) ? Vector3.zero : toPos, GroupOnAdd);
            else
                tFunctCreateNodeBlock((toPos == Vector3.zero) ? tNodes[tNodes.Count - 1].position + tNodes[tNodes.Count - 1].forward * 2 : toPos, GroupOnAdd);
        }
        /// <summary>
        /// Remove last node
        /// </summary>
        public void PUBLICtFunct_RemoveNode()
        {
            if (Application.isPlaying)
                return;
            Awake();

            tFunctRemoveNodeBlock();
        }
        /// <summary>
        /// Clear all nodes
        /// </summary>
        public void PUBLICtFunct_ClearAll()
        {
            if (Application.isPlaying)
                return;

            tVertices.Clear();
            tTriangles.Clear();
            tUV.Clear();

            int c = tNodes.Count;
            for (int i = 0; i < c; i++)
            {
                if (tNodes[i] == null)
                    continue;
                DestroyImmediate(tNodes[i].gameObject);
            }

            tNodes.Clear();

            if(GetComponent<MeshFilter>())
            GetComponent<MeshFilter>().sharedMesh = null;
            tCurrentMesh = null;

            Awake();
        }
        /// <summary>
        /// Refresh current tunnel mesh
        /// </summary>
        public void PUBLICtFunct_RefreshNodes()
        {
            if (tNodes.Count == 0)
                return;
            if (Application.isPlaying)
                return;

            if (tVertexCountInternal != tVertexCount)
                return;

            int iiindex = 0;

            for (int i = 0; i < tNodes.Count; i++)
            {
                if (tNodes.Count > 1 && i > 0 && tEnableSmartRotation)
                    tNodes[i].rotation = Quaternion.LookRotation(tNodes[i].position - tNodes[i - 1].position);
                tFunctRefreshMesh(iiindex, i);
                iiindex += tVertexCount;
            }

            if (tRevertNormals)
            {
                tCurrentMesh.triangles = tCurrentMesh.triangles.Reverse().ToArray();
                tCurrentMesh.normals = tCurrentMesh.normals.Reverse().ToArray();
            }
            else
                tCurrentMesh.triangles = tCurrentMesh.triangles.ToArray();

            tCurrentMesh.RecalculateBounds();
            GetComponent<MeshFilter>().mesh = tCurrentMesh;
        }

        /// <summary>
        /// Apply changed vertex count and refresh
        /// </summary>
        public void PUBLICtFunct_ApplyVertexCount()
        {
            tVertexCountInternal = tVertexCount;

            if (tNodes.Count <= 1)
                return;
            List<Vector3> verts = new List<Vector3>();

            foreach (Transform item in tNodes)
                verts.Add(item.position);

            tFunct_ClearAllInternal();
            for (int i = 0; i < verts.Count; i++)
                PUBLICtFunct_AddNode(verts[i]);

            PUBLICtFunct_UngroupNodes(null);

            fFunctUpdateMeshParams();
            PUBLICtFunct_RefreshNodes();
        }

        private void tFunct_ClearAllInternal()
        {
            if (Application.isPlaying)
                return;

            for (int i = tNodes.Count - 1; i >= 0; i--)
                DestroyImmediate(tNodes[i].gameObject);
            tNodes.Clear();

            tVertices.Clear();
            tTriangles.Clear();
            tUV.Clear();

            GetComponent<MeshFilter>().sharedMesh = null;
            tCurrentMesh = null;

            Awake();
        }

        /// <summary>
        /// Group all nodes together in hierarchy
        /// </summary>
        public void PUBLICtFunct_GroupNodesTogether()
        {
            for (int i = 0; i < tNodes.Count; i++)
            {
                if (i <= 0)
                    continue;
                tNodes[i].parent = tNodes[i - 1];
            }
        }
        /// <summary>
        /// Ungroup all nodes to 'empty' or to 'some object'
        /// </summary>
        public void PUBLICtFunct_UngroupNodes(Transform Detachto)
        {
            for (int i = 0; i < tNodes.Count; i++)
            {
                if (i <= 0)
                    continue;
                tNodes[i].parent = (Detachto) ? Detachto : null;
            }
        }

        #endregion

        //----INTERNAL - Creating tunnel completed blocks
        private void tFunctCreateNodeBlock(Vector3 OriginPosition, bool GroupOnAdd = true)
        {
            Transform newOrigin = MD_Octahedron.Generate().transform;
            newOrigin.name = "Node" + tNodes.Count.ToString();
            newOrigin.localScale = Vector3.one * tNodeSize;
            DestroyImmediate(newOrigin.GetComponent<SphereCollider>());
            newOrigin.position = OriginPosition;

            if (GroupOnAdd && tNodes.Count >= 1)
                newOrigin.transform.parent = tNodes[tNodes.Count - 1].transform;

            tNodes.Add(newOrigin.transform);

            tProcess_CreateVerticePanel(newOrigin);
            tProcess_CreateUV();

            if (tNodes.Count <= 1)
                return;

            tProcess_CreateTrianglePanel();

            fFunctUpdateMeshParams();
        }
        private void tFunctRemoveNodeBlock()
        {
            if (tNodes.Count == 0)
                return;
            else if (tNodes.Count == 1)
            {
                DestroyImmediate(tNodes[tNodes.Count - 1].gameObject);
                tNodes.RemoveAt(tNodes.Count - 1);
                tVertices.RemoveRange(tVertices.Count - tVertexCount, tVertexCount);
                tUV.RemoveRange(tUV.Count - (tVertexCount), tVertexCount);
                return;
            }

            DestroyImmediate(tNodes[tNodes.Count - 1].gameObject);
            tNodes.RemoveAt(tNodes.Count - 1);

            tTriangles.RemoveRange(tTriangles.Count - (tVertexCount * 6), tVertexCount * 6);
            tUV.RemoveRange(tUV.Count - (tVertexCount), tVertexCount);
            tVertices.RemoveRange(tVertices.Count - tVertexCount, tVertexCount);

            fFunctUpdateMeshParams(true);
        }

        private void tFunctRefreshMesh(int VertexQueue, int OriginQueue)
        {
            Transform OriginPosition = tNodes[OriginQueue];
            float deltaTheta = (((tHemiCylinder) ? 1 : 2) * Mathf.PI) / tVertexCount;
            float currentTheta = 0;

            for (int i = VertexQueue; i < VertexQueue + tVertexCount; i++)
            {
                Matrix4x4 m = Matrix4x4.TRS(OriginPosition.position, OriginPosition.rotation, tApplyNodeLocalScale ? OriginPosition.localScale : Vector3.one);
                Vector3 pos;
                if (tHemiCylinderVertical)
                    pos = new Vector3(tRadius * Mathf.Sin(currentTheta), tRadius * Mathf.Cos(currentTheta), 0);
                else
                    pos = new Vector3(tRadius * -Mathf.Cos(currentTheta), tRadius * Mathf.Sin(currentTheta), 0);

                pos = m.MultiplyPoint3x4(pos);
                currentTheta += deltaTheta;
                tVertices[i] = pos;
            }

            if (tUseCustomUVData)
                tFunctUpdateUVsAtIndex(OriginQueue);
            else
                PUBLICtFunct_UpdateUVs(tUVMode);

            fFunctUpdateMeshParams();
        }

        private void fFunctUpdateMeshParams(bool deletingMesh = false)
        {
            if (!deletingMesh)
                tCurrentMesh.vertices = tVertices.ToArray();
            tCurrentMesh.triangles = tTriangles.ToArray();
            if (deletingMesh)
                tCurrentMesh.vertices = tVertices.ToArray();
            tCurrentMesh.RecalculateNormals();
            tCurrentMesh.RecalculateTangents();
            tCurrentMesh.RecalculateBounds();
            if (!GetComponent<MeshFilter>())
                gameObject.AddComponent<MeshFilter>();
            if(!GetComponent<MeshRenderer>())
                gameObject.AddComponent<MeshRenderer>();
            GetComponent<MeshFilter>().sharedMesh = tCurrentMesh;
        }

        //----INTERNAL/PUBLIC - UV Managing
        #region Accessible functions - UV Manipulation

        /// <summary>
        /// Update UV sets with specific UV mode
        /// </summary>
        public void PUBLICtFunct_UpdateUVs(MD_TunnelNodeUVData._UVMode uvMode)
        {
            for (int v = 0; v < tVertices.Count; v++)
            {
                float uvModeX = tVertices[v].x;
                float uvModeY = tVertices[v].y;
                switch (uvMode)
                {
                    case MD_TunnelNodeUVData._UVMode.uvXY:
                        uvModeX = tVertices[v].x;
                        uvModeY = tVertices[v].y;
                        break;

                    case MD_TunnelNodeUVData._UVMode.uvXZ:
                        uvModeX = tVertices[v].x;
                        uvModeY = tVertices[v].z;
                        break;

                    case MD_TunnelNodeUVData._UVMode.uvYX:
                        uvModeX = tVertices[v].y;
                        uvModeY = tVertices[v].x;
                        break;
                    case MD_TunnelNodeUVData._UVMode.uvYZ:
                        uvModeX = tVertices[v].y;
                        uvModeY = tVertices[v].z;
                        break;

                    case MD_TunnelNodeUVData._UVMode.uvZX:
                        uvModeX = tVertices[v].z;
                        uvModeY = tVertices[v].x;
                        break;
                    case MD_TunnelNodeUVData._UVMode.uvZY:
                        uvModeX = tVertices[v].z;
                        uvModeY = tVertices[v].y;
                        break;
                }
                tUV[v] = new Vector2(uvModeX, uvModeY);
            }
            if(tUV.Count == tCurrentMesh.vertexCount)
                tCurrentMesh.uv = tUV.ToArray();
        }

        #endregion

        private void tFunctUpdateUVsAtIndex(int originQueue)
        {
            int atIndex = tVertexCount * originQueue + tVertexCount;
            int vCount = tVertexCount;
            int counter = 0;
            bool applyFirstRound = false;
            for (int i = originQueue + 1; i < tNodes.Count; i++)
            {
                if (i >= tNodes.Count)
                    break;
                if (tNodes[i].GetComponent<MD_TunnelNodeUVData>())
                    break;
                counter++;
            }
            if (counter != 0)
                vCount *= counter;
            if (originQueue > 0)
                applyFirstRound = true;

            int indexLength = atIndex + vCount;
            MD_TunnelNodeUVData uvdat;

            if (tNodes[originQueue].GetComponent<MD_TunnelNodeUVData>())
                uvdat = tNodes[originQueue].GetComponent<MD_TunnelNodeUVData>();
            else return;

            bool firstRoundCompleted = false;
            int firstFullVertArray = 0;
            for (int v = atIndex; v < indexLength; v++)
            {
                firstFullVertArray++;
                Vector2 uvcor = tReturnCorrectUVMode(uvdat.UVMode, v);
                if (!firstRoundCompleted && applyFirstRound)
                    tUV[v - tVertexCount] = uvcor + uvdat.UvTransition;
                if (firstFullVertArray >= tVertexCount && applyFirstRound && !firstRoundCompleted)
                    firstRoundCompleted = true;

                tUV[v] = uvcor + uvdat.UvOffset;
            }
            tCurrentMesh.uv = tUV.ToArray();
        }
        private Vector2 tReturnCorrectUVMode(MD_TunnelNodeUVData._UVMode uvMode, int v)
        {
            switch (uvMode)
            {
                default:
                    return new Vector2(tVertices[v].x, tVertices[v].y);
                case MD_TunnelNodeUVData._UVMode.uvXZ:
                    return new Vector2(tVertices[v].x, tVertices[v].z);

                case MD_TunnelNodeUVData._UVMode.uvYX:
                    return new Vector2(tVertices[v].y, tVertices[v].x);
                case MD_TunnelNodeUVData._UVMode.uvYZ:
                    return new Vector2(tVertices[v].y, tVertices[v].z);

                case MD_TunnelNodeUVData._UVMode.uvZX:
                    return new Vector2(tVertices[v].z, tVertices[v].x);
                case MD_TunnelNodeUVData._UVMode.uvZY:
                    return new Vector2(tVertices[v].z, tVertices[v].y);
            }
        }

        //----INTERNAL - Creating vertice connectors
        private void tProcess_CreateVerticePanel(Transform newOrigin)
        {
            float deltaTheta = (2 * Mathf.PI) / tVertexCount;
            float currentTheta = 0;

            for (int i = 0; i < tVertexCount; i++)
            {
                Vector3 pos = new Vector3(tRadius * Mathf.Sin(currentTheta), tRadius * Mathf.Cos(currentTheta), newOrigin.position.z);
                currentTheta += deltaTheta;

                tVertices.Add(pos);
            }
        }
        private void tProcess_CreateTrianglePanel()
        {
            int lastVerticeIndex = tVertices.Count - (tVertexCount * 2);
            for (int i = lastVerticeIndex; i < tVertices.Count - tVertexCount; i++)
                tProcess_CreateFace(i, tVertexCount, tVertices.Count);
        }
        private void tProcess_CreateFace(int index, int maxAdd, int maxCount)
        {
            int i = index;
            int LargestAdd = maxAdd;
            bool final = false;

            if (i >= maxCount - maxAdd - 1)
                final = true;

            int[] Faces;

            if (final)
            {
                Faces = new int[]
                {
                i - LargestAdd + 1, i + 1, i + LargestAdd,
                i - LargestAdd + 1, i + LargestAdd, i,
                };
            }
            else
            {
                Faces = new int[]
                {
                i + 1, i + LargestAdd, i,
                i + 1, i + LargestAdd + 1, i + LargestAdd,
                };
            }

            tTriangles.AddRange(Faces);
        }
        private void tProcess_CreateUV()
        {
            int lastVerticeIndex = tVertices.Count - tVertexCount;
            for (int i = lastVerticeIndex; i < tVertices.Count; i++)
                tUV.Add(new Vector2(0, 0));
        }
    }
}

#if UNITY_EDITOR

namespace MD_PluginEditor
{
    [CustomEditor(typeof(MD_TunnelCreator))]
    public class EDITOR_TunnelCreatorEditor : MD_EditorUtilities
    {
        private MD_TunnelCreator tc;

        private void OnEnable()
        {
            tc = (MD_TunnelCreator)target;
        }

        private GameObject obj;

        public override void OnInspectorGUI()
        {
            s(10);
            l("Mesh Settings");
            bv();
            DrawProperty("tVertexCount", "Tunnel Vertex Count");
            if (tc.tVertexCountInternal != tc.tVertexCount)
                EditorGUILayout.HelpBox("The vertex count has changed. Press Apply to change the tunnel vertex count. [" + tc.tVertexCountInternal.ToString() + "]", MessageType.Info);
            if (GUILayout.Button("Apply Vertex Count", GUILayout.Width(125)))
            {
                if (EditorUtility.DisplayDialog("Warning", "You are about to apply new vertex count. This will reset all created nodes but the path will remain (All node components will be removed). Are you sure to continue?", "Yes", "No"))
                    tc.PUBLICtFunct_ApplyVertexCount();
            }
            s();
            DrawProperty("tRadius", "Tunnel Radius");
            DrawProperty("tNodeSize", "Node Size");
            s();
            DrawProperty("tHemiCylinder", "Hemi Tunnel");
            if (tc.tHemiCylinder)
                DrawProperty("tHemiCylinderVertical", "Vertical Hemi Tunnel");
            DrawProperty("tRevertNormals", "Revert Normals");
            DrawProperty("tApplyNodeLocalScale", "Apply Node Local Scale", "If enabled, the tunnel radius will change according to nodes local size");
            ev();

            s(10);
            l("Node Settings");
            bv();
            l("Current Node Count: " + tc.tNodes.Count.ToString());
            DrawProperty("tNodes", "Nodes", "Array of available tunnel nodes", true);
            s();
            obj = EditorGUILayout.ObjectField(new GUIContent("Root Node"), (Object)obj, typeof(GameObject), true) as GameObject;
            if(GUILayout.Button("Load Nodes From Root Node To Tunnel") && obj!=null)
            {
                tc.tNodes.Clear();
                foreach (Transform node in obj.GetComponentsInChildren<Transform>())
                    tc.tNodes.Add(node);
                tc.PUBLICtFunct_RefreshNodes();
            }
            ev();

            s(10);
            l("UV Settings");
            bv();
            DrawProperty("tUseCustomUVData", "Use Custom UV Data", "If enabled, the UV data will be get from the nodes that contain MDM_TunnelNodeUVData behaviour");
            if (!tc.tUseCustomUVData)
            {
                DrawProperty("tUVMode", "UV Mode");
                if (tc.tUpdateEveryFrame == false)
                {
                    if (GUILayout.Button("Refresh UVs"))
                        tc.PUBLICtFunct_UpdateUVs(tc.tUVMode);
                }
            }
            else EditorGUILayout.HelpBox("UV will be set from the tunnel nodes", MessageType.Info);
            ev();

            s(10);
            l("Logic Settings");
            bv();
            DrawProperty("tUpdateEveryFrame", "Update Every Frame");
            if (!tc.tUpdateEveryFrame)
            {
                if (GUILayout.Button("Refresh Nodes"))
                    tc.PUBLICtFunct_RefreshNodes();
            }
            DrawProperty("tEnableSmartRotation", "Enable Smart Rotation", "If enabled, the nodes will rotate naturally towards the direction");
            ev();

            s();

            DrawProperty("tEnableDebug", "Enable Debug");

            s(10);
            if (GUILayout.Button(new GUIContent("Show Tunnel Editor", tc.tc_ICON_ShowW)))
                MD_TunnelCreatorEditorWindow.Init(tc);

            s(10);

            bh();
            if (GUILayout.Button("Show All Points"))
            {
                foreach (Transform p in tc.tNodes)
                    p.GetComponent<MeshRenderer>().enabled = true;
            }

            if (GUILayout.Button("Hide All Points"))
            {
                foreach (Transform p in tc.tNodes)
                    p.GetComponent<MeshRenderer>().enabled = false;
            }
            eh();
            if (GUILayout.Button("Clear All"))
            {
                if (EditorUtility.DisplayDialog("Warning", "You are about to clear all nodes and whole tunnel mesh. Are you sure? There is no way back.", "OK", "Cancel"))
                    tc.PUBLICtFunct_ClearAll();
            }

            s();
            Color c = Color.black;
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
