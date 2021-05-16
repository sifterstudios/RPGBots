using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using MD_Plugin;
#endif

namespace MD_Plugin
{
    [ExecuteInEditMode]
    public class MD_AdvancedShapes : MonoBehaviour
    {
        public enum ShapeType { Plane, Box, Cone, Torus, Sphere, Tube };
        public ShapeType shapeType;
        public bool ppUpdateEveryFrame = false;

        #region Plane
        public float G_Plane_length = 1f;
        public float G_Plane_width = 1f;
        [Range(2, 200)]
        public int G_Plane_resX = 2;
        #endregion

        #region Box
        public float G_Box_length = 1f;
        public float G_Box_height = 1f;
        public float G_Box_width = 1f;
        #endregion

        #region Cone
        public float G_Cone_height = 1f;
        public float G_Cone_botRadius = .25f;
        public float G_Cone_topRadius = .05f;
        [Range(3, 220)]
        public int G_Cone_verticalSides = 18;
        private const int G_Cone_horizontalSides = 1;
        #endregion

        #region Torus
        public float G_Torus_radius0 = 1f;
        public float G_Torus_radius1 = .3f;
        [Range(3, 220)]
        public int G_Torus_segments = 24;
        [Range(3, 220)]
        public int G_Torus_sides = 18;
        #endregion

        #region Sphere
        public float G_Sphere_radius0 = 1f;
        [Range(3, 100)]
        public int G_Sphere_segments = 12;
        [Range(3, 100)]
        public int G_Sphere_stack = 18;
        [Range(0, 360)]
        public int G_Sphere_sliceMax = 360;
        [Range(0, 180)]
        public int G_Sphere_verticalMax = 180;
        #endregion

        #region Tube
        public float G_Tube_radius0 = 1f;
        public float G_Tube_radius1 = 0.5f;
        public float G_Tube_height = 1;
        [Range(3, 100)]
        public int G_Tube_segments = 12;
        #endregion

        private MeshFilter meshFilter;

#if UNITY_EDITOR
        [MenuItem("GameObject/3D Object" + MD_Debug.PACKAGENAME + "Advanced/Advanced Shapes")]
#endif
        public static GameObject Generate()
        {
            GameObject newGM = new GameObject();
            newGM.AddComponent<MeshRenderer>();
            newGM.AddComponent<MeshFilter>();
            newGM.AddComponent<MD_AdvancedShapes>().AdvancedShapes_Generate(ShapeType.Plane);
            newGM.name = "Advanced_Shape";
            Shader shad = null;
            shad = Shader.Find("Standard");

            Material mat = new Material(shad);
            newGM.GetComponent<Renderer>().material = mat;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Selection.activeGameObject = newGM.transform.gameObject;
                newGM.transform.position = SceneView.lastActiveSceneView.camera.transform.position + SceneView.lastActiveSceneView.camera.transform.forward * 3f;
            }
#endif
            return newGM;
        }

        private void Awake()
        {
            if (!MD_MeshProEditor.MeshProEditor_Utilities.util_CheckFellowModifiers(gameObject, this))
            {
                DestroyImmediate(this);
                return;
            }

            meshFilter = GetComponent<MeshFilter>();

            if (!meshFilter)
            {
                MD_Debug.Debug(this, "There's no mesh filter", MD_Debug.DebugType.Error);
                DestroyImmediate(this);
            }
        }

        private void Update()
        {
            if (!ppUpdateEveryFrame) return;

            AdvancedShapes_Generate(shapeType);
        }

        /// <summary>
        /// Generate specific shape
        /// </summary>
        public void AdvancedShapes_Generate(ShapeType shape)
        {
            shapeType = shape;
            switch (shape)
            {
                case ShapeType.Plane: GeneratePlane(G_Plane_length, G_Plane_width, G_Plane_resX); break;
                case ShapeType.Box: GenerateBox(G_Box_length, G_Box_width, G_Box_height); break;
                case ShapeType.Cone: GenerateCone(G_Cone_height, G_Cone_botRadius, G_Cone_topRadius, G_Cone_verticalSides, G_Cone_horizontalSides); break;
                case ShapeType.Torus: GenerateTorus(G_Torus_radius0, G_Torus_radius1, G_Torus_segments, G_Torus_sides); break;
                case ShapeType.Sphere: GenerateSphere(G_Sphere_radius0, G_Sphere_segments, G_Sphere_stack, G_Sphere_sliceMax, G_Sphere_verticalMax); break;
                case ShapeType.Tube: GenerateTube(G_Tube_radius0, G_Tube_radius1, G_Tube_height, G_Tube_segments); break;
            }
        }

        /// <summary>
        /// Switch current shape to specific shape ID
        /// </summary>
        public void AdvancedShapes_SwitchShape(int shapeID)
        {
            AdvancedShapes_Generate((ShapeType)shapeID);
        }

        private void GeneratePlane(float length, float width, int res)
        {
            Mesh mesh = new Mesh();
            int resZ = res;

            length = Mathf.Clamp(length, 1, length);
            width = Mathf.Clamp(width, 1, width);
            res = Mathf.Clamp(res, 1, res);

            Vector3[] vertices = new Vector3[res * resZ];
            for (int z = 0; z < resZ; z++)
            {
                float zPos = ((float)z / (resZ - 1) - .5f) * length;
                for (int x = 0; x < res; x++)
                {
                    float xPos = ((float)x / (res - 1) - .5f) * width;
                    vertices[x + z * res] = new Vector3(xPos, 0f, zPos);
                }
            }

            Vector3[] normals = new Vector3[vertices.Length];
            for (int n = 0; n < normals.Length; n++)
                normals[n] = Vector3.up;

            Vector2[] uvs = new Vector2[vertices.Length];
            for (int v = 0; v < resZ; v++)
            {
                for (int u = 0; u < res; u++)
                    uvs[u + v * res] = new Vector2((float)u / (res - 1), (float)v / (resZ - 1));
            }

            int nbFaces = (res - 1) * (resZ - 1);
            int[] triangles = new int[nbFaces * 6];
            int t = 0;
            for (int face = 0; face < nbFaces; face++)
            {
                int i = face % (res - 1) + (face / (resZ - 1) * res);

                triangles[t++] = i + res;
                triangles[t++] = i + 1;
                triangles[t++] = i;

                triangles[t++] = i + res;
                triangles[t++] = i + res + 1;
                triangles[t++] = i + 1;
            }

            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            meshFilter.sharedMesh = mesh;
        }

        private void GenerateBox(float Length, float Width, float Height)
        {
            Mesh mesh = new Mesh();

            float s = 0.5f;
            Vector3 p0 = new Vector3(-s * Length, -s, s * Width);
            Vector3 p1 = new Vector3(s, -s, s * Width);
            Vector3 p2 = new Vector3(s, -s, -s);
            Vector3 p3 = new Vector3(-s * Length, -s, -s);

            Vector3 p4 = new Vector3(-s * Length, s * Height, s * Width);
            Vector3 p5 = new Vector3(s, s * Height, s * Width);
            Vector3 p6 = new Vector3(s, s * Height, -s);
            Vector3 p7 = new Vector3(-s * Length, s * Height, -s);

            Vector3[] vertices = new Vector3[]
            {
	            // Bottom
	            p0, p1, p2, p3,
 
	            // Left
	            p7, p4, p0, p3,
 
	            // Front
	            p4, p5, p1, p0,
 
	            // Back
	            p6, p7, p3, p2,
 
	            // Right
	            p5, p6, p2, p1,
 
	            // Top
	            p7, p6, p5, p4
            };

            Vector3 up = Vector3.up;
            Vector3 down = Vector3.down;
            Vector3 front = Vector3.forward;
            Vector3 back = Vector3.back;
            Vector3 left = Vector3.left;
            Vector3 right = Vector3.right;

            Vector3[] normals = new Vector3[]
            {
	            // Bottom
	            down, down, down, down,
 
	            // Left
	            left, left, left, left,
 
	            // Front
	            front, front, front, front,
 
	            // Back
	            back, back, back, back,
 
	            // Right
	            right, right, right, right,
 
	            // Top
	            up, up, up, up
            };

            Vector2 _00 = new Vector2(0f, 0f);
            Vector2 _10 = new Vector2(1f, 0f);
            Vector2 _01 = new Vector2(0f, 1f);
            Vector2 _11 = new Vector2(1f, 1f);

            Vector2[] uvs = new Vector2[]
            {
	            // Bottom
	            _11, _01, _00, _10,
 
	            // Left
	            _11, _01, _00, _10,
 
	            // Front
	            _11, _01, _00, _10,
 
	            // Back
	            _11, _01, _00, _10,
 
	            // Right
	            _11, _01, _00, _10,
 
	            // Top
	            _11, _01, _00, _10,
            };

            int[] triangles = new int[]
            {
	            // Bottom
	            3, 1, 0,
                3, 2, 1,			
 
	            // Left
	            3 + 4 * 1, 1 + 4 * 1, 0 + 4 * 1,
                3 + 4 * 1, 2 + 4 * 1, 1 + 4 * 1,
 
	            // Front
	            3 + 4 * 2, 1 + 4 * 2, 0 + 4 * 2,
                3 + 4 * 2, 2 + 4 * 2, 1 + 4 * 2,
 
	            // Back
	            3 + 4 * 3, 1 + 4 * 3, 0 + 4 * 3,
                3 + 4 * 3, 2 + 4 * 3, 1 + 4 * 3,
 
	            // Right
	            3 + 4 * 4, 1 + 4 * 4, 0 + 4 * 4,
                3 + 4 * 4, 2 + 4 * 4, 1 + 4 * 4,
 
	            // Top
	            3 + 4 * 5, 1 + 4 * 5, 0 + 4 * 5,
                3 + 4 * 5, 2 + 4 * 5, 1 + 4 * 5,
            };

            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            meshFilter.sharedMesh = mesh;
        }

        private void GenerateCone(float Height, float BotRadius, float TopRadius, int VerticalSides, int HorizontalSides)
        {
            int nbVerticesCap = G_Cone_verticalSides + 1;
            Mesh mesh = new Mesh();

            Height = Mathf.Clamp(Height, 0.01f, Height);
            BotRadius = Mathf.Clamp(BotRadius, .01f, BotRadius);
            TopRadius = Mathf.Clamp(TopRadius, .01f, TopRadius);
            VerticalSides = Mathf.Clamp(VerticalSides, 1, VerticalSides);
            HorizontalSides = Mathf.Clamp(HorizontalSides, 1, HorizontalSides);

            Vector3[] vertices = new Vector3[nbVerticesCap + nbVerticesCap + VerticalSides * HorizontalSides * 2 + 2];
            int vert = 0;
            float _2pi = Mathf.PI * 2f;

            vertices[vert++] = new Vector3(0f, 0f, 0f);
            while (vert <= VerticalSides)
            {
                float rad = (float)vert / VerticalSides * _2pi;
                vertices[vert] = new Vector3(Mathf.Cos(rad) * BotRadius, 0f, Mathf.Sin(rad) * BotRadius);
                vert++;
            }

            vertices[vert++] = new Vector3(0f, Height, 0f);
            while (vert <= VerticalSides * 2 + 1)
            {
                float rad = (float)(vert - VerticalSides - 1) / VerticalSides * _2pi;
                vertices[vert] = new Vector3(Mathf.Cos(rad) * TopRadius, Height, Mathf.Sin(rad) * TopRadius);
                vert++;
            }

            int v = 0;
            while (vert <= vertices.Length - 4)
            {
                float rad = (float)v / VerticalSides * _2pi;
                vertices[vert] = new Vector3(Mathf.Cos(rad) * TopRadius, Height, Mathf.Sin(rad) * TopRadius);
                vertices[vert + 1] = new Vector3(Mathf.Cos(rad) * BotRadius, 0, Mathf.Sin(rad) * BotRadius);
                vert += 2;
                v++;
            }
            vertices[vert] = vertices[VerticalSides * 2 + 2];
            vertices[vert + 1] = vertices[VerticalSides * 2 + 3];

            Vector3[] normals = new Vector3[vertices.Length];
            vert = 0;

            while (vert <= VerticalSides)
                normals[vert++] = Vector3.down;

            while (vert <= VerticalSides * 2 + 1)
                normals[vert++] = Vector3.up;

            v = 0;
            while (vert <= vertices.Length - 4)
            {
                float rad = (float)v / VerticalSides * _2pi;
                float cos = Mathf.Cos(rad);
                float sin = Mathf.Sin(rad);

                normals[vert] = new Vector3(cos, 0f, sin);
                normals[vert + 1] = normals[vert];

                vert += 2;
                v++;
            }
            normals[vert] = normals[VerticalSides * 2 + 2];
            normals[vert + 1] = normals[VerticalSides * 2 + 3];

            Vector2[] uvs = new Vector2[vertices.Length];

            // Bottom cap
            int u = 0;
            uvs[u++] = new Vector2(0.5f, 0.5f);
            while (u <= VerticalSides)
            {
                float rad = (float)u / VerticalSides * _2pi;
                uvs[u] = new Vector2(Mathf.Cos(rad) * .5f + .5f, Mathf.Sin(rad) * .5f + .5f);
                u++;
            }

            // Top cap
            uvs[u++] = new Vector2(0.5f, 0.5f);
            while (u <= VerticalSides * 2 + 1)
            {
                float rad = (float)u / VerticalSides * _2pi;
                uvs[u] = new Vector2(Mathf.Cos(rad) * .5f + .5f, Mathf.Sin(rad) * .5f + .5f);
                u++;
            }

            // Sides
            int u_sides = 0;
            while (u <= uvs.Length - 4)
            {
                float t = (float)u_sides / VerticalSides;
                uvs[u] = new Vector3(t, 1f);
                uvs[u + 1] = new Vector3(t, 0f);
                u += 2;
                u_sides++;
            }
            uvs[u] = new Vector2(1f, 1f);
            uvs[u + 1] = new Vector2(1f, 0f);

            int nbTriangles = VerticalSides + VerticalSides + VerticalSides * 2;
            int[] triangles = new int[nbTriangles * 3 + 3];

            // Bottom cap
            int tri = 0;
            int i = 0;
            while (tri < VerticalSides - 1)
            {
                triangles[i] = 0;
                triangles[i + 1] = tri + 1;
                triangles[i + 2] = tri + 2;
                tri++;
                i += 3;
            }
            triangles[i] = 0;
            triangles[i + 1] = tri + 1;
            triangles[i + 2] = 1;
            tri++;
            i += 3;

            // Top cap
            //tri++;
            while (tri < VerticalSides * 2)
            {
                triangles[i] = tri + 2;
                triangles[i + 1] = tri + 1;
                triangles[i + 2] = nbVerticesCap;
                tri++;
                i += 3;
            }

            triangles[i] = nbVerticesCap + 1;
            triangles[i + 1] = tri + 1;
            triangles[i + 2] = nbVerticesCap;
            tri++;
            i += 3;
            tri++;

            // Sides
            while (tri <= nbTriangles)
            {
                triangles[i] = tri + 2;
                triangles[i + 1] = tri + 1;
                triangles[i + 2] = tri + 0;
                tri++;
                i += 3;

                triangles[i] = tri + 1;
                triangles[i + 1] = tri + 2;
                triangles[i + 2] = tri + 0;
                tri++;
                i += 3;
            }

            //this one was pretty hard!

            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            meshFilter.sharedMesh = mesh;
        }

        private void GenerateTorus(float Radius1, float Radius2, int Segments, int Sides)
        {
            Mesh mesh = new Mesh();

            Radius1 = Mathf.Clamp(Radius1, 0.01f, Radius1);
            Radius2 = Mathf.Clamp(Radius2, 0.01f, Radius2);
            Segments = Mathf.Clamp(Segments, 1, Segments);
            Sides = Mathf.Clamp(Sides, 1, Sides);

            Vector3[] vertices = new Vector3[(Segments + 1) * (Sides + 1)];
            float _2pi = Mathf.PI * 2f;
            for (int seg = 0; seg <= Segments; seg++)
            {
                int currSeg = seg == Segments ? 0 : seg;

                float t1 = (float)currSeg / Segments * _2pi;
                Vector3 r1 = new Vector3(Mathf.Cos(t1) * Radius1, 0f, Mathf.Sin(t1) * Radius1);

                for (int side = 0; side <= Sides; side++)
                {
                    int currSide = side == Sides ? 0 : side;

                    float t2 = (float)currSide / Sides * _2pi;
                    Vector3 r2 = Quaternion.AngleAxis(-t1 * Mathf.Rad2Deg, Vector3.up) * new Vector3(Mathf.Sin(t2) * Radius2, Mathf.Cos(t2) * Radius2);

                    vertices[side + seg * (Sides + 1)] = r1 + r2;
                }
            }

            Vector3[] normals = new Vector3[vertices.Length];
            for (int seg = 0; seg <= Segments; seg++)
            {
                int currSeg = seg == Segments ? 0 : seg;

                float t1 = (float)currSeg / Segments * _2pi;
                Vector3 r1 = new Vector3(Mathf.Cos(t1) * Radius1, 0f, Mathf.Sin(t1) * Radius1);

                for (int side = 0; side <= Sides; side++)
                    normals[side + seg * (Sides + 1)] = (vertices[side + seg * (Sides + 1)] - r1).normalized;
            }

            Vector2[] uvs = new Vector2[vertices.Length];
            for (int seg = 0; seg <= Segments; seg++)
                for (int side = 0; side <= Sides; side++)
                    uvs[side + seg * (Sides + 1)] = new Vector2((float)seg / Segments, (float)side / Sides);

            int nbFaces = vertices.Length;
            int nbTriangles = nbFaces * 2;
            int nbIndexes = nbTriangles * 3;
            int[] triangles = new int[nbIndexes];

            int i = 0;
            for (int seg = 0; seg <= Segments; seg++)
            {
                for (int side = 0; side <= Sides - 1; side++)
                {
                    int current = side + seg * (Sides + 1);
                    int next = side + (seg < (Segments) ? (seg + 1) * (Sides + 1) : 0);

                    if (i < triangles.Length - 6)
                    {
                        triangles[i++] = current;
                        triangles[i++] = next;
                        triangles[i++] = next + 1;

                        triangles[i++] = current;
                        triangles[i++] = next + 1;
                        triangles[i++] = current + 1;
                    }
                }
            }

            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            meshFilter.sharedMesh = mesh;
        }

        private void GenerateSphere(float radius, int segments, int stacks, float slicesMax, float verticalMax)
        {
            Mesh m = new Mesh();

            if (segments < 3) segments = 3;
            if (stacks < 3) stacks = 3;

            int vertexCapacity = (segments + 1) * (stacks + 1);

            List<Vector3> vertices = new List<Vector3>(vertexCapacity);
            List<int> indices = new List<int>(segments * stacks * 6);
            List<Vector3> normals = new List<Vector3>(vertexCapacity);
            List<Vector2> uvs = new List<Vector2>(vertexCapacity);

            float stacksAngle = verticalMax * Mathf.Deg2Rad;
            float slicesAngle = slicesMax * Mathf.Deg2Rad;
            float phiStep = stacksAngle / stacks;
            float thetaStep = slicesAngle / segments;

            for (int i = 0; i <= stacks; ++i)
            {
                float phi = i * phiStep;

                for (int j = 0; j <= segments; ++j)
                {
                    float theta = j * thetaStep;

                    Vector3 position = new Vector3(radius * Mathf.Sin(phi) * Mathf.Cos(theta), radius * Mathf.Cos(phi), radius * Mathf.Sin(phi) * Mathf.Sin(theta));
                    vertices.Add(position);
                    normals.Add(position.normalized);
                    uvs.Add(new Vector2(theta / slicesAngle, 1f - phi / stacksAngle));
                }
            }

            int ringVertexCount = segments + 1;
            for (int i = 0; i < stacks; ++i)
            {
                for (int j = 0; j < segments; ++j)
                {
                    indices.Add(i * ringVertexCount + j);
                    indices.Add(i * ringVertexCount + j + 1);
                    indices.Add((i + 1) * ringVertexCount + j);

                    indices.Add((i + 1) * ringVertexCount + j);
                    indices.Add(i * ringVertexCount + j + 1);
                    indices.Add((i + 1) * ringVertexCount + j + 1);
                }
            }

            m.vertices = vertices.ToArray();
            m.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);
            m.normals = normals.ToArray();
            m.uv = uvs.ToArray();
            m.RecalculateBounds();
            meshFilter.sharedMesh = m;
        }

        private void GenerateTube(float oRadius, float iRadius, float height, int sides)
        {
            Mesh m = new Mesh();

            if (sides < 3) sides = 3;
            if (iRadius > oRadius) iRadius = oRadius;

            List<Vector3> vertices = new List<Vector3>();
            List<int> indices = new List<int>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();

            float deltaTheta = Mathf.PI * 2.0f / sides;
            float radiusDiff = oRadius - iRadius;
            float total = 2f * (height + radiusDiff);
            float v1 = height / total;
            float v2 = v1 + radiusDiff / total;
            float v3 = v2 + v1;

            for (int i = 0; i <= sides; ++i)
            {
                float theta = i * deltaTheta;

                float x = Mathf.Cos(theta);
                float y = Mathf.Sin(theta);

                Vector3 position = new Vector3(oRadius * x, 0f, oRadius * y);
                Vector3 normal = position.normalized;
                Vector2 uv = new Vector2((float)i / sides, 0f);

                vertices.Add(position);
                vertices.Add(position);
                normals.Add(Vector3.down);
                normals.Add(normal);
                uvs.Add(new Vector2(uv.x, 1f));
                uvs.Add(uv);

                position.y = height;
                uv.y = v1;

                vertices.Add(position);
                vertices.Add(position);
                normals.Add(normal);
                normals.Add(Vector3.up);
                uvs.Add(uv);
                uvs.Add(uv);

                position = new Vector3(iRadius * x, height, iRadius * y);
                normal = -normal;
                uv.y = v2;

                vertices.Add(position);
                vertices.Add(position);
                normals.Add(Vector3.up);
                normals.Add(normal);
                uvs.Add(uv);
                uvs.Add(uv);

                position.y = 0f;
                uv.y = v3;

                vertices.Add(position);
                vertices.Add(position);
                normals.Add(normal);
                normals.Add(Vector3.down);
                uvs.Add(uv);
                uvs.Add(uv);
            }

            int i0, i1, i2, i3;

            for (int i = 0; i < sides; ++i)
            {
                // Front
                i0 = i * 8 + 1;
                i1 = i0 + 1;
                i2 = (i + 1) * 8 + 1;
                i3 = i2 + 1;

                indices.Add(i0);
                indices.Add(i1);
                indices.Add(i3);
                indices.Add(i0);
                indices.Add(i3);
                indices.Add(i2);

                // Top
                i0 = i1 + 1;
                i1 = i0 + 1;
                i2 = i3 + 1;
                i3 = i2 + 1;

                indices.Add(i0);
                indices.Add(i1);
                indices.Add(i3);
                indices.Add(i0);
                indices.Add(i3);
                indices.Add(i2);

                // Back
                i0 = i1 + 1;
                i1 = i0 + 1;
                i2 = i3 + 1;
                i3 = i2 + 1;

                indices.Add(i0);
                indices.Add(i1);
                indices.Add(i3);
                indices.Add(i0);
                indices.Add(i3);
                indices.Add(i2);

                // Bottom
                i0 = i1 + 1;
                i1 = i * 8;
                i2 = i3 + 1;
                i3 = (i + 1) * 8;

                indices.Add(i0);
                indices.Add(i1);
                indices.Add(i3);
                indices.Add(i0);
                indices.Add(i3);
                indices.Add(i2);
            }

            m.vertices = vertices.ToArray();
            m.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);
            m.normals = normals.ToArray();
            m.uv = uvs.ToArray();
            m.RecalculateBounds();
            meshFilter.sharedMesh = m;
        }
    }
}

#if UNITY_EDITOR
namespace MD_PluginEditor
{
    [CustomEditor(typeof(MD_AdvancedShapes))]
    public class MD_AdvancedShapes_Editor : MD_EditorUtilities
    {
        private MD_AdvancedShapes asp;

        private void OnEnable()
        {
            asp = (MD_AdvancedShapes)target;
        }

        public override void OnInspectorGUI()
        {
            ps();
            pv();
            pv();
            ppDrawProperty("shapeType", "Shape Type");
            pve();
            EditorGUI.indentLevel++;
            ppDrawProperty("ppUpdateEveryFrame", "Update Every Frame");
            EditorGUI.indentLevel--;
            if (!asp.ppUpdateEveryFrame)
            {
                if (pb("Update Mesh"))
                    asp.AdvancedShapes_Generate(asp.shapeType);
            }
            pve();
            ps();
            pv();
            switch (asp.shapeType)
            {
                case MD_AdvancedShapes.ShapeType.Plane:
                    ppDrawProperty("G_Plane_length", "Length");
                    ppDrawProperty("G_Plane_width", "Width");
                    ps(5);
                    ppDrawProperty("G_Plane_resX", "Resolution");
                    break;

                case MD_AdvancedShapes.ShapeType.Box:
                    ppDrawProperty("G_Box_length", "Length");
                    ppDrawProperty("G_Box_height", "Height");
                    ppDrawProperty("G_Box_width", "Width");
                    break;

                case MD_AdvancedShapes.ShapeType.Cone:
                    ppDrawProperty("G_Cone_height", "Height");
                    ps(5);
                    ppDrawProperty("G_Cone_botRadius", "Radius - Bottom");
                    ppDrawProperty("G_Cone_topRadius", "Radius - Top");
                    ps(5);
                    ppDrawProperty("G_Cone_verticalSides", "Sides Count");
                    break;

                case MD_AdvancedShapes.ShapeType.Torus:
                    ppDrawProperty("G_Torus_radius0", "Radius - Length");
                    ppDrawProperty("G_Torus_radius1", "Radius - Size");
                    ps(5);
                    ppDrawProperty("G_Torus_segments", "Count Of Segments");
                    ppDrawProperty("G_Torus_sides", "Count Of Sides");
                    break;

                case MD_AdvancedShapes.ShapeType.Sphere:
                    ppDrawProperty("G_Sphere_radius0", "Radius");
                    ppDrawProperty("G_Sphere_segments", "Segments");
                    ppDrawProperty("G_Sphere_stack", "Stack");
                    ps(5);
                    ppDrawProperty("G_Sphere_sliceMax", "Horizontal Slice");
                    ppDrawProperty("G_Sphere_verticalMax", "Vertical Slice");
                    break;

                case MD_AdvancedShapes.ShapeType.Tube:
                    ppDrawProperty("G_Tube_radius0", "Outer Radius");
                    ppDrawProperty("G_Tube_radius1", "Inner Radius");
                    ppDrawProperty("G_Tube_height", "Height");
                    ps(5);
                    ppDrawProperty("G_Tube_segments", "Segments Count");
                    break;
            }
            pve();
            ps(15);
            ppAddMeshColliderRefresher(asp.gameObject);
            ppBackToMeshEditor(asp);
            ps();
            if (target != null) serializedObject.Update();
        }
    }
}
#endif
