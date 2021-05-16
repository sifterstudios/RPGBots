using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using MD_Plugin;
#endif

namespace MD_Plugin
{
    /// <summary>
    /// MDM(Mesh Deformation Modifier): Surface Tracking
    /// Surface tracking for specific shader & material [requires Easy Mesh Tracker shader]
    /// Create tracks & footprints with a very quick & simple way
    /// </summary>
    [ExecuteInEditMode]
    [AddComponentMenu(MD_Debug.ORGANISATION + MD_Debug.PACKAGENAME + "Modifiers/Surface Tracking GPU")]
    public class MDM_SurfaceTracking : MonoBehaviour
    {
        public Camera ppVirtualTrackCamera;
        public RenderTexture ppTrackerSource;

        public bool notSet = true;

        public float ppViewSize = 5;
        public float ppVirtualCameraHeight = 0.2f;

        public bool ppFlip = false;

        private void Update()
        {
            if (!ppVirtualTrackCamera)
                return;

            ppVirtualTrackCamera.transform.localPosition = Vector3.zero + Vector3.up * ppVirtualCameraHeight;
            ppVirtualTrackCamera.transform.localRotation = !ppFlip ? Quaternion.LookRotation(Vector3.down) : Quaternion.LookRotation(Vector3.up);
            ppVirtualTrackCamera.transform.localScale = Vector3.one;

            ppVirtualTrackCamera.orthographicSize = ppViewSize;

            if (ppTrackerSource != null && ppVirtualTrackCamera.targetTexture == null)
                ppVirtualTrackCamera.targetTexture = ppTrackerSource;
        }

        /// <summary>
        /// Clear & reset current surface
        /// </summary>
        public void SurfTracking_ResetSurface()
        {
            if (ppTrackerSource == null) return;
            ppTrackerSource.Release();
        }
    }
}
#if UNITY_EDITOR
namespace MD_PluginEditor
{
    [CustomEditor(typeof(MDM_SurfaceTracking))]
    [CanEditMultipleObjects]
    public class MDM_LandscapeTracking_Editor : MD_EditorUtilities
    {
        private MDM_SurfaceTracking m;

        private void OnEnable()
        {
            m = (MDM_SurfaceTracking)target;
        }

        private string LT_LayerName;
        private bool LT_Choose;
        public override void OnInspectorGUI()
        {
            ps();
            phb("Object must contains MD_EasyMeshTracker shader");
            ps();

            if (m.notSet)
            {
                phb("The Tracking system is not yet set. Please write your custom Tracking Layer name that all trackable objects will have an access to", MessageType.Warning);
                if (!LT_Choose)
                {
                    LT_LayerName = GUILayout.TextField(LT_LayerName);
                    GUILayout.Label("Or you can choose an exists layer manually");
                }
                LT_Choose = GUILayout.Toggle(LT_Choose, "Choose layer manually");

                ps(10);

                if (pb("Apply Layer & Create All Requirements [RT, Camera]"))
                {
                    if (!LT_Choose)
                    {
                        if (string.IsNullOrEmpty(LT_LayerName))
                            EditorUtility.DisplayDialog("Error", "Please fill the layer name", "OK");
                        LT_Internal_CreateLayer(LT_LayerName);
                    }

                    LT_Internal_CreateCamera();
                    LT_Internal_CreateRT();
                    m.notSet = false;
                }
                FinishEditor();
                return;
            }
            else
            {
                if (!m.ppVirtualTrackCamera)
                {
                    phb("There is no Virtual Track Camera. Press the reset button or fill the required field!", MessageType.Warning);
                    ppDrawProperty("ppVirtualTrackCamera", "Virtual Track Camera");
                    FinishEditor();
                    return;
                }
                if (!m.ppTrackerSource)
                {
                    phb("There is no Tracking RT Source. Press the reset button or fill the required field!", MessageType.Warning);
                    ppDrawProperty("ppTrackerSource", "VT Tracker Source");
                    FinishEditor();
                    return;
                }

                GUI.color = Color.gray;
                ppDrawProperty("ppVirtualTrackCamera", "Virtual Track Camera", "");
                ppDrawProperty("ppTrackerSource", "VT Tracker Source", "");

                ps(10);

                GUI.color = Color.white;

                pv();
                ppDrawProperty("ppViewSize", "VT Camera View Size", "");
                ppDrawProperty("ppVirtualCameraHeight", "VT Camera Height", "");
                pve();

                if (pb("Flip"))
                {
                    m.ppFlip = !m.ppFlip;
                    if (m.ppFlip)
                    {
                        Vector3 v = m.transform.localScale;
                        v.x = -v.x;
                        v.z = -v.z;
                        m.transform.localScale = v;
                    }
                    else
                    {
                        Vector3 v = m.transform.localScale;
                        v.x = Mathf.Abs(v.x);
                        v.z = Mathf.Abs(v.z);
                        m.transform.localScale = v;
                    }
                }
                if (pb("Clean Tracker Source RT"))
                    m.ppTrackerSource.Release();
            }

            FinishEditor();
        }

        private void FinishEditor()
        {
            ps();

            ppAddMeshColliderRefresher(m.gameObject);
            ppBackToMeshEditor(m);
            if (target != null) serializedObject.Update();
        }

        private void LT_Internal_CreateCamera()
        {
            GameObject newCamera = new GameObject("TrackingCamera_" + m.name);
            Camera c = newCamera.AddComponent<Camera>();
            c.gameObject.layer = 30;
            c.transform.parent = m.transform;
            c.orthographic = true;
            c.clearFlags = CameraClearFlags.Nothing;
            c.nearClipPlane = 0.1f;
            c.farClipPlane = 200;
            c.depth = 0;
            c.allowHDR = false;
            c.allowMSAA = false;
            c.useOcclusionCulling = false;
            c.targetDisplay = 0;
            c.cullingMask = 1 << 30;
            m.ppVirtualTrackCamera = c;
            newCamera.transform.parent = m.transform;
        }
        private void LT_Internal_CreateRT()
        {
            RenderTexture rt = new RenderTexture(500, 500, 0, RenderTextureFormat.Depth);
            rt.antiAliasing = 1;
            rt.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
            AssetDatabase.CreateAsset(rt, "Assets/MDM_ST_" + m.name + "_RT.renderTexture");
            AssetDatabase.Refresh();
            if (m.gameObject.GetComponent<Renderer>() && m.gameObject.GetComponent<Renderer>().sharedMaterial && m.gameObject.GetComponent<Renderer>().sharedMaterial.HasProperty("_DispTex"))
                m.gameObject.GetComponent<Renderer>().sharedMaterial.SetTexture("_DispTex", rt);
            else
            {
                try
                {
                    Material mat = new Material(Shader.Find("Matej Vanco/Mesh Deformation Package/MD_EasyMeshTracker"));
                    m.gameObject.GetComponent<Renderer>().sharedMaterial = mat;
                    m.gameObject.GetComponent<Renderer>().sharedMaterial.SetTexture("_DispTex", rt);
                }
                catch
                {
                    MD_Debug.Debug(m, "Your object doesn't contain EasyMeshTracker shader. Create object with mesh filter and add material with shader Easy Mesh Tracker", MD_Debug.DebugType.Error);
                }
            }
            m.ppTrackerSource = rt;
        }
        private void LT_Internal_CreateLayer(string LayerName)
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);

            SerializedProperty layers = tagManager.FindProperty("layers");
            if (layers == null || !layers.isArray)
                return;

            SerializedProperty layerSP = layers.GetArrayElementAtIndex(30);
            layerSP.stringValue = LayerName;

            tagManager.ApplyModifiedProperties();
        }
    }
}
#endif

