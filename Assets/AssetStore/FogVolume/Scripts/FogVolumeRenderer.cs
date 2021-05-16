using UnityEngine;
using UnityEngine.Profiling;
using FogVolumeUtilities;
using UnityEngine.XR;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class FogVolumeRenderer : MonoBehaviour
{
    bool ShowCamerasBack = true;
    public bool ShowCamera;
    //public bool DestroyOnDisable;
    private int m_screenWidth = 0;
    private int m_screenHeight = 0;
    public string FogVolumeResolution;
    //	public bool RenderableInSceneView = true;
    public enum BlendMode
    {
        PremultipliedTransparency = (int)UnityEngine.Rendering.BlendMode.One,
        TraditionalTransparency = (int)UnityEngine.Rendering.BlendMode.SrcAlpha,
    };
    RenderTextureFormat rt_DepthFormat;
    public BlendMode _BlendMode = BlendMode.PremultipliedTransparency;
    public bool GenerateDepth = true;
    RenderTexture RT_FogVolume, RT_FogVolumeR;
    [SerializeField]
    [Range(0, 8)]
    public int _Downsample = 1;
    public void setDownsample(int val) { _Downsample = val; }
    //public bool useRectangularStereoRT = false;

    public bool _showBilateralEdge = false;


    public bool showBilateralEdge
    {
        set
        {
            if (value != _showBilateralEdge)
                ShowBilateralEdge(value);
        }
        get
        {
            return _showBilateralEdge;
        }
    }
    public void ShowBilateralEdge(bool b)
    {

        _showBilateralEdge = b;
        if (bilateralMaterial)
        {

            if (showBilateralEdge)
                bilateralMaterial.EnableKeyword("VISUALIZE_EDGE");
            else
                bilateralMaterial.DisableKeyword("VISUALIZE_EDGE");
        }
    }

    [SerializeField]
    //public FogVolumeCamera.UpsampleMode USMode = FogVolumeCamera.UpsampleMode.DOWNSAMPLE_CHESSBOARD;
    [System.Serializable]
    public enum UpsampleMode
    {
        DOWNSAMPLE_MIN,
        DOWNSAMPLE_MAX,
        DOWNSAMPLE_CHESSBOARD
    };
    public enum UpsampleMaterialPass
    {
        DEPTH_DOWNSAMPLE = 0,
        BILATERAL_UPSAMPLE = 1
    };
    Material bilateralMaterial;

    public bool _useBilateralUpsampling = true;
    public bool useBilateralUpsampling
    {
        get { return _useBilateralUpsampling; }
        set
        {
            if (_useBilateralUpsampling != value)
                SetUseBilateralUpsampling(value);
        }
    }
    void SetUseBilateralUpsampling(bool b)
    {

        _useBilateralUpsampling = b /*&& BilateralUpsamplingEnabled()*/;//works on dx9
        if (_useBilateralUpsampling)
        {

            if (bilateralMaterial == null)
            {
                bilateralMaterial = new Material(Shader.Find("Hidden/Upsample"));
                if (bilateralMaterial == null)
                    Debug.Log("#ERROR# Hidden/Upsample");

                // refresh keywords
                UpdateBilateralDownsampleModeSwitch();
                ShowBilateralEdge(_showBilateralEdge);
            }
        }
        else
        {
            // release resources
            bilateralMaterial = null;
        }
    }
    // [SerializeField]
    public UpsampleMode _upsampleMode = UpsampleMode.DOWNSAMPLE_MAX;
    public UpsampleMode upsampleMode
    {

        set
        {
            if (value != _upsampleMode)
                SetUpsampleMode(value);
        }
        get
        {
            return _upsampleMode;
        }
    }
    void UpdateBilateralDownsampleModeSwitch()
    {
        if (bilateralMaterial != null)
        {
            switch (_upsampleMode)
            {
                case UpsampleMode.DOWNSAMPLE_MIN:
                    bilateralMaterial.EnableKeyword("DOWNSAMPLE_DEPTH_MODE_MIN");
                    bilateralMaterial.DisableKeyword("DOWNSAMPLE_DEPTH_MODE_MAX");
                    bilateralMaterial.DisableKeyword("DOWNSAMPLE_DEPTH_MODE_CHESSBOARD");
                    break;
                case UpsampleMode.DOWNSAMPLE_MAX:
                    bilateralMaterial.DisableKeyword("DOWNSAMPLE_DEPTH_MODE_MIN");
                    bilateralMaterial.EnableKeyword("DOWNSAMPLE_DEPTH_MODE_MAX");
                    bilateralMaterial.DisableKeyword("DOWNSAMPLE_DEPTH_MODE_CHESSBOARD");
                    break;
                case UpsampleMode.DOWNSAMPLE_CHESSBOARD:
                    bilateralMaterial.DisableKeyword("DOWNSAMPLE_DEPTH_MODE_MIN");
                    bilateralMaterial.DisableKeyword("DOWNSAMPLE_DEPTH_MODE_MAX");
                    bilateralMaterial.EnableKeyword("DOWNSAMPLE_DEPTH_MODE_CHESSBOARD");
                    break;
                default:
                    break;
            }
        }
    }
    void SetUpsampleMode(UpsampleMode value)
    {

        _upsampleMode = value;
        UpdateBilateralDownsampleModeSwitch();

    }

    Camera ThisCamera = null;
    RenderTexture RT_Depth, RT_DepthR;
    Shader depthShader = null;
    [HideInInspector]
    Camera _FogVolumeCamera;
    // [SerializeField]
    GameObject _FogVolumeCameraGO;
    [SerializeField]
    [Range(0, .01f)]
    public float upsampleDepthThreshold = 0.00187f;
    public bool HDR;
    public bool TAA = false;
    public FogVolumeTAA _TAA = null;
    private FogVolumePlaydeadTAA.VelocityBuffer _TAAvelocity = null;
    private FogVolumePlaydeadTAA.FrustumJitter _TAAjitter = null;
    //[HideInInspector]
    //public LayerMask DepthLayer = -1;
    [SerializeField]
    // [HideInInspector]
    //string _DepthLayersName = "Water";
    public int DepthLayer2 = 0;
    //public string DepthLayersName
    //{
    //    get { return _DepthLayersName; }
    //    set
    //    {
    //        if (_DepthLayersName != value)
    //            SetDepthLayer(value);
    //    }
    //}

    //void SetDepthLayer(string NewDepthLayersName)
    //{
    //    _DepthLayersName = NewDepthLayersName;
    //    DepthLayer = ThisCamera.cullingMask;
    //    DepthLayer &= ~(1 << LayerMask.NameToLayer(_DepthLayersName));
    //    //DepthLayer = LayerMask.NameToLayer(_DepthLayersName);
    //}

    //void OnValidate()
    //{
    //    SetDepthLayer(_DepthLayersName);
    //}
    public RenderTextureReadWrite GetRTReadWrite()
    {
        //return RenderTextureReadWrite.Default;
        return (ThisCamera.allowHDR) ? RenderTextureReadWrite.Default : RenderTextureReadWrite.Linear;

    }

    public RenderTextureFormat GetRTFormat()
    {
        return (ThisCamera.allowHDR == true) ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;
    }
    protected void GetRT(ref RenderTexture rt, int2 size, string name)
    {
        // Release existing one
        ReleaseRT(rt);
        rt = RenderTexture.GetTemporary(size.x, size.y, 0, GetRTFormat(), GetRTReadWrite());
        rt.filterMode = FilterMode.Bilinear;
        rt.name = name;
        rt.wrapMode = TextureWrapMode.Clamp;

    }
    public void ReleaseRT(RenderTexture rt)
    {
        if (rt != null)
        {
            RenderTexture.ReleaseTemporary(rt);
            rt = null;
        }
    }

    protected void Get_RT_Depth(ref RenderTexture rt, int2 size, string name)
    {
        // Release existing one
        ReleaseRT(rt);
        rt = RenderTexture.GetTemporary(size.x, size.y, 24, rt_DepthFormat);//3.4.2 Changed 16 with 24
        rt.filterMode = FilterMode.Bilinear;
        rt.name = name;
        rt.wrapMode = TextureWrapMode.Clamp;

    }
    void RenderDepth()
    {
        if (GenerateDepth && _FogVolumeCamera)
        {
            if (_TAAjitter)
            {
                //_TAA.enabled = false;
                _TAAjitter.patternScale = 0;
            }

            Profiler.BeginSample("FogVolume Depth");
            //Gimme scene depth

            // ThisCamera.cullingMask = _FogVolumeCamera.cullingMask;//Render the same than scene camera
            //3.2.1p2

            _FogVolumeCamera.cullingMask = DepthLayer2;
            #region Stereo
            if (ThisCamera.stereoEnabled)
            {

                Shader.EnableKeyword("FOG_VOLUME_STEREO_ON");
                //Left eye            

                if (ThisCamera.stereoTargetEye == StereoTargetEyeMask.Both || ThisCamera.stereoTargetEye == StereoTargetEyeMask.Left)
                {
                    _FogVolumeCamera.worldToCameraMatrix = ThisCamera.GetStereoViewMatrix(Camera.StereoscopicEye.Left);
                    _FogVolumeCamera.projectionMatrix = ThisCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);

                    Get_RT_Depth(ref RT_Depth, new int2(m_screenWidth, m_screenHeight), "RT_DepthLeft");

                    _FogVolumeCamera.targetTexture = RT_Depth;
                    _FogVolumeCamera.RenderWithShader(depthShader, "RenderType");


                }
                //Right eye            

                if (ThisCamera.stereoTargetEye == StereoTargetEyeMask.Both || ThisCamera.stereoTargetEye == StereoTargetEyeMask.Right)
                {
                    _FogVolumeCamera.worldToCameraMatrix = ThisCamera.GetStereoViewMatrix(Camera.StereoscopicEye.Right);
                    _FogVolumeCamera.projectionMatrix = ThisCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);

                    Get_RT_Depth(ref RT_DepthR, new int2(m_screenWidth, m_screenHeight), "RT_DepthRight");
                    _FogVolumeCamera.targetTexture = RT_DepthR;
                    _FogVolumeCamera.RenderWithShader(depthShader, "RenderType");


                    Shader.SetGlobalTexture("RT_DepthR", RT_DepthR);
                }
            }
            #endregion
            //MONO
            else
            {
                _FogVolumeCamera.projectionMatrix = ThisCamera.projectionMatrix;
                Shader.DisableKeyword("FOG_VOLUME_STEREO_ON");
                Get_RT_Depth(ref RT_Depth, new int2(m_screenWidth, m_screenHeight), "RT_Depth");

                _FogVolumeCamera.targetTexture = RT_Depth;

                _FogVolumeCamera.RenderWithShader(depthShader, "RenderType");

            }


            Shader.SetGlobalTexture("RT_Depth", RT_Depth);
            Profiler.EndSample();
        }

    }
    public RenderTexture[] lowProfileDepthRT;
    void ReleaseLowProfileDepthRT()
    {
        if (lowProfileDepthRT != null)
        {
            for (int i = 0; i < lowProfileDepthRT.Length; i++)
                RenderTexture.ReleaseTemporary(lowProfileDepthRT[i]);

            lowProfileDepthRT = null;
        }
    }
    public RenderTexture[] lowProfileDepthRRT;
    void ReleaseLowProfileDepthRRT()
    {
        if (lowProfileDepthRRT != null)
        {
            for (int i = 0; i < lowProfileDepthRRT.Length; i++)
                RenderTexture.ReleaseTemporary(lowProfileDepthRRT[i]);

            lowProfileDepthRRT = null;
        }
    }
    void RenderColor()
    {
        if (_TAA && _TAAjitter)
        {
            _TAA.enabled = TAA;
            _TAAvelocity.enabled = false;
            _TAAjitter.enabled = TAA;
            _TAAjitter.patternScale = 0.2f;
        }
        //Textured Fog
        _FogVolumeCamera.cullingMask = 1 << LayerMask.NameToLayer("FogVolume");//show Fog volume
        _FogVolumeCamera.cullingMask |= 1 << LayerMask.NameToLayer("FogVolumeShadowCaster");//show FogVolumeShadowCaster
        int2 resolution = new int2(m_screenWidth / _Downsample, m_screenHeight / _Downsample);
        FogVolumeResolution = resolution.x + " X " + resolution.y;

        if (ThisCamera.stereoEnabled)
        {

            Profiler.BeginSample("FogVolume Render Stereo");
            Shader.EnableKeyword("FOG_VOLUME_STEREO_ON");
            //Left eye            

            if (ThisCamera.stereoTargetEye == StereoTargetEyeMask.Both || ThisCamera.stereoTargetEye == StereoTargetEyeMask.Left)
            {
                _FogVolumeCamera.projectionMatrix = ThisCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
                _FogVolumeCamera.worldToCameraMatrix = ThisCamera.GetStereoViewMatrix(Camera.StereoscopicEye.Left);

                GetRT(ref RT_FogVolume, resolution, "RT_FogVolumeLeft");
                _FogVolumeCamera.targetTexture = RT_FogVolume;
                _FogVolumeCamera.Render();
            }

            //Right eye            

            if (ThisCamera.stereoTargetEye == StereoTargetEyeMask.Both || ThisCamera.stereoTargetEye == StereoTargetEyeMask.Right)
            {

                _FogVolumeCamera.projectionMatrix = ThisCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
                _FogVolumeCamera.worldToCameraMatrix = ThisCamera.GetStereoViewMatrix(Camera.StereoscopicEye.Right);

                GetRT(ref RT_FogVolumeR, resolution, "RT_FogVolumeRight");
                _FogVolumeCamera.targetTexture = RT_FogVolumeR;
                _FogVolumeCamera.Render();

            }
            Profiler.EndSample();
        }
        else
        {
            Profiler.BeginSample("FogVolume Render");
            Shader.DisableKeyword("FOG_VOLUME_STEREO_ON");
            _FogVolumeCamera.projectionMatrix = ThisCamera.projectionMatrix;
            GetRT(ref RT_FogVolume, resolution, "RT_FogVolume");
            _FogVolumeCamera.targetTexture = RT_FogVolume;
            _FogVolumeCamera.Render();
            Profiler.EndSample();
        }


        if (TAA)
            _TAA.TAA(ref RT_FogVolume);


        if (ThisCamera.stereoEnabled)
        {
            if (TAA)
                _TAA.TAA(ref RT_FogVolumeR);

        }


        if (useBilateralUpsampling && GenerateDepth)
        {

            //MONO
            #region BILATERAL_DEPTH_DOWNSAMPLE
            Profiler.BeginSample("FogVolume Upsample");
            // Compute downsampled depth-buffer for bilateral upsampling
            if (bilateralMaterial)
            {
                bilateralMaterial.SetInt("RightSide", 0);
                ReleaseLowProfileDepthRT();
                lowProfileDepthRT = new RenderTexture[_Downsample];

                for (int downsampleStep = 0; downsampleStep < _Downsample; downsampleStep++)
                {
                    int targetWidth = m_screenWidth / (downsampleStep + 1);
                    int targetHeight = m_screenHeight / (downsampleStep + 1);

                    int stepWidth = m_screenWidth / Mathf.Max(downsampleStep, 1);
                    int stepHeight = m_screenHeight / Mathf.Max(downsampleStep, 1);
                    Vector4 texelSize = new Vector4(1.0f / stepWidth, 1.0f / stepHeight, 0.0f, 0.0f);
                    bilateralMaterial.SetFloat("_UpsampleDepthThreshold", upsampleDepthThreshold);
                    bilateralMaterial.SetVector("_TexelSize", texelSize);

                    bilateralMaterial.SetTexture("_HiResDepthBuffer", RT_Depth);

                    lowProfileDepthRT[downsampleStep] =
                        RenderTexture.GetTemporary(targetWidth, targetHeight, 0, rt_DepthFormat, GetRTReadWrite());
                    lowProfileDepthRT[downsampleStep].name = "lowProfileDepthRT_" + downsampleStep;
                    Graphics.Blit(null, lowProfileDepthRT[downsampleStep], bilateralMaterial, (int)UpsampleMaterialPass.DEPTH_DOWNSAMPLE);
                }

                Shader.SetGlobalTexture("RT_Depth", lowProfileDepthRT[lowProfileDepthRT.Length - 1]);


            }


            #endregion

            #region BILATERAL_UPSAMPLE

            // Upsample convolution RT
            if (bilateralMaterial)
            {
                for (int downsampleStep = _Downsample - 1; downsampleStep >= 0; downsampleStep--)
                {
                    int targetWidth = m_screenWidth / Mathf.Max(downsampleStep, 1);
                    int targetHeight = m_screenHeight / Mathf.Max(downsampleStep, 1);

                    // compute Low-res texel size
                    int stepWidth = m_screenWidth / (downsampleStep + 1);
                    int stepHeight = m_screenHeight / (downsampleStep + 1);
                    Vector4 texelSize = new Vector4(1.0f / stepWidth, 1.0f / stepHeight, 0.0f, 0.0f);
                    bilateralMaterial.SetVector("_TexelSize", texelSize);
                    bilateralMaterial.SetVector("_InvdUV", new Vector4(RT_FogVolume.width, RT_FogVolume.height, 0, 0));
                    // High-res depth texture

                    bilateralMaterial.SetTexture("_HiResDepthBuffer", RT_Depth);
                    bilateralMaterial.SetTexture("_LowResDepthBuffer", lowProfileDepthRT[downsampleStep]);

                    bilateralMaterial.SetTexture("_LowResColor", RT_FogVolume);
                    RenderTexture newRT = RenderTexture.GetTemporary(targetWidth, targetHeight, 0, GetRTFormat(), GetRTReadWrite());
                    newRT.filterMode = FilterMode.Bilinear;
                    Graphics.Blit(null, newRT, bilateralMaterial, (int)UpsampleMaterialPass.BILATERAL_UPSAMPLE);

                    // Swap and release
                    RenderTexture swapRT = RT_FogVolume;
                    RT_FogVolume = newRT;
                    RenderTexture.ReleaseTemporary(swapRT);

                }


            }
            ReleaseLowProfileDepthRT();
            #endregion

            //Stereo right side
            if (ThisCamera.stereoEnabled)
            {

                #region BILATERAL_DEPTH_DOWNSAMPLE
                Profiler.BeginSample("FogVolume Upsample Right");
                // Compute downsampled depth-buffer for bilateral upsampling
                if (bilateralMaterial)
                {
                    bilateralMaterial.EnableKeyword("FOG_VOLUME_STEREO_ON");
                    bilateralMaterial.SetInt("RightSide", 1);
                    ReleaseLowProfileDepthRRT();
                    lowProfileDepthRRT = new RenderTexture[_Downsample];

                    for (int downsampleStep = 0; downsampleStep < _Downsample; downsampleStep++)
                    {
                        int targetWidth = m_screenWidth / (downsampleStep + 1);
                        int targetHeight = m_screenHeight / (downsampleStep + 1);

                        int stepWidth = m_screenWidth / Mathf.Max(downsampleStep, 1);
                        int stepHeight = m_screenHeight / Mathf.Max(downsampleStep, 1);
                        Vector4 texelSize = new Vector4(1.0f / stepWidth, 1.0f / stepHeight, 0.0f, 0.0f);
                        bilateralMaterial.SetFloat("_UpsampleDepthThreshold", upsampleDepthThreshold);
                        bilateralMaterial.SetVector("_TexelSize", texelSize);

                        bilateralMaterial.SetTexture("_HiResDepthBufferR", RT_DepthR);

                        lowProfileDepthRRT[downsampleStep] =
                            RenderTexture.GetTemporary(targetWidth, targetHeight, 0, rt_DepthFormat, GetRTReadWrite());
                        lowProfileDepthRRT[downsampleStep].name = "lowProfileDepthRRT_" + downsampleStep;
                        Graphics.Blit(null, lowProfileDepthRRT[downsampleStep], bilateralMaterial, (int)UpsampleMaterialPass.DEPTH_DOWNSAMPLE);
                    }

                    Shader.SetGlobalTexture("RT_DepthR", lowProfileDepthRRT[lowProfileDepthRRT.Length - 1]);


                }


                #endregion

                #region BILATERAL_UPSAMPLE

                // Upsample convolution RT
                if (bilateralMaterial)
                {
                    for (int downsampleStep = _Downsample - 1; downsampleStep >= 0; downsampleStep--)
                    {
                        int targetWidth = m_screenWidth / Mathf.Max(downsampleStep, 1);
                        int targetHeight = m_screenHeight / Mathf.Max(downsampleStep, 1);

                        // compute Low-res texel size
                        int stepWidth = m_screenWidth / (downsampleStep + 1);
                        int stepHeight = m_screenHeight / (downsampleStep + 1);
                        Vector4 texelSize = new Vector4(1.0f / stepWidth, 1.0f / stepHeight, 0.0f, 0.0f);
                        bilateralMaterial.SetVector("_TexelSize", texelSize);
                        bilateralMaterial.SetVector("_InvdUV", new Vector4(RT_FogVolumeR.width, RT_FogVolumeR.height, 0, 0));
                        // High-res depth texture

                        bilateralMaterial.SetTexture("_HiResDepthBufferR", RT_DepthR);
                        bilateralMaterial.SetTexture("_LowResDepthBufferR", lowProfileDepthRRT[downsampleStep]);

                        bilateralMaterial.SetTexture("_LowResColorR", RT_FogVolumeR);
                        RenderTexture newRT = RenderTexture.GetTemporary(targetWidth, targetHeight, 0, GetRTFormat(), GetRTReadWrite());
                        newRT.filterMode = FilterMode.Bilinear;
                        Graphics.Blit(null, newRT, bilateralMaterial, (int)UpsampleMaterialPass.BILATERAL_UPSAMPLE);

                        // Swap and release
                        RenderTexture swapRT = RT_FogVolumeR;
                        RT_FogVolumeR = newRT;
                        RenderTexture.ReleaseTemporary(swapRT);

                    }


                }
                ReleaseLowProfileDepthRRT();
                #endregion

            }
            else
                bilateralMaterial.DisableKeyword("FOG_VOLUME_STEREO_ON");
            Profiler.EndSample();
        }




        if (ThisCamera.stereoEnabled)
            Shader.SetGlobalTexture("RT_FogVolumeR", RT_FogVolumeR);
        Shader.SetGlobalTexture("RT_FogVolume", RT_FogVolume);


    }
    void CameraUpdateSharedProperties()
    {

        //AspectRatio = (float)m_screenWidth / m_screenHeight;

        if (_FogVolumeCamera)
        {
            _FogVolumeCamera.farClipPlane = ThisCamera.farClipPlane;
            _FogVolumeCamera.nearClipPlane = ThisCamera.nearClipPlane;

            _FogVolumeCamera.allowHDR = ThisCamera.allowHDR;

            if (_FogVolumeCamera.stereoEnabled == false)
            {
                _FogVolumeCamera.fieldOfView = ThisCamera.fieldOfView;
                _FogVolumeCamera.projectionMatrix = ThisCamera.projectionMatrix;//https://forum.unity.com/threads/fog-volume-3.225513/page-23#post-5236694
            }
        }
    }
    void TAASetup()
    {
        if (_Downsample > 1 && TAA)
        {
            if (_FogVolumeCameraGO.GetComponent<FogVolumeTAA>() == null)
                _FogVolumeCameraGO.AddComponent<FogVolumeTAA>();
            _TAA = _FogVolumeCameraGO.GetComponent<FogVolumeTAA>();
            _TAAvelocity = _FogVolumeCameraGO.GetComponent<FogVolumePlaydeadTAA.VelocityBuffer>();
            _TAAjitter = _FogVolumeCameraGO.GetComponent<FogVolumePlaydeadTAA.FrustumJitter>();
        }
    }

    void CreateFogCamera()
    {
        //if (_Downsample > 1)
        {
            _FogVolumeCameraGO = new GameObject();
            _FogVolumeCameraGO.transform.parent = gameObject.transform;

            _FogVolumeCameraGO.transform.localEulerAngles = Vector3.zero;
            _FogVolumeCameraGO.transform.localPosition = Vector3.zero;
            _FogVolumeCameraGO.name = "FogVolumeCamera";
            //_FogVolumeCamera = _FogVolumeCameraGO.AddComponent<FogVolumeCamera>();
            _FogVolumeCamera = _FogVolumeCameraGO.AddComponent<Camera>();

            _FogVolumeCamera.depth = -666;
            _FogVolumeCamera.clearFlags = CameraClearFlags.SolidColor;
            _FogVolumeCamera.backgroundColor = new Color(0, 0, 0, 0);
            // _FogVolumeCameraGO.hideFlags = HideFlags.HideInHierarchy;
            _FogVolumeCamera.enabled = false;
            _FogVolumeCamera.renderingPath = RenderingPath.Forward;

            _FogVolumeCamera.allowMSAA = false;
#if UNITY_EDITOR
            EditorExtension.ToggleInHierarchy(_FogVolumeCameraGO, ShowCamera);
#endif
        }
    }

    void FindFogCamera()
    {
        _FogVolumeCameraGO = GameObject.Find("FogVolumeCamera");
        if (_FogVolumeCameraGO)
        {
            _FogVolumeCamera = _FogVolumeCameraGO.GetComponent<Camera>();


            //_FogVolumeCameraGO.transform.parent = gameObject.transform;


        }
        //if (_FogVolumeCameraGO)
        //	DestroyImmediate(_FogVolumeCameraGO);//the RT is not created in VR on start. Resetting here for now
        if (_FogVolumeCameraGO == null)
            CreateFogCamera();
        //_FV_Baker = _FogVolumeCameraGO.GetComponent<FogVolumeCamera>();
        TAASetup();
    }
    Vector4 TexelSize = Vector4.zero;
    void TexelUpdate()
    {
        //if (_FogVolumeCamera.RT_FogVolume)
        {
            TexelSize.x = 1.0f / ThisCamera.pixelWidth;
            TexelSize.y = 1.0f / ThisCamera.pixelHeight;
            TexelSize.z = ThisCamera.pixelWidth;
            TexelSize.w = ThisCamera.pixelHeight;
            Shader.SetGlobalVector("RT_FogVolume_TexelSize", TexelSize);
        }
        //  print(TexelSize);
    }

    void OnEnable()
    {
        SetUseBilateralUpsampling(_useBilateralUpsampling);
        SetUpsampleMode(_upsampleMode);
        ShowBilateralEdge(_showBilateralEdge);
        ThisCamera = gameObject.GetComponent<Camera>();

        depthShader = Shader.Find("Hidden/Fog Volume/Depth");
        if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RFloat))
            rt_DepthFormat = RenderTextureFormat.RFloat;
        else
            rt_DepthFormat = RenderTextureFormat.DefaultHDR;
        if (depthShader == null) print("Hidden/Fog Volume/Depth #SHADER ERROR#");

        FindFogCamera();



        //clean old scenes . In 3.4 things are different. Find the existing camera and remove the unused scripts:
        Component[] components = _FogVolumeCameraGO.GetComponents<Component>();
        if (_FogVolumeCamera.GetComponent("FogVolumeCamera"))
        {
            print("Destroyed Old Camera");
            SafeDestroy(_FogVolumeCameraGO);
            CreateFogCamera();
        }

        for (int i = 0; i < components.Length; i++)
        {

            if (components[i] == null)
            {
                print("Destroyed Old Camera");
                SafeDestroy(_FogVolumeCameraGO);
                CreateFogCamera();
                break;
            }
        }

        //3.2.2 clean meshes. Some users pretends to add FogVolume.cs here ¬¬
        if (GetComponent<FogVolume>())
        {
            print("Don't add FogVolume here. Create a new one using the menu buttons and follow the instructions");
            DestroyImmediate(GetComponent<FogVolume>());
        }

        if (ThisCamera.GetComponent<MeshFilter>())
            DestroyImmediate(ThisCamera.GetComponent<MeshFilter>());

        if (ThisCamera.GetComponent<MeshRenderer>())
            DestroyImmediate(ThisCamera.GetComponent<MeshRenderer>());

        SurrogateMaterial = (Material)Resources.Load("Fog Volume Surrogate");
        //UpdateParams();

        if (DepthLayer2 == 0)
            DepthLayer2 = 1;
        //DepthLayer2 = ThisCamera.cullingMask;
    }
    void UpdateParams()
    {
        //_FV_Baker._Downsample = _Downsample;
        // if (_FogVolumeCamera && _Downsample > 1)
        {
            //   _FV_Baker.useBilateralUpsampling = BilateralUpsampling;


            if (useBilateralUpsampling && GenerateDepth)
            {
                //   _FV_Baker.upsampleMode = USMode;

                //   _FV_Baker.showBilateralEdge = ShowBilateralEdge;
                //   _FV_Baker.upsampleDepthThreshold = upsampleDepthThreshold;
            }

            if (GenerateDepth)
            {
                SurrogateMaterial.SetInt("_ztest", (int)UnityEngine.Rendering.CompareFunction.Always);
                //_FogVolumeCamera.DepthMask = instance.DepthLayer;
                // _FogVolumeCamera.DepthMask = ThisCamera.cullingMask;
                //_FogVolumeCamera.DepthMask &= ~(1 << DepthLayer2);
                DepthLayer2 &= ~(1 << LayerMask.NameToLayer("FogVolume"));//hide FogVolume
                DepthLayer2 &= ~(1 << LayerMask.NameToLayer("FogVolumeShadowCaster"));//hide FogVolumeShadowCaster
                DepthLayer2 &= ~(1 << LayerMask.NameToLayer("FogVolumeSurrogate"));//hide FogVolumeSurrogate
                DepthLayer2 &= ~(1 << LayerMask.NameToLayer("FogVolumeUniform"));//hide FogVolumeUniform
                DepthLayer2 &= ~(1 << LayerMask.NameToLayer("UI"));//hide UI
                //_FV_Baker.DepthMask = DepthLayer2;
            }
            else
                SurrogateMaterial.SetInt("_ztest", (int)UnityEngine.Rendering.CompareFunction.LessEqual);

            if (!_TAA)
                TAASetup();



#if UNITY_5_6_OR_NEWER
            HDR = ThisCamera.allowHDR;
#else
        HDR = ThisCamera.hdr;
#endif
        }
    }
    Material SurrogateMaterial;

    public bool SceneBlur = true;

    void OnPreRender()
    {
        m_screenWidth = ThisCamera.pixelWidth;
        m_screenHeight = ThisCamera.pixelHeight;

        //#if UNITY_EDITOR
        if (ThisCamera == null)
        {
            ThisCamera = gameObject.GetComponent<Camera>();
            Debug.Log("No camera found");
        }
        //#endif
        //#if UNITY_EDITOR
        //        // if destroyed...
        //        FindFogCamera();
        //#endif
        if (_FogVolumeCamera == null
            && _Downsample > 1)//3.2.1
            FindFogCamera();
        if (_Downsample == 1) SafeDestroy(_FogVolumeCameraGO);
        CameraUpdateSharedProperties();

        if (_Downsample > 1 && _FogVolumeCameraGO && this.isActiveAndEnabled)
        {

            ThisCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("FogVolume"));//hide FogVolume
            ThisCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("FogVolumeShadowCaster"));//hide FogVolumeShadowCaster
            FogVolumeResolution = m_screenWidth + " X " + m_screenHeight;
            ThisCamera.cullingMask |= 1 << LayerMask.NameToLayer("FogVolumeSurrogate");//show surrogate
        }
        else
        {
            
            ThisCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("FogVolumeSurrogate"));//hide surrogate

            ThisCamera.cullingMask |= 1 << LayerMask.NameToLayer("FogVolume");//show FogVolume
            ThisCamera.cullingMask |= 1 << LayerMask.NameToLayer("FogVolumeShadowCaster");//show FogVolumeShadowCaster


        }


        int InitialpixelLights = QualitySettings.pixelLightCount;
        ShadowQuality InitialShadows = QualitySettings.shadows;
        QualitySettings.pixelLightCount = 0;
        QualitySettings.shadows = ShadowQuality.Disable;

        //#if UNITY_EDITOR

        if (ThisCamera == null)
            ThisCamera = gameObject.GetComponent<Camera>();
        //#endif
        UpdateParams();
        SurrogateMaterial.SetInt("_SrcBlend", (int)_BlendMode);
        if (_Downsample > 1 && _FogVolumeCamera)
        {
            //SurrogateMaterial.SetInt("_SrcBlend", (int)_BlendMode);
            Shader.EnableKeyword("_FOG_LOWRES_RENDERER");
            Profiler.BeginSample("FogVolume Render");
            RenderDepth();
            RenderColor();
            Profiler.EndSample();


            //  TexelUpdate();

            Shader.DisableKeyword("_FOG_LOWRES_RENDERER");
        }
        else
        {
            Shader.DisableKeyword("_FOG_LOWRES_RENDERER");


        }

        QualitySettings.pixelLightCount = InitialpixelLights;
        QualitySettings.shadows = InitialShadows;
    }

    void Update()
    {

#if UNITY_EDITOR
        if (ShowCamerasBack != ShowCamera)
        {
            EditorExtension.ToggleInHierarchy(_FogVolumeCameraGO, ShowCamera);

            ShowCamerasBack = ShowCamera;
        }
#else
        ShowCamerasBack = ShowCamera;
#endif
    }
    void SafeDestroy(Object obj)
    {
        if (obj != null)
        {
            //print("Destroyed " + obj);
            obj = null;
            DestroyImmediate(obj);
        }
        obj = null;
    }
    void OnDisable()
    {
        //		Shader.DisableKeyword("RENDER_SCENE_VIEW");
        Shader.DisableKeyword("_FOG_LOWRES_RENDERER");

        ThisCamera.cullingMask |= (1 << LayerMask.NameToLayer("FogVolume"));
        ThisCamera.cullingMask |= 1 << LayerMask.NameToLayer("FogVolumeShadowCaster");
        ThisCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("FogVolumeSurrogate"));//hide surrogate

        SafeDestroy(_TAA);
        SafeDestroy(_TAAvelocity);
        SafeDestroy(_TAAjitter);
        SafeDestroy(_FogVolumeCameraGO);
        SafeDestroy(RT_FogVolume);
        SafeDestroy(RT_FogVolumeR);
        SafeDestroy(RT_Depth);
        SafeDestroy(RT_DepthR);

        //if (ThisCamera.depthTextureMode == DepthTextureMode.None)
        //    Debug.LogWarning("............ATTENTION, this camera is not generating the required Depth for Fog Volume. " +
        //        "Add -EnableDepthInForwardCamera.cs- to this camera");
    }
    //void OnGUI()
    //{ 
    //GUI.Label(new Rect(10, 40, 150, 100), ThisCamera.name);
    //GUI.Label(new Rect(10, 60, 150, 100), _FogVolumeCamera.name);
    //GUI.Label(new Rect(10, 80, 150, 100), Shader.IsKeywordEnabled("_FOG_LOWRES_RENDERER").ToString());

    //}
}
