using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
public class FogVolumeLight : MonoBehaviour
{
    private void OnEnable()
    {
        SphereCollider collider = GetComponent<SphereCollider>();
#if UNITY_EDITOR
        if (collider != null) { DestroyImmediate(collider); }
#else
        if (collider != null) { Destroy(collider); }
#endif
    }

    public bool IsAddedToNormalLight;
    public bool IsPointLight;
    public bool Enabled = true;
    public Color Color = Color.white;
    public float Intensity = 1.0f;
    public float Range = 10.0f;
    public float Angle = 30.0f;
}