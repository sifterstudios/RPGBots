using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using MD_Plugin;
#endif

namespace MD_Plugin
{
    /// <summary>
    /// MDM(Mesh Deformation Modifier): Mesh Fit
    /// Modify mesh vertices by surface height
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter))]
    [AddComponentMenu(MD_Debug.ORGANISATION + MD_Debug.PACKAGENAME + "Modifiers/Mesh Fit")]
    public class MDM_MeshFit : MonoBehaviour
    {
        public bool ppUpdateEveryFrame = false;

        [SerializeField] private MeshFilter meshFilter;
        public bool ppAutoRecalculateNormals = true;
        public bool ppAutoRecalculateBounds = true;

        //---Vertices & Points
        public List<Vector3> verts = new List<Vector3>();
        public List<Transform> points = new List<Transform>();

        public Vector3[] originalVertices;
        public Vector3[] workingVertices;

        public float ppMODIF_MeshFitterOffset = 0.03f;
        public float ppMODIF_MeshFitterSurfaceDetection = 3;
        public enum MeshFitterMode { FitWholeMesh, FitSpecificVertices };
        public MeshFitterMode ppMODIF_MeshFitterType = MeshFitterMode.FitSpecificVertices;
        public bool ppMODIF_MeshFitterContinuousEffect = false;
        public GameObject[] ppMODIF_MeshFitter_SelectedVertexes;

        public bool GotOriginal = false;

        private GameObject vertexRoot;

        private void Awake()
        {
            if (meshFilter != null) return;

            ppAutoRecalculateBounds = MD_GlobalPreferences.autoRecalcBounds;
            ppAutoRecalculateNormals = MD_GlobalPreferences.autoRecalcNormals;

            meshFilter = GetComponent<MeshFilter>();
            MD_MeshProEditor.MeshProEditor_Utilities.util_PrepareMeshDeformationModifier(this, meshFilter);

            originalVertices = meshFilter.sharedMesh.vertices;
            workingVertices = meshFilter.sharedMesh.vertices;
        }

        private void Update()
        {
            if (ppUpdateEveryFrame) MeshFit_UpdateMeshState();
        }

        /// <summary>
        /// Refresh vertices active state
        /// </summary>
        internal void MeshFit_RefreshVerticesActiveState()
        {
            if (points == null || ppMODIF_MeshFitter_SelectedVertexes == null) return;
            if (points.Count == 0 || ppMODIF_MeshFitter_SelectedVertexes.Length == 0) return;

            foreach (Transform gm in points)
                gm.gameObject.SetActive(false);
            foreach (GameObject gm in ppMODIF_MeshFitter_SelectedVertexes)
                gm.SetActive(true);
        }

        /// <summary>
        /// Show/ Hide points
        /// </summary>
        public void MeshFit_ShowHidePoints(bool activation)
        {
            if (points == null) return;
            if (points.Count == 0) return;

            if (ppMODIF_MeshFitter_SelectedVertexes != null && ppMODIF_MeshFitter_SelectedVertexes.Length > 0)
            {
                foreach (GameObject p in ppMODIF_MeshFitter_SelectedVertexes)
                {
                    if (p && p.GetComponent<Renderer>())
                        p.GetComponent<Renderer>().enabled = activation;
                }
                return;
            }
            foreach (Transform p in points)
            {
                if (p.transform.parent.name.Contains(this.name) && p.GetComponent<Renderer>())
                    p.GetComponent<Renderer>().enabled = activation;
            }
        }
        /// <summary>
        /// Generate points on the mesh
        /// </summary>
        public void MeshFit_GeneratePoints()
        {
            MeshFit_ClearPoints();

            Vector3 lastPos = transform.position;
            Quaternion lastRot = transform.rotation;

            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;

            vertexRoot = new GameObject("VertexRoot_" + this.name);
            vertexRoot.transform.position = Vector3.zero;
            vertexRoot.transform.rotation = Quaternion.identity;

            verts = new List<Vector3>();
            points = new List<Transform>();

            //---Generating Points & Vertices
            var vertices = meshFilter.sharedMesh.vertices;
            for (int i = 0; i < vertices.Length; i++)
            {
                GameObject gm;

                Material new_Mat = new Material(Shader.Find("Unlit/Color"));
                new_Mat.color = Color.red;
                gm = MD_Octahedron.Generate();
                DestroyImmediate(gm.GetComponent<Collider>());
                gm.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
                gm.GetComponent<Renderer>().material = new_Mat;

                gm.transform.parent = vertexRoot.transform;

                gm.transform.position = vertices[i];
                verts.Add(vertices[i]);
                points.Add(gm.transform);

                gm.name = "P" + i.ToString();
            }

            //---Fixing Point Hierarchy
            foreach (Transform vertice in points)
            {
                foreach (Transform vertice2 in points)
                {
                    if (vertice.transform.position == vertice2.transform.position)
                    {
                        vertice.transform.parent = vertice2.transform;
                        vertice.gameObject.SetActive(false);
                    }
                }
            }

            //---Renaming Points
            int counter = 1;
            foreach (Transform vertice in vertexRoot.transform)
            {
                vertice.gameObject.SetActive(true);
                vertice.name = "P" + counter.ToString();
                counter++;
            }

            vertexRoot.transform.parent = transform;
            transform.position = lastPos;
            transform.rotation = lastRot;
        }
        /// <summary>
        /// Restore original mesh
        /// </summary>
        public void MeshFit_RestoreOriginal()
        {
            MeshFit_ClearPoints();

            meshFilter.sharedMesh.vertices = workingVertices;
            originalVertices = workingVertices;

            meshFilter.sharedMesh.RecalculateNormals();
            meshFilter.sharedMesh.RecalculateBounds();
        }
        /// <summary>
        /// Clear points on the mesh [if possible]
        /// </summary>
        public void MeshFit_ClearPoints()
        {
            verts = new List<Vector3>();
            points = new List<Transform>();
            ppMODIF_MeshFitter_SelectedVertexes = null;

            if (vertexRoot) DestroyImmediate(vertexRoot);
        }
        /// <summary>
        /// Reset mesh matrix transform [Set scale to 1 and keep the shape]
        /// </summary>
        public void MeshFit_BakeMesh()
        {
            MeshFit_ClearPoints();

            Vector3[] vertsNew = meshFilter.sharedMesh.vertices;
            Vector3 lastPos = transform.position;
            Quaternion lastRot = transform.rotation;
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            for (int i = 0; i < vertsNew.Length; i++)
                vertsNew[i] = transform.TransformPoint(meshFilter.sharedMesh.vertices[i]);
            transform.localScale = Vector3.one;
            meshFilter.sharedMesh.vertices = vertsNew;

            originalVertices = meshFilter.sharedMesh.vertices;
            workingVertices = meshFilter.sharedMesh.vertices;

            transform.position = lastPos;
            transform.rotation = lastRot;
        }

        /// <summary>
        /// Update mesh state
        /// </summary>
        public void MeshFit_UpdateMeshState()
        {
            if (verts.Count == 0 || points.Count == 0)
                return;

            if (ppMODIF_MeshFitterType == MeshFitterMode.FitWholeMesh)
            {
                for (int i = 0; i < verts.Count; i++)
                {
                    points[i].gameObject.layer = 2;
                    gameObject.layer = 2;

                    Ray ray = new Ray(points[i].transform.position + Vector3.up * ppMODIF_MeshFitterSurfaceDetection, Vector3.down);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                    {
                        if (hit.collider)
                            points[i].transform.position = new Vector3(hit.point.x, hit.point.y + ppMODIF_MeshFitterOffset, hit.point.z);
                        else if (!ppMODIF_MeshFitterContinuousEffect) points[i].transform.position = transform.TransformPoint(originalVertices[i]);
                    }
                    else if (!ppMODIF_MeshFitterContinuousEffect) points[i].transform.position = transform.TransformPoint(originalVertices[i]);

                    verts[i] = new Vector3(points[i].position.x - (meshFilter.transform.position.x - Vector3.zero.x), points[i].position.y - (meshFilter.transform.position.y - Vector3.zero.y), points[i].position.z - (meshFilter.transform.position.z - Vector3.zero.z));

                    points[i].gameObject.layer = 0;
                    gameObject.layer = 0;
                }
            }
            else if (ppMODIF_MeshFitterType == MeshFitterMode.FitSpecificVertices)
            {
                if (ppMODIF_MeshFitter_SelectedVertexes == null || ppMODIF_MeshFitter_SelectedVertexes.Length == 0)
                    return;

                for (int i = 0; i < verts.Count; i++)
                {
                    points[i].gameObject.layer = 2;
                    gameObject.layer = 2;

                    if (points[i].gameObject.activeInHierarchy)
                    {
                        Ray ray = new Ray(points[i].transform.position + Vector3.up * ppMODIF_MeshFitterSurfaceDetection, Vector3.down);
                        RaycastHit hit;
                        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                        {
                            if (hit.collider)
                                points[i].transform.position = new Vector3(hit.point.x, hit.point.y + ppMODIF_MeshFitterOffset, hit.point.z);
                            else if (!ppMODIF_MeshFitterContinuousEffect) points[i].transform.position = transform.TransformPoint(originalVertices[i]);
                        }
                        else if (!ppMODIF_MeshFitterContinuousEffect) points[i].transform.position = transform.TransformPoint(originalVertices[i]);
                    }

                    verts[i] = new Vector3(points[i].position.x - (meshFilter.transform.position.x - Vector3.zero.x), points[i].position.y - (meshFilter.transform.position.y - Vector3.zero.y), points[i].position.z - (meshFilter.transform.position.z - Vector3.zero.z));
                    gameObject.layer = 0;
                }
            }

            meshFilter.sharedMesh.vertices = verts.ToArray();
            if (ppAutoRecalculateNormals) meshFilter.sharedMesh.RecalculateNormals();
            if (ppAutoRecalculateBounds) meshFilter.sharedMesh.RecalculateBounds();
        }
    }
}

#if UNITY_EDITOR
namespace MD_PluginEditor
{
    [CustomEditor(typeof(MDM_MeshFit))]
    public class MDM_MeshFit_Editor : MD_EditorUtilities
    {
        private MDM_MeshFit m;

        private void OnEnable()
        {
            m = (MDM_MeshFit)target;
        }

        public override void OnInspectorGUI()
        {
            ps();

            pv();
            ph();
            if (pb("Generate Interactive Points"))
                m.MeshFit_GeneratePoints();
            if (pb("Refresh Mesh Transform"))
                m.MeshFit_BakeMesh();
            phe();

            if (m.points.Count == 0)
            {
                pve();
                ps(15);
                ppAddMeshColliderRefresher(m.gameObject);
                ppBackToMeshEditor(m);
                return;
            }

            ps();
            ph();
            if (pb("Show Points"))
                m.MeshFit_ShowHidePoints(true);
            if (pb("Hide Points"))
                m.MeshFit_ShowHidePoints(false);
            if (pb("Clear Points"))
                m.MeshFit_ClearPoints();
            phe();

            ps();

            pv();
            ps(5);
            ppDrawProperty("ppMODIF_MeshFitterType", "Type");
            ppDrawProperty("ppUpdateEveryFrame", "Update Every Frame");
            if (!m.ppUpdateEveryFrame)
            {
                if (pb("Update Mesh State"))
                    m.MeshFit_UpdateMeshState();
            }
            ppDrawProperty("ppAutoRecalculateNormals", "Auto Recalculate Normals");
            ppDrawProperty("ppAutoRecalculateBounds", "Auto Recalculate Bounds");
            ps();
            ppDrawProperty("ppMODIF_MeshFitterOffset", "Raycast Offset", "Vertex position offset after raycast");
            ppDrawProperty("ppMODIF_MeshFitterSurfaceDetection", "Raycast Distance", "Interactivity radius amount");
            ppDrawProperty("ppMODIF_MeshFitterContinuousEffect", "Continuous Effect", "If enabled, every vertex position won't jump to its original state & will continue in the last position");
            pve();
            if (m.ppMODIF_MeshFitterType == MDM_MeshFit.MeshFitterMode.FitSpecificVertices)
            {
                ppDrawProperty("ppMODIF_MeshFitter_SelectedVertexes", "Selected Vertices", "", true);
                if (pb("Open Vertices Assignator"))
                {
                    MD_VerticeSelectorTool mdvTool = new MD_VerticeSelectorTool();
                    mdvTool.minSize = new Vector2(400, 20);
                    mdvTool.maxSize = new Vector2(410, 20);
                    m.transform.parent = null;
                    mdvTool.Show();
                    mdvTool.sender = m;
                }

                if (m.ppMODIF_MeshFitter_SelectedVertexes != null && m.ppMODIF_MeshFitter_SelectedVertexes.Length > 0)
                    if (pb("Clear Selected Points"))
                    {
                        m.ppMODIF_MeshFitter_SelectedVertexes = null;
                        m.MeshFit_ShowHidePoints(true);
                    }
            }
            ps();
            pv();
            if (pb("Restore Mesh"))
                m.MeshFit_RestoreOriginal();
            pve();
            pve();
            ps(15);
            ppAddMeshColliderRefresher(m.gameObject);
            ppBackToMeshEditor(m);
            ps();
            if (target != null) serializedObject.Update();
        }
    }
}
#endif
