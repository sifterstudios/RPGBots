
using UnityEngine;


[ExecuteInEditMode]
public class FogVolumePrimitive : MonoBehaviour
{
    public FogVolumePrimitive()
    {
        SphereColl = null;
        BoxColl = null;
    }

    public Transform GetTransform { get { return gameObject.transform; } }

    public Vector3 GetPrimitiveScale
    {
        get
        {
            return new Vector3(Mathf.Max(MinScale, transform.lossyScale.x),
                               Mathf.Max(MinScale, transform.lossyScale.y),
                               Mathf.Max(MinScale, transform.lossyScale.z));
        }
    }

    public Bounds Bounds
    {
        get
        {
            if (BoxColl != null) { return BoxColl.bounds; }
            else if (SphereColl != null) { return SphereColl.bounds; }
            else
            {
                return new Bounds(gameObject.transform.position, gameObject.transform.lossyScale);
            }
        }
    }

    public void AddColliderIfNeccessary(EFogVolumePrimitiveType _type)
    {
        Type = _type;
        switch (Type)
        {
            case EFogVolumePrimitiveType.None:
            {
                break;
            }
            case EFogVolumePrimitiveType.Box:
            {
                if (BoxColl == null) { BoxColl = gameObject.AddComponent<BoxCollider>(); }
                break;
            }
            case EFogVolumePrimitiveType.Sphere:
            {
                if (SphereColl == null) { SphereColl = gameObject.AddComponent<SphereCollider>(); }
                break;
            }
        }
    }

    private void OnEnable()
    {
        Primitive = gameObject;
        _Renderer = Primitive.GetComponent<MeshRenderer>();
        if (!PrimitiveMaterial)
        {
            PrimitiveMaterial = (Material) Resources.Load("PrimitiveMaterial");
        }
        _Renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
        _Renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        _Renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _Renderer.receiveShadows = false;

        GetComponent<MeshRenderer>().material = PrimitiveMaterial;

        BoxColl = GetComponent<BoxCollider>();
        SphereColl = GetComponent<SphereCollider>();

        if (BoxColl == null &&
            SphereColl == null)
        {
            BoxColl = gameObject.AddComponent<BoxCollider>();
            Type = EFogVolumePrimitiveType.Box;
        }
        else
        {
            if (BoxColl != null) { Type = EFogVolumePrimitiveType.Box; }
            else if (SphereColl != null) { Type = EFogVolumePrimitiveType.Sphere; }
            else { Type = EFogVolumePrimitiveType.None; }
        }
    }

    public BoxCollider BoxColl;

    public SphereCollider SphereColl;

    public bool IsPersistent = true;

    public EFogVolumePrimitiveType Type;

    public bool IsSubtractive;

    public Material PrimitiveMaterial;

    private GameObject Primitive;

    private Renderer _Renderer;

    private readonly float MinScale = .0001f;
}
