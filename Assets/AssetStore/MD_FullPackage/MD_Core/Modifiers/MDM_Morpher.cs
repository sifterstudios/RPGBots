using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
#if UNITY_EDITOR
using UnityEditor;
using MD_Plugin;
#endif

namespace MD_Plugin
{
    /// <summary>
    /// MDM(Mesh Deformation Modifier): Mesh Morpher
    /// Blend mesh between list of stored & captured vertices
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter))]
    [AddComponentMenu(MD_Debug.ORGANISATION + MD_Debug.PACKAGENAME + "Modifiers/Morpher")]
    public class MDM_Morpher : MonoBehaviour
    {
        public bool ppUpdateEveryFrame = true;
        public bool ppRecalculateNormals = true;
        public bool ppRecalculateBounds = true;

        public bool ppMultithreadingSupported = false;
        protected Thread Multithread;

        public Vector3[] originalVertices;
        public Vector3[] threadVertices;

        [SerializeField] public MeshFilter meshFilter;

        public bool ppInterpolation = false;
        public float ppInterpolationSpeed = 0.5f;

        [Range(0, 1)] public float ppBlendValue = 0;

        public Mesh[] ppTargetMorphMeshes = new Mesh[0];
        public int ppIndexOfTargetMesh = 0;

        public bool ppRestartVertState = true;

        [System.Serializable]
        public class registeredMorphs
        {
            public List<Vector3> vertices = new List<Vector3>();
            public List<int> indexes = new List<int>();
        }
        public List<registeredMorphs> ppRegisteredMorphs = new List<registeredMorphs>();

        public bool modeSwitch = false;

        private void Awake()
        {
            if (meshFilter != null) return;

            ppRecalculateBounds = MD_GlobalPreferences.autoRecalcBounds;
            ppRecalculateNormals = MD_GlobalPreferences.autoRecalcNormals;

            meshFilter = GetComponent<MeshFilter>();
            MD_MeshProEditor.MeshProEditor_Utilities.util_PrepareMeshDeformationModifier(this, meshFilter, false);

            if (meshFilter.sharedMesh.vertexCount > MD_GlobalPreferences.vertexLimit)
                ppMultithreadingSupported = true;
            originalVertices = meshFilter.sharedMesh.vertices;
        }

        private void Start()
        {
            if (ppMultithreadingSupported && Application.isPlaying)
            {
                threadVertices = meshFilter.mesh.vertices;
                Multithread = new Thread(ThreadWorker_UpdateMorpher);
                Multithread.Start();
            }
        }

        /// <summary>
        /// Change current target morph mesh index
        /// </summary>
        public void Morpher_ChangeMeshIndex(int entry)
        {
            if (!meshFilter)
                return;
            if (ppRestartVertState) meshFilter.sharedMesh.vertices = originalVertices;
            ppIndexOfTargetMesh = entry;
        }

        /// <summary>
        /// Set current blend value
        /// </summary>
        public void Morpher_SetBlendValue(Slider entry)
        {
            ppBlendValue = entry.value;
            if (!ppUpdateEveryFrame) Morpher_UpdateMorpher();
        }
        /// <summary>
        /// Set current blend value
        /// </summary>
        public void Morpher_SetBlendValue(float entry)
        {
            ppBlendValue = entry;
            if (!ppUpdateEveryFrame) Morpher_UpdateMorpher();
        }

        /// <summary>
        /// Refresh target meshes - target meshes must be registered
        /// </summary>
        public void Morpher_RefreshTargetMeshes()
        {
            if (ppRegisteredMorphs == null)
                ppRegisteredMorphs = new List<registeredMorphs>();
            else
                ppRegisteredMorphs.Clear();

            if (!meshFilter) return;

            foreach (Mesh m in ppTargetMorphMeshes)
            {
                if (originalVertices.Length != m.vertices.Length)
                {
                    MD_Debug.Debug(this, "Target mesh must have the same vertex count & mesh identity as its original reference", MD_Debug.DebugType.Error);
                    return;
                }

                registeredMorphs regMorph = new registeredMorphs();
                for (int i = 0; i < m.vertices.Length; i++)
                {
                    if (ppMultithreadingSupported)
                    {
                        regMorph.vertices.Add(m.vertices[i]);
                        regMorph.indexes.Add(i);
                    }
                    else if (!Equals(m.vertices[i], originalVertices[i]))
                    {
                        regMorph.vertices.Add(m.vertices[i]);
                        regMorph.indexes.Add(i);
                    }
                }
                ppRegisteredMorphs.Add(regMorph);
            }
        }

        private Vector3 LinearInterpolation(Vector3 A, Vector3 B, float dist)
        {
            Vector3 final = dist * (B - A) + A;
            return final;
        }

        private void Update()
        {
            if (!ppUpdateEveryFrame)
                return;

            Morpher_UpdateMorpher();
        }

        /// <summary>
        /// Update & refresh current morpher
        /// </summary>
        public void Morpher_UpdateMorpher()
        {
            if (ppMultithreadingSupported)
            {
                if (threadVertices.Length == 0) return;
                meshFilter.sharedMesh.vertices = threadVertices;
                if (ppRecalculateNormals) meshFilter.sharedMesh.RecalculateNormals();
                if (ppRecalculateBounds) meshFilter.sharedMesh.RecalculateBounds();
                return;
            }

            if (ppRegisteredMorphs.Count > 0 && (ppIndexOfTargetMesh >= 0 && ppIndexOfTargetMesh < ppRegisteredMorphs.Count))
            {
                if (ppRegisteredMorphs[ppIndexOfTargetMesh] == null)
                    return;
                Vector3[] Vertices = meshFilter.sharedMesh.vertices;
                for (int i = 0; i < ppRegisteredMorphs[ppIndexOfTargetMesh].vertices.Count; i++)
                {
                    if (!ppInterpolation)
                        Vertices[ppRegisteredMorphs[ppIndexOfTargetMesh].indexes[i]] = LinearInterpolation(originalVertices[ppRegisteredMorphs[ppIndexOfTargetMesh].indexes[i]], ppRegisteredMorphs[ppIndexOfTargetMesh].vertices[i], ppBlendValue);
                    else
                        Vertices[ppRegisteredMorphs[ppIndexOfTargetMesh].indexes[i]] = Vector3.Lerp(Vertices[ppRegisteredMorphs[ppIndexOfTargetMesh].indexes[i]], LinearInterpolation(originalVertices[ppRegisteredMorphs[ppIndexOfTargetMesh].indexes[i]], ppRegisteredMorphs[ppIndexOfTargetMesh].vertices[i], ppBlendValue), ppInterpolationSpeed * Time.deltaTime);
                }
                meshFilter.sharedMesh.vertices = Vertices;
                if (ppRecalculateNormals) meshFilter.sharedMesh.RecalculateNormals();
                if (ppRecalculateBounds) meshFilter.sharedMesh.RecalculateBounds();
            }
        }

        private void ThreadWorker_UpdateMorpher()
        {
            while (true)
            {
                if (ppRegisteredMorphs.Count > 0 && (ppIndexOfTargetMesh >= 0 && ppIndexOfTargetMesh < ppRegisteredMorphs.Count))
                {
                    if (ppRegisteredMorphs[ppIndexOfTargetMesh] == null)
                        return;
                    for (int i = 0; i < ppRegisteredMorphs[ppIndexOfTargetMesh].vertices.Count; i++)
                    {
                        if (!ppInterpolation)
                            threadVertices[ppRegisteredMorphs[ppIndexOfTargetMesh].indexes[i]] = LinearInterpolation(originalVertices[ppRegisteredMorphs[ppIndexOfTargetMesh].indexes[i]], ppRegisteredMorphs[ppIndexOfTargetMesh].vertices[i], ppBlendValue);
                        else
                            threadVertices[ppRegisteredMorphs[ppIndexOfTargetMesh].indexes[i]] = Vector3.Lerp(threadVertices[ppRegisteredMorphs[ppIndexOfTargetMesh].indexes[i]], LinearInterpolation(originalVertices[ppRegisteredMorphs[ppIndexOfTargetMesh].indexes[i]], ppRegisteredMorphs[ppIndexOfTargetMesh].vertices[i], ppBlendValue), ppInterpolationSpeed);
                    }
                }

                Thread.Sleep(1);
            }
        }

        private void OnApplicationQuit()
        {
            if (Multithread != null && Multithread.IsAlive)
                Multithread.Abort();
        }
        private void OnDestroy()
        {
            if (Multithread != null && Multithread.IsAlive)
                Multithread.Abort();
        }
    }
}

#if UNITY_EDITOR
namespace MD_PluginEditor
{
    [CustomEditor(typeof(MDM_Morpher))]
    [CanEditMultipleObjects]
    public class MDM_Morpher_Editor : MD_EditorUtilities
    {
        private MDM_Morpher m;

        private void OnEnable()
        {
            m = (MDM_Morpher)target;
        }

        public override void OnInspectorGUI()
        {
            ps();
            pv();
            ppDrawProperty("ppUpdateEveryFrame", "Update Every Frame");
            ppDrawProperty("ppRecalculateNormals", "Recalculate Normals");
            ppDrawProperty("ppRecalculateBounds", "Recalculate Bounds");
            pve();
            pv();
            pv();
            ppDrawProperty("ppMultithreadingSupported", "Multithreading Supported", "If enabled, the morph system will be ready for complex meshes");
            if (m.ppMultithreadingSupported)
                phb("If multithreading is enabled, morpher will run at runtime only");
            pve();
            ppDrawProperty("ppInterpolation", "Enable Interpolation", "Enable smooth motion of vertices");
            if (m.ppInterpolation)
                ppDrawProperty("ppInterpolationSpeed", "Interpolation Speed");
            ps(5);
            ppDrawProperty("ppBlendValue", "Blend Value", "Blend value between original mesh & target mesh");
            pv();
            ppDrawProperty("ppTargetMorphMeshes", "Target Meshes", "Registered target meshes array", true);
            pve();
            pv();
            ppDrawProperty("ppIndexOfTargetMesh", "Selected Index");
            pve();
            ppDrawProperty("ppRestartVertState", "Restart Vertex State", "Restart mesh state after changing index");
            ps(5);
            pv();
            if (pb("Register Target Morphs"))
            {
                m.Morpher_RefreshTargetMeshes();
                m.modeSwitch = m.ppMultithreadingSupported;
            }
            if (m.modeSwitch != m.ppMultithreadingSupported) phb("^ Please register target meshes ^");
            pve();
            pve();
            ps(15);

            ppBackToMeshEditor(m);
            ps();
            if (target != null) serializedObject.Update();
        }
    }
}
#endif
