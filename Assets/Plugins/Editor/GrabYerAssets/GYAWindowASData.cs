using UnityEngine;
using UnityEditor;

namespace XeirGYA
{
    public class GYAWindowASData : EditorWindow
    {
        public GYAVars gyaVars = GYA.gyaVars;	
        internal static int toolbarInt; // Set default tab when opening window
        public string[] toolbarStrings = { "Package Data (Raw)" };

        public static void Init(int pVal = 1)
        {
            toolbarInt = pVal;
            bool isUtilityWindow = true;

            float width = 700f;
            float height = 520f;

            var window = (GYAWindowASData)EditorWindow.GetWindow(typeof(GYAWindowASData), isUtilityWindow, "GYA Asset Data", true);
            window.minSize = new Vector2(width, height);
            window.maxSize = new Vector2(width, height);
            window.CenterOnMainWin();
        }

        public void OnInspectorUpdate()
        {
        }

        void OnDestroy()
        {
            GYA.Instance.Focus();
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

            this.Repaint();
            GYA.Instance.Focus();
        }

        void ShowAssetInfo()
        {
            EditorGUILayout.PrefixLabel("");
            EditorGUILayout.LabelField(GYA.svCurrentPkg.title);

            EditorGUILayout.PrefixLabel("");
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical(GUILayout.Width(345));
            ShowPackagesGYA();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUILayout.Width(345));
            ShowPackagesPurchased();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.PrefixLabel("");
        }

        public void ShowPackagesGYA()
        {
            string infoString =
                "ID:\t\t" + GYA.svCurrentPkg.id +
                "\nName:\t\t" + GYA.svCurrentPkg.title +
                "\nUnity Version:\t" + GYA.svCurrentPkg.unity_version +
                "\nUpload ID:\t" + GYA.svCurrentPkg.upload_id +
                "\nVersion:\t\t" + GYA.svCurrentPkg.version +
                "\nVersion ID:\t" + GYA.svCurrentPkg.version_id +
                "\n\nDate Package:\t" + GYA.svCurrentPkg.fileDataCreated +
                "\nDate File:  \t" + GYA.svCurrentPkg.fileDateCreated +
                "\n\nCategory ID:\t" + GYA.svCurrentPkg.category.id +
                "\nCategory Name:\t" + GYA.svCurrentPkg.category.label +
                "\nPublisher ID:\t" + GYA.svCurrentPkg.publisher.id +
                "\nPublisher Name:\t" + GYA.svCurrentPkg.publisher.label +
                "\nLink ID:\t\t" + GYA.svCurrentPkg.link.id +
                "\nLink Type:\t\t" + GYA.svCurrentPkg.link.type +
                "\n\nCollection:\t" + GYA.svCurrentPkg.collection +
                "\nIs Deprecated:\t" + GYA.svCurrentPkg.isDeprecated +
                "\n\nIs In a Group:\t" + GYA.svCurrentPkg.isInAGroup +
                "\nIs Locked Version:\t" + GYA.svCurrentPkg.isInAGroupLockedVersion +
                "\n\nFile Size:\t\t" + GYA.svCurrentPkg.fileSize.BytesToKB() +
                "\n\nFile Path:\t\t" + GYA.svCurrentPkg.filePath;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("GYA Package Info:  ");
            if (GUILayout.Button("Copy To Clipboard"))
            {
                GYAFile.CopyToClipboard(GYA.svCurrentPkg);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.HelpBox(infoString, MessageType.None);
        }

        public void ShowPackagesPurchased()
        {
            ASPurchased.Result pur = null;
            var tags = "";
            string infoString = "";

            if (GYA.asPurchased.results != null)
            {
                pur = GYA.asPurchased.results.Find(x =>
                        (x.id == GYA.svCurrentPkg.id)
                );
                if (pur != null) tags = GYAExt.ToJson(pur.tags);
            }

            if (pur != null)
            {
                infoString =
                    "ID:\t\t" + pur.id +
                    "\nName:\t\t" + pur.name +
                    "\nLocal Ver Name:\t" + pur.local_version_name +
                    "\nUser Rating:\t" + pur.user_rating + // int, 0-5
                    "\nStatus:\t\t" + pur.status + // string, published, deprecated
                    "\nIn Users Downloads:\t" + pur.in_users_downloads +
                    "\n\nDate Purchased:\t" + pur.purchased_at + // date, 
                    "\nDate Created:\t" + pur.created_at +
                    "\nDate Published:\t" + pur.published_at +
                    "\nDate Updated:\t" + pur.updated_at +
                    "\nLast Downloaded:\t" + pur.last_downloaded_at +
                    "\n\nCategory ID:\t" + pur.category.id +
                    "\nCategory Name:\t" + pur.category.name +
                    "\nPublisher ID:\t" + pur.publisher.id +
                    "\nPublisher Name:\t" + pur.publisher.name +
                    "\n\nCan Download:\t" + pur.can_download.ToBool() + // int, 1 true, 0 false
                    "\nCan Update:\t" + pur.can_update.ToBool() +
                    "\nComplete Project:\t" + pur.is_complete_project.ToBool() +
                    "\n\nType:\t\t" + pur.type +
                    "\nTags:\t\t" + tags +
                    "\n\nLocal Path:\t" + pur.local_path +
                    "\n\nIcon:\t" + pur.icon;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Purchased Asset Info:");
            if (pur != null)
            {
                if (GUILayout.Button("Copy To Clipboard"))
                {
                    GYAFile.CopyToClipboard(pur);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.HelpBox(pur != null ? infoString : "N/A", MessageType.None);
        }

        public void ShowPackagesLocal()
        {
            // asPackages id can be either int (ID number) OR string (filePath)
            ASPackageList.Result pkg = null;
            if (GYA.asPackages.results != null)
            {
                // Exported = path, AS = id
                if (GYA.svCurrentPkg.isExported)
                    pkg = GYA.asPackages.results.Find(x => x.id == GYA.svCurrentPkg.filePath);
                else
                    pkg = GYA.asPackages.results.Find(x =>
                        (GYAExt.IntOrZero(x.id) == GYA.svCurrentPkg.id)
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