using UnityEngine;

namespace MD_Plugin
{
    [ExecuteInEditMode]
    public class MD_Octahedron : MonoBehaviour 
    {
#if UNITY_EDITOR
        [UnityEditor.MenuItem("GameObject/3D Object" + MD_Debug.PACKAGENAME + "Octahedron")]
#endif
        public static GameObject Generate()
        {
            Transform transform = new GameObject("Octahedron").transform;
            Vector3[] Vertices = 
            {
                Vector3.down,
                Vector3.forward,
                Vector3.left,
                Vector3.back,
                Vector3.right,
                Vector3.up
            };

            int[] Triangles = 
            {
                0, 1, 2,
                0, 2, 3,
                0, 3, 4,
                0, 4, 1,

                5, 2, 1,
                5, 3, 2,
                5, 4, 3,
                5, 1, 4
            };

            transform.gameObject.AddComponent<MeshFilter>();
            transform.gameObject.AddComponent<MeshRenderer>();

            Mesh myMesh = new Mesh();

            Vector2[] UV = new Vector2[Vertices.Length];
            CreateUV(Vertices, UV);

            myMesh.vertices = Vertices;
            myMesh.triangles = Triangles;
            myMesh.uv = UV;
            myMesh.RecalculateNormals();
            myMesh.RecalculateBounds();
            myMesh.RecalculateTangents();

            transform.GetComponent<MeshFilter>().mesh = myMesh;

            myMesh.name = "Octahedron" + Random.Range(1, 999).ToString();
            transform.GetComponent<MeshFilter>().mesh = myMesh;

            if (!transform.GetComponent<SphereCollider>())
                transform.gameObject.AddComponent<SphereCollider>();

            Shader shad = Shader.Find("Standard");
            Material mat = new Material(shad);
            transform.GetComponent<Renderer>().material = mat;

            return transform.gameObject;
        }

        private static void CreateUV(Vector3[] vertices, Vector2[] uv)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 v = vertices[i];
                Vector2 textureCoordinates;
                textureCoordinates.x = Mathf.Atan2(v.x, v.z) / (-2f * Mathf.PI);
                if (textureCoordinates.x < 0f)
                    textureCoordinates.x += 1f;

                textureCoordinates.y = Mathf.Asin(v.y) / Mathf.PI + 0.5f;
                uv[i] = textureCoordinates;
            }
        }
    }
}