﻿using UnityEngine;
using MD_Plugin;
using UnityEngine.SceneManagement;

public class MD_Examples_Manager : MonoBehaviour
{
    [Multiline(5)]
    public string SceneDestription;
    public Color DescColor = Color.gray;

    public Texture2D Logo;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
            SceneManager.LoadScene("MDExample_Introduction");
        if (Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        if (Input.GetMouseButtonDown(0))
        {
            Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit h;
            if (!Physics.Raycast(r, out h)) return;
            if (!h.collider) return;
            if (h.collider.GetComponent<MDM_MeshDamage>()) h.collider.GetComponent<MDM_MeshDamage>().MeshDamage_ModifyMesh(h.point, 0.15f, 1.0f, transform.forward);
        }
    }

    private void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 25;
        style.richText = true;
        style.normal.textColor = Color.gray;
        GUILayout.BeginArea(new Rect(50, Screen.height - 80, 500, 100));
        GUILayout.Label("Press <b>ESC</b> to Menu\nPress <b>R</b> to Restart", style);
        GUILayout.EndArea();

        GUILayout.BeginArea(new Rect(10, 10, 500, 500));
        GUILayout.Label(Logo);
        GUILayout.EndArea();

        GUILayout.BeginArea(new Rect(Screen.width - 650, Screen.height - 200, 600, 300));
        style.fontSize = 20;
        style.normal.textColor = DescColor;
        GUILayout.Label(SceneDestription, style);
        GUILayout.EndArea();
    }
}
