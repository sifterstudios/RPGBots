using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Assertions;


public class FogVolumePrimitiveManager : MonoBehaviour
{
    public int CurrentPrimitiveCount { get; private set; }

    public int VisiblePrimitiveCount { get; private set; }

    public bool AlreadyUsesTransformForPoI { get { return m_pointOfInterestTf != null; } }

    public void FindPrimitivesInFogVolume()
    {
        CurrentPrimitiveCount = 0;
        VisiblePrimitiveCount = 0;
        m_primitives.Clear();
        m_primitivesInFrustum.Clear();
        for (int i = 0; i < MaxPrimitivesCount; i++)
        {
            m_primitives.Add(new PrimitiveData());
            m_primitivesInFrustum.Add(new PrimitiveData());
        }

        if (m_boxCollider == null) { m_boxCollider = gameObject.GetComponent<BoxCollider>(); }
        Bounds boundingBox = m_boxCollider.bounds;

        FogVolumePrimitive[] primitives = FindObjectsOfType<FogVolumePrimitive>();
        for (int i = 0; i < primitives.Length; i++)
        {
            var data = primitives[i];

            if (boundingBox.Intersects(data.Bounds))
            {
                if (data.BoxColl != null) { data.Type = EFogVolumePrimitiveType.Box; }
                else if (data.SphereColl != null) { data.Type = EFogVolumePrimitiveType.Sphere; }
                else
                {
                    data.BoxColl = data.GetTransform.gameObject.AddComponent<BoxCollider>();
                    data.Type = EFogVolumePrimitiveType.Box;
                }

                if (data.Type == EFogVolumePrimitiveType.Box)
                {
                    AddPrimitiveBox(data);
                }
                else if (data.Type == EFogVolumePrimitiveType.Sphere)
                {
                    AddPrimitiveSphere(data);
                }
            }
        }
    }

    public bool AddPrimitiveBox(FogVolumePrimitive _box)
    {
        Assert.IsTrue(CurrentPrimitiveCount < MaxPrimitivesCount,
                      "The maximum amount of primitives is already reached!");

        int index = _FindFirstFreePrimitive();
        if (index != InvalidIndex)
        {
            PrimitiveData data = m_primitives[index];
            CurrentPrimitiveCount++;
            data.PrimitiveType = EFogVolumePrimitiveType.Box;
            data.Transform = _box.transform;
            data.Renderer = _box.GetComponent<Renderer>();
            data.Primitive = _box;
            data.Bounds = new Bounds(data.Transform.position, _box.GetPrimitiveScale);
            return true;
        }

        return false;
    }

    public bool AddPrimitiveSphere(FogVolumePrimitive _sphere)
    {
        Assert.IsTrue(CurrentPrimitiveCount < MaxPrimitivesCount,
                      "The maximum amount of primitives is already reached!");

        int index = _FindFirstFreePrimitive();
        if (index != InvalidIndex)
        {
            PrimitiveData data = m_primitives[index];
            CurrentPrimitiveCount++;
            data.PrimitiveType = EFogVolumePrimitiveType.Sphere;
            data.Transform = _sphere.transform;
            data.Renderer = _sphere.GetComponent<Renderer>();
            data.Primitive = _sphere;
            data.Bounds = new Bounds(data.Transform.position, _sphere.GetPrimitiveScale);
            return true;
        }

        return false;
    }

    public bool RemovePrimitive(Transform _primitiveToRemove)
    {
        int count = m_primitives.Count;
        for (int i = 0; i < count; i++)
        {
            PrimitiveData data = m_primitives[i];
            if (ReferenceEquals(m_primitives[i].Transform, _primitiveToRemove))
            {
                data.Reset();
                CurrentPrimitiveCount--;
                return true;
            }
        }

        return false;
    }


    //=============================================================================================
    /**
     *  @brief Sets the point of interest to a fixed position.
     *
     *  @param _pointOfInterest The point that will be used for prioritizing which primitives need
     *                          to be rendered.
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


    public void OnDrawGizmos() { hideFlags = HideFlags.HideInInspector; }


    public void ManualUpdate(ref Plane[] _frustumPlanes)
    {
        m_camera = m_fogVolumeData != null ? m_fogVolumeData.GameCamera : null;
        if (m_camera == null) { return; }
        FrustumPlanes = _frustumPlanes;
        if (m_boxCollider == null) { m_boxCollider = m_fogVolume.GetComponent<BoxCollider>(); }
        _UpdateBounds();
        _FindPrimitivesInFrustum();
        if (m_primitivesInFrustum.Count > MaxVisiblePrimitives) { _SortPrimitivesInFrustum(); }
        _PrepareShaderArrays();
    }

    public void SetVisibility(bool _enabled)
    {
        int count = m_primitives.Count;
        for (int i = 0; i < count; i++)
        {
            if (m_primitives[i].Renderer != null) { m_primitives[i].Renderer.enabled = _enabled; }
        }
    }

    public void Initialize()
    {
        m_fogVolume = gameObject.GetComponent<FogVolume>();
        m_fogVolumeData = FindObjectOfType<FogVolumeData>();
        m_camera = null;
        m_boxCollider = null;
        CurrentPrimitiveCount = 0;

        if (m_primitives == null)
        {
            m_primitives = new List<PrimitiveData>(MaxPrimitivesCount);
            m_primitivesInFrustum = new List<PrimitiveData>();

            for (int i = 0; i < MaxPrimitivesCount; i++)
            {
                m_primitives.Add(new PrimitiveData());
                m_primitivesInFrustum.Add(new PrimitiveData());
            }
        }
    }


    public void Deinitialize() { VisiblePrimitiveCount = 0; }


    public Vector4[] GetPrimitivePositionArray() { return m_primitivePos; }


    public Vector4[] GetPrimitiveScaleArray() { return m_primitiveScale; }


    public Matrix4x4[] GetPrimitiveTransformArray() { return m_primitiveTf; }


    public Vector4[] GetPrimitiveDataArray() { return m_primitiveData; }


    private void _UpdateBounds()
    {
        int count = m_primitives.Count;
        for (int i = 0; i < count; i++)
        {
            PrimitiveData data = m_primitives[i];

            if (data.PrimitiveType == EFogVolumePrimitiveType.None) { continue; }

            if (data.Primitive == null)
            {
                RemovePrimitive(data.Transform);
                continue;
            }

            if (data.PrimitiveType == EFogVolumePrimitiveType.Box)
            {
                // Check if collider was removed by the user and add it again.
                if (data.Primitive.BoxColl == null)
                {
                    Debug.LogWarning("FogVolumePrimitive requires a collider.\nThe collider will be automatically created.");
                    data.Primitive.AddColliderIfNeccessary(EFogVolumePrimitiveType.Box);
                }
                data.Bounds = data.Primitive.BoxColl.bounds;

            }
            else if (data.PrimitiveType == EFogVolumePrimitiveType.Sphere)
            {
                // Check if collider was removed by the user and add it again.
                if (data.Primitive.SphereColl == null)
                {
                    Debug.LogWarning("FogVolumePrimitive requires a collider.\nThe collider will be automatically created.");
                    data.Primitive.AddColliderIfNeccessary(EFogVolumePrimitiveType.Sphere);
                }
                data.Bounds = data.Primitive.SphereColl.bounds;

            }
        }
    }

    private int _FindFirstFreePrimitive()
    {
        if (CurrentPrimitiveCount < MaxPrimitivesCount)
        {
            int count = m_primitives.Count;
            for (int i = 0; i < count; i++)
            {
                if (m_primitives[i].PrimitiveType == EFogVolumePrimitiveType.None) { return i; }
            }
        }

        return InvalidIndex;
    }

    private void _FindPrimitivesInFrustum()
    {
        m_inFrustumCount = 0;
        Vector3 cameraPos = m_camera.gameObject.transform.position;
        int count = m_primitives.Count;
        for (int i = 0; i < count; i++)
        {
            PrimitiveData primitive = m_primitives[i];

            if (primitive.Transform == null)
            {
                primitive.PrimitiveType = EFogVolumePrimitiveType.None;
            }
            if (primitive.PrimitiveType == EFogVolumePrimitiveType.None) { continue; }

            if (primitive.Primitive.IsPersistent)
            {
                Vector3 pos = primitive.Transform.position;
                primitive.SqDistance = (pos - m_pointOfInterest).sqrMagnitude;
                primitive.Distance2Camera = (pos - cameraPos).magnitude;
                m_primitivesInFrustum[m_inFrustumCount++] = primitive;
            }
            else if (GeometryUtility.TestPlanesAABB(FrustumPlanes, m_primitives[i].Bounds))
            {
                Vector3 pos = primitive.Transform.position;
                primitive.SqDistance = (pos - m_pointOfInterest).sqrMagnitude;
                primitive.Distance2Camera = (pos - cameraPos).magnitude;
                m_primitivesInFrustum[m_inFrustumCount++] = primitive;
            }
        }
    }

    private void _SortPrimitivesInFrustum()
    {
        bool finishedSorting = false;
        do
        {
            finishedSorting = true;
            for (int i = 0; i < m_inFrustumCount - 1; i++)
            {
                if (m_primitivesInFrustum[i].SqDistance > m_primitivesInFrustum[i + 1].SqDistance)
                {
                    PrimitiveData tempData = m_primitivesInFrustum[i];
                    m_primitivesInFrustum[i] = m_primitivesInFrustum[i + 1];
                    m_primitivesInFrustum[i + 1] = tempData;
                    finishedSorting = false;
                }
            }
        }
        while (!finishedSorting);
    }

    private void _PrepareShaderArrays()
    {
        VisiblePrimitiveCount = 0;

        Quaternion fogVolumeRotation = m_fogVolume.gameObject.transform.rotation;

        for (int i = 0; i < MaxVisiblePrimitives; i++)
        {
            if (i >= m_inFrustumCount) { break; }

            PrimitiveData data = m_primitivesInFrustum[i];
            Vector3 position = data.Transform.position;
            m_primitivePos[i] = gameObject.transform.InverseTransformPoint(position);
            m_primitiveTf[i].SetTRS(position,
                                    Quaternion.Inverse(data.Transform.rotation) * fogVolumeRotation,
                                    Vector3.one);
            m_primitiveScale[i] = data.Primitive.GetPrimitiveScale;
            m_primitiveData[i] =
                    new Vector4(data.PrimitiveType == EFogVolumePrimitiveType.Box ? 0.5f : 1.5f,
                                data.Primitive.IsSubtractive ? 1.5f : 0.5f,
                                0.0f,
                                0.0f);
            VisiblePrimitiveCount++;
        }
    }

#if UNITY_EDITOR
    private void Update() { hideFlags = HideFlags.HideInInspector; }
#endif

    private FogVolume m_fogVolume = null;

    private FogVolumeData m_fogVolumeData = null;

    private Camera m_camera = null;

    private BoxCollider m_boxCollider = null;

    private Transform m_pointOfInterestTf = null;

    private Vector3 m_pointOfInterest = Vector3.zero;

    private List<PrimitiveData> m_primitives = null;

    private List<PrimitiveData> m_primitivesInFrustum = null;

    private int m_inFrustumCount = 0;

    private Plane[] FrustumPlanes = null;

    private readonly Vector4[] m_primitivePos = new Vector4[MaxVisiblePrimitives];

    private readonly Vector4[] m_primitiveScale = new Vector4[MaxVisiblePrimitives];

    private readonly Matrix4x4[] m_primitiveTf = new Matrix4x4[MaxVisiblePrimitives];

    private readonly Vector4[] m_primitiveData = new Vector4[MaxVisiblePrimitives];

    private const int InvalidIndex = -1;

    private const int MaxVisiblePrimitives = 20;

    private const int MaxPrimitivesCount = 1000;

    protected class PrimitiveData
    {
        public PrimitiveData()
        {
            PrimitiveType = EFogVolumePrimitiveType.None;
            Primitive = null;
            Transform = null;
            Renderer = null;
            SqDistance = 0.0f;
            Distance2Camera = 0.0f;
            Bounds = new Bounds();
        }

        public EFogVolumePrimitiveType PrimitiveType { get; set; }

        public FogVolumePrimitive Primitive { get; set; }

        public Transform Transform { get; set; }

        public Renderer Renderer { get; set; }

        public float SqDistance { get; set; }

        public float Distance2Camera { get; set; }

        public Bounds Bounds { get; set; }

        public void Reset()
        {
            PrimitiveType = EFogVolumePrimitiveType.None;
            Primitive = null;
            Transform = null;
            Renderer = null;
            SqDistance = 0.0f;
            Distance2Camera = 0.0f;
        }
    }
}
