using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using MD_Plugin;
#endif

namespace MD_Plugin
{
    /// <summary>
    /// MDM(Mesh Deformation Modifier): Mesh Bend
    /// Twist mesh to the specific direction & with specific value
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter))]
    [AddComponentMenu(MD_Debug.ORGANISATION + MD_Debug.PACKAGENAME + "Modifiers/Mesh Twist")]
    public class MDM_Twist : MonoBehaviour
    {
        public bool ppUpdateEveryFrame = false;
        public bool ppRecalculateNormals = true;
        public bool ppRecalculateBounds = true;

        public enum Direction_ { X, Y, Z }
        public Direction_ ppTwistDirection = Direction_.X;

        public float ppValue = 0;
        public bool ppMirrored = true;

        [SerializeField] private Vector3[] originalVertices;
        [SerializeField] private Vector3[] workingVertices;

        [SerializeField] private MeshFilter meshFilter;

        private void Awake()
        {
            if (meshFilter != null) return;

            ppRecalculateBounds = MD_GlobalPreferences.autoRecalcBounds;
            ppRecalculateNormals = MD_GlobalPreferences.autoRecalcNormals;

            meshFilter = GetComponent<MeshFilter>();
            MD_MeshProEditor.MeshProEditor_Utilities.util_PrepareMeshDeformationModifier(this, meshFilter);
            Twist_RegisterCurrentState();
        }

        private void Update()
        {
            if (!ppUpdateEveryFrame) return;
            if (meshFilter.sharedMesh == null) return;

            Twist_ProcessTwist();
        }

        public void Twist_ProcessTwist()
        {
            if (!ppUpdateEveryFrame) MD_MeshProEditor.MeshProEditor_Utilities.util_CheckVerticeCount(originalVertices.Length, this.gameObject);

            for (int i = 0; i < originalVertices.Length; i++) workingVertices[i] = TwistObject(originalVertices[i], originalVertices[i].z * ppValue);
            meshFilter.sharedMesh.vertices = workingVertices;
            if (ppRecalculateNormals) meshFilter.sharedMesh.RecalculateNormals();
            if (ppRecalculateBounds) meshFilter.sharedMesh.RecalculateBounds();
        }

        private Vector3 TwistObject(Vector3 vert, float val)
        {
            if (val == 0.0f) return vert;
            if (!ppMirrored && vert.y < 0) return vert;

            float sin = Mathf.Sin(val);
            float cos = Mathf.Cos(val);
            Vector3 final = Vector3.zero;
            switch (ppTwistDirection)
            {
                case Direction_.X:
                    final.y = vert.y * cos - vert.z * sin;
                    final.z = vert.y * sin + vert.z * cos;
                    final.x = vert.x;
                    break;
                case Direction_.Y:
                    final.x = vert.x * cos - vert.z * sin;
                    final.z = vert.x * sin + vert.z * cos;
                    final.y = vert.y;
                    break;
                case Direction_.Z:
                    final.x = vert.x * cos - vert.y * sin;
                    final.y = vert.x * sin + vert.y * cos;
                    final.z = vert.z;
                    break;
            }
            return final;
        }

        /// <summary>
        /// Refresh & register current mesh state. This will refresh original vertices to the current state
        /// </summary>
        public void Twist_RegisterCurrentState()
        {
            originalVertices = meshFilter.sharedMesh.vertices;
            workingVertices = new Vector3[originalVertices.Length];
            System.Array.Copy(originalVertices, workingVertices, originalVertices.Length);
            ppValue = 0;
        }

        /// <summary>
        /// Twist object by the UI Slider value
        /// </summary>
        public void Twist_TwistObject(UnityEngine.UI.Slider entry)
        {
            ppValue = entry.value;
            if (!ppUpdateEveryFrame) Twist_ProcessTwist();
        }

        /// <summary>
        /// Twist object by float value
        /// </summary>
        public void Twist_TwistObject(float entry)
        {
            ppValue = entry;
            if (!ppUpdateEveryFrame) Twist_ProcessTwist();
        }
    }

}

#if UNITY_EDITOR
namespace MD_PluginEditor
{
    [CustomEditor(typeof(MDM_Twist))]
    [CanEditMultipleObjects]
    public class MDM_Twist_Editor : MD_EditorUtilities
    {
        private MDM_Twist m;

        private void OnEnable()
        {
            m = (MDM_Twist)target;
        }

        public override void OnInspectorGUI()
        {
            ps();
            pv();
            ppDrawProperty("ppUpdateEveryFrame", "Update Every Frame");
            EditorGUI.indentLevel++;
            ppDrawProperty("ppRecalculateNormals", "Recalculate Normals");
            ppDrawProperty("ppRecalculateBounds", "Recalculate Bounds");
            EditorGUI.indentLevel--;
            if (!m.ppUpdateEveryFrame)
            {
                if (pb("Update Mesh")) m.Twist_ProcessTwist();
            }
            pve();
            ps();
            pv();
            ppDrawProperty("ppTwistDirection", "Twist Direction");
            ppDrawProperty("ppValue", "Twist Value");
            ppDrawProperty("ppMirrored", "Mirrored", "If enabled, the twist will process on both sides of the mesh");
            pve();
            ps();
            pv();
            if (pb("Register Mesh")) m.Twist_RegisterCurrentState();
            phb("Refresh current mesh & register original vertices to the edited vertices");
            pve();
            ps(15);
            ppAddMeshColliderRefresher(m.gameObject);
            ppBackToMeshEditor(m);

            if (target != null) serializedObject.Update();
        }
    }
}
#endif

