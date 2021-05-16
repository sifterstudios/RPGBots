//    Grab Yer Assets
//    Copyright Frederic Bell, 2014

#region Defines

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

#if UNITY_4 || UNITY_5
#define UNITY_5_OR_OLDER
#endif

#if UNITY_4 || UNITY_5 || UNITY_2017
#define UNITY_2017_OR_OLDER
#endif

#if UNITY_4 || UNITY_5 || UNITY_2017 || UNITY_2018
#define UNITY_2018_OR_OLDER
#endif

// Unity 5.3.4 and newer, auto assigns: UNITY_x_y_OR_NEWER

#if UNITY_2019_1_OR_NEWER
//#define UNITY_2019_GUI_TWEAKS
#define ENABLE_PAL

#endif

//#define EnableZiosEditorThemeTweaks
//#define TESTING // In-house testing: Misc
#endregion

using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using GYAInternal.Json;
using GYAInternal.Json.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using UnityEngine.Internal;

#if TESTING
//using SharpCompress;
//using SharpCompress.Reader;
//using SharpCompress.Common;
#endif

#if ENABLE_PAL
// ClearNotVisible
//getPurchasedAssetsListDuringRefresh

#endif

namespace XeirGYA
{
    public partial class GYA : EditorWindow
    {
        public static GYA Instance;
        public static Event evt = null;

        public static Rect menuRect;
        public Rect svPopupWindowRect;

        // Detect recompile, check in OnInspectorUpdate
        internal class ScriptRecompiled { }
        internal ScriptRecompiled scriptRecompiled;

        // GYA Version
	    internal static string gyaVersion = GYAVersion.SetAppVersion(3, 21, 2, 18, 1);

        // JSON & Data handling
        private static GYAVars _gyaVars = new GYAVars(); // GYA variables
        public static GYAVars gyaVars
        {
            get { return _gyaVars; }
            set { _gyaVars = value; }
        }
        private static GYAData _gyaData = new GYAData(); // GYA Asset data
        public static GYAData gyaData
        {
            get { return _gyaData; }
            set { _gyaData = value; }
        }
        internal static ASPackageList asPackages = new ASPackageList(); // AS Asset data Unity Internal (AS Folder ONLY)
        internal static ASPurchased asPurchased = new ASPurchased(); // AS Asset data Purchased
        internal static List<GYAData.Asset> svData = new List<GYAData.Asset>(); // SV data list to display
        internal static GYAData.Asset svCurrentPkg = new GYAData.Asset(); // SV Data for currently hilighted package

        internal int svCurrentPkgNumber = 0;
        internal int svLastPkgNumber = -1;
        // Group data for group popups (Rename, delete, etc)
        internal static Dictionary<int, List<GYAData.Asset>> grpData = new Dictionary<int, List<GYAData.Asset>>();
        // GUI placement
        internal int wTop = 0; // Top position for item placement in window

#if UNITY_2019_GUI_TWEAKS
        internal int controlHeight = 20; // Toolbar controls height - Unity 2019
#else
        internal int controlHeight = 17; // Toolbar controls height - Unity 2018 and older
#endif

        // Search & Display related
        internal string fldSearch = String.Empty; // Search field
        internal bool infoChanged = true; // If info to display has changed, refresh the view
        internal string foPackageInfo = String.Empty; // foldout package text
        internal int ddPublisher = 0; // Publisher dropdown selection
        internal Dictionary<int, string> ddCollections = new Dictionary<int, string>(); // Asset & Group section popup data

        // Scrollview area
        public SVMain svMain = new SVMain();

        public class SVMain
        {
            public Vector2 position { get; set; }
            public Rect frame { get; set; }
            public Rect list { get; set; }
            public Rect toggle { get; set; }
            public Rect button { get; set; }
            public int height { get; set; }
            public float lineHeight { get; set; }
            public int headerCount { get; set; }
            public List<SVHeaderLine> headerLine { get; set; }

            public SVMain()
            {
                position = new Vector2();
                frame = new Rect();
                list = new Rect();
                toggle = new Rect();
                button = new Rect();
                height = 0;
                lineHeight = 0f;
                headerCount = 0;
                headerLine = new List<SVHeaderLine>();
            }
        }

        // GUIStyles Toolbar
        public TBStyle tbStyle = new TBStyle();

        public class TBStyle
        {
            public GUIStyle d { get; set; }
            public GUIStyle dropdown { get; set; }
            public GUIStyle group { get; set; }
            public GUIStyle button { get; set; }
            public GUIStyle tb { get; set; }

            public TBStyle()
            {
            }
        }

        // GUIStyles Scrollview
        public SVStyle svStyle = new SVStyle();

        public class SVStyle
        {
            public GUIStyle d { get; set; }
            public GUIStyle store { get; set; }
            public GUIStyle standard { get; set; }
            public GUIStyle old { get; set; }
            public GUIStyle oldToMove { get; set; }
            public GUIStyle user { get; set; }
            public GUIStyle project { get; set; }
            public GUIStyle seperator { get; set; }
            public GUIStyle icon { get; set; }
            public GUIStyle iconLeft { get; set; }

            public SVStyle()
            {
            }
        }

        GUIStyle infoStyle = null;

        // For calculating headers shown in SV
        public class SVHeaderLine
        {
            public int hRow { get; set; }
            public string hText { get; set; }

            public SVHeaderLine()
            {
                hRow = 0;
                hText = String.Empty;
            }
        }

        // Stop GUI Reasons
        internal static ErrorCode errorCode = ErrorCode.None;

        public enum ErrorCode
        {
            None,
            Error,
            ErrorStep2
        }

        // Test for Unity Pro Skin change
        UnityGUISkin GUISkinChangedCurrent;

        UnityGUISkin GUISkinChangedLast;

        // Used to detect changes between dark/light theme in Zios
        bool ZiosEditorThemeIsDark;

        internal enum UnityGUISkin
        {
            Pro,
            NonPro
        }

        // Show Assets Toggle
        internal string activeCollectionText = String.Empty; // Active collection dropdown text

        internal static svCollection showActive;
        internal static int showGroup = 0; // Group to show if showActive = Group

        public enum svCollection
        {
            All,
            Store,
            User,
            Standard,
            Old,
            Group,  // User created groups
            Project // Local Project Assets, shown at the top of the All collection
        }

        // Sort Toggle
        internal static svSortBy sortActive;

        public enum svSortBy
        {
            TitleNestedVersions,
            Title,
            Size,
            Publisher,
            PackageID,
            VersionID,
            Category,
            CategorySub,
            UploadID,
            DateFile,       // File Date
            DateBuild,      // Build Date
            DatePublish,    // Publish Date
            DatePurchased,  // * Package Purchase Date
            DateCreated,    // * Package Created Date
            DateUpdated     // * Package Updated Date
        }

        // Search Toggle - Embedded fields inside the unitypackage files unless otherwise noted
        internal static svSearchBy searchActive;

        public enum svSearchBy
        {
            Title,
            Category,
            Publisher,

            Description,
            //PublishNotes,
            UserNotes
        }

        // Dropdown categories
        internal string ddCategoryText = String.Empty; // Category Text for searching, linked to UserSelection

        internal static ddCategories ddCategory;

        public enum ddCategories
        {
            All,
	        BuiltWithUnity2020,     // "Unity 2020"
	        BuiltWithUnity2019,     // "Unity 2019"
	        BuiltWithUnity2018,     // "Unity 2018"
            BuiltWithUnity2017,     // "Unity 2017"
            BuiltWithUnity5,        // "Unity 5"
            BuiltWithUnity4,        // "Unity 4"
            BuiltWithUnity3,        // "Unity 3"
            BuiltWithUnityUnknown,  // "Unity Unknown"
            PackageAssetStore,      // "Asset Store"
            PackageExported,        // "Exported"
            Ungrouped,              // "Ungrouped"
            MultiVersion,           // "Multiple Versions"
            HasNotes,               // "Has Notes"
            Damaged,                // "Damaged"
            Deprecated,             // "Deprecated"
            NotInPurchased,         // "Not In Purchased" - AS Pkgs not listed in the Purchased list
            NotDownloaded,          // "Not Downloaded"
            IsMarked,               // Show all isMarked assets
            UserSelection,          // User Selected category from dropdown, refer to ddCategoryText
            MissingFiles            // Packages with missing files
        }

        // Json deserialization override - StringToIntConverter
        internal class StringToIntConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(int);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
                JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null)
                    return 0;

                if (reader.TokenType == JsonToken.Integer)
                    return Convert.ToInt32(reader.Value);

                if (reader.TokenType == JsonToken.String)
                {
                    if (string.IsNullOrEmpty((string)reader.Value))
                        return 0;

                    int num = 0;
                    if (int.TryParse((string)reader.Value, out num))
                        return num;

                    throw new JsonReaderException(string.Format("Expected integer, got {0}", reader.Value));
                }
                throw new JsonReaderException(string.Format("Unexcepted token {0}", reader.TokenType));
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                writer.WriteValue(value);
            }
        }

        internal class IntToStringConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return typeof(Int32) == objectType;
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
                JsonSerializer serializer)
            {
                JToken jt = JToken.ReadFrom(reader);
                return jt.Value<int>();
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                serializer.Serialize(writer, value.ToString());
            }
        }

        // Add to the Window menu
        [MenuItem("Window/Grab Yer Assets", false, 490)]
        internal static void EditorWindowGYA()
        {
            GYA window = (GYA)EditorWindow.GetWindow(typeof(GYA));
            window.Show();
        }

        internal static void EditorWindowGYAPrefs()
        {
            GYAWindowPrefs.Init(1);
        }

        internal static void EditorWindowGYAGroups()
        {
            GYAWindowPrefs.Init(3);
        }

        internal static void EditorWindowASData()
        {
            GYAWindowASData.Init(0);
        }

        public static bool EditorWindowFind<WindowType>() where WindowType : EditorWindow
        {
            WindowType[] windows = Resources.FindObjectsOfTypeAll<WindowType>();
            if (windows != null && windows.Length > 0)
            {
                // do something...
                return true;
            }
            return false;
        }

        public static bool IsInstance
        {
            get { return Instance != null; }
        }

        internal bool isMouseOutsideFoldOut; // svFOHandling
        internal int svFoldOutTop; // svFOHandling

        public void OnGUI()
        {
            //wantsMouseMove = true;

            evt = Event.current;
            isMouseOutsideFoldOut = (evt.mousePosition.y < svFoldOutTop); // svFOHandling

            UpdateWindow();
        }

        void Update()
        {
            try
            {
                if (evt.type == EventType.MouseMove && mouseOverWindow.GetType() == (typeof(GYA))) // Less impact in deep profile .. callUpdateFunctions 1.5%
                {
                    if (svLastPkgNumber != svCurrentPkgNumber)
                    {
                        svLastPkgNumber = svCurrentPkgNumber;
                        if (svCurrentPkgNumber >= 0)
                        {
                            svCurrentPkg = svData[svCurrentPkgNumber];
                            UpdateFOInfo(svCurrentPkgNumber);
                        }
                        this.Repaint();
                    }
                }
            }
            catch { }
        }

        public void OnInspectorUpdate()
        {
            // Script Recompile Check
            if (this.scriptRecompiled == null) // Recompiled
            {
                this.scriptRecompiled = new ScriptRecompiled();
                // Perform after recompile
                infoChanged = true;
                CheckIfGUISkinHasChanged(true); // Force reload
            }
            else // Not recompiled
            {
                CheckIfGUISkinHasChanged();
            }
        }

        public void OnFocus()
        {
            wantsMouseMove = true;
        }

        public void OnUnFocus()
        {
            wantsMouseMove = false;
        }

        public void GYAStatus(object pObj)
        {
            GYAStatus();
        }

        public void GYAStatus()
        {
            // Show Raw Asset Data Window
            GYAWindowASData.Init(0);

            // Used for quick testing ..
            GYAQuickTest();
        }

        // For testing things on the fly ..
        public void GYAQuickTest()
	    {
		    //GYAExt.LogAsJson(gyaData.Groups);
		    
            //GYAExt.Log(
            //    "\n1" + svCurrentPkg.title
            //    + "\n1" + svCurrentPkg.titleWithVersion
            //    + "\n2" + GYAFile.GetAssetVersionStringToAppend(svCurrentPkg, true)
            //    + "\n3" + GYAFile.GetTitleVersionAppended(svCurrentPkg, true)
            //    //+ "\n3" + GYAFile.GetTitleVersionRemoved(svCurrentPkg, true)
            //    + "\n4" + GYAFile.GetTitleCleaned(svCurrentPkg)
            //    + "\n4" + GYAFile.GetTitleAsFilename(svCurrentPkg)
            ////+ "\n5" + GYAFile.RemoveInvalidCharsUnityTitleAsFilename(svCurrentPkg.title)
            //);

            //GYAExt.Log(GetNewestPkgVersionOfGYAAsString());
            //GetNewestPkgVersionOfGYA();
            //UpdateToLatestVersionOfGYA();

            //// Works with scripting runtime 4.0
            //gyaVars.Files.Prefs.fileExists = true;
            //string jsonText = File.ReadAllText(GYA.gyaVars.Files.Prefs.file);
            //GYAExt.Log("Prefs - text: ", jsonText);
            ////GYA.gyaVars.Prefs = JsonConvert.DeserializeObject<GYAPrefs>(jsonText);
            //var jsonText2 = JsonConvert.DeserializeObject<dynamic>(jsonText);
            //GYAExt.LogAsJson(jsonText2);

        }

        public static string Serialize(object o, StringEscapeHandling stringEscapeHandling)
        {
            StringWriter wr = new StringWriter();
            var jsonWriter = new JsonTextWriter(wr);
            jsonWriter.StringEscapeHandling = stringEscapeHandling;
            new JsonSerializer().Serialize(jsonWriter, o);
            return wr.ToString();
        }

        // Called by DecryptFile
        private void HexStringToByteArray(string hex, byte[] array, int offset)
        {
            if (offset + array.Length * 2 > hex.Length)
            {
                throw new ArgumentException("Hex string too short");
            }
            for (int i = 0; i < array.Length; i++)
            {
                string text = hex.Substring(i * 2 + offset, 2);
                array[i] = byte.Parse(text, System.Globalization.NumberStyles.HexNumber);
            }
        }

        // Decrypt AS Downloaded Encrypted Asset
        public void DecryptFile(string inputFile, string outputFile, string keyIV)
        {
            byte[] array = new byte[32];
            byte[] array2 = new byte[16];
            HexStringToByteArray(keyIV, array, 0);
            HexStringToByteArray(keyIV, array2, 64);
            GYAExt.LogAsJson(array);
            GYAExt.LogAsJson(array2);
            FileStream fileStream = File.Open(inputFile, FileMode.Open);
            FileStream fileStream2 = File.Open(outputFile, FileMode.CreateNew);
            AesManaged aesManaged = new AesManaged();
            aesManaged.Key = array;
            aesManaged.IV = array2;
            CryptoStream cryptoStream = new CryptoStream(fileStream, aesManaged.CreateDecryptor(aesManaged.Key, aesManaged.IV), CryptoStreamMode.Read);
            try
            {
                byte[] array3 = new byte[40960];
                int num;
                while ((num = cryptoStream.Read(array3, 0, array3.Length)) > 0)
                {
                    fileStream2.Write(array3, 0, num);
                }
            }
            finally
            {
                cryptoStream.Close();
                fileStream.Close();
                fileStream2.Close();
            }
        }

        public void BuildGYAStrings()
        {
            gyaVars.version = gyaVersion;

            string filePrefix = gyaVars.abbr + " ";
            // File Names
            gyaVars.Files.Prefs.fileName = filePrefix + "Prefs v3.json";
            gyaVars.Files.Groups.fileName = filePrefix + "Groups.json";
            gyaVars.Files.Assets.fileName = filePrefix + "Assets v3.json";
            gyaVars.Files.ASPackage.fileName = "AS Packages.json"; // Unity Local Data
            gyaVars.Files.ASPurchase.fileName = "AS Purchased.json"; // Unity Web Data
            gyaVars.Files.AssetInfo.fileName = filePrefix + "User Data.json"; // Unity Web Data
            // File Paths
            gyaVars.Files.Prefs.file = GYAExt.FileInGYADataFiles(gyaVars.Files.Prefs.fileName);
            gyaVars.Files.Groups.file = GYAExt.FileInGYADataFiles(gyaVars.Files.Groups.fileName);
            gyaVars.Files.Assets.file = GYAExt.FileInGYADataFiles(gyaVars.Files.Assets.fileName);
            gyaVars.Files.ASPackage.file = GYAExt.FileInGYADataFiles(gyaVars.Files.ASPackage.fileName);
            gyaVars.Files.ASPurchase.file = GYAExt.FileInGYADataFiles(gyaVars.Files.ASPurchase.fileName);
            gyaVars.Files.AssetInfo.file = GYAExt.FileInGYADataFiles(gyaVars.Files.AssetInfo.fileName);

            gyaVars.pathOldAssetsFolder =
                Path.GetFullPath(Path.Combine(GYAExt.PathGYADataFiles, gyaVars.pathOldAssetsFolderName));
        }

        public void OnEnable()
        {
            if (Instance == null)
                Instance = this;

            // Change GYA window title
            string wTitle = "Grab Yer Assets";
#if UNITY_5_0_OR_OLDER
            title = wTitle;
#else
            titleContent = new GUIContent(wTitle);
#endif

            // Create GYA Data Folder if needed
            if (!Directory.Exists(GYAExt.PathGYADataFiles))
                GYAFile.CreateFolder(GYAExt.PathGYADataFiles);

            BuildGYAStrings();
            // Convert Settings from v2 to v3 if required
            GYAFile.ConvertGYAv2Tov3();
            LoadData();
        }

        public void LoadData()
        {
            ErrorStateClear();

            GYATexture.LoadTextures();
            SetStyles();

            if (ErrorStateActive)
                return;

            // If data files exists load it, else create it
            if (Directory.Exists(GYAExt.PathGYADataFiles))
            {
                GYAFile.LoadGYAPrefs();
                GYAFile.LoadGYAGroups();
                GYAFile.LoadASPurchased();

                if (!File.Exists(gyaVars.Files.Assets.file))
                {
                    // Scan and Create if needed
                    GYAPackage.RefreshAllCollections();
                }
                else
                {
                    // Load if exists
                    GYAFile.LoadGYAAssets();
                    GYAFile.LoadGYAUserData();
                    //PrecalcTempListData();
                    RefreshSV();
                }
            }
            else
            {
                ErrorStateSet(ErrorCode.Error);
                GYAExt.LogWarning("Error Folder: " + GYAExt.PathGYADataFiles + " does not exist.");
            }

            CheckIfGUISkinHasChanged(true);
            infoChanged = true;
        }

        // Rebuild data to show in scrollview
        public void RefreshSV(bool processGroups = true)
        {
            PrecalcTempListData();
            GYAPackage.BuildOldAssetsList();
            if (processGroups)
                GroupUpdatePkgData();
            BuildPrevNextList();
            GYAFile.ProcessASPurchased();
            SVPopUpCollection(showActive);
            SVPopulate();
        }

        // Update temp data for packages
        // This is done to reduce processing during OnGUI
        public void PrecalcTempListData()
        {
            foreach (GYAData.Asset package in gyaData.Assets)
            {
                string tmpUnityVer = package.unity_version;
                if (package.unity_version.Length == 0)
                    tmpUnityVer = "0.0.0";

                // Mark if both asset and unity versions match, this is for altIcon use
                if (GYAVersion.UnityVersionIsEqualTo(tmpUnityVer, 1))
                    package.isSameVersionAsUnity = true;

                // Mark if in Favorites Group
                if (GroupContainsAsset(0, package))
                    package.isFavorite = true;

                // Mark if title endswith the asset version
                if (package.title.EndsWith(package.version, StringComparison.InvariantCultureIgnoreCase))
                    package.isVersionAppended = true;

                // Title with version precalc
                if (!package.isVersionAppended)
                    package.titleWithVersion = package.title + " v" + GYAFile.GetSafeFilename(package.version).TrimStart('v', 'V');
                else package.titleWithVersion = package.title;

                // If title meta damaged, missing/blank .. use filename as title
                if (package.isDamaged)
                {
                    package.titleWithVersion = Path.GetFileNameWithoutExtension(package.filePath);
                }

                package.pubDateToDateTime = PubDateToDateTime(package.pubdate);

            }
        }

        public DateTime PubDateToDateTime(string pubDate)
        {
            DateTime time;
            if (DateTime.TryParse(pubDate, out time))
                return time;

            return time;
        }

        // Set reason for ErrorStatusHandler to Override GUI
        internal void ErrorStateSet(ErrorCode pReason)
        {
            errorCode = pReason;
        }

        // Get Status, Get reason for ErrorStatusHandler
        internal static bool ErrorStateActive
        {
            get { return (errorCode != ErrorCode.None); }
        }

        // Stop overriding the GUI main frame and reset text field
        internal void ErrorStateClear()
        {
            GUIUtility.keyboardControl = 0;
            ErrorStateSet(ErrorCode.None);
        }

        internal void ErrorStateHandler()
        {
            // Reason: Error
            if (errorCode == ErrorCode.Error || errorCode == ErrorCode.ErrorStep2)
            {
                string errorMsg =
                    "Please check the console log for any relevant messages and make sure that you are running the latest version of GYA.\n\n";
                errorMsg = errorMsg + "Click on 'Refresh' to manaully rebuild your asset list.\n\n";
                errorMsg = errorMsg + "If the error persists, contact: support@unity.xeir.com\n\n";
                errorMsg = errorMsg + "Please include any error messages & a copy of the following:\n";
                errorMsg = errorMsg + "\n" + gyaVars.Files.Prefs.file;
                errorMsg = errorMsg + "\n" + gyaVars.Files.Assets.file;
                errorMsg = errorMsg + "\n" + gyaVars.Files.Groups.file;
                errorMsg = errorMsg +
                           "\n\nThese files are optional to include, but help in determining the cause of the error.";
                errorMsg = errorMsg + "";

                EditorGUI.LabelField(new Rect(4, wTop + controlHeight, position.width - 8, controlHeight),
                    "Error encountered:");
                EditorGUI.TextArea(
                    new Rect(4, wTop + controlHeight * 2, position.width - 8,
                        position.height - (wTop + controlHeight + 4)), errorMsg, EditorStyles.wordWrappedLabel);
            }
        }

        internal string MenuItemEscape(string pString)
        {
            //"[^a-zA-Z0-9 ()\-+_]"
            pString = pString.Replace('/', '-');
            pString = pString.Replace('&', '+');
            pString = pString.Replace('%', ' ');
            pString = pString.Replace('#', ' ');

            return pString;
        }

        // UpdateWindow called from OnGui
        internal void UpdateWindow()
        {
            SetStylesTB();

#if UNITY_2019_GUI_TWEAKS
            wTop = 0; // Vertical positioning
#else
            wTop = 0; // Vertical positioning
#endif

            int infoHeight = controlHeight; // info 1 line
            int tbRows = 0; // Each TBDrawLine<x> increases tbRow count

            // If foldInfo is true, adjust height of Info pane
            if (gyaVars.Prefs.showSVFoldOut)
            {
                if (gyaVars.Prefs.showSVInfo) infoHeight = controlHeight * 8;
            }

            // -- 1st Toolbar
            tbRows += TBDrawLine1();

            // Check for GUI Override/Error State and prevent the rest of the GUI from displaying if so
            if (ErrorStateActive)
            {
                ErrorStateHandler();
            }
            else
            {
                // -- 2nd Toolbar
                tbRows += TBDrawLine2();

                // -- 3rd Toolbar
                if (gyaVars.Prefs.enableToolbarCollections)
                    tbRows += TBDrawLine3();

                // -- ScrollView List
#if UNITY_2019_GUI_TWEAKS
                svMain.height = (int)position.height - (infoHeight + (tbRows * controlHeight) - 2); // Scrollview height
#else
                svMain.height = (int)position.height - (infoHeight + (tbRows * controlHeight)); // Scrollview height
#endif

                SVDraw(infoHeight);

                // -- ScrollView FoldOut
                SVFoldOutPane(infoHeight);

                // **** Check for change and update scrollview data to show
                if (infoChanged || GUI.changed)
                {
                    if (infoChanged)
                        PrecalcTempListData();

                    GYAPackage.countMarkedToImport = GYAImport.CountToImport();
                    SVPopUpCollection(showActive);
                    SVPopulate();

                    infoChanged = false;
                }
            }

            if (!isMouseOutsideFoldOut || evt.mousePosition.y < (tbRows * controlHeight)) svCurrentPkgNumber = -1;

        }

        // SV FOLDOUT at top for notes
        internal void SVFoldOutNotes()
        {
            if (true)
            {
                var infoHeight = controlHeight * 7;
                GUI.BeginGroup(new Rect(0, wTop + controlHeight, position.width, infoHeight), infoStyle);

                if (svCurrentPkgNumber != -1) // -1 = ignore header bars
                {
                    var notesInfo = "";
                    notesInfo += "" + svCurrentPkg.title + ":\n";
                    notesInfo += svCurrentPkg.AssetInfo.Notes;

                    //Notes Field - gyaData.Assets[svCurrentPkgNumber].AssetInfo.Notes
                    GUI.Label(new Rect(0, controlHeight * 7 + (controlHeight / 2), position.width, infoHeight), notesInfo, infoStyle);
                }
            }
            GUI.EndGroup();
        }


        // SV FOLDOUT - Asset Info
        internal void SVFoldOutPane(int infoHeight)
        {
            svFoldOutTop = wTop = (int)position.height - infoHeight; // svFOHandling
            GUI.BeginGroup(new Rect(0, wTop, position.width, infoHeight), svStyle.d);

            Rect infoFoldOut = new Rect(0, 0, position.width, controlHeight);
            bool bSVFoldout = gyaVars.Prefs.showSVFoldOut;
            gyaVars.Prefs.showSVFoldOut = GUI.Toggle(new Rect(infoFoldOut.x + 40, infoFoldOut.y, position.width - 80, controlHeight), gyaVars.Prefs.showSVFoldOut, "", EditorStyles.toolbar); // Toggle active

            // Only update prefs if foldout changed
            if (bSVFoldout != gyaVars.Prefs.showSVFoldOut)
            {
                GYAFile.SaveGYAPrefs();
            }

            // Text on infoFoldOut line
            var infoText = "Package Info: " + (gyaVars.Prefs.showSVFoldOut ? "HIDE" : "SHOW");
            GUI.Label(new Rect(infoFoldOut.x, infoFoldOut.y, position.width, infoHeight), infoText, tbStyle.tb);

            //Notes button
            bool bSVNotes = GYA.gyaVars.Prefs.showSVNotes;

#if UNITY_2019_GUI_TWEAKS
            gyaVars.Prefs.showSVNotes = GUI.Toggle(new Rect(0, 1, 40, controlHeight), gyaVars.Prefs.showSVNotes, "Notes", tbStyle.button);
#else
            gyaVars.Prefs.showSVNotes = GUI.Toggle(new Rect(0, 0, 40, controlHeight), gyaVars.Prefs.showSVNotes, "Notes", tbStyle.button);
#endif

            if (bSVNotes != GYA.gyaVars.Prefs.showSVNotes)
            {
                GYAFile.SaveGYAPrefs();
            }

            // HIDE/SHOW Package Info Panel
            if (gyaVars.Prefs.showSVFoldOut)
            {
                if (gyaVars.Prefs.showSVNotes)
                {
                    if (svCurrentPkgNumber != -1) // -1 = ignore header bars
                    {
                        var notesInfo = "";
                        notesInfo += svCurrentPkg.AssetInfo.Notes;
                        GUI.Label(new Rect(0, controlHeight + 1, position.width, infoHeight), notesInfo, infoStyle);
                    }
                }
                else
                {
                    // Package Info
                    GUI.Label(new Rect(0, controlHeight + 1, position.width, infoHeight), foPackageInfo, infoStyle);
                }
            }
            GUI.EndGroup();
        }

        // 1st Toolbar
        internal int TBDrawLine1()
        {
#if UNITY_2019_GUI_TWEAKS
            wTop += 1;
            int gTop = wTop - 1;
            int sTop = wTop;
            float xPos = 0; // Current Position for control/button
            int xOffset = 6; // Left offset
            int bWidth = 26; // Button Width
            int bSearch = 16; // Unity Search Icon
#else
            int gTop = wTop;
            int sTop = wTop + 2;
            float xPos = 0; // Current Position for control/button
            int xOffset = 6; // Left offset
            int bWidth = 26; // Button Width
            int bSearch = 16; // Unity Search Icon
#endif

            // Toolbar 1 Group
            GUI.BeginGroup(new Rect(0, gTop, position.width, controlHeight), EditorStyles.toolbar);
            xPos += xOffset;

            // Menu Button
            float menuPos = xPos;
            Rect menuButton = new Rect(xPos, wTop, bWidth, controlHeight);
            xPos += bWidth;

            // Category dropdown
            float catPos = xPos;
            Rect catButton = new Rect(xPos, wTop, bWidth, controlHeight);
            xPos += bWidth;

            // Publisher dropdown
            float pubPos = xPos;
            Rect pubButton = new Rect(xPos, wTop, bWidth, controlHeight);
            xPos += bWidth;

            // Search Button/Icon, If left-click show search popup
            xPos += xOffset;
            float searchPos = xPos;
            Rect searchButton = new Rect(xPos, sTop, bSearch, controlHeight);
            // Search field
            Rect fldSearchRect = new Rect(xPos, sTop,
                position.width - ((xOffset + bSearch + bWidth) + (bWidth) + xPos), controlHeight);
            // Search Icon at the end of the search field, blank or cancel
            xPos = position.width - (xOffset + bSearch + bWidth * 2);
            float searchEndPos = xPos;
            Rect searchEndButton = new Rect(searchEndPos, sTop, bSearch, controlHeight);

            // Refresh Button
            xPos += bSearch;
            float refreshPos = xPos;
            Rect refreshButton = new Rect(refreshPos, wTop, bWidth * 2, controlHeight);
            GUI.EndGroup();

            // Process Events

            // Menu Button
            if (GUI.Button(menuButton, new GUIContent(GYATexture.iconMenu, (gyaVars.Prefs.isSilent ? "" : "Main Menu")), EditorStyles.toolbarButton))
            {
                if (evt.button == 0)
                {
                    // Standard Menu
                    var bRect = new Rect(menuPos, wTop, 0, 18);
                    GenericMenu infoMenu = new GenericMenu();

                    // Experimental Menu
#if TESTING
                    infoMenu.AddItem(new GUIContent("-- INTERNAL TESTING --"), false, TBPopUpCallback, "");
                    infoMenu.AddItem(new GUIContent("Auto Protect Asset Store Files"), (gyaVars.Prefs.autoPreventASOverwrite), TBPopUpCallback, "AutoPreventASOverwrite");
                    infoMenu.AddSeparator("");
#endif

                    // Settings
                    infoMenu.AddItem(new GUIContent("Preferences"), false, TBPopUpCallback, "winPrefs");
                    infoMenu.AddSeparator("");

                    // Open Folder/URL Options
                    infoMenu.AddDisabledItem(new GUIContent("Open Folder:"));

                    // Unity Data Folder - Assets
                    if (Directory.Exists(GYAExt.PathUnityDataFiles))
                        infoMenu.AddItem(new GUIContent("Unity Assets"), false, TBPopUpCallback, "UnityDataFolder");
                    else
                        infoMenu.AddDisabledItem(new GUIContent("Asset Store"));

                    // Folder User
                    if (gyaVars.Prefs.pathUserAssets.Any() && Directory.Exists(gyaVars.Prefs.pathUserAssets[0]))
                        infoMenu.AddItem(new GUIContent("User Assets"), false, TBPopUpCallback, "UserAssetsFolder");
                    else
                        infoMenu.AddDisabledItem(new GUIContent("User Assets"));

                    if (gyaVars.FilesCount.standard != 0 || Directory.Exists(GYAExt.PathUnityStandardAssets))
                        infoMenu.AddItem(new GUIContent("" + GYAExt.FolderUnityStandardAssets), false, TBPopUpCallback, "StandardAssetsFolder");
                    else
                        infoMenu.AddDisabledItem(new GUIContent("" + GYAExt.FolderUnityStandardAssets));

                    if (Directory.Exists(gyaVars.pathOldAssetsFolder))
                        infoMenu.AddItem(new GUIContent("Old Assets"), false, TBPopUpCallback, "OldAssetsFolder");
                    else
                        infoMenu.AddDisabledItem(new GUIContent("Old Assets"));

                    infoMenu.AddSeparator("");
                    infoMenu.AddItem(new GUIContent("GYA Data Folder"), false, TBPopUpCallback, "GYADataFolder");

                    infoMenu.AddSeparator("");
                    infoMenu.AddItem(new GUIContent("Sorted Assets Folder"), false, TBPopUpCallback, "SortedAssetsFolder");

                    // If NOT in error state
                    if (errorCode == ErrorCode.None)
                    {
                        infoMenu.AddSeparator("");
                        infoMenu.AddDisabledItem(new GUIContent("Store      : " + gyaVars.FilesSize.store.BytesToKB()));
                        infoMenu.AddDisabledItem(new GUIContent("User       : " + gyaVars.FilesSize.user.BytesToKB()));
                        infoMenu.AddDisabledItem(new GUIContent("Standard: " + gyaVars.FilesSize.standard.BytesToKB()));
                        infoMenu.AddDisabledItem(new GUIContent("Old         : " + gyaVars.FilesSize.old.BytesToKB()));
                    }
                    else
                    {
                        // In error state
                        infoMenu.AddItem(new GUIContent("WARNING - GYA has detected an error!"), false, TBPopUpCallback, "");
                    }
                    // Version
                    infoMenu.AddSeparator("");
                    infoMenu.AddItem(new GUIContent(gyaVars.abbr + " v" + gyaVars.version), false, GYAStatus, "");

                    // Offset menu from right of editor window
                    infoMenu.DropDown(bRect);
                }
            }

            // Category dropdown
            if (GUI.Button(catButton, new GUIContent((ddCategory == ddCategories.All ? GYATexture.iconCategory : GYATexture.iconCategoryX), (gyaVars.Prefs.isSilent ? "" : "Categories &\nPackage Types")), EditorStyles.toolbarButton))
            {
                if (errorCode == ErrorCode.None)
                {
                    if (evt.button == 0)
                    {
                        var bRect = new Rect(catPos, wTop, 0, 18);
                        GenericMenu infoMenu = new GenericMenu();

                        // Option Menu
                        infoMenu.AddItem(new GUIContent("ALL"), (ddCategory == ddCategories.All), () => { TBCatCallback(ddCategories.All); });
                        infoMenu.AddSeparator("");

                        var mBuiltHeader = "Packages Built with/";

	                    infoMenu.AddItem(new GUIContent(mBuiltHeader + "Unity 2020"), (ddCategory == ddCategories.BuiltWithUnity2020),
	                    () => { TBCatCallback(ddCategories.BuiltWithUnity2020); });
	                    infoMenu.AddItem(new GUIContent(mBuiltHeader + "Unity 2019"), (ddCategory == ddCategories.BuiltWithUnity2019),
	                    () => { TBCatCallback(ddCategories.BuiltWithUnity2019); });
                        infoMenu.AddItem(new GUIContent(mBuiltHeader + "Unity 2018"), (ddCategory == ddCategories.BuiltWithUnity2018),
                 () => { TBCatCallback(ddCategories.BuiltWithUnity2018); });
                        infoMenu.AddItem(new GUIContent(mBuiltHeader + "Unity 2017"), (ddCategory == ddCategories.BuiltWithUnity2017),
                 () => { TBCatCallback(ddCategories.BuiltWithUnity2017); });
                        infoMenu.AddItem(new GUIContent(mBuiltHeader + "Unity 5"), (ddCategory == ddCategories.BuiltWithUnity5),
                                         () => { TBCatCallback(ddCategories.BuiltWithUnity5); });
                        infoMenu.AddItem(new GUIContent(mBuiltHeader + "Unity 4"), (ddCategory == ddCategories.BuiltWithUnity4),
                                         () => { TBCatCallback(ddCategories.BuiltWithUnity4); });
                        infoMenu.AddItem(new GUIContent(mBuiltHeader + "Unity 3"), (ddCategory == ddCategories.BuiltWithUnity3),
                                         () => { TBCatCallback(ddCategories.BuiltWithUnity3); });

                        infoMenu.AddItem(
                            new GUIContent(mBuiltHeader + "Unknown   "), (ddCategory == ddCategories.BuiltWithUnityUnknown),
                            () => { TBCatCallback(ddCategories.BuiltWithUnityUnknown); });

                        var mPackageHeader = "Packages of Type/";

                        // Add category for Unity packages
                        if (gyaData.Assets.FindAll(x => !x.isExported).Count > 0)
                            infoMenu.AddItem(new GUIContent(mPackageHeader + "Asset Store"), (ddCategory == ddCategories.PackageAssetStore),
                                             () => { TBCatCallback(ddCategories.PackageAssetStore); });
                        else
                            infoMenu.AddDisabledItem(new GUIContent(mPackageHeader + "Asset Store"));
                        // Add category for Exported packages
                        if (gyaData.Assets.FindAll(x => x.isExported).Count > 0)
                            infoMenu.AddItem(new GUIContent(mPackageHeader + "Exported"), (ddCategory == ddCategories.PackageExported),
                                             () => { TBCatCallback(ddCategories.PackageExported); });
                        else
                            infoMenu.AddDisabledItem(new GUIContent(mPackageHeader + "Exported"));

                        // Ungrouped
                        infoMenu.AddItem(new GUIContent(mPackageHeader + "Ungrouped"), (ddCategory == ddCategories.Ungrouped),
                                         () => { TBCatCallback(ddCategories.Ungrouped); });

                        // Multiple Versions
                        infoMenu.AddItem(new GUIContent(mPackageHeader + "Multiple Versions"), (ddCategory == ddCategories.MultiVersion),
                                         () => { TBCatCallback(ddCategories.MultiVersion); });

                        // Has Notes
                        infoMenu.AddItem(new GUIContent(mPackageHeader + "Has Notes"), (ddCategory == ddCategories.HasNotes),
                                         () => { TBCatCallback(ddCategories.HasNotes); });

                        // Damaged
                        if (gyaData.Assets.FindAll(x => x.isDamaged).Count > 0)
                            infoMenu.AddItem(new GUIContent(mPackageHeader + "Damaged"), (ddCategory == ddCategories.Damaged),
                                             () => { TBCatCallback(ddCategories.Damaged); });
                        else
                            infoMenu.AddDisabledItem(new GUIContent(mPackageHeader + "Damaged"));

                        // Deprecated
                        if (GYA.asPurchased.results != null)
                            infoMenu.AddItem(new GUIContent(mPackageHeader + "Deprecated" + "*"), (ddCategory == ddCategories.Deprecated),
                                             () => { TBCatCallback(ddCategories.Deprecated); });
                        else
                            infoMenu.AddDisabledItem(new GUIContent(mPackageHeader + "Deprecated" + "*"));

                        // notInPurchased
                        if (GYA.asPurchased.results != null)
                            infoMenu.AddItem(new GUIContent(mPackageHeader + "Not In Purchased List" + "*"), (ddCategory == ddCategories.NotInPurchased),
                                             () => { TBCatCallback(ddCategories.NotInPurchased); });
                        else
                            infoMenu.AddDisabledItem(new GUIContent(mPackageHeader + "Not In Purchased" + "*"));

                        // notDownloaded
                        if (GYA.asPurchased.results != null)
                            infoMenu.AddItem(new GUIContent(mPackageHeader + "Not Downloaded" + "*"), (ddCategory == ddCategories.NotDownloaded),
                                             () => { TBCatCallback(ddCategories.NotDownloaded); });
                        else
                            infoMenu.AddDisabledItem(new GUIContent(mPackageHeader + "Not Downloaded" + "*"));

                        // missingFiles
                        if (GYA.asPurchased.results != null)
                            infoMenu.AddItem(new GUIContent(mPackageHeader + "Missing Files (Groups)"), (ddCategory == ddCategories.MissingFiles),
                                    () => { TBCatCallback(ddCategories.MissingFiles); });
                        else
                            infoMenu.AddDisabledItem(new GUIContent(mPackageHeader + "Missing Files (Groups)" + "*"));

                        // -- Categories

                        infoMenu.AddSeparator("");
                        infoMenu.AddDisabledItem(new GUIContent("Categories:"));

                        List<string> tempCatFull = new List<string>();
                        List<string> tempCatRoot = new List<string>();
                        IEnumerable<string> tempCatFinal = null;

                        // Nested List
                        tempCatFull = SubCategoryListBuildNested();
                        tempCatRoot = SubCategoryListBuildNested(true); // Nested -1 last element
                        tempCatFinal = tempCatFull.Except(tempCatRoot);

                        List<string> tempCat = tempCatFinal.ToList();

                        // Sub-categories

                        if (tempCat.Count > 0)
                        {
                            foreach (string t in tempCat)
                                infoMenu.AddItem(new GUIContent(t), (ddCategoryText == t), () => { TBCatCallback(t.Replace('/', '\\')); });

                            // Sub-categories ALL menu
                            if (tempCatRoot.Count > 0)
                                foreach (string t in tempCatRoot)
                                {
                                    infoMenu.AddItem(new GUIContent(t + "/"), false, () => { TBCatCallback(""); });
                                    infoMenu.AddItem(new GUIContent(t + "/All"), (ddCategoryText == t), () => { TBCatCallback(t.Replace('/', '\\')); });
                                }
                        }
                        else
                            infoMenu.AddDisabledItem(new GUIContent("No Sub-Categories"));

                        infoMenu.DropDown(bRect);
                    }
                }
            }

            // Publisher dropdown
            if (GUI.Button(pubButton, new GUIContent((ddPublisher == 0 ? GYATexture.iconPublisher : GYATexture.iconPublisherX), (gyaVars.Prefs.isSilent ? "" : "Publishers")), EditorStyles.toolbarButton))
            {
                if (errorCode == ErrorCode.None)
                {
                    if (evt.button == 0)
                    {
                        var bRect = new Rect(pubPos, 0, 0, 18);
                        var infoMenu = new GenericMenu();
                        // Option Menu
                        infoMenu.AddItem(new GUIContent("ALL"), (ddPublisher == 0), () => { TBCatCallback(ddCategories.All); });
                        infoMenu.AddSeparator("");
                        infoMenu.AddDisabledItem(new GUIContent("Publisher:"));

                        // Publisher List
                        var tempPub = PublisherListBuild();
                        if (tempPub.Count > 0)
                        {
                            // Show list as sub-menus
                            foreach (KeyValuePair<string, int> lineItem in tempPub)
                                infoMenu.AddItem(new GUIContent(lineItem.Key.Substring(0, 1).ToUpper() + "/" + lineItem.Key), (ddPublisher == lineItem.Value), () => { TBPubCallback(lineItem.Value); });
                        }
                        else
                            infoMenu.AddDisabledItem(new GUIContent("No Publishers"));

                        infoMenu.DropDown(bRect);
                    }
                }
            }

            // Search Button/Icon, If left-click show search popup
            if (GUI.Button(searchButton, new GUIContent("", (gyaVars.Prefs.isSilent ? "" : "Search & Sort Options")), GUI.skin.FindStyle("ToolbarSeachTextFieldPopup")))
            {
                if (errorCode == ErrorCode.None)
                {
                    if (evt.button == 0)
                    {
                        // Activate Popup menu
                        var bRect = new Rect(searchPos, 0, 0, 18);
                        GenericMenu searchMenu = new GenericMenu();
                        searchMenu.AddDisabledItem(new GUIContent("Search:"));
                        searchMenu.AddItem(new GUIContent("in Title"), IsSearchActive(svSearchBy.Title), PackagesSearchBy, svSearchBy.Title);
                        searchMenu.AddItem(new GUIContent("in Category"), IsSearchActive(svSearchBy.Category), PackagesSearchBy, svSearchBy.Category);
                        searchMenu.AddItem(new GUIContent("in Publisher"), IsSearchActive(svSearchBy.Publisher), PackagesSearchBy, svSearchBy.Publisher);
                        searchMenu.AddItem(new GUIContent("in Notes"), IsSearchActive(svSearchBy.UserNotes), PackagesSearchBy, svSearchBy.UserNotes);
                        searchMenu.AddSeparator("");
                        searchMenu.AddDisabledItem(new GUIContent("Sort:"));
                        searchMenu.AddItem(new GUIContent("by Title (Nested Versions)"),
                            IsSortActive(svSortBy.TitleNestedVersions), PackagesSortBy, svSortBy.TitleNestedVersions);
                        searchMenu.AddItem(new GUIContent("by Title (Basic)"), IsSortActive(svSortBy.Title), PackagesSortBy, svSortBy.Title);
                        searchMenu.AddItem(new GUIContent("by Category"), IsSortActive(svSortBy.Category), PackagesSortBy, svSortBy.Category);
                        searchMenu.AddItem(new GUIContent("by Subcategory"), IsSortActive(svSortBy.CategorySub), PackagesSortBy, svSortBy.CategorySub);
                        searchMenu.AddItem(new GUIContent("by Publisher"), IsSortActive(svSortBy.Publisher), PackagesSortBy, svSortBy.Publisher);
                        searchMenu.AddItem(new GUIContent("by Size"), IsSortActive(svSortBy.Size), PackagesSortBy, svSortBy.Size);

                        searchMenu.AddSeparator("");
                        searchMenu.AddItem(new GUIContent("by Package ID"), IsSortActive(svSortBy.PackageID), PackagesSortBy, svSortBy.PackageID);
                        searchMenu.AddItem(new GUIContent("by Version ID"), IsSortActive(svSortBy.VersionID), PackagesSortBy, svSortBy.VersionID);
                        searchMenu.AddItem(new GUIContent("by Upload ID"), IsSortActive(svSortBy.UploadID), PackagesSortBy, svSortBy.UploadID);

                        searchMenu.AddSeparator("");
                        searchMenu.AddItem(new GUIContent("by Date (File Creation)"), IsSortActive(svSortBy.DateFile), PackagesSortBy, svSortBy.DateFile);
                        searchMenu.AddItem(new GUIContent("by Date (Build)"), IsSortActive(svSortBy.DateBuild), PackagesSortBy, svSortBy.DateBuild);
                        searchMenu.AddItem(new GUIContent("by Date (Publish)"), IsSortActive(svSortBy.DatePublish), PackagesSortBy, svSortBy.DatePublish);

                        if (GYA.asPurchased.results != null)
                        {
                            searchMenu.AddItem(new GUIContent("by Date (Purchased)*"), IsSortActive(svSortBy.DatePurchased), PackagesSortBy, svSortBy.DatePurchased);
                            searchMenu.AddItem(new GUIContent("by Date (Created)*"), IsSortActive(svSortBy.DateCreated), PackagesSortBy, svSortBy.DateCreated);
                            searchMenu.AddItem(new GUIContent("by Date (Updated)*"), IsSortActive(svSortBy.DateUpdated), PackagesSortBy, svSortBy.DateUpdated);
                        }
                        else
                        {
                            searchMenu.AddDisabledItem(new GUIContent("by Date (Purchased)*"));
                            searchMenu.AddDisabledItem(new GUIContent("by Date (Created)*"));
                            searchMenu.AddDisabledItem(new GUIContent("by Date (Updated)*"));
                        }

                        searchMenu.DropDown(bRect);
                    }
                }
            }

            // Search field
            fldSearch = EditorGUI.TextField(fldSearchRect, fldSearch, GUI.skin.FindStyle("ToolbarSeachTextField"));

            // Search magnifying button with down arrow, overlays default search icon
            if (fldSearch.Length > 0)
            {
                if (GUI.Button(searchEndButton, "", GUI.skin.FindStyle("ToolbarSeachCancelButton")))
                    SearchClear();
            }
            else
                GUI.Button(searchEndButton, "", GUI.skin.FindStyle("ToolbarSeachCancelButtonEmpty"));

            // Refresh Button
            if (GUI.Button(refreshButton, new GUIContent(GYATexture.iconRefresh, (gyaVars.Prefs.isSilent ? "" : "Refresh List (Rescan Asset Folders)\n\nRight-Click to manually download\nyour Purchased Assets list")), EditorStyles.toolbarButton))
            {
                if (EditorApplication.isCompiling)
                    GYAExt.Log("Refresh disabled while Unity is compiling.");
                else
                {
                    var isForceUpdate = false || evt.button == 1;

                    if (evt.button == 0 || evt.button == 1)
                    {
                        if (errorCode != ErrorCode.ErrorStep2)
                        {
                            // Update all collections
                            GYAPackage.RefreshAllCollections(isForceUpdate);

                            // If Persist enabled AND was updated, refresh Standard pkg
                            if (PersistEnable())
                            {
                                GYAPackage.RefreshStandard();
                            }

                            RefreshSV(true);

                            ddCategory = ddCategories.All;
                            ddCategoryText = string.Empty;
                            ddPublisher = 0;
                        }
                        // If in error state step2 restart gya from onenablestep2
                        else
                        {
                            LoadData();
                        }
                    }
                }
            }

            return 1;
        }

        // 2nd Toolbar
        internal int TBDrawLine2()
        {

#if UNITY_2019_GUI_TWEAKS
            wTop += controlHeight - 2; // Move to next row
            int gTop = wTop;

            int rTop = wTop + 1;
            //int sTop = wTop;
            float xPos = 0; // Current Position for control/button
            int xOffset = 6; // Left offset
            int bWidth = 26; // Button Width
#else
            wTop += controlHeight; // Move to next row
            int gTop = wTop;
            int rTop = wTop;
            //int sTop = wTop + 2;
            float xPos = 0; // Current Position for control/button
            int xOffset = 6; // Left offset
            int bWidth = 26; // Button Width
#endif

            // Calculate control layout
            GUI.BeginGroup(new Rect(xPos, gTop, position.width, controlHeight), EditorStyles.toolbar);
            xPos += xOffset;

            // Marked Dropdown
            float markedPos = xPos;
            Rect markedButton = new Rect(markedPos, wTop, bWidth * 2, controlHeight);
            xPos += bWidth * 2;

            // Reset button
            float resetPos = xPos;
            Rect resetButton = new Rect(resetPos, rTop, bWidth, controlHeight);
            xPos += bWidth;

            // Collection selection
            float collectionPos = xPos;
            Rect collectionButton = new Rect(collectionPos, wTop, position.width - (xPos * 2 - bWidth), controlHeight);

            // Prev
            float prevPos = position.width - (xOffset + bWidth * 2); // Calculate from the right
            Rect prevButton = new Rect(prevPos, rTop, bWidth, controlHeight);
            xPos = prevPos + bWidth;

            // Next
            float nextPos = xPos; // Calculate from the right
            Rect nextButton = new Rect(nextPos, rTop, bWidth, controlHeight);
            GUI.EndGroup();

            // Process Events

            // Marked Assets Popup
            if (GUI.Button(markedButton, new GUIContent(GYAPackage.countMarkedToImport.ToString(), (gyaVars.Prefs.isSilent ? "" : "# of Marked Assets")), tbStyle.dropdown))
            {
                if (evt.button == 0)
                {
                    TBPopUpMarked(markedPos);
                }
            }

            // Reset button
            bool resetActive = isResetActive();

            if (GUI.Button(resetButton, new GUIContent((!resetActive ? GYATexture.iconReset : GYATexture.iconResetX), (gyaVars.Prefs.isSilent ? "" : "Reset View to defaults except for:\nCurrent Collection/Group\nMarked Assets")), EditorStyles.toolbarButton))
            {
                if (evt.button == 0)
                {
                    SearchClear();
                    PackagesSearchBy(svSearchBy.Title);
                    PackagesSortBy(svSortBy.TitleNestedVersions);
                    ddCategory = ddCategories.All;
                    ddCategoryText = String.Empty;
                    ddPublisher = 0;
                }
            }

            // Collection selection
            if (GUI.Button(collectionButton, new GUIContent(activeCollectionText, (gyaVars.Prefs.isSilent ? "" : "Select a Collection\nor Group")), tbStyle.dropdown))
            {
                if (evt.button == 0)
                {
                    TBPopUpAssets(collectionPos);
                }
            }

            // Prev
            if (GUI.Button(prevButton, new GUIContent(GYATexture.iconPrev, (gyaVars.Prefs.isSilent ? "" : "Previous Collection\nor Group")), EditorStyles.toolbarButton))
            {
                if (evt.button == 0)
                {
                    PrevSelection();
                }
            }

            // Next
            if (GUI.Button(nextButton, new GUIContent(GYATexture.iconNext, (gyaVars.Prefs.isSilent ? "" : "Next Collection\nor Group")), EditorStyles.toolbarButton))
            {
                if (evt.button == 0)
                {
                    NextSelection();
                }
            }

            return 1;
        }

        // Is reset button active
        internal bool isResetActive()
        {
            return fldSearch != "" || searchActive != svSearchBy.Title ||
                   sortActive != svSortBy.TitleNestedVersions ||
                   ddCategory != ddCategories.All || ddPublisher != 0;
        }

        // 3rd Toolbar
        internal int TBDrawLine3()
        {

#if UNITY_2019_GUI_TWEAKS
            wTop += controlHeight - 2; // Move to next row
            int gTop = wTop;
            wTop += 1;

            float xPos = 0;
            int xOffset = 6;
            int numButtons = 7;
            float adjWidth = (position.width - xOffset * 2) / numButtons;
            int clickLeft = -1;
#else
            wTop += controlHeight; // Move to next row
            int gTop = wTop;
            float xPos = 0;
            int xOffset = 6;
            int numButtons = 7;
            float adjWidth = (position.width - xOffset * 2) / numButtons;
            int clickLeft = -1;
#endif

            Rect[] buttonTB3 = new Rect[numButtons];

            // Calculate control layout
            GUI.BeginGroup(new Rect(xPos, gTop, position.width, controlHeight), EditorStyles.toolbar);
            GUI.EndGroup();
            xPos += xOffset;

            // Loop Buttons
            for (int i = 0; i < numButtons; i++)
            {
                // Rects
                buttonTB3[i] = new Rect(xPos, wTop, adjWidth, controlHeight);
                xPos += adjWidth;
                // Events
                if ((evt.type == EventType.MouseUp) && buttonTB3[i].Contains(evt.mousePosition))
                {
                    if (evt.button == 0)
                    {
                        clickLeft = i;
                    }
                }
            }

            // Process Event, could use 'showActive = GUI.Toggle(..)', but to allow extra control when desired, it's handled this way
            switch (clickLeft)
            {
                case 0:
                    if (GYAPackage.countMarkedToImport != 0) // Only toggle if there are marked assets
                        TBPopUpCallback("ShowAllSelected");
                    else
                    {
                        bool resetActive = isResetActive();
                        if ((GYAPackage.countMarkedToImport == 0) && resetActive) // Only toggle if there are 0 marked & reset is active
                            TBPopUpCallback("ShowAllSelected");
                    }
                    break;
                case 1:
                    showActive = svCollection.All;
                    break;
                case 2:
                    showActive = svCollection.Store;
                    break;
                case 3:
                    showActive = svCollection.User;
                    break;
                case 4:
                    showActive = svCollection.Standard;
                    break;
                case 5:
                    showActive = svCollection.Old;
                    break;
                case 6:
                    showActive = svCollection.Group;
                    break;
                default:
                    break;
            }

            // Draw controls as defined by the layout, processed after events or they will not trigger
            GUI.Toggle(buttonTB3[0], (ddCategory == ddCategories.IsMarked), new GUIContent(svData.Count.ToString(), (gyaVars.Prefs.isSilent ? "" : "# of Visible Assets\n\nToggle for 'Show All Selected'")), EditorStyles.toolbarButton);
            GUI.Toggle(buttonTB3[1], (showActive == svCollection.All), new GUIContent(GYATexture.iconAll, (gyaVars.Prefs.isSilent ? "" : "All Assets")), EditorStyles.toolbarButton);
            GUI.Toggle(buttonTB3[2], (showActive == svCollection.Store), new GUIContent(GYATexture.iconStore, (gyaVars.Prefs.isSilent ? "" : "Store Assets")), EditorStyles.toolbarButton);
            GUI.Toggle(buttonTB3[3], (showActive == svCollection.User), new GUIContent(GYATexture.iconUser, (gyaVars.Prefs.isSilent ? "" : "User Assets")), EditorStyles.toolbarButton);
            GUI.Toggle(buttonTB3[4], (showActive == svCollection.Standard), new GUIContent(GYATexture.iconStandard, (gyaVars.Prefs.isSilent ? "" : "Standard Assets")), EditorStyles.toolbarButton);
            GUI.Toggle(buttonTB3[5], (showActive == svCollection.Old), new GUIContent(GYATexture.iconOld, (gyaVars.Prefs.isSilent ? "" : "Old Assets")), EditorStyles.toolbarButton);
            GUI.Toggle(buttonTB3[6], (showActive == svCollection.Group), new GUIContent(GYATexture.iconOptions, (gyaVars.Prefs.isSilent ? "" : "Group Assets\n\nUse the Prev/Next buttons or\nCollections/Groups dropdown\nto change visible group.")), EditorStyles.toolbarButton);
            return 1;
        }

        // Builds complete sorted publisher.label list
        internal List<String> PublisherListBuildLIST()
        {
            List<GYAData.Asset> packagesTemp = new List<GYAData.Asset>(gyaData.Assets);

            packagesTemp.RemoveAll(x => x.publisher.label == "");
            packagesTemp.Sort((x, y) => String.CompareOrdinal(x.publisher.label, y.publisher.label));
            List<string> pkgList = packagesTemp.Select(p => p.publisher.label).Distinct().ToList();
            packagesTemp = null;

            // Categories
            for (int i = 0; i < pkgList.Count; ++i)
            {
                pkgList[i] = pkgList[i].Replace('/', '\\').Replace('&', '+');
            }

            return pkgList;
        }

        // Builds complete sorted publisher.label/id dict
        internal Dictionary<string, int> PublisherListBuild()
        {
            List<GYAData.Asset> packagesTemp = new List<GYAData.Asset>(gyaData.Assets);
            packagesTemp.RemoveAll(x => x.publisher.label == "");
            packagesTemp.Sort((x, y) => String.CompareOrdinal(x.publisher.label, y.publisher.label));


            IEnumerable<GYAData.Asset> resultsDupes = from package in packagesTemp
                                                      group package by package.publisher.label
                into grouped
                                                      from package in grouped.Skip(0)
                                                      select package;
            packagesTemp = null;

            List<GYAData.Asset> resultsOld = resultsDupes.ToList();

            Dictionary<string, int> pkgList = new Dictionary<string, int>();
            // Build list minus dupes
            foreach (var result in resultsOld)
            {
                var pubLabel = result.publisher.label.Replace('/', '\\').Replace('&', '+');
                if (!pkgList.ContainsKey(pubLabel))
                {
                    pkgList.Add(pubLabel, result.publisher.id);
                }
            }
            return pkgList;
        }

        // Builds complete sorted category.label list
        internal List<string> CategoryListBuild()
        {
            List<GYAData.Asset> packagesTemp = new List<GYAData.Asset>(gyaData.Assets);

            // Remove Exported entries as they do not have a category
            packagesTemp.RemoveAll(x => x.category.label == "");
            packagesTemp.Sort((x, y) => String.CompareOrdinal(x.category.label, y.category.label));
            List<string> pkgCategories = packagesTemp.Select(p => (p.category.label).Split('/')[0]).Distinct().ToList();
            packagesTemp = null;

            // Categories
            for (int i = 0; i < pkgCategories.Count; ++i)
            {
                pkgCategories[i] = pkgCategories[i].Replace('/', '\\').Replace('&', '+');
            }

            return pkgCategories;
        }

        // Builds complete sorted sub-category.label list
        internal List<String> SubCategoryListBuild()
        {
            List<GYAData.Asset> packagesTemp = new List<GYAData.Asset>(gyaData.Assets);

            // Remove Exported entries as they do not have a category
            packagesTemp.RemoveAll(x => x.category.label == "");
            // Remove any entries that are not a sub-category
            packagesTemp.RemoveAll(x => !x.category.label.Contains("/"));
            packagesTemp.Sort((x, y) => String.CompareOrdinal(x.category.label, y.category.label));
            List<string> pkgCategories = packagesTemp.Select(p => p.category.label).Distinct().ToList();
            packagesTemp = null;

            // Sub-Categories
            for (int i = 0; i < pkgCategories.Count; ++i)
            {
                pkgCategories[i] = pkgCategories[i].Replace('/', '\\').Replace('&', '+');
            }

            return pkgCategories;
        }

        // Builds complete sorted sub-category.label list for Nested Sub-Menu's
        internal List<String> SubCategoryListBuildNested(bool getRoot = false)
        {
            List<GYAData.Asset> packagesTemp = new List<GYAData.Asset>(gyaData.Assets);

            // Remove Exported entries as they do not have a category
            packagesTemp.RemoveAll(x => x.category.label == "");
            packagesTemp.Sort((x, y) => String.CompareOrdinal(x.category.label, y.category.label));
            List<string> pkgCategories = packagesTemp.Select(p => p.category.label).Distinct().ToList();
            packagesTemp = null;

            // Sub-Categories
            for (int i = 0; i < pkgCategories.Count; ++i)
            {
                pkgCategories[i] = pkgCategories[i].Replace('&', '+');
            }

            List<string> newList = new List<string>();

            foreach (string t in pkgCategories)
            {
                int itemElements = t.Split('/').Length;

                if (getRoot)
                {
                    // Return category minus lastindex
                    if (itemElements > 1)
                        newList.Add(t.Substring(0, t.LastIndexOf('/')));
                }
                // Return full category
                else
                    newList.Add(t);
            }
            newList = newList.Distinct().ToList();
            newList.Sort();
            pkgCategories = null;

            return newList;
        }

        internal void PrevSelection()
        {
            //        (int)showActive = 0 = All, 1 = Store, 2 = Standard, 3 = User, 4 = Old, 5 = Groups
            //        ddCollections.ElementAt(currentSelection).Key
            //        0-4 = Selections
            //        5+  = Groups, starting with Favorites

            int itemActive = (int)showActive;

            if (itemActive == 0)
            {
                // Selection All Packages
                itemActive = 5; // Selection Groups
                showGroup = gyaData.Groups.Count - 1; // Group Last
            }
            else
            {
                // If Groups and current group != 0
                if (itemActive == 5 && showGroup > 0)
                    showGroup -= 1; // Group Prev
                else
                    itemActive -= 1; // Selection Prev
            }

            switch (itemActive)
            {
                case 0: // Selections
                    if (gyaVars.FilesCount.all > 0)
                    {
                        SVPopUpCollection(svCollection.All);
                        break;
                    }
                    goto case 5;
                case 1:
                    if (gyaVars.FilesCount.store > 0)
                    {
                        SVPopUpCollection(svCollection.Store);
                        break;
                    }
                    goto case 0;
                case 2:
                    if (gyaVars.FilesCount.user > 0)
                    {
                        SVPopUpCollection(svCollection.User);
                        break;
                    }
                    goto case 1;
                case 3:
                    if (gyaVars.FilesCount.standard > 0)
                    {
                        SVPopUpCollection(svCollection.Standard);
                        break;
                    }
                    goto case 2;
                case 4:
                    if (gyaVars.FilesCount.old > 0)
                    {
                        SVPopUpCollection(svCollection.Old);
                        break;
                    }
                    goto case 3;
                case 5: // Groups
                    if (gyaData.Groups.Count > 0)
                    {
                        SVPopUpCollection(svCollection.Group);
                        break;
                    }
                    goto case 4;
                default:
                    SVPopUpCollection(svCollection.All);
                    break;
            }
        }

        internal void NextSelection()
        {
            //        (int)showActive = 0 = All, 1 = Store, 2 = Standard, 3 = User, 4 = Old, 5 = Groups
            //        ddCollections.ElementAt(currentSelection).Key
            //        0-4 = Selections
            //        5+  = Groups, starting with Favorites

            int itemActive = (int)showActive;

            // Next
            if (itemActive != 5)
            {
                // Selection Groups
                itemActive += 1; // Selection Next
                showGroup = 0; // Group First - Favorites
            }
            else
            {
                // If Groups and current group != last group
                if (itemActive == 5 && showGroup < gyaData.Groups.Count - 1)
                    showGroup += 1; // Next Group
                else
                    itemActive = 0; // Selection Asset Store
            }

            switch (itemActive)
            {
                case 0: // Selections
                    if (gyaVars.FilesCount.all > 0)
                    {
                        SVPopUpCollection(svCollection.All);
                        break;
                    }
                    goto case 1;
                case 1:
                    if (gyaVars.FilesCount.store > 0)
                    {
                        SVPopUpCollection(svCollection.Store);
                        break;
                    }
                    goto case 2;
                case 2:
                    if (gyaVars.FilesCount.user > 0)
                    {
                        SVPopUpCollection(svCollection.User);
                        break;
                    }
                    goto case 3;
                case 3:
                    if (gyaVars.FilesCount.standard > 0)
                    {
                        SVPopUpCollection(svCollection.Standard);
                        break;
                    }
                    goto case 4;
                case 4:
                    if (gyaVars.FilesCount.old > 0)
                    {
                        SVPopUpCollection(svCollection.Old);
                        break;
                    }
                    goto case 5;
                case 5: // Groups
                    if (gyaData.Groups.Count > 0)
                    {
                        SVPopUpCollection(svCollection.Group);
                        break;
                    }
                    goto case 0;
                default:
                    SVPopUpCollection(svCollection.All);
                    break;
            }
        }

        // Builds same list as shown in Asset sections/groups popup
        internal void BuildPrevNextList()
        {
            // Get current counts
            GYAPackage.TallyAssets();

            int iterateCount = 0;

            // Catch invalid data
            if (ddCollections != null)
                ddCollections.Clear();
            else
            {
                ErrorStateSet(ErrorCode.ErrorStep2);
                return;
            }

            ddCollections.Add(iterateCount, svCollection.All + " ( " + gyaVars.FilesCount.all + " )");
            iterateCount += 1;
            ddCollections.Add(iterateCount, svCollection.Store + " ( " + gyaVars.FilesCount.store + " )");
            iterateCount += 1;
            ddCollections.Add(iterateCount, svCollection.User + " ( " + gyaVars.FilesCount.user + " )");
            iterateCount += 1;
            ddCollections.Add(iterateCount, svCollection.Standard + " ( " + gyaVars.FilesCount.standard + " )");
            iterateCount += 1;

            if (gyaVars.FilesCount.oldToMove > 0)
                ddCollections.Add(iterateCount, svCollection.Old + " ( " + gyaVars.FilesCount.old + " - " + gyaVars.FilesCount.oldToMove + " )");
            else
                ddCollections.Add(iterateCount, svCollection.Old + " ( " + gyaVars.FilesCount.old + " )");

            iterateCount += 1;

            for (int i = 0; i < gyaData.Groups.Count; ++i)
            {
                string grpLine = gyaData.Groups[i].name;
                ddCollections.Add(iterateCount, grpLine);
                iterateCount += 1;
            }
            PackagesSortBy(sortActive);
        }

        // Popup window routine for marked packages
        internal void TBPopUpMarked(float xPos)
        {
	        GenericMenu markedMenu = new GenericMenu();
            
	        markedMenu.AddItem(new GUIContent("Selected Assets Options:"), false, TBPopUpCallback, "null");
	        markedMenu.AddSeparator("");
	        
	        if (GYAPackage.countMarkedToImport > 0)
            {
                markedMenu.AddItem(new GUIContent("Import Selected"), false, TBPopUpCallback, "PackageImportMultiple");
                if (showActive == svCollection.Group)
                    markedMenu.AddItem(new GUIContent("Import Entire Group"), false, TBPopUpCallback, "PackageImportGroup");
                else
                    markedMenu.AddDisabledItem(new GUIContent("Import Entire Group"));

                markedMenu.AddSeparator("");

                markedMenu.AddItem(new GUIContent("Open URL of Selected Packages"), false,
                    GYAPackage.BrowseSelectedPackages, "BrowseSelectedPackages");

                markedMenu.AddSeparator("");

                // grpsub: handle sub groups
                for (int i = 0; i < gyaData.Groups.Count; ++i)
                {
                    string grpLine = ddCollections.ElementAt(5 + i).Value;

                    if ((i + 1 < gyaData.Groups.Count) && ddCollections.ElementAt(5 + (i + 1)).Value.Contains(grpLine))
                    {
                        //assetMenu.AddItem(new GUIContent(grpLine + "/"), (showGroup == i && showActive == svCollection.Group), TBPopUpShowGroup, i);
                        markedMenu.AddItem(new GUIContent("Add to Group/" + grpLine + "/"), false, GroupAddToMultiple, i);
                        markedMenu.AddItem(new GUIContent("Add to Group/" + grpLine), false, GroupAddToMultiple, i);
                    }
                    else // Show normal
                    {
                        markedMenu.AddItem(new GUIContent("Add to Group/" + grpLine), false, GroupAddToMultiple, i);
                    }
                }

                if (showActive == svCollection.Group)
                {
                    // grpsub: handle sub groups - Move To Group
                    for (int i = 0; i < gyaData.Groups.Count; ++i)
                    {
                        string grpLine = ddCollections.ElementAt(5 + i).Value;

                        if ((i + 1 < gyaData.Groups.Count) && ddCollections.ElementAt(5 + (i + 1)).Value.Contains(grpLine))
                        {
                            //assetMenu.AddItem(new GUIContent(grpLine + "/"), (showGroup == i && showActive == svCollection.Group), TBPopUpShowGroup, i);
                            markedMenu.AddItem(new GUIContent("Move to Group/" + grpLine + "/"), false, GroupMoveToMultiple, i);
                            markedMenu.AddItem(new GUIContent("Move to Group/" + grpLine), false, GroupMoveToMultiple, i);
                        }
                        else // Show normal
                        {
                            markedMenu.AddItem(new GUIContent("Move to Group/" + grpLine), false, GroupMoveToMultiple, i);
                        }
                    }

                    //markedMenu.AddSeparator("");
                    //if (showActive == svCollection.Group)
                    //{
                    // Remove asset from group
                    markedMenu.AddItem(new GUIContent("Remove from Current Group"), false, GroupRemoveAssetMultiple, showGroup);
                }
                else
                {
                    markedMenu.AddDisabledItem(new GUIContent("Move to Group"));
                    markedMenu.AddDisabledItem(new GUIContent("Remove from Current Group"));
                }
                markedMenu.AddSeparator("");

                // Project specific assets (installed)
                //markedMenu.AddItem(new GUIContent("Add to Current Project"), false, ProjectAddAssetMultiple, null);
                //markedMenu.AddItem(new GUIContent("Remove from Current Project"), false, ProjectRemoveAssetMultiple, null);
                //markedMenu.AddSeparator("");

                // FILE OPTIONS - Copy To ...
                if (gyaVars.Prefs.pathUserAssets.Any() && Directory.Exists(gyaVars.Prefs.pathUserAssets[0]))
                {
                    markedMenu.AddItem(new GUIContent("File Options/Copy To User Folder"), false, TBPopUpCallback, "CopyToUserMulti");
                }
                else
                {
                    markedMenu.AddDisabledItem(new GUIContent("File Options/Copy To User Folder"));
                }
                markedMenu.AddItem(new GUIContent("File Options/Copy To ..."), false, TBPopUpCallback, "CopyToSelectableMulti");
                markedMenu.AddItem(new GUIContent("File Options/"), false, TBPopUpCallback, "");
                markedMenu.AddItem(new GUIContent("File Options/Copy To Sorted Folder"), false, TBPopUpCallback, "CopyToSortedMulti");
                markedMenu.AddItem(new GUIContent("File Options/Move To Sorted Folder"), false, TBPopUpCallback, "MoveToSortedMulti");
                markedMenu.AddItem(new GUIContent("File Options/"), false, TBPopUpCallback, "");
                markedMenu.AddItem(new GUIContent("File Options/Rename with Version"), false, TBPopUpCallback, "RenameWithVersionSelected");
                markedMenu.AddItem(new GUIContent("File Options/"), false, TBPopUpCallback, "");
                markedMenu.AddItem(new GUIContent("File Options/Delete ALL Selected"), false, TBPopUpCallback, "DeleteAssetMultiple");

                markedMenu.AddSeparator("");
                if (gyaData.Assets.FindAll(x => x.isMarked).Any())
                    markedMenu.AddItem(new GUIContent("Show All Selected"), (ddCategory == ddCategories.IsMarked), TBPopUpCallback, "ShowAllSelected");
                else
                    markedMenu.AddDisabledItem(new GUIContent("Show All Selected"));

                markedMenu.AddSeparator("");
                markedMenu.AddDisabledItem(new GUIContent("Post-Action Toggles:"));
                if (gyaVars.Prefs.clearMarkedAfterGlobal)
                    markedMenu.AddDisabledItem(new GUIContent("Auto-Deselect Visible List"));
                else
                    markedMenu.AddItem(new GUIContent("Auto-Deselect Visible List"), gyaVars.Prefs.clearMarkedAfter, TBPopUpCallback, "ClearMarkedAfter");
                markedMenu.AddItem(new GUIContent("Auto-Deselect Master List"), gyaVars.Prefs.clearMarkedAfterGlobal, TBPopUpCallback, "ClearMarkedAfterGlobal");
                markedMenu.AddSeparator("");
                markedMenu.AddDisabledItem(new GUIContent("Actions on Marked Items (Visible):"));
                markedMenu.AddItem(new GUIContent("Select All"), false, TBPopUpCallback, "SelectVisible");
                markedMenu.AddItem(new GUIContent("Select All Old Versions"), false, TBPopUpCallback, "SelectMultipleVersions");
                markedMenu.AddItem(new GUIContent("Invert Selections"), false, TBPopUpCallback, "InvertMarked");
		        markedMenu.AddItem(new GUIContent("Deselect All (Visible)"), false, TBPopUpCallback, "ClearMarked");
		        markedMenu.AddItem(new GUIContent("Deselect All (Except Visible)"), false, TBPopUpCallback, "ClearNotVisible");
		        markedMenu.AddSeparator("");
                markedMenu.AddItem(new GUIContent("Deselect All (Master)"), false, TBPopUpCallback, "ClearMarkedEverything");
                markedMenu.AddSeparator("");
                markedMenu.AddItem(new GUIContent("Export List as CSV"), false, TBPopUpCallback, "ExportListAsCSV");
            }
            else
            {
                markedMenu.AddDisabledItem(new GUIContent("Import Selected"));
                if (showActive == svCollection.Group)
                    markedMenu.AddItem(new GUIContent("Import Entire Group"), false, TBPopUpCallback, "PackageImportGroup");
                else
                    markedMenu.AddDisabledItem(new GUIContent("Import Entire Group"));

                markedMenu.AddSeparator("");
                markedMenu.AddDisabledItem(new GUIContent("Open URL of Selected Packages"));
                markedMenu.AddSeparator("");
                markedMenu.AddDisabledItem(new GUIContent("Add to Group"));
                markedMenu.AddDisabledItem(new GUIContent("Move to Group"));
                //markedMenu.AddSeparator("");
                markedMenu.AddDisabledItem(new GUIContent("Remove from Current Group"));
                markedMenu.AddSeparator("");

                // Project specific assets (installed)
                //markedMenu.AddDisabledItem(new GUIContent("Add to Current Project"));
                //markedMenu.AddDisabledItem(new GUIContent("Remove from Current Project"));
                //markedMenu.AddSeparator("");

                markedMenu.AddDisabledItem(new GUIContent("File Options/Copy To User Folder"));
                markedMenu.AddDisabledItem(new GUIContent("File Options/Copy To ..."));
                markedMenu.AddDisabledItem(new GUIContent("File Options/"));
                markedMenu.AddDisabledItem(new GUIContent("File Options/Copy To Sorted Folder"));
                markedMenu.AddDisabledItem(new GUIContent("File Options/Move To Sorted Folder"));
                markedMenu.AddDisabledItem(new GUIContent("File Options/"));
                markedMenu.AddDisabledItem(new GUIContent("File Options/Rename with Version"));
                markedMenu.AddDisabledItem(new GUIContent("File Options/"));
                markedMenu.AddDisabledItem(new GUIContent("File Options/Delete ALL Selected"));

                markedMenu.AddDisabledItem(new GUIContent("File Options/"));
                if (showActive == svCollection.Group)
                    markedMenu.AddItem(new GUIContent("Export Group Data as CSV"), false, TBPopUpCallback, "ExportAsCSVGroup");

                markedMenu.AddSeparator("");
                markedMenu.AddDisabledItem(new GUIContent("Show All Selected"));

                markedMenu.AddSeparator("");
                markedMenu.AddDisabledItem(new GUIContent("Post-Action Toggles:"));
                if (gyaVars.Prefs.clearMarkedAfterGlobal)
                    markedMenu.AddDisabledItem(new GUIContent("Auto-Deselect Visible List"));
                else
                    markedMenu.AddItem(new GUIContent("Auto-Deselect Visible List"), gyaVars.Prefs.clearMarkedAfter, TBPopUpCallback, "ClearMarkedAfter");
                markedMenu.AddItem(new GUIContent("Auto-Deselect Master List"), gyaVars.Prefs.clearMarkedAfterGlobal, TBPopUpCallback, "ClearMarkedAfterGlobal");
                markedMenu.AddSeparator("");
                markedMenu.AddDisabledItem(new GUIContent("Actions on Marked Items (Visible):"));
                markedMenu.AddItem(new GUIContent("Select All"), false, TBPopUpCallback, "SelectVisible");
                markedMenu.AddItem(new GUIContent("Select All Old Versions"), false, TBPopUpCallback, "SelectMultipleVersions");
                markedMenu.AddDisabledItem(new GUIContent("Invert Selections"));
	            markedMenu.AddDisabledItem(new GUIContent("Deselect All (Visible)"));
	            markedMenu.AddDisabledItem(new GUIContent("Deselect All (Except Visible)"));
                markedMenu.AddSeparator("");
                markedMenu.AddDisabledItem(new GUIContent("Deselect All (Global)"));
                markedMenu.AddSeparator("");
                markedMenu.AddItem(new GUIContent("Export List as CSV"), false, TBPopUpCallback, "ExportListAsCSV");
            }
            markedMenu.DropDown(new Rect(xPos, wTop, 0, 18));
            evt.Use();
        }

        // Popup window routine to Select which collection to show
        internal void TBPopUpAssets(float xPos)
        {
            // Show Asset sections
            GenericMenu assetMenu = new GenericMenu();

            assetMenu.AddDisabledItem(new GUIContent("Collections:"));
            assetMenu.AddItem(new GUIContent(ddCollections.ElementAt(0).Value), (showActive == svCollection.All), TBPopUpCallback, "AllAssets");
            assetMenu.AddItem(new GUIContent(ddCollections.ElementAt(1).Value), (showActive == svCollection.Store), TBPopUpCallback, "StoreAssets");
            assetMenu.AddItem(new GUIContent(ddCollections.ElementAt(2).Value), (showActive == svCollection.User), TBPopUpCallback, "UserAssets");
            assetMenu.AddItem(new GUIContent(ddCollections.ElementAt(3).Value), (showActive == svCollection.Standard), TBPopUpCallback, "StandardAssets");
            assetMenu.AddItem(new GUIContent(ddCollections.ElementAt(4).Value), (showActive == svCollection.Old), TBPopUpCallback, "OldAssets");
            assetMenu.AddSeparator("");

            assetMenu.AddDisabledItem(new GUIContent("Groups:"));

            // Prescan to check if is a subgrp, if so append '/'
            for (int i = 0; i < gyaData.Groups.Count; ++i)
            {
                string grpLine = ddCollections.ElementAt(5 + i).Value;

                if ((i + 1 < gyaData.Groups.Count) && ddCollections.ElementAt(5 + (i + 1)).Value.Contains(grpLine))
                {
                    assetMenu.AddItem(new GUIContent(grpLine + "/"), false, TBPopUpShowGroup, i);
                    assetMenu.AddItem(new GUIContent(grpLine), (showGroup == i && showActive == svCollection.Group), TBPopUpShowGroup, i);
                }
                else // Show normal
                {
                    assetMenu.AddItem(new GUIContent(grpLine), (showGroup == i && showActive == svCollection.Group), TBPopUpShowGroup, i);
                }
            }

            // End subgrp testing

            assetMenu.AddSeparator("");
            assetMenu.AddItem(new GUIContent("Add New Group .."), false, EditorWindowGYAGroups);

            assetMenu.DropDown(new Rect(xPos, wTop, 0, 18));
            evt.Use();
        }

        // Group Popup Add To Menu Callback
        internal void TBPopUpShowGroup(object obj)
        {
            showGroup = (int)obj;
            SVPopUpCollection(svCollection.Group);
            infoChanged = true;
        }

        internal void TBCatCallback(object obj)
        {
            if (obj is ddCategories)
            {
                ddCategory = (ddCategories)obj;
                ddCategoryText = String.Empty;
                ddPublisher = 0;

                SVPopUpCollection(svCollection.All);
                showGroup = 0;
            }
            else
            {
                ddCategory = ddCategories.UserSelection;
                ddCategoryText = obj.ToString();
                ddPublisher = 0;
            }
            infoChanged = true;
        }

        internal void TBPubCallback(object obj)
        {
            if (obj.ToString() == "ALL")
            {
                ddCategory = ddCategories.All;
                ddCategoryText = String.Empty;
                ddPublisher = 0;
            }
            else
            {
                ddCategory = ddCategories.All;
                ddCategoryText = String.Empty;
                ddPublisher = Convert.ToInt32(obj.ToString());
            }
            infoChanged = true;
        }


        // Callback - Copy passed text to clipboard
        internal void ClipboardCallback(object obj)
        {
            if (obj.ToString() != null)
            {
                TextEditor te = new TextEditor();
#if UNITY_5_2_OR_OLDER
                te.content = new GUIContent (obj.ToString ());
#else
                te.text = obj.ToString();
#endif
                te.SelectAll();
                te.Copy();
            }
        }

        void GYAPopup(int unusedWindowID)
        {
            GUILayout.Button("Hi");
            GUI.DragWindow();
        }

        // Callback check from object
        internal void TBPopUpCallback(object obj)
        {
            string passedVar = String.Empty;

            ////null check, no package is selected upon start, so this is just to be safe
            //if (svCurrentPkg == null || string.IsNullOrEmpty(svCurrentPkg.title))
            //{
            //    GYAExt.Log("svCurrentPkg returned null - Nothing was processed, please try again ..", "");
            //    return;
            //}

            // If : next obj is a passed value
            if (obj.ToString().Contains(":"))
            {
                passedVar = obj.ToString().Split(':')[1];
                if (passedVar != null)
                {
                }
            }

            // PopupNotes - .Notes
            if (obj.ToString().Contains("PopupNotes"))
            {
                try
                {
                    GYAWindowNotes.Init();
                }
                catch (Exception e)
                {
                    if (e.GetType() != typeof(ExitGUIException))
                    {
                        Debug.LogException(e);
                    }
                }
            }

            if (obj.ToString().Contains("AutoPreventASOverwrite"))
            {
                if (gyaVars.Prefs.autoPreventASOverwrite)
                {
                    // If enabled, verify disabling
                    if (EditorUtility.DisplayDialog(gyaVars.abbr + " - Disable Overwrite Protection",
                        "Are you sure you want to DISABLE Overwrite Protection for Asset Store files?\n\n"
                        + "Newly added assets will no longer be renamed.\n"
                        + "As noted when enabling, disabling will NOT revert the renamed files.\n"
                        , "Cancel", "DISABLE"))
                    {
                        // Cancel - do nothing
                    }
                    else
                    {
                        gyaVars.Prefs.autoPreventASOverwrite = false;
                        GYAExt.Log("Protect Current Versions from Overwrite: " + (gyaVars.Prefs.autoPreventASOverwrite ? "ENABLED" : "DISABLED")
                                  + " .. New package downloads will no longer be protected\n");
                        GYAFile.SaveGYAPrefs();
                    }
                }
                else
                {
                    // If disabled, verify enabling
                    if (EditorUtility.DisplayDialog(gyaVars.abbr + " - Enable Overwrite Protection",
                        "WARNING: This will RENAME EVERY asset file in the Asset Store folder!\n\n"
                        + "This will prevent asset updates from overwriting existing/older versions.\n\n"
                        + "Once enabled, even disabling will NOT revert the renamed files.\n"
                        + "NOTE: This may take a few minutes.\n"
                        , "Cancel", "ENABLE"))
                    {
                        // Cancel - do nothing
                    }
                    else
                    {
                        gyaVars.Prefs.autoPreventASOverwrite = true;
                        GYAExt.Log("Protect Current Versions from Overwrite: " + (gyaVars.Prefs.autoPreventASOverwrite ? "ENABLED" : "DISABLED")
                                   + " .. Current packages in the Asset Store folder and future downloads will be protected."
                                   , "Renaming files - This may take a minute.  Once done, your Asset Store folder will be rescanned.");
                        GYAFile.SaveGYAPrefs();
                        GYAPackage.RefreshAllCollections();
                        GYAExt.Log("Asset Store packages within the Asset Store folder are now protected from Overwrite.");
                    }
                }
            }

            if (obj.ToString().Contains("RenameWithVersionRemovedCollection"))
            {
                // Do not run if autoPreventASOverwrite is enabled
                if (gyaVars.Prefs.autoPreventASOverwrite)
                {
                    EditorUtility.DisplayDialog(gyaVars.abbr + " - Attention ..",
                        "Unable to proceed while Overwrite Protection is enabled!\n\n"
                        + "Please disable Overwrite Protection and try again.\n"
                        , "OK");
                }
                else
                {
                    // Verify renaming
                    if (EditorUtility.DisplayDialog(gyaVars.abbr + " - Rename Without Versions",
                        "WARNING: This will RENAME EVERY asset file in the Asset Store folder!\n\n"
                        + "This will remove the GYA applied version by renaming each asset to it's 'title'.\n\n"
                        + "NOTE: This may take a few minutes.\n"
                        , "Cancel", "OK"))
                    {
                        // Cancel - do nothing
                    }
                    else
                    {
                        //gyaVars.Prefs.autoPreventASOverwrite = false;
                        GYAExt.Log("Removing Versions from Files .. New package downloads will no longer be protected.",
                                 "Once done, your Asset Store folder will be rescanned.");
                        GYAFile.RenameWithVersionRemovedCollection();
                    }
                }
            }

            // ForceRebuildFolderStore
            if (obj.ToString().Contains("ForceRebuildFolderStore"))
            {
                if (EditorUtility.DisplayDialog(gyaVars.abbr + " - Rebuild AS Folder",
                    "This will remove all Store entries and rescan the Asset Store folder to rebuild the list.\n"
                    , "REBUILD", "Cancel"))
                {
                    GYAPackage.AssetIndexRemoveEntryAll(svCollection.Store);
                    GYAPackage.RefreshAllCollections();
                    //GYAPackage.RefreshStore();
                    //RefreshSV(false);
                    //GYAFile.SaveGYAAssets();
                    //GYAFile.LoadGYAUserData();
                }
            }

            // Quick Ref window
            if (obj.ToString() == "QuickRef")
                GYAWindowPrefs.Init(0);

            // Prefs window
            if (obj.ToString() == "winPrefs")
                GYAWindowPrefs.Init(1);

            // Data window
            if (obj.ToString() == "winData")
                GYAWindowASData.Init(0);

            if (obj.ToString().Contains("BackupUserFile"))
            {
                if (EditorUtility.DisplayDialog(gyaVars.abbr + " - Backup User File",
                    "Are you sure you want to BACKUP the User File?\n\n" +
                    "This will save your current Prefs & Groups.\n\n"
                    , "BACKUP", "Cancel"))
                {
                    GYAFile.BackupUserFiles();
                }
            }

            if (obj.ToString().Contains("RestoreUserFile"))
            {
                if (EditorUtility.DisplayDialog(gyaVars.abbr + " - Restore User File",
                    "Are you sure you want to RESTORE the User File?\n\n" +
                    "This will restore saved Prefs & Groups.\n\n"
                    , "RESTORE", "Cancel"))
                {
                    GYAFile.RestoreUserFiles();
                    GYAPackage.RefreshAllCollections();
                }
            }

            // Move asset to old assets folder - Used prior to updating a package from the Asset Store
            if (obj.ToString() == "MoveToOld")
            {
                if (EditorUtility.DisplayDialog(gyaVars.abbr + " - Move Selected Asset",
                    "Are you sure you want to MOVE this asset?\n\n" +
                    "This is handy to temporarily backup the asset just prior to downloading a new version.\n\n" +
                    svCurrentPkg.title + "\n\nTo: " + gyaVars.pathOldAssetsFolder
                    , "MOVE", "Cancel"))
                {
                    // move
                    var fileData = GYAFile.MoveAssetToPath(svCurrentPkg);
                    if (fileData.Key > 0)
                    {
                        //if (!gyaVars.Prefs.isSilent)
                        GYAExt.Log("( " + fileData.Key + " ) package(s) moved to the Old Assets Folder.", fileData.Value);
                    }

                    GYAPackage.RefreshAllCollections();
                }
            }

            // Copy assets to user folder
            if (obj.ToString() == "CopyToUserMulti")
            {
                if (EditorUtility.DisplayDialog(gyaVars.abbr + " - Copy Selected Assets To User folder",
                    "Copying is NOT performed in a seperate thread at this time.\n\n" +
                    "Unity may seem to pause during the copy.\n\n" +
                    "This is here purely for convenience.\n\n" +
                    "Copy To: " + gyaVars.Prefs.pathUserAssets[0]
                    , "COPY", "Cancel"))
                {
                    GYAFile.CopyToSelected(gyaVars.Prefs.pathUserAssets[0]);

                    // Instead of refreshing, modify data directly and save
                    GYAPackage.RefreshAllCollections();
                }
            }

            // Copy assets to selectable folder
            if (obj.ToString() == "CopyToSelectableMulti")
            {
                string path = EditorUtility.SaveFolderPanel(gyaVars.abbr + " - Copy to Selected Folder", "", "");

                if (path.Length != 0)
                {
                    GYAFile.CopyToSelected(path);
                    // Do not auto refresh, assume user is copying outside known folders
                    GYAExt.Log("Make sure to 'Refresh' after you are done copying files if required.");
                }
            }

            // Copy asset to selectable folder
            if (obj.ToString() == "CopyToSelectable")
            {
                string path = EditorUtility.SaveFolderPanel(gyaVars.abbr + " - Copy to Selected Folder", "", "");
                if (path.Length != 0)
                {
                    // copy
                    var fileData = GYAFile.MoveAssetToPath(svCurrentPkg, path, true);
                    if (fileData.Key > 0)
                    {
                        GYAExt.Log("( " + fileData.Key + " ) package(s) copied:", fileData.Value);
                    }
                    // Do not auto refresh, assume user is copying outside known folders
                }
            }

            // Copy asset to User folder
            if (obj.ToString() == "CopyToUser")
            {
                // copy
                var fileData = GYAFile.MoveAssetToPath(svCurrentPkg, gyaVars.Prefs.pathUserAssets[0], true);
                if (fileData.Key > 0)
                {
                    GYAExt.Log("( " + fileData.Key + " ) package(s) copied to the User Assets Folder.", fileData.Value);
                }

                GYAPackage.RefreshUser(false);
                RefreshSV(false);
                GYAFile.SaveGYAAssets();
            }

            // Delete asset
            if (obj.ToString() == "DeleteAsset")
            {
                if (EditorUtility.DisplayDialog(gyaVars.abbr + " - Delete Selected Asset",
                    "Are you sure you want to DELETE this asset?\n\n" + svCurrentPkg.title + "\n\n" +
                    svCurrentPkg.filePath
                    , "DELETE", "Cancel"))
                {
                    // Yes, delete file
                    var fileData = GYAFile.DeleteAsset(svCurrentPkg);
                    if (fileData.Key > 0)
                    {
                        GYAExt.Log("( " + fileData.Key + " ) package(s) deleted.", fileData.Value);
                    }

                    RefreshSV(false);
                    GYAFile.SaveGYAAssets();
                }
            }

            // Delete assets multiple
            if (obj.ToString() == "DeleteAssetMultiple")
            {
                if (EditorUtility.DisplayDialog(gyaVars.abbr + " - Delete ALL Selected Assets",
                    "Are you sure you want to DELETE ALL selected assets?\n\n" +
                    GYAPackage.countMarkedToImport + " file(s) selected for deletion.\n\n" +
                    "Deleted file(s) are NOT moved to the Trash!\n"
                    , "DELETE ALL SELECTED", "Cancel"))
                {
                    GYAFile.DeleteAssetMultiple();
                }
            }

            // Show ALl isMarked
            if (obj.ToString() == "ShowAllSelected")
            {
                ddCategory = ddCategory == ddCategories.IsMarked ? ddCategories.All : ddCategories.IsMarked;

                SVPopUpCollection(svCollection.All);
                ddCategoryText = string.Empty;
                ddPublisher = 0;
                showGroup = 0;
                fldSearch = string.Empty;
            }

            // Show asset selection
            if (obj.ToString() == "AllAssets")
            {
                SVPopUpCollection(svCollection.All);
                showGroup = 0;
            }
            if (obj.ToString() == "StoreAssets")
            {
                SVPopUpCollection(svCollection.Store);
                showGroup = 0;
            }
            if (obj.ToString() == "UserAssets")
            {
                SVPopUpCollection(svCollection.User);
                showGroup = 0;
            }
            if (obj.ToString() == "StandardAssets")
            {
                SVPopUpCollection(svCollection.Standard);
                showGroup = 0;
            }
            if (obj.ToString() == "OldAssets")
            {
                SVPopUpCollection(svCollection.Old);
                showGroup = 0;
            }
            if (obj.ToString() == "PackageImportGroup")
            {
                // Verify Import Multiple
                if (EditorUtility.DisplayDialog(gyaVars.abbr + " - Import Entire Group",
                    "Are you sure you want to import this group?\n\nThis may take awhile depending on the number/size of assets in the group."
                    , "Import Group", "Cancel"))
                {
                    GYAImport.ImportMultiple(true); // true = Import entire group
                }
            }
            if (obj.ToString() == "PackageImportMultiple")
            {
                // Verify Import Multiple
                if (EditorUtility.DisplayDialog(gyaVars.abbr + " - Import Selected Assets",
                    "Are you sure you want to import selected assets?\n\nThis may take awhile depending on the number/size of assets selected."
                    , "Import Selected", "Cancel"))
                {
                    GYAImport.ImportMultiple();
                }
            }

            // RenameWithVersionSelected
            if (obj.ToString() == "RenameWithVersionSelected")
            {
                // Verify Import Multiple
                if (EditorUtility.DisplayDialog(gyaVars.abbr + " - Rename Selected Assets",
                    "This will append the package/unity version to the filename(s).\n\nNote: Only applies to actual Asset Store packages."
                    , "Rename Selected", "Cancel"))
                {
                    GYAFile.RenameWithVersionSelected();
                }
            }

            // InvertMarked
            if (obj.ToString() == "InvertMarked")
                MarkedForImportInvert();

            // ClearMarked
            if (obj.ToString() == "ClearMarked")
                MarkedForImportClear(true);

	        // ClearNotVisible
	        if (obj.ToString() == "ClearNotVisible")
		        MarkedForImportClearNotVisible();
		        
            // ClearMarkedGlobal
            if (obj.ToString() == "ClearMarkedEverything")
                MarkedForImportClearEverything(true);

            // ClearMarkedAfter toggle
            if (obj.ToString() == "ClearMarkedAfter")
            {
                gyaVars.Prefs.clearMarkedAfter = !gyaVars.Prefs.clearMarkedAfter;
                GYAFile.SaveGYAPrefs();
            }

            // ClearMarkedAfterGlobal toggle
            if (obj.ToString() == "ClearMarkedAfterGlobal")
            {
                gyaVars.Prefs.clearMarkedAfterGlobal = !gyaVars.Prefs.clearMarkedAfterGlobal;
                GYAFile.SaveGYAPrefs();
            }

            // SelectVisible
            if (obj.ToString() == "SelectVisible")
                SelectVisible();

            // SelectMultipleVersions
            if (obj.ToString() == "SelectMultipleVersions")
                SelectMultipleVersions();

            // Popup Menu

            if (obj.ToString() == "PopupDownload")
            {
                GYAPackage.Download_UAS_Asset(svCurrentPkg.id);
            }

            if (obj.ToString() == "PopupImport")
                GYAImport.ImportSingle(svCurrentPkg.filePath);

            if (obj.ToString() == "PopupImportInteractive")
                GYAImport.ImportSingle(svCurrentPkg.filePath, true);

            if (obj.ToString() == "DownloadPackage2Folder")
                GYAExt.OpenAssetURL(64829, true);

            if (obj.ToString() == "ImportPackage2Folder")
                GYAImport.ImportSingle(GYAPackage.GetAssetByID(64829).filePath, true);

            if (obj.ToString() == "PopupPackage2Folder")
            {
                string basePath = GYAExt.PathUnityProjectAssets;
                string path = GYAFile.NiceWinPath(EditorUtility.SaveFolderPanel(gyaVars.abbr + " - Import to Selected Folder", basePath, ""));
                var pathRelative = "";

                // Convert to relative path
                if (path.StartsWith(GYAFile.NiceWinPath(GYAExt.PathUnityProject), StringComparison.InvariantCultureIgnoreCase))
                {
                    pathRelative = path.Split(new[] { GYAFile.NiceWinPath(GYAExt.PathUnityProject) }, StringSplitOptions.None)[1];
                    if (!char.IsLetterOrDigit(pathRelative[0]))
                        pathRelative = pathRelative.Trim().Substring(1).TrimStart();
                }

                if (!pathRelative.StartsWith("Assets", StringComparison.InvariantCultureIgnoreCase))
                {
                    // If not cancelled then report path error
                    if (path.Length != 0)
                        GYAExt.Log("Import To Folder: Must import within the current Project:", path + "\n\t" + pathRelative);
                }
                else
                {
                    GYAReflect.PackageToFolder(svCurrentPkg.filePath, pathRelative, true);
                }
            }

            if (obj.ToString() == "AssetFolder")
                GYAExt.ShellOpenFolder(svCurrentPkg.filePath, true);

            if (obj.ToString() == "AssetURL")
                GYAExt.OpenAssetURL(svCurrentPkg.link.id, gyaVars.Prefs.openURLInUnity);

            if (obj.ToString() == "AssetURLinUnity")
                GYAExt.OpenAssetURL(svCurrentPkg.link.id, true);

            if (obj.ToString() == "PublisherURL")
            {
                //GYAExt.OpenAssetURL(svCurrentPkg.publisher.id, link); // OLD - browser only
                GYAExt.OpenPublisherURL(svCurrentPkg.publisher.id, gyaVars.Prefs.openURLInUnity);
                //GYAExt.OpenPublisherURL(svCurrentPkg.publisher.id, false);
            }

            if (obj.ToString() == "PublisherURLinUnity")
            {
                //GYAExt.OpenAssetURL(svCurrentPkg.publisher.id, link);
                GYAExt.OpenPublisherURL(svCurrentPkg.publisher.id, true);
            }

            if (obj.ToString() == "UnityDataFolder")
                GYAExt.ShellOpenFolder(GYAExt.PathUnityDataFiles);

            if (obj.ToString() == "StandardAssetsFolder")
                GYAExt.ShellOpenFolder(GYAExt.PathUnityStandardAssets);

            if (obj.ToString() == "OldAssetsFolder")
                GYAExt.ShellOpenFolder(gyaVars.pathOldAssetsFolder);

            if (obj.ToString() == "GYADataFolder")
                GYAExt.ShellOpenFolder(GYAExt.PathGYADataFiles);

            if (obj.ToString() == "UserAssetsFolder")
                GYAExt.ShellOpenFolder(gyaVars.Prefs.pathUserAssets[0]);

            if (obj.ToString() == "AssetStoreURL")
            {
                // Open the Asset Store in the default browser
                string openURL = "https://assetstore.unity.com/";

                if (GYAExt.IsOSWin)
                {
                    // Open in Unity's Asset Store Window
                    if (gyaVars.Prefs.openURLInUnity)
                        UnityEditorInternal.AssetStore.Open(null); // Unity browser
                    else
                        System.Diagnostics.Process.Start(openURL);
                }
                else
                {
                    // Open in Unity's Asset Store Window
                    if (gyaVars.Prefs.openURLInUnity)
                        UnityEditorInternal.AssetStore.Open(null); // Unity browser
                    else
                        System.Diagnostics.Process.Start("open", openURL); // Default browser
                }
            }

            if (obj.ToString() == "AssetStoreSaleURL")
            {
                // Open the Asset Store in the default browser
                string openURL = "https://assetstore.unity.com/?on_sale=true";

                if (GYAExt.IsOSWin)
                    System.Diagnostics.Process.Start(openURL);
                else
                    System.Diagnostics.Process.Start("open", openURL); // Default browser
            }

            if (obj.ToString() == "AssetStoreForumAssetsURL")
            {
                // Open the Asset Store in the default browser
                string openURL = "http://forum.unity.com/forums/assets-and-asset-store.32/";

                if (GYAExt.IsOSWin)
                    System.Diagnostics.Process.Start(openURL);
                else
                    System.Diagnostics.Process.Start("open", openURL); // Default browser
            }

            // DeleteEmptySubFolders
            if (obj.ToString() == "DeleteEmptySubFolders")
            {
                GYAFile.DeleteEmptySubFolders(GYAExt.PathUnityAssetStoreActive);
            }

            // ExportAsCSV
            if (obj.ToString() == "ExportAsCSV")
            {
                string fileName = "GYA Assets Export (All)";
                string path = EditorUtility.SaveFilePanel(gyaVars.abbr + " - Export CSV file as: " + fileName + ".csv",
                    "", fileName + ".csv", "csv");
                if (path.Length != 0)
                {
                    GYAFile.SaveAsCSV(gyaData.Assets, path);
                }
            }

            // ExportAsCSVGroup
            if (obj.ToString() == "ExportAsCSVGroup")
            {
                string fileName = "GYA Assets Export (" + gyaData.Groups[showGroup].name + ")";
                string path = EditorUtility.SaveFilePanel(gyaVars.abbr + " - Export CSV file as: " + fileName + ".csv",
                    "", fileName + ".csv", "csv");
                if (path.Length != 0)
                {
                    GYAFile.SaveAsCSVGroup(path);
                }
            }

            // ExportListAsCSV
            if (obj.ToString() == "ExportListAsCSV")
            {
                string fileName = "GYA Assets Export List";

                string path = EditorUtility.SaveFilePanel(gyaVars.abbr + " - Export CSV file as: " + fileName + ".csv",
                        "", fileName + ".csv", "csv");
                if (path.Length != 0)
                {
                    GYAFile.SaveListAsCSV(path);
                }
            }

            // Copy asset to the sorted folder
            if (obj.ToString() == "CopyToSorted")
            {
                var path = promptForSortedFolder();
                if (path != "")
                {
                    //if (!gyaVars.Prefs.isSilent)
                    GYAExt.Log("Copying file: " + svCurrentPkg.titleWithVersion);
                    // copy w/ category
                    var fileData = GYAFile.MoveAssetToPath(svCurrentPkg, path, true, false, false, true);
                    if (fileData.Key > 0)
                    {
                        GYAExt.Log("( " + fileData.Key + " ) package(s) copied to the Sorted Assets Folder.  Please 'Refresh' when done.", fileData.Value);
                    }

                    //GYAPackage.RefreshUser(true);
                    //RefreshSV(false);
                    //GYAFile.SaveGYAAssets();
                }
            }

            // Move asset to the sorted folder
            if (obj.ToString() == "MoveToSorted")
            {
                var path = promptForSortedFolder();
                if (path != "")
                {
                    //if (!gyaVars.Prefs.isSilent)
                    GYAExt.Log("Moving file: " + svCurrentPkg.titleWithVersion);
                    // move w/ category
                    var fileData = GYAFile.MoveAssetToPath(svCurrentPkg, path, false, false, false, true);
                    if (fileData.Key > 0)
                    {
                        GYAExt.Log("( " + fileData.Key + " ) package(s) moved to the Sorted Assets Folder.  Please 'Refresh' when done.", fileData.Value);
                    }

                    //GYAPackage.RefreshUser(true);
                    //RefreshSV(false);
                    //GYAFile.SaveGYAAssets();
                }
            }

            // Copy assets to selectable folder
            if (obj.ToString() == "CopyToSortedMulti")
            {
                var path = promptForSortedFolder();
                if (path != "")
                {
                    if (EditorUtility.DisplayDialog(gyaVars.abbr + " - Copy Selected Assets To " + path,
                            "Copying is NOT performed in a seperate thread at this time.\n\n" +
                            "Unity may seem to pause during the copy.\n\n" +
                            "Copy To: " + path
                            , "COPY", "Cancel"))
                    {
                        GYAFile.CopyToSelected(path, true);

                        GYAPackage.RefreshAllCollections();
                    }
                }
            }

            // Copy assets to selectable folder
            if (obj.ToString() == "MoveToSortedMulti")
            {
                var path = promptForSortedFolder();
                if (path != "")
                {
                    if (EditorUtility.DisplayDialog(gyaVars.abbr + " - Move Selected Assets To " + path,
                            "Moving is NOT performed in a seperate thread at this time.\n\n" +
                            "Unity may seem to pause during the copy.\n\n" +
                            "Move To: " + path
                            , "MOVE", "Cancel"))
                    {
                        GYAFile.CopyToSelected(path, true, true);

                        GYAPackage.RefreshAllCollections();
                    }
                }
            }

            // Open the sorted Assets folder
            if (obj.ToString() == "SortedAssetsFolder")
            {
                GYAExt.ShellOpenFolder(gyaVars.Prefs.pathSortedAssetsFolder);
            }

            infoChanged = true;
        }

        // Sorted Assets - Prompt for folder if should ask for folder or if folder has not been assigned
        internal string promptForSortedFolder()
        {
            var path = gyaVars.Prefs.pathSortedAssetsFolder;
            if (gyaVars.Prefs.alwaysAskForSortedAssetsFolder || gyaVars.Prefs.pathSortedAssetsFolder.Length == 0)
            {
                // Prompt for folder
                string pathTemp = EditorUtility.SaveFolderPanel(gyaVars.abbr + " - Select Folder to Copy/Move as Sorted Assets:", null, null);
                if (pathTemp != "")
                {
                    path = pathTemp;
                }
            }
            return path;
        }

        // Update selected assets to Show
        internal int SVPopulate()
        {
            // Show selected assets
            if (showActive == svCollection.All)
                svData = gyaData.Assets;
            if (showActive == svCollection.Store)
                svData = gyaData.Assets.FindAll(x => x.collection == svCollection.Store);
            if (showActive == svCollection.Standard)
                svData = gyaData.Assets.FindAll(x => x.collection == svCollection.Standard);
            if (showActive == svCollection.User)
                svData = gyaData.Assets.FindAll(x => x.collection == svCollection.User);
            if (showActive == svCollection.Old)
                svData = gyaData.Assets.FindAll(x => x.collection == svCollection.Old || x.isOldToMove);
	        if (showActive == svCollection.Group)
	        {
		        ddCategory = ddCategories.All;
		        svData = grpData[showGroup];
	        }

            // Dropdown Category
            if (ddCategory != ddCategories.All)
            {
                List<GYAData.Asset> tAssets = new List<GYAData.Asset>();
                switch (ddCategory)
                {
                    case ddCategories.All:
                        break;
                    case ddCategories.BuiltWithUnity2020:
	                    svData = svData.FindAll(
		                    x => x.unity_version.StartsWith("2020.", StringComparison.InvariantCultureIgnoreCase));
	                    break;
                    case ddCategories.BuiltWithUnity2019:
                        svData = svData.FindAll(
                            x => x.unity_version.StartsWith("2019.", StringComparison.InvariantCultureIgnoreCase));
                        break;
                    case ddCategories.BuiltWithUnity2018:
                        svData = svData.FindAll(
                            x => x.unity_version.StartsWith("2018.", StringComparison.InvariantCultureIgnoreCase));
                        break;
                    case ddCategories.BuiltWithUnity2017:
                        svData = svData.FindAll(
                            x => x.unity_version.StartsWith("2017.", StringComparison.InvariantCultureIgnoreCase));
                        break;
                    case ddCategories.BuiltWithUnity5:
                        svData = svData.FindAll(
                            x => x.unity_version.StartsWith("5.", StringComparison.InvariantCultureIgnoreCase));
                        break;
                    case ddCategories.BuiltWithUnity4:
                        svData = svData.FindAll(
                            x => x.unity_version.StartsWith("4.", StringComparison.InvariantCultureIgnoreCase));
                        break;
                    case ddCategories.BuiltWithUnity3:
                        svData = svData.FindAll(
                            x => x.unity_version.StartsWith("3.", StringComparison.InvariantCultureIgnoreCase));
                        break;
                    case ddCategories.BuiltWithUnityUnknown:
                        svData = svData.FindAll(x => x.unity_version.Length == 0 && !x.isExported);
                        break;
                    case ddCategories.PackageAssetStore:
                        svData = svData.FindAll(x => !x.isExported);
                        break;
                    case ddCategories.PackageExported:
                        svData = svData.FindAll(x => x.isExported);
                        break;
                    case ddCategories.Ungrouped:
                        svData = svData.FindAll(x => !x.isInAGroup);
                        break;
                    case ddCategories.Damaged:
                        svData = svData.FindAll(x => x.isDamaged);
                        svData.OrderByDescending(x => x.titleWithVersion);
                        break;
                    case ddCategories.HasNotes:
                        svData = svData.FindAll(x => !string.IsNullOrEmpty(x.AssetInfo.Notes));
                        break;
                    case ddCategories.Deprecated:
                        svData = svData.FindAll(x => x.isDeprecated);
                        break;
                    case ddCategories.IsMarked:
                        svData = svData.FindAll(x => x.isMarked);
                        break;
                    case ddCategories.NotInPurchased:
                        svData = svData.FindAll(x => !x.isInPurchasedList && !x.isExported);
                        break;
                    case ddCategories.NotDownloaded:
                        SVPopUpCollection(svCollection.All);
                        showGroup = 0;

                        // ASPurchased asset NOT DOWNLOADED so entry is missing in gyaData.Assets, add it to svData
                        foreach (var asPurc in GYA.asPurchased.results)
                        {
                            int tCount = tAssets.Count;
                            var purc = asPurc;
                            var vTmp = GYA.gyaData.Assets.Find(x => x.id == purc.id);
                            if (vTmp == null) // asset not found
                            {
                                tAssets.Add(new GYAData.Asset());
                                tAssets[tCount].id = asPurc.id;
                                tAssets[tCount].title = asPurc.name;
                                tAssets[tCount].titleWithVersion = asPurc.name;
                                tAssets[tCount].category.id = asPurc.category.id;
                                tAssets[tCount].category.label = asPurc.category.name;
                                tAssets[tCount].publisher.id = asPurc.publisher.id;
                                tAssets[tCount].publisher.label = asPurc.publisher.name;
                                tAssets[tCount].link.id = asPurc.id;
                                tAssets[tCount].link.type = asPurc.type;
                                tAssets[tCount].pubdate = "";
                                tAssets[tCount].filePath = "Not_Downloaded_ID_" + asPurc.id;
                                tAssets[tCount].isDeprecated = (asPurc.status.ToLower() == "deprecated"); // Is deprecated
                                tAssets[tCount].icon = asPurc.icon; // Icon link
                                tAssets[tCount].notDownloaded = true;

                                tAssets[tCount].isSameVersionAsUnity = false;
                                tAssets[tCount].isFavorite = false; //GroupContainsAsset(0, svData[i]);
                                tAssets[tCount].isVersionAppended = false;
                            }
                        }

                        // Sort
                        tAssets = tAssets
                            .OrderByDescending(x => x.isDamaged && x.title.StartsWith("unknown", StringComparison.InvariantCultureIgnoreCase))
                            .ThenByDescending(x => x.collection == svCollection.Project)
                            .ThenBy(x => RemoveLeading(x.title)).ThenByDescending(x => x.version_id)
                            .ThenBy(x => x.collection).ToList();

                        svData = tAssets;
                        break;
                    case ddCategories.MissingFiles:
                        svData = svData.FindAll(x => x.isFileMissing);
                        break;
                    case ddCategories.MultiVersion:
                        SVPopUpCollection(svCollection.All);
                        showGroup = 0;

                        // 1st Sort - Special / Title / vID / collection
                        gyaData.Assets = gyaData.Assets
                            .OrderByDescending(x => x.isDamaged && x.title.StartsWith("unknown", StringComparison.InvariantCultureIgnoreCase))
                            .ThenByDescending(x => x.collection == svCollection.Project)
                            .ThenBy(x => RemoveLeading(x.title))
                            .ThenByDescending(x => x.version_id)
                            .ThenBy(x => x.collection)
                            .ToList();

                        // Copy damaged/project to tAssets

                        // 2nd Sort - Group by ID maintaining vID
                        IEnumerable<GYAData.Asset> sortedVersionsTmp =
                            from package in gyaData.Assets
                            where (package.collection != svCollection.Project)
                            orderby package.id, package.version_id descending
                            group package by package.id
                            into grouped
                            from package in grouped
                            where (grouped.Count() > 1 && package.id > 0)
                            select package;
                        var sortedVersions = sortedVersionsTmp.ToList();

                        // Build complete list
                        foreach (var g in gyaData.Assets)
                        {
                            if (!tAssets.Contains(g)) // if (s)gyaData.Assets does NOT exist in tAssets
                            {
                                if (sortedVersions.Contains(g)) // if exist in sortedVersions
                                {
                                    var sTmp = sortedVersions.FindAll(x => x.id == g.id);
                                    if (g.version_id == sTmp[0].version_id) // version_id's match
                                    {
                                        foreach (var s in sTmp) // add entries
                                        {
                                            if (s.version_id != sTmp[0].version_id)
                                            {
                                                // version_id's don't match
                                                s.isLatestVersion = false; // Mark latest version asset
                                            }
                                            tAssets.Add(s);
                                        }
                                    }
                                }
                            }
                        }
                        svData = tAssets;
                        break;
                    case ddCategories.UserSelection:
                        svData = svData.FindAll(x => (x.category.label).Replace('/', '\\').Replace('&', '+')
                            .StartsWith(ddCategoryText, StringComparison.InvariantCultureIgnoreCase));
                        break;
                    default:
                        break;
                }
            }

            // Dropdown Publisher
            if (ddPublisher != 0)
                svData = svData.FindAll(x => x.publisher.id == ddPublisher);

            // Replaces SVGetTitleText .. OR should seperate from sort?
            // Figure out how many items are returned if searching, all if not
            if (searchActive == svSearchBy.Title)
                svData = svData.FindAll(x => x.titleWithVersion.Contains(fldSearch, StringComparison.OrdinalIgnoreCase));
            if (searchActive == svSearchBy.Category)
                svData = svData.FindAll(x => x.category.label.Contains(fldSearch, StringComparison.OrdinalIgnoreCase));
            if (searchActive == svSearchBy.Publisher)
                svData = svData.FindAll(x => x.publisher.label.Contains(fldSearch, StringComparison.OrdinalIgnoreCase));
            if (searchActive == svSearchBy.UserNotes)
                svData = svData.FindAll(x => x.AssetInfo.Notes.Contains(fldSearch, StringComparison.OrdinalIgnoreCase));

            svMain.headerCount = SVGetHeaderCount();

            return svData.Count;
        }

        // --

        // SelectVisible - Selects/Marks all visible assets in the list
        internal void SelectVisible()
        {
            // Handle differences for searchActive
            if (searchActive == svSearchBy.Title)
            {
                svData.FindAll(x => x.titleWithVersion.Contains(fldSearch, StringComparison.OrdinalIgnoreCase))
                    .ForEach(x => x.isMarked = true);
            }
            if (searchActive == svSearchBy.Category)
            {
                svData.FindAll(x => x.category.label.Contains(fldSearch, StringComparison.OrdinalIgnoreCase))
                    .ForEach(x => x.isMarked = true);
            }
            if (searchActive == svSearchBy.Publisher)
            {
                svData.FindAll(x => x.publisher.label.Contains(fldSearch, StringComparison.OrdinalIgnoreCase))
                    .ForEach(x => x.isMarked = true);
            }
            if (searchActive == svSearchBy.UserNotes)
            {
                svData.FindAll(x => x.AssetInfo.Notes.Contains(fldSearch, StringComparison.OrdinalIgnoreCase))
                    .ForEach(x => x.isMarked = true);
            }
        }

        // Select multiple versions of all assets, excepting the newest version
        internal void SelectMultipleVersions()
        {
            svData.FindAll(x => x.isLatestVersion == false).ForEach(x => x.isMarked = true);
        }

        // Invert isMarked
        internal void MarkedForImportInvert()
        {
            svData.ForEach(x => x.isMarked = !x.isMarked);
        }

        // Clear isMarked from current view
        internal void MarkedForImportClear(bool forceClear = false)
        {
            if (!forceClear && gyaVars.Prefs.clearMarkedAfterGlobal) // Global overrides Visible - fixed missing !forceClear
                MarkedForImportClearEverything(forceClear);

            else if (forceClear || gyaVars.Prefs.clearMarkedAfter)
                svData.ForEach(x => x.isMarked = false);
        }
        
        
        
        
	    // ClearNotVisible
	    // Clear isMarked from entire list except current view
	    internal void MarkedForImportClearNotVisible()
	    {
	    	// WIP - ref GroupUpdatePkgData
	    	
	    	//// clear global
	    	//gyaData.Assets.ForEach(x => x.isMarked = false);
	    	//// clear visible
	    	//svData.ForEach(x => x.isMarked = false);
	    	

	    	
		    //List<GYAData.Asset> gyaMarked = gyaData.Assets.FindAll(x => x.isInAGroup && x.isMarked); // Removes only from current group - pt 1
		    List<GYAData.Asset> gyaMarked = gyaData.Assets.FindAll(x => x.isMarked); // Removes only from current group - pt 1
		    List<GYAData.Asset> svMarked = svData.FindAll(x => x.isMarked); // Removes only from current group - pt 1
		    
		    GYAExt.LogAsJson(gyaMarked.Count());
		    GYAExt.LogAsJson(svMarked.Count());

		    
		    //foreach (GYAData.Asset package in gyaData.Assets) // Removes from all groups, needs adjustment
	        
		    foreach (GYAData.Asset svItem in svMarked) // Removes only from current group - pt 2
		    {
		    	
			    for (int i = 0; i < gyaMarked.Count; ++i)
			    {
			    	
		    		// Check if item exists in main list
				    if (svItem.isMarked)
				    {
				    	
				    }
			    

			    }
			    
			    //if (t.isExported)
				//    GroupRemoveAsset(t.filePath, false);
			    //else
				//    GroupRemoveAsset(t.id, false);
		    }



	    	
		    //MarkedForImportClear();
		    //infoChanged = true;

		    //RefreshSV(true);
		    //GYAFile.SaveGYAGroups();
	    }




        // Clear isMarked from master list
        internal void MarkedForImportClearEverything(bool forceClear = false)
        {
            //SVPopUpCollection(svCollection.All);
            //ddCategoryText = string.Empty;
            //ddPublisher = 0;
            //showGroup = 0;
            //fldSearch = string.Empty;

            ////showActive = svCollection.All;
            ////SVPopUpCollection(svCollection.All);
            ////SVPopUpCollection(showActive);
            //SVPopulate();
            ////RefreshSV();
            //svData.ForEach(x => x.isMarked = false);
            if (forceClear || gyaVars.Prefs.clearMarkedAfterGlobal)
                gyaData.Assets.ForEach(x => x.isMarked = false);
        }

        // svCollection.Group
        internal bool IsActiveGroup(int grpID)
        {
            return (showActive == svCollection.Group && showGroup == grpID);
        }

        // Create grpData Package List, for display, from gyaData info
        // pSort true = sort by sortActive value
        internal void GroupUpdatePkgData(bool pSort = true)
        {
            var tAssets = gyaData.Assets;

            // Reset isInAGroupLockedVersion to False
            tAssets.ForEach(x => x.isInAGroupLockedVersion = false);

            // Catch invalid data
            if (grpData != null)
                grpData.Clear();
            else
            {
                ErrorStateSet(ErrorCode.ErrorStep2);
                return;
            }

            // Process each group
            for (int i = 0; i < gyaData.Groups.Count; ++i)
            {
                // Add group if not exist
                if (!grpData.ContainsKey(i))
                    grpData.Add(i, new List<GYAData.Asset>());

                // Populate grpData from gyaData.Assets
                foreach (GYAData.GroupAsset t in gyaData.Groups[i].Assets)
                {
                    GYAData.GroupAsset curAsset = t;

                    // Find pkg in Assets & Mark isInAGroup
                    List<GYAData.Asset> pkgResults;
                    if (!curAsset.isExported) // AS Package (find via id)
                    {
                        tAssets.FindAll(x => x.id == curAsset.id).ForEach(x => x.isInAGroup = true);
                        tAssets.Sort((x, y) => -x.version_id.CompareTo(y.version_id));
                        pkgResults = tAssets.FindAll(x => x.id == curAsset.id);
                    }
                    else // Exported Package (find via filePath)
                    {
                        tAssets.FindAll(x => x.filePath == curAsset.filePath).ForEach(x => x.isInAGroup = true);
                        pkgResults = tAssets.FindAll(x => x.filePath == curAsset.filePath);
                    }

                    if (!curAsset.useLatestVersion)
                    {
                        tAssets.FindAll(x => x.id == curAsset.id && x.version_id == curAsset.version_id)
                            .ForEach(x => x.isInAGroupLockedVersion = true);
                    }

                    // Mark as MISSING & Manually add entry to show in list - Asset Is in Group BUT NOT in Assets List
                    if (!pkgResults.Any())
                    {
	                    t.isFileMissing = true;
	                    t.group_ID = i;
	                    
                        pkgResults.Add(new GYAData.Asset());
                        pkgResults[pkgResults.Count - 1].id = curAsset.id;
                        pkgResults[pkgResults.Count - 1].title = curAsset.title;
                        pkgResults[pkgResults.Count - 1].titleWithVersion = curAsset.title;
                        pkgResults[pkgResults.Count - 1].isExported = curAsset.isExported;

                        pkgResults[pkgResults.Count - 1].collection =
                            curAsset.isExported ? svCollection.User : svCollection.Store;

                        pkgResults[pkgResults.Count - 1].isFileMissing = curAsset.isFileMissing;
                        pkgResults[pkgResults.Count - 1].link.id = curAsset.id;
                        pkgResults[pkgResults.Count - 1].pubdate = "";

                        pkgResults[pkgResults.Count - 1].version_id = curAsset.version_id;
                        pkgResults[pkgResults.Count - 1].filePath = curAsset.filePath;
                    }
                    else // Asset found, process as needed
                    {
                        t.isFileMissing = false;
	                    t.group_ID = i;

                        if (curAsset.useLatestVersion) // *** .isInAGroupLockedVersion
                        {
                            pkgResults = pkgResults.OrderByDescending(x => x.collection == svCollection.Store)
                                .ThenByDescending(x => x.version_id).ToList();
                        }
                        else
                        {
                            pkgResults =
                                pkgResults.FindAll(x => x.version_id == curAsset.version_id &&
                                                        x.collection == svCollection.User);
                        }
                    }
                    grpData[i].Add(pkgResults[0]);
                }
            }

            if (pSort)
                PackagesSortBy(sortActive);
        }

        // Check if Group already contains a given asset
        internal bool GroupContainsAsset(int grpID, GYAData.Asset pItem) // LAG
        {
            if (pItem == null)
            {
                ErrorStateSet(ErrorCode.ErrorStep2);
                return false;
            }

            if (pItem.isExported)
            {
                // No asset Id, use filepath
                return gyaData.Groups[grpID].Assets.Any(x => x.filePath == pItem.filePath); // 
            }
            else
            {
                // IS Asset Store ID, use asset ID
                return gyaData.Groups[grpID].Assets.Any(x => x.id == pItem.id);
            }
        }

        internal bool AssetIDExist(int assetID)
        {
            int results = 0;

            results = GYA.gyaData.Assets.FindAll(x => x.id == assetID).Count;

            // If found return true
            if (results > 0)
                return true;

            return false;
        }

        internal bool GroupContainsAsset(int grpID, GYAData.GroupAsset aItem)
        {
            int results = 0;

            if (aItem.isExported)
                // No asset Id, use filepath
                results = gyaData.Groups[grpID].Assets.FindAll(x => x.filePath == aItem.filePath).Count;
            else
                // IS Asset Store ID, use asset ID
                results = gyaData.Groups[grpID].Assets.FindAll(x => x.id == aItem.id).Count;

            // If found return true
            return results > 0;
        }

        // Create New Group
        internal void GroupCreate(string grpName)
        {
            List<GYAData.Group> grpTemp = new List<GYAData.Group>
            {
                // Create Group without Asset
                new GYAData.Group {name = grpName, Assets = new List<GYAData.GroupAsset> { }}
            };
            gyaData.Groups.Add(grpTemp[0]);

            GroupUpdatePkgData(false);
            GYAFile.SaveGYAGroups();
        }

        // Group Popup Remove from
        internal void GroupRemoveAsset(object obj)
        {
            GroupRemoveAsset(true);
        }

        internal void GroupRemoveAsset(bool pUseSVCurrentPkg)
        {
            //GroupRemoveAsset (0, null, pUseSVCurrentPkg, false);
            GroupRemoveAsset(0, null, pUseSVCurrentPkg, true);
        }

        internal void GroupRemoveAsset(int pID, bool refreshWhenDone = true)
        {
            GroupRemoveAsset(pID, null, false, refreshWhenDone);
        }

        internal void GroupRemoveAsset(string pFilePath, bool refreshWhenDone = true)
        {
            GroupRemoveAsset(0, pFilePath, false, refreshWhenDone);
        }

        internal void GroupRemoveAsset(int pID, string pFilePath, bool pUseSVCurrentPkg, bool refreshWhenDone = true)
        {
            int tID = pID;
            string tFilePath = pFilePath;

            if (pUseSVCurrentPkg)
            {
                tID = svCurrentPkg.id;
                tFilePath = svCurrentPkg.filePath;
            }

            if (tID == 0) // Exported
            {
                gyaData.Groups[showGroup].Assets.RemoveAll(x => x.filePath == tFilePath);
                if (showGroup == 0)
                    gyaData.Assets.FindAll(x => x.filePath == tFilePath).ForEach(x => x.isFavorite = false);
            }
            else // AS
            {
                gyaData.Groups[showGroup].Assets.RemoveAll(x => x.id == tID);
                if (showGroup == 0)
                {
                    gyaData.Assets.FindAll(x => x.id == tID).ForEach(x => x.isFavorite = false);
                }
            }

            if (refreshWhenDone)
            {
                RefreshSV(true);
                GYAFile.SaveGYAGroups();
                //infoChanged = true;
            }
        }

	    internal void GroupRemoveAssetFromGroupID(int gID, int pID, string pFilePath)
	    {
		    if (pID == 0) // Exported
		    {
			    gyaData.Groups[gID].Assets.RemoveAll(x => x.filePath == pFilePath);
			    if (showGroup == 0)
				    gyaData.Assets.FindAll(x => x.filePath == pFilePath).ForEach(x => x.isFavorite = false);
		    }
		    else // AS
		    {
			    gyaData.Groups[gID].Assets.RemoveAll(x => x.id == pID);
			    if (showGroup == 0)
			    {
				    gyaData.Assets.FindAll(x => x.id == pID).ForEach(x => x.isFavorite = false);
			    }
		    }
	    }
	    
	    internal void GroupRemoveSelectedAssetsFromAllGroups()
	    {
		    // cycle thru each found asset 
		    List<GYAData.Asset> itemSource = gyaData.Assets.FindAll(x => x.isInAGroup && x.isMarked);
		    //List<GYAData.Asset> itemSource = gyaData.Assets.FindAll(x => x.isMarked);
		    foreach (GYAData.Asset t in itemSource)
		    {
			    // Cycle thru each group
			    for (int i = 0; i < gyaData.Groups.Count; ++i)
			    {
			    	//// cycle thru each group asset and compare
				    //foreach (GYAData.GroupAsset g in gyaData.Groups[i].Assets)
				    //{
				    	
				    //}
				    	
				    GroupRemoveAssetFromGroupID(i, t.id, t.filePath);

			    }

		    }
	    }

        internal void GroupRemoveAssetMultiple(object obj)
        {
            GroupRemoveAssetMultiple();
        }

	    // Does NOT remove from all groups, just current
	    internal void GroupRemoveAssetMultiple()
        {
	        List<GYAData.Asset> itemSource = svData.FindAll(x => x.isMarked); // Removes only from current group - pt 1

	        //foreach (GYAData.Asset package in gyaData.Assets) // Removes from all groups, needs adjustment
	        
	        foreach (GYAData.Asset t in itemSource) // Removes only from current group - pt 2
            {
                if (t.isExported)
                    GroupRemoveAsset(t.filePath, false);
                else
                    GroupRemoveAsset(t.id, false);
            }

            MarkedForImportClear();
            infoChanged = true;

            RefreshSV(true);
            GYAFile.SaveGYAGroups();
        }

	    // Test - Try to get it to remove from all groups
	    // GroupRemoveAsset uses showGroup so it ONLY removes from current group
	    internal void GroupRemoveAssetMultiple_TEST()
	    {
		    //List<GYAData.Asset> itemSource = gyaData.Assets.FindAll(x => x.isInAGroup && x.isMarked); // Removes only from current group - pt 1
		    //foreach (GYAData.Asset t in itemSource) // Remove from all groups via master list
		    //{
			//    if (t.isExported)
			//	    GroupRemoveAsset(t.filePath, false);
			//    else
			//	    GroupRemoveAsset(t.id, false);
		    //}

		    GroupRemoveSelectedAssetsFromAllGroups();

		    MarkedForImportClear(true);
		    infoChanged = true;

		    RefreshSV(true);
		    GYAFile.SaveGYAGroups();
	    }

        internal void GroupAssignVersion(object obj)
        {
            List<GYAData.Asset> tVersions = new List<GYAData.Asset>();
            var tVersionID = (int)obj;

            if (tVersionID == 0)
            {
                // Find latest version
                tVersions = gyaData.Assets.FindAll(x => x.id == svCurrentPkg.id && !x.isExported);
                tVersions.Sort((x, y) => -x.version_id.CompareTo(y.version_id));
            }
            else
            {
                // Find requested version_id
                tVersions = gyaData.Assets.FindAll(x => x.id == svCurrentPkg.id && x.version_id == tVersionID &&
                                                        !x.isExported && x.collection == svCollection.User);
            }

            if (tVersions.Count > 0)
            {
                foreach (GYAData.GroupAsset gAsset in gyaData.Groups[showGroup].Assets
                    .Where(x => x.id == svCurrentPkg.id))
                {
                    gAsset.useLatestVersion = (tVersionID == 0);
                    gAsset.version_id = tVersionID;
                    gAsset.filePath = tVersions[0].filePath;
                }

                RefreshSV();
                GYAFile.SaveGYAGroups();
            }
            else
            {
                GYAExt.LogWarning("GroupAssignVersion: Failed to assign asset version for: " + tVersions[0].title);
            }
        }

        // Group Popup Add to
        internal void GroupAddTo(object obj)
        {
            GroupAddTo((int)obj, false, null);
        }

        internal void GroupAddTo(int grpID, bool batch = false, GYAData.Asset pkgTemp = null)
        {
            if (pkgTemp == null)
                pkgTemp = svCurrentPkg;

            bool tmpUseLatest = true;
            int tmpVersionID = 0;
            string tmpTitle = String.Empty;

            // Adds a complete group
            // tmpUselatest version, version_id doesn't matter in this case else Use specific version
            tmpVersionID = tmpUseLatest ? 0 : pkgTemp.version_id;

            tmpTitle = pkgTemp.isExported ? Path.GetFileNameWithoutExtension(pkgTemp.filePath) : pkgTemp.title;

            List<GYAData.GroupAsset> grpAssets = new List<GYAData.GroupAsset>
            {
                new GYAData.GroupAsset
                {
                    title = tmpTitle,
                    isExported = pkgTemp.isExported,
                    id = pkgTemp.id,
                    useLatestVersion = tmpUseLatest,
                    version_id = tmpVersionID,
                    filePath = pkgTemp.filePath
                }
            };

            // If asset already in group then bypass
            if (GroupContainsAsset(grpID, grpAssets[0]))
            {
                return;
            }

            // If asset already in group then bypass
            if (pkgTemp.collection == svCollection.Project)
            {
                GYAExt.Log("Local Project files cannot be added to a group: " + gyaData.Groups[grpID].name +
                           " - Title: " + pkgTemp.title);
                return;
            }

            // Add to group
            gyaData.Groups[grpID].Assets.Add(grpAssets[0]);

            // Don't process if running in batch mode (multiple selections)
            if (!batch)
            {
                // Add to in-memory grpData List
                RefreshSV();
                GYAFile.SaveGYAGroups();
            }
        }

        // Group Popup Add to
        internal void GroupAddToMultiple(object obj)
        {
            GroupAddToMultiple((int)obj);
        }

        internal void GroupAddToMultiple(int grpID)
        {
            foreach (GYAData.Asset package in gyaData.Assets)
            {
                if (package.isMarked)
                    GroupAddTo(grpID, true, package);
            }

            // Add to in-memory grpData List
            RefreshSV();
            GYAFile.SaveGYAGroups();
            MarkedForImportClear();
        }

        // Group Popup Move to
        internal void GroupMoveTo(object obj)
        {
            GroupMoveTo((int)obj);
        }

        internal void GroupMoveTo(int grpID)
        {
            //GroupAddToMultiple(grpID);
            GroupAddTo(grpID);
            GroupRemoveAsset(svCurrentPkg);

        }

        internal void GroupMoveToMultiple(object obj)
        {
            GroupMoveToMultiple((int)obj);
        }

	    //original - moves all selections BUT only removes from current group
	    internal void GroupMoveToMultiple_ORIGINAL(int grpID)
	    {
            //GroupAddToMultiple(grpID);
		    foreach (GYAData.Asset package in gyaData.Assets) // Add from all groups via master list
            {
                if (package.isMarked)
                    GroupAddTo(grpID, true, package);
            }

            GroupRemoveAssetMultiple();
        }

	    // test - move AND remove from current group only
	    internal void GroupMoveToMultiple(int grpID)
	    {
		    List<GYAData.Asset> itemSource = svData.FindAll(x => x.isMarked); // Removes only from current group - pt 1
		    //List<GYAData.Asset> itemSource = gyaData.Assets.FindAll(x => x.isMarked); // Removes only from current group - pt 1

		    //foreach (GYAData.Asset package in gyaData.Assets) // Removes from all groups, needs adjustment
	        
		    foreach (GYAData.Asset package in itemSource) // Removes only from current group - pt 2
		    {
			    if (package.isMarked)
				    GroupAddTo(grpID, true, package);
		    }

		    GroupRemoveAssetMultiple();
	    }

        // --- User managed asset management per project


        // ProjectAddAsset
        internal void ProjectAddAsset(object obj)
        {
            //ProjectAddAsset((int)obj, false, null);
            ProjectAddAsset(false, null);
        }

        internal void ProjectAddAsset(bool batch = false, GYAData.Asset pkgTemp = null)
        {
            if (pkgTemp == null)
                pkgTemp = svCurrentPkg;

            bool tmpUseLatest = true;
            int tmpVersionID = 0;
            string tmpTitle = String.Empty;

            // Adds a complete group
            // tmpUselatest version, version_id doesn't matter in this case else Use specific version
            tmpVersionID = tmpUseLatest ? 0 : pkgTemp.version_id;

            tmpTitle = pkgTemp.isExported ? Path.GetFileNameWithoutExtension(pkgTemp.filePath) : pkgTemp.title;

            List<GYAData.GroupAsset> grpAssets = new List<GYAData.GroupAsset>
            {
                new GYAData.GroupAsset
                {
                title = tmpTitle,
                isExported = pkgTemp.isExported,
                id = pkgTemp.id,
                useLatestVersion = tmpUseLatest,
                version_id = tmpVersionID,
                filePath = pkgTemp.filePath
                }
            };

            // If asset already in group then bypass
            if (ProjectContainsAsset(grpAssets[0]))
            {
                return;
            }

            // If asset already in group then bypass
            if (pkgTemp.collection == svCollection.Project)
            {
                GYAExt.Log("Local Project files cannot be added to the project: - Title: " + pkgTemp.title);
                return;
            }

            // Add to group
            //gyaData.Groups[grpID].Assets.Add(grpAssets[0]);

            // Don't process if running in batch mode (multiple selections)
            if (!batch)
            {
                // Add to in-memory grpData List
                RefreshSV();
                GYAFile.SaveGYAGroups();
            }
        }

        // ProjectRemoveAsset
        internal void ProjectRemoveAsset(object obj)
        {

        }


        // ProjectAddAssetMultiple
        internal void ProjectAddAssetMultiple(object obj)
        {
            foreach (GYAData.Asset package in gyaData.Assets)
            {
                if (package.isMarked)
                    ProjectAddAsset(true, package);
            }

            // Add to in-memory grpData List
            RefreshSV();

            //GYAFile.SaveGYAGroups();

            MarkedForImportClear();
        }

        // ProjectRemoveAssetMultiple
        internal void ProjectRemoveAssetMultiple(object obj)
        {

        }

        // ProjectContainsAsset
        internal bool ProjectContainsAsset(object obj)
        {
            return true;
        }

        // ---



        internal void GroupDelete(int grpID)
        {
            // Do not allow deleting index 0 (Favorites)
            if (grpID != 0)
            {
                gyaData.Groups.RemoveAt(grpID);

                // Reset scrollview
                if (showGroup > gyaData.Groups.Count - 1)
                    showGroup = gyaData.Groups.Count - 1;
                SVPopUpCollection(svCollection.Group);

                RefreshSV();
                GYAFile.SaveGYAGroups();
            }
        }

        // Set which assets to show along with any pre-processing
        internal void SVPopUpCollection(svCollection svType, string showCount = "")
        {
            showActive = svType;

            // If multi/not downloaded and not svAll, switch back to catAll
            if (showActive != svCollection.All && (ddCategory == ddCategories.MultiVersion ||
                                                   ddCategory == ddCategories.NotDownloaded))
            {
                ddCategory = ddCategories.All;
            }

            if (showCount != "")
                showCount = "" + svData.Count + " / ";

            // Show asset selection
            if (showActive == svCollection.All)
                activeCollectionText = showActive + " ( " + showCount + gyaVars.FilesCount.all + " ) ";
            if (showActive == svCollection.Store)
                activeCollectionText = showActive + " ( " + showCount + gyaVars.FilesCount.store + " ) ";
            if (showActive == svCollection.User)
                activeCollectionText = showActive + " ( " + showCount + gyaVars.FilesCount.user + " )";
            if (showActive == svCollection.Standard)
                activeCollectionText = showActive + " ( " + showCount + gyaVars.FilesCount.standard + " )";
            if (showActive == svCollection.Old)
            {
                activeCollectionText = showActive + " ( " + showCount + gyaVars.FilesCount.old;
                if (gyaVars.FilesCount.oldToMove > 0)
                    activeCollectionText += " - " + gyaVars.FilesCount.oldToMove;
                activeCollectionText += " )";
            }
            if (showActive == svCollection.Group)
            {
                // Make sure that showGroup is never more then the count-1
                if (showGroup > gyaData.Groups.Count - 1)
                    showGroup = gyaData.Groups.Count - 1;

                activeCollectionText = showGroup + " - " + gyaData.Groups[showGroup].name;
            }
        }

        // Return Header text
        internal string SVGetHeaderText(int i)
        {
            string headerText = String.Empty;

            // Title header text, reduce to first char of string
            if (sortActive == svSortBy.Title || sortActive == svSortBy.TitleNestedVersions)
            {
                if (svData[i].collection == svCollection.Project)
                    headerText = "- Local Project -";
                else
                {
                    if (svData[i].isDamaged && svData[i].title == "unknown")
                        headerText = "- Unknown Asset Title -";
                    else if (svData[i].title != null)
                    {
                        // Selected Assets Header
                        if (ddCategory == ddCategories.IsMarked)
                        {
                            headerText = "Show All Selected";
                        }
                        else
                        {
                            // Default Title header
                            headerText = RemoveLeading(svData[i].titleWithVersion)[0].ToString();

                            // If same ID as last asset, use its title header
                            var tItem = svData.Find(x => x.id == svData[i].id);
                            if (!svData[i].isExported && i > 0 && tItem.id == svData[i].id)
                            {
                                headerText = RemoveLeading(tItem.titleWithVersion)[0].ToString();
                            }
                        }
                    }
                }
            }

            if (sortActive == svSortBy.Category)
                headerText = svData[i].category.label.Split('/')[0];
            if (sortActive == svSortBy.CategorySub)
                headerText = svData[i].category.label;
            if (sortActive == svSortBy.Publisher)
                headerText = svData[i].publisher.label;
            if (sortActive == svSortBy.Size)
                headerText = GYAExt.GetByteRangeHeader(svData[i].fileSize);
            if (sortActive == svSortBy.DateFile)
                headerText = "Most Recent by Date (File Creation)";
            if (sortActive == svSortBy.DateBuild)
                headerText = "Most Recent by Date (Build)";
            if (sortActive == svSortBy.DatePublish)
                headerText = "Most Recent by Date (Publish)";
            if (sortActive == svSortBy.DatePurchased)
            {
                // Check if valid Date is present, 0001 = MinVal
                if (svData[i].datePurchased.ToString("yyyy") == "0001")
                    headerText = "No Data Avail for Date (Purchased)";
                else
                    headerText = "Most Recent by Date (Purchased)*";
            }
            if (sortActive == svSortBy.DateCreated)
            {
                // Check if valid Date is present, 0001 = MinVal
                if (svData[i].dateCreated.ToString("yyyy") == "0001")
                    headerText = "No Data Avail for Date (Created)";
                else
                    headerText = "Most Recent by Date (Created)*";
            }
            if (sortActive == svSortBy.DateUpdated)
            {
                // Check if valid Date is present, 0001 = MinVal
                if (svData[i].dateUpdated.ToString("yyyy") == "0001")
                    headerText = "No Data Avail for Date (Updated)";
                else
                    headerText = "Most Recent by Date (Updated)*";
            }

            if (sortActive == svSortBy.PackageID)
                headerText = "Most Recent by Package ID";
            if (sortActive == svSortBy.VersionID)
                headerText = "Most Recent by Version ID";
            if (sortActive == svSortBy.UploadID)
                headerText = "Most Recent by Upload ID";

            if (sortActive != svSortBy.Category && sortActive != svSortBy.CategorySub &&
                sortActive != svSortBy.Publisher)
                return headerText.ToUpper();
            if (headerText.Length == 0)
                headerText = "Exported Packages";

            return headerText.ToUpper();
        }

        // Count optional lines/headers to draw in the list: Categories, etc
        internal int SVGetHeaderCount()
        {
            svMain.headerLine.Clear();

            string headerText = String.Empty;
            string headerLast = String.Empty;
            bool forceHeaders = false; // Force headers to true if not sortign by title
            int headerCount = 0; // Count optional lines to draw in the list: Categories, etc

            // Force Headers if required
            if (sortActive != svSortBy.Title && sortActive != svSortBy.TitleNestedVersions)
                forceHeaders = true;

            if (gyaVars.Prefs.enableHeaders || forceHeaders)
            {
                // Pre-calc extra list height to account for showing headers
                for (int i = 0; i < svData.Count; i++)
                {
                    // Title header text, reduce to first char of string
                    headerText = SVGetHeaderText(i);

                    if (headerLast != headerText)
                    {
                        headerLast = headerText;
                        headerCount += 1;

                        svMain.headerLine.Add(new SVHeaderLine { hRow = i, hText = headerText });
                    }
                }
            }

            svMain.headerCount = headerCount;
            return headerCount;
        }

        // Draw SV Area - 3.16.8.1201 last ver before SVDraw re-write
        internal void SVDraw(int infoHeight)
        {
            bool enableLineNumbers = false;
            int pkgIDLast = 0;

#if UNITY_2019_GUI_TWEAKS
            wTop -= 1; // Move to next row
            svMain.lineHeight = 16;
#else
            svMain.lineHeight = 16;
#endif

            if (!gyaVars.Prefs.enableCollectionTypeIcons)
            {
                svMain.toggle = new Rect(0, 0, 18, svMain.lineHeight);
                svMain.button = new Rect(18, 0, position.width - 18, svMain.lineHeight);
            }
            else
            {
                svMain.toggle = new Rect(0, 0, 36, svMain.lineHeight);
                svMain.button = new Rect(36, 0, position.width - 36, svMain.lineHeight);
            }

            // -- SV Vars

#if UNITY_2019_GUI_TWEAKS
            // Rect for the viewable frame of the scrollview
            svMain.frame = new Rect(0, (wTop + controlHeight), position.width, svMain.height);
            GUI.Box(svMain.frame, "", svStyle.d);

            // Calculate visible line count within svMain.frame
            var svVisibleCount = (int)(svMain.frame.height / svMain.lineHeight) + 1; // +1 to account for header scroll
            int firstIndex = (int)(svMain.position.y / svMain.lineHeight);
            firstIndex = Mathf.Clamp(firstIndex, 0, Mathf.Max(0, svData.Count - svVisibleCount));
            int lastIndex = Mathf.Min(svData.Count, firstIndex + svVisibleCount);
            // Headers are injected on the fly
            int svHeadersVisible = svMain.headerLine.FindAll(x => x.hRow >= firstIndex && x.hRow <= lastIndex).Count; // Header Counts
            int svHeadersVisibleHeight = (int)svMain.lineHeight * svHeadersVisible; // Header height visible

            svMain.list = new Rect(0, 0, position.width - 15, (svMain.lineHeight * svData.Count) + svHeadersVisibleHeight);
#else
            // Rect for the viewable frame of the scrollview
            svMain.frame = new Rect(0, (wTop + controlHeight), position.width, svMain.height);
            GUI.Box(svMain.frame, "", svStyle.d);

            // Calculate visible line count within svMain.frame
            var svVisibleCount = (int)(svMain.frame.height / svMain.lineHeight) + 1; // +1 to account for header scroll
            int firstIndex = (int)(svMain.position.y / svMain.lineHeight);
            firstIndex = Mathf.Clamp(firstIndex, 0, Mathf.Max(0, svData.Count - svVisibleCount));
            int lastIndex = Mathf.Min(svData.Count, firstIndex + svVisibleCount);
            // Headers are injected on the fly
            int svHeadersVisible = svMain.headerLine.FindAll(x => x.hRow >= firstIndex && x.hRow <= lastIndex).Count; // Header Counts
            int svHeadersVisibleHeight = (int)svMain.lineHeight * svHeadersVisible; // Header height visible

            svMain.list = new Rect(0, 0, position.width - 15, (svMain.lineHeight * svData.Count) + svHeadersVisibleHeight);
#endif

            // -- SV Setup

            // Ignore GUI.changed while dragging scrollbar - redraw
            EditorGUI.BeginChangeCheck();

#if UNITY_5_2_OR_OLDER
            svMain.position = GUI.BeginScrollView (svMain.frame, svMain.position, svMain.list, false, false);
#else // Fix for (negative) svMain.position.y values in 5.0/5.1/5.2
            svMain.position = GUI.BeginScrollView(svMain.frame, svMain.position, svMain.list, false, true);
#endif
            // Ignore GUI.changed while dragging scrollbar
            if (EditorGUI.EndChangeCheck())
                GUI.changed = false;

            float svGroupY = svMain.lineHeight * firstIndex;
            float svGroupHeight = svMain.height + svHeadersVisibleHeight + svMain.lineHeight;

            Rect rectGroup = new Rect(0, svGroupY, svMain.frame.width, svGroupHeight);
            GUI.BeginGroup(rectGroup);

            // -- SV Loop ****

            for (int i = firstIndex; i < lastIndex; i++)
            {
                // Draw header & increment line IF needed
                SVDrawHeader(svMain.button, i); // redraw 

                // Determine show/hide version indent (arrow icon), if previous id != current id
                var disableIndent = ((!svData[i].isExported) && pkgIDLast != svData[i].id);

                // Draw package button
                SVDrawLine(svMain.button, i, disableIndent, enableLineNumbers); // redraw 

                // set up rectangles for the next line
                svMain.button = svMain.button.SetRect(null, (svMain.button.y + svMain.lineHeight));
                svMain.toggle = svMain.toggle.SetRect(null, (svMain.toggle.y + svMain.lineHeight));

                // Store id as pkgIDLast for processing multiple of the same ID, ie- old versions of the same asset
                pkgIDLast = svData[i].id;
            }

            GUI.EndGroup();
            GUI.EndScrollView();
        }

        internal void SVDrawHeader(Rect pButton, int i)
        {
            bool forceHeaders = false;

            // Draw header & increment line IF needed
            if (gyaVars.Prefs.enableHeaders || forceHeaders)
            {
                for (int j = 0; j < svMain.headerCount; j++)
                {
                    if (svMain.headerLine[j].hRow > i) break; // Break out early if hRow larger then current row

                    if (svMain.headerLine[j].hRow == i)
                    {
                        GUI.Box(svMain.toggle, "", svStyle.seperator);
                        GUI.Box(svMain.button, svMain.headerLine[j].hText, svStyle.seperator);

                        if (isMouseOutsideFoldOut && svMain.button.Contains(evt.mousePosition))
                            svCurrentPkgNumber = -1;

                        // set up rectangles for the next line
                        svMain.button = svMain.button.SetRect(null, (svMain.button.y + svMain.lineHeight));
                        svMain.toggle = svMain.toggle.SetRect(null, (svMain.toggle.y + svMain.lineHeight));
                    }
                }
            }
        }

        // If hovering, get current assets info
        internal void SVDrawLine(Rect pButton, int i, bool disableVerIndent, bool enableLineNumbers = false)
        {
            // Full item line in sv
            var lineRect = new Rect(0, pButton.y, position.width, svMain.lineHeight);
            // FoldOut - svFOHandling
            if (isMouseOutsideFoldOut && lineRect.Contains(evt.mousePosition))
                svCurrentPkgNumber = i;

            // Enable Selection line Hightlight of the toggle & icon
            GUI.BeginGroup(lineRect, svStyle.d);
            GUI.EndGroup();

            // TOGGLE - Asset Marked
            svData[i].isMarked = GUI.Toggle(svMain.toggle, svData[i].isMarked, "");

            // ICON - Pin Collection Type icon to left of list if enabled
            if (gyaVars.Prefs.enableCollectionTypeIcons)
            {
                Rect svIconLeft = new Rect(18, pButton.y + 1, 18, svMain.lineHeight);

                // Show defualt icon for current Unity version, else show Alt Icons                                         
                bool useDefaultIcon = true;
                if (gyaVars.Prefs.enableAltIconForOldVersions && (!svData[i].isSameVersionAsUnity))
                    useDefaultIcon = false;

                switch (svData[i].collection)
                {
                    case svCollection.Store:
                        GUI.Box(svIconLeft, (useDefaultIcon ? GYATexture.iconStore : GYATexture.iconStoreAlt), svStyle.iconLeft);
                        break;
                    case svCollection.User:
                        GUI.Box(svIconLeft, (useDefaultIcon ? GYATexture.iconUser : GYATexture.iconUserAlt), svStyle.iconLeft);
                        break;
                    case svCollection.Standard:
                        GUI.Box(svIconLeft, (useDefaultIcon ? GYATexture.iconStandard : GYATexture.iconStandardAlt), svStyle.iconLeft);
                        break;
                    case svCollection.Old:
                        GUI.Box(svIconLeft, (useDefaultIcon ? GYATexture.iconOld : GYATexture.iconOldAlt), svStyle.iconLeft);
                        break;
                    case svCollection.Project:
                        GUI.Box(svIconLeft, GYATexture.iconProject, svStyle.iconLeft);
                        break;
                    default:
                        break;
                }
            }

            // ScrollView Click handing
            if (GUI.Button(pButton, new GUIContent(""), EditorStyles.label))
            {
                // Left Click
                if (evt.button == 0)
                    svData[i].isMarked = !svData[i].isMarked;

                // Right Click
                if (evt.button == 1)
                {
                    this.Focus(); // Required, else popup after an import may appear in bottom left corner of main window
                    SVPopUpRightClick();
                }
            }

            // List Item - Asset Title append Version if needed, bypass appending to assets that have already been renamed with the version appended
            var sbPkgTitle = new System.Text.StringBuilder();
            // Show Line #'s - used for testing when adjusting SV
            if (enableLineNumbers)
            {
                sbPkgTitle.Append((i + 1).ToString("0000"));
                sbPkgTitle.Append(" - ");
            }
            // Icon if multiple versions, valid for sortActive: TitleNestedVersions, PackageID
            if (!disableVerIndent && !(svData[i].isLatestVersion) && (sortActive == svSortBy.TitleNestedVersions || sortActive == svSortBy.PackageID))
            {
                sbPkgTitle.Append(" \u27A5  ");
            }

            // Show title with(out) version
            if (!svData[i].isVersionAppended)
                sbPkgTitle.Append(svData[i].titleWithVersion);
            else
                sbPkgTitle.Append(svData[i].title);

            var pkgTitle = sbPkgTitle.ToString();

            // Select appropriate button style
            GUIStyle svButton = svStyle.standard;

            switch (svData[i].collection)
            {
                case svCollection.Store:
                    if (svData[i].isOldToMove)
                        svButton = svStyle.oldToMove;
                    else
                        svButton = svStyle.store;
                    break;
                case svCollection.User:
                    svButton = svStyle.user;
                    break;
                case svCollection.Standard:
                    svButton = svStyle.standard;
                    break;
                case svCollection.Old:
                    svButton = svData[i].isOldToMove ? svStyle.oldToMove : svStyle.old;
                    break;
                case svCollection.Project:
                    svButton = svStyle.project;
                    break;
                case svCollection.All:
                    break;
                case svCollection.Group:
                    break;
                default:
                    break;
            }

            // Show button and icons
            GUI.Box(pButton, pkgTitle, svButton);
            PinIcons(i);
        }

        internal void UpdateFOInfo(int i)
        {
            // Asset info for foldout
            if (gyaVars.Prefs.showSVInfo) // redraw 2% 3MB's High GC
            {
                if (!isMouseOutsideFoldOut) svCurrentPkgNumber = -1;

                var sbfoPackageInfo = new System.Text.StringBuilder();

                sbfoPackageInfo.Append("<b>Title</b>:          ");

                if (svData[i].isFileMissing)
                    sbfoPackageInfo.Append("<color=orange><b>[MISSING]</b></color> ");

                if (svData[i].isDamaged)
                    sbfoPackageInfo.Append("<color=orange><b>[DAMAGED]</b></color> ");

                //sbfoPackageInfo.Append(GYAFile.GetTitleCleaned(svData[i]) + GYAFile.GetVersionStringToAppend(svData[i], true));
                sbfoPackageInfo.Append(svData[i].titleWithVersion);

                // This is so the info window shows blank when nothing has been initially selected
                // Show if Asset Store Package, if not then show minimal info
                string mTimeInfo = svData[i].fileDataCreated.ToString("yyyy-MM-dd");
                if (!svData[i].isExported)
                {
                    sbfoPackageInfo.Append("\n<b>Category</b>:   ");
                    sbfoPackageInfo.Append(svData[i].category.label);
                    sbfoPackageInfo.Append("\n<b>Publisher</b>:  ");
                    sbfoPackageInfo.Append(svData[i].publisher.label);

                    // Hide if not downloaded
                    if (!svData[i].notDownloaded)
                    {
                        sbfoPackageInfo.Append("\n<b>Size</b>:  ");
                        sbfoPackageInfo.Append(svData[i].fileSize.BytesToKB().PadLeft(12));
                        sbfoPackageInfo.Append("   <b>Unity Ver</b>: ");
                        sbfoPackageInfo.Append(GYAPackage.GetUnityVersionOrUnknown(svData[i].unity_version).PadRight(18));
                        sbfoPackageInfo.Append("\n<b>Bld</b>:   ");
                        sbfoPackageInfo.Append(mTimeInfo + "");
                        sbfoPackageInfo.Append("   <b>Pub</b>: ");
                        sbfoPackageInfo.Append(Convert.ToDateTime(svData[i].pubDateToDateTime).ToString("yyyy-MM-dd"));
                        sbfoPackageInfo.Append("   <b>ID</b>: ");
                        sbfoPackageInfo.Append(svData[i].id);
                    }
                    else
                    {
                        sbfoPackageInfo.Append("\n<b>ID             </b>:  ");
                        sbfoPackageInfo.Append(svData[i].id);
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(sbfoPackageInfo.ToString()))
                    {
                        sbfoPackageInfo.Append("\n<b>Category</b>:   n/a");
                        sbfoPackageInfo.Append("\n<b>Publisher</b>:  n/a");
                        sbfoPackageInfo.Append("\n<b>Size</b>: ");
                        sbfoPackageInfo.Append(svData[i].fileSize.BytesToKB().PadLeft(12));
                        sbfoPackageInfo.Append("\n<b>Bld</b>:   ");
                        sbfoPackageInfo.Append(mTimeInfo);
                    }
                }

                if (IsSortActive(svSortBy.DateFile))
                {
                    sbfoPackageInfo.Append("\n<b>File Date</b>: ");
                    sbfoPackageInfo.Append(GYAExt.DateAsISO8601(svData[i].fileDateCreated));
                }
                else if (IsSortActive(svSortBy.DatePublish))
                {
                    sbfoPackageInfo.Append("\n<b>Publish Date</b>: ");
                    sbfoPackageInfo.Append(Convert.ToDateTime(svData[i].pubDateToDateTime).ToString("yyyy-MM-dd"));
                }
                else if (IsSortActive(svSortBy.DateBuild))
                {
                    sbfoPackageInfo.Append("\n<b>Build Date</b>: ");
                    sbfoPackageInfo.Append(mTimeInfo);
                }
                else if (IsSortActive(svSortBy.DatePurchased))
                {
                    sbfoPackageInfo.Append("\n<b>Purchased Date</b>: ");
                    sbfoPackageInfo.Append(GYAExt.DateAsISO8601(svData[i].datePurchased));
                }
                else if (IsSortActive(svSortBy.DateCreated))
                {
                    sbfoPackageInfo.Append("\n<b>Created Date</b>: ");
                    sbfoPackageInfo.Append(GYAExt.DateAsISO8601(svData[i].dateCreated));
                }
                else if (IsSortActive(svSortBy.DateUpdated))
                {
                    sbfoPackageInfo.Append("\n<b>Updated Date</b>: ");
                    sbfoPackageInfo.Append(GYAExt.DateAsISO8601(svData[i].dateUpdated));
                }
                else if (!svData[i].notDownloaded)
                {
                    sbfoPackageInfo.Append("\n<b>Path</b>: ");
                    sbfoPackageInfo.Append(svData[i].filePath);
                }
                foPackageInfo = sbfoPackageInfo.ToString();
            }
        }

        internal void DrawIcon(ref int pinSize, ref float pinPos, ref Rect iconRect, Texture2D iconToShow, bool isActive = true, bool addBG = true)
        {
            pinPos -= pinSize;
            iconRect.x = pinPos;
            Color guiColor = GUI.color;

            if (!isActive)
                GUI.color = Color.gray;

            if (addBG)
                EditorGUI.DrawPreviewTexture(iconRect, GYATexture.GetUnitySkinTexture());

            GUI.DrawTexture(iconRect, iconToShow, ScaleMode.ScaleToFit);

            if (!isActive)
                GUI.color = guiColor;
        }

        internal void DrawIconAsButton(ref int pinSize, ref float pinPos, ref Rect iconRect, Texture2D iconToShow, bool isActive = true, bool addBG = true)
        {
            pinPos -= pinSize;
            iconRect.x = pinPos;
            Color guiColor = GUI.color;

            if (!isActive)
                GUI.color = Color.gray;

            if (addBG)
                EditorGUI.DrawPreviewTexture(iconRect, GYATexture.GetUnitySkinTexture());

            GUI.DrawTexture(iconRect, iconToShow, ScaleMode.ScaleToFit);

            if (!isActive)
                GUI.color = guiColor;
        }

        // Icons pinned to right of SV
        internal void PinIcons(int i) // i = svData[i]
        {

#if UNITY_2019_GUI_TWEAKS
            // Icon X position
            int xOffset = 1;
            int yOffset = 2;
            int pinSize = 14;
            float pinPos = position.width - pinSize;
            Rect iconRect = new Rect(pinPos, svMain.button.y + 1, pinSize, pinSize);
#else
            // Icon X position
            int xOffset = -1;
            int yOffset = -1;
            int pinSize = 14;
            float pinPos = position.width - pinSize;
            Rect iconRect = new Rect(pinPos, svMain.button.y + 1, pinSize, pinSize);
#endif

            // Notes Icon - svCurrentPkg.AssetInfo.Notes
            if (string.IsNullOrEmpty(svData[i].AssetInfo.Notes))
            {
                //DrawIconAsButton(ref pinSize, ref pinPos, ref iconRect, GYATexture.iconCategory);
                //DrawIcon(ref pinSize, ref pinPos, ref iconRect, GYATexture.iconLock);
            }
            else
            {
                pinPos -= pinSize;
                iconRect.x = pinPos;
                svStyle.icon.contentOffset = new Vector2(xOffset, yOffset);
                EditorGUI.DrawPreviewTexture(iconRect, GYATexture.GetUnitySkinTexture());

#if UNITY_2019_GUI_TWEAKS
                GUI.Box(iconRect, "<size=12><color=BLUE>\u270E</color></size>", svStyle.icon); // pencil

#else
                if (GYAExt.IsProSkin)
                    GUI.Box(iconRect, "<size=16><color=WHITE>\u270E</color></size>", svStyle.icon); // pencil
                else
                    GUI.Box(iconRect, "<size=16><color=BLACK>\u270E</color></size>", svStyle.icon); // pencil
#endif

            }

            // Favorite icon
            if (svData[i].isFavorite)
                DrawIcon(ref pinSize, ref pinPos, ref iconRect, GYATexture.iconFavorite);

            // Not Downloaded
            if (svData[i].notDownloaded)
                DrawIcon(ref pinSize, ref pinPos, ref iconRect, GYATexture.iconDownload);

            // Deprecated Icon
            if (svData[i].isDeprecated)
            {

#if UNITY_2019_GUI_TWEAKS
                pinPos -= pinSize;
                iconRect.x = pinPos;
                xOffset = 0;
                yOffset = 1;
                svStyle.icon.contentOffset = new Vector2(xOffset, yOffset);
                EditorGUI.DrawPreviewTexture(iconRect, GYATexture.GetUnitySkinTexture());
                GUI.Box(iconRect, "<size=14><color=#FF8C00ff>\u2622</color></size>", svStyle.icon); // radioactive
#else
                pinPos -= pinSize;
                iconRect.x = pinPos;
                svStyle.icon.contentOffset = new Vector2(xOffset, yOffset);
                EditorGUI.DrawPreviewTexture(iconRect, GYATexture.GetUnitySkinTexture());
                GUI.Box(iconRect, "<size=15><color=#FF8C00ff>\u2622</color></size>", svStyle.icon); // radioactive
#endif

            }

            // Show icon specific to current Group
            if (showActive == svCollection.Group && svData[i].isInAGroup)
            {
                // Find asset in group and check if useLatestVersion is true
                var tAsset = gyaData.Groups[showGroup].Assets.FindAll(x => x.id == svData[i].id && !x.useLatestVersion);

                if (tAsset.Count > 0)
                    DrawIcon(ref pinSize, ref pinPos, ref iconRect, GYATexture.iconLock);
            }
            else // Show icon as indicator that asset is locked to a Group
            {
                if (svData[i].isInAGroupLockedVersion)
                    DrawIcon(ref pinSize, ref pinPos, ref iconRect, GYATexture.iconLock);
            }

            // Damaged Icon
            if (svData[i].isDamaged)
                DrawIcon(ref pinSize, ref pinPos, ref iconRect, GYATexture.iconDamaged);

            // isFileMissing Icon
            if (svData[i].isFileMissing)
            {
                pinPos -= pinSize;
                iconRect.x = pinPos;
                EditorGUI.DrawPreviewTexture(iconRect, GYATexture.GetUnitySkinTexture());
                GUI.Box(iconRect, "<color=orange>[MISSING]</color>", svStyle.icon);
                //GUI.DrawTexture(iconRect, GYATexture.iconFavorite, ScaleMode.ScaleToFit);
            }
        }

        // Popup window routine for Right button
        internal void SVPopUpRightClick()
        {
            // Adjust for Icons
            float ddLoc = 18;
            if (gyaVars.Prefs.enableCollectionTypeIcons)
                ddLoc += 18;

            Rect bRect;

            // Fix a popup y axis position difference between the 2 platforms in the scrollview
            // Double check this in case it has changed
            if (GYAExt.IsOSWin)
                bRect = new Rect(ddLoc + 4, svMain.button.y + 16, 0, 0);
            else
                bRect = new Rect(ddLoc, svMain.button.y + 8, 0, 0);

            // Now create the menu, add items and show it
            var popupMenu = new GenericMenu();

#if TESTING
            popupMenu.AddItem(new GUIContent("-- INTERNAL TESTING --"), false, TBPopUpCallback, "");
            if (!svCurrentPkg.isExported)
            {
                popupMenu.AddItem (new GUIContent ("Download AssetID Info"), false, TBPopUpCallback, "DownloadASAssetID" );
            }
            popupMenu.AddSeparator("");
#endif
	        //popupMenu.AddItem(new GUIContent("Single Asset Options:"));
	        popupMenu.AddItem(new GUIContent("Single Assets Options:"), false, TBPopUpCallback, "null");
	        popupMenu.AddSeparator("");

	        // Import
            if (!(svCurrentPkg.isFileMissing || svCurrentPkg.notDownloaded))
            {
                popupMenu.AddItem(new GUIContent("Import"), false, TBPopUpCallback, "PopupImport");
                popupMenu.AddItem(new GUIContent("Import Interactively"), false, TBPopUpCallback, "PopupImportInteractive");

                if (GYAReflect.NamespaceExists("CodeStage.PackageToFolder"))
                {
                    popupMenu.AddItem(new GUIContent("Import To Folder"), false, TBPopUpCallback,
                        "PopupPackage2Folder");
                }
                else
                {
                    popupMenu.AddDisabledItem(new GUIContent("Import To Folder/"));
                    popupMenu.AddDisabledItem(new GUIContent("Import To Folder/3rd Party Asset Required:"));
                    // Package2Folder AssetID = 64829
                    if (AssetIDExist(64829))
                        popupMenu.AddItem(new GUIContent("Import To Folder/Import Package2Folder"), false, TBPopUpCallback, "ImportPackage2Folder");
                    else
                        popupMenu.AddItem(new GUIContent("Import To Folder/Download Package2Folder"), false, TBPopUpCallback, "DownloadPackage2Folder");
                }
            }
            else
            {
                if (svCurrentPkg.notDownloaded)
                {
                }
                else
                {
                    popupMenu.AddDisabledItem(new GUIContent("Import"));
                    popupMenu.AddDisabledItem(new GUIContent("Import Interactively"));
                    popupMenu.AddDisabledItem(new GUIContent("Import To Folder"));
                }
            }

            // User Notes - svCurrentPkg.UserData.Notes
            popupMenu.AddSeparator("");
            popupMenu.AddItem(new GUIContent("Add or Edit Note"), false, TBPopUpCallback, "PopupNotes");

            // Copy to clipboard
            popupMenu.AddSeparator("");
            if (!svCurrentPkg.notDownloaded)
            {
                popupMenu.AddItem(new GUIContent("Copy To Clipboard/Path"), false, ClipboardCallback, svCurrentPkg.filePath);
                popupMenu.AddDisabledItem(new GUIContent("Copy To Clipboard/"));
                if (!svCurrentPkg.isExported)
                {
                    popupMenu.AddItem(new GUIContent("Copy To Clipboard/Title"), false, ClipboardCallback, svCurrentPkg.title);
                    popupMenu.AddItem(new GUIContent("Copy To Clipboard/ID"), false, ClipboardCallback, svCurrentPkg.id);
                    popupMenu.AddItem(new GUIContent("Copy To Clipboard/Publisher"), false, ClipboardCallback, svCurrentPkg.publisher.label);
                    popupMenu.AddItem(new GUIContent("Copy To Clipboard/URL"), false, ClipboardCallback, "https://www.assetstore.unity3d.com/#/content/" + svCurrentPkg.link.id);
                    popupMenu.AddDisabledItem(new GUIContent("Copy To Clipboard/"));
                    popupMenu.AddItem(new GUIContent("Copy To Clipboard/Package Info"), false, ClipboardCallback, GYAExt.ToJson(svCurrentPkg, true));
                }
            }
            else
            {
                popupMenu.AddDisabledItem(new GUIContent("Copy To Clipboard/Path"));
            }

            popupMenu.AddSeparator("");
            if (!svCurrentPkg.notDownloaded)
                popupMenu.AddItem(new GUIContent("Open Folder"), false, TBPopUpCallback, "AssetFolder");
            else
                popupMenu.AddDisabledItem(new GUIContent("Open Folder"));

            // Open URL
            if (!svCurrentPkg.isExported)
            {
                if (svCurrentPkg.notDownloaded)
                {
                    popupMenu.AddItem(new GUIContent("Open URL to Download"), false, TBPopUpCallback, "AssetURLinUnity");
                    //popupMenu.AddItem(new GUIContent("Open URL/Publisher"), false, TBPopUpCallback, "PublisherURLinUnity");
                }
                else
                {
                    popupMenu.AddItem(new GUIContent("Open URL/Asset"), false, TBPopUpCallback, "AssetURL");
                    popupMenu.AddItem(new GUIContent("Open URL/Publisher"), false, TBPopUpCallback, "PublisherURL");
                }
            }
            else
            {
                popupMenu.AddDisabledItem(new GUIContent("Open URL/Asset"));
                popupMenu.AddDisabledItem(new GUIContent("Open URL/Publisher"));
            }

            popupMenu.AddSeparator("");

            // Group Add To Menu - Don't show for Old/Project as assets in there are only temporary
            if (!svCurrentPkg.isFileMissing && svCurrentPkg.collection != svCollection.Old &&
                svCurrentPkg.collection != svCollection.Project && !svCurrentPkg.notDownloaded)
            {
                // grpsub: handle sub groups
                for (int i = 0; i < gyaData.Groups.Count; ++i)
                {
                    string grpLine = ddCollections.ElementAt(5 + i).Value;

                    if ((i + 1 < gyaData.Groups.Count) && ddCollections.ElementAt(5 + (i + 1)).Value.Contains(grpLine))
                    {
                        popupMenu.AddItem(new GUIContent("Add to Group/" + grpLine + "/"), false, GroupAddTo, i);
                        popupMenu.AddItem(new GUIContent("Add to Group/" + grpLine), false, GroupAddTo, i);
                    }
                    else // Show normal
                    {
                        popupMenu.AddItem(new GUIContent("Add to Group/" + grpLine), false, GroupAddTo, i);
                    }
                }
            }
            else
            {
                popupMenu.AddDisabledItem(new GUIContent("Add to Group"));
            }

            if (showActive == svCollection.Group)
            {
                // grpsub: handle sub groups - Move To Group
                for (int i = 0; i < gyaData.Groups.Count; ++i)
                {
                    string grpLine = ddCollections.ElementAt(5 + i).Value;

                    if ((i + 1 < gyaData.Groups.Count) && ddCollections.ElementAt(5 + (i + 1)).Value.Contains(grpLine))
                    {
                        //assetMenu.AddItem(new GUIContent(grpLine + "/"), (showGroup == i && showActive == svCollection.Group), TBPopUpShowGroup, i);
                        popupMenu.AddItem(new GUIContent("Move to Group/" + grpLine + "/"), false, GroupMoveTo, i);
                        popupMenu.AddItem(new GUIContent("Move to Group/" + grpLine), false, GroupMoveTo, i);
                    }
                    else // Show normal
                    {
                        popupMenu.AddItem(new GUIContent("Move to Group/" + grpLine), false, GroupMoveTo, i);
                    }
                }

                //markedMenu.AddSeparator("");
                //if (showActive == svCollection.Group)
                //{
                // Remove asset from group
                //popupMenu.AddItem(new GUIContent("Remove from Current Group"), false, GroupRemoveAssetMultiple, showGroup);
            }
            else
            {
                popupMenu.AddDisabledItem(new GUIContent("Move to Group"));
                //popupMenu.AddDisabledItem(new GUIContent("Remove from Current Group"));
            }
            popupMenu.AddSeparator("");

            // Remove asset from group
            if (showActive == svCollection.Group)
            {
                popupMenu.AddItem(new GUIContent("Remove from Group"), false, GroupRemoveAsset, null);
            }

            // Assign specific version for a grouped asset ***
            if (showActive == svCollection.Group)
            {
                popupMenu.AddSeparator("");

                var tVersions =
                    gyaData.Assets.FindAll(x => x.id == svCurrentPkg.id && !x.isExported &&
                                                x.collection == svCollection.User);
                if (tVersions.Count > 0)
                {
                    tVersions.Sort((x, y) => -x.version_id.CompareTo(y.version_id));
                    var tGroupPkg = gyaData.Groups[showGroup].Assets.Find(x => x.id == svCurrentPkg.id);

                    popupMenu.AddItem(new GUIContent("Lock Version for this Group/Newest"), (tGroupPkg.version_id == 0), GroupAssignVersion, 0);
                    popupMenu.AddItem(new GUIContent("Lock Version for this Group/"), false, GroupAssignVersion, null);
                    foreach (GYAData.Asset t in tVersions)
                    {
                        popupMenu.AddItem(new GUIContent("Lock Version for this Group/" + GYAFile.GetSafeFilename(t.version)), (tGroupPkg.version_id == t.version_id), GroupAssignVersion,
                            t.version_id);
                    }
                }
                else
                {
                    popupMenu.AddDisabledItem(new GUIContent("Lock Version for this Group"));
                }
            }

            popupMenu.AddSeparator("");

            // Project specific assets (installed)
            //popupMenu.AddItem(new GUIContent("Add to Current Project"), false, ProjectAddAsset, null);
            //popupMenu.AddItem(new GUIContent("Remove from Current Project"), false, ProjectRemoveAsset, null);
            //popupMenu.AddSeparator("");

            // File Options
            if (!svCurrentPkg.notDownloaded && !svCurrentPkg.isFileMissing)
            {
                popupMenu.AddSeparator("");
                // If not in User Assets folder
                if ((gyaVars.Prefs.pathUserAssets.Any() &&
                     !svCurrentPkg.filePath.Contains(gyaVars.Prefs.pathUserAssets[0],
                         StringComparison.OrdinalIgnoreCase)))
                {
                    popupMenu.AddItem(new GUIContent("File Options/Copy to User Folder"), false, TBPopUpCallback, "CopyToUser");
                }
                else
                {
                    popupMenu.AddDisabledItem(new GUIContent("File Options/Copy to User Folder"));
                }
                popupMenu.AddItem(new GUIContent("File Options/Copy to ..."), false, TBPopUpCallback,
                    "CopyToSelectable");
                if (!svCurrentPkg.filePath.Replace('/', '\\').StartsWith(gyaVars.pathOldAssetsFolder.Replace('/', '\\'),
                        StringComparison.OrdinalIgnoreCase) && showActive != svCollection.Standard &&
                    (!svCurrentPkg.isExported))
                {
                    popupMenu.AddItem(new GUIContent("File Options/Move to Old Assets"), false, TBPopUpCallback, "MoveToOld");
                }
                else
                {
                    popupMenu.AddDisabledItem(new GUIContent("File Options/Move to Old Assets"));
                }

                popupMenu.AddItem(new GUIContent("File Options/"), false, TBPopUpCallback, "");
                popupMenu.AddItem(new GUIContent("File Options/Copy To Sorted Assets"), false, TBPopUpCallback, "CopyToSorted");
                popupMenu.AddItem(new GUIContent("File Options/Move to Sorted Assets"), false, TBPopUpCallback, "MoveToSorted");
                popupMenu.AddItem(new GUIContent("File Options/"), false, TBPopUpCallback, "");

                if (!svCurrentPkg.isExported)
                {
                    popupMenu.AddItem(new GUIContent("File Options/Rename with Version"), false, GYAFile.RenameWithVersion, svCurrentPkg);
                    popupMenu.AddItem(new GUIContent("File Options/Rename without Version"), false, GYAFile.RenameWithoutVersion, svCurrentPkg);
                }
                else
                {
                    popupMenu.AddDisabledItem(new GUIContent("File Options/Rename with Version"));
                    popupMenu.AddDisabledItem(new GUIContent("File Options/Rename without Version"));
                }
                popupMenu.AddItem(new GUIContent("File Options/"), false, TBPopUpCallback, "");
                popupMenu.AddItem(new GUIContent("File Options/Delete Asset"), false, TBPopUpCallback, "DeleteAsset");
            }

            // category.label: Change '/' and '&' as they affect the popup
            string pkgTitle = MenuItemEscape(svCurrentPkg.title);
            string pkgCategoryLabel = MenuItemEscape(svCurrentPkg.category.label);

            // Disabled for now, replaced with Version locking, enablePopupDetails = false
            if (gyaVars.Prefs.enablePopupDetails)
            {
                popupMenu.AddSeparator("");
                popupMenu.AddDisabledItem(new GUIContent("Title: " + pkgTitle));
                if (!svCurrentPkg.isExported)
                    popupMenu.AddDisabledItem(new GUIContent("Version: " + svCurrentPkg.version));

                popupMenu.AddDisabledItem(new GUIContent("Size: " + svCurrentPkg.fileSize.BytesToKB()));
                if (!gyaVars.Prefs.showSVInfo)
                {
                    if (!svCurrentPkg.isExported)
                    {
                        popupMenu.AddDisabledItem(new GUIContent("Category: " + pkgCategoryLabel));
                        popupMenu.AddDisabledItem(new GUIContent("Publisher: " + svCurrentPkg.publisher.label));
                        popupMenu.AddDisabledItem(new GUIContent("Date: " + svCurrentPkg.pubdate));
                    }
                }
            }

#if TESTING
            // Dump package info to console
            popupMenu.AddItem (new GUIContent ("Dump Package Info to Console"), false, SVPopUpCallback, svCurrentPkg.version_id );
#endif

            // Fix a popup y axis position difference between the 2 platforms in the scrollview
            // Double check this in case it has changed
            if (GYAExt.IsOSWin)
                popupMenu.DropDown(bRect);
            else
                popupMenu.DropDown(bRect);

            evt.Use();
        }

        // TODO: Dump package info to Console
        internal void SVPopUpCallback(object pObj)
        {
            TextEditor te = new TextEditor();
#if UNITY_5_2_OR_OLDER
            te.content = new GUIContent (pObj.ToString ());
#else
            te.text = pObj.ToString();
#endif
            te.SelectAll();
            te.Copy();
        }

        // Check if search is active
        internal bool IsSearchActive(svSearchBy searchVal)
        {
            return searchVal == searchActive;
        }

        // Check if sort is active
        internal bool IsSortActive(svSortBy sortVal)
        {
            return sortVal == sortActive;
        }

        internal string RemoveLeading(string s)
        {
            if (!String.IsNullOrEmpty(s))
                s = s.Trim();

            if (String.IsNullOrEmpty(s))
                return s;

            return !char.IsLetterOrDigit(s[0]) ? s.Trim().Substring(1).TrimStart() : s;
        }

        // Sort packages by ...
        internal void PackagesSearchBy(object searchBy)
        {
            PackagesSearchBy((svSearchBy)searchBy);
        }

        internal void PackagesSearchBy(svSearchBy searchBy)
        {
            searchActive = searchBy;
            infoChanged = true;
        }

        // Sort packages by ...
        internal void PackagesSortBy(object sortBy)
        {
            PackagesSortBy((svSortBy)sortBy);
        }

        // Not working??
        internal string StripLeadAndDiacritics(string pString)
        {
            byte[] tempBytes;
            tempBytes = System.Text.Encoding.GetEncoding("ISO-8859-8").GetBytes(pString);
            string asciiStr = System.Text.Encoding.UTF8.GetString(tempBytes);
            return asciiStr;
        }

        internal void PackagesSortBy(svSortBy sortBy)
        {
            if (sortBy == svSortBy.TitleNestedVersions)
            {
                sortActive = svSortBy.TitleNestedVersions;

                List<GYAData.Asset> tAssets = new List<GYAData.Asset>();

                // Copy x.Project assets to tAssets
                tAssets = gyaData.Assets
                    .Where(x => x.collection == svCollection.Project)
                    .OrderBy(x => RemoveLeading(x.titleWithVersion))
                    .ThenByDescending(x => x.version_id)
                    .ToList();

                // Title - Default, nested MAY be out of order
                gyaData.Assets = gyaData.Assets
                    .Where(x => x.collection != svCollection.Project)
                    .OrderByDescending(x => x.isDamaged &&
                                       x.titleWithVersion.StartsWith("unknown", StringComparison.InvariantCultureIgnoreCase))
                    .ThenByDescending(x => x.collection == svCollection.Project)
                    .ThenBy(x => RemoveLeading(x.titleWithVersion))
                    .ThenByDescending(x => x.version_id)
                    .ThenBy(x => x.collection)
                    .ToList();

                // Nested Versions ONLY - Sort by version_id where there are multiple entries for an id
                IEnumerable<GYAData.Asset> sortedVersionsTmp =
                    from package in gyaData.Assets
                    where (package.collection != svCollection.Project)
                    orderby package.id, package.version_id descending
                    group package by package.id
                    into grouped
                    from package in grouped
                    where (grouped.Count() > 1 && package.id > 0)
                    select package;
                var sortedVersions = sortedVersionsTmp.ToList();

                // Build complete list
                foreach (var g in gyaData.Assets)
                {
                    if (!tAssets.Contains(g)) // if (s)gyaData.Assets does NOT exist in tAssets
                    {
                        if (sortedVersions.Contains(g)) // if exist in sortedVersions
                        {
                            var sTmp = sortedVersions.FindAll(x => x.id == g.id);
                            if (g.version_id == sTmp[0].version_id) // version_id's match
                            {
                                foreach (var s in sTmp) // add entries
                                {
                                    if (s.version_id != sTmp[0].version_id) // version_id's != latest ver
                                        s.isLatestVersion = false; // Mark older versions as false
                                    tAssets.Add(s);
                                }
                            }
                        }
                        else // add entry from (g)gyaData.Assets
                        {
                            tAssets.Add(g);
                        }
                    }
                }
                gyaData.Assets = tAssets;

                // Sort Groups
                for (int i = 0; i < grpData.Count; ++i)
                {
                    grpData[i] = grpData[i].OrderBy(x => RemoveLeading(x.titleWithVersion)).ThenByDescending(x => x.version_id).ToList();
                }
            }

            // Sort by Title
            if (sortBy == svSortBy.Title)
            {
                sortActive = svSortBy.Title;

                gyaData.Assets = gyaData.Assets
                .OrderByDescending(x => x.isDamaged && x.titleWithVersion.StartsWith("unknown", StringComparison.InvariantCultureIgnoreCase))
                .ThenByDescending(x => x.collection == svCollection.Project).ThenBy(x => RemoveLeading(x.titleWithVersion))
                .ThenByDescending(x => x.version_id).ThenBy(x => x.collection).ToList();

                // Sort Groups
                for (int i = 0; i < grpData.Count; ++i)
                {
                    grpData[i] = grpData[i].OrderBy(x => RemoveLeading(x.titleWithVersion)).ThenByDescending(x => x.version_id).ToList();
                }
            }
            // Sort by Main Category and Title
            if (sortBy == svSortBy.Category)
            {
                sortActive = svSortBy.Category;
                gyaData.Assets = gyaData.Assets.OrderBy(x => x.category.label.Split('/')[0]).ThenBy(x => x.titleWithVersion).ToList();
                // Sort Groups
                for (int i = 0; i < grpData.Count; ++i)
                {
                    grpData[i] = grpData[i].OrderBy(x => x.category.label.Split('/')[0]).ThenBy(x => x.titleWithVersion).ToList();
                }
            }
            // Sort by Sub Categories and Title
            if (sortBy == svSortBy.CategorySub)
            {
                sortActive = svSortBy.CategorySub;
                gyaData.Assets = gyaData.Assets.OrderBy(x => x.category.label).ThenBy(x => x.titleWithVersion).ToList();
                // Sort Groups
                for (int i = 0; i < grpData.Count; ++i)
                {
                    grpData[i] = grpData[i].OrderBy(x => x.category.label).ThenBy(x => x.titleWithVersion).ToList();
                }
            }
            // Sort by Publisher and Title
            if (sortBy == svSortBy.Publisher)
            {
                sortActive = svSortBy.Publisher;
                gyaData.Assets = gyaData.Assets.OrderBy(x => x.publisher.label).ThenBy(x => x.titleWithVersion).ToList();
                // Sort Groups
                for (int i = 0; i < grpData.Count; ++i)
                {
                    grpData[i] = grpData[i].OrderBy(x => x.publisher.label).ThenBy(x => x.titleWithVersion).ToList();
                }
            }
            // Sort by Size
            if (sortBy == svSortBy.Size)
            {
                sortActive = svSortBy.Size;
                gyaData.Assets.Sort((x, y) => -x.fileSize.CompareTo(y.fileSize));
                // Sort Groups
                for (int i = 0; i < grpData.Count; ++i)
                {
                    grpData[i].Sort((x, y) => -x.fileSize.CompareTo(y.fileSize));
                }
            }
            // Sort by Date File
            if (sortBy == svSortBy.DateFile)
            {
                sortActive = svSortBy.DateFile;
                gyaData.Assets.Sort((x, y) => -x.fileDateCreated.CompareTo(y.fileDateCreated));
                // Sort Groups
                for (int i = 0; i < grpData.Count; ++i)
                {
                    grpData[i].Sort((x, y) => -x.fileDateCreated.CompareTo(y.fileDateCreated));
                }
            }

            // Sort by Date Publish
            if (sortBy == svSortBy.DatePublish)
            {
                sortActive = svSortBy.DatePublish;
                gyaData.Assets.Sort((x, y) => -(x.pubDateToDateTime).CompareTo(y.pubDateToDateTime));
                // Sort Groups
                for (int i = 0; i < grpData.Count; ++i)
                {
                    grpData[i].Sort((x, y) => -(x.pubDateToDateTime).CompareTo(y.pubDateToDateTime));
                }
            }

            // Sort by Date Package Build
            if (sortBy == svSortBy.DateBuild)
            {
                sortActive = svSortBy.DateBuild;
                gyaData.Assets.Sort((x, y) => -x.fileDataCreated.CompareTo(y.fileDataCreated));
                // Sort Groups
                for (int i = 0; i < grpData.Count; ++i)
                {
                    grpData[i].Sort((x, y) => -x.fileDataCreated.CompareTo(y.fileDataCreated));
                }
            }

            // Sort by Date Package Purchased
            if (sortBy == svSortBy.DatePurchased)
            {
                sortActive = svSortBy.DatePurchased;
                gyaData.Assets.Sort((x, y) => -x.datePurchased.CompareTo(y.datePurchased));

                // Sort Groups
                for (int i = 0; i < grpData.Count; ++i)
                {
                    grpData[i].Sort((x, y) => -x.datePurchased.CompareTo(y.datePurchased));
                }
            }

            if (sortBy == svSortBy.DateCreated)
            {
                sortActive = svSortBy.DateCreated;
                gyaData.Assets.Sort((x, y) => -x.dateCreated.CompareTo(y.dateCreated));

                // Sort Groups
                for (int i = 0; i < grpData.Count; ++i)
                {
                    grpData[i].Sort((x, y) => -x.dateCreated.CompareTo(y.dateCreated));
                }
            }

            if (sortBy == svSortBy.DateUpdated)
            {
                sortActive = svSortBy.DateUpdated;
                gyaData.Assets.Sort((x, y) => -x.dateUpdated.CompareTo(y.dateUpdated));

                // Sort Groups
                for (int i = 0; i < grpData.Count; ++i)
                {
                    grpData[i].Sort((x, y) => -x.dateUpdated.CompareTo(y.dateUpdated));
                }
            }

            // Sort by Package ID
            if (sortBy == svSortBy.PackageID)
            {
                sortActive = svSortBy.PackageID;
                gyaData.Assets = gyaData.Assets.OrderByDescending(x => x.id).ThenByDescending(x => x.version_id).ToList();
                // Sort Groups
                for (int i = 0; i < grpData.Count; ++i)
                {
                    grpData[i] = grpData[i].OrderByDescending(x => x.id).ThenByDescending(x => x.version_id).ToList();
                }
            }
            // Sort by Version ID
            if (sortBy == svSortBy.VersionID)
            {
                sortActive = svSortBy.VersionID;
                gyaData.Assets.Sort((x, y) => -x.version_id.CompareTo(y.version_id));
                // Sort Groups
                for (int i = 0; i < grpData.Count; ++i)
                {
                    grpData[i].Sort((x, y) => -x.version_id.CompareTo(y.version_id));
                }
            }
            // Sort by Upload ID
            if (sortBy == svSortBy.UploadID)
            {
                sortActive = svSortBy.UploadID;
                gyaData.Assets = gyaData.Assets.OrderByDescending(x => x.upload_id).ThenBy(x => RemoveLeading(x.titleWithVersion)).ToList();

                // Sort Groups
                for (int i = 0; i < grpData.Count; ++i)
                {
                    grpData[i] = grpData[i].OrderByDescending(x => x.upload_id).ThenBy(x => RemoveLeading(x.titleWithVersion)).ToList();
                }
            }

            infoChanged = true;
        }

        // Verify JSON objects exist
        internal bool JSONObjectsAreNotNULL
        {
            get
            {
                if (gyaData.Assets == null)
                {
                    GYAExt.Log("JSON Object Changed or Missing.  Refreshing the data file.");
                    ErrorStateSet(ErrorCode.Error);
                    return false;
                }

                ErrorStateClear();
                return true;
            }
        }

        // Clear search
        internal void SearchClear()
        {
            GUIUtility.keyboardControl = 0;
            fldSearch = String.Empty;
        }

        // Remove GYA from Standard Assets
        internal void PersistDisable()
        {
            List<GYAData.Asset> resultsStandard =
                gyaData.Assets.FindAll(x => x.id == gyaVars.asset_id && x.collection == svCollection.Standard);
            int countInStandard = resultsStandard.Count;

            gyaVars.Prefs.isPersist = false;

            // If found, delete instance of GYA from Standard Assets
            if (countInStandard > 0)
            {
                string pathDeleteAsset = resultsStandard[0].filePath;
                try
                {
                    // Delete if exists
                    File.Delete(pathDeleteAsset);
                }
                catch (Exception ex)
                {
                    GYAExt.LogWarning("Persist Error: " + pathDeleteAsset, ex.Message);
                }
            }
        }

        // Persist in Standard Assets (Copy/Update GYA version in Standard Assets)
        // If enabled, this will be called from RefreshPackages so that it is always up-to-date
        internal bool PersistEnable()
        {
            if (gyaVars.Prefs.isPersist)
            {
                // Check if GYA is already in the Standard Assets folder
                List<GYAData.Asset> resultsStore =
                gyaData.Assets.FindAll(x => x.id == gyaVars.asset_id && x.collection == svCollection.Store);
                List<GYAData.Asset> resultsStandard =
                    gyaData.Assets.FindAll(x => x.id == gyaVars.asset_id && x.collection == svCollection.Standard);

                int verIDInStore = 0;
                int verIDInStandard = 0;

                //Changed to prevent checking resultsStandard if resultsStore was 0
                //Valid store entry?
                if (resultsStore.Count > 0 && resultsStore[0].unity_version.Length > 0)
                {
                    if (int.Parse(resultsStore[0].unity_version.Before(".")) <= GYAVersion.GetUnityVersionMajor)
                        verIDInStore = resultsStore[0].version_id;

                    //Valid standard assets entry?
                    if (resultsStandard.Count > 0 && resultsStandard[0].unity_version.Length > 0)
                    {
                        if (int.Parse(resultsStore[0].unity_version.Before(".")) <= GYAVersion.GetUnityVersionMajor)
                            verIDInStandard = resultsStandard[0].version_id;
                    }
                }

                // Perform update if required
                if (verIDInStore > verIDInStandard)
                {
                    // Copy GYA to Standard Asset folder
                    string pathCopyAsset = GYAExt.PathUnityStandardAssets;
                    string fileName = Path.GetFileName(resultsStore[0].filePath);

                    // Delete existing copy in SA
                    if (resultsStandard.Count > 0)
                    {
                        string pathDeleteAsset = resultsStandard[0].filePath;
                        try
                        {
                            // Delete if exists
                            File.Delete(pathDeleteAsset);
                        }
                        catch (Exception ex)
                        {
                            GYAExt.LogWarning("Persist Error: " + pathDeleteAsset, ex.Message);
                        }
                    }

                    // Copy the file
                    try
                    {
                        if (Directory.Exists(pathCopyAsset))
                        {
                            //// Add 2 spaces to packagename so GYA is at the top of the New Project->Assets List
                            //pathCopyAsset = Path.GetFullPath(Path.Combine(pathCopyAsset, "  " + fileName));
                            pathCopyAsset = Path.GetFullPath(Path.Combine(pathCopyAsset, fileName));

                            if (File.Exists(resultsStore[0].filePath))
                                File.SetAttributes(resultsStore[0].filePath, FileAttributes.Normal);

                            if (File.Exists(pathCopyAsset))
                                File.SetAttributes(pathCopyAsset, FileAttributes.Normal);

                            File.Copy(GYAExt.PathFixedForOS(resultsStore[0].filePath), pathCopyAsset, true);
                        }
                        else
                        {
                            GYAExt.LogWarning("Persist - Folder not found: " + GYAExt.PathUnityStandardAssets,
                            "To use Persist, please create the folder.");
                            return false;
                        }
                        // Verification
                        if (File.Exists(pathCopyAsset))
                        {
                            GYAExt.Log("Persist: Latest version (" + resultsStore[0].version + ") copied to the " +
                                       GYAExt.FolderUnityStandardAssets + " folder.");
                        }
                        else
                        {
                            GYAExt.LogWarning("Persist - File Copy Failed: " + pathCopyAsset,
                                "Make sure you have permissions to write to the " + GYAExt.PathUnityStandardAssets +
                                " folder.");
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        GYAExt.LogWarning("Persist - Exception Copying: " + pathCopyAsset,
                            "Please make sure you have Write Access to the '" + GYAExt.FolderUnityStandardAssets +
                            "' folder.\n" + ex.Message);
                        return false; // Try failed
                    }
                    return true; // Persist is running, package refresh should not be shown
                }
            }
            return false; // Persist did not copy/update, package refresh should be shown

        }

        //Return the version string of the newest gya pkg available
        internal string GetNewestPkgVersionOfGYAAsString()
        {
            //// Check if GYA is already in the Standard Assets folder
            //List<GYAData.Asset> resultsStore =
            //    gyaData.Assets.FindAll(x => x.id == gyaVars.asset_id && x.collection == svCollection.Store);

            ////Verify valid store entry
            //if (resultsStore.Count > 0 && resultsStore[0].unity_version.Length > 0)
            //{
            //    if (int.Parse(resultsStore[0].unity_version.Before(".")) <= GYAVersion.GetUnityVersionMajor)
            //    {
            //        //GYAExt.Log("Newest Pkg Version: " + resultsStore[0].version);
            //        return resultsStore[0].version;
            //    }
            //}

            //return "0.0.0.0";
            return GetNewestPkgVersionOfGYA().version;
        }

        //Return the pkg of the newest gya pkg available
        internal GYAData.Asset GetNewestPkgVersionOfGYA()
        {
            // Check if GYA is already in the Standard Assets folder
            List<GYAData.Asset> resultsStore =
                gyaData.Assets.FindAll(x => x.id == gyaVars.asset_id && x.collection == svCollection.Store);

            //Verify valid store entry
            if (resultsStore.Count > 0 && resultsStore[0].unity_version.Length > 0)
            {
                if (int.Parse(resultsStore[0].unity_version.Before(".")) <= GYAVersion.GetUnityVersionMajor)
                {
                    //GYAExt.Log("Newest Pkg Version: " + resultsStore[0].version);
                    return resultsStore[0];
                }
            }

            return new GYAData.Asset();
        }

        // Update to newest version of GYA if available
        internal bool UpdateToLatestVersionOfGYA()
        {
            bool hasUpdated = false; // changed if successful
            bool isNewVersion = false;
            var pkg = GetNewestPkgVersionOfGYA();

            string pkgUnityVer = pkg.unity_version;
            if (pkgUnityVer.Length == 0)
                pkgUnityVer = "0.0.0";

            // If active unity version is equal or greater than that of the pkg
            if (GYAVersion.UnityVersionIsEqualOrNewerThan(pkgUnityVer, 3))
            {
                isNewVersion = GYAVersion.IsNewGYAVersionAvailable(pkg.version);

                // If true, then update
                if (isNewVersion)
                {
                    // Prompt before updating?
                    //if (gyaVars.Prefs.promptBeforeUpdateGYA) // If enabled, don't forget 2 entries in WindowPrefs
                    if (true) // forced prompt for the time being
                    {
                        // If enabled, verify disabling
                        if (EditorUtility.DisplayDialog(gyaVars.abbr + " - Update Available",
                            "A new version of GYA has been detected.\n\n"
                            + "Would you like to Update GYA in this project?\n\n"
                            + "You can change update options in the prefs."
                            , "Cancel", "UPDATE"))
                        {
                            // Cancel - do nothing
                            GYAExt.Log("GYA Update - Cancelled by User.\n");
                            return false;
                        }
                        else
                        {
                            // UPDATE, passed thru to auto-update
                        }
                    }

                    // Perform update
                    GYAExt.Log("GYA Update - From: " + gyaVersion + " To: " + pkg.version + "\n");
                    hasUpdated = true;
                    //GYAImport.ImportSingle(GYAPackage.GetAssetByID(72902).filePath, true);
                    GYAImport.ImportSingle(pkg.filePath);

                }
                else
                {
                    // No update found
                    //GYAExt.Log("No update found at this time.\n");
                }
            }
            return hasUpdated;
        }

        internal void CheckIfGUISkinHasChanged(bool forceReload = false)
        {
            // Check if GUI Skin has been changed
            if (GUISkinHasChanged || forceReload)
            {
                GYATexture.LoadTextures();

                // DARKSKIN
                svStyle.d.normal.background = GYATexture.GetUnitySkinTexture();
                svStyle.store.normal.background = GYATexture.GetUnitySkinTexture();
                svStyle.user.normal.background = GYATexture.GetUnitySkinTexture();
                svStyle.standard.normal.background = GYATexture.GetUnitySkinTexture();
                svStyle.old.normal.background = GYATexture.GetUnitySkinTexture();
                svStyle.oldToMove.normal.background = GYATexture.GetUnitySkinTexture();
                svStyle.project.normal.background = GYATexture.GetUnitySkinTexture();

                if (infoStyle != null)
                    infoStyle.normal.background = GYATexture.GetUnitySkinTexture();

                svStyle.store.hover.background = GYATexture.texSelector;
                svStyle.store.active.background = GYATexture.texSelector;
                svStyle.user.hover.background = GYATexture.texSelector;
                svStyle.user.active.background = GYATexture.texSelector;
                svStyle.standard.hover.background = GYATexture.texSelector;
                svStyle.standard.active.background = GYATexture.texSelector;
                svStyle.old.hover.background = GYATexture.texSelector;
                svStyle.old.active.background = GYATexture.texSelector;
                svStyle.oldToMove.hover.background = GYATexture.texSelector;
                svStyle.oldToMove.active.background = GYATexture.texSelector;
                svStyle.project.hover.background = GYATexture.texSelector;
                svStyle.project.active.background = GYATexture.texSelector;
                svStyle.seperator.normal.background = GYATexture.texDivider;

                // Pro Skin
                if (GYAExt.IsProSkin)
                {
                    // Pro Colors Enabled
                    if (gyaVars.Prefs.enableColors)
                    {
                        if (infoStyle != null)
                            infoStyle.normal.textColor = Tango.Aluminium2;

                        svStyle.d.normal.textColor = Tango.Aluminium2;

                        svStyle.store.normal.textColor = Tango.Chameleon1;
                        svStyle.store.hover.textColor = Tango.Aluminium2;
                        svStyle.store.active.textColor = Tango.Aluminium2;
                        svStyle.user.normal.textColor = Tango.SkyBlue1;
                        svStyle.user.hover.textColor = Tango.Aluminium2;
                        svStyle.user.active.textColor = Tango.Aluminium2;
                        svStyle.standard.normal.textColor = Tango.Plum1;
                        svStyle.standard.hover.textColor = Tango.Aluminium2;
                        svStyle.standard.active.textColor = Tango.Aluminium2;
                        svStyle.old.normal.textColor = Tango.Chocolate2;
                        svStyle.old.hover.textColor = Tango.Aluminium2;
                        svStyle.old.active.textColor = Tango.Aluminium2;
                        svStyle.oldToMove.normal.textColor = Tango.Alt.DarkRed1;
                        svStyle.oldToMove.hover.textColor = Tango.Aluminium2;
                        svStyle.oldToMove.active.textColor = Tango.Aluminium2;
                        svStyle.project.normal.textColor = Tango.Aluminium3;
                        svStyle.project.hover.textColor = Tango.Aluminium2;
                        svStyle.project.active.textColor = Tango.Aluminium2;
                        svStyle.seperator.normal.textColor = Tango.Alt.Gold;
                    }
                    else // Pro Colors Disabled
                    {
                        if (infoStyle != null)
                            infoStyle.normal.textColor = Tango.Aluminium2;

                        svStyle.d.normal.textColor = Tango.Aluminium2;
                        svStyle.store.normal.textColor = Tango.Aluminium3;
                        svStyle.store.hover.textColor = Tango.Aluminium2;
                        svStyle.store.active.textColor = Tango.Aluminium2;
                        svStyle.user.normal.textColor = Tango.Aluminium3;
                        svStyle.user.hover.textColor = Tango.Aluminium2;
                        svStyle.user.active.textColor = Tango.Aluminium2;
                        svStyle.standard.normal.textColor = Tango.Aluminium3;
                        svStyle.standard.hover.textColor = Tango.Aluminium2;
                        svStyle.standard.active.textColor = Tango.Aluminium2;
                        svStyle.old.normal.textColor = Tango.Aluminium3;
                        svStyle.old.hover.textColor = Tango.Aluminium2;
                        svStyle.old.active.textColor = Tango.Aluminium2;
                        svStyle.oldToMove.normal.textColor = Tango.Aluminium3;
                        svStyle.oldToMove.hover.textColor = Tango.Aluminium2;
                        svStyle.oldToMove.active.textColor = Tango.Aluminium2;
                        svStyle.project.normal.textColor = Tango.Aluminium3;
                        svStyle.project.hover.textColor = Tango.Aluminium2;
                        svStyle.project.active.textColor = Tango.Aluminium2;
                        svStyle.seperator.normal.textColor = Tango.Alt.Gold;
                    }
                }
                else // Free
                {
                    // Free Colors Enabled
                    if (gyaVars.Prefs.enableColors)
                    {
                        if (infoStyle != null)
                            infoStyle.normal.textColor = Color.black;

                        svStyle.d.normal.textColor = Tango.Aluminium1;
                        svStyle.store.normal.textColor = Tango.Alt.DarkGreen;
                        svStyle.store.hover.textColor = Tango.Aluminium1;
                        svStyle.store.active.textColor = Tango.Aluminium1;
                        svStyle.user.normal.textColor = Tango.Alt.DarkBlue;
                        svStyle.user.hover.textColor = Tango.Aluminium1;
                        svStyle.user.active.textColor = Tango.Aluminium1;
                        svStyle.standard.normal.textColor = Tango.Alt.DarkMagenta;
                        svStyle.standard.hover.textColor = Tango.Aluminium1;
                        svStyle.standard.active.textColor = Tango.Aluminium1;
                        svStyle.old.normal.textColor = Tango.Orange3;
                        svStyle.old.hover.textColor = Tango.Aluminium1;
                        svStyle.old.active.textColor = Tango.Aluminium1;
                        svStyle.oldToMove.normal.textColor = Tango.ScarletRed3;
                        svStyle.oldToMove.hover.textColor = Tango.Aluminium1;
                        svStyle.oldToMove.active.textColor = Tango.Aluminium1;
                        svStyle.project.normal.textColor = Color.black;
                        svStyle.project.hover.textColor = Tango.Aluminium1;
                        svStyle.project.active.textColor = Tango.Aluminium1;
                        svStyle.seperator.normal.textColor = Tango.Aluminium1;
                    }
                    else // Free Colors Disabled
                    {
                        if (infoStyle != null)
                            infoStyle.normal.textColor = Color.black;

                        svStyle.d.normal.textColor = Tango.Aluminium1;
                        svStyle.store.normal.textColor = Color.black;
                        svStyle.store.hover.textColor = Tango.Aluminium1;
                        svStyle.store.active.textColor = Tango.Aluminium1;
                        svStyle.user.normal.textColor = Color.black;
                        svStyle.user.hover.textColor = Tango.Aluminium1;
                        svStyle.user.active.textColor = Tango.Aluminium1;
                        svStyle.standard.normal.textColor = Color.black;
                        svStyle.standard.hover.textColor = Tango.Aluminium1;
                        svStyle.standard.active.textColor = Tango.Aluminium1;
                        svStyle.old.normal.textColor = Color.black;
                        svStyle.old.hover.textColor = Tango.Aluminium1;
                        svStyle.old.active.textColor = Tango.Aluminium1;
                        svStyle.oldToMove.normal.textColor = Color.black;
                        svStyle.oldToMove.hover.textColor = Tango.Aluminium1;
                        svStyle.oldToMove.active.textColor = Tango.Aluminium1;
                        svStyle.project.normal.textColor = Color.black;
                        svStyle.project.hover.textColor = Tango.Aluminium1;
                        svStyle.project.active.textColor = Tango.Aluminium1;
                        svStyle.seperator.normal.textColor = Tango.Aluminium1;
                    }
                }

#if EnableZiosEditorThemeTweaks
                if (tbStyle.d != null) // Verify initialized
                {
                    // Text coloring - force required for Zios
                    if (GYAExt.IsProSkin)
                    {
                        tbStyle.d.normal.textColor = Tango.Aluminium3;
                        tbStyle.button.normal.textColor = Tango.Aluminium3;
                        tbStyle.dropdown.normal.textColor = Tango.Aluminium3;
                        tbStyle.group.normal.textColor = Tango.Aluminium3;
                        tbStyle.tb.normal.textColor = Tango.Aluminium3;
                    }
                    else
                    {
                        tbStyle.d.normal.textColor = Color.black;
                        tbStyle.button.normal.textColor = Color.black;
                        tbStyle.dropdown.normal.textColor = Color.black;
                        tbStyle.group.normal.textColor = Color.black;
                        tbStyle.tb.normal.textColor = Color.black;
                    }

                    tbStyle.button = EditorStyles.toolbarButton;
                    tbStyle.button.alignment = TextAnchor.MiddleCenter;
                    tbStyle.button.fontSize = 10;
                    tbStyle.dropdown = EditorStyles.toolbarDropDown;
                    tbStyle.dropdown.alignment = TextAnchor.MiddleCenter;
                    tbStyle.dropdown.fontSize = 10;
                    tbStyle.tb = EditorStyles.toolbar;
                    tbStyle.tb.alignment = TextAnchor.MiddleCenter;
                    tbStyle.tb.fontSize = 9;
                }
#endif

            }
        }

        // Check if Unity Skin has changed
        internal bool GUISkinHasChanged
        {
            get
            {
                // Current state of the Pro skin
                GUISkinChangedCurrent = GYAExt.IsProSkin ? UnityGUISkin.Pro : UnityGUISkin.NonPro;
                if (GUISkinChangedLast != GUISkinChangedCurrent)
                {
                    GUISkinChangedLast = GUISkinChangedCurrent;
                    return true;
                }

#if EnableZiosEditorThemeTweaks
                bool ZiosEditorThemeIsDarkCurrent = GYAExt.IsZiosEditorThemeDark;
                if (ZiosEditorThemeIsDark != ZiosEditorThemeIsDarkCurrent)
                {
                    ZiosEditorThemeIsDark = ZiosEditorThemeIsDarkCurrent;
                    return true;
                }
#endif
                return false;
            }
        }

        internal void SetStylesTB()
        {
            if (infoStyle == null)
            {
                infoStyle = new GUIStyle(GUI.skin.GetStyle("HelpBox"));

                infoStyle.richText = true;
                infoStyle.wordWrap = true;
                infoStyle.fontSize = 10;
                infoStyle.normal.background = GYATexture.GetUnitySkinTexture();
            }
            infoStyle.wordWrap = true; // For some reason wordwrap is being lost in the infobox randomly

            // ToolBar Style
            if (tbStyle.d == null)
            {
                tbStyle.d = new GUIStyle(EditorStyles.toolbar)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 9
                };
            }

            if (tbStyle.dropdown == null)
            {
                // 2nd Toolbar Style
                tbStyle.dropdown = new GUIStyle(EditorStyles.toolbarDropDown)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 10
                };
            }

            if (tbStyle.group == null)
            {
                // 3rd Toolbar Style
                tbStyle.group = new GUIStyle(EditorStyles.toggleGroup)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 10
                };
            }

            if (tbStyle.button == null)
            {
                // Toolbar Button Style
                tbStyle.button = new GUIStyle(EditorStyles.toolbarButton)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 10
                };
            }

            if (tbStyle.tb == null)
            {
                // Toolbar Style
                tbStyle.tb = new GUIStyle(EditorStyles.toolbar)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 9
                };
            }
        }

        internal void SetStyles()
        {
            int fontSize = 12; // 12 has been the defualt in the past
#if UNITY_2019_GUI_TWEAKS
	        fontSize = 12; // 11 is a good default for 2019
#endif

            //fontSize = 12; // Force-override
            int fontSizeSeperator = 10;
            int fixedHeight = 14;

            if (svStyle.icon == null)
            {
                svStyle.icon = new GUIStyle
                {
                    alignment = TextAnchor.MiddleRight,
                    fixedHeight = fixedHeight,
                    normal = { background = GYATexture.texTransparent },
                    hover = { background = GYATexture.texTransparent },
                    active = { background = GYATexture.texTransparent }
                };
            }

            if (svStyle.iconLeft == null)
            {
                svStyle.iconLeft = new GUIStyle
                {
                    alignment = TextAnchor.MiddleLeft,
                    fixedHeight = fixedHeight,
                    normal = { background = GYATexture.texTransparent },
                    hover = { background = GYATexture.texTransparent },
                    active = { background = GYATexture.texTransparent }
                };
            }

            if (svStyle.d == null)
            {
                // Asset style based on default (SVDrawLine SVFoldOut)
                svStyle.d = new GUIStyle
                {
                    alignment = TextAnchor.MiddleLeft,
                    fontSize = fontSize,
                    normal = { background = GYATexture.texTransparent }
                };
            }

            if (svStyle.store == null)
            {
                svStyle.store = new GUIStyle
                {
                    alignment = TextAnchor.MiddleLeft,
                    fontSize = fontSize,
                    normal = { background = GYATexture.texTransparent }
                    //imagePosition = ImagePosition.ImageLeft
                };
            }

            if (svStyle.standard == null)
            {
                svStyle.standard = new GUIStyle
                {
                    alignment = TextAnchor.MiddleLeft,
                    fontSize = fontSize,
                    normal = { background = GYATexture.texTransparent }
                };
            }

            if (svStyle.old == null)
            {
                svStyle.old = new GUIStyle
                {
                    alignment = TextAnchor.MiddleLeft,
                    fontSize = fontSize,
                    normal = { background = GYATexture.texTransparent }
                };
            }

            if (svStyle.oldToMove == null)
            {
                svStyle.oldToMove = new GUIStyle
                {
                    alignment = TextAnchor.MiddleLeft,
                    fontSize = fontSize,
                    normal = { background = GYATexture.texTransparent }
                };
            }

            if (svStyle.user == null)
            {
                svStyle.user = new GUIStyle
                {
                    alignment = TextAnchor.MiddleLeft,
                    fontSize = fontSize,
                    normal = { background = GYATexture.texTransparent }
                };
            }

            if (svStyle.project == null)
            {
                svStyle.project = new GUIStyle
                {
                    alignment = TextAnchor.MiddleLeft,
                    fontSize = fontSize,
                    normal = { background = GYATexture.texTransparent }
                };
            }

            if (svStyle.seperator == null)
            {
                // Category style
                svStyle.seperator = new GUIStyle
                {
                    alignment = TextAnchor.MiddleLeft,
                    fontSize = fontSizeSeperator,
                    fontStyle = FontStyle.Bold
                };
            }
        }

        internal string GetPathOfThisScript(bool returnFullPath = true)
        {
            MonoScript script = MonoScript.FromScriptableObject(this);
            string scriptPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(script));

            return returnFullPath ? Path.GetFullPath(scriptPath) : scriptPath;
        }

        // BEGIN - Methods to remove Diacritics
        static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(System.Text.NormalizationForm.FormD);
            var stringBuilder = new System.Text.StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            var fixedString = stringBuilder.ToString().Normalize(System.Text.NormalizationForm.FormC);

            return fixedString;
        }

        public partial class ProgressBar : IDisposable
        {
            string title;
            float stepCount;
            Func<int, string> infoFormatter;
            long refreshIntervalTicks;
            long nextUpdateTicks;
            bool showProgressBar;
            bool canBeCancelled;

            public ProgressBar(
                string title,
                int stepCount,
                int refreshIntervalMilliseconds,
                Func<int, string> infoFormatter,
                bool showProgressBar = true,
                bool canBeCancelled = false)
            {
                if (string.IsNullOrEmpty(title))
                    title = "Progress";

                if (title.Length > 58)
                {
                    title = string.Format(
                        "{0}...{1}",
                        title.Substring(0, 33),
                        title.Substring(title.Length - 22)
                    );
                }

                this.title = title;
                this.stepCount = stepCount;
                this.refreshIntervalTicks = (TimeSpan.TicksPerMillisecond * refreshIntervalMilliseconds);
                if (infoFormatter != null)
                {
                    this.infoFormatter = infoFormatter;
                }
                else
                {
                    this.infoFormatter = stepNumber =>
                        string.Format("Step {0}", (stepNumber + 1));
                }
                this.showProgressBar = showProgressBar;
                this.canBeCancelled = canBeCancelled;
            }

            public void Update(int stepNumber, string pText = "")
            {
                if (string.IsNullOrEmpty(title))
                    title = String.Empty;
                if (showProgressBar && !GYAExt.IsIntNull(stepNumber))
                {
                    long currentTicks = DateTime.UtcNow.Ticks;
                    if (nextUpdateTicks <= currentTicks)
                    {
                        nextUpdateTicks = (currentTicks + refreshIntervalTicks);
                        if (canBeCancelled)
                        {
                            EditorUtility.DisplayCancelableProgressBar(
                                title,
                                infoFormatter(stepNumber) + pText,
                                (stepNumber / stepCount)
                            );
                        }
                        else
                        {
                            EditorUtility.DisplayProgressBar(
                                title,
                                infoFormatter(stepNumber) + pText,
                                (stepNumber / stepCount)
                            );
                        }
                    }
                }
            }

            public void Dispose()
            {
                EditorUtility.ClearProgressBar();
            }
        }
    }
}
