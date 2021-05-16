using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Assertions;


//=================================================================================================
/**
 *  @brief This class manages all lights for a particular FogVolume.
 * 
 *************************************************************************************************/
public class FogVolumeLightManager : MonoBehaviour
{
    public int CurrentLightCount { get; private set; }

    public int VisibleLightCount { get; private set; }

    public bool DrawDebugData { get; set; }

    public bool AlreadyUsesTransformForPoI { get { return m_pointOfInterestTf != null; } }

    // Very slow and garbage allocating. Only call this once!
    public void FindLightsInScene()
    {
        CurrentLightCount = 0;
        VisibleLightCount = 0;
        m_lights.Clear();
        m_lightsInFrustum.Clear();
        for (int i = 0; i < MaxLightCount; i++)
        {
            m_lights.Add(new LightData());
            m_lightsInFrustum.Add(new LightData());
        }

        FogVolumeLight[] lights = FindObjectsOfType<FogVolumeLight>();
        for (int i = 0; i < lights.Length; i++)
        {
            Light unityLight = lights[i].GetComponent<Light>();
            if (unityLight != null)
            {
                switch (unityLight.type)
                {
                    case LightType.Point:
                    {
                        AddPointLight(unityLight);
                        lights[i].IsAddedToNormalLight = true;
                        break;
                    }
                    case LightType.Spot:
                    {
                        AddSpotLight(unityLight);
                        lights[i].IsAddedToNormalLight = true;
                        break;
                    }
                }
            }
            else
            {
                if (lights[i].IsPointLight)
                {
                    AddSimulatedPointLight(lights[i]);
                    lights[i].IsAddedToNormalLight = false;
                }
                else
                {
                    AddSimulatedSpotLight(lights[i]);
                    lights[i].IsAddedToNormalLight = false;
                }
            }
        }
    }

    // Very slow and garbage allocating. Only call this once!
    public void FindLightsInFogVolume()
    {
        CurrentLightCount = 0;
        VisibleLightCount = 0;
        m_lights.Clear();
        m_lightsInFrustum.Clear();
        for (int i = 0; i < MaxLightCount; i++)
        {
            m_lights.Add(new LightData());
            m_lightsInFrustum.Add(new LightData());
        }

        if (m_boxCollider == null) { m_boxCollider = gameObject.GetComponent<BoxCollider>(); }
        Bounds boundingBox = m_boxCollider.bounds;

        FogVolumeLight[] lights = FindObjectsOfType<FogVolumeLight>();
        for (int i = 0; i < lights.Length; i++)
        {
            if (boundingBox.Intersects(new Bounds(lights[i].gameObject.transform.position,
                                                  Vector3.one * LightInVolumeBoundsSize)))
            {
                Light unityLight = lights[i].GetComponent<Light>();
                if (unityLight != null)
                {
                    switch (unityLight.type)
                    {
                        case LightType.Point:
                        {
                            AddPointLight(unityLight);
                            lights[i].IsAddedToNormalLight = true;
                            break;
                        }
                        case LightType.Spot:
                        {
                            AddSpotLight(unityLight);
                            lights[i].IsAddedToNormalLight = true;
                            break;
                        }
                    }
                }
                else
                {
                    if (lights[i].IsPointLight)
                    {
                        AddSimulatedPointLight(lights[i]);
                        lights[i].IsAddedToNormalLight = false;
                    }
                    else
                    {
                        AddSimulatedSpotLight(lights[i]);
                        lights[i].IsAddedToNormalLight = false;
                    }
                }
            }
        }
    }

    //=============================================================================================
    /**
     *  @brief Add a simulated point light to the manager.
     *  
     *  Note that a simulated point light is any GameObject with a SimulatedPointLight component on
     *  it.
     *  
     *  @param _light The SimulatedPointLight component of an existing GameObject.
     *  
     *  @return True if the light was added to the managers list of lights.
     *  @return False if the manager already contains the maximum amount of lights that was 
     *          specified in the constructor.
     * 
     *********************************************************************************************/
    public bool AddSimulatedPointLight(FogVolumeLight _light)
    {
        Assert.IsTrue(CurrentLightCount < MaxLightCount,
                      "The maximum number of lights is already reached!");

        int index = _FindFirstFreeLight();
        if (index != InvalidIndex)
        {
            LightData data = m_lights[index];
            CurrentLightCount++;
            data.LightType = EFogVolumeLightType.FogVolumePointLight;
            data.Transform = _light.transform;
            data.Light = null;
            data.FogVolumeLight = _light;
            data.Bounds = new Bounds(data.Transform.position,
                                     Vector3.one * data.FogVolumeLight.Range * 2.5f);
            return true;
        }

        return false;
    }

    //=============================================================================================
    /**
     *  @brief Add a simulated spot light to the manager.
     *  
     *  Note that a simulated spot light is any GameObject with a SimulatedSpotLight component on
     *  it.
     *  
     *  @param _light The SimulatedSpotLight component of an existing GameObject.
     *  
     *  @return True if the light was added to the managers list of lights.
     *  @return False if the manager already contains the maximum amount of lights that was 
     *          specified in the constructor.
     * 
     *********************************************************************************************/
    public bool AddSimulatedSpotLight(FogVolumeLight _light)
    {
        Assert.IsTrue(CurrentLightCount < MaxLightCount,
                      "The maximum number of lights is already reached!");

        int index = _FindFirstFreeLight();
        if (index != InvalidIndex)
        {
            LightData data = m_lights[index];
            CurrentLightCount++;
            data.LightType = EFogVolumeLightType.FogVolumeSpotLight;
            data.Transform = _light.transform;
            data.Light = null;
            data.FogVolumeLight = _light;
            Vector3 center = data.Transform.position +
                             data.Transform.forward * data.FogVolumeLight.Range * 0.5f;
            data.Bounds = new Bounds(center,
                                     Vector3.one * data.FogVolumeLight.Range *
                                     (0.75f + data.FogVolumeLight.Angle * 0.03f));
            return true;
        }

        return false;
    }

    //=============================================================================================
    /**
     *  @brief Add an existing point light to the manager.
     *  
     *  @param _light The light component of an existing point light.
     *  
     *  @return True if the light was added to the managers list of lights.
     *  @return False if the manager already contains the maximum amount of lights that was 
     *          specified in the constructor.
     * 
     *********************************************************************************************/
    public bool AddPointLight(Light _light)
    {
        Assert.IsTrue(CurrentLightCount < MaxLightCount,
                      "The maximum number of lights is already reached!");

        int index = _FindFirstFreeLight();
        if (index != InvalidIndex)
        {
            LightData data = m_lights[index];
            CurrentLightCount++;
            data.LightType = EFogVolumeLightType.PointLight;
            data.Transform = _light.transform;
            data.Light = _light;
            data.FogVolumeLight = null;
            data.Bounds = new Bounds(data.Transform.position,
                                     Vector3.one * data.Light.range * 2.5f);
            return true;
        }

        return false;
    }

    //=============================================================================================
    /**
     *  @brief Add an existing spot light to the manager.
     *  
     *  @param _light The light component of an existing spot light.
     *  
     *  @return True if the light was added to the managers list of lights.
     *  @return False if the manager already contains the maximum amount of lights that was 
     *          specified in the constructor.
     * 
     *********************************************************************************************/
    public bool AddSpotLight(Light _light)
    {
        Assert.IsTrue(CurrentLightCount < MaxLightCount,
                      "The maximum number of lights is already reached!");

        int index = _FindFirstFreeLight();
        if (index != InvalidIndex)
        {
            LightData data = m_lights[index];
            CurrentLightCount++;
            data.LightType = EFogVolumeLightType.SpotLight;
            data.Transform = _light.transform;
            data.Light = _light;
            data.FogVolumeLight = null;
            Vector3 center = data.Transform.position +
                             data.Transform.forward * data.Light.range * 0.5f;
            data.Bounds = new Bounds(center,
                                     Vector3.one * data.Light.range *
                                     (0.75f + data.Light.spotAngle * 0.03f));
            return true;
        }

        return false;
    }

    //=============================================================================================
    /**
     *  @brief Removes the light with the specified transform.
     *  
     *  Note that nothing will happen if the light is not currently present in the manager.
     *  
     *  @param _lightToRemove The light that will be removed from this manager.
     *  
     *  @return True if the light was found inside the manager and removed successfully.
     *  @return False if the light was not found an thus not removed.
     * 
     *********************************************************************************************/
    public bool RemoveLight(Transform _lightToRemove)
    {
        int count = m_lights.Count;
        for (int i = 0; i < count; i++)
        {
            if (ReferenceEquals(m_lights[i].Transform, _lightToRemove))
            {
                m_lights[i].LightType = EFogVolumeLightType.None;
                CurrentLightCount--;
                return true;
            }
        }

        return false;
    }

    //=============================================================================================
    /**
     *  @brief Updates the lights that will be rendered.
     *  
     *  Note that this method should be called once per frame, before any data is sent to the 
     *  shaders.
     * 
     *********************************************************************************************/
    public void ManualUpdate(ref Plane[] _frustumPlanes)
    {
        FrustumPlanes = _frustumPlanes;
        m_camera = m_fogVolumeData != null ? m_fogVolumeData.GameCamera : null;
        if (m_camera == null) { return; }

        if (m_boxCollider == null) { m_boxCollider = m_fogVolume.GetComponent<BoxCollider>(); }
        if (m_pointOfInterestTf != null) { m_pointOfInterest = m_pointOfInterestTf.position; }
        _UpdateBounds();
        _FindLightsInFrustum();
        if (m_lightsInFrustum.Count > MaxVisibleLights) { _SortLightsInFrustum(); }
        _PrepareShaderArrays();
    }

    //=============================================================================================
    /**
     *  @brief Draws the gizmos if the user wants to see debug data.
     * 
     *********************************************************************************************/
    public void OnDrawGizmos()
    {
        hideFlags = HideFlags.HideInInspector;
        if (m_camera == null) { return; }
        if (!DrawDebugData) { return; }

        Color tempColor = Gizmos.color;

        Gizmos.color = Color.green;
        for (int i = 0; i < VisibleLightCount; i++)
        {
            Gizmos.DrawWireCube(m_lightsInFrustum[i].Bounds.center,
                                m_lightsInFrustum[i].Bounds.size);
        }

        Gizmos.color = Color.magenta;
        Matrix4x4 currentMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(m_camera.transform.position,
                                      m_camera.transform.rotation,
                                      Vector3.one);

        //Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawFrustum(m_camera.transform.position,
                           m_camera.fieldOfView,
                           m_camera.nearClipPlane,
                           m_fogVolume.PointLightingDistance2Camera,
                           m_camera.aspect);

        Gizmos.color = tempColor;
        Gizmos.matrix = currentMatrix;
    }

    //=============================================================================================
    /**
     *  @brief Sets the multiplier that is applied to the bounding box size of point lights.
     *  
     *  @param _cullSizeMultiplier The multiplier that is applied to the AABB of point lights.
     * 
     *********************************************************************************************/
    public void SetPointLightCullSizeMultiplier(float _cullSizeMultiplier)
    {
        m_pointLightCullSizeMultiplier = _cullSizeMultiplier;
    }

    //=============================================================================================
    /**
     *  @brief Sets the point of interest to a fixed position.
     *  
     *  @param _pointOfInterest The point that will be used for prioritizing which lights need to 
     *                          be rendered.
     * 
     *********************************************************************************************/
    public void SetPointOfInterest(Vector3 _pointOfInterest)
    {
        m_pointOfInterestTf = null;
        m_pointOfInterest = _pointOfInterest;
    }

    //=============================================================================================
    /**
     *  @brief Sets the point of interest to the specified transform.
     *  
     *  The point of interest will be updated from the position of the transform. It is therefore 
     *  not necessary to call this method more than once.
     * 
     *********************************************************************************************/
    public void SetPointOfInterest(Transform _pointOfInterest)
    {
        Assert.IsTrue(_pointOfInterest != null, "_pointOfInterest must not be null!");
        m_pointOfInterestTf = _pointOfInterest;
    }

    //=============================================================================================
    /**
     *  @brief Returns the array that contains all light positions.
     *  
     *  Remember to call Update() before using this method, otherwise old data will be sent to the
     *  shaders.
     * 
     *********************************************************************************************/
    public Vector4[] GetLightPositionArray() { return m_lightPos; }

    //=============================================================================================
    /**
     *  @brief Returns the array that contains all light rotations.
     *  
     *  Remember to call Update() before using this method, otherwise old data will be sent to the
     *  shaders.
     * 
     *********************************************************************************************/
    public Vector4[] GetLightRotationArray() { return m_lightRot; }

    //=============================================================================================
    /**
     *  @brief Returns the array that contains all light colors.
     *  
     *  Remember to call Update() before using this method, otherwise old data will be sent to the
     *  shaders.
     *********************************************************************************************/
    public Color[] GetLightColorArray() { return m_lightColor; }

    //=============================================================================================
    /**
     *  @brief Returns the array that contains all light data (intensity, range, spotlight angle,
     *         none).
     *  
     *  Remember to call Update() before using this method, otherwise old data will be sent to the
     *  shaders.
     * 
     *********************************************************************************************/
    public Vector4[] GetLightData() { return m_lightData; }

    //=============================================================================================
    /**
     *  @brief Initializes the LightManager with default values.
     *  
     *********************************************************************************************/
    public void Initialize()
    {
        m_fogVolume = gameObject.GetComponent<FogVolume>();
        m_fogVolumeData = FindObjectOfType<FogVolumeData>();
        m_camera = null;
        m_boxCollider = null;
        CurrentLightCount = 0;
        DrawDebugData = false;

        if (m_lights == null)
        {
            m_lights = new List<LightData>(MaxLightCount);
            m_lightsInFrustum = new List<LightData>(MaxLightCount);

            for (int i = 0; i < MaxLightCount; i++)
            {
                m_lights.Add(new LightData());
                m_lightsInFrustum.Add(new LightData());
            }
        }
    }

    //=============================================================================================
    /**
     *  @brief Clears the LightManager and prepares it toi be reinitialized.
     *  
     *********************************************************************************************/
    public void Deinitialize()
    {
        VisibleLightCount = 0;
        DrawDebugData = false;
    }


    public void SetFrustumPlanes(ref Plane[] _frustumPlanes) { FrustumPlanes = _frustumPlanes; }

#if UNITY_EDITOR
    private void Update() { hideFlags = HideFlags.HideInInspector; }
#endif

    //=============================================================================================
    /**
     *  @brief Updates the axis aligned bounding boxes of all registered lights.
     *  
     *********************************************************************************************/
    private void _UpdateBounds()
    {
        int count = m_lights.Count;
        for (int i = 0; i < count; i++)
        {
            LightData data = m_lights[i];

            if (data.LightType == EFogVolumeLightType.None) { continue; }


            switch (data.LightType)
            {
                case EFogVolumeLightType.None:
                {
                    break;
                }
                case EFogVolumeLightType.PointLight:
                {
                    data.Bounds = new Bounds(data.Transform.position,
                                             Vector3.one * data.Light.range *
                                             m_pointLightCullSizeMultiplier);
                    break;
                }
                case EFogVolumeLightType.SpotLight:
                {
                    Vector3 center = data.Transform.position +
                                     data.Transform.forward * data.Light.range * 0.5f;
                    data.Bounds = new Bounds(center,
                                             Vector3.one * data.Light.range *
                                             m_pointLightCullSizeMultiplier * 1.25f);
                    break;
                }
                case EFogVolumeLightType.FogVolumePointLight:
                {
                    data.Bounds = new Bounds(data.Transform.position,
                                             Vector3.one * data.FogVolumeLight.Range *
                                             m_pointLightCullSizeMultiplier);
                    break;
                }
                case EFogVolumeLightType.FogVolumeSpotLight:
                {
                    Vector3 center = data.Transform.position +
                                     data.Transform.forward * data.FogVolumeLight.Range * 0.5f;
                    data.Bounds = new Bounds(center,
                                             Vector3.one * data.FogVolumeLight.Range *
                                             m_pointLightCullSizeMultiplier * 1.25f);
                    break;
                }
            }
        }
    }

    //=============================================================================================
    /**
     *  @brief Finds the first free light in the list of all lights.
     *  
     *  Note that this method is necessary to ensure that adding/removing lights does not allocate
     *  any garbage.
     *  
     *********************************************************************************************/
    private int _FindFirstFreeLight()
    {
        if (CurrentLightCount < MaxLightCount)
        {
            int count = m_lights.Count;
            for (int i = 0; i < count; i++)
            {
                if (m_lights[i].LightType == EFogVolumeLightType.None) { return i; }
            }
        }

        return InvalidIndex;
    }

    //=============================================================================================
    /**
     *  @brief Finds all lights that are currently in the view frustum of the camera and calculates
     *         all necessary data for them.
     * 
     *********************************************************************************************/
    private void _FindLightsInFrustum()
    {
        m_inFrustumCount = 0;
        Vector3 CameraPos = m_camera.gameObject.transform.position;
        int count = m_lights.Count;
        for (int i = 0; i < count; i++)
        {
            if (m_lights[i].Transform == null) { m_lights[i].LightType = EFogVolumeLightType.None; }

            if (m_lights[i].LightType == EFogVolumeLightType.None) { continue; }

            float distanceToCamera = (m_lights[i].Transform.position - CameraPos).magnitude;
            if (distanceToCamera > m_fogVolume.PointLightingDistance2Camera) { continue; }

            switch (m_lights[i].LightType)
            {
                case EFogVolumeLightType.None:
                {
                    continue;
                }
                case EFogVolumeLightType.PointLight:
                case EFogVolumeLightType.SpotLight:
                {
                    if (m_lights[i].Light.enabled == false) { continue; }

                    break;
                }
                case EFogVolumeLightType.FogVolumePointLight:
                {
                    if (m_lights[i].FogVolumeLight.Enabled == false) { continue; }

                    break;
                }
                case EFogVolumeLightType.FogVolumeSpotLight:
                {
                    if (m_lights[i].FogVolumeLight.Enabled == false) { continue; }

                    break;
                }
            }

            if (GeometryUtility.TestPlanesAABB(FrustumPlanes, m_lights[i].Bounds))
            {
                LightData light = m_lights[i];
                Vector3 lightPos = light.Transform.position;
                light.SqDistance = (lightPos - m_pointOfInterest).sqrMagnitude;
                light.Distance2Camera = (lightPos - CameraPos).magnitude;
                m_lightsInFrustum[m_inFrustumCount++] = light;

                if (light.FogVolumeLight != null)
                {
                    if (light.LightType == EFogVolumeLightType.FogVolumePointLight &&
                        !light.FogVolumeLight.IsPointLight)
                    {
                        light.LightType = EFogVolumeLightType.FogVolumeSpotLight;
                    }
                    else if (light.LightType == EFogVolumeLightType.FogVolumeSpotLight &&
                             light.FogVolumeLight.IsPointLight)
                    {
                        light.LightType = EFogVolumeLightType.FogVolumePointLight;
                    }
                }
            }
        }
    }

    //=============================================================================================
    /**
     *  @brief Sorts the lights that are currently within the view frustum of the camera by 
     *         distance to the camera.
     *         
     *  This method will only be called when there are more than MaxVisibleLights in the view 
     *  frustum of the camera.
     * 
     *********************************************************************************************/
    private void _SortLightsInFrustum()
    {
        bool finishedSorting = false;
        do
        {
            finishedSorting = true;
            for (int i = 0; i < m_inFrustumCount - 1; i++)
            {
                if (m_lightsInFrustum[i].SqDistance > m_lightsInFrustum[i + 1].SqDistance)
                {
                    LightData tempData = m_lightsInFrustum[i];
                    m_lightsInFrustum[i] = m_lightsInFrustum[i + 1];
                    m_lightsInFrustum[i + 1] = tempData;
                    finishedSorting = false;
                }
            }
        }
        while (!finishedSorting);
    }

    //=============================================================================================
    /**
     *  @brief Prepares the data of the currently visible light and writes it to the arrays that
     *         can then be sent to the shaders.
     * 
     *********************************************************************************************/
    private void _PrepareShaderArrays()
    {
        VisibleLightCount = 0;
        for (int i = 0; i < MaxVisibleLights; i++)
        {
            if (i >= m_inFrustumCount) { break; }

            LightData data = m_lightsInFrustum[i];

            switch (data.LightType)
            {
                case EFogVolumeLightType.FogVolumePointLight:
                {
                    FogVolumeLight light = data.FogVolumeLight;
                    m_lightPos[i] =
                            gameObject.transform.InverseTransformPoint(data.Transform.position);
                    m_lightRot[i] =
                            gameObject.transform.InverseTransformVector(data.Transform.forward);
                    m_lightColor[i] = light.Color;
                    m_lightData[i] =
                            new Vector4(light.Intensity * m_fogVolume.PointLightsIntensity *
                                        (1.0f - Mathf.Clamp01(data.Distance2Camera / m_fogVolume
                                                                      .PointLightingDistance2Camera)
                                        ),
                                        light.Range / PointLightRangeDivider,
                                        InvalidSpotLightAngle,
                                        NoData);
                    VisibleLightCount++;
                    break;
                }
                case EFogVolumeLightType.FogVolumeSpotLight:
                {
                    FogVolumeLight light = data.FogVolumeLight;
                    m_lightPos[i] =
                            gameObject.transform.InverseTransformPoint(data.Transform.position);
                    m_lightRot[i] =
                            gameObject.transform.InverseTransformVector(data.Transform.forward);
                    m_lightColor[i] = light.Color;
                    m_lightData[i] =
                            new Vector4(light.Intensity * m_fogVolume.PointLightsIntensity *
                                        (1.0f - Mathf.Clamp01(data.Distance2Camera / m_fogVolume
                                                                      .PointLightingDistance2Camera)
                                        ),
                                        light.Range / SpotLightRangeDivider,
                                        light.Angle,
                                        NoData);
                    VisibleLightCount++;
                    break;
                }
                case EFogVolumeLightType.PointLight:
                {
                    Light light = data.Light;
                    m_lightPos[i] =
                            gameObject.transform.InverseTransformPoint(data.Transform.position);
                    m_lightRot[i] =
                            gameObject.transform.InverseTransformVector(data.Transform.forward);
                    m_lightColor[i] = light.color;
                    m_lightData[i] =
                            new Vector4(light.intensity * m_fogVolume.PointLightsIntensity *
                                        (1.0f - Mathf.Clamp01(data.Distance2Camera / m_fogVolume
                                                                      .PointLightingDistance2Camera)
                                        ),
                                        light.range / PointLightRangeDivider,
                                        InvalidSpotLightAngle,
                                        NoData);
                    VisibleLightCount++;
                    break;
                }
                case EFogVolumeLightType.SpotLight:
                {
                    Light light = data.Light;
                    m_lightPos[i] =
                            gameObject.transform.InverseTransformPoint(data.Transform.position);
                    m_lightRot[i] =
                            gameObject.transform.InverseTransformVector(data.Transform.forward);
                    m_lightColor[i] = light.color;
                    m_lightData[i] =
                            new Vector4(light.intensity * m_fogVolume.PointLightsIntensity *
                                        (1.0f - Mathf.Clamp01(data.Distance2Camera / m_fogVolume
                                                                      .PointLightingDistance2Camera)
                                        ),
                                        light.range / SpotLightRangeDivider,
                                        light.spotAngle,
                                        NoData);
                    VisibleLightCount++;

                    break;
                }
                case EFogVolumeLightType.None:
                {
                    break;
                }
            }
        }
    }

    private float m_pointLightCullSizeMultiplier = 1.0f;

    private FogVolume m_fogVolume = null;

    private FogVolumeData m_fogVolumeData = null;

    private Camera m_camera = null;

    private BoxCollider m_boxCollider = null;

    private Transform m_pointOfInterestTf = null;

    private Vector3 m_pointOfInterest = Vector3.zero;

    private readonly Vector4[] m_lightPos = new Vector4[MaxVisibleLights];

    private readonly Vector4[] m_lightRot = new Vector4[MaxVisibleLights];

    private readonly Color[] m_lightColor = new Color[MaxVisibleLights];

    private readonly Vector4[] m_lightData = new Vector4[MaxVisibleLights];

    private List<LightData> m_lights = null;

    private List<LightData> m_lightsInFrustum = null;

    private int m_inFrustumCount = 0;

    private Plane[] FrustumPlanes = null;

    private const int InvalidIndex = -1;

    private const int MaxVisibleLights = 64;

    private const float InvalidSpotLightAngle = -1.0f;

    private const float NoData = 0.0f;

    private const float PointLightRangeDivider = 5.0f;

    private const float SpotLightRangeDivider = 5.0f;

    private const int MaxLightCount = 1000;

    /// The assumed size of a light. Used by realtime search when searching for lights inside a FV.
    private const float LightInVolumeBoundsSize = 5.0f;

    protected class LightData
    {
        public LightData()
        {
            LightType = EFogVolumeLightType.None;
            Light = null;
            FogVolumeLight = null;
            Transform = null;
            SqDistance = 0.0f;
            Distance2Camera = 0.0f;
            Bounds = new Bounds();
        }

        public EFogVolumeLightType LightType { get; set; }

        public Light Light { get; set; }

        public FogVolumeLight FogVolumeLight { get; set; }

        public Transform Transform { get; set; }

        public float SqDistance { get; set; }

        public float Distance2Camera { get; set; }

        public Bounds Bounds { get; set; }
    }
}
