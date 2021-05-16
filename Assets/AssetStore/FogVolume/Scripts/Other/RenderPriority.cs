using UnityEngine;
using System.Collections;

namespace FogVolumeRenderPriority
{
    [ExecuteInEditMode]
    public class RenderPriority : MonoBehaviour {
    public int DrawOrder = 0;
    public bool UpdateRealTime = false;
	// Use this for initialization
	void OnEnable () {
        gameObject.GetComponent<Renderer>().sortingOrder = DrawOrder;
	}
	
	// Update is called once per frame
	void Update () {
        if(UpdateRealTime)
            gameObject.GetComponent<Renderer>().sortingOrder = DrawOrder;
    }
}
}