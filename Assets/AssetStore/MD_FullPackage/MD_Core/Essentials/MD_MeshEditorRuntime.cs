using System.Collections.Generic;
using UnityEngine;

namespace MD_Plugin
{
    /// <summary>
    /// MD(Mesh Deformation) Essential Component: Mesh Editor Runtime
    /// Essential component for general mesh-vertex-editing at runtime [Non-VR]
    /// </summary>
    [AddComponentMenu(MD_Debug.ORGANISATION + MD_Debug.PACKAGENAME + "Mesh Editor Runtime")]
    public class MD_MeshEditorRuntime : MonoBehaviour
    {
        public bool ppAXIS_EDITOR = false;

        // NON-AXIS EDITOR --- PARAMS

        public enum VertexControlMode {GrabDropVertex, PushVertex, PullVertex };
        public VertexControlMode ppVertexControlMode = VertexControlMode.GrabDropVertex;

        public bool ppMobileSupported = false;

        // Axis lock
        public bool ppLockAxis_X = false;
        public bool ppLockAxis_Y = false;

        // Input
        public KeyCode ppPCInput = KeyCode.Mouse0;

        // Cursor
        public bool ppCursorIsOrigin = true;
        public bool ppLockAndHideCursor = true;

        // Appearance
        public bool ppSwitchAppearance = true;
        public Color ppToColor = Color.green;
        public bool ppCustomMaterial = false;
        public Material ppTargetMaterial;

        // Pull-Push Settings
        public float ppPullPushVertexSpeed = 0.15f;
        public float ppMaxMinPullPushDistance = Mathf.Infinity;
        public bool ppContinuousPullPushDetection = false;
        public enum PullPushType { Radial, Directional };
        public PullPushType ppPullPushType = PullPushType.Directional;

        // Conditions
        public bool ppAllowSpecificPoints = false;
        public string ppAllowedTag;

        // Raycast
        public bool ppAllowBackfaces = true;
        public LayerMask ppAllowedLayerMask = -1;
        public float ppRaycastDistance = 1000.0f;
        public float ppRaycastRadius = 0.25f;

        // DEBUG
        public bool ppShowDebug = true;
        public bool INPUT_DOWN = false;

        private struct PotentialPoints
        {
            public Transform parent;
            public Transform point;
            public Material material;
            public Color originalCol;
        }
        private List<PotentialPoints> potentialPoints = new List<PotentialPoints>();

        //----------------------------------------------------------------

        // AXIS EDITOR --- PARAMS
        public GameObject ppAXIS_AxisObject;

        public MD_MeshProEditor ppAXIS_TargetObject;

        public KeyCode ppAXIS_SelectionInput = KeyCode.Mouse0;
        public KeyCode ppAXIS_AddPointsInput = KeyCode.LeftShift;
        public KeyCode ppAXIS_RemovePointsInput = KeyCode.LeftAlt;

        public bool ppAXIS_LocalSpace = false;
        public float ppAXIS_Speed = 4;
        private Color ppAXIS_StoragePointColor;
        public Color ppAXIS_SelectedPointColor = Color.green;
        public Color ppAXIS_SelectionGridColor = Color.black;

        private bool ppAXIS_Selecting = false;
        private bool ppAXIS_Moving = false;

        public enum AXIS_MovingTo {X, Y, Z};
        public AXIS_MovingTo ppAXIS_MovingTo;
        private Vector3 ppAXIS_CursorPosOrigin;
        private List<Transform> ppAXIS_TotalPoints = new List<Transform>();
        private List<Transform> ppAXIS_SelectedPoints = new List<Transform>();
        private GameObject ppAXIS_GroupSelector;
        private List<Transform> ppAXIS_UndoStoredObjects = new List<Transform>();

        public Camera ppMainCam;
        private Vector3 ppCenterPoint;
        private Transform ppSelectionHelper; //This object saves a lot of math. God bless this object
        //----------------------------------------------------------------

        private void Start () 
        {
            if (ppMainCam == null)
                ppMainCam = Camera.main != null ? Camera.main : GetComponent<Camera>() != null ? GetComponent<Camera>() : null;
            if(ppMainCam == null)
            {
                MD_Debug.Debug(this, "Main Camera is missing!", MD_Debug.DebugType.Error);
                return;
            }

            if (ppAXIS_EDITOR)
            {
                if(ppAXIS_TargetObject == null)
                {
                    MD_Debug.Debug(this, "Target object is empty! Script was disabled.", MD_Debug.DebugType.Error);
                    enabled = false;
                    return;
                }
                ppAXIS_AxisObject.SetActive(false);
                AXIS_SwitchTarget(ppAXIS_TargetObject);
                return;
            }

            ppSelectionHelper = new GameObject("MeshEditorRuntimeHelper").transform;
        }

        private void Update () 
        {
            if (!ppAXIS_EDITOR)
                InternalProcess_NonAxisEditor();
            else
                InternalProcess_AxisEditor();
        }

        private void OnDrawGizmos()
        {
            if (!ppShowDebug) return;
            if (ppAXIS_EDITOR) return;
            if (ppCursorIsOrigin) return;
            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * ppRaycastDistance);
            Gizmos.DrawWireSphere(transform.position + transform.forward * ppRaycastDistance, ppRaycastRadius);
        }

        private void InternalProcess_NonAxisEditor()
        {
            if (!ppCursorIsOrigin && ppLockAndHideCursor && !ppMobileSupported)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            //If input is pressed/down, process the runtime editor
            if (INPUT_DOWN && potentialPoints.Count > 0)
            {
                if (ppVertexControlMode != VertexControlMode.GrabDropVertex)
                {
                    InternalProcess_ProcessPullPush();
                    if (ppContinuousPullPushDetection) INPUT_DOWN = false;
                }
                else
                {
                    Vector3 origPos = ppSelectionHelper.position;
                    Vector3 futurePos = ppMainCam.ScreenToWorldPoint(new Vector3(
                        ppLockAxis_X ? ppMainCam.WorldToScreenPoint(origPos).x : Input.mousePosition.x,
                        ppLockAxis_Y ? ppMainCam.WorldToScreenPoint(origPos).y : Input.mousePosition.y, 
                        (ppMainCam.transform.position - ppCenterPoint).magnitude));
                    ppSelectionHelper.position = futurePos;
                }
                //Check for input-UP
                if (!Internal_GetControlInput())
                {
                    if (ppVertexControlMode == VertexControlMode.GrabDropVertex)
                        foreach (PotentialPoints tr in potentialPoints)
                            tr.point.parent = tr.parent;
                    INPUT_DOWN = false;
                }
                if (INPUT_DOWN) return;
            }

            if (!ppSwitchAppearance && !Internal_GetControlInput()) return;

            Ray ray = new Ray();

            if (!ppMobileSupported)
            {
                if (ppCursorIsOrigin)
                    ray = ppMainCam.ScreenPointToRay(Input.mousePosition);
                else
                    ray = new Ray(transform.position, transform.forward);
            }
            else
            {
                if (Input.touchCount > 0)
                    ray = ppMainCam.ScreenPointToRay(Input.GetTouch(0).position);
            }

            //If input is up, raycast for potential points in sphere radius
            RaycastHit[] raycast = Physics.SphereCastAll(ray, ppRaycastRadius, ppRaycastDistance, ppAllowedLayerMask);

            //Reset a potential points list
            if (potentialPoints.Count > 0)
            {
                if (ppSwitchAppearance)
                    foreach (PotentialPoints tr in potentialPoints)
                        InternalProcess_ChangeMaterialToPoints(tr, false);
                potentialPoints.Clear();
            }

            if (raycast.Length == 0) return;

            //Declare a new potential points chain
            foreach (RaycastHit h in raycast)
            {
                if (!h.transform.GetComponentInParent<MD_MeshProEditor>())
                    continue;
                if (ppAllowSpecificPoints && !h.transform.CompareTag(ppAllowedTag))
                    continue;
                if (!ppAllowBackfaces && !h.transform.gameObject.GetComponent<Renderer>().isVisible)
                    continue;
                Renderer r = h.transform.gameObject.GetComponent<Renderer>();
                PotentialPoints ppp = new PotentialPoints() { point = h.transform, parent = h.transform.parent, material = r.material, originalCol = r.material.color };
                potentialPoints.Add(ppp);
                InternalProcess_ChangeMaterialToPoints(ppp, true);
            }

            //Manage final control_down = if pressed, process the runtime editor next frame
            if (Internal_GetControlInput())
            {
                //Getting the center point of all vectors
                Vector3 center = new Vector3(0, 0, 0);
                foreach (PotentialPoints tr in potentialPoints)
                    center += tr.point.position;
                ppCenterPoint = center / potentialPoints.Count;

                ppSelectionHelper.position = ppMainCam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, (ppMainCam.transform.position - ppCenterPoint).magnitude));
                foreach (PotentialPoints tr in potentialPoints)
                {
                    if(ppVertexControlMode == VertexControlMode.GrabDropVertex)
                        tr.point.parent = ppSelectionHelper;
                    InternalProcess_ChangeMaterialToPoints(tr, false);
                }
                INPUT_DOWN = true;
            }
        }

        private void InternalProcess_ChangeMaterialToPoints(PotentialPoints p, bool selected)
        {
            if (!ppSwitchAppearance)
                return;

            if (selected)
            {
                if (ppCustomMaterial)
                    p.point.GetComponent<Renderer>().material = ppTargetMaterial;
                else
                    p.point.GetComponent<Renderer>().material.color = ppToColor;
            }
            else
            {
                if (ppCustomMaterial)
                    p.point.GetComponent<Renderer>().material = p.material;
                else
                    p.point.GetComponent<Renderer>().material.color = p.originalCol;
            }
        }

        private void InternalProcess_ProcessPullPush()
        {
            foreach (PotentialPoints tr in potentialPoints)
            {
                Vector3 tvector = ppPullPushType == PullPushType.Radial ? (tr.point.position - ppCenterPoint) : transform.forward;
                float dist = (tr.point.position - ppCenterPoint).magnitude;
                if (ppVertexControlMode == VertexControlMode.PushVertex && dist > ppMaxMinPullPushDistance)
                    continue;
                if (ppVertexControlMode == VertexControlMode.PullVertex && dist < ppMaxMinPullPushDistance && ppMaxMinPullPushDistance != Mathf.Infinity)
                    continue;
                tr.point.position += (ppVertexControlMode == VertexControlMode.PushVertex ? tvector : -tvector) * ppPullPushVertexSpeed * Time.deltaTime;
            }
        }


        private void InternalProcess_AxisEditor()
        {
            //---BEFORE SELECTION
            if (Input.GetKeyDown(ppAXIS_SelectionInput))
            {
                Ray ray = ppMainCam.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if(Physics.Raycast(ray,out hit))
                {
                    bool hitt = true;
                    switch(hit.collider.name)
                    {
                        case "AXIS_X":
                            ppAXIS_Moving = true;
                            ppAXIS_MovingTo = AXIS_MovingTo.X;
                            return;
                        case "AXIS_Y":
                            ppAXIS_Moving = true;
                            ppAXIS_MovingTo = AXIS_MovingTo.Y;
                            return;
                        case "AXIS_Z":
                            ppAXIS_Moving = true;
                            ppAXIS_MovingTo = AXIS_MovingTo.Z;
                            return;
                        default:
                            hitt = false;
                            break;
                    }
                    if(!hitt)
                    {
                        if(hit.collider.transform.parent != null)
                        {
                            if (InternalAxis_CheckSideFunctions() && ppAXIS_SelectedPoints.Count > 0)
                            {
                                if (Input.GetKey(ppAXIS_AddPointsInput) && hit.collider.transform.parent == ppAXIS_TargetObject.ppVerticesRoot.transform)
                                {
                                    hit.collider.gameObject.GetComponentInChildren<Renderer>().material.color = ppAXIS_SelectedPointColor;
                                    hit.collider.gameObject.transform.parent = ppAXIS_GroupSelector.transform;
                                    ppAXIS_SelectedPoints.Add(hit.collider.gameObject.transform);
                                    ppAXIS_Moving = true;
                                    ppAXIS_AxisObject.SetActive(true);

                                    InternalAxis_RefreshBounds();
                                    return;
                                }
                                else if (Input.GetKey(ppAXIS_RemovePointsInput) && hit.collider.transform.parent == ppAXIS_GroupSelector.transform)
                                {
                                    hit.collider.gameObject.GetComponentInChildren<Renderer>().material.color = ppAXIS_StoragePointColor;
                                    hit.collider.gameObject.transform.parent = ppAXIS_TargetObject.ppVerticesRoot;
                                    ppAXIS_SelectedPoints.Remove(hit.collider.gameObject.transform);
                                    ppAXIS_Moving = true;
                                    ppAXIS_AxisObject.SetActive(true);

                                    InternalAxis_RefreshBounds();
                                    return;
                                }
                            }
                            else if (ppAXIS_SelectedPoints.Count == 0 && hit.collider.transform.parent == ppAXIS_TargetObject.ppVerticesRoot.transform)
                            {
                                ppAXIS_SelectedPoints.Add(hit.collider.gameObject.transform);

                                ppAXIS_Moving = true;
                                ppAXIS_AxisObject.SetActive(true);
                                ppAXIS_GroupSelector.transform.position = hit.collider.transform.position;

                                ppAXIS_StoragePointColor = hit.collider.transform.GetComponentInChildren<Renderer>().material.color;
                                hit.collider.gameObject.GetComponentInChildren<Renderer>().material.color = ppAXIS_SelectedPointColor;

                                InternalAxis_RefreshBounds(hit.collider.gameObject.transform);

                                ppAXIS_UndoStoredObjects.Clear();
                                ppAXIS_UndoStoredObjects.Add(ppAXIS_SelectedPoints[0]);
                                return;
                            }
                        }
                    }
                }

                ppAXIS_Selecting = true;
                ppAXIS_CursorPosOrigin = Input.mousePosition;
                if (!InternalAxis_CheckSideFunctions())
                {
                    if (ppAXIS_SelectedPoints.Count > 0)
                    {
                        ppAXIS_UndoStoredObjects.Clear();
                        foreach (Transform t in ppAXIS_SelectedPoints)
                        {
                            t.GetComponentInChildren<Renderer>().material.color = ppAXIS_StoragePointColor;
                            t.transform.parent = ppAXIS_TargetObject.ppVerticesRoot.transform;
                            ppAXIS_UndoStoredObjects.Add(t);
                        }
                    }
                    ppAXIS_AxisObject.SetActive(false);
                    ppAXIS_SelectedPoints.Clear();
                }
            }

            if(ppAXIS_Moving)
            {
                if (ppAXIS_MovingTo == AXIS_MovingTo.X)
                {
                    float PosFix = 1;
                    if (ppMainCam.transform.position.z > ppAXIS_AxisObject.transform.position.z)
                        PosFix *= -1;
                    ppAXIS_GroupSelector.transform.position += ppAXIS_GroupSelector.transform.right * (Input.GetAxis("Mouse X") * PosFix) * ppAXIS_Speed * Time.deltaTime;
                }
                if (ppAXIS_MovingTo == AXIS_MovingTo.Y)
                    ppAXIS_GroupSelector.transform.position += ppAXIS_GroupSelector.transform.up * Input.GetAxis("Mouse Y") * ppAXIS_Speed * Time.deltaTime;
                if (ppAXIS_MovingTo == AXIS_MovingTo.Z)
                {
                    float PosFix = 1;
                    if (ppMainCam.transform.position.x < ppAXIS_AxisObject.transform.position.x)
                        PosFix *= -1;
                    ppAXIS_GroupSelector.transform.position += ppAXIS_GroupSelector.transform.forward * (Input.GetAxis("Mouse X") * PosFix) * ppAXIS_Speed * Time.deltaTime;
                }

                ppAXIS_AxisObject.transform.position = ppAXIS_GroupSelector.transform.position;
            }

            //---AFTER SELECTION
            if (Input.GetKeyUp(ppAXIS_SelectionInput))
            {
                if(ppAXIS_Moving)
                {
                    ppAXIS_Moving = false;
                    return;
                }

                if (ppAXIS_TotalPoints.Count == 0)
                    return;

                int c = 0;
                foreach (Transform t in ppAXIS_TotalPoints)
                {
                    if (t == null)
                        continue;
                    if (AxisEditor_Utilities.IsInsideSelection(ppMainCam, ppAXIS_CursorPosOrigin, t.gameObject))
                    {
                        if (!Input.GetKey(ppAXIS_RemovePointsInput))
                        {
                            if (c == 0)
                                ppAXIS_StoragePointColor = t.GetComponentInChildren<Renderer>().material.color;
                            ppAXIS_SelectedPoints.Add(t);
                            t.GetComponentInChildren<Renderer>().material.color = ppAXIS_SelectedPointColor;
                        }
                        else
                        {
                            t.GetComponentInChildren<Renderer>().material.color = ppAXIS_StoragePointColor;
                            t.transform.parent = ppAXIS_TargetObject.ppVerticesRoot;
                            ppAXIS_SelectedPoints.Remove(t);
                            continue;
                        }
                        c++;
                    }
                }
                ppAXIS_Selecting = false;
                if (ppAXIS_SelectedPoints.Count>0)
                {
                    ppAXIS_AxisObject.SetActive(true);

                    InternalAxis_RefreshBounds();
                }
                else
                    ppAXIS_AxisObject.SetActive(false);
            }
        }

        #region AXIS EDITOR Methods

        private bool InternalAxis_CheckSideFunctions()
        {
            return (Input.GetKey(ppAXIS_AddPointsInput) || Input.GetKey(ppAXIS_RemovePointsInput));
        }

        private void InternalAxis_RefreshBounds(Transform center = null)
        {
            if (InternalAxis_CheckSideFunctions())
            {
                foreach (Transform p in ppAXIS_SelectedPoints)
                    p.parent = null;
            }

            Vector3 Center = AxisEditor_Utilities.FindCenterPoint(ppAXIS_SelectedPoints.ToArray());
            ppAXIS_GroupSelector.transform.position = Center;

            if (!ppAXIS_LocalSpace)
                ppAXIS_GroupSelector.transform.rotation = Quaternion.identity;
            else
            {
                if (!center)
                    ppAXIS_GroupSelector.transform.rotation = ppAXIS_TargetObject.ppVerticesRoot.transform.rotation;
                else
                    ppAXIS_GroupSelector.transform.rotation = center.rotation;
            }

            foreach (Transform p in ppAXIS_SelectedPoints)
                p.parent = ppAXIS_GroupSelector.transform;

            ppAXIS_AxisObject.transform.position = ppAXIS_GroupSelector.transform.position;
            ppAXIS_AxisObject.transform.rotation = ppAXIS_GroupSelector.transform.rotation;
        }

        private void OnGUI()
        {
            if (!ppAXIS_EDITOR)
                return;

            if (ppAXIS_Selecting)
            {
                var rect = AxisEditor_Utilities.GetScreenRect(ppAXIS_CursorPosOrigin, Input.mousePosition);
                AxisEditor_Utilities.DrawScreenRect(rect, new Color(0.8f, 0.8f, 0.95f, 0.25f), ppAXIS_SelectionGridColor);
                AxisEditor_Utilities.DrawScreenRectBorder(rect, 2, new Color(0.8f, 0.8f, 0.95f), ppAXIS_SelectionGridColor);
            }
        }

        /// <summary>
        /// Axis method - switch editor target
        /// </summary>
        /// <param name="Target"></param>
        public void AXIS_SwitchTarget(MD_MeshProEditor Target)
        {
            ppAXIS_TargetObject = Target;

            ppAXIS_UndoStoredObjects.Clear();
            ppAXIS_TotalPoints.Clear();
            ppAXIS_SelectedPoints.Clear();

            if (!ppAXIS_TargetObject)
            {
                MD_Debug.Debug(this, "Target Object is missing!", MD_Debug.DebugType.Error);
                return;
            }
            if (!ppAXIS_TargetObject.ppVerticesRoot)
            {
                MD_Debug.Debug(this, "Target Objects vertices root is missing!", MD_Debug.DebugType.Error);
                return;
            }
            if (!ppAXIS_GroupSelector)
                ppAXIS_GroupSelector = new GameObject("AxisEditor_GroupSelector");
            foreach (Transform t in ppAXIS_TargetObject.ppVerticesRoot.transform)
                ppAXIS_TotalPoints.Add(t);
        }

        /// <summary>
        /// Axis method - undo selection
        /// </summary>
        public void AXIS_Undo()
        {
            if (ppAXIS_UndoStoredObjects.Count == 0)
                return;

            if (ppAXIS_SelectedPoints.Count > 0)
                foreach (Transform t in ppAXIS_SelectedPoints)
                {
                    t.GetComponentInChildren<Renderer>().material.color = ppAXIS_StoragePointColor;
                    t.transform.parent = ppAXIS_TargetObject.ppVerticesRoot.transform;
                }

            foreach (Transform t in ppAXIS_UndoStoredObjects)
            {
                if (t != null)
                    ppAXIS_SelectedPoints.Add(t);
            }
            ppAXIS_UndoStoredObjects.Clear();

            ppAXIS_Selecting = false;
            ppAXIS_AxisObject.SetActive(true);

            Vector3 Center = AxisEditor_Utilities.FindCenterPoint(ppAXIS_SelectedPoints.ToArray());
            ppAXIS_GroupSelector.transform.position = Center;
            if (ppAXIS_LocalSpace && ppAXIS_SelectedPoints.Count == 1)
                ppAXIS_GroupSelector.transform.rotation = ppAXIS_SelectedPoints[0].rotation;
            else
                ppAXIS_GroupSelector.transform.rotation = Quaternion.identity;
            ppAXIS_StoragePointColor = ppAXIS_SelectedPoints[0].GetComponentInChildren<Renderer>().material.color;
            foreach (Transform p in ppAXIS_SelectedPoints)
            {
                p.parent = ppAXIS_GroupSelector.transform;
                p.GetComponentInChildren<Renderer>().material.color = ppAXIS_SelectedPointColor;
            }

            ppAXIS_AxisObject.transform.position = ppAXIS_GroupSelector.transform.position;
            ppAXIS_AxisObject.transform.rotation = ppAXIS_GroupSelector.transform.rotation;
        }

        private static class AxisEditor_Utilities
        {
            //---Creating Grid Texture
            static Texture2D GridTexture;
            public static Texture2D GridColor(Color tex)
            {
                    if (GridTexture == null)
                    {
                        GridTexture = new Texture2D(1, 1);
                        GridTexture.SetPixel(0, 0, tex);
                        GridTexture.Apply();
                    }
                    return GridTexture;
            }
            //---Drawing Grid Borders
            public static void DrawScreenRectBorder(Rect re, float thic, Color c, Color mainC)
            {
                DrawScreenRect(new Rect(re.xMin, re.yMin, re.width, thic), c, mainC);
                DrawScreenRect(new Rect(re.xMin, re.yMin, thic, re.height), c, mainC);
                DrawScreenRect(new Rect(re.xMax - thic, re.yMin, thic, re.height), c, mainC);
                DrawScreenRect(new Rect(re.xMin, re.yMax - thic, re.width, thic), c, mainC);
            }
            public static Rect GetScreenRect(Vector3 screenPosition1, Vector3 screenPosition2)
            {
                screenPosition1.y = Screen.height - screenPosition1.y;
                screenPosition2.y = Screen.height - screenPosition2.y;
                var topLeft = Vector3.Min(screenPosition1, screenPosition2);
                var bottomRight = Vector3.Max(screenPosition1, screenPosition2);
                return Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
            }
            //---Drawing Screen Rect
            public static void DrawScreenRect(Rect rect, Color color, Color mainCol)
            {
                GUI.color = color;
                GUI.DrawTexture(rect, GridColor(mainCol));
                GUI.color = Color.white;
            }
            //---Generating Bounds
            public static Bounds GetViewportBounds(Camera camera, Vector3 screenPosition1, Vector3 screenPosition2)
            {
                Vector3 v1 = camera.ScreenToViewportPoint(screenPosition1);
                Vector3 v2 = camera.ScreenToViewportPoint(screenPosition2);
                Vector3 min = Vector3.Min(v1, v2);
                Vector3 max = Vector3.Max(v1, v2);
                min.z = camera.nearClipPlane;
                max.z = camera.farClipPlane;

                Bounds bounds = new Bounds();
                bounds.SetMinMax(min, max);
                return bounds;
            }
            //---Checking Selection
            public static bool IsInsideSelection(Camera camSender, Vector3 MousePos, GameObject ObjectInsideSelection)
            {
                Camera camera = camSender;
                Bounds viewportBounds = GetViewportBounds(camera, MousePos, Input.mousePosition);
                return viewportBounds.Contains(camera.WorldToViewportPoint(ObjectInsideSelection.transform.position));
            }
            //---Find Center In List
            public static Vector3 FindCenterPoint(Transform[] Senders)
            {
                if (Senders.Length == 0)
                    return Vector3.zero;
                if (Senders.Length == 1)
                    return Senders[0].position;
                Bounds bounds = new Bounds(Senders[0].position, Vector3.zero);
                for (int i = 1; i < Senders.Length; i++)
                    bounds.Encapsulate(Senders[i].position);
                return bounds.center;
            }
        }

        #endregion

        #region NON-AXIS EDITOR Methods

        /// <summary>
        /// Switch current control mode by index [1-Grab/Drop,2-Push,3-Pull]
        /// </summary>
        public void NON_AXIS_SwitchControlMode(int index)
        {
            ppVertexControlMode = (VertexControlMode)index;
        }

        #endregion

        private bool Internal_GetControlInput()
        {
            if(!ppMobileSupported)  return Input.GetKey(ppPCInput);
            else                    return Input.touchCount > 0;
        }
    }
}