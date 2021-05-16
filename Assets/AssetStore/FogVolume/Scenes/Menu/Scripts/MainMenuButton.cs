using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuButton : MonoBehaviour {

	public static GameObject menuInstance;

	void Awake()
	{
		DontDestroyOnLoad(this.gameObject);

		if (menuInstance == null) {
			menuInstance = this.gameObject;
		} else {
			Destroy(this.gameObject);
		}
	}

	// Use this for initialization
	void Start () {

	}


	void Update() {

		if (Input.GetKey (KeyCode.Escape)) {

			if (SceneManager.GetActiveScene ().buildIndex != 0)
				SceneManager.LoadScene (0, LoadSceneMode.Single);
			else
				Application.Quit ();
		}

	}

	public void LoadMainMenu()
	{
		SceneManager.LoadScene(0, LoadSceneMode.Single);
	}



}
