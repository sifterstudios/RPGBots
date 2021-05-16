using UnityEngine;
using FogVolumeUtilities;

[ExecuteInEditMode]
public class FogVolumeScreen : MonoBehaviour
{

    [Header("Scene blur")]
    [Range(1, 8)]
    public int Downsample = 8;
    [SerializeField]
    [Range(.001f, 15)]
    float _Falloff = 1;
    public int screenX
    {
        get
        {

            return SceneCamera.pixelWidth;

        }
    }
    float FOV_compensation = 0;
    public int screenY
    {
        get
        {
            return SceneCamera.pixelHeight;

        }
    }
    Shader _BlurShader = null;
    Camera UniformFogCamera;
    GameObject UniformFogCameraGO;

    [HideInInspector]
    public Camera SceneCamera;
    //[SerializeField]
    RenderTexture RT_FogVolumeConvolution;
    // [SerializeField]
    RenderTextureFormat RT_Format;
    [HideInInspector]
    public int FogVolumeLayer = -1;
    [SerializeField]
    [HideInInspector]
    string _FogVolumeLayerName = "FogVolumeUniform";
    public string FogVolumeLayerName
    {
        get { return _FogVolumeLayerName; }
        set
        {
            if (_FogVolumeLayerName != value)
                SetFogVolumeLayer(value);
        }
    }

    void SetFogVolumeLayer(string NewFogVolumeLayerName)
    {
        _FogVolumeLayerName = NewFogVolumeLayerName;
        FogVolumeLayer = LayerMask.NameToLayer(_FogVolumeLayerName);
    }
    void OnValidate()
    {
        SetFogVolumeLayer(_FogVolumeLayerName);
    }
    Material _BlurMaterial = null;
    Material BlurMaterial
    {
        get
        {
            if (_BlurMaterial == null)
            {
                _BlurMaterial = new Material(_BlurShader);
                _BlurMaterial.hideFlags = HideFlags.HideAndDontSave;
            }
            return _BlurMaterial;
        }
    }

    [Range(0, 10)]
    public int iterations = 3;
    // [Range(0, 1)]
    // public float _Dither = .8f;
    [Range(0.0f, 1.0f)]
    public float blurSpread = 0.6f;
    // [Range(0, 1)]
    //  public float ImageDistortion = 0;
    //BLOOM stuff

    public enum BlurType
    {
        Standard = 0,
        Sgx = 1,
    }


    [Header("Bloom")]
    [Range(1, 5)]
    public int _BloomDowsample = 8;
    [Range(0.0f, 1.5f)]
    public float threshold = 0.35f;
    [Range(0.0f, 10)]
    public float intensity = 2.5f;
    [Range(0, 1)]
    public float _Saturation = 1;
    [Range(0, 5f)]
    public float blurSize = 1;


    [Range(1, 10)]
    public int blurIterations = 4;

    BlurType blurType = BlurType.Standard;

    Shader fastBloomShader = null;
    Material _fastBloomMaterial = null;
    Material fastBloomMaterial
    {
        get
        {
            if (_fastBloomMaterial == null)
            {
                _fastBloomMaterial = new Material(fastBloomShader);
                _fastBloomMaterial.hideFlags = HideFlags.HideAndDontSave;
            }
            return _fastBloomMaterial;
        }
    }

    void CreateUniformFogCamera()
    {


        UniformFogCameraGO = GameObject.Find("Uniform Fog Volume Camera");

        if (UniformFogCameraGO == null)
        {
            UniformFogCameraGO = new GameObject();
            UniformFogCameraGO.name = "Uniform Fog Volume Camera";
            if (UniformFogCamera == null)
                UniformFogCamera = UniformFogCameraGO.AddComponent<Camera>();

            UniformFogCamera.backgroundColor = new Color(0, 0, 0, 0);
            UniformFogCamera.clearFlags = CameraClearFlags.SolidColor;
            UniformFogCamera.renderingPath = RenderingPath.Forward;
            UniformFogCamera.enabled = false;
            UniformFogCamera.farClipPlane = SceneCamera.farClipPlane;


#if UNITY_5_6_OR_NEWER
            UniformFogCamera.GetComponent<Camera>().allowMSAA = false;
#endif
        }
        else
        {
            UniformFogCamera = UniformFogCameraGO.GetComponent<Camera>();

        }


        //UniformFogCameraGO.hideFlags = HideFlags.None;
        UniformFogCameraGO.hideFlags = HideFlags.HideInHierarchy;


        initFOV = SceneCamera.fieldOfView;
    }
    float initFOV;

    void OnEnable()
    {
        SceneCamera = gameObject.GetComponent<Camera>();

        _BlurShader = Shader.Find("Hidden/FogVolumeDensityFilter");
        if (_BlurShader == null) print("Hidden/FogVolumeDensityFilter #SHADER ERROR#");

        fastBloomShader = Shader.Find("Hidden/FogVolumeBloom");
        if (fastBloomShader == null) print("Hidden/FogVolumeBloom #SHADER ERROR#");



        CreateUniformFogCamera();

    }

    protected void OnDisable()
    {
        if (_BlurMaterial)
        {
            DestroyImmediate(_BlurMaterial);
        }

        if (_fastBloomMaterial)
        {
            DestroyImmediate(_fastBloomMaterial);
        }


        //3.2.1 lets destroy it
        if (UniformFogCameraGO)
            DestroyImmediate(UniformFogCameraGO);
    }



    public void FourTapCone(RenderTexture source, RenderTexture dest, int iteration)
    {

        float off = 0.5f + iteration * blurSpread;
        Graphics.BlitMultiTap(source, dest, BlurMaterial,
                               new Vector2(-off, -off),
                               new Vector2(-off, off),
                               new Vector2(off, off),
                               new Vector2(off, -off)
            );
    }
    // Downsamples the texture to a quarter resolution.
    private void DownSample4x(RenderTexture source, RenderTexture dest)
    {
        float off = 1.0f;
        Graphics.BlitMultiTap(source, dest, BlurMaterial,
                               new Vector2(-off, -off),
                               new Vector2(-off, off),
                               new Vector2(off, off),
                               new Vector2(off, -off)
            );
    }
    public RenderTextureFormat GetRTFormat()
    {

#if UNITY_5_6_OR_NEWER
         RT_Format=(SceneCamera.allowHDR == true) ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;
        return RT_Format;
#else
        RT_Format = (SceneCamera.hdr == true) ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.ARGBHalf;
        return RT_Format;
#endif
        //return RT_Format;
    }
    public void ReleaseRT(RenderTexture rt)
    {
        if (rt != null)
        {
            RenderTexture.ReleaseTemporary(rt);
            rt = null;
        }
    }
    public RenderTextureReadWrite GetRTReadWrite()
    {
        //return RenderTextureReadWrite.Default;
#if UNITY_5_6_OR_NEWER
        return (SceneCamera.allowHDR) ? RenderTextureReadWrite.Default : RenderTextureReadWrite.Linear;
#else
        return (SceneCamera.hdr) ? RenderTextureReadWrite.Default : RenderTextureReadWrite.Linear;
#endif
    }
    protected void GetRT(ref RenderTexture rt, int2 size, string name)
    {

        // Release existing one
        ReleaseRT(rt);
        rt = RenderTexture.GetTemporary(size.x, size.y, 0, GetRTFormat(), GetRTReadWrite());
        rt.filterMode = FilterMode.Bilinear;
        rt.name = name;
        rt.wrapMode = TextureWrapMode.Repeat;

    }

    public void ConvolveFogVolume()
    {
        if (UniformFogCameraGO == null) CreateUniformFogCamera();

        int2 resolution = new int2(screenX, screenY);
        UniformFogCamera.projectionMatrix = SceneCamera.projectionMatrix;
        UniformFogCamera.transform.position = SceneCamera.transform.position;
        UniformFogCamera.transform.rotation = SceneCamera.transform.rotation;
        GetRT(ref RT_FogVolumeConvolution, resolution, "RT_FogVolumeConvolution");
        UniformFogCamera.targetTexture = RT_FogVolumeConvolution;

        UniformFogCamera.Render();
        Shader.SetGlobalTexture("RT_FogVolumeConvolution", RT_FogVolumeConvolution);
    }
    public bool SceneBloom = false;
    #region instance
    private static FogVolumeScreen _instance;
    public static FogVolumeScreen instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<FogVolumeScreen>();

            }

            return _instance;
        }
    }
    #endregion
    RenderTexture _source;

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        ConvolveFogVolume();
        GetRT(ref _source, new int2(Screen.width, Screen.height), "_source");
        Graphics.Blit(source, _source);
        fastBloomMaterial.SetTexture("_source", _source);
        BlurMaterial.SetTexture("_source", _source);
        //Graphics.Blit(_source, destination, BlurMaterial);
        // RT_Format = source.format;
        #region Density convolution
        UniformFogCamera.cullingMask = 1 << instance.FogVolumeLayer;
        //UniformFogCamera.cullingMask |= 1 << LayerMask.NameToLayer("FogVolumeUniform");//add Fog volume uniforms
        FOV_compensation = initFOV / SceneCamera.fieldOfView;
        Shader.SetGlobalFloat("FOV_compensation", FOV_compensation);
        fastBloomMaterial.SetFloat("_Falloff", _Falloff);
        // BlurMaterial.SetFloat("_Dither", _Dither);
        // BlurMaterial.SetFloat("_Distortion", ImageDistortion * -.08f);

        RenderTexture RT_DensityBlur = RenderTexture.GetTemporary(screenX / Downsample, screenY / Downsample, 0, RT_Format);


        DownSample4x(source, RT_DensityBlur);

        for (int i = 0; i < iterations; i++)
        {
            RenderTexture RT_DensityBlur2 = RenderTexture.GetTemporary(screenX / Downsample, screenY / Downsample, 0, RT_Format);
            FourTapCone(RT_DensityBlur, RT_DensityBlur2, i);
            RenderTexture.ReleaseTemporary(RT_DensityBlur);
            RT_DensityBlur = RT_DensityBlur2;
        }
        // Graphics.Blit(RT_DensityBlur, destination);       

        #endregion

        #region Bloom
        if (intensity > 0)
        {
            Rendering.EnsureKeyword(fastBloomMaterial, "BLOOM", true);
            float widthMod = 2.0f / (float)_BloomDowsample;
            fastBloomMaterial.SetFloat("_Saturation", _Saturation);
            fastBloomMaterial.SetVector("_Parameter", new Vector4(blurSize * widthMod, 0.0f, threshold, intensity));
            var rtW = source.width / _BloomDowsample;
            var rtH = source.height / _BloomDowsample;

            // downsample
            RenderTexture rt = RenderTexture.GetTemporary(rtW, rtH, 0, RT_Format);
            rt.filterMode = FilterMode.Bilinear;

            if (SceneBloom)
                Graphics.Blit(source, rt, fastBloomMaterial, 1);
            else
                Graphics.Blit(RT_DensityBlur, rt, fastBloomMaterial, 1);

            var passOffs = blurType == BlurType.Standard ? 0 : 2;

            for (int i = 1; i < blurIterations; i++)
            {
                fastBloomMaterial.SetVector("_Parameter", new Vector4(blurSize * widthMod + (i * 1.0f), 0.0f, threshold, intensity));

                // vertical blur
                RenderTexture rt2 = RenderTexture.GetTemporary(rtW, rtH, 0, RT_Format);
                rt2.filterMode = FilterMode.Bilinear;
                Graphics.Blit(rt, rt2, fastBloomMaterial, 2 + passOffs);
                RenderTexture.ReleaseTemporary(rt);
                rt = rt2;

                // horizontal blur
                rt2 = RenderTexture.GetTemporary(rtW, rtH, 0, RT_Format);
                rt2.filterMode = FilterMode.Bilinear;
                Graphics.Blit(rt, rt2, fastBloomMaterial, 3 + passOffs);
                RenderTexture.ReleaseTemporary(rt);
                rt = rt2;
            }

            fastBloomMaterial.SetTexture("_Bloom", rt);
            RenderTexture.ReleaseTemporary(rt);
        }
        else
            Rendering.EnsureKeyword(fastBloomMaterial, "BLOOM", false);
        #endregion
        Graphics.Blit(RT_DensityBlur, destination, fastBloomMaterial, 0);
        RenderTexture.ReleaseTemporary(RT_DensityBlur);

    }
}
