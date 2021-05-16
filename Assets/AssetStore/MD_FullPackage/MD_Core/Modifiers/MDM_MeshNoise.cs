using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using MD_Plugin;
#endif

namespace MD_Plugin
{
    /// <summary>
    /// MDM(Mesh Deformation Modifier): Mesh Noise
    /// Physically-based perlin noise generator
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter))]
    [AddComponentMenu(MD_Debug.ORGANISATION + MD_Debug.PACKAGENAME + "Modifiers/Mesh Noise")]
    public class MDM_MeshNoise : MonoBehaviour
    {
        public bool ppUpdateEveryFrame = false;
        public bool ppAutoRecalculateNormals = true;
        public bool ppAutoRecalculateBounds = true;

        public enum NoiseType { GeneralNoice, VerticalNoise }
        public NoiseType ppNoiseType = NoiseType.GeneralNoice;

        public float ppMeshNoiseAmount = 1;
        public float ppMeshNoiseSpeed = 0.5f;
        public float ppMeshNoiseIntensity = 0.5f;

        private MD_MeshMathUtilities.Perlin ppNoise;
        [SerializeField] private Vector3[] originalVertices;
        [SerializeField] private Vector3[] workingVertices;

        [SerializeField] private MeshFilter meshFilter;

        private void Awake()
        {
            if (meshFilter != null) return;

            ppAutoRecalculateBounds = MD_GlobalPreferences.autoRecalcBounds;
            ppAutoRecalculateNormals = MD_GlobalPreferences.autoRecalcNormals;

            meshFilter = GetComponent<MeshFilter>();
            MD_MeshProEditor.MeshProEditor_Utilities.util_PrepareMeshDeformationModifier(this, meshFilter);

            originalVertices = meshFilter.sharedMesh.vertices;
            workingVertices = meshFilter.sharedMesh.vertices;
            ppNoise = new MD_MeshMathUtilities.Perlin();
        }

        private void Update()
        {
            if (!ppUpdateEveryFrame) return;

            if (ppNoiseType == NoiseType.VerticalNoise)
                MeshNoise_UpdateVerticalNoise();
            else if (ppNoiseType == NoiseType.GeneralNoice)
                MeshNoise_UpdateGeneralNoise();
        }

        /// <summary>
        /// Process vertical noise
        /// </summary>
        public void MeshNoise_UpdateVerticalNoise()
        {
            for (int i = 0; i < workingVertices.Length; i++)
            {
                float pX = (workingVertices[i].x * ppMeshNoiseAmount) + (Time.timeSinceLevelLoad * ppMeshNoiseSpeed);
                float pZ = (workingVertices[i].z * ppMeshNoiseAmount) + (Time.timeSinceLevelLoad * ppMeshNoiseSpeed);

                workingVertices[i].y = (Mathf.PerlinNoise(pX, pZ) - 0.5f) * ppMeshNoiseIntensity;
            }

            meshFilter.sharedMesh.vertices = workingVertices;
            if (ppAutoRecalculateNormals) meshFilter.sharedMesh.RecalculateNormals();
            if (ppAutoRecalculateBounds) meshFilter.sharedMesh.RecalculateBounds();
        }

        /// <summary>
        /// Process general noise
        /// </summary>
        public void MeshNoise_UpdateGeneralNoise()
        {
            if (ppNoise == null) ppNoise = new MD_MeshMathUtilities.Perlin();

            Vector3[] verts = new Vector3[originalVertices.Length];

            float timex = (Time.time * ppMeshNoiseSpeed) + 0.1365143f;
            float timey = (Time.time * ppMeshNoiseSpeed) + 1.21688f;
            float timez = (Time.time * ppMeshNoiseSpeed) + 2.5564f;

            for (var i = 0; i < verts.Length; i++)
            {
                Vector3 vertex = originalVertices[i];
                vertex.x += ppNoise.Noise(timex + vertex.x, timex + vertex.y, timex + vertex.z) * ppMeshNoiseIntensity;
                vertex.y += ppNoise.Noise(timey + vertex.x, timey + vertex.y, timey + vertex.z) * ppMeshNoiseIntensity;
                vertex.z += ppNoise.Noise(timez + vertex.x, timez + vertex.y, timez + vertex.z) * ppMeshNoiseIntensity;
                verts[i] = vertex;
            }

            meshFilter.sharedMesh.vertices = verts;
            if (ppAutoRecalculateNormals) meshFilter.sharedMesh.RecalculateNormals();
            if (ppAutoRecalculateBounds) meshFilter.sharedMesh.RecalculateBounds();
        }

        /// <summary>
        /// Change overall noise intensity
        /// </summary>
        public void MeshNoise_ChangeIntensity(UnityEngine.UI.Slider sliderEntry)
        {
            ppMeshNoiseIntensity = sliderEntry.value;
        }
    }
}

#if UNITY_EDITOR
namespace MD_PluginEditor
{
    [CustomEditor(typeof(MDM_MeshNoise))]
    [CanEditMultipleObjects]
    public class MDM_MeshNoise_Editor : MD_EditorUtilities
    {
        private MDM_MeshNoise m;

        private void OnEnable()
        {
            m = (MDM_MeshNoise)target;
        }

        public override void OnInspectorGUI()
        {
            ps();
            pv();
            ppDrawProperty("ppUpdateEveryFrame", "Update Every Frame");
            ppDrawProperty("ppAutoRecalculateNormals", "Auto Recalculate Normals");
            ppDrawProperty("ppAutoRecalculateBounds", "Auto Recalculate Bounds");
            pve();
            pv();
            ppDrawProperty("ppNoiseType", "Noise Type");
            EditorGUI.indentLevel++;
            ppDrawProperty("ppMeshNoiseIntensity", "Intensity");
            ppDrawProperty("ppMeshNoiseSpeed", "Speed");
            if (m.ppNoiseType == MDM_MeshNoise.NoiseType.VerticalNoise)
            {
                ppDrawProperty("ppMeshNoiseAmount", "Amount");

                if (!m.ppUpdateEveryFrame)
                    if (GUILayout.Button("Update Noise in Editor"))
                        m.MeshNoise_UpdateVerticalNoise();
            }
            else
            {
                if (!m.ppUpdateEveryFrame)
                    if (GUILayout.Button("Update Noise in Editor"))
                        m.MeshNoise_UpdateGeneralNoise();
            }
            EditorGUI.indentLevel--;
            pve();
            ps(15);
            ppAddMeshColliderRefresher(m.gameObject);
            ppBackToMeshEditor(m);
            if (target != null) serializedObject.Update();
        }
    }
}
#endif

