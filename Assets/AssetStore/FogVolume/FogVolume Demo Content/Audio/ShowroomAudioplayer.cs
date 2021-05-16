using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class ShowroomAudioplayer : MonoBehaviour
{
   // public float fade
    public AudioSource ShowroomAudio;
    void OnEnable()
    {
        ShowroomAudio = GetComponent<AudioSource>();
        float TrackLength = ShowroomAudio.clip.length;
        float PlayStart = Random.Range(2, TrackLength);
        ShowroomAudio.time = PlayStart;

    }

    // Update is called once per frame
    void Update()
    {

    }
}
