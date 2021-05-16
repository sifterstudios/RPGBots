using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using MD_Plugin;
#endif

namespace MD_Plugin
{
    /// <summary>
    /// MD(Mesh Deformation) Essential Component: Mesh Collider Refresher
    /// Essential component for general mesh-collider refreshing
    /// </summary>
    [AddComponentMenu(MD_Debug.ORGANISATION + MD_Debug.PACKAGENAME + "Mesh Collider Refresher")]
    public class MD_MeshColliderRefresher : MonoBehaviour
    {
        public enum RefreshType { Once, PerFrame, Interval, Never };
        public RefreshType refreshType = RefreshType.Once;

        public float intervalSeconds = 1f;
        public bool convexMeshCollider = false;
        public MeshColliderCookingOptions cookingOptions = MeshColliderCookingOptions.None;

        public bool ignoreRaycast = false;

        public Vector3 colliderOffset = Vector3.zero;

        private void Awake()
        {
            if (refreshType == RefreshType.Never) return;
            MeshCollider_UpdateMeshCollider();
        }

        private float intervalTimer = 0;
        private void LateUpdate()
        {
            if (refreshType == RefreshType.PerFrame)
                MeshCollider_UpdateMeshCollider();
            else if (refreshType == RefreshType.Interval)
            {
                intervalTimer += Time.deltaTime;
                if (intervalTimer > intervalSeconds)
                {
                    MeshCollider_UpdateMeshCollider();
                    intervalTimer = 0;
                }
            }
        }

        public void MeshCollider_UpdateMeshCollider()
        {
            if (refreshType == RefreshType.Never) return;

            if (ignoreRaycast) gameObject.layer = 2;

            if (!GetComponent<Renderer>())
            {
                MD_Debug.Debug(this, "Object " + this.name + " doesn't contain any Mesh Renderer Component. Mesh Collider Refreshed could not be proceeded", MD_Debug.DebugType.Error);
                return;
            }

            MeshCollider mc = GetComponent<MeshCollider>();
            if (!mc) mc = gameObject.AddComponent<MeshCollider>();
            mc.convex = convexMeshCollider;
            mc.cookingOptions = cookingOptions;

            if (colliderOffset == Vector3.zero)
                return;

            Mesh newMeshCol = new Mesh();
            newMeshCol.vertices = mc.sharedMesh.vertices;
            newMeshCol.triangles = mc.sharedMesh.triangles;
            newMeshCol.normals = mc.sharedMesh.normals;
            Vector3[] verts = newMeshCol.vertices;
            for (int i = 0; i < verts.Length; i++)
                verts[i] += colliderOffset;
            newMeshCol.vertices = verts;
            mc.sharedMesh = newMeshCol;
        }
    }
}

#if UNITY_EDITOR
namespace MD_PluginEditor
{
    [CustomEditor(typeof(MD_MeshColliderRefresher))]
    [CanEditMultipleObjects]
    public class MD_MeshColliderRefresher_Editor : MD_EditorUtilities
    {
        private MD_MeshColliderRefresher m;

        private void OnEnable()
        {
            m = (MD_MeshColliderRefresher)target;
        }

        public override void OnInspectorGUI()
        {
            ps();
            Color c;
            ColorUtility.TryParseHtmlString("#9fe6b2", out c);
            GUI.color = c;
            pv();
            ppDrawProperty("refreshType", "Collider Refresh Type");
            if (m.refreshType == MD_MeshColliderRefresher.RefreshType.Interval)
                ppDrawProperty("intervalSeconds", "Interval [in Seconds]", "Set the interval value for mesh collider refreshing in seconds");
            else if (m.refreshType == MD_MeshColliderRefresher.RefreshType.Once)
                ppDrawProperty("colliderOffset", "Collider Offset", "Specific offset of the mesh collider generated after start");
            ps(10);
            ppDrawProperty("convexMeshCollider", "Convex Mesh Collider");
            ppDrawProperty("cookingOptions", "Cooking Options", "Specify the mesh collider in higher details by choosing proper cooking options");
            ps(5);
            ppDrawProperty("ignoreRaycast", "Ignore Raycast", "If enabled, the objects layer mask will be set to 2 [Ignore raycast]. Otherwise the masks will be untouched");
            pve();

            if (target != null) serializedObject.Update();
        }
    }
}
#endif
