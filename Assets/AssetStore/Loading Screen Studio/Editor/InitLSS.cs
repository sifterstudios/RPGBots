#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace Michsky.LSS
{
    public class InitLSS
    {
        [InitializeOnLoad]
        public class InitOnLoad
        {
            static InitOnLoad()
            {
                if (!EditorPrefs.HasKey("LSS.Installed"))
                {
                    EditorPrefs.SetInt("LSS.Installed", 1);
                    EditorUtility.DisplayDialog("Hello there!", "Thank you for purchasing Loading Screen Studio.\r\r" +
                        "First of all, import TextMesh Pro from Package Manager if you haven't already." +
                        "\r\rYou can contact me at support@michsky.com for support.", "Got it!");

                    PlayerPrefs.SetString("LSS_SelectedLS", "Standard");    
                }         
            }
        }
    }
}
#endif