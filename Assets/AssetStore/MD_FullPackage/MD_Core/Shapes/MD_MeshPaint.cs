using System.Collections.Generic;
using UnityEngine;

namespace MD_Plugin
{
    /// <summary>
    /// MD(Mesh Deformation) Component: Mesh Paint
    /// Full system for advanced mesh painting
    /// </summary>
    [AddComponentMenu(MD_Debug.ORGANISATION + MD_Debug.PACKAGENAME + "Mesh Paint")]
    public class MD_MeshPaint : MonoBehaviour
    {
        //MESH DATA
        [SerializeField] private List<Vector3> internal_vertices = new List<Vector3>();
        [SerializeField] private List<int> internal_triangles = new List<int>();
        [SerializeField] private List<Vector2> internal_uvs = new List<Vector2>();

        [SerializeField] private Transform internal_p1;
        [SerializeField] private Transform internal_p2;
        [SerializeField] private Transform internal_p3;
        [SerializeField] private Transform internal_p4;
        [SerializeField] private Transform internal_p5;
        [SerializeField] private Transform internal_p6;
        [SerializeField] private Transform internal_p7;
        [SerializeField] private Transform internal_p8;

        [SerializeField] private Transform internal_BrushHelper;
        [SerializeField] private Transform internal_BrushRoot;

        //PLATFORM
        public enum MP_PlatformInternal { PC, VR, Mobile };
        public MP_PlatformInternal MP_Platform = MP_PlatformInternal.PC;

        //INPUT
        //--PC
        public KeyCode MP_INPUT_PC_MeshPaintInput = KeyCode.Mouse0;

        //BRUSH & MESH SETTINGS
        public float MP_BrushSize = 0.5f;

        public bool MP_SmoothBrushMovement = true;
        public float MP_BSmoothMSpeed = 10f;
        public bool MP_SmoothBrushRotation = true;
        public float MP_BSmoothRSpeed = 20;

        public bool MP_DistanceLimitation = true;
        public float MP_MinDistanceLimit = 0.5f;

        public bool MP_ConnectMeshOnRelease = false;

        public enum MP_MeshPaintTypeInternal { DrawOnScreen, DrawOnRaycastHit, CustomDraw };
        public MP_MeshPaintTypeInternal MP_MeshPaintType;
        public enum MP_RotationModeInternal { FollowOneAxis, FollowSpatialAxis };
        public MP_RotationModeInternal MP_RotationMode = MP_RotationModeInternal.FollowOneAxis;
        public Vector3 MP_RotationmodeOffset = new Vector3(0, 0, -1);

        public enum MP_ShapeTypeInternal { Plane, Triangle, Cube};
        public MP_ShapeTypeInternal MP_ShapeType;

        //---Type - screen
        public bool MP_TypeScreen_UseMainCamera = true;
        public Camera MP_TypeScreen_TargetCamera;
        public float MP_TypeScreen_Depth = 10;
        //---Type - raycast
        public bool MP_TypeRaycast_RaycastFromCursor = true;
        public Transform MP_TypeRaycast_RaycastOriginFORWARD;
        public LayerMask MP_TypeRaycast_AllowedLayers = -1;
        public bool MP_TypeRaycast_CastAllObjects = true;
        public string MP_TypeRaycast_TagForRaycast;
        public Vector3 MP_TypeRaycast_BrushOffset = new Vector3(0, 1, 0);
        public bool MP_TypeRaycast_IgnoreSelfCasting = true;
        //---Type - custom
        public bool MP_TypeCustom_DRAW = false;
        private bool MP_TypeCustom_DRAWStart = false;
        public bool MP_TypeCustom_CustomBrushTransform = true;
        public bool MP_TypeCustom_EnableSmartRotation = true;
        public Transform MP_TypeCustom_BrushParent;

        //MESH APPEARANCE
        public int MP_CurrentlySelectedAppearanceSlot = 0;
        public bool MP_MaterialSlots = false;
        public Material[] MP_Color_AvailableMaterials;
        public Color[] MP_Color_AvailableColors = new Color[1] { Color.blue };

        public bool MP_FollowBrushTransform = false;
        public Transform MP_ObjectForFollowing;
        public bool MP_HideCustomBrushIfNotRaycasting = true;

        public bool MP_RefreshMeshCollider = true;

        private void Awake()
        {
            if (!internal_BrushHelper)
                internal_BrushHelper = new GameObject("MD_MESHPAINT_BrushHelper").transform;
            internal_BrushHelper.hideFlags = HideFlags.HideInHierarchy;
            if (!internal_BrushRoot)
                internal_BrushRoot = new GameObject("MD_MESHPAINT_BrushRoot").transform;
            internal_BrushRoot.hideFlags = HideFlags.HideInHierarchy;

            Vector3 vp1 = Vector3.zero, vp2 = Vector3.zero, vp3 = Vector3.zero, vp4 = Vector3.zero,
                vp5 = Vector3.zero, vp6 = Vector3.zero, vp7 = Vector3.zero, vp8 = Vector3.zero;

            switch(MP_ShapeType)
            {
                case MP_ShapeTypeInternal.Plane:
                    vp1 = new Vector3(0.5f, 0, 0);
                    vp2 = new Vector3(-0.5f, 0, 0);
                    vp3 = new Vector3(-0.5f, 0, 0.5f);
                    vp4 = new Vector3(0.5f, 0, 0.5f);
                    break;

                case MP_ShapeTypeInternal.Triangle:
                    vp1 = new Vector3(0.5f, -0.5f, 0);
                    vp2 = new Vector3(-0.5f, -0.5f, 0);
                    vp3 = new Vector3(0, 0.5f, 0);
                    break;

                case MP_ShapeTypeInternal.Cube:
                    vp1 = new Vector3(-0.5f, -0.5f, -0.5f);
                    vp2 = new Vector3(-0.5f, 0.5f, -0.5f);
                    vp3 = new Vector3(0.5f, 0.5f, -0.5f);
                    vp4 = new Vector3(0.5f, -0.5f, -0.5f);

                    vp5 = new Vector3(-0.5f, -0.5f, 0.5f);
                    vp6 = new Vector3(-0.5f, 0.5f, 0.5f);
                    vp7 = new Vector3(0.5f, 0.5f, 0.5f);
                    vp8 = new Vector3(0.5f, -0.5f, 0.5f);
                    break;
            }

            if (!internal_p1)
                internal_p1 = new GameObject("MD_MESHPAINT_P1").transform;
            internal_p1.parent = internal_BrushRoot;
            internal_p1.localPosition = vp1;
            if (!internal_p2)
                internal_p2 = new GameObject("MD_MESHPAINT_P2").transform;
            internal_p2.parent = internal_BrushRoot;
            internal_p2.localPosition = vp2;
            if (!internal_p3)
                internal_p3 = new GameObject("MD_MESHPAINT_P3").transform;
            internal_p3.parent = internal_BrushRoot;
            internal_p3.localPosition = vp3;

            if (MP_ShapeType == MP_ShapeTypeInternal.Plane)
            {
                if (!internal_p4)
                    internal_p4 = new GameObject("MD_MESHPAINT_P4").transform;
                internal_p4.parent = internal_BrushRoot;
                internal_p4.localPosition = vp4;
            }
            else if (MP_ShapeType == MP_ShapeTypeInternal.Cube)
            {
                if (!internal_p4)
                    internal_p4 = new GameObject("MD_MESHPAINT_P4").transform;
                internal_p4.parent = internal_BrushRoot;
                internal_p4.localPosition = vp4;
                if (!internal_p5)
                    internal_p5 = new GameObject("MD_MESHPAINT_P5").transform;
                internal_p5.parent = internal_BrushRoot;
                internal_p5.localPosition = vp5;
                if (!internal_p6)
                    internal_p6 = new GameObject("MD_MESHPAINT_P6").transform;
                internal_p6.parent = internal_BrushRoot;
                internal_p6.localPosition = vp6;
                if (!internal_p7)
                    internal_p7 = new GameObject("MD_MESHPAINT_P7").transform;
                internal_p7.parent = internal_BrushRoot;
                internal_p7.localPosition = vp7;
                if (!internal_p8)
                    internal_p8 = new GameObject("MD_MESHPAINT_P8").transform;
                internal_p8.parent = internal_BrushRoot;
                internal_p8.localPosition = vp8;
            }
            else
            {
                if (internal_p4)
                    Destroy(internal_p4.gameObject);
                if (internal_p5)
                    Destroy(internal_p5.gameObject);
                if (internal_p6)
                    Destroy(internal_p6.gameObject);
                if (internal_p7)
                    Destroy(internal_p7.gameObject);
                if (internal_p8)
                    Destroy(internal_p8.gameObject);
            }
        }

        private void Start()
        {
            if (MP_MaterialSlots && MP_Color_AvailableMaterials.Length == 0)
                MD_Debug.Debug(this, "At least one material must be assigned", MD_Debug.DebugType.Error);
            else if (!MP_MaterialSlots && MP_Color_AvailableColors.Length == 0)
                MD_Debug.Debug(this, "At least one color must be added", MD_Debug.DebugType.Error);

            if (MP_MeshPaintType == MP_MeshPaintTypeInternal.DrawOnScreen && MP_TypeScreen_UseMainCamera && Camera.main == null)
                MD_Debug.Debug(this, "Main Camera is null. Please choose one camera and change its tag to MainCamera.", MD_Debug.DebugType.Error);
            else if (MP_MeshPaintType == MP_MeshPaintTypeInternal.DrawOnScreen && !MP_TypeScreen_UseMainCamera && MP_TypeScreen_TargetCamera == null)
                MD_Debug.Debug(this, "Target camera is null.", MD_Debug.DebugType.Error);
        }

        GameObject internal_currentlyTargetMesh;
        Vector3 internal_ppplastposition;
        Vector3 internal_ppplastpress;
        Quaternion internal_ppplastrotation;
        private void Update()
        {
            switch (MP_MeshPaintType)
            {
                case MP_MeshPaintTypeInternal.DrawOnScreen:
                    INTERNAL_UPDATE_DrawOnScreen();
                    break;
                case MP_MeshPaintTypeInternal.DrawOnRaycastHit:
                    INTERNAL_UPDATE_DrawOnRaycast();
                    break;
                case MP_MeshPaintTypeInternal.CustomDraw:
                    INTERNAL_UPDATE_DrawOnCustom();
                    break;
            }

            if (MP_TypeCustom_DRAW)
            {
                if (!MP_TypeCustom_DRAWStart)
                    PUBLIC_PaintMesh(internal_BrushRoot.position, MeshPaintModeInternal.StartPaint);
                else
                    PUBLIC_PaintMesh(internal_BrushRoot.position, MeshPaintModeInternal.Painting);
            }
            else
            {
                if (MP_TypeCustom_DRAWStart)
                    PUBLIC_PaintMesh(internal_BrushRoot.position, MeshPaintModeInternal.EndPaint);
            }

            internal_ppplastposition = internal_BrushHelper.transform.position;

            if (MP_MeshPaintType == MP_MeshPaintTypeInternal.CustomDraw)
            {
                if (MP_TypeCustom_CustomBrushTransform && MP_TypeCustom_BrushParent)
                    internal_BrushHelper.position = MP_TypeCustom_BrushParent.position;
                else
                    internal_BrushHelper.position = transform.position;
            }

            if (MP_FollowBrushTransform)
            {
                if (!MP_ObjectForFollowing)
                    return;
                else
                {
                    MP_ObjectForFollowing.position = internal_BrushRoot.position;
                    MP_ObjectForFollowing.rotation = internal_BrushRoot.rotation;
                    MP_ObjectForFollowing.localScale = Vector3.one * MP_BrushSize;
                }
            }
        }

        #region INTERNAL FUNCTIONS

        //---TYPE _ SCREEN
        private void INTERNAL_UPDATE_DrawOnScreen()
        {
            Vector3 location = INTERNAL_GetScreenPosition();
            Vector3 rotationdirection = internal_BrushHelper.InverseTransformDirection(location - internal_ppplastposition);
            if (rotationdirection != Vector3.zero)
            {
                if (MP_RotationMode == MP_RotationModeInternal.FollowOneAxis)
                    internal_ppplastrotation = Quaternion.LookRotation(rotationdirection, MP_RotationmodeOffset);
                else
                    internal_ppplastrotation = Quaternion.LookRotation(rotationdirection);
            }

            internal_BrushHelper.position = location;

            if (MP_SmoothBrushMovement)
                internal_BrushRoot.position = Vector3.Lerp(internal_BrushRoot.position, internal_BrushHelper.position, Time.deltaTime * MP_BSmoothMSpeed);
            else
                internal_BrushRoot.position = internal_BrushHelper.position;

            if (MP_SmoothBrushRotation)
                internal_BrushRoot.rotation = Quaternion.Lerp(internal_BrushRoot.rotation, internal_ppplastrotation, Time.deltaTime * MP_BSmoothRSpeed);
            else
                internal_BrushRoot.rotation = internal_ppplastrotation;

            if (!MP_TypeCustom_DRAW)
            {
                if (INTERNAL_GetInput(false))
                    MP_TypeCustom_DRAW = true;
            }
            else
            {
                if (INTERNAL_GetInput(true))
                    MP_TypeCustom_DRAW = false;
            }
        }
        private Vector3 INTERNAL_GetScreenPosition()
        {
            Vector3 p = Input.mousePosition;
            p.z = MP_TypeScreen_Depth;
            if (MP_TypeScreen_UseMainCamera)
                MP_TypeScreen_TargetCamera = Camera.main;

            p = MP_TypeScreen_TargetCamera.ScreenToWorldPoint(p);
            return p;
        }

        //---TYPE _ RAYCAST
        private void INTERNAL_UPDATE_DrawOnRaycast()
        {
            Vector3 location = INTERNAL_GetRaycastPosition();
            location += MP_TypeRaycast_BrushOffset;

            Vector3 rotationdirection = internal_BrushHelper.InverseTransformDirection(location - internal_ppplastposition);

            internal_BrushHelper.position = location;

            if (rotationdirection != Vector3.zero)
            {
                if (MP_RotationMode == MP_RotationModeInternal.FollowOneAxis)
                    internal_ppplastrotation = Quaternion.LookRotation(rotationdirection, MP_RotationmodeOffset);
                else
                    internal_ppplastrotation = Quaternion.LookRotation(rotationdirection);
            }

            if (MP_SmoothBrushMovement)
                internal_BrushRoot.position = Vector3.Lerp(internal_BrushRoot.position, internal_BrushHelper.position, Time.deltaTime * MP_BSmoothMSpeed);
            else
                internal_BrushRoot.position = internal_BrushHelper.position;

            if (MP_SmoothBrushRotation)
                internal_BrushRoot.rotation = Quaternion.Lerp(internal_BrushRoot.rotation, internal_ppplastrotation, Time.deltaTime * MP_BSmoothRSpeed);
            else
                internal_BrushRoot.rotation = internal_ppplastrotation;

            if (location == Vector3.zero)
                return;

            if (!MP_TypeCustom_DRAW)
            {
                if (INTERNAL_GetInput())
                    MP_TypeCustom_DRAW = true;
            }
            else
            {
                if (INTERNAL_GetInput(true))
                    MP_TypeCustom_DRAW = false;
            }

        }
        private Vector3 INTERNAL_GetRaycastPosition()
        {
            Camera c = null;
            if (MP_TypeScreen_UseMainCamera)
                c = Camera.main;
            else
                c = MP_TypeScreen_TargetCamera;

            Vector3 result = Vector3.zero;
            Ray r = new Ray();
            if (MP_TypeRaycast_RaycastFromCursor)
                r = c.ScreenPointToRay(Input.mousePosition);
            else
                r = new Ray(MP_TypeRaycast_RaycastOriginFORWARD.position, MP_TypeRaycast_RaycastOriginFORWARD.forward);

            RaycastHit hit = new RaycastHit();

            if (MP_FollowBrushTransform)
                MP_ObjectForFollowing.gameObject.SetActive(true);

            if (Physics.Raycast(r, out hit, Mathf.Infinity, MP_TypeRaycast_AllowedLayers))
            {
                if (MP_TypeRaycast_CastAllObjects)
                {
                    if (hit.collider)
                        return hit.point;
                }
                else
                {
                    if (hit.collider.tag == MP_TypeRaycast_TagForRaycast)
                        return hit.point;
                    else if (MP_FollowBrushTransform && MP_HideCustomBrushIfNotRaycasting)
                    {
                        MP_ObjectForFollowing.gameObject.SetActive(false);
                        MP_TypeCustom_DRAW = false;
                    }
                    else
                        MP_TypeCustom_DRAW = false;
                }
            }
            else if (MP_FollowBrushTransform && MP_HideCustomBrushIfNotRaycasting)
            {
                MP_ObjectForFollowing.gameObject.SetActive(false);
                MP_TypeCustom_DRAW = false;
            }
            else
                MP_TypeCustom_DRAW = false;

            return Vector3.zero;
        }

        //---TYPE _ CUSTOM
        private void INTERNAL_UPDATE_DrawOnCustom()
        {
            if (!MP_TypeCustom_CustomBrushTransform)
                return;

            Vector3 rotationdirection = internal_BrushHelper.InverseTransformDirection((MP_TypeCustom_BrushParent == null ? transform.position : MP_TypeCustom_BrushParent.position) - internal_ppplastposition);
            if (rotationdirection != Vector3.zero && MP_TypeCustom_EnableSmartRotation)
            {
                if (MP_RotationMode == MP_RotationModeInternal.FollowOneAxis)
                    internal_ppplastrotation = Quaternion.FromToRotation(Vector3.forward, rotationdirection);
                else
                    internal_ppplastrotation = Quaternion.LookRotation(rotationdirection);
            }

            if (MP_SmoothBrushMovement)
                internal_BrushRoot.position = Vector3.Lerp(internal_BrushRoot.position, internal_BrushHelper.position, Time.deltaTime * MP_BSmoothMSpeed);
            else
                internal_BrushRoot.position = internal_BrushHelper.position;

            if (MP_SmoothBrushRotation)
                internal_BrushRoot.rotation = Quaternion.Lerp(internal_BrushRoot.rotation, internal_ppplastrotation, Time.deltaTime * MP_BSmoothRSpeed);
            else
                internal_BrushRoot.rotation = internal_ppplastrotation;
        }


        //-Input and others
        private bool INTERNAL_GetInput(bool Up = false)
        {
            switch (MP_Platform)
            {
                case MP_PlatformInternal.PC:
                    if (!Up)
                        return Input.GetKeyDown(MP_INPUT_PC_MeshPaintInput);
                    else
                        return Input.GetKeyUp(MP_INPUT_PC_MeshPaintInput);

                case MP_PlatformInternal.Mobile:
                    if (!Up && Input.touchCount > 0)
                        return true;
                    else if (Up && Input.touchCount == 0)
                        return true;
                    else
                        return false;

                default:
                    return false;
            }
        }

        private void INTERNAL_ChangeBrushSize(float size)
        {
            if (MP_ShapeType == MP_ShapeTypeInternal.Triangle)
            {
                internal_p1.transform.localPosition = new Vector3(size, -size, 0);
                internal_p2.transform.localPosition = new Vector3(-size, -size, 0);
                internal_p3.transform.localPosition = new Vector3(0, size, 0);
            }
            else if (MP_ShapeType == MP_ShapeTypeInternal.Plane)
            {
                internal_p1.transform.localPosition = new Vector3(size, 0, -size);
                internal_p2.transform.localPosition = new Vector3(-size, 0, -size);
                internal_p3.transform.localPosition = new Vector3(-size, 0, size);
                internal_p4.transform.localPosition = new Vector3(size, 0, size);
            }
            else if (MP_ShapeType == MP_ShapeTypeInternal.Cube)
            {
                internal_p1.transform.localPosition = new Vector3(-size, -size, -size);
                internal_p2.transform.localPosition = new Vector3(-size, size, -size);
                internal_p3.transform.localPosition = new Vector3(size, size, -size);
                internal_p4.transform.localPosition = new Vector3(size, -size, -size);

                internal_p5.transform.localPosition = new Vector3(-size, -size, size);
                internal_p6.transform.localPosition = new Vector3(-size, size, size);
                internal_p7.transform.localPosition = new Vector3(size, size, size);
                internal_p8.transform.localPosition = new Vector3(size, -size, size);
            }
        }

        private void INTERNAL_Generation_Triangle(Vector3 Position, MeshPaintModeInternal MeshPaintMode)
        {
            Vector3[] newVertArray = new Vector3[] { internal_p1.position, internal_p2.position, internal_p3.position };

            if (MeshPaintMode == MeshPaintModeInternal.EndPaint)
                newVertArray = new Vector3[] { internal_vertices[internal_vertices.Count - 3], internal_vertices[internal_vertices.Count - 2], internal_vertices[internal_vertices.Count - 1] };

            int last = 0;
            if (internal_triangles.Count > 0)
                last = internal_vertices.Count-1;
            int[] newTrinArray = new int[] { };

            if (MeshPaintMode == MeshPaintModeInternal.StartPaint)
            {
                internal_ppplastpress = Position;
                PUBLIC_CreateNewPaintPattern("Paint_TargetMesh", MP_RefreshMeshCollider);

                internal_vertices.Clear();
                internal_triangles.Clear();
                internal_uvs.Clear();

                newTrinArray = new int[]
                 {
                0,1,2
                 };

                MP_TypeCustom_DRAWStart = true;
            }
            else if (MeshPaintMode == MeshPaintModeInternal.Painting)
            {
                internal_ppplastpress = Position;
                newTrinArray = new int[]
                {
                    //----Left-Down
                    last-1,last+2, last+3,
                     //----Left-Up
                    last+3,last, last-1,

                     //----Right-Down
                    last+1,last-2, last,
                     //----Right-Up
                    last, last+3, last+1,

                    //----Down-Right-Down
                    last+1,last+2, last-1,
                     //----Down-Left-Down
                    last-1, last-2, last+1,
                };
            }
            else if (MeshPaintMode == MeshPaintModeInternal.EndPaint)
            {
                if (!MP_ConnectMeshOnRelease)
                {
                    newTrinArray = new int[]
                    {
                    //----New Front Side
                    last+3, last+2, last+1,
                    };
                }
                else
                {
                    newTrinArray = new int[]
                    {
                        //----Left-Down
                        last-1, 1, 2,
                         //----Left-Up
                        last-1, 2, last,

                         //----Right-Down
                        0, last-2, last,
                         //----Right-Up
                        0, last, 2,

                        //----Down-Right-Down
                        0, 1, last-1,
                         //----Down-Left-Down
                        0, last-1, last-2,
                    };
                }

                MP_TypeCustom_DRAWStart = false;

                if (MP_TypeRaycast_IgnoreSelfCasting)
                    internal_currentlyTargetMesh.layer = 2;
                else
                    internal_currentlyTargetMesh.layer = 0;
            }

            if(newVertArray != null)
                internal_vertices.AddRange(newVertArray);

            internal_triangles.AddRange(newTrinArray);
            internal_uvs.AddRange(new List<Vector2> { new Vector2(0.5f, -0.5f), new Vector2(-0.5f, -0.5f), new Vector2(0, 0.5f) });
        }

        int UVFixer;
        private void INTERNAL_Generation_Plane(Vector3 Position, MeshPaintModeInternal MeshPaintMode)
        {
            Vector3[] newVertArray = null;
            if(MeshPaintMode == MeshPaintModeInternal.StartPaint)
                newVertArray = new Vector3[] { internal_p1.position, internal_p2.position, internal_p3.position, internal_p4.position };
            else if(MeshPaintMode == MeshPaintModeInternal.Painting)
                newVertArray = new Vector3[] { internal_p3.position, internal_p4.position };

            int last = 0;
            if (internal_triangles.Count > 0)
                last = internal_vertices.Count-1;
            int[] newTrinArray = new int[] { };

            if (MeshPaintMode == MeshPaintModeInternal.StartPaint)
            {
                UVFixer = 0;
                internal_ppplastpress = Position;
                PUBLIC_CreateNewPaintPattern("Paint_TargetMesh", MP_RefreshMeshCollider);

                internal_vertices.Clear();
                internal_triangles.Clear();
                internal_uvs.Clear();

                newTrinArray = new int[]
                 {
                    0,1,2,
                    0,2,3
                 };

                MP_TypeCustom_DRAWStart = true;
            }
            else if (MeshPaintMode == MeshPaintModeInternal.Painting)
            {
                if (UVFixer == 0)
                    UVFixer = 1;
                else
                    UVFixer = 0;
                internal_ppplastpress = Position;
                newTrinArray = new int[]
                {
                    //----Right
                    last, last-1, last+2,
                     //----Left
                    last-1,last+1, last+2,

                };
            }
            else if (MeshPaintMode == MeshPaintModeInternal.EndPaint)
            {
                if(MP_ConnectMeshOnRelease)
                {
                    newTrinArray = new int[]
                    {
                        //----Right
                        last, last-1, 1,
                         //----Left
                        last,1, 0,
                     };
                }

                MP_TypeCustom_DRAWStart = false;

                if (MP_TypeRaycast_IgnoreSelfCasting)
                    internal_currentlyTargetMesh.layer = 2;
                else
                    internal_currentlyTargetMesh.layer = 0;
            }

            if (newVertArray != null)
                internal_vertices.AddRange(newVertArray);

            internal_triangles.AddRange(newTrinArray);
            if (MeshPaintMode == MeshPaintModeInternal.StartPaint)
                internal_uvs.AddRange(new List<Vector2> { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) });
            else if (MeshPaintMode == MeshPaintModeInternal.Painting)
            {
                if (UVFixer == 0)
                    internal_uvs.AddRange(new List<Vector2> { new Vector2(0, 0), new Vector2(1, 0) });
                else
                    internal_uvs.AddRange(new List<Vector2> { new Vector2(0, 1), new Vector2(1, 1) });
            }
        }

        private void INTERNAL_Generation_Cube(Vector3 Position, MeshPaintModeInternal MeshPaintMode)
        {
            Vector3[] newVertArray = null;

            int last = 0;
            if (internal_triangles.Count > 0)
                last = internal_vertices.Count-1;
            int[] newTrinArray = new int[] { };

            if (MeshPaintMode == MeshPaintModeInternal.StartPaint)
            {
                newVertArray = new Vector3[] { internal_p1.position, internal_p2.position, internal_p3.position, internal_p4.position,
                 internal_p5.position, internal_p6.position, internal_p7.position, internal_p8.position};

                UVFixer = 0;
                internal_ppplastpress = Position;
                PUBLIC_CreateNewPaintPattern("Paint_TargetMesh", MP_RefreshMeshCollider);

                internal_vertices.Clear();
                internal_triangles.Clear();
                internal_uvs.Clear();

                newTrinArray = new int[]
                 {
                    //---Back Side
                    3,0,1,
                    3,1,2,

                    //---Front Side
                    4,7,6,
                    4,6,5,

                    //---Right Side
                    7,3,2,
                    7,2,6,

                    //---Left Side
                    0,4,5,
                    0,5,1,

                    //---Upper Side
                    2,1,5,
                    2,5,6,

                    //---Lower Side
                    0,3,7,
                    0,7,4
                 };

                MP_TypeCustom_DRAWStart = true;
            }
            else if (MeshPaintMode == MeshPaintModeInternal.Painting)
            {
                newVertArray = new Vector3[] {internal_p5.position, internal_p6.position, internal_p7.position, internal_p8.position};

                if (UVFixer == 0)
                    UVFixer = 1;
                else
                    UVFixer = 0;
                internal_ppplastpress = Position;
                newTrinArray = new int[]
                {
                    //---Right Side
                    last+4,last,last-1,
                    last+4,last-1,last+3,

                    //---Left Side
                    last-3,last+1,last+2,
                    last-3,last+2,last-2,

                    //---Upper Side
                    last-1,last-2,last+2,
                    last-1,last+2,last+3,

                    //---Lower Side
                    last-3,last,last+4,
                    last-3,last+4,last+1
                };
            }
            else if (MeshPaintMode == MeshPaintModeInternal.EndPaint)
            {
                if (MP_ConnectMeshOnRelease)
                {
                    newTrinArray = new int[]
                    {
                        //---Right Side
                        3,last,last-1,
                        3,last-1,2,

                        //---Left Side
                        last-3,0,1,
                        last-3,1,last-2,

                        //---Upper Side
                        last-1,last-2,1,
                        last-1,1,2,

                        //---Lower Side
                        3,last-3,last,
                        3,0,last-3
                    };
                }
                else
                {
                    newTrinArray = new int[]
                     {
                    //---Last Front Side
                    last-3,last,last-1,
                    last-3,last-1,last-2,
                     };
                }

                MP_TypeCustom_DRAWStart = false;

                if (MP_TypeRaycast_IgnoreSelfCasting)
                    internal_currentlyTargetMesh.layer = 2;
                else
                    internal_currentlyTargetMesh.layer = 0;
            }

            if(newVertArray != null)
                internal_vertices.AddRange(newVertArray);

            internal_triangles.AddRange(newTrinArray);

            if (MeshPaintMode == MeshPaintModeInternal.StartPaint)
                internal_uvs.AddRange(new List<Vector2> { new Vector2(-0.4f,0.4f), new Vector2(0,0.2f), new Vector2(-0.2f,-0.4f), new Vector2(-0.4f,-0.4f),
                new Vector2(0.4f,0.4f),new Vector2(-0.2f,0),new Vector2(0.2f,-0.4f),new Vector2(-0.2f,0)});
            else if (MeshPaintMode == MeshPaintModeInternal.Painting)
            {
                if(UVFixer == 0)
                    internal_uvs.AddRange(new List<Vector2> { new Vector2(-0.4f, 0.4f), new Vector2(0, 0.4f), new Vector2(0, -0.4f), new Vector2(-0.4f, -0.4f) });
                else
                    internal_uvs.AddRange(new List<Vector2> { new Vector2(0.2f, 0.2f), new Vector2(0.4f, 0.2f), new Vector2(0.4f, -0.5f), new Vector2(0.2f, -0.4f) });
            }
        }

        #endregion

        public enum MeshPaintModeInternal { StartPaint, Painting, EndPaint };
        public MeshPaintModeInternal MeshPaintMode;

        #region PUBLIC FUNCTIONS

        /// <summary>
        /// Create new painting pattern such as New Mesh Filter & make it ready for painting
        /// </summary>
        public void PUBLIC_CreateNewPaintPattern(string MeshName = "TargetMesh", bool addCollider = true)
        {
            GameObject Entry = new GameObject(MeshName);
            Entry.AddComponent<MeshFilter>();
            Entry.AddComponent<MeshRenderer>();
            Mesh m = new Mesh();
            Entry.GetComponent<MeshFilter>().mesh = m;
            if (MP_MaterialSlots)
                Entry.GetComponent<Renderer>().material = MP_Color_AvailableMaterials[MP_CurrentlySelectedAppearanceSlot];
            else
            {
                Material mat = new Material(Shader.Find("Diffuse"));
                mat.color = MP_Color_AvailableColors[MP_CurrentlySelectedAppearanceSlot];
                Entry.GetComponent<Renderer>().material = mat;
            }
            if (addCollider)
            {
                Entry.AddComponent<MeshCollider>();
                Entry.layer = 2;
            }
            internal_currentlyTargetMesh = Entry;
        }

        /// <summary>
        /// Paint mesh on the specific location by the selected method
        /// </summary>
        public void PUBLIC_PaintMesh(Vector3 Position, MeshPaintModeInternal MeshPaintMode)
        {
            if (MP_DistanceLimitation)
            {
                if (Vector3.Distance(Position, internal_ppplastpress) < MP_MinDistanceLimit)
                    return;
            }

            if (UnityEngine.EventSystems.EventSystem.current && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                return;

            INTERNAL_ChangeBrushSize(MP_BrushSize);

            if (MP_ShapeType == MP_ShapeTypeInternal.Triangle)
                INTERNAL_Generation_Triangle(Position, MeshPaintMode);
            else if (MP_ShapeType == MP_ShapeTypeInternal.Plane)
                INTERNAL_Generation_Plane(Position, MeshPaintMode);
            else if (MP_ShapeType == MP_ShapeTypeInternal.Cube)
                INTERNAL_Generation_Cube(Position, MeshPaintMode);

            MeshFilter Meshf = internal_currentlyTargetMesh.GetComponent<MeshFilter>();
            Meshf.mesh.vertices = internal_vertices.ToArray();
            Meshf.mesh.triangles = internal_triangles.ToArray();
            Meshf.mesh.uv = internal_uvs.ToArray();
            Mesh m = Meshf.mesh;

            if (MP_RefreshMeshCollider)
                internal_currentlyTargetMesh.GetComponent<MeshCollider>().sharedMesh = m;

            Meshf.mesh.RecalculateNormals();
            Meshf.mesh.RecalculateBounds();
        }

        /// <summary>
        /// Change brush size manually
        /// </summary>
        public void PUBLIC_ChangeBrushSize(float size)
        {
            MP_BrushSize = size;
        }

        /// <summary>
        /// Increase brush size manually
        /// </summary>
        public void PUBLIC_IncreaseBrushSize(float size)
        {
            MP_BrushSize += size;
        }

        /// <summary>
        /// Decrease brush size manually
        /// </summary>
        public void PUBLIC_DecreaseBrushSize(float size)
        {
            MP_BrushSize -= size;
        }

        /// <summary>
        /// Change brush size manually by UI Slider
        /// </summary>
        public void PUBLIC_ChangeBrushSize(UnityEngine.UI.Slider size)
        {
            MP_BrushSize = size.value;
        }

        /// <summary>
        /// Enable/ Disable drawing externally
        /// </summary>
        public void PUBLIC_EnableDisableDrawing(bool activation)
        {
            MP_TypeCustom_DRAW = activation;
        }

        /// <summary>
        /// Change currently selected material/color by index
        /// </summary>
        public void PUBLIC_ChangeAppearanceIndex(int index)
        {
            MP_CurrentlySelectedAppearanceSlot = index;
        }

        /// <summary>
        /// Change shape type [0 = Plane, 1 = Triangle...]
        /// </summary>
        public void PUBLIC_ChangeShapeType(int ShapeT)
        {
            MP_ShapeType = (MP_ShapeTypeInternal)ShapeT;
            Awake();
        }


        /// <summary>
        /// Set control input from 3rd party source (such as SteamVR, Oculus or other)
        /// </summary>
        /// <param name="setInputTo">Input down or up?</param>
        public void GlobalReceived_SetControlInput(bool setInputTo)
        {
            MP_TypeCustom_DRAW = setInputTo;
        }

        #endregion

    }
}
