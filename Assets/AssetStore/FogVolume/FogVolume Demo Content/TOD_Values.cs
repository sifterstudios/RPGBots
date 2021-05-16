using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class TOD_Values : MonoBehaviour
{

    public bool Active;
    public Color ambientLight;
    public Vector3 SunDirection = Vector3.zero;
    Light Sun;
    public float SunIntensity = 1;
    public Color SunColor = Color.white;
   
    void OnEnable()
    {
        
        Sun = gameObject.GetComponent<FogVolume>().Sun;
    }
    void Update()
    {
        if (this.Active && Sun)
        {
            Sun.transform.eulerAngles=SunDirection;
            Sun.color = SunColor;
            Sun.intensity = SunIntensity;
            RenderSettings.ambientLight = this.ambientLight;
        }
    }
}