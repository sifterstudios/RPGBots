using UnityEngine;

namespace MD_Plugin
{
    [ExecuteInEditMode]
    public class MD_Triangle : MonoBehaviour 
    {
#if UNITY_EDITOR
        [UnityEditor.MenuItem("GameObject/3D Object" + MD_Debug.PACKAGENAME + "Triangle")]
#endif
        public static GameObject Generate() 
        {
            Transform transform = new GameObject("Triangle").transform;

            Vector3[] Vertices = new Vector3[]
            {
                new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0.5f, 1, 0.5f),
                new Vector3(1, 0, 0), new Vector3(1, 0, 1), new Vector3(0.5f, 1, 0.5f),
                new Vector3(1, 0, 1), new Vector3(0, 0, 1), new Vector3(0.5f, 1, 0.5f),
                new Vector3(0, 0, 1), new Vector3(0, 0, 0), new Vector3(0.5f, 1, 0.5f),
                new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 0, 1), new Vector3(0, 0, 1)
            };

            int[] Triangles = new int[]
            {
                0, 2, 1,
                3, 5, 4,

                6, 8, 7,
                9, 11, 10,

                12, 13, 14,
                14, 15, 12,
            };

            Vector2[] UV = new Vector2[]
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),

                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),

                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),

                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),

                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(1, 1),

            };

            Vector3[] Normals = new Vector3[]
            {
                Vector3.back,
                Vector3.back,
                Vector3.back,

                Vector3.right,
                Vector3.right,
                Vector3.right,

                Vector3.forward,
                Vector3.forward,
                Vector3.forward,

                Vector3.left,
                Vector3.left,
                Vector3.left,

                Vector3.down,
                Vector3.down,
                Vector3.down,
                Vector3.down
            };

            transform.gameObject.AddComponent<MeshFilter>();
            transform.gameObject.AddComponent<MeshRenderer>();

            Mesh myMesh = new Mesh();

            myMesh.vertices = Vertices;
            myMesh.triangles = Triangles;
            myMesh.normals = Normals;
            myMesh.uv = UV;
            myMesh.RecalculateBounds();
            myMesh.RecalculateTangents();

            myMesh.name = "Triangle" + Random.Range(1, 999).ToString();
            transform.GetComponent<MeshFilter>().mesh = myMesh;
            transform.gameObject.AddComponent<MeshCollider>().convex = true;

            Shader shad = Shader.Find("Standard");
            Material mat = new Material(shad);
            transform.GetComponent<Renderer>().material = mat;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.Selection.activeGameObject = transform.gameObject;
                transform.position = UnityEditor.SceneView.lastActiveSceneView.camera.transform.position + UnityEditor.SceneView.lastActiveSceneView.camera.transform.forward * 3f;
            }
#endif
            return transform.gameObject;
        }
    }
}