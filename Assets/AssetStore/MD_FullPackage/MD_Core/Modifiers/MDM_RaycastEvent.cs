using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
using MD_Plugin;
#endif

namespace MD_Plugin
{
    /// <summary>
    /// MDM(Mesh Deformation Modifier): Raycast Event
    /// Raycast behaviour with customizable events
    /// </summary>
    [AddComponentMenu(MD_Debug.ORGANISATION + MD_Debug.PACKAGENAME + "Modifiers/Raycast Event")]
    public class MDM_RaycastEvent : MonoBehaviour
    {
        public bool ppUpdateRayPerFrame = true;

        public float ppDistanceRay = 5.0f;
        public bool ppPointRay = true;
        public float ppSphericalRadius = 0.2f;
        public bool ppLocalRay = true;
        public Vector3 ppGlobalRayDir = new Vector3(0, -1, 0);

        public bool ppSavePerformanceByRigidbody = false;
        public Rigidbody ppTargetRigidbody;
        public float ppTargetVelocitySpeed = 0.01f;

        public bool ppRaycastWithSpecificTag = false;
        public string ppRaycastTag = "";

        public UnityEvent ppEventOnRaycast;
        public UnityEvent ppEventOnRaycastExit;

        public RaycastHit[] hits;
        public Ray ray;

        private void OnDrawGizmosSelected()
        {
            if (ppPointRay == false)
                Gizmos.DrawWireSphere(transform.position + (ppLocalRay ? transform.forward : ppGlobalRayDir) * ppDistanceRay, ppSphericalRadius);
            Gizmos.DrawLine(transform.position, transform.position + (ppLocalRay ? transform.forward : ppGlobalRayDir) * ppDistanceRay);
        }

        private void Update()
        {
            if (ppUpdateRayPerFrame)
                RayEvent_UpdateRaycastState();
        }

        /// <summary>
        /// Get Raycast state
        /// </summary>
        public bool RayEvent_IsRaycasting()
        {
            return raycastingState;
        }

        protected bool raycastingState = false;

        /// <summary>
        /// Update current Raycast
        /// </summary>
        public void RayEvent_UpdateRaycastState()
        {
            if (ppSavePerformanceByRigidbody)
            {
                if (ppTargetRigidbody != null && ppTargetRigidbody.velocity.magnitude <= ppTargetVelocitySpeed)
                    return;
            }
            RaycastHit hit;
            ray = new Ray(transform.position, (ppLocalRay) ? transform.forward : ppGlobalRayDir);
            if (!Physics.Raycast(ray, out hit, ppDistanceRay))
            {
                if (raycastingState) ppEventOnRaycastExit?.Invoke();
                raycastingState = false;
                return;
            }
            else if (ppRaycastWithSpecificTag && hit.collider.tag != ppRaycastTag)
            {
                if (raycastingState) ppEventOnRaycastExit?.Invoke();
                raycastingState = false;
                return;
            }

            raycastingState = true;

            hits = new RaycastHit[0];
            if (ppPointRay)
                hits = Physics.RaycastAll(ray, ppDistanceRay);
            else
                hits = Physics.SphereCastAll(ray, ppSphericalRadius, ppDistanceRay);

            if (hits.Length > 0) ppEventOnRaycast?.Invoke();
        }
    }
}
#if UNITY_EDITOR
namespace MD_PluginEditor
{
    [CustomEditor(typeof(MDM_RaycastEvent))]
    [CanEditMultipleObjects]
    public class MDM_RaycastEvent_Editor : MD_EditorUtilities
    {
        private MDM_RaycastEvent m;
        private void OnEnable()
        {
            m = (MDM_RaycastEvent)target;
        }

        public override void OnInspectorGUI()
        {
            ps();
            pv();
            ppDrawProperty("ppUpdateRayPerFrame", "Update Ray Per Frame", "If disabled, you are able to invoke your own method to Update ray state");
            ps();
            pv();
            ppDrawProperty("ppDistanceRay", "Ray Distance");
            ppDrawProperty("ppPointRay", "Point Ray", "If disabled, raycast will be generated as Spherical Ray");
            if (!m.ppPointRay)
                ppDrawProperty("ppSphericalRadius", "Radius");
            pve();
            ps(5);
            pv();
            ppDrawProperty("ppLocalRay", "Local Ray");
            if (m.ppLocalRay == false)
                ppDrawProperty("ppGlobalRayDir", "Global Ray Direction");
            pve();
            ps(5);
            pv();
            ppDrawProperty("ppRaycastWithSpecificTag", "Raycast Specific Tag", "If disabled, raycast will accept every object with collider");
            if (m.ppRaycastWithSpecificTag)
                ppDrawProperty("ppRaycastTag", "Raycast Tag");
            pve();
            ps(5);
            pv();
            ppDrawProperty("ppSavePerformanceByRigidbody", "Save Performance By Rigidbody", "If enabled, target rigidbody will be required. If the rigidbody is assigned, the script will be updated only if the rigidbody velocity will be greater than 'Target Velocity'");
            if (m.ppSavePerformanceByRigidbody)
            {
                ppDrawProperty("ppTargetRigidbody", "Target Rigidbody");
                ppDrawProperty("ppTargetVelocitySpeed", "Target Velocity");
            }
            pve();
            pve();
            ps();
            ppDrawProperty("ppEventOnRaycast", "Event Raycast Hit", "Event on raycast enter");
            ppDrawProperty("ppEventOnRaycastExit", "Event Raycast Exit", "Event on raycast exit");

            if (target != null) serializedObject.Update();
        }
    }
}
#endif
