using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerticalOscilator : MonoBehaviour
{
    Vector3 _Position;
    float _Time;
    float _Random = 0;
    [Range(0, 1)]
    public float Speed = 1;
    [Range(0, 1)]
    public float Amplitude = 1;
    // Use this for initialization
    void Start()
    {
        _Position = transform.position;
        _Random = Random.Range(0.1f, 10);
    }

    // Update is called once per frame
    void Update()
    {
        _Time += Time.deltaTime * Speed;
        _Position.y = Mathf.Sin(_Time+ _Random) * Amplitude + transform.position.y;
        gameObject.transform.position = _Position;
    }
}
