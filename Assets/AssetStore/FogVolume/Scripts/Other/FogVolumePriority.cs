using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class FogVolumePriority : MonoBehaviour {
    public Camera GameCamera;
    public int FogOrderCameraAbove=1;
    public int FogOrderCameraBelow=-1;
    public float HeightThreshold=30;
    public FogVolume thisFog;
    FogVolumeData _FogVolumeData;
    public float CurrentHeight;
   // public bool AutoAssignCameraCurrent = true;
    public GameObject Horizon;
    // Use this for initialization
    void OnEnable () {
        
        thisFog = GetComponent<FogVolume>();
        _FogVolumeData = thisFog._FogVolumeData;
       
    }

    // Update is called once per frame
    void Update()
    {
        // if (AutoAssignCameraCurrent)
        // GameCamera = Camera.current;
        if(Horizon)
        HeightThreshold = Horizon.transform.position.y;
        //if(!Application.isPlaying)
        //    GameCamera = Camera.current;
        if (_FogVolumeData)
            GameCamera = _FogVolumeData.GameCamera;
        else
            GameCamera = Camera.main;

        if (GameCamera)
        {
            if (!Application.isPlaying)
            {
                if (Camera.current != null)

                    CurrentHeight = Camera.current.gameObject.transform.position.y;
            }
            else
                CurrentHeight = GameCamera.gameObject.transform.position.y;
            if (HeightThreshold > CurrentHeight && Horizon != null)
                thisFog.DrawOrder = FogOrderCameraBelow;
            else
                thisFog.DrawOrder = FogOrderCameraAbove;
        }
    }
}
