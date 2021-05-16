using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using MD_Plugin;
#endif

namespace MD_Plugin
{
    [ExecuteInEditMode]
    public class MD_AdvancedPlane : MonoBehaviour
    {
        [Range(1, 40)]
        public int ppPlaneSizeAngle = 5;
        [Range(1, 125)]
        public int ppPlaneSize = 5;
        public Vector3 ppPlaneOffset;
        public bool ppUpdateEveryFrame = true;

        public bool ppEnableAngle = false;
        [Range(-1f, 1f)]
        public float ppAngle = 0;
        [Range(1, 10f)]
        public float ppAngleDensity = 1;

        public bool ppEnableLandscapeFitter = false;
        [Range(0.1f, 2)]
        public float ppTranslationSpeed = 1;

        private Vector3[] vertx;
        private int[] trisx;
        private Vector2[] uvs;
        private MeshFilter meshFilter;
        private Vector3 GizmoPosition1;
        private Vector3 GizmoPosition2;

        private void Awake()
        {
            if (meshFilter != null) return;
            meshFilter = GetComponent<MeshFilter>();
        }

        private void OnDrawGizmosSelected()
        {
            if (ppEnableAngle)
            {
                Gizmos.DrawWireSphere(GizmoPosition1, this.transform.localScale.y / 2);
                Gizmos.DrawWireSphere(GizmoPosition2, this.transform.localScale.y / 2);
            }
        }

        private void Update()
        {
            if (!ppUpdateEveryFrame) return;

            AdvancedPlane_Modify();
        }

#if UNITY_EDITOR
        [MenuItem("GameObject/3D Object"+ MD_Debug.PACKAGENAME + "Advanced /Advanced Plane")]
#endif
        public static GameObject Generate()
        {
            Transform transform = new GameObject("AdvancedPlane").transform;

            transform.gameObject.AddComponent<MeshFilter>();
            transform.gameObject.AddComponent<MeshRenderer>();

            Shader shad = Shader.Find("Standard");
            Material mat = new Material(shad);
            transform.GetComponent<Renderer>().material = mat;

            transform.gameObject.AddComponent<MD_AdvancedPlane>();
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Selection.activeGameObject = transform.gameObject;
                transform.position = SceneView.lastActiveSceneView.camera.transform.position + UnityEditor.SceneView.lastActiveSceneView.camera.transform.forward * 3f;
            }
#endif
            return transform.gameObject;
        }

        /// <summary>
        /// Modify advanced plane mesh
        /// </summary>
        public void AdvancedPlane_Modify()
        {
            if (ppEnableAngle)
                AdvancedPlane_AngleGenerator();
            else
                AdvancedPlane_NoAngleGenerator();
        }

        private void AdvancedPlane_NoAngleGenerator()
        {
            float OffsetVertex = 0.5f;
            ppPlaneOffset.y = 0;
            ppAngle = 0;
            ppAngleDensity = 1;
            ppEnableLandscapeFitter = false;

            Mesh m = new Mesh();

            vertx = new Vector3[ppPlaneSize * ppPlaneSize * 4];
            trisx = new int[ppPlaneSize * ppPlaneSize * 6];
            uvs = new Vector2[ppPlaneSize * ppPlaneSize * 4];

            int v = 0;
            int t = 0;

            for (int x = 0; x < ppPlaneSize; x++)
            {
                for (int y = 0; y < ppPlaneSize; y++)
                {
                    Vector3 cellOffset = new Vector3(x, 0, y);

                    vertx[v] = new Vector3(-OffsetVertex, 0, -OffsetVertex) + cellOffset + ppPlaneOffset;
                    vertx[v + 1] = new Vector3(-OffsetVertex, 0, OffsetVertex) + cellOffset + ppPlaneOffset;
                    vertx[v + 2] = new Vector3(OffsetVertex, 0, -OffsetVertex) + cellOffset + ppPlaneOffset;
                    vertx[v + 3] = new Vector3(OffsetVertex, 0, OffsetVertex) + cellOffset + ppPlaneOffset;

                    trisx[t] = v;
                    trisx[t + 1] = v + 1;
                    trisx[t + 2] = v + 2;
                    trisx[t + 3] = v + 2;
                    trisx[t + 4] = v + 1;
                    trisx[t + 5] = v + 3;

                    uvs[v] = new Vector2(vertx[v].x, vertx[v].y);
                    uvs[v + 1] = new Vector2(vertx[v].x, vertx[v].y - 1);
                    uvs[v + 2] = new Vector2(vertx[v].x - 1, vertx[v].y);
                    uvs[v + 3] = new Vector2(vertx[v].x - 1, vertx[v].y - 1);

                    v += 4;
                    t += 6;
                }
            }

            m.vertices = vertx;
            m.triangles = trisx;
            m.uv = uvs;
            m.RecalculateNormals();
            m.RecalculateBounds();
            m.RecalculateTangents();

            meshFilter.sharedMesh = m;
        }

        private void AdvancedPlane_AngleGenerator()
        {
            float OffsetVertex = 0.5f;
            ppPlaneOffset.y = 0;

            if (!ppEnableAngle)
            {
                ppAngle = 0;
                ppAngleDensity = 1;
                ppEnableLandscapeFitter = false;
            }

            Mesh m = new Mesh();

            vertx = new Vector3[ppPlaneSizeAngle * 4];
            trisx = new int[ppPlaneSizeAngle * 6];
            uvs = new Vector2[ppPlaneSizeAngle * 4];

            int v = 0;
            int t = 0;
            float a = 0;
            float aoff = 0;

            for (int x = 0; x < ppPlaneSizeAngle; x++)
            {
                int vy1Off = 0;
                int vy2Off = 0;
                if (v - 1 > 0 && v - 2 > 0)
                {
                    vy1Off = 2;
                    vy2Off = 1;
                }
                Vector3 cellOffset = new Vector3(x, 0, 0);

                vertx[v] = new Vector3(-OffsetVertex, vertx[v - vy2Off].y, -OffsetVertex) + cellOffset + ppPlaneOffset;
                vertx[v + 1] = new Vector3(-OffsetVertex, vertx[v - vy1Off].y, OffsetVertex) + cellOffset + ppPlaneOffset;
                vertx[v + 2] = new Vector3(OffsetVertex, a, -OffsetVertex) + cellOffset + ppPlaneOffset;
                vertx[v + 3] = new Vector3(OffsetVertex, a, OffsetVertex) + cellOffset + ppPlaneOffset;

                trisx[t] = v;
                trisx[t + 1] = v + 1;
                trisx[t + 2] = v + 2;
                trisx[t + 3] = v + 2;
                trisx[t + 4] = v + 1;
                trisx[t + 5] = v + 3;

                uvs[v] = new Vector2(vertx[v].x, vertx[v].y);
                uvs[v + 1] = new Vector2(vertx[v].x, vertx[v].y - 1);
                uvs[v + 2] = new Vector2(vertx[v].x - 1, vertx[v].y);
                uvs[v + 3] = new Vector2(vertx[v].x - 1, vertx[v].y - 1);

                v += 4;
                t += 6;

                aoff += ppAngle;
                a += ppAngle + a / ppAngleDensity;
            }
            Vector3 point1 = transform.TransformPoint(vertx[vertx.Length - 1]);
            Vector3 point2 = transform.TransformPoint(vertx[vertx.Length - 2]);
            GizmoPosition1 = point1;
            GizmoPosition2 = point2;

            m.vertices = vertx;
            m.triangles = trisx;
            m.uv = uvs;
            m.RecalculateNormals();
            m.RecalculateBounds();
            m.RecalculateTangents();

            meshFilter.sharedMesh = m;

            if (ppEnableLandscapeFitter)
                AdvancedPlane_FitToLandscape();
        }

        protected void AdvancedPlane_FitToLandscape()
        {
            Vector3 point1 = transform.TransformPoint(vertx[vertx.Length - 1]);

            Ray r = new Ray(new Vector3(point1.x, transform.position.y, point1.z), Vector3.down);
            RaycastHit hit;
            if (Physics.Raycast(r, out hit))
            {
                if (hit.collider)
                {
                    if (point1.y > hit.point.y + 2)
                        ppAngle -= ppTranslationSpeed * Time.deltaTime;
                    else if (point1.y < hit.point.y)
                        ppAngle += ppTranslationSpeed * Time.deltaTime;
                }
            }
        }
    }
}
#if UNITY_EDITOR
namespace MD_PluginEditor
{
    [CustomEditor(typeof(MD_AdvancedPlane))]
    public class MD_AdvancedPlane_Editor : MD_EditorUtilities
    {
        private MD_AdvancedPlane ap;

        private void OnEnable()
        {
            ap = (MD_AdvancedPlane)target;
        }

        public override void OnInspectorGUI()
        {
            ps();
            pv();
            pl("Mesh Transform");
            if (ap.ppEnableAngle)
                ppDrawProperty("ppPlaneSizeAngle", "Plane Size");
            else
                ppDrawProperty("ppPlaneSize", "Plane Size");
            ppDrawProperty("ppPlaneOffset", "Plane Pivot Offset");
            ppDrawProperty("ppUpdateEveryFrame", "Update Every Frame");
            pve();

            ppDrawProperty("ppEnableAngle", "Enable Angle Property");
            if (ap.ppEnableAngle)
            {
                pv();
                ppDrawProperty("ppAngle", "Angle");
                ppDrawProperty("ppAngleDensity", "Angle Density");
                pve();
            }

            if (ap.ppEnableAngle)
            {
                GUILayout.Space(10);

                GUI.color = Color.white;
                ppDrawProperty("ppEnableLandscapeFitter", "Enable Landscape Fitter");
                if (ap.ppEnableLandscapeFitter)
                {
                    pv();
                    ppDrawProperty("ppTranslationSpeed", "Translation Speed");
                    pve();
                }
            }

            ppAddMeshColliderRefresher(ap.gameObject);
            ppBackToMeshEditor(ap);

            if (target != null) serializedObject.Update();
        }
    }
}
#endif
