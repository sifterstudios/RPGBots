using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using MD_Plugin;
#endif

namespace MD_Plugin
{
    [ExecuteInEditMode]
    public class MD_HexagonGrid : MonoBehaviour
    {
        [Range(1, 90)]
        public int ppCount = 1;
        public Vector3 ppOffset;
        public float ppCellSize = 1;

        [Range(0.0f, 5)]
        public float ppOffsetX = 0.0f;
        [Range(-0.25f, 5)]
        public float ppOffsetZ = -0.25f;

        public float ppRandomHeightRange = 1;

        public bool ppPlanerHexagon = true;
        public bool ppInvert = false;

        public bool ppUpdateEveryFrame = true;
        private MeshFilter meshFilter;

        private Vector3[] verts;
        private int[] tris;
        private Vector2[] uvs;

#if UNITY_EDITOR
        [MenuItem("GameObject/3D Object" + MD_Debug.PACKAGENAME + "Advanced/Hexagon Grid")]
#endif
        public static GameObject Generate()
        {
            Transform transform = new GameObject("HexagonGrid").transform;

            transform.gameObject.AddComponent<MeshFilter>();
            transform.gameObject.AddComponent<MeshRenderer>();

            transform.gameObject.AddComponent<MD_HexagonGrid>();

            Shader shad = Shader.Find("Standard");
            Material mat = new Material(shad);

            transform.GetComponent<Renderer>().material = mat;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Selection.activeGameObject = transform.gameObject;
                transform.position = SceneView.lastActiveSceneView.camera.transform.position + SceneView.lastActiveSceneView.camera.transform.forward * 3f;
            }
#endif

            return transform.gameObject;
        }

#if UNITY_EDITOR
        [MenuItem("GameObject/3D Object" + MD_Debug.PACKAGENAME + "Planar Hexagon")]
#endif
        public static GameObject Generate_SinglePlanarHexagon()
        {
            Transform transform = new GameObject("PlanarHexagon").transform;

            transform.gameObject.AddComponent<MeshFilter>();
            transform.gameObject.AddComponent<MeshRenderer>();

            transform.gameObject.AddComponent<MD_HexagonGrid>();
            transform.gameObject.GetComponent<MD_HexagonGrid>().HexagonGrid_ModifyPlanar();
            transform.gameObject.GetComponent<MeshFilter>().sharedMesh.name = "Hexagon";

            Shader shad = Shader.Find("Standard");
            Material mat = new Material(shad);
            transform.GetComponent<Renderer>().material = mat;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Selection.activeGameObject = transform.gameObject;
                transform.position = SceneView.lastActiveSceneView.camera.transform.position + SceneView.lastActiveSceneView.camera.transform.forward * 3f;
            }
#endif
            DestroyImmediate(transform.gameObject.GetComponent<MD_HexagonGrid>());
            return transform.gameObject;
        }

#if UNITY_EDITOR
        [MenuItem("GameObject/3D Object" + MD_Debug.PACKAGENAME + "Spatial Hexagon")]
#endif
        public static GameObject Generate_SingleSpatialHexagon()
        {
            Transform transform = new GameObject("Hexagon").transform;

            transform.gameObject.AddComponent<MeshFilter>();
            transform.gameObject.AddComponent<MeshRenderer>();

            transform.gameObject.AddComponent<MD_HexagonGrid>();
            transform.gameObject.GetComponent<MD_HexagonGrid>().HexagonGrid_ModifySpatial();
            transform.gameObject.GetComponent<MeshFilter>().sharedMesh.name = "Hexagon";

            Shader shad = Shader.Find("Standard");
            Material mat = new Material(shad);

            transform.GetComponent<Renderer>().material = mat;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Selection.activeGameObject = transform.gameObject;
                transform.position = SceneView.lastActiveSceneView.camera.transform.position + SceneView.lastActiveSceneView.camera.transform.forward * 3f;
            }
#endif
            DestroyImmediate(transform.gameObject.GetComponent<MD_HexagonGrid>());
            return transform.gameObject;
        }


        private void Awake()
        {
            if (meshFilter != null) return;

            meshFilter = GetComponent<MeshFilter>();
        }

        private void Update()
        {
            if (!ppUpdateEveryFrame)
                return;

            HexagonGrid_ModifyHexagon();
        }

        public void HexagonGrid_ModifyHexagon()
        {
            if (ppPlanerHexagon)
                HexagonGrid_ModifyPlanar();
            else
                HexagonGrid_ModifySpatial();
        }

        public void HexagonGrid_ModifyPlanar()
        {
            Mesh m = new Mesh();
            m.name = "HexaGrid";
            verts = new Vector3[ppCount * ppCount * 7];
            tris = new int[ppCount * ppCount * 18];
            uvs = new Vector2[ppCount * ppCount * 7];

            int v = 0;
            int t = 0;

            for (int x = 0; x < ppCount; x++)
            {
                for (int z = 0; z < ppCount; z++)
                {
                    Vector3 Center = new Vector3(x, 0, z);
                    Center.x = (x + z * 0.5f - z / 2) * (0.5f * 2);
                    Center.x += x * ppOffsetX;
                    Center.z += z * ppOffsetZ;

                    //Mid
                    verts[v] = (Center) + ppOffset;
                    //Down
                    verts[v + 1] = new Vector3(0, 0, 0.5f) * ppCellSize + Center + ppOffset;
                    //L-Down
                    verts[v + 2] = new Vector3(0.5f, 0, 0.25f) * ppCellSize + Center + ppOffset;
                    //L-Up
                    verts[v + 3] = new Vector3(0.5f, 0, -0.25f) * ppCellSize + Center + ppOffset;
                    //Up
                    verts[v + 4] = new Vector3(0f, 0, -0.5f) * ppCellSize + Center + ppOffset;
                    //R-Up
                    verts[v + 5] = new Vector3(-0.5f, 0, -0.25f) * ppCellSize + Center + ppOffset;
                    //R-Down
                    verts[v + 6] = new Vector3(-0.5f, 0, 0.25f) * ppCellSize + Center + ppOffset;

                    tris[t] = v;
                    tris[t + 1] = v + 1;
                    tris[t + 2] = v + 2;

                    tris[t + 3] = v;
                    tris[t + 4] = v + 2;
                    tris[t + 5] = v + 3;

                    tris[t + 6] = v;
                    tris[t + 7] = v + 3;
                    tris[t + 8] = v + 4;

                    tris[t + 9] = v;
                    tris[t + 10] = v + 4;
                    tris[t + 11] = v + 5;

                    tris[t + 12] = v;
                    tris[t + 13] = v + 5;
                    tris[t + 14] = v + 6;

                    tris[t + 15] = v;
                    tris[t + 16] = v + 6;
                    tris[t + 17] = v + 1;

                    //Mid
                    uvs[v] = new Vector2(verts[v].x, verts[v].y);
                    //Down
                    uvs[v + 1] = new Vector2(verts[v].x - 1, verts[v].y);
                    //L-Down
                    uvs[v + 2] = new Vector2(verts[v].x - 1, verts[v].y - 1);
                    //L-Up
                    uvs[v + 3] = new Vector2(verts[v].x, verts[v].y - 1);
                    //Up
                    uvs[v + 4] = new Vector2(verts[v].x + 1, verts[v].y);
                    //R-Up
                    uvs[v + 5] = new Vector2(verts[v].x + 1, verts[v].y + 1);
                    //R-Down
                    uvs[v + 6] = new Vector2(verts[v].x, verts[v].y + 1);

                    v += 7;
                    t += 18;
                }
            }

            m.vertices = verts;
            m.triangles = tris;
            m.uv = uvs;
            m.RecalculateNormals();
            m.RecalculateBounds();
            m.RecalculateTangents();
            meshFilter.sharedMesh = m;
        }

        public void HexagonGrid_ModifySpatial(float AddHeightRand = 0)
        {
            Mesh m = new Mesh();
            m.name = "HexaGrid";
            verts = new Vector3[ppCount * ppCount * 14];
            tris = new int[ppCount * ppCount * 72];
            uvs = new Vector2[ppCount * ppCount * 14];

            int v = 0;
            int t = 0;

            for (int x = 0; x < ppCount; x++)
            {
                for (int z = 0; z < ppCount; z++)
                {
                    Vector3 Center = new Vector3(x, 0, z);
                    Vector3 AddHeight = new Vector3(0, 1, 0);
                    float RandomHeight = Random.Range(0, AddHeightRand);
                    Center.x = (x + z * 0.5f - z / 2) * (0.5f * 2);
                    Center.x += x * ppOffsetX;
                    Center.z += z * ppOffsetZ;

                    //Mid
                    verts[v] = Center + ppOffset;
                    //Down
                    verts[v + 1] = new Vector3(0, 0, 0.5f) * ppCellSize + Center + ppOffset;
                    //L-Down
                    verts[v + 2] = new Vector3(0.5f, 0, 0.25f) * ppCellSize + Center + ppOffset;
                    //L-Up
                    verts[v + 3] = new Vector3(0.5f, 0, -0.25f) * ppCellSize + Center + ppOffset;
                    //Up
                    verts[v + 4] = new Vector3(0f, 0, -0.5f) * ppCellSize + Center + ppOffset;
                    //R-Up
                    verts[v + 5] = new Vector3(-0.5f, 0, -0.25f) * ppCellSize + Center + ppOffset;
                    //R-Down
                    verts[v + 6] = new Vector3(-0.5f, 0, 0.25f) * ppCellSize + Center + ppOffset;

                    if (RandomHeight != 0)
                        Center.y = RandomHeight;

                    //Mid
                    verts[v + 7] = Center + ppOffset + AddHeight;
                    //Down
                    verts[v + 8] = new Vector3(0, 0, 0.5f) * ppCellSize + Center + ppOffset + AddHeight;
                    //L-Down
                    verts[v + 9] = new Vector3(0.5f, 0, 0.25f) * ppCellSize + Center + ppOffset + AddHeight;
                    //L-Up
                    verts[v + 10] = new Vector3(0.5f, 0, -0.25f) * ppCellSize + Center + ppOffset + AddHeight;
                    //Up
                    verts[v + 11] = new Vector3(0f, 0, -0.5f) * ppCellSize + Center + ppOffset + AddHeight;
                    //R-Up
                    verts[v + 12] = new Vector3(-0.5f, 0, -0.25f) * ppCellSize + Center + ppOffset + AddHeight;
                    //R-Down
                    verts[v + 13] = new Vector3(-0.5f, 0, 0.25f) * ppCellSize + Center + ppOffset + AddHeight;

                    #region Tris
                    if (ppInvert)
                    {
                        //--TOP
                        tris[t] = v;
                        tris[t + 1] = v + 1;
                        tris[t + 2] = v + 2;

                        tris[t + 3] = v;
                        tris[t + 4] = v + 2;
                        tris[t + 5] = v + 3;

                        tris[t + 6] = v;
                        tris[t + 7] = v + 3;
                        tris[t + 8] = v + 4;

                        tris[t + 9] = v;
                        tris[t + 10] = v + 4;
                        tris[t + 11] = v + 5;

                        tris[t + 12] = v;
                        tris[t + 13] = v + 5;
                        tris[t + 14] = v + 6;

                        tris[t + 15] = v;
                        tris[t + 16] = v + 6;
                        tris[t + 17] = v + 1;


                        //--Sides
                        tris[t + 36] = v + 2;
                        tris[t + 37] = v + 1;
                        tris[t + 38] = v + 8;
                        tris[t + 39] = v + 8;
                        tris[t + 40] = v + 9;
                        tris[t + 41] = v + 2;

                        tris[t + 42] = v + 3;
                        tris[t + 43] = v + 2;
                        tris[t + 44] = v + 9;
                        tris[t + 45] = v + 9;
                        tris[t + 46] = v + 10;
                        tris[t + 47] = v + 3;

                        tris[t + 48] = v + 4;
                        tris[t + 49] = v + 3;
                        tris[t + 50] = v + 10;
                        tris[t + 51] = v + 10;
                        tris[t + 52] = v + 11;
                        tris[t + 53] = v + 4;

                        tris[t + 54] = v + 5;
                        tris[t + 55] = v + 4;
                        tris[t + 56] = v + 11;
                        tris[t + 57] = v + 11;
                        tris[t + 58] = v + 12;
                        tris[t + 59] = v + 5;

                        tris[t + 60] = v + 6;
                        tris[t + 61] = v + 5;
                        tris[t + 62] = v + 12;
                        tris[t + 63] = v + 12;
                        tris[t + 64] = v + 13;
                        tris[t + 65] = v + 6;

                        tris[t + 66] = v + 1;
                        tris[t + 67] = v + 6;
                        tris[t + 68] = v + 13;
                        tris[t + 69] = v + 13;
                        tris[t + 70] = v + 8;
                        tris[t + 71] = v + 1;


                        //----BOTTOM
                        tris[t + 18] = v + 7;
                        tris[t + 19] = v + 9;
                        tris[t + 20] = v + 8;

                        tris[t + 21] = v + 7;
                        tris[t + 22] = v + 10;
                        tris[t + 23] = v + 9;

                        tris[t + 24] = v + 7;
                        tris[t + 25] = v + 11;
                        tris[t + 26] = v + 10;

                        tris[t + 27] = v + 7;
                        tris[t + 28] = v + 12;
                        tris[t + 29] = v + 11;

                        tris[t + 30] = v + 7;
                        tris[t + 31] = v + 13;
                        tris[t + 32] = v + 12;

                        tris[t + 33] = v + 7;
                        tris[t + 34] = v + 8;
                        tris[t + 35] = v + 13;
                    }
                    else
                    {
                        //--TOP
                        tris[t] = v;
                        tris[t + 1] = v + 2;
                        tris[t + 2] = v + 1;

                        tris[t + 3] = v;
                        tris[t + 4] = v + 3;
                        tris[t + 5] = v + 2;

                        tris[t + 6] = v;
                        tris[t + 7] = v + 4;
                        tris[t + 8] = v + 3;

                        tris[t + 9] = v;
                        tris[t + 10] = v + 5;
                        tris[t + 11] = v + 4;

                        tris[t + 12] = v;
                        tris[t + 13] = v + 6;
                        tris[t + 14] = v + 5;

                        tris[t + 15] = v;
                        tris[t + 16] = v + 1;
                        tris[t + 17] = v + 6;



                        tris[t + 36] = v + 2;
                        tris[t + 37] = v + 8;
                        tris[t + 38] = v + 1;
                        tris[t + 39] = v + 8;
                        tris[t + 40] = v + 2;
                        tris[t + 41] = v + 9;

                        tris[t + 42] = v + 3;
                        tris[t + 43] = v + 9;
                        tris[t + 44] = v + 2;
                        tris[t + 45] = v + 9;
                        tris[t + 46] = v + 3;
                        tris[t + 47] = v + 10;

                        tris[t + 48] = v + 4;
                        tris[t + 49] = v + 10;
                        tris[t + 50] = v + 3;
                        tris[t + 51] = v + 10;
                        tris[t + 52] = v + 4;
                        tris[t + 53] = v + 11;

                        tris[t + 54] = v + 5;
                        tris[t + 55] = v + 11;
                        tris[t + 56] = v + 4;
                        tris[t + 57] = v + 11;
                        tris[t + 58] = v + 5;
                        tris[t + 59] = v + 12;

                        tris[t + 60] = v + 6;
                        tris[t + 61] = v + 12;
                        tris[t + 62] = v + 5;
                        tris[t + 63] = v + 12;
                        tris[t + 64] = v + 6;
                        tris[t + 65] = v + 13;

                        tris[t + 66] = v + 1;
                        tris[t + 67] = v + 13;
                        tris[t + 68] = v + 6;
                        tris[t + 69] = v + 13;
                        tris[t + 70] = v + 1;
                        tris[t + 71] = v + 8;


                        //----BOTTOM
                        tris[t + 18] = v + 7;
                        tris[t + 19] = v + 8;
                        tris[t + 20] = v + 9;

                        tris[t + 21] = v + 7;
                        tris[t + 22] = v + 9;
                        tris[t + 23] = v + 10;

                        tris[t + 24] = v + 7;
                        tris[t + 25] = v + 10;
                        tris[t + 26] = v + 11;

                        tris[t + 27] = v + 7;
                        tris[t + 28] = v + 11;
                        tris[t + 29] = v + 12;

                        tris[t + 30] = v + 7;
                        tris[t + 31] = v + 12;
                        tris[t + 32] = v + 13;

                        tris[t + 33] = v + 7;
                        tris[t + 34] = v + 13;
                        tris[t + 35] = v + 8;
                    }
                    #endregion

                    //Mid
                    uvs[v] = new Vector2(0, 0);
                    //Down
                    uvs[v + 1] = new Vector2(0, -1);
                    //L-Down
                    uvs[v + 2] = new Vector2(-1, -1);
                    //L-Up
                    uvs[v + 3] = new Vector2(-1, 1);
                    //Up
                    uvs[v + 4] = new Vector2(0, 1);
                    //R-Up
                    uvs[v + 5] = new Vector2(1, 1);
                    //R-Down
                    uvs[v + 6] = new Vector2(1, -1);

                    //Mid
                    uvs[v + 7] = new Vector2(0, 0);
                    //Down
                    uvs[v + 8] = new Vector2(0, -1);
                    //L-Down
                    uvs[v + 9] = new Vector2(-1, -1);
                    //L-Up
                    uvs[v + 10] = new Vector2(-1, 1);
                    //Up
                    uvs[v + 11] = new Vector2(0, 1);
                    //R-Up
                    uvs[v + 12] = new Vector2(1, 1);
                    //R-Down
                    uvs[v + 13] = new Vector2(1, -1);

                    v += 14;
                    t += 72;
                }
            }

            m.vertices = verts;
            m.triangles = tris;
            m.uv = uvs;
            m.RecalculateNormals();
            m.RecalculateBounds();
            m.RecalculateTangents();
            meshFilter.sharedMesh = m;
        }

        public void HexagonGrid_RandomizeHeight(float Offset)
        {
            HexagonGrid_ModifySpatial(Offset);
        }
    }
}

#if UNITY_EDITOR
namespace MD_PluginEditor
{
    [CustomEditor(typeof(MD_HexagonGrid))]
    public class MD_HexagonGrid_Editor : MD_EditorUtilities
    {
        private MD_HexagonGrid hg;

        private void OnEnable()
        {
            hg = (MD_HexagonGrid)target;
        }

        public override void OnInspectorGUI()
        {
            ps();
            pv();
            ppDrawProperty("ppCount", "Hexagon Count");
            ppDrawProperty("ppCellSize", "Cell Size");
            ppDrawProperty("ppPlanerHexagon", "Planar");
            if (hg.ppPlanerHexagon == false)
                ppDrawProperty("ppInvert", "Invert");
            ps(5);
            ppDrawProperty("ppOffset", "Pivot Offset");
            ps(5);
            ppDrawProperty("ppOffsetX", "Offset X");
            ppDrawProperty("ppOffsetZ", "Offset Z");
            if (!hg.ppUpdateEveryFrame && !hg.ppPlanerHexagon)
            {
                ps(5);
                if (pb("Randomize Height"))
                    hg.HexagonGrid_RandomizeHeight(hg.ppRandomHeightRange);
                ppDrawProperty("ppRandomHeightRange", "Random Height Range");
            }
            ps(5);
            ppDrawProperty("ppUpdateEveryFrame", "Update Every Frame");
            if (!hg.ppUpdateEveryFrame)
            {
                if (pb("Update Hexagon"))
                    hg.HexagonGrid_ModifyHexagon();
            }
            pve();

            ps(15);

            ppBackToMeshEditor(hg);

            if (target != null) serializedObject.Update();
        }
    }
}
#endif
