using System;
using System.Collections.Generic;
using GYAInternal.Json;
using GYAInternal.Json.Linq;

using UnityEngine;
//using System.Collections;

namespace XeirGYA
{
    public class GYAVars
    {
        public string name { get; set; }
        public string abbr { get; set; }
        public string version { get; set; } // GYA Version that made the Package file
        public int asset_id { get; set; }
        public int asset_idV2 { get; set; }
        public string pathOldAssetsFolderName { get; set; }
        public string pathOldAssetsFolder { get; set; }
        public GYAPrefs Prefs { get; set; }
        public GYAFiles Files { get; set; }
        public GYAFilesCount FilesCount { get; set; }
        public GYAFilesSize FilesSize { get; set; }

        public GYAVars()
        {
            name = "Grab Yer Assets";
            abbr = "GYA";
            version = String.Empty;
            asset_id = 72902;
            asset_idV2 = 15398;
            pathOldAssetsFolderName = "Asset Store-Old";
            Prefs = new GYAPrefs();
            Files = new GYAFiles();
            FilesCount = new GYAFilesCount();
            FilesSize = new GYAFilesSize();
        }
    }

    // GYA Settings file
    public class GYAPrefs
    {
        public bool isPersist { get; set; }
        public List<string> pathUserAssets { get; set; }
        public bool enableHeaders { get; set; }
        public bool enableColors { get; set; }
        public bool showSVFoldOut { get; set; }
        public bool showSVInfo { get; set; }
        public bool showSVNotes { get; set; }
        public bool nestedVersions { get; set; }
        public bool enableCollectionTypeIcons { get; set; }
        public bool enableAltIconForOldVersions { get; set; }
        public bool enableOfflineMode { get; set; }
        public bool openURLInUnity { get; set; }
        public bool getPurchasedAssetsListDuringRefresh { get; set; }
        public GYAImport.MultiImportType multiImportOverride { get; set; }
        public bool enableToolbarCollections { get; set; }
        public bool enablePopupDetails { get; set; }
        public bool isSilent { get; set; }
        public int urlsToOpenPerBatch { get; set; }
        public bool forceDarkMode { get; set; }
        public string pathSortedAssetsFolder { get; set; }
        public bool alwaysAskForSortedAssetsFolder { get; set; }
        public bool autoPreventASOverwrite { get; set; }
        public string userVersionString { get; set; }
        public bool autoUpdateGYA { get; set; }
        public bool promptBeforeUpdateGYA { get; set; }
        public bool collapseUnityCategoryPaths { get; set; }
        public bool clearMarkedAfter { get; set; }
        public bool clearMarkedAfterGlobal { get; set; }

        [JsonIgnore]
        public string kharmaSession { get; set; }

        //[JsonExtensionData]
        //public IDictionary<string, JToken> AdditionalPrefs { get; set; }

        public UIColor uiColor { get; set; }
        public class UIColor
        {
            public string all { get; set; }
            public string store { get; set; }
            public string user { get; set; }
            public string standard { get; set; }
            public string old { get; set; }
            public string oldToMove { get; set; }
            public string project { get; set; }

            public UIColor()
            {
                all = String.Empty;
                store = String.Empty;
                user = String.Empty;
                standard = String.Empty;
                old = String.Empty;
                oldToMove = String.Empty;
                project = String.Empty;
            }
        }

        public GYAPrefs()
        {
            uiColor = new UIColor();
            isPersist = false;
            pathUserAssets = new List<string>();
            enableHeaders = true;
            enableColors = true;
            showSVFoldOut = true;
            showSVInfo = true;
            showSVNotes = false;
            nestedVersions = false;
            enableCollectionTypeIcons = true;
            enableAltIconForOldVersions = true;
            enableOfflineMode = false;
            openURLInUnity = false;
            getPurchasedAssetsListDuringRefresh = false; // Default false, Get purchased list form Unity
            multiImportOverride = GYAImport.MultiImportType.Default;
            enableToolbarCollections = true;
            enablePopupDetails = false; // Show asset details in right-click popup
            isSilent = false; // Silent Mode - Hide default console msg's
            urlsToOpenPerBatch = 10;
            forceDarkMode = false;
            pathSortedAssetsFolder = String.Empty;
            alwaysAskForSortedAssetsFolder = false;
            autoPreventASOverwrite = false;
            userVersionString = " ({version}_{version_id}_{unity_version})";
            autoUpdateGYA = true;
            promptBeforeUpdateGYA = true;
            collapseUnityCategoryPaths = true;
            clearMarkedAfter = true;  // Clear marked items after user action
            clearMarkedAfterGlobal = false;   // Clear marked items after user action (Global/Entire List), if true, used instead of clearViewAfter

            kharmaSession = String.Empty;

            //AdditionalPrefs = new Dictionary<string, JToken>();
        }
    }

    public class GYAFiles
    {
        public GYAFileInfo Prefs { get; set; }
        public GYAFileInfo Groups { get; set; }
        public GYAFileInfo Assets { get; set; }
        public GYAFileInfo ASPackage { get; set; }
        public GYAFileInfo ASPurchase { get; set; }
        public GYAFileInfo AssetInfo { get; set; }

        public GYAFiles()
        {
            Prefs = new GYAFileInfo();
            Groups = new GYAFileInfo();
            Assets = new GYAFileInfo();
            ASPackage = new GYAFileInfo();
            ASPurchase = new GYAFileInfo();
            AssetInfo = new GYAFileInfo();
        }
    }

    public class GYAFileInfo
    {
        public string fileName { get; set; }
        public string file { get; set; }

        public bool fileExists { get; set; }

        public GYAFileInfo()
        {
            fileName = String.Empty;
            file = String.Empty;
            fileExists = false;
        }
    }

    // Data Tallys
    public class GYAFilesCount
    {
        public int all { get; set; }
        public int store { get; set; }
        public int user { get; set; }
        public int standard { get; set; }
        public int old { get; set; }
        public int oldToMove { get; set; }
        public int project { get; set; }
    }

    public class GYAFilesSize
    {
        public double all { get; set; }
        public double store { get; set; }
        public double user { get; set; }
        public double standard { get; set; }
        public double old { get; set; }
        public double oldToMove { get; set; }
        public double project { get; set; }
    }
}