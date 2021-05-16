using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using UnityEngine.EventSystems;
using System.Linq;

namespace MD_Plugin
{
    /// <summary>
    /// MDM(Mesh Deformation Modifier): Sculpting Pro
    /// Full sculpting system for mesh renderers in editor / at runtime
    /// Big thanks to community for optimizing suggestions!
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter))]
    [AddComponentMenu(MD_Debug.ORGANISATION + MD_Debug.PACKAGENAME + "Sculpting Pro")]
    public class MDM_SculptingPro : MonoBehaviour
    {
        public bool SS_AtRuntime = false;
        public bool SS_InEditMode = false;
        public bool SS_MobileSupport = false;

        public bool SS_AutoRecalculateNormals = true;
        public bool SS_AutoRecalculateBounds = true;

        public bool SS_UseBrushProjection = true;
        public GameObject SS_BrushProjection;

        public float SS_BrushSize = 0.5f;
        public float SS_BrushStrength = 0.05f;

        public bool SS_MultithreadingSupported = false;
        [Range(1,30)] public int SS_MultithreadingProcessDelay = 10;
        [SerializeField] private Thread Multithread;
        [SerializeField] private ManualResetEvent Multithread_ManualEvent = new ManualResetEvent(true);
        [System.Serializable]
        public class Multithreading_Internal
        {
            public Vector3 mth_WorldPoint;
            public float mth_Radius;
            public float mth_Strength;
            public SS_State_Internal mth_State;
            public Vector3 mth_WorldPos;
            public Quaternion mth_WorldRot;
            public Vector3 mth_WorldScale;
            public Vector3 mth_Direction;
            public float mth_StylizeIntensity;
            public float mth_SmoothIntensity;
            public int mth_SmoothingType;
            public SS_NoiseFunctDirections mth_NoiseDirs;
            public SS_SculptingType_ SS_SculptingType;
            public int SS_MultithreadingProcessDelay;

            public void SetParams(Vector3 worldPoint, float Radius, float Strength, Vector3 Dir, SS_State_Internal State, Vector3 RealPos, Vector3 RealScale, Quaternion RealRot, SS_NoiseFunctDirections NoiseDirs, SS_SculptingType_ SS_SculptingType__, int SS_MultithreadingProcessDelay_, float stylizedIntens, float smoothIntens, int smoothType)
            {
                mth_WorldPoint = worldPoint;
                mth_Radius = Radius / 20;
                mth_Strength = Strength;
                mth_State = State;
                mth_WorldPos = RealPos;
                mth_WorldRot = RealRot;
                mth_WorldScale = RealScale;
                mth_Direction = Dir;
                mth_NoiseDirs = NoiseDirs;
                SS_SculptingType = SS_SculptingType__;
                SS_MultithreadingProcessDelay = SS_MultithreadingProcessDelay_;
                mth_StylizeIntensity = stylizedIntens;
                mth_SmoothIntensity = smoothIntens;
                mth_SmoothingType = smoothType;
            }
        }
        public Multithreading_Internal Multithreading_Manager;

        public enum SS_State_Internal : int { None = 0, Raise = 1, Lower = 2, Revert = 3, Noise = 4, Smooth = 5, Stylize = 6 };
        public SS_State_Internal SS_State = SS_State_Internal.None;

        public enum SS_MeshSculptMode_Internal { VerticesDirection, BrushDirection, CustomDirection, CustomDirectionObject, InternalScriptDirection };
        //VerticesDirection         Sets the direction by vertice normals
        //BrushDirection            Sets the direction by brush rotation
        //CustomDirection           Sets the direction by custom euler values
        //CustomDirectionObject     Sets the direction by specific object's local direction
        //InternalScriptDirection   Sets the direction by internal script (programmer may declare an input for the direction right in the Sculpting method)
        public SS_MeshSculptMode_Internal SS_MeshSculptMode = SS_MeshSculptMode_Internal.BrushDirection;
        public Vector3 SS_CustomDirection;
        public bool SS_EnableHeightLimitations = false;
        public Vector2 SS_HeightLimitations;
        public bool SS_EnableDistanceLimitations = false;
        public float SS_DistanceLimitation = 1.0f;
        public enum SS_CustomDirObjDirection_Internal { Up, Down, Forward, Back, Right, Left};
        public SS_CustomDirObjDirection_Internal SS_CustomDirObjDirection;
        public GameObject SS_CustomDirectionObject;
        public bool SS_UpdateColliderAfterRelease = true;
        public enum SS_SculptingType_ { Linear, Exponential};
        public SS_SculptingType_ SS_SculptingType = SS_SculptingType_.Exponential;

        //---Runtime Settings - Input
        public bool SS_UseInput = true;
        public bool SS_VRInput = false;
        public bool SS_UseRaiseFunct = true;
        public bool SS_UseLowerFunct = true;
        public bool SS_UseRevertFunct = false;
        public bool SS_UseNoiseFunct = false;
        public bool SS_UseSmoothFunct = false;
        public bool SS_UseStylizeFunct = false;

        public enum SS_NoiseFunctDirections { XYZ, XZ, XY, YZ, Z, Y, X, Centrical};
        public SS_NoiseFunctDirections SS_NoiseFunctionDirections = SS_NoiseFunctDirections.XYZ;
        [Range(0.01f,0.99f)]    public float SS_StylizeIntensity = 0.65f;
        [Range(0.01f, 1f)]      public float SS_SmoothIntensity = 0.5f;
        public enum SS_SmoothType : int { HcFilter, LaplacianFilter };
        public SS_SmoothType SS_smoothType = SS_SmoothType.HcFilter;
        public KeyCode SS_SculptingRaiseInput = KeyCode.Mouse0;
        public KeyCode SS_SculptingLowerInput = KeyCode.Mouse1;
        public KeyCode SS_SculptingRevertInput = KeyCode.Mouse2;
        public KeyCode SS_SculptingNoiseInput = KeyCode.LeftControl;
        public KeyCode SS_SculptingSmoothInput = KeyCode.LeftAlt;
        public KeyCode SS_SculptingStylizeInput = KeyCode.Z;

        public Camera SS_MainCamera;
        public bool SS_SculptFromCursor = true;
        public Transform SS_SculptOrigin;
        public LayerMask SS_SculptingLayerMask = ~0;

        public bool SS_RecordHistory = false;
        [Range(1,20)] [Tooltip("It's recommended to have limited history records due to the performance & memory")] public int SS_MaxHistoryRecords = 5;
        public struct HistoryRecords
        {
            public string historyNotes;
            public Vector3[] VertexPositions;
        }
        public List<HistoryRecords> historyRecords = new List<HistoryRecords>();

        //---Internal Essentials

        private bool vrInputDown = false;

        [SerializeField] public MeshFilter meshFilter;
        //Original stored vertices from first setup
        [SerializeField] protected Vector3[] originVertices;
        //Working vertices set right after startup
        [SerializeField] protected Vector3[] workingVertices;
        //In case of using multithreading - thread vertices
        [SerializeField] protected Vector3[] threadVertices;
        //In case of using multithreading - thread triangles
        [SerializeField] protected int[] threadTriangles;

        private void Awake()
        {
            if (meshFilter) return;            

            SS_AutoRecalculateBounds = MD_GlobalPreferences.autoRecalcBounds;
            SS_AutoRecalculateNormals = MD_GlobalPreferences.autoRecalcNormals;

            meshFilter = GetComponent<MeshFilter>();
            MD_MeshProEditor.MeshProEditor_Utilities.util_PrepareMeshDeformationModifier(this, meshFilter, false);
            if (meshFilter.sharedMesh.vertexCount > MD_GlobalPreferences.vertexLimit)
                SS_MultithreadingSupported = true;

            originVertices = meshFilter.sharedMesh.vertices;
            workingVertices = meshFilter.sharedMesh.vertices;
            if (SS_MultithreadingSupported)
            {
                threadVertices = meshFilter.sharedMesh.vertices;
                if (Multithread == null)
                {
                    Multithread = new Thread(SS_Funct_DoSculpting_ThreadWorker);
                    Multithread.Start();
                }
            }

            if (!GetComponent<MeshCollider>())
                mc = gameObject.AddComponent<MeshCollider>();
        }

        private void Start()
        {
            if (SS_BrushProjection == null && SS_UseBrushProjection)
            {
                GameObject brushProj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Material m = new Material(Shader.Find("Transparent/Diffuse"));
                m.color = new Color(0, 128, 0, 0.2f);
                brushProj.GetComponent<Renderer>().sharedMaterial = m;
                DestroyImmediate(brushProj.GetComponent<Collider>());
                brushProj.name = "Brush_Object";

                if (SS_MultithreadingSupported)
                {
                    GameObject br = new GameObject("BrushTemp_" + this.name);

                    SS_BrushProjection = br;
                    SS_BrushProjection.SetActive(false);

                    brushProj.transform.parent = br.transform;
                    brushProj.transform.localPosition = Vector3.zero;
                    brushProj.transform.localRotation = Quaternion.identity;
                    brushProj.transform.localScale = Vector3.one * 0.1f;
                }
                else
                {
                    brushProj.SetActive(false);
                    SS_BrushProjection = brushProj;
                }
            }

            if (Application.isPlaying == false) return;

            originVertices = meshFilter.sharedMesh.vertices;
            workingVertices = meshFilter.sharedMesh.vertices;

            if (!SS_MainCamera)
            {
                if (SS_MobileSupport || (!SS_MobileSupport && SS_SculptFromCursor))
                {
                    SS_MainCamera = Camera.main;
                    if (!SS_MainCamera) Debug.LogError("There is no main camera assigned!");
                }
            }

            if (SS_MultithreadingSupported)
            {
                threadVertices = meshFilter.sharedMesh.vertices;

                if(SS_UseSmoothFunct || SS_VRInput)
                    threadTriangles = meshFilter.sharedMesh.triangles;

                CheckForThread(true, 0.5f);
            }
        }

        private RaycastHit[] SSInternal_RaycasthitStorage = new RaycastHit[1];
        private void Update()
        {
            if (!Application.isPlaying)
                return;
            if (!SS_AtRuntime)
                return;

            if (SS_MultithreadingSupported)
            {
                if (Multithreading_Manager == null)
                {
                    Multithreading_Manager = new Multithreading_Internal();
                    Multithread_ManualEvent = new ManualResetEvent(true);
                }
                Multithreading_Manager.mth_WorldPos = transform.position;
                Multithreading_Manager.mth_WorldRot = transform.rotation;
                Multithreading_Manager.mth_WorldScale = transform.localScale;
            }

            if (!SS_UseInput)
                return;            

            bool eventSystemOverObject = false;
            if (EventSystem.current != null) eventSystemOverObject = EventSystem.current.IsPointerOverGameObject();

            Ray r = new Ray();
            if (!SS_MobileSupport)
            {
                if (SS_SculptFromCursor && !SS_VRInput)
                    r = SS_MainCamera.ScreenPointToRay(Input.mousePosition);
                else
                    r = new Ray(SS_SculptOrigin.transform.position, SS_SculptOrigin.transform.forward);
            }
            else
            {
                if (Input.touchCount > 0)
                    r = SS_MainCamera.ScreenPointToRay(Input.GetTouch(0).position);
            }

            if (Physics.RaycastNonAlloc(r, SSInternal_RaycasthitStorage, Mathf.Infinity, SS_SculptingLayerMask) > 0 && !eventSystemOverObject)
            {
                RaycastHit hit = SSInternal_RaycasthitStorage[0];
                if (hit.collider.gameObject == gameObject)
                {
                    Internal_SetActiveOptimized(SS_BrushProjection, true);
                    SS_BrushProjection.transform.position = hit.point;
                    SS_BrushProjection.transform.rotation = Quaternion.FromToRotation(-Vector3.forward, hit.normal);
                    SS_BrushProjection.transform.localScale = new Vector3(SS_BrushSize, SS_BrushSize, SS_BrushSize);

                    SS_Funct_ManageControls(hit);
                }
                else
                    Internal_SetActiveOptimized(SS_BrushProjection, false);
            }
            else
                Internal_SetActiveOptimized(SS_BrushProjection, false);


            if (SS_MobileSupport)
            {
                if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
                    SS_Funct_RecordOnControlsUp();
            }
            else if(!SS_VRInput)
            {
                if (Input.GetKeyUp(SS_SculptingRaiseInput) && SS_UseRaiseFunct)
                    SS_Funct_RecordOnControlsUp();
                if (Input.GetKeyUp(SS_SculptingLowerInput) && SS_UseLowerFunct)
                    SS_Funct_RecordOnControlsUp();
                if (Input.GetKeyUp(SS_SculptingRevertInput) && SS_UseRevertFunct)
                    SS_Funct_RecordOnControlsUp();
                if (Input.GetKeyUp(SS_SculptingRevertInput) && SS_UseNoiseFunct)
                    SS_Funct_RecordOnControlsUp();
                if (Input.GetKeyUp(SS_SculptingSmoothInput) && SS_UseSmoothFunct)
                    SS_Funct_RecordOnControlsUp();
                if (Input.GetKeyUp(SS_SculptingStylizeInput) && SS_UseStylizeFunct)
                    SS_Funct_RecordOnControlsUp();
            }
        }

        private void Internal_SetActiveOptimized(GameObject target, bool state)
        {
            if (!SS_UseBrushProjection)
            {
                if(target.activeSelf != SS_UseBrushProjection)
                    target.SetActive(false);
                return;
            }

            if (target != null && target.activeSelf != state)
                target.SetActive(state);
        }

        private void SS_Funct_ManageControls(RaycastHit hit)
        {
            if (SS_MobileSupport)
            {
                if (SS_UseRaiseFunct && SS_State == SS_State_Internal.Raise)
                {
                    SS_Funct_DoSculpting(hit.point, SS_BrushProjection.transform.forward, SS_BrushSize, SS_BrushStrength, SS_State_Internal.Raise);
                    if (!SS_UpdateColliderAfterRelease)
                        SS_Funct_RefreshMeshCollider();
                }
                else if (SS_UseLowerFunct && SS_State == SS_State_Internal.Lower)
                {
                    SS_Funct_DoSculpting(hit.point, SS_BrushProjection.transform.forward, SS_BrushSize, SS_BrushStrength, SS_State_Internal.Lower);
                    if (!SS_UpdateColliderAfterRelease)
                        SS_Funct_RefreshMeshCollider();
                }
                else if (SS_UseRevertFunct && SS_State == SS_State_Internal.Revert)
                {
                    SS_Funct_DoSculpting(hit.point, SS_BrushProjection.transform.forward, SS_BrushSize, SS_BrushStrength, SS_State_Internal.Revert);
                    if (!SS_UpdateColliderAfterRelease)
                        SS_Funct_RefreshMeshCollider();
                }
                else if (SS_UseNoiseFunct && SS_State == SS_State_Internal.Noise)
                {
                    SS_Funct_DoSculpting(hit.point, SS_BrushProjection.transform.forward, SS_BrushSize, SS_BrushStrength, SS_State_Internal.Noise);
                    if (!SS_UpdateColliderAfterRelease)
                        SS_Funct_RefreshMeshCollider();
                }
                else if (SS_UseSmoothFunct && SS_State == SS_State_Internal.Smooth)
                {
                    SS_Funct_DoSculpting(hit.point, SS_BrushProjection.transform.forward, SS_BrushSize, SS_BrushStrength, SS_State_Internal.Smooth);
                    if (!SS_UpdateColliderAfterRelease)
                        SS_Funct_RefreshMeshCollider();
                }
                else if (SS_UseStylizeFunct && SS_State == SS_State_Internal.Stylize)
                {
                    SS_Funct_DoSculpting(hit.point, SS_BrushProjection.transform.forward, SS_BrushSize, SS_BrushStrength, SS_State_Internal.Stylize);
                    if (!SS_UpdateColliderAfterRelease)
                        SS_Funct_RefreshMeshCollider();
                }
            }
            else if(!SS_VRInput)
            {
                if (Internal_GetControlInput(SS_SculptingRaiseInput) && SS_UseRaiseFunct)
                {
                    SS_Funct_DoSculpting(hit.point, SS_BrushProjection.transform.forward, SS_BrushSize, SS_BrushStrength, SS_State_Internal.Raise);
                    if (!SS_UpdateColliderAfterRelease)
                        SS_Funct_RefreshMeshCollider();
                }
                else if (Internal_GetControlInput(SS_SculptingLowerInput) && SS_UseLowerFunct)
                {
                    SS_Funct_DoSculpting(hit.point, SS_BrushProjection.transform.forward, SS_BrushSize, SS_BrushStrength, SS_State_Internal.Lower);
                    if (!SS_UpdateColliderAfterRelease)
                        SS_Funct_RefreshMeshCollider();
                }
                else if (Internal_GetControlInput(SS_SculptingRevertInput) && SS_UseRevertFunct)
                {
                    SS_Funct_DoSculpting(hit.point, SS_BrushProjection.transform.forward, SS_BrushSize, SS_BrushStrength, SS_State_Internal.Revert);
                    if (!SS_UpdateColliderAfterRelease)
                        SS_Funct_RefreshMeshCollider();
                }
                else if (Internal_GetControlInput(SS_SculptingNoiseInput) && SS_UseNoiseFunct)
                {
                    SS_Funct_DoSculpting(hit.point, SS_BrushProjection.transform.forward, SS_BrushSize, SS_BrushStrength, SS_State_Internal.Noise);
                    if (!SS_UpdateColliderAfterRelease)
                        SS_Funct_RefreshMeshCollider();
                }
                else if (Internal_GetControlInput(SS_SculptingSmoothInput) && SS_UseSmoothFunct)
                {
                    SS_Funct_DoSculpting(hit.point, SS_BrushProjection.transform.forward, SS_BrushSize, SS_BrushStrength, SS_State_Internal.Smooth);
                    if (!SS_UpdateColliderAfterRelease)
                        SS_Funct_RefreshMeshCollider();
                }
                else if (Internal_GetControlInput(SS_SculptingStylizeInput) && SS_UseStylizeFunct)
                {
                    SS_Funct_DoSculpting(hit.point, SS_BrushProjection.transform.forward, SS_BrushSize, SS_BrushStrength, SS_State_Internal.Stylize);
                    if (!SS_UpdateColliderAfterRelease)
                        SS_Funct_RefreshMeshCollider();
                }
            }
            else
            {
                if (vrInputDown)
                {
                    SS_Funct_DoSculpting(hit.point, SS_BrushProjection.transform.forward, SS_BrushSize, SS_BrushStrength, SS_State);
                    if (!SS_UpdateColliderAfterRelease)
                        SS_Funct_RefreshMeshCollider();
                }
            }
        }

        private bool Internal_GetControlInput(KeyCode key)
        {
            bool final;

            if (!SS_MobileSupport)
            {
                final = Input.GetKey(key);
                if (SS_MultithreadingSupported)
                {
                    if (!final && Multithread != null && Multithread.IsAlive)
                        Multithread_ManualEvent.Reset();
                }
            }
            else
            {
                final = Input.touchCount > 0;
                if (SS_MultithreadingSupported)
                {
                    if (!final && Multithread != null && Multithread.IsAlive)
                        Multithread_ManualEvent.Reset();
                }
            }
            if (final) SS_Funct_RecordOnControlsDown();
            return final;
        }

        #region Essential Functions

        /// <summary>
        /// Restore original mesh
        /// </summary>
        public void SS_Funct_RestoreOriginal()
        {
            if (meshFilter == null || (meshFilter && meshFilter.sharedMesh == null))
            {
                Debug.Log("Sculpting Pro: The object doesn't contain Mesh Filter.");
                return;
            }

            if (originVertices.Length == 0) return;

            meshFilter.sharedMesh.SetVertices(originVertices);
            System.Array.Copy(originVertices, workingVertices, originVertices.Length);

            if (SS_MultithreadingSupported) threadVertices = meshFilter.sharedMesh.vertices;
            if (SS_AutoRecalculateBounds) meshFilter.sharedMesh.RecalculateBounds();
            if (SS_AutoRecalculateNormals) meshFilter.sharedMesh.RecalculateNormals();
            if (SS_RecordHistory) historyRecords = new List<HistoryRecords>();
            SS_Funct_RefreshMeshCollider();
        }

        /// <summary>
        /// Sculpt current mesh by specific parameters
        /// </summary>
        /// <param name="WorldPoint">World point [example: raycast hit point]</param>
        /// <param name="Radius">Range or radius of the point</param>
        /// <param name="Strength">Strenght & hardness of the sculpting</param>
        /// <param name="State">Sculpting state</param>
        public void SS_Funct_DoSculpting(Vector3 WorldPoint, Vector3 Direction, float Radius, float Strength, SS_State_Internal State)
        {
            if(SS_AutoRecalculateNormals) meshFilter.sharedMesh.RecalculateNormals();
            if (SS_AutoRecalculateBounds) meshFilter.sharedMesh.RecalculateBounds();

            //---Multithreaded sculpting
            if (SS_MultithreadingSupported)
            {
                if (!threadDone)
                    return;
                if (SS_MeshSculptMode == SS_MeshSculptMode_Internal.CustomDirectionObject)
                {
                    if (SS_CustomDirObjDirection == SS_CustomDirObjDirection_Internal.Up)
                        Direction = SS_CustomDirectionObject.transform.up;
                    else if (SS_CustomDirObjDirection == SS_CustomDirObjDirection_Internal.Down)
                        Direction = -SS_CustomDirectionObject.transform.up;
                    else if (SS_CustomDirObjDirection == SS_CustomDirObjDirection_Internal.Forward)
                        Direction = SS_CustomDirectionObject.transform.forward;
                    else if (SS_CustomDirObjDirection == SS_CustomDirObjDirection_Internal.Back)
                        Direction = -SS_CustomDirectionObject.transform.forward;
                    else if (SS_CustomDirObjDirection == SS_CustomDirObjDirection_Internal.Right)
                        Direction = SS_CustomDirectionObject.transform.right;
                    else if (SS_CustomDirObjDirection == SS_CustomDirObjDirection_Internal.Left)
                        Direction = -SS_CustomDirectionObject.transform.right;
                }

                Multithreading_Manager.SetParams(transform.InverseTransformPoint(WorldPoint), Radius, Strength, Direction, State, transform.position, transform.localScale, transform.rotation, SS_NoiseFunctionDirections, SS_SculptingType, SS_MultithreadingProcessDelay, SS_StylizeIntensity, SS_SmoothIntensity, (int)SS_smoothType);
                meshFilter.sharedMesh.vertices = threadVertices;
                Multithread_ManualEvent.Set();
                return;
            }
            else if(State == SS_State_Internal.Smooth || State == SS_State_Internal.Stylize)
            {
                MD_Debug.Debug(this, "Due to the performance & safety, the Smooth & Stylize functions in Sculpting Pro with non-multithreaded modifier are not allowed! Please enable 'Multithreading' option.", MD_Debug.DebugType.Error);
                return;
            }

            //---Main-Thread sculpting
            int i = 0;
            while (i < workingVertices.Length)
            {
                if (Vector3.Distance(transform.TransformPoint(workingVertices[i]), WorldPoint) < Radius)
                {
                    float str = Strength;
                    if (State == SS_State_Internal.Raise)
                        str *= -1;

                    Vector3 origin = transform.TransformPoint(workingVertices[i]);
                    Vector3 oStorate = origin;

                    if (State == SS_State_Internal.Revert) workingVertices[i] = Vector3.Lerp(workingVertices[i], originVertices[i], 0.1f);
                    else
                    {
                        Vector3 Dir = Direction;

                        if (SS_MeshSculptMode == SS_MeshSculptMode_Internal.CustomDirection)
                            Dir = SS_CustomDirection;
                        else if (SS_MeshSculptMode == SS_MeshSculptMode_Internal.CustomDirectionObject)
                        {
                            if (SS_CustomDirObjDirection == SS_CustomDirObjDirection_Internal.Up)
                                Dir = SS_CustomDirectionObject.transform.up;
                            else if (SS_CustomDirObjDirection == SS_CustomDirObjDirection_Internal.Down)
                                Dir = -SS_CustomDirectionObject.transform.up;
                            else if (SS_CustomDirObjDirection == SS_CustomDirObjDirection_Internal.Forward)
                                Dir = SS_CustomDirectionObject.transform.forward;
                            else if (SS_CustomDirObjDirection == SS_CustomDirObjDirection_Internal.Back)
                                Dir = -SS_CustomDirectionObject.transform.forward;
                            else if (SS_CustomDirObjDirection == SS_CustomDirObjDirection_Internal.Right)
                                Dir = SS_CustomDirectionObject.transform.right;
                            else if (SS_CustomDirObjDirection == SS_CustomDirObjDirection_Internal.Left)
                                Dir = -SS_CustomDirectionObject.transform.right;
                        }
                        else if (SS_MeshSculptMode == SS_MeshSculptMode_Internal.VerticesDirection)
                            Dir = transform.TransformDirection(-meshFilter.sharedMesh.vertices[i]);

                        if (SS_EnableHeightLimitations)
                        {
                            if (origin.y < SS_HeightLimitations.x && State == SS_State_Internal.Lower)
                                str = 0;
                            if (origin.y > SS_HeightLimitations.y && State == SS_State_Internal.Raise)
                                str = 0;
                        }
                        
                        if (State == SS_State_Internal.Noise)
                        {
                            float rand_x = Random.Range(-0.01f, 0.01f);
                            float rand_y = Random.Range(-0.01f, 0.01f);
                            float rand_z = Random.Range(-0.01f, 0.01f);
                            switch (SS_NoiseFunctionDirections)
                            {
                                case SS_NoiseFunctDirections.X:
                                    origin.x += rand_x * str;
                                    break;
                                case SS_NoiseFunctDirections.Y:
                                    origin.y += rand_y * str;
                                    break;
                                case SS_NoiseFunctDirections.Z:
                                    origin.z += rand_z * str;
                                    break;

                                case SS_NoiseFunctDirections.XY:
                                    origin.x += rand_x * str;
                                    origin.y += rand_y * str;
                                    break;
                                case SS_NoiseFunctDirections.XZ:
                                    origin.x += rand_x * str;
                                    origin.z += rand_z * str;
                                    break;

                                case SS_NoiseFunctDirections.YZ:
                                    origin.y += rand_y * str;
                                    origin.z += rand_z * str;
                                    break;
                                case SS_NoiseFunctDirections.XYZ:
                                    origin.x += rand_x * str;
                                    origin.y += rand_y * str;
                                    origin.z += rand_z * str;
                                    break;
								case SS_NoiseFunctDirections.Centrical:
								 	float ran = Random.Range(-0.01f, 0.01f);
									Vector3 v = (origin - transform.position) * ran;
                                    origin.x += v.x * str;
                                    origin.y += v.y * str;
                                    origin.z += v.z * str;
									break;
                            }
                        }
                        else
                        {
                            str *= 0.05f;
                            if (SS_SculptingType == SS_SculptingType_.Exponential)
                                str *= (Radius - Vector3.Distance(origin, WorldPoint));
                            origin += Dir * str;
                        }

                        if (SS_EnableDistanceLimitations)
                        {
                            Vector3 cur = workingVertices[i];
                            Vector3 stor = originVertices[i];
                            float curDist = Vector3.Distance(cur, stor);
                            bool inrange = (curDist > SS_DistanceLimitation);
                            bool reversed = Vector3.Distance(cur + (Dir * str), stor) > curDist;

                            if (inrange && reversed)
                                origin = oStorate;
                        }

                        workingVertices[i] = transform.InverseTransformPoint(origin);
                    }
                }
                i++;
            }
                
            meshFilter.sharedMesh.SetVertices(workingVertices);
        }

        #endregion

        #region Multithreading

        private bool threadDone = true;
        private void SS_Funct_DoSculpting_ThreadWorker()
        {
            if (threadVertices == null) return;

            while (true)
            {
                Multithread_ManualEvent.WaitOne();

                int i = 0;
                threadDone = false;
                if(Multithreading_Manager == null)
                {
                    threadDone = true;
                    continue;
                }

                if ( Multithreading_Manager.mth_State == SS_State_Internal.Smooth)
                {
                    switch (Multithreading_Manager.mth_SmoothingType)
                    {
                        case 0:
                            threadVertices = MD_MeshMathUtilities.smoothing_HCFilter.HCFilter(threadVertices, threadTriangles.ToArray(), new MD_MeshMathUtilities.SculptingAttributes()
                            {
                                radius = Multithreading_Manager.mth_Radius,
                                transPos = Multithreading_Manager.mth_WorldPos,
                                transRot = Multithreading_Manager.mth_WorldRot,
                                transScale = Multithreading_Manager.mth_WorldScale,
                                worldPoint = Multithreading_Manager.mth_WorldPoint,
                            });
                            break;
                        case 1:
                            threadVertices = MD_MeshMathUtilities.smoothing_LaplacianFilter.LaplacianFilter(threadVertices, threadTriangles.ToArray(), Multithreading_Manager.mth_SmoothIntensity, new MD_MeshMathUtilities.SculptingAttributes()
                            {
                                radius = Multithreading_Manager.mth_Radius,
                                transPos = Multithreading_Manager.mth_WorldPos,
                                transRot = Multithreading_Manager.mth_WorldRot,
                                transScale = Multithreading_Manager.mth_WorldScale,
                                worldPoint = Multithreading_Manager.mth_WorldPoint,
                            });
                            break;
                    }

                }
                else
                {
                    while (i < threadVertices.Length)
                    {
                        if (Vector3.Distance(threadVertices[i], Multithreading_Manager.mth_WorldPoint) < Multithreading_Manager.mth_Radius)
                        {
                            if (Multithreading_Manager.mth_State == SS_State_Internal.Stylize)
                            {
                                Vector3 v = Vector3.positiveInfinity;
                                float minD = Mathf.Infinity;
                                for (int x = 0; x < threadVertices.Length; x++)
                                {
                                    if (x == i)
                                        continue;
                                    float dist = Vector3.Distance(threadVertices[x], threadVertices[i]);
                                    if (dist < minD)
                                    {
                                        minD = dist;
                                        v = threadVertices[x];
                                    }
                                }
                                Vector3 ttt = MD_MeshMathUtilities.CustomLerp(threadVertices[i], v, Multithreading_Manager.mth_StylizeIntensity);
                                threadVertices[i] = ttt;
                                i++;
                                continue;
                            }

                            float str = Multithreading_Manager.mth_Strength;
                            if (Multithreading_Manager.mth_State == SS_State_Internal.Lower)
                                str *= -1;

                            if (Multithreading_Manager.mth_State == SS_State_Internal.Revert)
                                threadVertices[i] = Vector3.Lerp(threadVertices[i], originVertices[i], 0.1f);

                            Vector3 origin = MD_MeshMathUtilities.TransformPoint(Multithreading_Manager.mth_WorldPos, Multithreading_Manager.mth_WorldRot, Multithreading_Manager.mth_WorldScale, threadVertices[i]);
                            Vector3 oStorage = origin;

                            if (Multithreading_Manager.mth_State != SS_State_Internal.Revert)
                            {
                                Vector3 Dir = Multithreading_Manager.mth_Direction;

                                if (SS_MeshSculptMode == SS_MeshSculptMode_Internal.CustomDirection)
                                    Dir = SS_CustomDirection;
                                else if (SS_MeshSculptMode == SS_MeshSculptMode_Internal.CustomDirectionObject)
                                {

                                }
                                else if (SS_MeshSculptMode == SS_MeshSculptMode_Internal.VerticesDirection)
                                    Dir = MD_MeshMathUtilities.TransformDirection(Multithreading_Manager.mth_WorldPos, Multithreading_Manager.mth_WorldRot, Multithreading_Manager.mth_WorldScale, -threadVertices[i]);

                                if (SS_EnableHeightLimitations)
                                {
                                    if (origin.y < SS_HeightLimitations.x && Multithreading_Manager.mth_State == SS_State_Internal.Lower)
                                        str = 0;
                                    if (origin.y > SS_HeightLimitations.y && Multithreading_Manager.mth_State == SS_State_Internal.Raise)
                                        str = 0;
                                }
                               
                                if (Multithreading_Manager.mth_State == SS_State_Internal.Noise)
                                {
                                    float rand_x = (float)GetRandomNumber(-0.01f, 0.01f);
                                    float rand_y = (float)GetRandomNumber(-0.01f, 0.01f);
                                    float rand_z = (float)GetRandomNumber(-0.01f, 0.01f);
                                    switch (Multithreading_Manager.mth_NoiseDirs)
                                    {
                                        case SS_NoiseFunctDirections.X:
                                            origin.x += rand_x * str;
                                            break;
                                        case SS_NoiseFunctDirections.Y:
                                            origin.y += rand_y * str;
                                            break;
                                        case SS_NoiseFunctDirections.Z:
                                            origin.z += rand_z * str;
                                            break;

                                        case SS_NoiseFunctDirections.XY:
                                            origin.x += rand_x * str;
                                            origin.y += rand_y * str;
                                            break;
                                        case SS_NoiseFunctDirections.XZ:
                                            origin.x += rand_x * str;
                                            origin.z += rand_z * str;
                                            break;

                                        case SS_NoiseFunctDirections.YZ:
                                            origin.y += rand_y * str;
                                            origin.z += rand_z * str;
                                            break;
                                        case SS_NoiseFunctDirections.XYZ:
                                            origin.x += rand_x * str;
                                            origin.y += rand_y * str;
                                            origin.z += rand_z * str;
                                            break;
										case SS_NoiseFunctDirections.Centrical:
										 	float ran = (float)GetRandomNumber(-0.01f, 0.01f);
											Vector3 v = (origin - Multithreading_Manager.mth_WorldPos) * ran;
                                            origin.x += v.x * str;
                                            origin.y += v.y * str;
                                            origin.z += v.z * str;
											break;
                                    }
                                }
                                else
                                {
                                    str *= 0.05f;
                                    if (Multithreading_Manager.SS_SculptingType == SS_SculptingType_.Exponential)
                                        str *= (Multithreading_Manager.mth_Radius - Vector3.Distance(origin, Multithreading_Manager.mth_WorldPoint));
                                    origin += Dir * str;
                                }

                                if (SS_EnableDistanceLimitations)
                                {
                                    Vector3 cur = threadVertices[i];
                                    Vector3 stor = originVertices[i];
                                    float curDist = Vector3.Distance(cur, stor);
                                    bool inrange = (curDist > SS_DistanceLimitation);
                                    bool reversed = Vector3.Distance(cur + (Dir * str), stor) > curDist;

                                    if (inrange && reversed)
                                        origin = oStorage;
                                }

                                threadVertices[i] = MD_MeshMathUtilities.TransformPointInverse(Multithreading_Manager.mth_WorldPos, Multithreading_Manager.mth_WorldRot, Multithreading_Manager.mth_WorldScale, origin);
                            }
                        }
                        i++;
                    }
                }
                
                threadDone = true;
                Thread.Sleep(Multithreading_Manager.SS_MultithreadingProcessDelay);
            }
        }

        private static System.Random random = new System.Random();
        public double GetRandomNumber(double minimum, double maximum)
        {
            return random.NextDouble() * (maximum - minimum) + minimum;
        }

        private bool threadCheckRunning = false;
        public void CheckForThread(bool CreateSculptThread = true, float Delay = 0.1f)
        {
            if (threadCheckRunning) return;
            StartCoroutine(CheckForThreadDelay(CreateSculptThread, Delay));
        }

        private IEnumerator CheckForThreadDelay(bool CreateSculptThread, float delay)
        {
            threadCheckRunning = true;
            yield return new WaitForSeconds(delay);
            if (CreateSculptThread)
            {
                if (Multithread == null)
                {
                    Multithread = new Thread(SS_Funct_DoSculpting_ThreadWorker);
                    Multithread.Start();
                }
                else
                {
                    Multithread.Abort();
                    Multithread = null;

                    yield return new WaitForEndOfFrame();

                    Multithread = new Thread(SS_Funct_DoSculpting_ThreadWorker);
                    Multithread.Start();
                }
            }
            else
            {
                if (Multithread != null)
                {
                    Multithread.Abort();
                    Multithread = null;
                }
            }
            threadCheckRunning = false;
        }

        private void OnApplicationQuit()
        {
            if (Multithread != null && Multithread.IsAlive)
                Multithread.Abort();
        }

        private void OnApplicationFocus(bool focus)
        {
            if (SS_MultithreadingSupported) CheckForThread();
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

        private void OnEnable()
        {
            if (SS_MultithreadingSupported) CheckForThread();
        }

        #endregion

        #region Additional Methods

        private bool controlsDown = false;
        public void SS_Funct_RecordOnControlsUp()
        {
            if (SS_UpdateColliderAfterRelease)
                SS_Funct_RefreshMeshCollider();
            if (controlsDown) controlsDown = false;
        }

        public void SS_Funct_RecordOnControlsDown()
        {
            if (controlsDown) return;
            controlsDown = true;
            if (SS_RecordHistory)
                SS_Funct_RecordToHistory();
        }

        //History Management

        /// <summary>
        /// Record current vertex positions to the history
        /// </summary>
        public void SS_Funct_RecordToHistory()
        {
            if (historyRecords.Count > SS_MaxHistoryRecords)
                historyRecords.RemoveAt(0);

            HistoryRecords h = new HistoryRecords() { historyNotes = "History " + historyRecords.Count.ToString(), VertexPositions = new Vector3[originVertices.Length] };
            System.Array.Copy( !SS_MultithreadingSupported ? workingVertices : threadVertices, h.VertexPositions, originVertices.Length);
            historyRecords.Add(h);
        }

        /// <summary>
        /// Make a step forward/backward in the history by the specified 'jumpToRecord' index. Type -1 for default = jump to the latest history record
        /// </summary>
        public void SS_Funct_Undo(int jumpToRecordIndex = -1)
        {
            if (historyRecords == null) return;
            if (historyRecords.Count == 0) return;

            int ind = jumpToRecordIndex == -1 ? historyRecords.Count - 1 : Mathf.Clamp(jumpToRecordIndex, 0, historyRecords.Count - 1);

            workingVertices = historyRecords[ind].VertexPositions;
            if (SS_MultithreadingSupported)
                System.Array.Copy(workingVertices, threadVertices, workingVertices.Length);
            meshFilter.sharedMesh.SetVertices(workingVertices);
            if (SS_AutoRecalculateBounds) meshFilter.sharedMesh.RecalculateBounds();
            if (SS_AutoRecalculateNormals) meshFilter.sharedMesh.RecalculateNormals();
            SS_Funct_RefreshMeshCollider();

            historyRecords.RemoveAt(ind);
        }

        /// <summary>
        /// Default undo method
        /// </summary>
        public void SS_Funct_Undo()
        {
            SS_Funct_Undo(-1);
        }

        //Mesh Collider Refresh & Bake

        private MeshCollider mc;
        /// <summary>
        /// Refresh mesh collider at runtime
        /// </summary>
        public void SS_Funct_RefreshMeshCollider()
        {
            if (!mc) mc = GetComponent<MeshCollider>();
            if (!mc) mc = gameObject.AddComponent<MeshCollider>();
            
            mc.sharedMesh = meshFilter.sharedMesh;
            if (SS_AutoRecalculateBounds) meshFilter.sharedMesh.RecalculateBounds();
            if (SS_AutoRecalculateNormals) meshFilter.sharedMesh.RecalculateNormals();
        }

        /// <summary>
        /// Reset mesh transform and matrix to One
        /// </summary>
        public void SS_Funct_BakeMesh()
        {
            Vector3[] vertsNew = meshFilter.sharedMesh.vertices;
            Vector3 lastPos = transform.position;
            Quaternion lastRot = transform.rotation;
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            for (int i = 0; i < vertsNew.Length; i++)
                vertsNew[i] = transform.TransformPoint(meshFilter.sharedMesh.vertices[i]);
            transform.localScale = Vector3.one;
            meshFilter.sharedMesh.vertices = vertsNew;
            if (SS_AutoRecalculateBounds) meshFilter.sharedMesh.RecalculateBounds();
            if (SS_AutoRecalculateNormals) meshFilter.sharedMesh.RecalculateNormals();

            transform.position = lastPos;
            transform.rotation = lastRot;
        }

        //Public available methods for essential parameters such as Size, Strength, Stylize Intensity, Smoothing Intensity etc

        /// <summary>
        /// Change brush size by float value
        /// </summary>
        public void SS_Funct_ChangeSize(float size)
        {
            SS_BrushSize = size;
        }
        /// <summary>
        /// Change brush size by Slider value
        /// </summary>
        /// <param name="size"></param>
        public void SS_Funct_ChangeSize(UnityEngine.UI.Slider size)
        {
            SS_BrushSize = size.value;
        }

        /// <summary>
        /// Change brush strength by float value
        /// </summary>
        public void SS_Funct_ChangeStrength(float strength)
        {
            SS_BrushStrength = strength;
        }
        /// <summary>
        /// Change brush strength by Slider value
        /// </summary>
        public void SS_Funct_ChangeStrength(UnityEngine.UI.Slider strength)
        {
            SS_BrushStrength = strength.value;
        }

        /// <summary>
        /// Change stylize intensity by float value
        /// </summary>
        public void SS_Funct_ChangeStylizeIntens(float intens)
        {
            SS_StylizeIntensity = intens;
        }
        /// <summary>
        /// Change stylize intensity by Slider value
        /// </summary>
        public void SS_Funct_ChangeStylizeIntens(UnityEngine.UI.Slider intens)
        {
            SS_StylizeIntensity = intens.value;
        }

        /// <summary>
        /// Change smooth intensity by float value
        /// </summary>
        public void SS_Funct_ChangeSmoothIntens(int intens)
        {
            SS_SmoothIntensity = intens;
        }
        /// <summary>
        /// Change smooth intensity by Slider value
        /// </summary>
        public void SS_Funct_ChangeSmoothIntens(UnityEngine.UI.Slider intens)
        {
            SS_SmoothIntensity = intens.value;
        }


        /// <summary>
        /// Change brush state by index value [0 = None, 1 = Raise, 2 = Lower, 3 = Revert etc...]
        /// </summary>
        public void SS_Funct_ChangeBrushState(int StateIndex)
        {
            SS_State = (SS_State_Internal)StateIndex;
        }

        /// <summary>
        /// Change basic sculpting values in one method
        /// </summary>
        public void SS_Funct_SetBasics(float Radius, float Strength, bool showBrush, Vector3 BrushPoint, Vector3 BrushDirection)
        {
            SS_BrushSize = Radius;
            SS_BrushStrength = Strength;
            SS_BrushProjection.transform.position = BrushPoint;
            SS_BrushProjection.transform.rotation = Quaternion.FromToRotation(-Vector3.forward, BrushDirection);
            SS_BrushProjection.transform.localScale = new Vector3(SS_BrushSize, SS_BrushSize, SS_BrushSize);
            if (SS_BrushProjection && SS_UseBrushProjection)
                    SS_BrushProjection.SetActive(showBrush);
        }

        #endregion

        #region VR Methods

        public void GlobalReceived_SetControlInput(bool entryInput)
        {
            bool prev = vrInputDown;
            vrInputDown = entryInput;
            if(prev != vrInputDown)
            {
                if (vrInputDown == false)
                {
                    SS_Funct_RecordOnControlsUp();
                    if (SS_MultithreadingSupported && Multithread != null && Multithread.IsAlive)
                        Multithread_ManualEvent.Reset();
                }
                else
                    SS_Funct_RecordOnControlsDown();
            }
        }

        #endregion
    }
}