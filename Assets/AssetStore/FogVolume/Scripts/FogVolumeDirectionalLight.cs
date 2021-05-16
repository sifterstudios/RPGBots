using UnityEngine;
using UnityEngine.UI;
/*not working on android*/
[ExecuteInEditMode]
public class FogVolumeDirectionalLight : MonoBehaviour
{
    public FogVolume[] _TargetFogVolumes;
    public Vector2 MiniaturePosition = new Vector2(110, 320);
    public FogVolume _ProminentFogVolume = null;

    //  [HideInInspector]
    public Material FogVolumeMaterial;

    public float _CameraVerticalPosition = 500;

    RenderTexture depthRT;

    public enum Resolution
    {
        _256 = 256,

        _512 = 512,

        _1024 = 1024,

        _2048 = 2048,

        _4096 = 4096
    };
    public enum Antialiasing
    {
        _1 = 1,

        _2 = 2,

        _4 = 4,

        _8 = 8
    };

    public Antialiasing _Antialiasing = Antialiasing._1;

    public Resolution Size = Resolution._512;

    // [Range(10, 300)]
    // float CameraSize = 100;
    //public bool ToggleKeyword = true;
    // [HideInInspector]
    public Camera ShadowCamera;

    public enum ScaleMode
    {
        VolumeMaxAxis,

        Manual
    };

    public float _FogVolumeShadowMapEdgeSoftness = 0.001f;

    public ScaleMode _ScaleMode = ScaleMode.VolumeMaxAxis;

    public LayerMask LayersToRender;

    [HideInInspector]
    public Shader outputDepth;

    [HideInInspector]
    public GameObject GOShadowCamera;

    public bool CameraVisible;

    public enum UpdateMode
    {
        OnStart,

        Interleaved
    };

    Image _CanvasImage;

    public UpdateMode _UpdateMode = UpdateMode.Interleaved;

    public float Scale = 50;

    [Range(0, 100)]
    public int SkipFrames = 2;

    //  CanvasRenderer DebugCanvas;
    public bool ShowMiniature = false;

    GameObject _GO_Canvas, _GO_Image;

    Canvas _Canvas;

    public Material DebugViewMaterial;

    GameObject Quad;

    Vector3 FocusPosition;

    FogVolumeData _FogVolumeData;

    Camera _GameCamera;

    public enum FocusMode
    {
        VolumeCenter,

        GameCameraPosition,

        GameObject
    };

    public Transform _GameObjectFocus;

    public FocusMode _FocusMode = FocusMode.VolumeCenter;

    Material quadMaterial = null;

    public Material QuadMaterial
    {
        get
        {
            if (quadMaterial == null) { CreateMaterial(); }
            return quadMaterial;
        }
    }

    public Shader quadShader;

    void OnEnable()
    {
        _GO_Canvas = GameObject.Find("FogVolume Debug Canvas");
        if (!_GO_Canvas) _GO_Canvas = new GameObject("FogVolume Debug Canvas");
        _GO_Image = GameObject.Find("FogVolume Image");
        if (!_GO_Image)
        {
            _GO_Image = new GameObject("FogVolume Image");
            _CanvasImage = _GO_Image.AddComponent<Image>();
            _CanvasImage.material = DebugViewMaterial;

            _CanvasImage.rectTransform.position = new Vector3(MiniaturePosition.x, MiniaturePosition.y, 0);
            _CanvasImage.rectTransform.pivot = new Vector2(.5f, .5f);
            _CanvasImage.rectTransform.anchorMax = new Vector2(0, 0);
            _CanvasImage.rectTransform.anchorMin = new Vector2(0, 0);
            _CanvasImage.rectTransform.localScale = new Vector3(2, 2, 2);
        }
        
        if (!_CanvasImage) _CanvasImage = _GO_Image.GetComponent<Image>();
        _CanvasImage.material = DebugViewMaterial;
        _GO_Image.transform.SetParent(_GO_Canvas.transform);
        _GO_Canvas.AddComponent<CanvasScaler>();
        _GO_Canvas.GetComponent<CanvasScaler>().scaleFactor = 1;
        _GO_Canvas.GetComponent<CanvasScaler>().referencePixelsPerUnit = 100;
        _Canvas = _GO_Canvas.GetComponent<Canvas>(); // ("Debug view canvas");
        _GO_Canvas.hideFlags = HideFlags.HideInHierarchy;

        // DebugCanvas = _GO_Canvas.AddComponent<CanvasRenderer>();
        _GO_Canvas.layer = LayerMask.NameToLayer("UI");
        _GO_Image.layer = LayerMask.NameToLayer("UI");

        _Canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        Initialize();

        if (_UpdateMode == UpdateMode.OnStart) { Render(); }
    }

    void CreateMaterial()
    {
        DestroyImmediate(quadMaterial);
        quadShader = Shader.Find("Hidden/DepthMapQuad");

        //
        quadMaterial = new Material(quadShader);
        quadMaterial.name = "Depth camera quad material";
        quadMaterial.hideFlags = HideFlags.HideAndDontSave;
    }
    RenderTextureFormat rt_DepthFormat;
    void Initialize()
    {
        CreateMaterial();

        if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RGFloat))
            rt_DepthFormat = RenderTextureFormat.RGFloat;
        else
            rt_DepthFormat = RenderTextureFormat.DefaultHDR;

        GameObject FogVolumeDataGO = GameObject.Find("Fog Volume Data");
        if (FogVolumeDataGO) _FogVolumeData = FogVolumeDataGO.GetComponent<FogVolumeData>();
        else return;

        _GameCamera = _FogVolumeData.GameCamera;

        GOShadowCamera = GameObject.Find("FogVolumeShadowCamera");
        if (!GOShadowCamera)
        {
            GOShadowCamera = new GameObject();
            GOShadowCamera.name = "FogVolumeShadowCamera";
        }

        if (!GOShadowCamera) print("Shadow camera is lost");
        else ShadowCamera = GOShadowCamera.GetComponent<Camera>();

        if (!depthRT)
        {
            depthRT = new RenderTexture((int) Size, (int) Size, 16, rt_DepthFormat);
            depthRT.antiAliasing = (int) _Antialiasing;
            depthRT.filterMode = FilterMode.Bilinear;
            depthRT.name = "FogVolumeShadowMap";
            depthRT.wrapMode = TextureWrapMode.Clamp;
        }

        if (!ShadowCamera) ShadowCamera = GOShadowCamera.AddComponent<Camera>();
        else ShadowCamera = GOShadowCamera.GetComponent<Camera>();

        ShadowCamera.clearFlags = CameraClearFlags.Color;
        ShadowCamera.backgroundColor = Color.black;
        ShadowCamera.orthographic = true;
        ShadowCamera.farClipPlane = 10000.0f;
        ShadowCamera.enabled = false;
        ShadowCamera.stereoTargetEye = StereoTargetEyeMask.None;
        ShadowCamera.targetTexture = depthRT;
        ShadowCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("FogVolume"));
        ShadowCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("FogVolumeUniform"));
        ShadowCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("FogVolumeSurrogate"));
        ShadowCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("FogVolumeShadowCaster"));

        //make it child
        ShadowCamera.transform.parent = gameObject.transform;

        Quad = GameObject.Find("Depth map background");
        if (!Quad) Quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Quad.name = "Depth map background";
        Quad.GetComponent<MeshRenderer>().sharedMaterial = QuadMaterial;
        Quad.transform.parent = ShadowCamera.transform;

        //remnove the collider
        DestroyImmediate(Quad.GetComponent<MeshCollider>());
        Quad.hideFlags = HideFlags.HideInHierarchy;
    }
    void EnableVolumetricShadow(bool b)
    {
        if (_TargetFogVolumes == null) { return; }
        if (_TargetFogVolumes.Length > 0)
        {
            float largestAxis = 0.0f;
            int largestIndex = 0;
            for (int FVindex = 0; FVindex < _TargetFogVolumes.Length; FVindex++)
            {
                var fogVolume = _TargetFogVolumes[FVindex];

                if ((fogVolume != null) && (fogVolume._FogType == FogVolume.FogType.Textured))
                {
                    if (fogVolume.enabled)
                    {
                        FogVolumeMaterial = fogVolume.FogMaterial;
                        FogVolumeMaterial.SetInt("_VolumetricShadowsEnabled", b ? 1 : 0);
                    }

                    float largest = _MaxOf(fogVolume.fogVolumeScale.x,
                                           fogVolume.fogVolumeScale.y,
                                           fogVolume.fogVolumeScale.z);
                    if (largest > largestAxis)
                    {
                        largestAxis = largest;
                        largestIndex = FVindex;
                    }
                }
            }

            _ProminentFogVolume = _TargetFogVolumes[largestIndex];
        }
    }

    void Update()
    {
        if (_CanvasImage.material != null) _CanvasImage.material = DebugViewMaterial;

        if (!ShadowCamera) Initialize();
        
        if (_TargetFogVolumes != null && _AtLeastOneFogVolumeInArray())
        {
            EnableVolumetricShadow(depthRT);          

            LayersToRender &= ~(1 << LayerMask.NameToLayer("FogVolume"));
            LayersToRender &= ~(1 << LayerMask.NameToLayer("FogVolumeUniform"));
            LayersToRender &= ~(1 << LayerMask.NameToLayer("FogVolumeSurrogate"));
            LayersToRender &= ~(1 << LayerMask.NameToLayer("FogVolumeShadowCaster"));
            ShadowCamera.cullingMask = LayersToRender;


            Refresh();
         
            //
            //now, adjust camera size to make it see the whole volume
            if (_ScaleMode == ScaleMode.VolumeMaxAxis)
            {
                if (_ProminentFogVolume != null)
                {
                    ShadowCamera.orthographicSize =
                            _MaxOf(_ProminentFogVolume.fogVolumeScale.x,
                                   _ProminentFogVolume.fogVolumeScale.y,
                                   _ProminentFogVolume.fogVolumeScale.z) * .5f;
                }
            }
            else ShadowCamera.orthographicSize = Scale;

            // ShadowCamera.orthographicSize = CameraSize;
            if (ShadowCamera.cullingMask != 0 &&
                _ProminentFogVolume != null &&
                _UpdateMode == UpdateMode.Interleaved)
            {
                if (FogVolumeUtilities.ExtensionMethods.TimeSnap(SkipFrames))
                {
                    Render();
                }
            }
        }
        else
        {
            if (depthRT)
            {
                DestroyImmediate(depthRT);
                DestroyImmediate(GOShadowCamera);
            }
        }

        if (!ShowMiniature &&
            _GO_Canvas.activeInHierarchy) _GO_Canvas.SetActive(ShowMiniature);

        if (ShowMiniature && !_GO_Canvas.activeInHierarchy) _GO_Canvas.SetActive(ShowMiniature);

        #if UNITY_EDITOR
                if(ShowMiniature)
                    _CanvasImage.rectTransform.position = new Vector3(MiniaturePosition.x, MiniaturePosition.y, 0);
        #endif
    }

    public void Refresh()
    {
        if (_TargetFogVolumes == null)
        {
            _ProminentFogVolume = null;
            return;
        }

        for (int i = 0; i < _TargetFogVolumes.Length; i++)
        {
            var fogVolume = _TargetFogVolumes[i];
            if ((fogVolume != null) &&
                (fogVolume._FogType == FogVolume.FogType.Textured))
            {
                if (fogVolume.HasUpdatedBoxMesh)
                {
                    float largestOfProminent = (_ProminentFogVolume != null)
                                                   ? _MaxOf(_ProminentFogVolume.fogVolumeScale.x,
                                                            _ProminentFogVolume.fogVolumeScale.y,
                                                            _ProminentFogVolume.fogVolumeScale.z)
                                                   : 0.0f;
                    float largest = _MaxOf(fogVolume.fogVolumeScale.x,
                                           fogVolume.fogVolumeScale.y,
                                           fogVolume.fogVolumeScale.z);
                    if (largest > largestOfProminent) { _ProminentFogVolume = fogVolume; }
                }
            }
        }
    }

    public void Render()
    {
        if (!depthRT)
        {
            Initialize();
        }

        if (depthRT.height != (int) Size)
        {
            DestroyImmediate(depthRT);
            Initialize();

            // Debug.Log("A tomar por culo la textura");
        }

        if ((int) _Antialiasing != depthRT.antiAliasing)
        {
            DestroyImmediate(depthRT);
            Initialize();
        }

        if (!ShadowCamera)
        {
            Initialize();
        }

        switch (_FocusMode)
        {
            case FocusMode.GameCameraPosition:
                FocusPosition = _GameCamera.transform.position;
                break;

            case FocusMode.VolumeCenter:
                if (_ProminentFogVolume != null)
                {
                    FocusPosition = _ProminentFogVolume.transform.position;
                }
                else
                {
                    FocusPosition = Vector3.zero;
                }
                break;

            case FocusMode.GameObject:
                if (_GameObjectFocus) FocusPosition = _GameObjectFocus.transform.position;
                break;
        }

        //move the camera to the target center
        Vector3 VerticalTranslate = new Vector3(0,
                                                0, /* _TargetFogVolume.fogVolumeScale.y / 2*/
                                                FocusPosition.y - _CameraVerticalPosition);

        ShadowCamera.transform.position = FocusPosition;
        ShadowCamera.transform.Translate(VerticalTranslate, Space.Self);

        Vector3 QuadScale = new Vector3(ShadowCamera.orthographicSize * 2,
                                        ShadowCamera.orthographicSize * 2,
                                        ShadowCamera.orthographicSize * 2);
        Quad.transform.localScale = QuadScale;

        //move it to the farclip
        Quad.transform.position = ShadowCamera.transform.position;
        Vector3 QuadTranslate = new Vector3(0, 0, ShadowCamera.farClipPlane - 50);
        Quad.transform.Translate(QuadTranslate, Space.Self);
        ShadowCamera.transform.rotation = Quaternion.LookRotation(transform.forward);
        ;
        Shader.SetGlobalVector("_ShadowCameraPosition", ShadowCamera.transform.position);
        Shader.SetGlobalMatrix("_ShadowCameraProjection", ShadowCamera.worldToCameraMatrix);
        Shader.SetGlobalFloat("_ShadowCameraSize", ShadowCamera.orthographicSize);
        Shader.SetGlobalVector("_ShadowLightDir", ShadowCamera.transform.forward);

        //depthRT.DiscardContents();
        quadShader.maximumLOD = 1;
        Shader.SetGlobalFloat("_FogVolumeShadowMapEdgeSoftness",
                              20.0f / _FogVolumeShadowMapEdgeSoftness);
        ShadowCamera.RenderWithShader(outputDepth, "RenderType");
        quadShader.maximumLOD = 100;
        Shader.SetGlobalTexture("_ShadowTexture", depthRT);
    }
    void OnDisable()
    {
        DestroyImmediate(depthRT);
        if (_GO_Canvas) _GO_Canvas.SetActive(false);

        //  DestroyImmediate(_Canvas);
        //   DestroyImmediate(GOShadowCamera);

        //  if (FogVolumeMaterial)
        //  FogVolumeUtilities.Rendering.EnsureKeyword(FogVolumeMaterial, "VOLUMETRIC_SHADOWS", false);
        //    FogVolumeMaterial.SetInt("_VolumetricShadowsEnabled", 0);
        EnableVolumetricShadow(false);
    }

    private void OnDestroy()
    {
        DestroyImmediate(GOShadowCamera);

        // print("A la mierda!");
        DestroyImmediate(_GO_Canvas);
        DestroyImmediate(Quad);
    }

    private bool _AtLeastOneFogVolumeInArray()
    {
        if (_TargetFogVolumes != null)
        {
            for (int i = 0; i < _TargetFogVolumes.Length; i++)
            {
                if (_TargetFogVolumes[i] != null) { return true; }
            }
        }

        return false;
    }

    public void AddAllFogVolumesToThisLight()
    {
        _ProminentFogVolume = null;
        var fogVolumes = FindObjectsOfType<FogVolume>();

        int validFogVolumeCount = 0;
        for (int i = 0; i < fogVolumes.Length; i++)
        {
            if ((fogVolumes[i] != null) &&
                (fogVolumes[i]._FogType == FogVolume.FogType.Textured)) { validFogVolumeCount++; }
        }

        _TargetFogVolumes = new FogVolume[validFogVolumeCount];
        int k = 0;
        for (var i = 0; i < fogVolumes.Length; i++)
        {
            var fogVolume = fogVolumes[i];
            if ((fogVolume != null) && (fogVolume._FogType == FogVolume.FogType.Textured))
            {
                _TargetFogVolumes[k++] = fogVolumes[i];
            }  
        }
    }

    public void RemoveAllFogVolumesFromThisLight()
    {
        _ProminentFogVolume = null;
        _TargetFogVolumes = null;
    }


    private float _MaxOf(float _a, float _b) { return _a >= _b ? _a : _b; }


    private float _MaxOf(float _a, float _b, float _c) { return _MaxOf(_MaxOf(_a, _b), _c); }
}
