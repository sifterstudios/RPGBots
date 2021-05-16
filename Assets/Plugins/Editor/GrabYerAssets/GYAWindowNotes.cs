using UnityEngine;
using UnityEditor;
using System;


namespace XeirGYA
{

    public class GYAWindowNotes : EditorWindow
    {
        public GYAVars gyaVars = GYA.gyaVars;	
        public GYAData.Asset svCurrentPkg;
        public int svCurrentPkgNumber;
        public ASPurchased.Result pur = null;
        public string noteTemp;

        internal Vector2 svPosition;

        internal static int toolbarInt; // Set default tab when opening window
        public string[] toolbarStrings = { "Package Data (Raw)" };

        public static void Init(int pVal = 1)
        {
            toolbarInt = pVal;
            bool isUtilityWindow = true;

            float width = 700f;
            float height = 320f;

            var winTitle = "GYA Asset Info:";
            var window = (GYAWindowNotes)EditorWindow.GetWindow(typeof(GYAWindowNotes), isUtilityWindow, winTitle, true);
            window.minSize = new Vector2(width, height);
            window.maxSize = new Vector2(width, height);
        }

        public void OnEnable()
        {
            svCurrentPkg = GYA.svCurrentPkg;
            svCurrentPkgNumber = GYA.Instance.svCurrentPkgNumber;
            noteTemp = svCurrentPkg.AssetInfo.Notes;
            if (string.IsNullOrEmpty(svCurrentPkg.filePath))
                this.Close();
        }

        public void OnInspectorUpdate()
        {

        }

        void OnDestroy()
        {
        }

        void OnGUI()
        {
            switch (toolbarInt)
            {
                case 0:
                    ShowAssetInfo();
                    break;
                default:
                    ShowAssetInfo();
                    break;
            }
        }

        void UpdateNote(int id)
        {
            svCurrentPkg.AssetInfo.id = svCurrentPkg.id;
            svCurrentPkg.AssetInfo.Notes = noteTemp;

            if (svCurrentPkg.AssetInfo.id == 0)
            {
                svCurrentPkg.AssetInfo.filePath = svCurrentPkg.filePath;
                GYA.gyaData.Assets[svCurrentPkgNumber].AssetInfo = svCurrentPkg.AssetInfo;
            }
            else
            {
                GYA.gyaData.Assets.FindAll(x => x.id == id).ForEach(y => y.AssetInfo = svCurrentPkg.AssetInfo);
            }

            GYAFile.SaveGYAUserData();
            this.Close();
        }

        void ShowAssetInfo()
        {
            ShowPackageInfo();

            EditorGUILayout.LabelField("Asset ID:\t\t\t" + svCurrentPkg.id);
            EditorGUILayout.LabelField("Asset Name (PKG):\t" + svCurrentPkg.title);
            if (pur != null) EditorGUILayout.LabelField("Asset Name (PAL):\t" + pur.name);

            EditorGUILayout.PrefixLabel("");
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical(GUILayout.Width(345));
            ShowPackagesGYA();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUILayout.Width(345));

            // Show notes field
            // -- Begin SV

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Asset Note:");
            if (GUILayout.Button("Save Note"))
            {
                UpdateNote(svCurrentPkg.id);
            }
            EditorGUILayout.EndHorizontal();

            GUIStyle noteStyle = GUI.skin.GetStyle("TextArea");
            noteStyle.wordWrap = true;
            noteStyle.stretchHeight = true;

            svPosition = EditorGUILayout.BeginScrollView(svPosition, GUILayout.Height(200));
            noteTemp = EditorGUILayout.TextArea(noteTemp, noteStyle);
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        public void ShowPackageInfo()
        {
            pur = null;
            ShowPackagesPurchased();
        }

        public void ShowPackagesGYA()
        {
            string infoString =
                //"ID:\t\t" + svCurrentPkg.id +
                //"\nName:\t\t" + svCurrentPkg.title +
                "Unity Version:\t" + svCurrentPkg.unity_version +
                //"\nUpload ID:\t" + svCurrentPkg.upload_id +
                "\nVersion:\t\t" + svCurrentPkg.version +
                //"\nVersion ID:\t" + svCurrentPkg.version_id +
                "\n\nDate Package:\t" + svCurrentPkg.fileDataCreated +
                "\nDate File:  \t" + svCurrentPkg.fileDateCreated +
                "\n\nCategory ID:\t" + svCurrentPkg.category.id +
                "\nCategory Name:\t" + svCurrentPkg.category.label +
                "\nPublisher ID:\t" + svCurrentPkg.publisher.id +
                "\nPublisher Name:\t" + svCurrentPkg.publisher.label +
                //"\nLink ID:\t\t" + svCurrentPkg.link.id +
                //"\nLink Type:\t\t" + svCurrentPkg.link.type +
                //"\n\nCollection:\t" + svCurrentPkg.collection +
                "\nIs Deprecated:\t" + svCurrentPkg.isDeprecated +
                //"\n\nIs In a Group:\t" + svCurrentPkg.isInAGroup +
                //"\nIs Locked Version:\t" + svCurrentPkg.isInAGroupLockedVersion +
                "\n\nFile Size:\t\t" + svCurrentPkg.fileSize.BytesToKB() +
                "\nFile Path:\t\t" + svCurrentPkg.filePath;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Copy To Clipboard:  ");
            if (GUILayout.Button("PKG Info"))
            {
                GYAFile.CopyToClipboard(svCurrentPkg);
            }
            if (pur != null)
            {
                if (GUILayout.Button("PAL Info"))
                {
                    GYAFile.CopyToClipboard(pur);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.HelpBox(infoString, MessageType.None);
        }

        public void ShowPackagesPurchased()
        {
            if (GYA.asPurchased.results != null)
            {
                pur = GYA.asPurchased.results.Find(x =>
                        (x.id == svCurrentPkg.id)
                );
            }
        }

        public void ShowPackagesLocal()
        {
            // asPackages id can be either int (ID number) OR string (filePath)
            ASPackageList.Result pkg = null;
            if (GYA.asPackages.results != null)
            {
                // Exported = path, AS = id
                if (svCurrentPkg.isExported)
                    pkg = GYA.asPackages.results.Find(x => x.id == svCurrentPkg.filePath);
                else
                    pkg = GYA.asPackages.results.Find(x =>
                        (GYAExt.IntOrZero(x.id) == svCurrentPkg.id)
                    );
            }

            EditorGUILayout.LabelField("Packages (Local) Info:");

            if (pkg != null)
            {
                EditorGUILayout.HelpBox(
                    "ID:\t\t" + pkg.id +
                    "\nTitle:\t\t" + pkg.title +
                    "\nVersion:\t\t" + pkg.version +
                    "\nVersion ID:\t" + pkg.version_id +
                    "\nPublishe Date:\t" + pkg.pubdate +
                    // root of each is null for standard assets
                    "\n\nLink ID:\t\t" + (pkg.link != null ? pkg.link.id : null) +
                    "\nLink Type:\t\t" + (pkg.link != null ? pkg.link.type : null) +
                    "\nCategory ID:\t" + (pkg.category != null ? pkg.category.id : null) +
                    "\nCategory Label:\t" + (pkg.category != null ? pkg.category.label : null) +
                    "\nPublisher ID:\t" + (pkg.publisher != null ? pkg.publisher.id : null) +
                    "\nPublisher Label:\t" + (pkg.publisher != null ? pkg.publisher.label : null) +
                    "\n\nLocal Path:\t" + pkg.local_path +
                    "", MessageType.None);
            }
        }
    }

}