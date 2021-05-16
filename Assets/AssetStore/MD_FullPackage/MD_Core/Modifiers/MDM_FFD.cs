using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using MD_Plugin;
#endif

namespace MD_Plugin
{
    /// <summary>
    /// MDM(Mesh Deformation Modifier): FFD (FreeFormDeformer)
    /// Deform mesh by specific weight values that correspond to specific weight points
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter))]
    [AddComponentMenu(MD_Debug.ORGANISATION + MD_Debug.PACKAGENAME + "Modifiers/FFD")]
    public class MDM_FFD : MonoBehaviour
    {
        public bool ppUpdateEveryFrame = true;

        public enum FFDType {ffd2x2x2, ffd3x3x3, ffd4x4x4, CustomSigned};
        public FFDType ppFFDType = FFDType.ffd2x2x2;
        public enum FFDShape { Octahedron, Sphere, Cube, Custom};
        public FFDShape ppFFDShape = FFDShape.Sphere;
        public GameObject ppCustomShape;

        public float ppFFDNodeSize = 0.25f;
        [Range(0.0f, 1.0f)] public float ppFFDOffset = 0.2f;
        [Range(2,12)] public int ppFFDCustomCount = 2;

        [Range(0,1)] public float ppWeight = 0.5f;
        public float ppThreshold = 0.15f;
        public float ppDensity = 1.35f;
        public float ppBias = 1.0f;
        public float ppMultiplier = 1.0f;

        public float ppDistanceLimitation = Mathf.Infinity;

        [SerializeField] public List<Transform> ppTargetNodes = new List<Transform>();
        [SerializeField] protected List<Vector3> ppRegisteredPoints = new List<Vector3>();
        [SerializeField] protected Vector3[] ppOriginalVertices;
        public Transform ppNodeRoot;

        [SerializeField] public MeshFilter meshFilter;
        public bool ppAutoRecalculateNormals = true;
        public bool ppAutoRecalculateBounds = true;

        private void Awake()
        {
            if (meshFilter != null) return;

            ppAutoRecalculateBounds = MD_GlobalPreferences.autoRecalcBounds;
            ppAutoRecalculateNormals = MD_GlobalPreferences.autoRecalcNormals;
            meshFilter = GetComponent<MeshFilter>();
            MD_MeshProEditor.MeshProEditor_Utilities.util_PrepareMeshDeformationModifier(this, meshFilter);
        }

        private void OnDrawGizmos()
        {
            if (ppTargetNodes.Count == 0) return;
            if (ppRegisteredPoints.Count == 0) return;
            if (ppTargetNodes.Count != ppRegisteredPoints.Count) return;

            Gizmos.color = Color.green;
            for (int i = 0; i < ppRegisteredPoints.Count; i++) Gizmos.DrawLine(ppRegisteredPoints[i], ppTargetNodes[i].position);
        }

        private void Update()
        {
            if (ppUpdateEveryFrame) FFD_UpdateMesh();
        }

        /// <summary>
        /// Update current mesh by FFD modifier
        /// </summary>
        public void FFD_UpdateMesh()
        {
            if (ppTargetNodes.Count == 0) return;
            if (ppRegisteredPoints.Count == 0) return;
            if (ppTargetNodes.Count != ppRegisteredPoints.Count) return;

            ppThreshold = Mathf.Max(ppThreshold, 0.1f);

            Vector3[] vvs = new Vector3[ppOriginalVertices.Length];
            System.Array.Copy(ppOriginalVertices, vvs, ppOriginalVertices.Length);
            for (int i = 0; i < ppOriginalVertices.Length; i++)
            {
                Vector3 curVert = transform.TransformPoint(ppOriginalVertices[i]);
                Vector3 originalVert = curVert;
                for (int x = 0; x < ppRegisteredPoints.Count; x++)
                {
                    Transform node = ppTargetNodes[x];
                    if (Vector3.Distance(ppRegisteredPoints[x], originalVert) > ppDistanceLimitation) continue;
                    curVert += (node.position - originalVert).normalized *
                        ((Vector3.Distance(node.position, originalVert)* ppMultiplier) *
                        (1.0f / Mathf.Pow(Vector3.Distance(ppRegisteredPoints[x], originalVert) * ppThreshold, ppDensity)) * ppBias) *
                        (Vector3.Distance(ppRegisteredPoints[x], node.position) * 0.01f);
                }
                vvs[i] = transform.InverseTransformPoint(MD_MeshMathUtilities.CustomLerp(originalVert, curVert, ppWeight));
            }

            meshFilter.sharedMesh.vertices = vvs;
            if (ppAutoRecalculateBounds) meshFilter.sharedMesh.RecalculateBounds();
            if (ppAutoRecalculateNormals) meshFilter.sharedMesh.RecalculateNormals();
        }

        /// <summary>
        /// Register current weights and 'bake' mesh
        /// </summary>
        public void FFD_RegisterWeights()
        {
            transform.rotation = Quaternion.identity;

            FFD_ClearFFDGrid();

            int ffd = ppFFDCustomCount - 1;

            Vector3 maxBounds = transform.position + Vector3.Scale(transform.localScale, meshFilter.sharedMesh.bounds.max) + (Vector3.one * ppFFDOffset);
            Vector3 minBounds = transform.position + Vector3.Scale(transform.localScale, meshFilter.sharedMesh.bounds.min) - (Vector3.one * ppFFDOffset);

            float Xstep = (maxBounds.x - minBounds.x) / ffd;
            float Ystep = (maxBounds.y - minBounds.y) / ffd;
            float Zstep = (maxBounds.z - minBounds.z) / ffd;

            Vector3 cp;
            GameObject gm;
            ppNodeRoot = new GameObject("FFDRoot_"+ppFFDType.ToString()).transform;
            ppNodeRoot.position = transform.position;
            for (int x = 0; x < ffd + 1; x++)
            {
                cp = minBounds;
                cp.x += Xstep * x;
                for (int y = 0; y < ffd + 1; y++)
                {
                    cp.y = minBounds.y + (Ystep * y);
                    for (int z = 0; z < ffd + 1; z++)
                    {
                        cp.z = minBounds.z + (Zstep * z);
                        gm =    ppFFDShape == FFDShape.Sphere ? GameObject.CreatePrimitive(PrimitiveType.Sphere) :
                                ppFFDShape == FFDShape.Cube ? GameObject.CreatePrimitive(PrimitiveType.Cube) : 
                                ppFFDShape == FFDShape.Custom ? (ppCustomShape == null ? MD_Octahedron.Generate() : Instantiate(ppCustomShape)) : 
                                MD_Octahedron.Generate();

                        gm.transform.localScale *= ppFFDNodeSize;
                        gm.transform.position = cp;
                        gm.transform.parent = ppNodeRoot;
                        gm.name = "FFDPoint" + ppNodeRoot.childCount.ToString();
                        ppRegisteredPoints.Add(gm.transform.position);
                        ppTargetNodes.Add(gm.transform);
                    }
                }
            }

            ppOriginalVertices = meshFilter.sharedMesh.vertices;
        }

        /// <summary>
        /// Refresh selected FFD type & its grid
        /// </summary>
        public void FFD_RefreshFFDGrid()
        {
            float ww = ppWeight;
            ppWeight = 0.0f;
            FFD_UpdateMesh();
            switch(ppFFDType)
            {
                case FFDType.ffd2x2x2:
                    ppFFDCustomCount = 2;
                    break;
                case FFDType.ffd3x3x3:
                    ppFFDCustomCount = 3;
                    break;
                case FFDType.ffd4x4x4:
                    ppFFDCustomCount = 4;
                    break;
            }
            FFD_RegisterWeights();
            ppWeight = ww;
            FFD_UpdateMesh();
        }

        /// <summary>
        /// Clear FFD grid (if possible)
        /// </summary>
        public void FFD_ClearFFDGrid()
        {
            foreach (Transform tn in ppTargetNodes) if (tn) DestroyImmediate(tn.gameObject);
            ppTargetNodes.Clear();
            ppRegisteredPoints.Clear();
            if (ppNodeRoot) DestroyImmediate(ppNodeRoot.gameObject);
        }
    }
}

#if UNITY_EDITOR
namespace MD_PluginEditor
{
    [CustomEditor(typeof(MDM_FFD))]
    public class MDM_FFDEditor : MD_EditorUtilities
    {
        private MDM_FFD targ;
        private void OnEnable()
        {
            targ = (MDM_FFD)target;
        }

        public override void OnInspectorGUI()
        {
            ps();
            pv();
            ppDrawProperty("ppUpdateEveryFrame", "Update Every Frame");
            if(!targ.ppUpdateEveryFrame)
            {
                if (pb("Update FFD Mesh"))
                    targ.FFD_UpdateMesh();
            }
            ps(3);
            ppDrawProperty("ppAutoRecalculateNormals", "Recalculate Normals");
            ppDrawProperty("ppAutoRecalculateBounds", "Recalculate Bounds");
            ps();
            pv();
            pv();
            ppDrawProperty("ppFFDType", "FFD Type");
            if (targ.ppFFDType == MDM_FFD.FFDType.CustomSigned)
                ppDrawProperty("ppFFDCustomCount", "Custom Signed Count");
            ppDrawProperty("ppFFDShape", "FFD Point Shape");
            if (targ.ppFFDShape == MDM_FFD.FFDShape.Custom)
                ppDrawProperty("ppCustomShape", "Custom Shape");
            pve();
            ph();
            if (pb("Refresh FFD Grid"))
                targ.FFD_RefreshFFDGrid();
            if (pb("Clear FFD Grid"))
                targ.FFD_ClearFFDGrid();
            phe();
            ps(5);
            ppDrawProperty("ppFFDNodeSize", "Node Size");
            ppDrawProperty("ppFFDOffset", "FFD Grid Offset");
            pve();
            ps();
            ppDrawProperty("ppWeight", "Weight");
            ppDrawProperty("ppThreshold", "Threshold");
            ppDrawProperty("ppDensity", "Density");
            ppDrawProperty("ppBias", "Bias");
            ppDrawProperty("ppMultiplier", "Overall Multiplier");
            ps(5);
            ppDrawProperty("ppDistanceLimitation", "Distance Limitation");
            pve();
            if (pb("Register Weights Manually"))
                targ.FFD_RegisterWeights();
           
            ps(15);
            ppAddMeshColliderRefresher(targ.gameObject);
            ppBackToMeshEditor(targ);
            ps();

            if (targ != null) serializedObject.Update();
        }
    }
}
#endif