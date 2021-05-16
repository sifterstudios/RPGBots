using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class ColorAnimation : MonoBehaviour {

    float X, Y, Z;
    [Range(0, 10)]
    public float _ColorSpeed = 6;
    float ColorSpeed;
    Vector3 RandomRangeXYZ;
    [SerializeField]
    [Range(1, 300)]
    float Intensity=8; 
    void OnEnable ()
    {
        RandomRangeXYZ.x = Random.Range(0f, 1f);
        RandomRangeXYZ.y = Random.Range(0f, 1f);
        RandomRangeXYZ.z = Random.Range(0f, 1f);
    }
	
	// Update is called once per frame
	void Update () {
        ColorSpeed += Time.deltaTime * _ColorSpeed;
        X = Mathf.Sin(ColorSpeed * RandomRangeXYZ.x) * 0.5f + 0.5f;
        Y = Mathf.Sin(ColorSpeed * RandomRangeXYZ.y) * 0.5f + 0.5f;
        Z = Mathf.Sin(ColorSpeed * RandomRangeXYZ.z) * 0.5f + 0.5f;
        float IntensityVariable = Mathf.Sin(ColorSpeed * RandomRangeXYZ.z) * 0.5f + 0.5f;
       
        Color RandomColor = new Color(X, Y, Z, 1);
        GetComponent<Renderer>().sharedMaterial.SetColor("_EmissionColor", IntensityVariable*Intensity * RandomColor);
    }
}
