using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class SunDirection : MonoBehaviour {
    public Material _Material;
    public bool RealtimeUpdate = false;
    public Vector3 L;
    void SetVector()
    {
        if (_Material)
        {
            L = transform.forward;
            _Material.SetVector("_L", -L);
        }
    }
	void OnEnable () {
        SetVector();
	}
	
	// Update is called once per frame
	void Update () {
        if(RealtimeUpdate)
        SetVector();
    }
}
