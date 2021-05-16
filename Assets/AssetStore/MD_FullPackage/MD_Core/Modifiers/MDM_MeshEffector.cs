using System.Collections.Generic;
using UnityEngine;
using System.Threading;
#if UNITY_EDITOR
using UnityEditor;
using MD_Plugin;
#endif

namespace MD_Plugin
{
    /// <summary>
    /// MDM(Mesh Deformation Modifier): Mesh Effector
    /// Deform mesh by specific weight values & effector radiuses
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter))]
    [AddComponentMenu(MD_Debug.ORGANISATION + MD_Debug.PACKAGENAME + "Modifiers/Mesh Effector")]
    public class MDM_MeshEffector : MonoBehaviour
    {
        public bool ppUpdateEveryFrame = true;

        [SerializeField] protected Vector3[] ppGeneratedPoints;
        [SerializeField] protected List<Vector3> ppGeneratedPointsOrigins = new List<Vector3>();

        [SerializeField] public MeshFilter meshFilter;
        public bool ppAutoRecalculateNormals = true;
        public bool ppAutoRecalculateBounds = true;

        public enum EffectorType { OnePointed, TwoPointed, ThreePointed, FourPointed};
        public EffectorType effectorType = EffectorType.OnePointed;

        public Transform ppWeightNode0;
        public Transform ppWeightNode1;
        public Transform ppWeightNode2;
        public Transform ppWeightNode3;

        [SerializeField] private Vector3 node0;
        [SerializeField] private Vector3 node1;
        [SerializeField] private Vector3 node2;
        [SerializeField] private Vector3 node3;

        [SerializeField] private Vector3 node0StartPos;
        [SerializeField] private Vector3 node1StartPos;
        [SerializeField] private Vector3 node2StartPos;
        [SerializeField] private Vector3 node3StartPos;

        [Range(0.0f,1.0f)]
        public float ppWeight = 0.5f;
        public float ppWeightMultiplier = 1.0f;
        public float ppWeightDensity = 3.0f;
        [Range(0f, 1f)]
        public float ppWeightEffectorA = 0.5f;
        [Range(0f, 1f)]
        public float ppWeightEffectorB = 0.5f;
        [Range(0f, 1f)]
        public float ppWeightEffectorC = 0.5f;

        public bool ppClampEffector = false;
        public float ppClampValue = 0.5f;

        public bool ppMultithreadingSupported = false;
        private Thread Multithread;
        private ManualResetEvent Multithread_ManualEvent = new ManualResetEvent(true);
        private bool threadDone = false;
        [Range(8, 25)]
        public int ppMultithreadingProcessDelay = 16;

        private void Awake()
        {
            if (meshFilter != null) return;

            ppAutoRecalculateBounds = MD_GlobalPreferences.autoRecalcBounds;
            ppAutoRecalculateNormals = MD_GlobalPreferences.autoRecalcNormals;

            meshFilter = GetComponent<MeshFilter>();
            MD_MeshProEditor.MeshProEditor_Utilities.util_PrepareMeshDeformationModifier(this, meshFilter, false);
            if (meshFilter.sharedMesh.vertexCount > MD_GlobalPreferences.vertexLimit)
                ppMultithreadingSupported = true;
        }

        private void Start()
        {
            if (ppMultithreadingSupported && Application.isPlaying)
            {
                Multithread = new Thread(ThreadWorker);
                Multithread_ManualEvent = new ManualResetEvent(true);
                Multithread.Start();
            }
        }

        private void OnDrawGizmos()
        {
            if (!ppUpdateEveryFrame)
                return;

            Gizmos.color = Color.green;

            switch(effectorType)
            {
                case EffectorType.OnePointed:
                    if (ppWeightNode0)
                    {
                        Gizmos.DrawWireSphere(node0StartPos, ppWeightNode0.localScale.magnitude / 2);
                        Gizmos.DrawLine(node0StartPos, ppWeightNode0.position);
                    }
                    break;

                case EffectorType.TwoPointed:
                    if (ppWeightNode0)
                    {
                        Gizmos.DrawWireSphere(node0StartPos, ppWeightNode0.localScale.magnitude / 2);
                        Gizmos.DrawLine(node0StartPos, ppWeightNode0.position);
                    }
                    if (ppWeightNode1)
                    {
                        Gizmos.DrawWireSphere(node1StartPos, ppWeightNode1.localScale.magnitude / 2);
                        Gizmos.DrawLine(node1StartPos, ppWeightNode1.position);
                    }
                    break;

                case EffectorType.ThreePointed:
                    if (ppWeightNode0)
                    {
                        Gizmos.DrawWireSphere(node0StartPos, ppWeightNode0.localScale.magnitude / 2);
                        Gizmos.DrawLine(node0StartPos, ppWeightNode0.position);
                    }
                    if (ppWeightNode1)
                    {
                        Gizmos.DrawWireSphere(node1StartPos, ppWeightNode1.localScale.magnitude / 2);
                        Gizmos.DrawLine(node1StartPos, ppWeightNode1.position);
                    }
                    if (ppWeightNode2)
                    {
                        Gizmos.DrawWireSphere(node2StartPos, ppWeightNode2.localScale.magnitude / 2);
                        Gizmos.DrawLine(node2StartPos, ppWeightNode2.position);
                    }
                    break;

                case EffectorType.FourPointed:
                    if (ppWeightNode0)
                    {
                        Gizmos.DrawWireSphere(node0StartPos, ppWeightNode0.localScale.magnitude / 2);
                        Gizmos.DrawLine(node0StartPos, ppWeightNode0.position);
                    }
                    if (ppWeightNode1)
                    {
                        Gizmos.DrawWireSphere(node1StartPos, ppWeightNode1.localScale.magnitude / 2);
                        Gizmos.DrawLine(node1StartPos, ppWeightNode1.position);
                    }
                    if (ppWeightNode2)
                    {
                        Gizmos.DrawWireSphere(node2StartPos, ppWeightNode2.localScale.magnitude / 2);
                        Gizmos.DrawLine(node2StartPos, ppWeightNode2.position);
                    }
                    if (ppWeightNode3)
                    {
                        Gizmos.DrawWireSphere(node3StartPos, ppWeightNode3.localScale.magnitude / 2);
                        Gizmos.DrawLine(node3StartPos, ppWeightNode3.position);
                    }
                    break;
            }
        }

        private void Update()
        {
            if (!ppUpdateEveryFrame)
                return;
            if (!meshFilter)
                return;
            if (ppGeneratedPoints == null)
                return;
            if (ppGeneratedPoints.Length != meshFilter.sharedMesh.vertices.Length)
                return;
            if (ppGeneratedPointsOrigins == null)
                return;
            if (ppGeneratedPointsOrigins.Count != meshFilter.sharedMesh.vertices.Length)
                return;

            if (ppMultithreadingSupported)
            {
                if (ppWeightNode0)
                    node0 = ppWeightNode0.position;
                if (ppWeightNode1)
                    node1 = ppWeightNode1.position;
                if (ppWeightNode2)
                    node2 = ppWeightNode2.position;
                if (ppWeightNode3)
                    node3 = ppWeightNode3.position;
                if (ppGeneratedPoints.Length == 0)
                    return;
                if(threadDone)
                    Effector_UpdateMesh();
                Multithread_ManualEvent.Set();
                return;
            }

            switch(effectorType)
            {
                case EffectorType.OnePointed:
                    if (ppWeightNode0)
                    {
                        for (int i = 0; i < ppGeneratedPoints.Length; i++)
                            ppGeneratedPoints[i] = VecInterpolation(ppGeneratedPointsOrigins[i], ppWeightNode0.position, (1.0f / Mathf.Pow(Vector3.Distance(ppGeneratedPointsOrigins[i], ppWeightNode0.position), ppWeightDensity)) * ppWeight * ppWeightMultiplier * (node0StartPos - ppWeightNode0.position).normalized.magnitude);
                    }
                    break;

                case EffectorType.TwoPointed:
                    if (ppWeightNode0 && ppWeightNode1)
                    {
                        for (int i = 0; i < ppGeneratedPoints.Length; i++)
                            ppGeneratedPoints[i] = InterpolationOfTwoPointed(ppGeneratedPointsOrigins[i], ppWeightNode0.position, ppWeightNode1.position);
                    }
                    break;

                case EffectorType.ThreePointed:
                    if (ppWeightNode0 && ppWeightNode1 && ppWeightNode2)
                    {
                        for (int i = 0; i < ppGeneratedPoints.Length; i++)
                            ppGeneratedPoints[i] = InterpolationOfThreePointed(ppGeneratedPointsOrigins[i], ppWeightNode0.position, ppWeightNode1.position, ppWeightNode2.position);
                    }
                    break;

                case EffectorType.FourPointed:
                    if (ppWeightNode0 && ppWeightNode1 && ppWeightNode2 && ppWeightNode3)
                    {
                        for (int i = 0; i < ppGeneratedPoints.Length; i++)
                            ppGeneratedPoints[i] = InterpolationOfFourPointed(ppGeneratedPointsOrigins[i], ppWeightNode0.position, ppWeightNode1.position, ppWeightNode2.position, ppWeightNode3.position);
                    }
                    break;
            }

            Effector_UpdateMesh();
        }

        /// <summary>
        /// Update current mesh state (in case if Update Every Frame is disabled)
        /// </summary>
        public void Effector_UpdateMesh()
        {
            if (!meshFilter)
                return;
            if (ppGeneratedPoints == null)
                return;
            if (ppGeneratedPoints.Length != meshFilter.sharedMesh.vertices.Length)
                return;

            Vector3[] verts = meshFilter.sharedMesh.vertices;
            for (int i = 0; i < ppGeneratedPoints.Length; i++)
                verts[i] = transform.InverseTransformPoint(ppGeneratedPoints[i]);

            meshFilter.sharedMesh.vertices = verts;
            if (ppAutoRecalculateBounds) meshFilter.sharedMesh.RecalculateBounds();
            if (ppAutoRecalculateNormals) meshFilter.sharedMesh.RecalculateNormals();
        }

        /// <summary>
        /// Apply & register effector weights
        /// </summary>
        public void Effector_ApplyWeights()
        {
            ppGeneratedPoints = meshFilter.sharedMesh.vertices;
            if(ppWeightNode0)
                node0StartPos = ppWeightNode0.position;
            if (ppWeightNode1)
                node1StartPos = ppWeightNode1.position;
            if (ppWeightNode2)
                node2StartPos = ppWeightNode2.position;
            if (ppWeightNode3)
                node3StartPos = ppWeightNode3.position;

            ppGeneratedPointsOrigins.Clear();
            for (int i = 0; i < ppGeneratedPoints.Length; i++)
                ppGeneratedPointsOrigins.Add(transform.TransformPoint(ppGeneratedPoints[i]));
        }


        private Vector3 InterpolationOfFourPointed(Vector3 p, Vector3 n0, Vector3 n1, Vector3 n2, Vector3 n3)
        {
            return VecInterpolation(
                VecInterpolation(
                    VecInterpolation(
                        VecInterpolation(p, n0, (1.0f / Mathf.Pow(Vector3.Distance(p, n0), ppWeightDensity)) * ppWeight * ppWeightMultiplier * (node0StartPos - n0).magnitude),
                        VecInterpolation(p, n1, (1.0f / Mathf.Pow(Vector3.Distance(p, n1), ppWeightDensity)) * ppWeight * ppWeightMultiplier * (node1StartPos - n1).magnitude),
                        ppWeightEffectorA),
                    VecInterpolation(
                        VecInterpolation(p, n0, (1.0f / Mathf.Pow(Vector3.Distance(p, n0), ppWeightDensity)) * ppWeight * ppWeightMultiplier * (node0StartPos - n0).magnitude),
                        VecInterpolation(p, n2, (1.0f / Mathf.Pow(Vector3.Distance(p, n2), ppWeightDensity)) * ppWeight * ppWeightMultiplier * (node2StartPos - n2).magnitude),
                        ppWeightEffectorA), 
                    ppWeightEffectorB),
                 VecInterpolation(
                    VecInterpolation(
                        VecInterpolation(p, n0, (1.0f / Mathf.Pow(Vector3.Distance(p, n0), ppWeightDensity)) * ppWeight * ppWeightMultiplier * (node0StartPos - n0).magnitude),
                        VecInterpolation(p, n3, (1.0f / Mathf.Pow(Vector3.Distance(p, n3), ppWeightDensity)) * ppWeight * ppWeightMultiplier * (node3StartPos - n3).magnitude),
                        ppWeightEffectorA),
                    VecInterpolation(
                        VecInterpolation(p, n0, (1.0f / Mathf.Pow(Vector3.Distance(p, n0), ppWeightDensity)) * ppWeight * ppWeightMultiplier * (node0StartPos - n0).magnitude),
                        VecInterpolation(p, n3, (1.0f / Mathf.Pow(Vector3.Distance(p, n3), ppWeightDensity)) * ppWeight * ppWeightMultiplier * (node3StartPos - n3).magnitude),
                        ppWeightEffectorA),
                    ppWeightEffectorB), 
                ppWeightEffectorC);
        }

        private Vector3 InterpolationOfThreePointed(Vector3 p, Vector3 n0, Vector3 n1, Vector3 n2)
        {
            return VecInterpolation(VecInterpolation(
                VecInterpolation(p, n0, (1.0f / Mathf.Pow(Vector3.Distance(p, n0), ppWeightDensity)) * ppWeight * ppWeightMultiplier * (node0StartPos - n0).magnitude),
                VecInterpolation(p, n1, (1.0f / Mathf.Pow(Vector3.Distance(p, n1), ppWeightDensity)) * ppWeight * ppWeightMultiplier * (node1StartPos - n1).magnitude), 
                ppWeightEffectorA), 
                VecInterpolation(
                VecInterpolation(p, n0, (1.0f / Mathf.Pow(Vector3.Distance(p, n0), ppWeightDensity)) * ppWeight * ppWeightMultiplier * (node0StartPos - n0).magnitude),
                VecInterpolation(p, n2, (1.0f / Mathf.Pow(Vector3.Distance(p, n2), ppWeightDensity)) * ppWeight * ppWeightMultiplier * (node2StartPos - n2).magnitude),
                ppWeightEffectorA), ppWeightEffectorB);
        }

        private Vector3 InterpolationOfTwoPointed(Vector3 p, Vector3 n0, Vector3 n1)
        {
            return VecInterpolation(
                VecInterpolation(p, n0, (1.0f / Mathf.Pow(Vector3.Distance(p, n0), ppWeightDensity)) * ppWeight * ppWeightMultiplier * (node0StartPos - n0).magnitude),
                VecInterpolation(p, n1, (1.0f / Mathf.Pow(Vector3.Distance(p, n1), ppWeightDensity)) * ppWeight * ppWeightMultiplier * (node1StartPos - n1).magnitude),
                ppWeightEffectorA);
        }

        private Vector3 VecInterpolation(Vector3 A, Vector3 B, float t)
        {
            Vector3 final = t * (B - A) + A;
            return ppClampEffector ? Vector3.ClampMagnitude(final, ppClampValue) : final;
        }


        private void ThreadWorker()
        {
            while (true)
            {
                threadDone = false;
                Multithread_ManualEvent.WaitOne();
                if (ppGeneratedPointsOrigins == null)
                    continue;
                if (ppGeneratedPointsOrigins.Count == 0)
                    continue;

                switch (effectorType)
                {
                    case EffectorType.OnePointed:
                        for (int i = 0; i < ppGeneratedPoints.Length; i++)
                            ppGeneratedPoints[i] = VecInterpolation(ppGeneratedPointsOrigins[i], node0, (1.0f / Mathf.Pow(Vector3.Distance(ppGeneratedPointsOrigins[i], node0), ppWeightDensity)) * ppWeight * ppWeightMultiplier * (node0StartPos - node0).magnitude);
                        break;

                    case EffectorType.TwoPointed:
                        for (int i = 0; i < ppGeneratedPoints.Length; i++)
                            ppGeneratedPoints[i] = InterpolationOfTwoPointed(ppGeneratedPointsOrigins[i], node0, node1);
                        break;

                    case EffectorType.ThreePointed:
                        for (int i = 0; i < ppGeneratedPoints.Length; i++)
                            ppGeneratedPoints[i] = InterpolationOfThreePointed(ppGeneratedPointsOrigins[i], node0, node1, node2);
                        break;

                    case EffectorType.FourPointed:
                        for (int i = 0; i < ppGeneratedPoints.Length; i++)
                            ppGeneratedPoints[i] = InterpolationOfFourPointed(ppGeneratedPointsOrigins[i], node0, node1, node2, node3);
                        break;
                }
                threadDone = true;
                Thread.Sleep(ppMultithreadingProcessDelay);
                Multithread_ManualEvent.Reset();
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
        private void OnDisable()
        {
            if (Multithread != null && Multithread.IsAlive)
                Multithread.Abort();
        }

        internal void Effector_ThreadOption(bool activateThread)
        {
            if (!ppMultithreadingSupported) return;
            if (Application.isPlaying)
            {
                MD_Debug.Debug(this, "It's prohibited to control Mesh Effector Thread during the playtime - the thread is handled automatically inside the script! No touchy!", MD_Debug.DebugType.Error);
                return;
            }
            if(activateThread == false)
            {
                if (Multithread != null && Multithread.IsAlive)
                    Multithread.Abort();
                return;
            }
            if (Multithread != null && Multithread.IsAlive)
                return;

            Multithread = new Thread(ThreadWorker);
            Multithread_ManualEvent = new ManualResetEvent(true);
            Multithread.Start();
        }
    }
}

#if UNITY_EDITOR
namespace MD_PluginEditor
{
    [CustomEditor(typeof(MDM_MeshEffector))]
    public class MDM_MeshEffectorEditor : MD_EditorUtilities
    {
        private MDM_MeshEffector targ;
        private void OnEnable()
        {
            targ = (MDM_MeshEffector)target;
        }

        public override void OnInspectorGUI()
        {
            ps();
            ppDrawProperty("ppUpdateEveryFrame", "Update Every Frame", "If enabled, the script will be refreshed every frame (even in Update)");
            ps(10);

            pv();
            ppDrawProperty("effectorType", "Effector Type");
            ppDrawProperty("ppWeightNode0", "Weight Node 0");
            if (targ.effectorType == MDM_MeshEffector.EffectorType.TwoPointed)
                ppDrawProperty("ppWeightNode1", "Weight Node 1");
            else if(targ.effectorType == MDM_MeshEffector.EffectorType.ThreePointed)
            {
                ppDrawProperty("ppWeightNode1", "Weight Node 1");
                ppDrawProperty("ppWeightNode2", "Weight Node 2");
            }
            else if (targ.effectorType == MDM_MeshEffector.EffectorType.FourPointed)
            {
                ppDrawProperty("ppWeightNode1", "Weight Node 1");
                ppDrawProperty("ppWeightNode2", "Weight Node 2");
                ppDrawProperty("ppWeightNode3", "Weight Node 3");
            }
            ppDrawProperty("ppAutoRecalculateBounds", "Auto Recalculate Bounds");
            ppDrawProperty("ppAutoRecalculateNormals", "Auto Recalculate Normals");
            pve();
            ps(10);

            pl("Effector Parameters");
            pv();
            ppDrawProperty("ppWeight", "Weight","Total weight");
            ppDrawProperty("ppWeightMultiplier", "Weight Multiplier", "Additional weight multiplier");
            ppDrawProperty("ppWeightDensity", "Weight Density");
            if(targ.effectorType == MDM_MeshEffector.EffectorType.TwoPointed)
                ppDrawProperty("ppWeightEffectorA", "Weight Effector", "Effector value between two weight nodes");
            else if(targ.effectorType == MDM_MeshEffector.EffectorType.ThreePointed)
            {
                ppDrawProperty("ppWeightEffectorA", "Weight Effector A", "Effector value between two weight nodes");
                ppDrawProperty("ppWeightEffectorB", "Weight Effector B", "Effector value between three weight nodes");
            }
            else if (targ.effectorType == MDM_MeshEffector.EffectorType.FourPointed)
            {
                ppDrawProperty("ppWeightEffectorA", "Weight Effector A", "Effector value between two weight nodes");
                ppDrawProperty("ppWeightEffectorB", "Weight Effector B", "Effector value between three weight nodes");
                ppDrawProperty("ppWeightEffectorC", "Weight Effector C", "Effector value between Four weight nodes");
            }
            ps();
            ppDrawProperty("ppClampEffector", "Clamp Effector", "If enabled, the affected vertices will be limited by the specific value below");
            if (targ.ppClampEffector)
                ppDrawProperty("ppClampValue", "Clamp Value");
            pve();
            ps(5);

            pl("Effector Advanced");
            pv();
            if (targ.meshFilter.sharedMesh.vertexCount > MD_GlobalPreferences.vertexLimit)
                phb("The multithreading support is recommended as the mesh has more than " + MD_GlobalPreferences.vertexLimit.ToString() + " vertices",MessageType.Warning);
            ppDrawProperty("ppMultithreadingSupported", "Multithreading Supported");
            if (targ.ppMultithreadingSupported)
            {
                ppDrawProperty("ppMultithreadingProcessDelay", "Multithreading Process Delay", "The bigger value is, the smoother results you get");
                ph();
                if (pb("Start Editor Thread"))
                    targ.Effector_ThreadOption(true);
                if (pb("Stop Editor Thread"))
                    targ.Effector_ThreadOption(false);
                phe();
                phb("If you are going to edit mesh in the Editor, it's required to manage 'effector thread' manually. Press 'Start' to start editor thread for Mesh Effector. Press 'Stop' to stop editor thread for Mesh Effector.");
            }
            pve();
            ps(10);
            pv();
            if (pb("Refresh Effector Weights"))
                targ.Effector_ApplyWeights();
            pve();

            ps(15);
            ppAddMeshColliderRefresher(targ.gameObject);
            ppBackToMeshEditor(targ);
            ps();

            if (targ != null) serializedObject.Update();
        }
    }
}
#endif