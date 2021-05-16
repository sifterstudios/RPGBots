using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class MockupManager : MonoBehaviour
{
    public GameObject CameraRoot, BoundariesInterior, BoundariesExterior;
    Rotator _Rotator;
    Camera GameCamera;
    public FogVolume _FogVolume;
    public FogVolume _ShadowCaster;
    ExplorationCamera _ExplorationCamera;
    FogVolumeRenderer _FogVolumeRenderer;
    public ShadowCamera.TextureSize OutsideShadowResolution = (ShadowCamera.TextureSize)256;
    public ShadowCamera.TextureSize InsideShadowResolution = (ShadowCamera.TextureSize)128;
    [Range(.001f, .1f)]
    public float FOVTransitionSpeed = .01f;
    public bool EnableFogVolumeRendererOutsideBox = false;
    public float CurrentFOV, targetFOV = 90;
    //float _ExplorationCameraInitialFOV;
    float _InitialFogVolumeSize;
    public float AtmosphereSizeInside = 100;
    public float AtmosphereSizeOutside = 49.99f;
    ReflectionProbe[] SceneReflectionProbes;
    // Use this for initialization
    bool isInside;
    public GameObject[] ObjectsToToggle;
    bool[] ObjectsToToggleDefaultVisible;
    //lets rotate only the first time
    int count = 0;
    public float InitialFOV;
    public Transform RespawnPoint;
    Vector3 CurrentCameraPosition;
    bool exit = false;
    bool enter = false;
    public GameObject DebugIntersectionPoint;
    BoxCollider[] BoundariesCollider;
    //public Texture2D FadeTexture;
    BoundingBox _InteriorBox, _CurrentPlayableBoundingBox;
    float fade = 0;
    public float CameraRotation = 1;
    struct BoundingBox
    {
        public Vector3 min, max;
        public BoundingBox(Vector3 _min, Vector3 _max)
        {
            min = Vector3.zero;
            max = Vector3.zero;

            min.x = _min.x;
            min.y = _min.y;
            min.x = _min.x;

            max.x = _max.x;
            max.y = _max.y;
            max.x = _max.x;

        }
    };

    void OnEnable()
    {



        BoundariesCollider = new BoxCollider[2];
        BoundariesCollider[0] = BoundariesExterior.GetComponent<BoxCollider>();
        BoundariesCollider[1] = BoundariesInterior.GetComponent<BoxCollider>();
        GameCamera = Camera.main;
        InitialFOV = GameCamera.fieldOfView;
        _ExplorationCamera = GameCamera.GetComponent<ExplorationCamera>();
        //_ExplorationCameraInitialFOV = _ExplorationCamera.FOV;
        _FogVolumeRenderer = Camera.main.GetComponent<FogVolumeRenderer>();
        if (CameraRoot)
            _Rotator = CameraRoot.GetComponent<Rotator>();
        if (_ShadowCaster)
            AtmosphereSizeOutside = _ShadowCaster.fogVolumeScale.x;

        SceneReflectionProbes = FindObjectsOfType(typeof(ReflectionProbe)) as ReflectionProbe[];

        ObjectsToToggleDefaultVisible = new bool[ObjectsToToggle.Length];

        for (int i = 0; i < ObjectsToToggle.Length; i++)
        {

            ObjectsToToggleDefaultVisible[i] = ObjectsToToggle[i].activeInHierarchy;
        }
        SetupOverlayEffect();
        _ExplorationCamera.enabled = true;
    }
    void PointIsInsideVolume(Vector3 PointPosition)
    {
        //for simplicity: using not rotated volumes which are not children of anyone
        bool result = false;
        Vector3 PlayableVolume = Vector3.zero;
        GameObject PlayableVolumeGO;
        if (isInside)
        {
            PlayableVolumeGO = BoundariesInterior;
            PlayableVolume = BoundariesInterior.transform.localScale;
        }
        else
        {
            PlayableVolumeGO = BoundariesExterior;
            PlayableVolume = BoundariesExterior.transform.localScale;
        }

        float PlayableVolumeXmax = PlayableVolumeGO.transform.position.x + PlayableVolume.x / 2;
        float PlayableVolumeXmin = PlayableVolumeGO.transform.position.x - PlayableVolume.x / 2;

        float PlayableVolumeYmax = PlayableVolumeGO.transform.position.y + PlayableVolume.y / 2;
        float PlayableVolumeYmin = PlayableVolumeGO.transform.position.y - PlayableVolume.y / 2;

        float PlayableVolumeZmax = PlayableVolumeGO.transform.position.z + PlayableVolume.z / 2;
        float PlayableVolumeZmin = PlayableVolumeGO.transform.position.z - PlayableVolume.z / 2;

        if (PlayableVolumeXmax > PointPosition.x && PlayableVolumeXmin < PointPosition.x)
        {
            // print("x dentro");
            if (PlayableVolumeYmax > PointPosition.y && PlayableVolumeYmin < PointPosition.y)
            {
                // print("y dentro");
                if (PlayableVolumeZmax > PointPosition.z && PlayableVolumeZmin < PointPosition.z)
                {
                    // print("z dentro");
                    result = true;

                }
            }

        }
        _CurrentPlayableBoundingBox.max = new Vector3(PlayableVolumeXmax, PlayableVolumeYmax, PlayableVolumeYmax);
        _CurrentPlayableBoundingBox.min = new Vector3(PlayableVolumeXmin, PlayableVolumeYmin, PlayableVolumeYmin);
        isInside = result;


    }

    void ToggleObjects()
    {
        for (int i = 0; i < ObjectsToToggle.Length; i++)
        {
            if (isInside)
                ObjectsToToggle[i].SetActive(!ObjectsToToggleDefaultVisible[i]);
            else
                ObjectsToToggle[i].SetActive(ObjectsToToggleDefaultVisible[i]);

        }
    }

    Fade FadeEffect;
    void SetupOverlayEffect()
    {
        FadeEffect=GameCamera.GetComponent<Fade>();
        if (FadeEffect == null)
        {
            FadeEffect = GameCamera.gameObject.AddComponent<Fade>();
            //FadeEffect.texture = FadeTexture;
           // FadeEffect.blendMode = UnityStandardAssets.ImageEffects.ScreenOverlay.OverlayBlendMode.AlphaBlend;
            //FadeEffect.overlayShader = Shader.Find("Hidden/BlendModesOverlay");
            FadeEffect._Color.a = 0;
        }
    }
    void Fade()
    {
        if (FadeEffect)
        {

            fade = Mathf.Lerp(fade, 0, .1f);
            FadeEffect._Color.a = fade;
        }
        else
            Debug.LogError("Fade effect not set");
    }
    void Teleport()
    {
        GameCamera.transform.localRotation = RespawnPoint.localRotation;
        GameCamera.transform.localPosition = RespawnPoint.localPosition;
    }
    void OnBoxExit()
    {
        fade = 1;
        GameCamera.transform.parent.eulerAngles = (Vector3.zero);
        exit = true;
        _ExplorationCamera.enabled = false;
        Teleport();
        //print("exit");
        GameCamera.fieldOfView = InitialFOV;
        _ExplorationCamera.FOV = InitialFOV;
        _ExplorationCamera.tilt = 0;
        _ExplorationCamera.Speed = _ExplorationCamera.InitialSpeed;
        _ExplorationCamera.enabled = true;
        //
       
    }


    void OnBoxEnter()
    {
        enter = true;
        fade = 1;
       // GameCamera.transform.parent.eulerAngles = (Vector3.zero);
        // print("enter");
    }

    Vector3 ClosestPoint(BoundingBox _BoundingBox, Vector3 Point)
    {
        Vector3 _ClosestPoint = new Vector3();
        //https://gdbooks.gitbooks.io/3dcollisions/content/Chapter1/closest_point_aabb.html

        if (Point.x > _BoundingBox.max.x)
            _ClosestPoint.x = _BoundingBox.max.x;
        else if (Point.x < _BoundingBox.min.x)
            _ClosestPoint.x = _BoundingBox.min.x;
        else
            _ClosestPoint.x = Point.x;

        if (Point.y > _BoundingBox.max.y)
            _ClosestPoint.y = _BoundingBox.max.y;
        else if (Point.y < _BoundingBox.min.y)
            _ClosestPoint.y = _BoundingBox.min.y;
        else
            _ClosestPoint.y = Point.y;

        if (Point.z > _BoundingBox.max.z)
            _ClosestPoint.z = _BoundingBox.max.z;
        else if (Point.z < _BoundingBox.min.z)
            _ClosestPoint.z = _BoundingBox.min.z;
        else
            _ClosestPoint.z = Point.z;

        return _ClosestPoint;
    }
    Vector3 _ClosestPoint = new Vector3();
    //float distance = 0;
    public float InteriorRadiusFade = 74.6f;
    public float RadialPow = 10.1f;
    void BoundariesDistanceCheck()
    {
        if (isInside)
        {
            float RadialDistance = Vector3.Magnitude(CurrentCameraPosition - BoundariesExterior.transform.position);
            RadialDistance /= InteriorRadiusFade;
            RadialDistance = Mathf.Pow(RadialDistance, RadialPow);

            RadialDistance = Mathf.Clamp(RadialDistance, 0, 1.1f);
            if (RadialDistance > .1)
                fade = RadialDistance;
            if (RadialDistance > .99f)
                Teleport();
            //  print(RadialDistance);
        }
        // _ClosestPoint = BoundariesCollider[0].ClosestPointOnBounds(CurrentCameraPosition);//el de unity usando el box collision. Rula
        _ClosestPoint = ClosestPoint(_CurrentPlayableBoundingBox, CurrentCameraPosition);//replicado. Falta proyectar el punto para que sea válido en el interior

        // print(_CurrentPlayableBoundingBox);
        //distance = Vector3.Distance(_ClosestPoint, CurrentCameraPosition);
        if (DebugIntersectionPoint != null)
            DebugIntersectionPoint.transform.position = _ClosestPoint;
        // print(distance);
    }
    void FixedUpdate()
    {

        CurrentCameraPosition = GameCamera.transform.position;
        if (_ShadowCaster && _ExplorationCamera && _Rotator && _FogVolumeRenderer && GameCamera && _FogVolume)
        {
            _ExplorationCamera.FOVTransitionSpeed = FOVTransitionSpeed;
            PointIsInsideVolume(GameCamera.transform.position);
            BoundariesDistanceCheck();
            if (isInside)
            {
                if (!enter)
                    OnBoxEnter();
                Fade();
                ToggleObjects();
               //_ShadowCaster._ShadowCamera.textureSize = InsideShadowResolution;
                count++;
                _ShadowCaster.fogVolumeScale.x = AtmosphereSizeInside;
                _ShadowCaster.fogVolumeScale.x = AtmosphereSizeInside;
                _ShadowCaster.UpdateBoxMesh();
                CurrentFOV = Mathf.Lerp(CurrentFOV, targetFOV, FOVTransitionSpeed);
                _ExplorationCamera.FOV = CurrentFOV;

                _Rotator.enabled = false;
                _FogVolumeRenderer.enabled = true;
                GameCamera.clearFlags = CameraClearFlags.Skybox;

                for (int i = 0; i < SceneReflectionProbes.Length; i++)
                {
                    SceneReflectionProbes[i].enabled = false;
                }
                exit = false;

               
            }
            else
            {

                Fade();
                enter = false;
                for (int i = 0; i < SceneReflectionProbes.Length; i++)
                {
                    SceneReflectionProbes[i].enabled = true;
                }
                ToggleObjects();
                GameCamera.clearFlags = CameraClearFlags.SolidColor;
                _ShadowCaster.fogVolumeScale.x = AtmosphereSizeOutside;
                _ShadowCaster.UpdateBoxMesh();

                //_ShadowCaster._ShadowCamera.textureSize = OutsideShadowResolution;
                if (count < 1)
                    _Rotator.enabled = true;
                CurrentFOV = _ExplorationCamera.FOV;
                if (!EnableFogVolumeRendererOutsideBox)
                    _FogVolumeRenderer.enabled = false;

                if (RespawnPoint != null)
                {
                    if (!exit)
                    {

                        // 

                        OnBoxExit();
                        _ExplorationCamera.enabled = true;

                    }
                }

            }
        }
        else print("Config not complete");

        _ShadowCaster.UpdateBoxMesh();
        _FogVolume.UpdateBoxMesh();
    }
    void Update()
    {

        if (!isInside)
            if (EnableFogVolumeRendererOutsideBox) _FogVolumeRenderer.enabled = true; else _FogVolumeRenderer.enabled = false;
    }

    //void OnGUI()
    //{
    //    EnableFogVolumeRendererOutsideBox=GUI.Toggle(new Rect(10, 50, 150, 50),EnableFogVolumeRendererOutsideBox, "Downsampled");
    //}
}