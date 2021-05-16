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
    /// MDM(Mesh Deformation Modifier): Interactive Surface
    /// Interactive mesh surface with physically based system
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter))]
    [AddComponentMenu(MD_Debug.ORGANISATION + MD_Debug.PACKAGENAME + "Modifiers/Interactive Surface")]
    public class MDM_InteractiveSurface : MonoBehaviour
    {
        public bool ppRecalculateNormals = true;
        public bool ppRecalculateBounds = true;

        public bool ppRigidbodiesAllowed = true;

        public bool ppMultithreadingSupported = false;
        [Range(1, 30)] public int ppThreadSleep = 10;
        protected Thread ppThread;

        public bool ppCustomInteractionSpeed = false;
        public bool ppContinuousEffect = false;
        public float ppInteractionSpeed = 1.5f;

        public bool ppExponentialDeformation = false;
        public float ppInstantRadius = 1.5f;

        public Vector3 ppDirection = new Vector3(0, -1, 0);
        public float ppRadius = 0.8f;
        public bool ppAdjustTrackSizeToInputSize = true;
        public float ppMinimumForceDetection = 0;

        private Vector3[] originalVertices;
        private Vector3[] storedVertices;
        private Vector3[] startingVertices;
        [SerializeField] public MeshFilter meshFilter;

        public bool ppRepairSurface;
        public float ppRepairSpeed = 0.5f;

        public bool ppCollideWithSpecificObjects = false;
        public string ppCollisionTag = "";

        private void Awake()
        {
            if (meshFilter != null) return;

            ppRecalculateBounds = MD_GlobalPreferences.autoRecalcBounds;
            ppRecalculateNormals = MD_GlobalPreferences.autoRecalcNormals;

            meshFilter = GetComponent<MeshFilter>();
            MD_MeshProEditor.MeshProEditor_Utilities.util_PrepareMeshDeformationModifier(this, meshFilter, false);
            if (meshFilter.sharedMesh.vertices.Length > MD_GlobalPreferences.vertexLimit)
                ppMultithreadingSupported = true;
        }

        private void Start()
        {
            if (!Application.isPlaying) return;
            if (meshFilter == null)
            {
                MD_Debug.Debug(this, "Mesh filter doesn't exist", MD_Debug.DebugType.Error);
                return;
            }
            if (meshFilter.mesh == null)
            {
                MD_Debug.Debug(this, "Mesh doesn't exist", MD_Debug.DebugType.Error);
                return;
            }

            originalVertices = meshFilter.mesh.vertices;
            startingVertices = meshFilter.mesh.vertices;
            storedVertices = meshFilter.mesh.vertices;

            if (ppMultithreadingSupported)
            {
                Thrd_RealRot = transform.rotation;
                Thrd_RealPos = transform.position;
                Thrd_RealSca = transform.localScale;

                ppThread = new Thread(ThreadWork_ModifyMesh);
                ppThread.Start();
            }
        }

        private bool checkForUpdate_InterSpeed, checkForUpdate_Repair = false;
        private void LateUpdate()
        {
            //Returns if multithreading is enabled (it wouldn't make any sense)
            if (ppMultithreadingSupported) return;

            //Update 'custom interaction'
            if (ppCustomInteractionSpeed)
            {
                if (checkForUpdate_InterSpeed)
                {
                    int doneAll = 0;
                    if (ppContinuousEffect)
                    {
                        for (int i = 0; i < originalVertices.Length; i++)
                        {
                            if (originalVertices[i] == storedVertices[i])
                                doneAll++;
                            originalVertices[i] = Vector3.Lerp(originalVertices[i], storedVertices[i], ppInteractionSpeed * Time.deltaTime);
                        }
                        if (doneAll == originalVertices.Length)
                            checkForUpdate_InterSpeed = false;
                        meshFilter.mesh.SetVertices(originalVertices);
                    }
                    else
                    {
                        List<Vector3> Verts = new List<Vector3>();
                        Verts.AddRange(meshFilter.mesh.vertices);
                        for (int i = 0; i < Verts.Count; i++)
                        {
                            if (Verts[i] == storedVertices[i])
                                doneAll++;
                            Verts[i] = Vector3.Lerp(Verts[i], storedVertices[i], ppInteractionSpeed * Time.deltaTime);
                        }
                        if (doneAll == Verts.Count)
                            checkForUpdate_InterSpeed = false;
                        meshFilter.mesh.SetVertices(Verts);
                    }

                    if (ppRecalculateNormals) meshFilter.mesh.RecalculateNormals();
                    if (ppRecalculateBounds) meshFilter.mesh.RecalculateBounds();
                }
            }

            //Update 'repair surface'
            if (ppRepairSurface)
            {
                if (checkForUpdate_Repair)
                {
                    int doneAll = 0;
                    for (int i = 0; i < storedVertices.Length; i++)
                    {
                        if (originalVertices[i] == storedVertices[i])
                            doneAll++;
                        storedVertices[i] = Vector3.Lerp(storedVertices[i], startingVertices[i], ppRepairSpeed * Time.deltaTime);
                    }
                    if (doneAll == storedVertices.Length)
                        checkForUpdate_Repair = false;
                    if (!ppCustomInteractionSpeed)
                    {
                        meshFilter.mesh.SetVertices(storedVertices);
                        if (ppRecalculateNormals) meshFilter.mesh.RecalculateNormals();
                        if (ppRecalculateBounds) meshFilter.mesh.RecalculateBounds();
                    }
                }
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            if (!Application.isPlaying)
                return;
            if (!ppRigidbodiesAllowed)
                return;
            if (collision.contacts.Length == 0)
                return;
            if (ppMinimumForceDetection != 0 && collision.relativeVelocity.magnitude < ppMinimumForceDetection)
                return;
            if (ppAdjustTrackSizeToInputSize)
                ppRadius = collision.transform.localScale.magnitude / 4;
            foreach (ContactPoint cp in collision.contacts)
                InteractiveSurface_ModifyMesh(cp.point, ppRadius, ppDirection);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!Application.isPlaying)
                return;
            if (!ppRigidbodiesAllowed)
                return;
            if (collision.contacts.Length == 0)
                return;
            if (ppMinimumForceDetection != 0 && collision.relativeVelocity.magnitude < ppMinimumForceDetection)
                return;
            if (ppAdjustTrackSizeToInputSize)
                ppRadius = collision.transform.localScale.magnitude / 4;
            foreach (ContactPoint cp in collision.contacts)
                InteractiveSurface_ModifyMesh(cp.point, ppRadius, ppDirection);
        }

        /// <summary>
        /// Modify mesh surface with specific point, size and vertice direction
        /// </summary>
        /// <param name="AtPoint">Enter point of modification</param>
        /// <param name="Radius">Enter interaction radius</param>
        /// <param name="Direction">Enter direction of the vertices</param>
        public void InteractiveSurface_ModifyMesh(Vector3 AtPoint, float Radius, Vector3 Direction)
        {
            if (ppAdjustTrackSizeToInputSize == false)
                Radius = ppRadius;

            //If multithreading enabled, process multihread
            if (ppMultithreadingSupported)
            {
                Thrd_AtPoint = AtPoint;
                Thrd_Radius = Radius;
                Thrd_Dir = Direction;
                Thrd_RealPos = transform.position;
                Thrd_RealRot = transform.rotation;
                Thrd_RealSca = transform.localScale;
            }
            //Otherwise go for the default main thread
            else
            {
                for (int i = 0; i < storedVertices.Length; i++)
                {
                    Vector3 TransformedPoint = transform.TransformPoint(storedVertices[i]);
                    float distance = Vector3.Distance(new Vector3(AtPoint.x, 0, AtPoint.z), new Vector3(TransformedPoint.x, 0, TransformedPoint.z));
                    if (distance < Radius)
                    {
                        //Modify vertex in specific radius by linear or exponential distance prediction
                        Vector3 modifVertex = originalVertices[i] + (Direction * (ppExponentialDeformation ? (distance > Radius - ppInstantRadius ? (Radius - (distance)) : 1) : 1));
                        if (ppExponentialDeformation && ((ppDirection.y < 0 ? modifVertex.y > storedVertices[i].y : modifVertex.y < storedVertices[i].y))) continue;
                        storedVertices[i] = modifVertex;
                    }
                }
            }

            //Set vertices & continue
            if (!ppCustomInteractionSpeed || ppMultithreadingSupported)
                meshFilter.mesh.SetVertices(storedVertices);
            if (ppRecalculateNormals) meshFilter.mesh.RecalculateNormals();
            if (ppRecalculateBounds) meshFilter.mesh.RecalculateBounds();
            checkForUpdate_Repair = true;
            checkForUpdate_InterSpeed = true;
        }

        /// <summary>
        /// Reset current surface (Reset all vertices to the starting position)
        /// </summary>
        public void InteractiveSurface_ResetSurface()
        {
            for (int i = 0; i < storedVertices.Length; i++)
                storedVertices[i] = startingVertices[i];
        }

        //------External thread params----
        private Vector3 Thrd_AtPoint;
        private float Thrd_Radius;
        private Vector3 Thrd_Dir;
        private Vector3 Thrd_RealPos;
        private Vector3 Thrd_RealSca;
        private Quaternion Thrd_RealRot;
        //--------------------------------

        /// <summary>
        /// Main thread for mesh modification
        /// </summary>
        private void ThreadWork_ModifyMesh()
        {
            while (true)
            {
                for (int i = 0; i < storedVertices.Length; i++)
                {
                    Vector3 TransformedPoint = TransformPoint(Thrd_RealPos, Thrd_RealRot, Thrd_RealSca, storedVertices[i]);
                    float distance = Vector3.Distance(new Vector3(Thrd_AtPoint.x, 0, Thrd_AtPoint.z), new Vector3(TransformedPoint.x, 0, TransformedPoint.z));
                    if (distance < Thrd_Radius)
                    {
                        Vector3 modifVertex = originalVertices[i] + (Thrd_Dir * (ppExponentialDeformation ? (distance > Thrd_Radius - ppInstantRadius ? (Thrd_Radius - (distance)) : 1) : 1));
                        if (ppExponentialDeformation && (modifVertex.y > storedVertices[i].y)) continue;
                        storedVertices[i] = modifVertex;
                    }
                }
                Thread.Sleep(ppThreadSleep);
            }
        }

        /// <summary>
        /// Transform point from local to world space utility
        /// </summary>
        private Vector3 TransformPoint(Vector3 WorldPos, Quaternion WorldRot, Vector3 WorldScale, Vector3 Point)
        {
            var localToWorldMatrix = Matrix4x4.TRS(WorldPos, WorldRot, WorldScale);
            return localToWorldMatrix.MultiplyPoint3x4(Point);
        }

        private void OnApplicationQuit()
        {
            //Abort thread if possible
            if (ppThread != null && ppThread.IsAlive)
                ppThread.Abort();
        }

        private void OnDestroy()
        {
            //Abort thread if possible
            if (ppThread != null && ppThread.IsAlive)
                ppThread.Abort();
        }

        /// <summary>
        /// Modify current mesh by custom RaycastEvent
        /// </summary>
        public void InteractiveSurface_ModifyMesh(MDM_RaycastEvent RayEvent)
        {
            if (!Application.isPlaying)
                return;
            if (RayEvent == null)
                return;
            if (RayEvent.hits.Length > 0 && RayEvent.hits[0].collider.gameObject != this.gameObject)
                return;
            if (ppAdjustTrackSizeToInputSize)
                ppRadius = RayEvent.ppPointRay ? ppRadius : RayEvent.ppSphericalRadius;

            foreach (RaycastHit hit in RayEvent.hits)
                InteractiveSurface_ModifyMesh(hit.point, ppRadius, ppDirection);
        }
    }
}

#if UNITY_EDITOR
namespace MD_PluginEditor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MDM_InteractiveSurface))]
    public class MDM_InteractiveLandscape_Editor : MD_EditorUtilities
    {
        private MDM_InteractiveSurface m;

        private void OnEnable()
        {
            m = (MDM_InteractiveSurface)target;
        }

        public override void OnInspectorGUI()
        {
            ps();
            pl("General Settings", true);
            pv();
            pv();
            ppDrawProperty("ppRecalculateNormals", "Recalculate Normals");
            ppDrawProperty("ppRecalculateBounds", "Recalculate Bounds");
            pve();
            ppDrawProperty("ppMultithreadingSupported", "Multithreading Supported", "If enabled, the mesh will be ready for complex operations.");
            if (m.ppMultithreadingSupported)
            {
                ppDrawProperty("ppThreadSleep", "Thread Sleep", "Overall thread sleep (in miliseconds; The lower value is, the faster thread processing will be; but more performance it may take)");
                phb("The Interactive Surface component is ready for complex meshes and will create a new separated thread");
            }
            ps(5);
            pv();
            ppDrawProperty("ppDirection", "Overall Direction", "Direction of vertices after interaction");
            pve();
            pv();
            ppDrawProperty("ppExponentialDeformation", "Exponential Deform", "If enabled, the mesh will be deformed expontentially (the results will be much smoother)");
            if (m.ppExponentialDeformation)
                ppDrawProperty("ppInstantRadius", "Instant Radius Size", "If 'Exponential Deform' is enabled, vertices inside the 'Instant Radius' will be instantly affected. This will be subtracted from the input radius");
            pve();
            if (m.ppRigidbodiesAllowed)
            {
                pv();
                ppDrawProperty("ppAdjustTrackSizeToInputSize", "Adjust To Input Object Size", "Adjust radius size by collided object size. This will set the overall interaction radius to the input radius parameter (recommended if Allow Rigidbodies is enabled)");
                if (!m.ppAdjustTrackSizeToInputSize)
                    ppDrawProperty("ppRadius", "Interactive Radius", "Radius of vertices to be interacted");
                pve();
            }
            pve();

            ps(20);
            pl("Conditions", true);
            pv();
            pv();
            ppDrawProperty("ppRigidbodiesAllowed", "Allow Rigidbodies", "Allow Collision Enter & Collision Stay functions for Rigidbodies & other physically-based entities");
            if (m.ppRigidbodiesAllowed)
            {
                ppDrawProperty("ppMinimumForceDetection", "Force Detection Level", "Minimum rigidbody velocity detection [zero is default = without detection]");
                pv();
                ppDrawProperty("ppCollideWithSpecificObjects", "Collision With Specific Tag", "If enabled, collision will be occured only with included tag below...");
                if (m.ppCollideWithSpecificObjects)
                    ppDrawProperty("ppCollisionTag", "Collision Tag");
                pve();
            }
            pve();
            pve();
            if (m.ppMultithreadingSupported == false)
            {
                ps(20);
                pl("Additional Interaction Settings", true);
                pv();
                ppDrawProperty("ppCustomInteractionSpeed", "Custom Interaction Speed", "If enabled, you will be able to customize vertices speed after its interaction/ collision");
                if (m.ppCustomInteractionSpeed)
                {
                    pv();
                    ppDrawProperty("ppInteractionSpeed", "Interaction Speed");
                    ppDrawProperty("ppContinuousEffect", "Enable Continuous Effect", "If enabled, interacted vertices will keep moving deeper");
                    pve();
                }
                pve();

                pv();
                ppDrawProperty("ppRepairSurface", "Repair Mesh", "Repair mesh after some time and interval");
                if (m.ppRepairSurface)
                    ppDrawProperty("ppRepairSpeed", "Repair Speed");
                pve();
            }
            ps(15);
            ppAddMeshColliderRefresher(m.gameObject);
            ppBackToMeshEditor(m);

            if (target != null) serializedObject.Update();
        }
    }
}
#endif
