using UnityEngine;

namespace MD_Plugin
{
    [ExecuteInEditMode]
    public class MD_TwoSidedPlane : MonoBehaviour 
    {
        #if UNITY_EDITOR
        [UnityEditor.MenuItem("GameObject/3D Object" + MD_Debug.PACKAGENAME + "Two-Sided Plane")]
        #endif
        public static GameObject Generate() 
        {
            Transform transform = new GameObject("Plane_TwoSided").transform;
            Vector3[] Vertices = new Vector3[]
            {
                new Vector3(-1, 0, -1), new Vector3(-1, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 0, -1),
                new Vector3(-1, 0, -1), new Vector3(-1, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 0, -1),
            };

            int[] Triangles = new int[]
            {
                0, 1, 2,
                2, 3, 0,

                4, 6, 5,
                6, 4, 7
            };

            Vector2[] UV = new Vector2[]
            {
                new Vector2(0, 0),
                new Vector2(0, 1),
                new Vector2(1, 1),
                new Vector2(1, 0),

                new Vector2(0, 0),
                new Vector2(0, 1),
                new Vector2(1, 1),
                new Vector2(1, 0)
            };

            transform.gameObject.AddComponent<MeshFilter>();
            transform.gameObject.AddComponent<MeshRenderer>();

            Mesh myMesh = new Mesh();
            myMesh.vertices = Vertices;
            myMesh.triangles = Triangles;
            myMesh.uv = UV;
            myMesh.RecalculateNormals();
            myMesh.RecalculateBounds();
            myMesh.RecalculateTangents();

            myMesh.name = "TwoSidedPlane" + Random.Range(1, 999).ToString();
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












