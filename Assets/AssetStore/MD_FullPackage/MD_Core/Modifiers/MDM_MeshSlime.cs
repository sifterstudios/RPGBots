using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using MD_Plugin;
#endif

namespace MD_Plugin
{
    /// <summary>
    /// MDM(Mesh Deformation Modifier): Mesh Slime
    /// Precious & soft slime body modifier with various settings & mobile support
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter))]
    [AddComponentMenu(MD_Debug.ORGANISATION + MD_Debug.PACKAGENAME + "Modifiers/Mesh Slime")]
    public class MDM_MeshSlime : MonoBehaviour
    {
        public bool ppUseControls = true;
        public KeyCode ppInput = KeyCode.Mouse0;
        public bool ppMobilePlatform = false;
        public Camera ppMainCamera;

        public enum exp_AxisType { Y_Only, TowardsObjectsPivot, CrossProduct, NormalsDirection };
        public exp_AxisType ppAxis = exp_AxisType.Y_Only;

        public bool ppRepair = false;
        public float ppRepairSpeed = 1f;

        public float ppRadius = 0.1f;
        public float ppFalloff = 1.0f;
        public float ppIntensity = 0.1f;

        public bool ppReversedDrag = false;
        public float ppDrag = 0.16f;
        public float ppDragFalloff = 0.8f;
        public float ppMaxDragSpeed = 0.5f;

        public bool ppRecalculateNormals = true;
        public bool ppRecalculateBounds = true;

        [SerializeField] private MeshFilter meshFilter;
        private Vector3[] workingVertices;
        private Vector3[] originalVertices;

        private void Awake()
        {
            if (meshFilter != null) return;

            ppRecalculateBounds = MD_GlobalPreferences.autoRecalcBounds;
            ppRecalculateNormals = MD_GlobalPreferences.autoRecalcNormals;

            meshFilter = GetComponent<MeshFilter>();
            MD_MeshProEditor.MeshProEditor_Utilities.util_PrepareMeshDeformationModifier(this, meshFilter);
        }

        private void Start()
        {
            if (!Application.isPlaying) return;
            workingVertices = meshFilter.mesh.vertices;
            originalVertices = meshFilter.mesh.vertices;
        }

        private Vector3 oldhPos;
        private Vector3 currhPos;

        private void Update()
        {
            if (!Application.isPlaying) return;

            if (ppRepair)
            {
                for (int i = 0; i < workingVertices.Length; i++)
                    workingVertices[i] = Vector3.Lerp(workingVertices[i], originalVertices[i], ppRepairSpeed * Time.deltaTime);
                meshFilter.mesh.vertices = workingVertices;

                if (ppRecalculateNormals) meshFilter.mesh.RecalculateNormals();
                if (ppRecalculateBounds) meshFilter.mesh.RecalculateBounds();
            }

            if (!ppUseControls)
                return;

            Ray r = new Ray();
            RaycastHit h;

            if (!ppMobilePlatform)
                r = ppMainCamera.ScreenPointToRay(Input.mousePosition);
            else if (Input.touchCount > 0)
                r = ppMainCamera.ScreenPointToRay(Input.GetTouch(0).position);

            bool c;
            if (!ppMobilePlatform)
                c = Input.GetKey(ppInput);
            else
                c = Input.touchCount > 0;


            if (!c) { oldhPos = Vector3.zero; return; }

            if (Physics.Raycast(r, out h))
            {
                if (h.collider)
                {
                    currhPos = ((!ppReversedDrag) ? (h.point - oldhPos) : (oldhPos - h.point));
                    if (currhPos.magnitude > ppMaxDragSpeed)
                        oldhPos = Vector3.zero;
                    MeshSlime_ModifyMesh(h.point);
                    oldhPos = h.point;
                }
            }
        }

        /// <summary>
        /// Modify mesh on specified world points
        /// </summary>
        public void MeshSlime_ModifyMesh(Vector3 worldPoint)
        {
            if (!Application.isPlaying) return;

            currhPos.y = 0;
            worldPoint = transform.InverseTransformPoint(worldPoint);
            for (int i = 0; i < workingVertices.Length; i++)
            {
                Vector3 vv = workingVertices[i];
                if (Vector3.Distance(vv, worldPoint) > ppRadius)
                    continue;
                float mult = ppFalloff * (ppRadius - (Vector3.Distance(worldPoint, vv)));
                Vector3 dir = Vector3.zero;
                switch (ppAxis)
                {
                    case exp_AxisType.Y_Only:
                        dir = Vector3.up;
                        break;
                    case exp_AxisType.TowardsObjectsPivot:
                        dir = -(transform.position - transform.TransformPoint(vv));
                        break;
                    case exp_AxisType.CrossProduct:
                        dir = Vector3.Cross(vv, worldPoint);
                        break;
                    case exp_AxisType.NormalsDirection:
                        dir = meshFilter.mesh.normals[i];
                        break;
                }
                vv -= (((dir * ppIntensity) * mult) + ((oldhPos == Vector3.zero) ? Vector3.zero : (currhPos * ((ppReversedDrag) ? (ppDrag / 2f) * ppDragFalloff : ppDrag * ppDragFalloff))));
                workingVertices[i] = vv;
            }
            meshFilter.mesh.vertices = workingVertices;
            if (ppRecalculateNormals) meshFilter.mesh.RecalculateNormals();
            if (ppRecalculateBounds) meshFilter.mesh.RecalculateBounds();
        }

        /// <summary>
        /// Modify mesh by specific RaycastEvent
        /// </summary>
        public void MeshSlime_ModifyMesh(MDM_RaycastEvent entry)
        {
            if (!Application.isPlaying) return;

            if (entry.hits.Length > 0 && entry.hits[0].collider.gameObject != this.gameObject)
                return;
            foreach (RaycastHit hit in entry.hits)
                MeshSlime_ModifyMesh(hit.point);
        }
    }
}

#if UNITY_EDITOR
namespace MD_PluginEditor
{
    [CustomEditor(typeof(MDM_MeshSlime))]
    public class MDM_MeshSlime_Editor : MD_EditorUtilities
    {
        private MDM_MeshSlime ms;

        private void OnEnable()
        {
            ms = (MDM_MeshSlime)target;
        }

        public override void OnInspectorGUI()
        {
            ps();
            pv();
            ppDrawProperty("ppRecalculateNormals", "Recalculate Normals");
            ppDrawProperty("ppRecalculateBounds", "Recalculate Bounds");
            pve();
            pv();
            ppDrawProperty("ppUseControls", "Use Contnrols", "If enabled, you will be able to drag the slime mesh with mouse/finger");
            ppDrawProperty("ppMobilePlatform", "Mobile Platform", "If enabled, the slime mesh will be ready for Mobile devices");
            if (ms.ppUseControls && !ms.ppMobilePlatform)
                ppDrawProperty("ppInput", "Input");
            ppDrawProperty("ppMainCamera", "Main Camera", "Main Cam target");
            pve();
            ps();
            pv();
            ppDrawProperty("ppAxis", "Slime Axis Type", "Select proper axis type, each axis type is specific and unique");
            pve();
            ps();
            pv();
            ppDrawProperty("ppRepair", "Repair Slime", "If enabled, the mesh will be 'repaired' to the starting shape");
            if (ms.ppRepair)
                ppDrawProperty("ppRepairSpeed", "Repair Speed");
            pve();
            ps();
            pv();
            ppDrawProperty("ppRadius", "Drag Radius");
            ppDrawProperty("ppFalloff", "Drag Falloff Radius");
            ppDrawProperty("ppIntensity", "Drag Intensity");
            pve();
            ps();
            pv();
            ppDrawProperty("ppReversedDrag", "Reversed Drag", "If enabled, the dragged vertices will move towards the cursor move position [if disabled - vice versa]");
            ppDrawProperty("ppDrag", "Drag Density", "Force in which the drag is proceed");
            ppDrawProperty("ppDragFalloff", "Drag Density Falloff");
            ppDrawProperty("ppMaxDragSpeed", "Max Drag Speed");
            pve();
            ps(15);
            ppAddMeshColliderRefresher(ms.gameObject);
            ppBackToMeshEditor(ms);
            if (target != null) serializedObject.Update();
        }
    }
}
#endif
