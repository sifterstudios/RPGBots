using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]

public class FogVolumeData : MonoBehaviour
{
    [SerializeField]
    bool _ForceNoRenderer;
    public bool ForceNoRenderer
    {
        get { return _ForceNoRenderer; }
        set
        {
            if (_ForceNoRenderer != value)
            {
                _ForceNoRenderer = value;
                ToggleFogVolumeRenderers();
            }
        }
    }
    [SerializeField]
    Camera _GameCamera;

    public Camera GameCamera
    {
        get { return _GameCamera; }
        set
        {
            if (_GameCamera != value)
            {
                _GameCamera = value;
                RefreshCamera();
            }
        }
    }

	public void setDownsample(int val)
	{
		if (_GameCamera.GetComponent<FogVolumeRenderer>())
			_GameCamera.GetComponent<FogVolumeRenderer>()._Downsample = val;
	}

    void RefreshCamera()
    {
        //refresh all fog folumes assigned camera
        //print("Refresh");

        FindFogVolumes();
        foreach (FogVolume _FogVolumes in SceneFogVolumes)
        {
            _FogVolumes.AssignCamera();
        }
        ToggleFogVolumeRenderers();
    }

    [SerializeField]
    List<Camera> FoundCameras;

    void OnEnable()
    {
        Initialize();
       
    }

    void Initialize()
    {
        if (FoundCameras == null)
            FoundCameras = new List<Camera>();
        FindCamera();
        RefreshCamera();
        if (FoundCameras.Count == 0)
            Debug.Log("Definetly, no camera available for Fog Volume");
    }

    [SerializeField]
    FogVolume[] SceneFogVolumes;

  	public void FindFogVolumes()
    {
        SceneFogVolumes = (FogVolume[])FindObjectsOfType(typeof(FogVolume));
    }
  
    void Update()
    {

        if (GameCamera == null)
        {
            Debug.Log("No Camera available for Fog Volume. Trying to find another one");
            Initialize();
        }

        #if UNITY_EDITOR
        for (int i = 0; i < SceneFogVolumes.Length; i++)
        {
            FogVolume SlotFogVolume = SceneFogVolumes[i];
            if(SlotFogVolume==null)
            {
                //reset and rebuild
                SceneFogVolumes = null;
                FindFogVolumes();
            }
        }

        
                if (SceneFogVolumes.Length == 0)
			        DestroyImmediate(gameObject);
#endif

        
    }
    void ToggleFogVolumeRenderers()
    {
        if (FoundCameras != null)
            for (int i = 0; i < FoundCameras.Count; i++)
            {
                if (FoundCameras[i] != _GameCamera)
                {
                    if (FoundCameras[i].GetComponent<FogVolumeRenderer>())
                        FoundCameras[i].GetComponent<FogVolumeRenderer>().enabled = false;
                }
                else if (FoundCameras[i].GetComponent<FogVolumeRenderer>() &&
                         !_ForceNoRenderer)
                {
                    FoundCameras[i].GetComponent<FogVolumeRenderer>().enabled = true;
                }
                else
                {
					var FVRenderer = FoundCameras[i].GetComponent<FogVolumeRenderer>();
                    if (FVRenderer == null)
                    {
                        if (ForceNoRenderer) { continue; }
                        FVRenderer = FoundCameras[i].gameObject.AddComponent<FogVolumeRenderer>();
                    }

					if(ForceNoRenderer)
					{
						FVRenderer.enabled = false;
					}
                }
            }
    }
    public void FindCamera()
    {
        //We will try to assign the typical MainCamera first. This search will be performed only when the field is null
        //This is just an initial attempt on assigning any camera available when the field 'Camera' is null.
        //We will be able to select any other camera later
        if (FoundCameras != null && FoundCameras.Count > 0) FoundCameras.Clear();
        //Find all cameras in scene and store
        Camera[] CamerasFound = (Camera[])FindObjectsOfType(typeof(Camera));
        for (int i = 0; i < CamerasFound.Length; i++)
            if (
                !CamerasFound[i].name.Contains("FogVolumeCamera")
                &&
                !CamerasFound[i].name.Contains("Shadow Camera")
                &&
                    CamerasFound[i].gameObject.hideFlags == HideFlags.None)//not you!
                FoundCameras.Add(CamerasFound[i]);

        if (GameCamera == null)
            GameCamera = Camera.main;

        //No MainCamera? Try to find any!
        if (GameCamera == null)
        {
            foreach (Camera FoundCamera in FoundCameras)
            {
                // Many effects may use hidden cameras, so let's filter a little bit until we get something valid
                if (FoundCamera.isActiveAndEnabled)
                    if (FoundCamera.gameObject.activeInHierarchy)
                        if (FoundCamera.gameObject.hideFlags == HideFlags.None)
                        {
                            GameCamera = FoundCamera;
                            break;
                        }
            }
        }

        if (GameCamera != null)
        {
            // Debug.Log("Fog Volume has been assigned with camera: " + GameCamera);
            //if (FindObjectOfType<FogVolumeCamera>())
            //    FindObjectOfType<FogVolumeCamera>().SceneCamera = GameCamera;

			//NOTE: This makes sure we have a depth texture which will be either free (deferred, etc) or internally generated through a replacement shader
			//Now, objects must be able to do shadow casting. If you’re using surface shaders, add the  "addshadow" directive 
			//only “opaque” objects (that which have their materials and shaders setup to use render queue <= 2500) are rendered into the depth texture.
			//GameCamera.depthTextureMode = DepthTextureMode.Depth;
        }

    }
    public Camera GetFogVolumeCamera
    {
        get
        {
            return GameCamera;
        }
    }

    void OnDisable()
    {
        FoundCameras.Clear();
        SceneFogVolumes = null;
    }
}
