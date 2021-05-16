using UnityEngine;
using System.Collections;
[ExecuteInEditMode]
public class EnableDepthInForwardCamera : MonoBehaviour {
    public string Message = "Add this script to generate depth if Fog Volume rendered is not used and rendering path is forward";
	// Use this for initialization
	void OnEnable() {
       
            
    }

    // Update is called once per frame
    void Update()
    {

        if (GetComponent<Camera>().depthTextureMode != DepthTextureMode.Depth)
            GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;
    }


}
