using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using MD_Plugin;
#endif

#if UNITY_EDITOR
namespace MD_PluginEditor
{
    /// <summary>
    /// Editor Only! Use MD_VertexToolModifier instead
    /// </summary>
    public class MD_VertexTool : MD_EditorWindowUtilities
    {
        [MenuItem("Window/MD_Package/Vertex Tool")]

        public static void Init()
        {
            MD_VertexTool vt = (MD_VertexTool)GetWindow(typeof(MD_VertexTool));
            vt.maxSize = new Vector2(110, 370);
            vt.minSize = new Vector2(109, 369);
            vt.titleContent = new GUIContent("VT");
            vt.Show();
        }

        private bool Active = false;

        private Object GroupObject;

        private int MultiplyCounter = 1;
        private Vector3 MultiplyAngle;
        private Vector3 MultiplyMoveOffset = new Vector3(1, 0, 0);

        public Texture2D _Attach;
        public Texture2D _Clone;
        public Texture2D _Weld;
        public Texture2D _Relax;

        private void OnGUI()
        {
            if (!Active)
                GUI.color = Color.gray;
            else
                GUI.color = Color.white;
            GUILayout.BeginVertical("Box");

            if (Selection.gameObjects.Length > 0)
                Active = true;
            else
                Active = false;

            GUI.color = Color.yellow;
            GUILayout.BeginVertical("Box");
            GUILayout.Label("Element");
            GUILayout.EndVertical();

            if (Selection.gameObjects.Length <= 1)
                GUI.color = Color.gray;
            else
                GUI.color = Color.white;

            //---------------Attach Function----------------
            //----------------------------------------------

            if (GUILayout.Button(new GUIContent("Attach", _Attach)))
            {
                if (!Active)
                    return;
                MD_VertexToolModifier.V_TOOL_Element_Attach(Selection.activeGameObject);
            }
            GUILayout.Space(5);

            if (!Active)
                GUI.color = Color.gray;
            else
                GUI.color = Color.white;

            //---------------Clone Function----------------
            //----------------------------------------------

            if (GUILayout.Button(new GUIContent("Clone", _Clone)))
            {
                if (!Active)
                    return;
                if (Selection.gameObjects.Length == 0)
                    return;
                MD_VertexToolModifier.V_TOOL_Element_Clone(Selection.activeGameObject, MultiplyCounter, MultiplyAngle, MultiplyMoveOffset);
            }
            MultiplyCounter = EditorGUILayout.IntField(MultiplyCounter);
            MultiplyMoveOffset = EditorGUILayout.Vector3Field("", MultiplyMoveOffset);
            MultiplyAngle = EditorGUILayout.Vector3Field("", MultiplyAngle);
            GUILayout.Space(10);

            GUI.color = Color.cyan;
            GUILayout.BeginVertical("Box");
            GUILayout.Label("Vertex");
            GUILayout.EndVertical();

            if (!Active)
                GUI.color = Color.gray;
            else
                GUI.color = Color.white;

            //---------------Weld Function----------------
            //----------------------------------------------

            if (GUILayout.Button(new GUIContent("Weld", _Weld)))
            {
                if (!Active)
                    return;
                if (Selection.gameObjects.Length == 0 || Selection.gameObjects.Length <= 1)
                    return;
                if (Selection.gameObjects[0] == null || Selection.gameObjects[1] == null)
                    return;
                MD_VertexToolModifier.V_TOOL_Vertex_Weld(Selection.gameObjects[0].transform, Selection.gameObjects[1].transform);
            }
            GUILayout.Space(5);
            if (GUILayout.Button(new GUIContent("Relax", _Relax)))
            {
                if (!Active)
                    return;
                MD_VertexToolModifier.V_TOOL_Vertex_Relax(Selection.activeGameObject.transform.GetComponent<MD_MeshProEditor>());
            }

            GUILayout.Space(10);

            if (Selection.gameObjects.Length == 0)
                GUI.color = Color.gray;
            else
                GUI.color = Color.white;
            GUILayout.BeginVertical("Box");
            GUILayout.Label("Scene");
            GUILayout.EndVertical();

            if (GUILayout.Button("Group [" + Selection.gameObjects.Length.ToString() + "]"))
            {
                if (Selection.gameObjects.Length < 1 || GroupObject == null)
                    return;
                foreach (GameObject gm in Selection.gameObjects)
                {
                    gm.transform.parent = ((GameObject)GroupObject as GameObject).transform;
                }
            }
            if (Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<MD_MeshProEditor>() && Selection.activeGameObject.GetComponent<MD_MeshProEditor>().ppEnableZoneGenerator)
                if (GUILayout.Button("Group Enabled"))
                {
                    MD_MeshProEditor p = Selection.activeGameObject.GetComponent<MD_MeshProEditor>();
                    if (p.workingTargetPoints.Count == 0 || GroupObject == null)
                        return;
                    foreach (Transform gm in p.workingTargetPoints)
                    {
                        if (gm.gameObject.activeInHierarchy)
                            gm.transform.parent = ((GameObject)GroupObject as GameObject).transform;
                    }
                }
            GroupObject = EditorGUILayout.ObjectField(GroupObject, typeof(GameObject), true);
            GUILayout.EndVertical();
        }
    }
}
#endif

namespace MD_Plugin
{
    /// <summary>
    /// Vertex tool modifier for advanced functionality
    /// </summary>
    public class MD_VertexToolModifier : MonoBehaviour
    {
        /// <summary>
        /// Attach 2 or more meshes [Meshes will be combined and will share the same material]​
        /// </summary>
        public static void V_TOOL_Element_Attach(GameObject RootObject)
        {
            GameObject AttachRoot = RootObject;

            if (!AttachRoot)
            {
                MD_Debug.Debug(null, "VertexTool: {ATTACH FUNCTION} At least, one object must be selected", MD_Debug.DebugType.Error);
                return;
            }
            if (!AttachRoot.GetComponent<MeshFilter>())
            {
                MD_Debug.Debug(null, "VertexTool: {ATTACH FUNCTION} The sender object doesn't contain Mesh Filter component", MD_Debug.DebugType.Error);
                return;
            }
            if (!AttachRoot.GetComponent<MD_MeshProEditor>())
            {
                MD_Debug.Debug(null, "VertexTool: {ATTACH FUNCTION} The sender object doesn't contain Mesh Pro Editor component", MD_Debug.DebugType.Error);
                return;
            }

            foreach (MeshFilter gm in RootObject.GetComponentsInChildren<MeshFilter>())
            {
                if (gm.GetComponent<MD_MeshProEditor>())
                    gm.GetComponent<MD_MeshProEditor>().MPE_ClearVerticeEditor();
                if (AttachRoot == gm)
                    continue;
                if (gm.GetComponent<Renderer>())
                    gm.transform.parent = AttachRoot.transform;
            }
            AttachRoot.GetComponent<MD_MeshProEditor>().MPE_CombineMeshQuick();
        }

        /// <summary>
        /// Clone selected mesh [You can see parameters below],​ Count – how many times will be the mesh cloned,​ Move Offset – which direction will be the mesh cloned,​ Rotation Angle – which rotation will be the mesh cloned.
        /// </summary>
        public static void V_TOOL_Element_Clone(GameObject ObjectToClone, int Count, Vector3 RotationAngle, Vector3 MoveOffset)
        {
            GameObject SelectedObj = ObjectToClone;
            if (!SelectedObj.GetComponent<MD_MeshProEditor>())
            {
                MD_Debug.Debug(null, "VertexTool: {CLONE FUNCTION} The selected object must contains Mesh Pro Editor to clone meshes inside...", MD_Debug.DebugType.Error);
                return;
            }

            SelectedObj.GetComponent<MD_MeshProEditor>().MPE_ClearVerticeEditor();
            SelectedObj.GetComponent<MD_MeshProEditor>().ppSelectedModification = MD_MeshProEditor.SelectedModification.None;

            Vector3 offset = SelectedObj.transform.position;
            if (SelectedObj.GetComponent<MD_MeshProEditor>().myStartupBounds != SelectedObj.GetComponent<MeshFilter>().sharedMesh.bounds.max)
                offset = SelectedObj.transform.position + new Vector3(SelectedObj.GetComponent<MeshFilter>().sharedMesh.bounds.max.x * MoveOffset.x, SelectedObj.GetComponent<MeshFilter>().sharedMesh.bounds.max.y * MoveOffset.y, SelectedObj.GetComponent<MeshFilter>().sharedMesh.bounds.max.z * MoveOffset.z);

            Vector3 rotOffset = Vector3.zero;
            List<GameObject> clones = new List<GameObject>();
            for (int i = 0; i < Count; i++)
            {
                offset += MoveOffset;
                rotOffset += RotationAngle;
                GameObject Cloned = Instantiate(SelectedObj, null);
                DestroyImmediate(Cloned.GetComponent<MD_MeshProEditor>());
                Cloned.transform.position = offset;
                Cloned.transform.rotation = Quaternion.Euler(rotOffset);
                clones.Add(Cloned);
            }
            foreach (GameObject g in clones)
                g.transform.parent = SelectedObj.transform;
            SelectedObj.GetComponent<MD_MeshProEditor>().MPE_CombineMeshQuick();
        }

        /// <summary>
        /// Weld selected vertices [Vertices will be welded – they will split into one]​
        /// </summary>
        public static void V_TOOL_Vertex_Weld(Transform PointA, Transform PointB)
        {
            PointA.parent = PointB;
            PointA.localPosition = Vector3.zero;
            PointA.gameObject.SetActive(false);
            PointA.hideFlags = HideFlags.HideInHierarchy;
        }

        /// <summary>
        /// Relax mesh vertices [Vertices will be normalized and their offset will be multiplied by the position of their mesh]
        /// </summary>
        public static void V_TOOL_Vertex_Relax(MD_MeshProEditor Sender)
        {
            if (Sender == null)
                return;
            if (Sender.workingTargetPoints == null)
                return;
            if (Sender.workingTargetPoints.Count > 0)
            {
                foreach (Transform points in Sender.workingTargetPoints)
                {
                    points.transform.LookAt(points.transform.root.transform);
                    points.transform.position += points.transform.forward * Vector3.Distance(points.transform.localPosition, Vector3.zero) / 2;
                }
            }
        }
    }
}
