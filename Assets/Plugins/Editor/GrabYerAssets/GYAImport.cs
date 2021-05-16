#if (UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5)
#define UNITY_3
#endif

#if (UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7)
#define UNITY_4
#endif

#if UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
#define UNITY_4_2_OR_NEWER
#endif

#if UNITY_4 || UNITY_5_0
#define UNITY_5_0_OR_OLDER
#endif

#if UNITY_4 || UNITY_5_0 || UNITY_5_1
#define UNITY_5_1_OR_OLDER
#endif

#if UNITY_4 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
#define UNITY_5_2_OR_OLDER
#endif

#if UNITY_4 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3
#define UNITY_5_3_OR_OLDER
#endif

using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace XeirGYA
{
    public class GYAImport : MonoBehaviour
    {
        public static List<string> importQueue = new List<string>();

        // Multi-Import Type
        public enum MultiImportType
        {
            // Default version preference
            Default, // Any				<Best option auto choosen>
            UnityAsync, // 4.2x - 5.2x		AssetDatabase.ImportPackage
            GYASync, // 5.3x - 5.4.0b12	GYAImport.ImportPackage -> AssetDatabase.ImportPackage
            UnitySync // 5.4.0b13 +		AssetDatabase.ImportPackageImmediately
        }

        // Count instances of package ID, ignore "Non Asset Store packages"
        public static int CountToImport()
        {
            return (GYA.gyaData.Assets.FindAll(x => x.isMarked).Count);
        }

        // Import single asset
        public static void ImportSingle(string pFilePath, bool pInteractive = false)
        {
            if (File.Exists(pFilePath))
            {
                GYAExt.Log("Import: " + pFilePath);

                AssetDatabase.ImportPackage(pFilePath, pInteractive);
                AssetDatabase.Refresh();
            }
            else
            {
                GYAExt.Log("Import failed - Asset not found: " + pFilePath);
            }
        }

        // Loop thru and import packages marked for import
        public static void ImportMultiple(bool importEntireGroup = false)
        {
            // Import for Unity 4.2 and up
            List<string> _importQueue = new List<string>();
            int countToImport = 0;
            string listToImport = String.Empty;

            if (importEntireGroup)
            {
                countToImport = GYA.grpData[GYA.showGroup].Count;
                foreach (GYAData.Asset package in GYA.grpData[GYA.showGroup])
                {
                    listToImport += "\nImport: " + package.title + " ( " + package.version + " )";
                    _importQueue.Add(package.filePath);
                }
            }
            else
            {
                countToImport = CountToImport();
                foreach (GYAData.Asset package in GYA.gyaData.Assets)
                {
                    if (package.isMarked)
                    {
                        listToImport += "\nImport: " + package.title + " ( " + package.version + " )";
                        _importQueue.Add(package.filePath);
                    }
                }
            }

            // Override the import option if required
            MultiImportType _internalMultiImportOverride;

            // Handle Unity 5.4.0b13 and newer
            if (GYAVersion.UnityVersionIsEqualOrNewerThan("5.4.0b13"))
            {
                if (GYA.gyaVars.Prefs.multiImportOverride == MultiImportType.Default)
                {
                    _internalMultiImportOverride = MultiImportType.UnitySync; // 5.6x - Works but reports error
                                                                              // Reload Assembly called from managed code directly. This will cause a crash. You should never refresh assets in synchronous mode or enter playmode synchronously from script code.
                }
                else
                {
                    _internalMultiImportOverride = GYA.gyaVars.Prefs.multiImportOverride;
                }
                // Enable this to override 5.4x import default
                _internalMultiImportOverride = MultiImportType.UnityAsync; // 5.6x - Force due to above error
            }
            // Standard import assignments follow
            else
            {
                // Handle Unity 5.3.x - 5.4.0b12, force GYASync
                if (GYAVersion.UnityVersionIsEqualOrNewerThan("5.3.0", 3))
                {
                    _internalMultiImportOverride = MultiImportType.GYASync;
                }
                // Handle Unity 5.2.x and older
                else
                {
                    // Force UnityAsync if Default or UnitySync
                    if (GYA.gyaVars.Prefs.multiImportOverride == MultiImportType.Default ||
                        GYA.gyaVars.Prefs.multiImportOverride == MultiImportType.UnitySync)
                        _internalMultiImportOverride = MultiImportType.UnityAsync;
                    // Pass thru GYASync or UnityAsync
                    else
                        _internalMultiImportOverride = GYA.gyaVars.Prefs.multiImportOverride;
                }
            }

            listToImport = "Import ( " + countToImport + " ) packages - (Method Selected: " +
                           GYA.gyaVars.Prefs.multiImportOverride + " / Method Used: " +
                           _internalMultiImportOverride + ")\n" + listToImport;

            // Unity Sync Import, Unity 5.4+ Only
            if (_internalMultiImportOverride == MultiImportType.UnitySync)
            {
#if UNITY_5_3_OR_OLDER
                //Do nothing
#else
                foreach (string pFilePath in _importQueue)
                {
                    GYAImport.ImportPackageImmediately(pFilePath); // UnitySync via Reflection
                }
#endif
                GYAExt.Log(listToImport);
            }

            // GYA Sync Import, Any Unity version, ONLY one that works for 5.3x - 5.4.0b12
            if (_internalMultiImportOverride == MultiImportType.GYASync)
            {
                GYACoroutine.start(GYAImport.ImportPackageCoroutine(_importQueue, listToImport));
            }

            // Unity Async Import (Default), Any Unity version EXCEPT 5.3x - 5.4.0b12
            if (_internalMultiImportOverride == MultiImportType.UnityAsync)
            {
                foreach (string pFilePath in _importQueue)
                {
                    AssetDatabase.ImportPackage(pFilePath, false); // Extract all, then start importing
                }
                GYAExt.Log(listToImport);
            }
        }

        public static IEnumerator ImportPackageCoroutine(List<string> _importQueue, string pMsg)
        {
            EditorApplication.LockReloadAssemblies();
            foreach (string pFilePath in _importQueue)
            {
                AssetDatabase.ImportPackage(pFilePath, false);
                yield return null;
            }
            GYAExt.Log(pMsg);
            EditorApplication.UnlockReloadAssemblies();
            AssetDatabase.Refresh();
            yield return null;
        }

        public static void ImportPackageImmediately(string pFilePath)
        {
            // class: public sealed, method: static
            GYAReflect.MI_Invoke("UnityEditor.AssetDatabase", "ImportPackageImmediately", pFilePath);
        }
    }
}