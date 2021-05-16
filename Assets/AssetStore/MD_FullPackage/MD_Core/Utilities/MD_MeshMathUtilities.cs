using System;
using System.Collections.Generic;
using UnityEngine;

namespace MD_Plugin
{
    public class MD_MeshMathUtilities
    {
        public struct SculptingAttributes
        {
            public Vector3 worldPoint;
            public float radius;
            public float intens;

            public Vector3 transPos;
            public Quaternion transRot;
            public Vector3 transScale;

            public bool NotInitialized;
        }

        //--------------------------------------Laplacian Filter smoothing method-------------------------------------
        /// <summary>
        /// Smoothing class with essential smoothing functions of Laplacian filter
        /// </summary>
        public class smoothing_LaplacianFilter
        {
            /// <summary>
            /// Returns smooth calculation of Laplacian method [Calculates entire mesh]
            /// </summary>
            /// <param name="mesh">Input mesh</param>
            /// <param name="times">Smoothing multiplier</param>
            /// <returns>Returns calculated mesh</returns>
            public static Mesh LaplacianFilter(Mesh mesh, float intensity, bool recalculateNormals = true)
            {
                mesh.vertices = LaplacianFilter(mesh.vertices, mesh.triangles, intensity);
                if (recalculateNormals) mesh.RecalculateNormals();
                mesh.RecalculateBounds();
                return mesh;
            }

            /// <summary>
            /// Returns smooth calculation of Laplacian method [Calculates specific vertices]
            /// </summary>
            /// <param name="vertices">Input vertices</param>
            /// <param name="triangles">Input triangles</param>
            /// <param name="times">Smoothing multiplier</param>
            /// <returns>Returns array of calculated vertices</returns>
            public static Vector3[] LaplacianFilter(Vector3[] vertices, int[] triangles, float intensity)
            {
                var network = vConnectors.BuildNetwork(triangles);
                vertices = LaplacianFilter(network, vertices, intensity, new SculptingAttributes() { NotInitialized = true });
                return vertices;
            }

            /// <summary>
            /// Returns smooth calculation of Laplacian method [Calculates specific vertices] specifically for sculpting
            /// </summary>
            /// <returns>Returns array of calculated vertices</returns>
            public static Vector3[] LaplacianFilter(Vector3[] vertices, int[] triangles, float intensity, SculptingAttributes atr)
            {
                var network = vConnectors.BuildNetwork(triangles);
                vertices = LaplacianFilter(network, vertices, intensity, atr);
                return vertices;
            }


            private static Vector3[] LaplacianFilter(Dictionary<int, vConnectors> network, Vector3[] origin, float intensity, SculptingAttributes atr)
            {
                intensity = Mathf.Clamp01(intensity);
                Vector3[] vertices = new Vector3[origin.Length];
                for (int i = 0, n = origin.Length; i < n; i++)
                {
                    if (!atr.NotInitialized)
                    {
                        Vector3 v0 = TransformPoint(atr.transPos, atr.transRot, atr.transScale, origin[i]);
                        Vector3 v1 = TransformPoint(atr.transPos, atr.transRot, atr.transScale, atr.worldPoint);
                        if (Vector3.Distance(v0, v1) > atr.radius)
                        {
                            vertices[i] = origin[i];
                            continue;
                        }
                    }

                    HashSet<int> connection = network[i].Connection;
                    Vector3 v = vertices[i];
                    foreach (int adj in connection) v += origin[adj];
                    vertices[i] = Vector3.Lerp(origin[i], (v / connection.Count), intensity); //Finalize weight calculation by dividing the total count of connections
                }
                return vertices;
            }

            public class vConnectors
            {
                public HashSet<int> Connection { get; }
                public vConnectors()
                {
                    this.Connection = new HashSet<int>();
                }

                public void Connect(int to)
                {
                    Connection.Add(to);
                }
                public static Dictionary<int, vConnectors> BuildNetwork(int[] triangles)
                {
                    var table = new Dictionary<int, vConnectors>();
                    for (int i = 0, n = triangles.Length; i < n; i += 3)
                    {
                        int a = triangles[i], b = triangles[i + 1], c = triangles[i + 2];
                        if (!table.ContainsKey(a)) table.Add(a, new vConnectors());
                        if (!table.ContainsKey(b)) table.Add(b, new vConnectors());
                        if (!table.ContainsKey(c)) table.Add(c, new vConnectors());
                        table[a].Connect(b); table[a].Connect(c);
                        table[b].Connect(a); table[b].Connect(c);
                        table[c].Connect(a); table[c].Connect(b);
                    }
                    return table;
                }
            }
        }

        //--------------------------------------HC Filter smoothing method--------------------------------------------
        /// <summary>
        /// Smoothing class with essential smoothing functions of Humphrey's Class [HC] filter
        /// </summary>
        public class smoothing_HCFilter
        {
            /// <summary>
            /// Returns smooth calculation of HC Filter method [Calculates vertices] specifically for sculpting
            /// </summary>
            /// <returns>Returns calculated vertices</returns>
            public static Vector3[] HCFilter(Vector3[] inVerts, int[] inTris, SculptingAttributes atr, float alpha = 0.8f, float beta = 0.94f)
            {
                Vector3[] originalVerts = new Vector3[inVerts.Length];
                Vector3[] workingVerts = GetWeightedVertices(inVerts, inTris, atr);

                for (int i = 0; i < workingVerts.Length; i++)
                {
                    originalVerts[i].x = workingVerts[i].x - (alpha * inVerts[i].x + (1 - alpha) * inVerts[i].x);
                    originalVerts[i].y = workingVerts[i].y - (alpha * inVerts[i].y + (1 - alpha) * inVerts[i].y);
                    originalVerts[i].z = workingVerts[i].z - (alpha * inVerts[i].z + (1 - alpha) * inVerts[i].z);
                }

                float dx;
                float dy;
                float dz;
                for (int j = 0; j < originalVerts.Length; j++)
                {
                    Vector3 v0 = TransformPoint(atr.transPos, atr.transRot, atr.transScale, originalVerts[j]);
                    Vector3 v1 = TransformPoint(atr.transPos, atr.transRot, atr.transScale, atr.worldPoint);
                    if (Vector3.Distance(v0, v1) > atr.radius)
                        continue;

                    List<int> AdjIndex = findAdjTris(inVerts, inTris, inVerts[j], atr);

                    dx = 0.0f;
                    dy = 0.0f;
                    dz = 0.0f;

                    for (int k = 0; k < AdjIndex.Count; k++)
                    {
                        dx += originalVerts[AdjIndex[k]].x;
                        dy += originalVerts[AdjIndex[k]].y;
                        dz += originalVerts[AdjIndex[k]].z;
                    }

                    workingVerts[j].x -= beta * originalVerts[j].x + ((1 - beta) / AdjIndex.Count) * dx;
                    workingVerts[j].y -= beta * originalVerts[j].y + ((1 - beta) / AdjIndex.Count) * dy;
                    workingVerts[j].z -= beta * originalVerts[j].z + ((1 - beta) / AdjIndex.Count) * dz;
                }
                return workingVerts;
            }

            /// <summary>
            /// Returns smooth calculation of HC Filter method [Calculates vertices]
            /// </summary>
            /// <returns>Returns calculated vertices</returns>
            public static Vector3[] HCFilter(Vector3[] inVerts, int[] inTris, float alpha = 0.8f, float beta = 0.94f)
            {
                Vector3[] originalVerts = new Vector3[inVerts.Length];
                Vector3[] workingVerts = GetWeightedVertices(inVerts, inTris, new SculptingAttributes() { NotInitialized = true });

                for (int i = 0; i < workingVerts.Length; i++)
                {
                    originalVerts[i].x = workingVerts[i].x - (alpha * inVerts[i].x + (1 - alpha) * inVerts[i].x);
                    originalVerts[i].y = workingVerts[i].y - (alpha * inVerts[i].y + (1 - alpha) * inVerts[i].y);
                    originalVerts[i].z = workingVerts[i].z - (alpha * inVerts[i].z + (1 - alpha) * inVerts[i].z);
                }

                float dx;
                float dy;
                float dz;
                for (int j = 0; j < originalVerts.Length; j++)
                {
                    List<int> AdjIndex = findAdjTris(inVerts, inTris, inVerts[j], new SculptingAttributes() { NotInitialized = true });

                    dx = 0.0f;
                    dy = 0.0f;
                    dz = 0.0f;

                    for (int k = 0; k < AdjIndex.Count; k++)
                    {
                        dx += originalVerts[AdjIndex[k]].x;
                        dy += originalVerts[AdjIndex[k]].y;
                        dz += originalVerts[AdjIndex[k]].z;
                    }

                    workingVerts[j].x -= beta * originalVerts[j].x + ((1 - beta) / AdjIndex.Count) * dx;
                    workingVerts[j].y -= beta * originalVerts[j].y + ((1 - beta) / AdjIndex.Count) * dy;
                    workingVerts[j].z -= beta * originalVerts[j].z + ((1 - beta) / AdjIndex.Count) * dz;
                }
                return workingVerts;
            }

            private static Vector3[] GetWeightedVertices(Vector3[] sv, int[] t, SculptingAttributes atr)
            {
                Vector3[] verts = new Vector3[sv.Length];

                float dx;
                float dy;
                float dz;

                for (int vi = 0; vi < sv.Length; vi++)
                {
                    if (!atr.NotInitialized)
                    {
                        Vector3 v0 = TransformPoint(atr.transPos, atr.transRot, atr.transScale, sv[vi]);
                        Vector3 v1 = TransformPoint(atr.transPos, atr.transRot, atr.transScale, atr.worldPoint);
                        if (Vector3.Distance(v0, v1) > atr.radius)
                        { verts[vi] = sv[vi]; continue; }
                    }

                    List<Vector3> adjVerts = findAdjVerts(sv, t, sv[vi],atr);

                    if (adjVerts.Count == 0) continue;

                    dx = 0.0f;
                    dy = 0.0f;
                    dz = 0.0f;

                    for (int j = 0; j < adjVerts.Count; j++)
                    {
                        dx += adjVerts[j].x;
                        dy += adjVerts[j].y;
                        dz += adjVerts[j].z;
                    }

                    verts[vi].x = dx / adjVerts.Count;
                    verts[vi].y = dy / adjVerts.Count;
                    verts[vi].z = dz / adjVerts.Count;
                }

                return verts;
            }

            private static List<Vector3> findAdjVerts(Vector3[] v, int[] t, Vector3 vertex, SculptingAttributes atr)
            {
                List<Vector3> Vertex = new List<Vector3>();
                HashSet<int> FaceCreator = new HashSet<int>();
                int FaceLength = 0;

                for (int i = 0; i < v.Length; i++)
                {
                    if (!atr.NotInitialized)
                    {
                        Vector3 v0 = TransformPoint(atr.transPos, atr.transRot, atr.transScale, v[i]);
                        Vector3 v1 = TransformPoint(atr.transPos, atr.transRot, atr.transScale, atr.worldPoint);
                        if (Vector3.Distance(v0, v1) > atr.radius)
                        { continue; }
                    }

                    if (Mathf.Approximately(vertex.x, v[i].x) &&
                        Mathf.Approximately(vertex.y, v[i].y) &&
                        Mathf.Approximately(vertex.z, v[i].z))
                    {
                        int v1;
                        int v2;
                        bool marker;

                        for (int k = 0; k < t.Length; k = k + 3)
                        {
                            if (FaceCreator.Contains(k) == false)
                            {
                                v1 = 0;
                                v2 = 0;
                                marker = false;

                                if (i == t[k])
                                {
                                    v1 = t[k + 1];
                                    v2 = t[k + 2];
                                    marker = true;
                                }

                                if (i == t[k + 1])
                                {
                                    v1 = t[k];
                                    v2 = t[k + 2];
                                    marker = true;
                                }

                                if (i == t[k + 2])
                                {
                                    v1 = t[k];
                                    v2 = t[k + 1];
                                    marker = true;
                                }

                                FaceLength++;
                                if (marker)
                                {
                                    FaceCreator.Add(k);
                                    if (vertExist(Vertex, v[v1]) == false) Vertex.Add(v[v1]);
                                    if (vertExist(Vertex, v[v2]) == false) Vertex.Add(v[v2]);
                                }
                            }
                        }
                    }
                }

                return Vertex;
            }
            private static List<int> findAdjTris(Vector3[] v, int[] t, Vector3 vertex, SculptingAttributes atr)
            {
                List<int> AdjIndex = new List<int>();
                List<Vector3> AdjVertex = new List<Vector3>();
                HashSet<int> AdjFace = new HashSet<int>();
                int FaceLength = 0;

                for (int i = 0; i < v.Length; i++)
                {
                    if (!atr.NotInitialized)
                    {
                        Vector3 v0 = TransformPoint(atr.transPos, atr.transRot, atr.transScale, v[i]);
                        Vector3 v1 = TransformPoint(atr.transPos, atr.transRot, atr.transScale, atr.worldPoint);
                        if (Vector3.Distance(v0, v1) > atr.radius)
                        { continue; }
                    }

                    if (Mathf.Approximately(vertex.x, v[i].x) &&
                        Mathf.Approximately(vertex.y, v[i].y) &&
                        Mathf.Approximately(vertex.z, v[i].z))
                    {
                        int v1;
                        int v2;
                        bool marker;

                        for (int k = 0; k < t.Length; k = k + 3)
                            if (AdjFace.Contains(k) == false)
                            {
                                v1 = 0;
                                v2 = 0;
                                marker = false;

                                if (i == t[k])
                                {
                                    v1 = t[k + 1];
                                    v2 = t[k + 2];
                                    marker = true;
                                }

                                if (i == t[k + 1])
                                {
                                    v1 = t[k];
                                    v2 = t[k + 2];
                                    marker = true;
                                }

                                if (i == t[k + 2])
                                {
                                    v1 = t[k];
                                    v2 = t[k + 1];
                                    marker = true;
                                }

                                FaceLength++;
                                if (marker)
                                {
                                    AdjFace.Add(k);

                                    if (vertExist(AdjVertex, v[v1]) == false)
                                    {
                                        AdjVertex.Add(v[v1]);
                                        AdjIndex.Add(v1);
                                    }

                                    if (vertExist(AdjVertex, v[v2]) == false)
                                    {
                                        AdjVertex.Add(v[v2]);
                                        AdjIndex.Add(v2);
                                    }
                                }
                            }
                    }
                }

                return AdjIndex;
            }

            private static bool vertExist(List<Vector3> adjVert, Vector3 v)
            {
                for (int i = 0; i < adjVert.Count; i++)
                {
                    if (Mathf.Approximately(adjVert[i].x, v.x) && Mathf.Approximately(adjVert[i].y, v.y) && Mathf.Approximately(adjVert[i].z, v.z)) return true;
                }
                return false;
            }
        }

        //--------------------------------------Mesh Subdivision-----------------------------------------------------
        /// <summary>
        /// Mesh subdivision main class
        /// </summary>
        public class mesh_Subdivision
        {
            private static List<Vector3> vertices;
            private static List<Vector3> normals;
            private static List<Color> colors;
            private static List<Vector2> uv;
            private static List<Vector2> uv2;
            private static List<Vector2> uv3;

            private static List<int> indices;
            private static Dictionary<uint, int> newVectices;

            /// <summary>
            /// Main subdivision function. Recommended subdivision levels = 0, 2, 3, 4, 6, 8, 9, 12, 16, 18, 24.
            /// </summary>
            public static void Subdivide(Mesh mesh, int level)
            {
                if (level < 2) return;

                while (level > 1)
                {
                    while (level % 3 == 0)
                    {
                        Mode_Subdivide2(mesh);
                        level /= 3;
                    }
                    while (level % 2 == 0)
                    {
                        Mode_Subdivide(mesh);
                        level /= 2;
                    }
                    if (level > 3) level++;
                }
            }

            private static void Clean()
            {
                vertices = null;
                normals = null;
                colors = null;
                uv = null;
                uv2 = null;
                uv3 = null;
                indices = null;
            }
            private static void InitArrays(Mesh mesh)
            {
                vertices = new List<Vector3>(mesh.vertices);
                normals = new List<Vector3>(mesh.normals);
                colors = new List<Color>(mesh.colors);
                uv = new List<Vector2>(mesh.uv);
                uv2 = new List<Vector2>(mesh.uv2);
                uv3 = new List<Vector2>(mesh.uv3);
                indices = new List<int>();
            }

            private static int GetNewVertex4(int i1, int i2)
            {
                int newIndex = vertices.Count;
                uint t1 = ((uint)i1 << 16) | (uint)i2;
                uint t2 = ((uint)i2 << 16) | (uint)i1;
                if (newVectices.ContainsKey(t2))
                    return newVectices[t2];
                if (newVectices.ContainsKey(t1))
                    return newVectices[t1];

                newVectices.Add(t1, newIndex);

                vertices.Add((vertices[i1] + vertices[i2]) * 0.5f);
                if (normals.Count > 0)
                    normals.Add((normals[i1] + normals[i2]).normalized);
                if (colors.Count > 0)
                    colors.Add((colors[i1] + colors[i2]) * 0.5f);
                if (uv.Count > 0)
                    uv.Add((uv[i1] + uv[i2]) * 0.5f);
                if (uv2.Count > 0)
                    uv2.Add((uv2[i1] + uv2[i2]) * 0.5f);
                if (uv3.Count > 0)
                    uv3.Add((uv3[i1] + uv3[i2]) * 0.5f);

                return newIndex;
            }

            private static void Mode_Subdivide(Mesh mesh)
            {
                newVectices = new Dictionary<uint, int>();

                InitArrays(mesh);

                int[] triangles = mesh.triangles;
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    int i1 = triangles[i + 0];
                    int i2 = triangles[i + 1];
                    int i3 = triangles[i + 2];

                    int a = GetNewVertex4(i1, i2);
                    int b = GetNewVertex4(i2, i3);
                    int c = GetNewVertex4(i3, i1);
                    indices.Add(i1); indices.Add(a); indices.Add(c);
                    indices.Add(i2); indices.Add(b); indices.Add(a);
                    indices.Add(i3); indices.Add(c); indices.Add(b);
                    indices.Add(a); indices.Add(b); indices.Add(c); // center triangle
                }
                mesh.vertices = vertices.ToArray();
                if (normals.Count > 0)
                    mesh.normals = normals.ToArray();
                if (colors.Count > 0)
                    mesh.colors = colors.ToArray();
                if (uv.Count > 0)
                    mesh.uv = uv.ToArray();
                if (uv2.Count > 0)
                    mesh.uv2 = uv2.ToArray();
                if (uv3.Count > 0)
                    mesh.uv3 = uv3.ToArray();

                mesh.triangles = indices.ToArray();
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
                mesh.RecalculateTangents();
                Clean();
            }

            private static int GetVert(int i1, int i2, int i3)
            {
                int newIndex = vertices.Count;

                if (i3 == i1 || i3 == i2)
                {
                    uint t1 = ((uint)i1 << 16) | (uint)i2;
                    if (newVectices.ContainsKey(t1))
                        return newVectices[t1];
                    newVectices.Add(t1, newIndex);
                }

                vertices.Add((vertices[i1] + vertices[i2] + vertices[i3]) / 3.0f);
                if (normals.Count > 0)
                    normals.Add((normals[i1] + normals[i2] + normals[i3]).normalized);
                if (colors.Count > 0)
                    colors.Add((colors[i1] + colors[i2] + colors[i3]) / 3.0f);
                if (uv.Count > 0)
                    uv.Add((uv[i1] + uv[i2] + uv[i3]) / 3.0f);
                if (uv2.Count > 0)
                    uv2.Add((uv2[i1] + uv2[i2] + uv2[i3]) / 3.0f);
                if (uv3.Count > 0)
                    uv3.Add((uv3[i1] + uv3[i2] + uv3[i3]) / 3.0f);
                return newIndex;
            }

            private static void Mode_Subdivide2(Mesh mesh)
            {
                newVectices = new Dictionary<uint, int>();

                InitArrays(mesh);

                int[] triangles = mesh.triangles;
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    int i1 = triangles[i + 0];
                    int i2 = triangles[i + 1];
                    int i3 = triangles[i + 2];

                    int a1 = GetVert(i1, i2, i1);
                    int a2 = GetVert(i2, i1, i2);
                    int b1 = GetVert(i2, i3, i2);
                    int b2 = GetVert(i3, i2, i3);
                    int c1 = GetVert(i3, i1, i3);
                    int c2 = GetVert(i1, i3, i1);

                    int d = GetVert(i1, i2, i3);

                    indices.Add(i1); indices.Add(a1); indices.Add(c2);
                    indices.Add(i2); indices.Add(b1); indices.Add(a2);
                    indices.Add(i3); indices.Add(c1); indices.Add(b2);
                    indices.Add(d); indices.Add(a1); indices.Add(a2);
                    indices.Add(d); indices.Add(b1); indices.Add(b2);
                    indices.Add(d); indices.Add(c1); indices.Add(c2);
                    indices.Add(d); indices.Add(c2); indices.Add(a1);
                    indices.Add(d); indices.Add(a2); indices.Add(b1);
                    indices.Add(d); indices.Add(b2); indices.Add(c1);
                }

                mesh.vertices = vertices.ToArray();
                if (normals.Count > 0)
                    mesh.normals = normals.ToArray();
                if (colors.Count > 0)
                    mesh.colors = colors.ToArray();
                if (uv.Count > 0)
                    mesh.uv = uv.ToArray();
                if (uv2.Count > 0)
                    mesh.uv2 = uv2.ToArray();
                if (uv3.Count > 0)
                    mesh.uv3 = uv3.ToArray();

                mesh.triangles = indices.ToArray();
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
                mesh.RecalculateTangents();

                Clean();
            }
        }

        //--------------------------------------Procedural Perlin Noise-----------------------------------------------
        /// <summary>
        /// Procedural perlin noise
        /// </summary>
        public class Perlin
        {
            private const int B = 0x100;
            private const int BM = 0xff;
            private const int N = 0x1000;

            private int[] p = new int[B + B + 2];
            private float[,] g3 = new float[B + B + 2, 3];
            private float[,] g2 = new float[B + B + 2, 2];
            private float[] g1 = new float[B + B + 2];

            private float s_curve(float t)
            {
                return t * t * (3.0F - 2.0F * t);
            }

            private float lerp(float t, float a, float b)
            {
                return a + t * (b - a);
            }

            private void setup(float value, out int b0, out int b1, out float r0, out float r1)
            {
                float t = value + N;
                b0 = ((int)t) & BM;
                b1 = (b0 + 1) & BM;
                r0 = t - (int)t;
                r1 = r0 - 1.0F;
            }

            private float at2(float rx, float ry, float x, float y) { return rx * x + ry * y; }
            private float at3(float rx, float ry, float rz, float x, float y, float z) { return rx * x + ry * y + rz * z; }

            public float Noise(float arg)
            {
                int bx0, bx1;
                float rx0, rx1, sx, u, v;
                setup(arg, out bx0, out bx1, out rx0, out rx1);

                sx = s_curve(rx0);
                u = rx0 * g1[p[bx0]];
                v = rx1 * g1[p[bx1]];

                return (lerp(sx, u, v));
            }

            public float Noise(float x, float y)
            {
                int bx0, bx1, by0, by1, b00, b10, b01, b11;
                float rx0, rx1, ry0, ry1, sx, sy, a, b, u, v;
                int i, j;

                setup(x, out bx0, out bx1, out rx0, out rx1);
                setup(y, out by0, out by1, out ry0, out ry1);

                i = p[bx0];
                j = p[bx1];

                b00 = p[i + by0];
                b10 = p[j + by0];
                b01 = p[i + by1];
                b11 = p[j + by1];

                sx = s_curve(rx0);
                sy = s_curve(ry0);

                u = at2(rx0, ry0, g2[b00, 0], g2[b00, 1]);
                v = at2(rx1, ry0, g2[b10, 0], g2[b10, 1]);
                a = lerp(sx, u, v);

                u = at2(rx0, ry1, g2[b01, 0], g2[b01, 1]);
                v = at2(rx1, ry1, g2[b11, 0], g2[b11, 1]);
                b = lerp(sx, u, v);

                return lerp(sy, a, b);
            }

            public float Noise(float x, float y, float z)
            {
                int bx0, bx1, by0, by1, bz0, bz1, b00, b10, b01, b11;
                float rx0, rx1, ry0, ry1, rz0, rz1, sy, sz, a, b, c, d, t, u, v;
                int i, j;

                setup(x, out bx0, out bx1, out rx0, out rx1);
                setup(y, out by0, out by1, out ry0, out ry1);
                setup(z, out bz0, out bz1, out rz0, out rz1);

                i = p[bx0];
                j = p[bx1];

                b00 = p[i + by0];
                b10 = p[j + by0];
                b01 = p[i + by1];
                b11 = p[j + by1];

                t = s_curve(rx0);
                sy = s_curve(ry0);
                sz = s_curve(rz0);

                u = at3(rx0, ry0, rz0, g3[b00 + bz0, 0], g3[b00 + bz0, 1], g3[b00 + bz0, 2]);
                v = at3(rx1, ry0, rz0, g3[b10 + bz0, 0], g3[b10 + bz0, 1], g3[b10 + bz0, 2]);
                a = lerp(t, u, v);

                u = at3(rx0, ry1, rz0, g3[b01 + bz0, 0], g3[b01 + bz0, 1], g3[b01 + bz0, 2]);
                v = at3(rx1, ry1, rz0, g3[b11 + bz0, 0], g3[b11 + bz0, 1], g3[b11 + bz0, 2]);
                b = lerp(t, u, v);

                c = lerp(sy, a, b);

                u = at3(rx0, ry0, rz1, g3[b00 + bz1, 0], g3[b00 + bz1, 2], g3[b00 + bz1, 2]);
                v = at3(rx1, ry0, rz1, g3[b10 + bz1, 0], g3[b10 + bz1, 1], g3[b10 + bz1, 2]);
                a = lerp(t, u, v);

                u = at3(rx0, ry1, rz1, g3[b01 + bz1, 0], g3[b01 + bz1, 1], g3[b01 + bz1, 2]);
                v = at3(rx1, ry1, rz1, g3[b11 + bz1, 0], g3[b11 + bz1, 1], g3[b11 + bz1, 2]);
                b = lerp(t, u, v);

                d = lerp(sy, a, b);

                return lerp(sz, c, d);
            }

            private void normalize2(ref float x, ref float y)
            {
                float s;
                s = (float)Math.Sqrt(x * x + y * y);
                x = y / s;
                y = y / s;
            }

            private void normalize3(ref float x, ref float y, ref float z)
            {
                float s;
                s = (float)Math.Sqrt(x * x + y * y + z * z);
                x = y / s;
                y = y / s;
                z = z / s;
            }

            public Perlin()
            {
                int i, j, k;
                System.Random rnd = new System.Random();

                for (i = 0; i < B; i++)
                {
                    p[i] = i;
                    g1[i] = (float)(rnd.Next(B + B) - B) / B;

                    for (j = 0; j < 2; j++)
                        g2[i, j] = (float)(rnd.Next(B + B) - B) / B;
                    normalize2(ref g2[i, 0], ref g2[i, 1]);

                    for (j = 0; j < 3; j++)
                        g3[i, j] = (float)(rnd.Next(B + B) - B) / B;

                    normalize3(ref g3[i, 0], ref g3[i, 1], ref g3[i, 2]);
                }

                while (--i != 0)
                {
                    k = p[i];
                    p[i] = p[j = rnd.Next(B)];
                    p[j] = k;
                }

                for (i = 0; i < B + 2; i++)
                {
                    p[B + i] = p[i];
                    g1[B + i] = g1[i];
                    for (j = 0; j < 2; j++)
                        g2[B + i, j] = g2[i, j];
                    for (j = 0; j < 3; j++)
                        g3[B + i, j] = g3[i, j];
                }
            }
        }


        //--------------------------------------Transform & Matrix Utilities------------------------------------------

        /// <summary>
        /// Convert point from local space to world space
        /// </summary>
        public static Vector3 TransformPoint(Vector3 WorldPos, Quaternion WorldRot, Vector3 WorldScale, Vector3 Point)
		{
			var localToWorldMatrix = Matrix4x4.TRS(WorldPos, WorldRot, WorldScale);
			return localToWorldMatrix.MultiplyPoint3x4(Point);
		}
		/// <summary>
		/// Convert point from world space to local space
		/// </summary>
		public static Vector3 TransformPointInverse(Vector3 WorldPos, Quaternion WorldRot, Vector3 WorldScale, Vector3 Point)
		{
			var localToWorldMatrix = Matrix4x4.TRS(WorldPos, WorldRot, WorldScale).inverse;
			return localToWorldMatrix.MultiplyPoint3x4(Point);
		}
		/// <summary>
		/// Convert point direction from local space to world space
		/// </summary>
		public static Vector3 TransformDirection(Vector3 WorldPos, Quaternion WorldRot, Vector3 WorldScale, Vector3 Point)
		{
			var localToWorldMatrix = Matrix4x4.TRS(WorldPos, WorldRot, WorldScale);
			return localToWorldMatrix.MultiplyVector(Point);
		}
		/// <summary>
		/// Custom linear interpolation between A to B
		/// </summary>
		public static Vector3 CustomLerp(Vector3 a, Vector3 b, float t)
		{
			return ((1 - t) * a + t * b);
		}
	}
}