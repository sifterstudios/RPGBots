using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneMenu : MonoBehaviour {


	public string[] sceneNames;
    public float Height = 70;

    public Transform prefabButton;
	private Canvas canvas;


    void FixedUpdate()
	{
		if (Input.GetKey(KeyCode.Escape)) Application.Quit();
    }
    
	void Start () {


		//canvas = FindObjectOfType<Canvas>();//noo!
        canvas = GetComponent<Canvas>();

        for (int i= 0; i < sceneNames.Length; i++ ) {

			Transform trans = Instantiate(prefabButton, Vector3.zero, Quaternion.identity);
			trans.SetParent( canvas.transform);
			trans.position -= Vector3.up * i * 50;//el último valor es el margen entre botones
            trans.position += Vector3.up*Height;
            trans.GetComponentInChildren<Text>().text = sceneNames[i];
			string a = sceneNames[i];
			trans.GetComponent<Button>().onClick.AddListener(() => onClick(a));
		}
	}
		
	void onClick(string _nameScene)
	{

		SceneManager.LoadScene(_nameScene, LoadSceneMode.Single);

	}



}
