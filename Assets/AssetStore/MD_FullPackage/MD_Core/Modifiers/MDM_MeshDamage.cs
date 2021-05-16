using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
using MD_Plugin;
#endif

namespace MD_Plugin
{
    /// <summary>
    /// MDM(Mesh Deformation Modifier): Mesh Damage
    /// Damage mesh by the specific parameters
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter))]
    [AddComponentMenu(MD_Debug.ORGANISATION + MD_Debug.PACKAGENAME + "Modifiers/Mesh Damage")]
    public class MDM_MeshDamage : MonoBehaviour
    {
        public bool ppRecalculateNormals = true;
        public bool ppRecalculateBounds = true;

        public bool ppAutoForce = true;
        public float ppForceAmount = 0.15f;
        public float ppForceMultiplier = 0.075f;
        public bool ppAutoGenerateRadius = false;
        public float ppRadius = 0.5f;
        public float ppForceDetection = 1.5f;

        public bool ppContinousDamage = false;

        public bool ppCollisionWithSpecificTag = false;
        public string ppCollisionTag = "";

        public bool ppEnableEvent;
        public UnityEvent ppEvent;

        public bool ppCreateNewReference = true;

        private Vector3[] originalVertices;
        private Vector3[] workingVertices;

        [SerializeField] private MeshFilter meshFilter;

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
            MeshDamage_RefreshVertices();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!Application.isPlaying)
                return;
            if (collision.contacts.Length == 0)
                return;
            if (ppForceDetection != 0 && collision.relativeVelocity.magnitude < ppForceDetection)
                return;
            if (ppCollisionWithSpecificTag && ppCollisionTag != collision.gameObject.tag)
                return;
            if (ppAutoForce)
                ppForceAmount = collision.relativeVelocity.magnitude * ppForceMultiplier;
            if (ppAutoGenerateRadius)
                ppRadius = collision.transform.localScale.magnitude / 4;

            foreach (ContactPoint cp in collision.contacts)
                MeshDamage_ModifyMesh(cp.point, ppRadius, ppForceAmount, collision.relativeVelocity);

            if (ppContinousDamage)
                MeshDamage_RefreshVertices();

            if (ppEnableEvent) ppEvent?.Invoke();
        }

        /// <summary>
        /// Modify current mesh by the point, radius and force
        /// </summary>
        public void MeshDamage_ModifyMesh(Vector3 atPoint, float radius, float force, Vector3 initialDirection)
        {
            initialDirection = initialDirection.normalized;

            for (int i = 0; i < workingVertices.Length; i++)
            {
                Vector3 ppp = transform.TransformPoint(originalVertices[i]);
                float distance = Vector3.Distance(atPoint, ppp);
                if (distance < radius)
                {
                    ppp += (initialDirection * force) * (radius - distance);
                    workingVertices[i] = transform.InverseTransformPoint(ppp);
                }
            }

            meshFilter.mesh.SetVertices(workingVertices);
            if (ppRecalculateNormals) meshFilter.mesh.RecalculateNormals();
            if (ppRecalculateBounds) meshFilter.mesh.RecalculateBounds();
        }

        /// <summary>
        /// Refresh vertices & register brand new original vertices state
        /// </summary>
        public void MeshDamage_RefreshVertices()
        {
            originalVertices = meshFilter.mesh.vertices;
            workingVertices = meshFilter.mesh.vertices;
        }

        /// <summary>
        /// Repair deformed mesh by specified speed value
        /// </summary>
        public void MeshDamage_RepairMesh(float speed = 0.5f)
        {
            for (int i = 0; i < workingVertices.Length; i++)
                workingVertices[i] = Vector3.Lerp(workingVertices[i], originalVertices[i], speed * Time.deltaTime);

            meshFilter.mesh.SetVertices(workingVertices);
            if (ppRecalculateNormals) meshFilter.mesh.RecalculateNormals();
            if (ppRecalculateBounds) meshFilter.mesh.RecalculateBounds();
        }

        /// <summary>
        /// Modify current mesh by custom RaycastEvent
        /// </summary>
        public void MeshDamage_ModifyMesh(MDM_RaycastEvent RayEvent)
        {
            if (!Application.isPlaying)
                return;
            if (RayEvent == null)
                return;
            if (RayEvent.hits.Length > 0 && RayEvent.hits[0].collider.gameObject != this.gameObject)
                return;
            if (ppAutoGenerateRadius)
            {
                if (!RayEvent.ppPointRay)
                    ppRadius = RayEvent.ppSphericalRadius;
                else
                    ppRadius = 0.1f;
            }

            foreach (RaycastHit hit in RayEvent.hits)
                MeshDamage_ModifyMesh(hit.point, ppRadius, ppForceAmount, RayEvent.ray.direction);
        }
    }
}

#if UNITY_EDITOR
namespace MD_PluginEditor
{
    [CustomEditor(typeof(MDM_MeshDamage))]
    [CanEditMultipleObjects]
    public class MDM_MeshDamage_Editor : MD_EditorUtilities
    {
        private MDM_MeshDamage m;

        private void OnEnable()
        {
            m = (MDM_MeshDamage)target;
        }

        public override void OnInspectorGUI()
        {
            ps();

            pv();
            ppDrawProperty("ppRecalculateNormals", "Recalculate Normals");
            ppDrawProperty("ppRecalculateBounds", "Recalculate Bounds");
            pve();

            ps();

            pv();
            ppDrawProperty("ppAutoGenerateRadius", "Auto Generate Radius", "The collision hit radius will be generated automatically");
            if (!m.ppAutoGenerateRadius)
            {
                EditorGUI.indentLevel++;
                ppDrawProperty("ppRadius", "Collision Radius");
                EditorGUI.indentLevel--;
            }
            ps(5);
            ppDrawProperty("ppAutoForce", "Auto Force Multiplier", "If enabled, the script will detect force multiplier automatically [collided rigidbody impact]");
            if (!m.ppAutoForce) ppDrawProperty("ppForceAmount", "Impact Force");
            else
            {
                EditorGUI.indentLevel++;
                ppDrawProperty("ppForceMultiplier", "Force Multiplier");
                EditorGUI.indentLevel--;
            }
            ps(5);
            ppDrawProperty("ppForceDetection", "Force Detection Level", "Minimum relative velocity impact detection level");
            pve();


            ps(10);
            pv();
            ppDrawProperty("ppContinousDamage", "Continuous Effect", "If enabled, vertices of the mesh will be able to go beyond the origin");
            ps(5);
            ppDrawProperty("ppCollisionWithSpecificTag", "Collision With Specific Tag", "If enabled, collision will be allowed for objects with tag below");
            if (m.ppCollisionWithSpecificTag)
                ppDrawProperty("ppCollisionTag", "Collision Tag");
            pve();

            ps();

            ppDrawProperty("ppEnableEvent", "Enable Event System");
            if (m.ppEnableEvent)
                ppDrawProperty("ppEvent", "Event On Collision", "Event will be proceeded after collision enter");

            ps(15);

            ppAddMeshColliderRefresher(m.gameObject);
            ppBackToMeshEditor(m);

            if (target != null) serializedObject.Update();
        }
    }
}
#endif
