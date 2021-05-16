using UnityEditor;
using System.IO;
using System.Linq;

namespace XeirGYA
{
    // Asset Post Processing
    public class GYAPostprocessor : AssetPostprocessor
    {
        static bool packageDetected = false;

        public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            //Check for imported unitypackages AND ignore changes to ProjectSettings.asset
            if (importedAssets.Any() && Path.GetFileName(importedAssets[0]).ToLower() != "projectsettings.asset")
            {
                foreach (string t in importedAssets)
                {
                    // Check if a unitypackage was extracted into the local project during import
                    if (Path.GetExtension(t).ToLower() == ".unitypackage")
                        packageDetected = true;
                }
            }

            // Check for deleted unitypackages
            if (deletedAssets.Any())
            {
                foreach (string t in deletedAssets)
                {
                    // Check if a unitypackage was deleted from the local project
                    if (Path.GetExtension(t).ToLower() == ".unitypackage")
                        packageDetected = true;
                }
            }

            // Check for moved unitypackages
            if (movedAssets.Any())
            {
                foreach (string t in movedAssets)
                {
                    // Check if a unitypackage was moved in the local project
                    if (Path.GetExtension(t).ToLower() == ".unitypackage")
                        packageDetected = true;
                }
            }

            if (movedFromAssetPaths.Any())
            {

            }

            // If a unitypackage was imported/deleted/moved, rescan the local project
            if (packageDetected)
            {
                packageDetected = false;
                if (GYA.Instance != null)
                {
                    GYAPackage.RefreshProject();
                    GYA.Instance.RefreshSV();
                }
            }
        }
    }
}