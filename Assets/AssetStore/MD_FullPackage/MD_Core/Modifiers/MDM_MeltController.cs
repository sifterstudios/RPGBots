using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using MD_Plugin;
#endif

namespace MD_Plugin
{
    /// <summary>
    /// MDM(Mesh Deformation Modifier): Melt Controller
    /// Control melt shader with the script
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter))]
    [AddComponentMenu(MD_Debug.ORGANISATION + MD_Debug.PACKAGENAME + "Modifiers/Melt Controller")]
    public class MDM_MeltController : MonoBehaviour
    {
        public bool ppMeltBySurfaceRaycast = true;
        public Vector3 ppRaycastOriginOffset = new Vector3(0, 0, 0);
        public Vector3 ppRaycastDirection = new Vector3(0, -1, 0);
        public float ppRaycastDistance = Mathf.Infinity;
        public float ppRaycastRadius = 0.5f;
        public LayerMask ppAllowedLayerMasks = -1;

        public bool ppEnableLinearInterpolationBlend = false;
        public float ppLinearInterpolationSpeed = 0.5f;

        public bool ppShowEditorGraphic = true;

        private Material ppSelfMaterial;
        private Transform ppRealTarget;

        private void OnDrawGizmosSelected()
        {
            if (!ppShowEditorGraphic)
                return;
            if (!ppMeltBySurfaceRaycast)
                return;
            if (!ppRealTarget)
                return;

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(ppRealTarget.position + ppRaycastOriginOffset, ppRealTarget.localScale.magnitude / 6);
            Gizmos.DrawLine(ppRealTarget.position + ppRaycastOriginOffset, ppRealTarget.position + ppRaycastOriginOffset + ppRaycastDirection.normalized * ppRaycastDistance);
            Gizmos.DrawWireSphere(ppRealTarget.position + ppRaycastOriginOffset + ppRaycastDirection.normalized * ppRaycastDistance, ppRaycastRadius);

        }

        private float targetValue;
        private float targetLerpValue;
        private void Awake()
        {
            ppSelfMaterial = GetComponent<Renderer>().sharedMaterial;
            ppRealTarget = transform;

            if (!Application.isPlaying) return;

            ppSelfMaterial = Instantiate(ppSelfMaterial);
            GetComponent<Renderer>().sharedMaterial = ppSelfMaterial;
        }

        private void Update()
        {
            if (!ppRealTarget) return;

            if (!ppMeltBySurfaceRaycast)
            {
                if (!ppEnableLinearInterpolationBlend)
                    ppSelfMaterial.SetFloat("_M_Zone", ppRealTarget.position.y);
                else
                    targetValue = ppRealTarget.position.y;
            }
            else
            {
                Ray r = new Ray(ppRealTarget.transform.position + ppRaycastOriginOffset, ppRaycastDirection.normalized);
                RaycastHit hit;
                if (Physics.SphereCast(r, ppRaycastRadius, out hit, ppRaycastDistance, ppAllowedLayerMasks))
                {
                    if (hit.collider)
                    {
                        if (!ppEnableLinearInterpolationBlend)
                            ppSelfMaterial.SetFloat("_M_Zone", hit.point.y);
                        else
                            targetValue = hit.point.y;
                    }
                    else
                    {
                        targetValue = ppRealTarget.position.y;
                        if (!ppEnableLinearInterpolationBlend)
                            ppSelfMaterial.SetFloat("_M_Zone", targetValue);
                    }
                }
                else
                {
                    targetValue = ppRealTarget.position.y;
                    if (!ppEnableLinearInterpolationBlend)
                        ppSelfMaterial.SetFloat("_M_Zone", targetValue);
                }
            }

            if (ppEnableLinearInterpolationBlend)
            {
                targetLerpValue = Mathf.Lerp(targetLerpValue, targetValue, Time.deltaTime * ppLinearInterpolationSpeed);
                ppSelfMaterial.SetFloat("_M_Zone", targetLerpValue);
            }
        }
    }
}

#if UNITY_EDITOR
namespace MD_PluginEditor
{
    [CustomEditor(typeof(MDM_MeltController))]
    [CanEditMultipleObjects]
    public class MDM_MeltController_Editor : MD_EditorUtilities
    {
        private MDM_MeltController m;

        private void OnEnable()
        {
            m = (MDM_MeltController)target;
        }

        public override void OnInspectorGUI()
        {
            ps();
            phb("Object must contains MD_Melt shader");
            ps();

            pv();
            ppDrawProperty("ppMeltBySurfaceRaycast", "Melt by Raycast", "If enabled, the Y value will be set to modified hit point Y. Otherwise you will be able to customize the value by yourself");
            if (m.ppMeltBySurfaceRaycast)
            {
                ppDrawProperty("ppAllowedLayerMasks", "Allowed Layer Masks");
                ppDrawProperty("ppRaycastOriginOffset", "Raycast Origin Offset");
                ppDrawProperty("ppRaycastDirection", "Raycast Direction");
                ppDrawProperty("ppRaycastDistance", "Raycast Distance");
                ppDrawProperty("ppRaycastRadius", "Raycast Radius");
                ps(5);
                ppDrawProperty("ppEnableLinearInterpolationBlend", "Enable Smooth Transition");
                if (m.ppEnableLinearInterpolationBlend)
                    ppDrawProperty("ppLinearInterpolationSpeed", "Smooth Speed");
            }
            pve();
            ps();
            ppDrawProperty("ppShowEditorGraphic", "Show Editor Graphic");
            ps();
            ppBackToMeshEditor(m);

            if (target != null) serializedObject.Update();
        }
    }
}
#endif

