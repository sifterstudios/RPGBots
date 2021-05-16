using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class ToggleChildren : MonoBehaviour {
    public KeyCode Key = KeyCode.F;
    // Use this for initialization
    GameObject[] Children=null;
    bool active=true;
	void OnEnable () {
        Children = new GameObject[gameObject.transform.childCount];
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            Children[i] = gameObject.transform.GetChild(i).gameObject;
        }
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(Key))
        {
            active = !active;
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                Children[i].SetActive(active);
            }

        }
        
          

    }
}
