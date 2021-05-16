//#define ENABLE_PAL

using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;

namespace XeirGYA
{
    public class GYAWindowPrefs : EditorWindow
    {
#if UI_Colors
        public string colorHexStore = GYA.gyaVars.Prefs.uiColor.store;
        public string colorHexStandard = GYA.gyaVars.Prefs.uiColor.standard;
        public string colorHexUser = GYA.gyaVars.Prefs.uiColor.user;
        public string colorHexOld = GYA.gyaVars.Prefs.uiColor.old;
        public string colorHexOldToMove = GYA.gyaVars.Prefs.uiColor.oldToMove;
        public string colorHexProject = GYA.gyaVars.Prefs.uiColor.project;
#endif

        internal Vector2 svPosition;
        internal static int toolbarInt; // Set default tab when opening window

        public string[] toolbarStrings =
            {"Quick Ref", "Preferences", "User Folders", "Groups", "Renaming", "Maintenance", "Info"};

        internal string kharmaSessionID = "";

        string userVersionString = GYA.gyaVars.Prefs.userVersionString;
        //GYAData.Asset verTestPackageGYA = GYAPackage.GetAssetByID(72902);

        public static void Init(int pVal = 1)
        {
            toolbarInt = pVal;

            float width = 700f;
            float height = 520f;

            var window = (GYAWindowPrefs)GetWindow(typeof(GYAWindowPrefs), true, "GrabYerAssets Preferences", true);
            window.minSize = new Vector2(width, height);
            window.maxSize = new Vector2(width, height);
            window.CenterOnMainWin();
        }

        void OnDestroy()
        {
            GYAFile.SaveGYAPrefs();
            GYA.Instance.Focus();
        }

        void OnGUI()
        {
            toolbarInt = GUILayout.Toolbar(toolbarInt, toolbarStrings, GUILayout.Height(26f));
            switch (toolbarInt)
            {
                case 0:
                    ShowQuickRef();
                    break;
                case 1:
                    ShowPreferences();
                    break;
                case 2:
                    ShowUserFolders();
                    break;
                case 3:
                    ShowGroups();
                    break;
                case 4:
                    ShowRenaming();
                    break;
                case 5:
                    ShowMaintenance();
                    break;
                case 6:
                    ShowStatus();
                    break;
                default:
                    break;
            }
        }

        void ShowQuickRef()
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical(GUILayout.Width(340));

            EditorGUILayout.HelpBox("General Usage:\n" +
                                    "\nRefresh Icon\t= Click to rescan your downloaded packages" +
                                    "\nLeft-Click  \t= Toggle asset for multi-import" +
                                    "\nRight-Click\t= Display asset specific popup", MessageType.None);
            EditorGUILayout.PrefixLabel("");
            EditorGUILayout.HelpBox("Color Chart for Collections:\n" +
                                    "\nGreen\t= Asset Store Assets" +
                                    "\nBlue\t= User Assets" +
                                    "\nPlum\t= Standard Assets" +
                                    "\nOrange\t= Old Assets that HAVE been consolidated" +
                                    "\nRed\t= Old Assets that HAVE NOT been consolidated", MessageType.None);
            EditorGUILayout.PrefixLabel("");
            EditorGUILayout.HelpBox("Icons In List (LEFT Side):\n" +
                                    "\nSolid\t= Unity Assets for the Current Running Version" +
                                    "\nOutline\t= Unity Assets for Older Unity Versions" +
                                    "\n\nCube\t= Asset Store Asset" +
                                    "\nPerson\t= User Asset (Contained within a User Folder)" +
                                    "\nPuzzle\t= Standard Asset" +
                                    "\nCircle\t= Old Asset" +
                                    "\n\nPinned Icons in List (RIGHT Side):\n" +
                                    "\nYellow Star\t= Favorite (is part of the Favorites group)" +
                                    "\nOrange Hazard\t= Deprecated" +
                                    "\nGreen Down Arrow = Not Downloaded" +
                                    "\nRed Warning\t= Damaged" +
                                    "\n\nNOTE: The version of Unity that a package was submitted with does not necessarily relate to what versions of Unity that package may support."
                , MessageType.None);

            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical(GUILayout.Width(340));

            EditorGUILayout.HelpBox("1st Row Icons:\n" +
                                    "\nGear\t= Main Menu" +
                                    "\nPages\t= Categories Drop-down" +
                                    "\nPeople\t= Publishers Drop-down" +
                                    "\nMagnifier\t= Sort & Search Drop-down" +
                                    "\nTextfield\t= Search for Assets" +
                                    "\nRefresh\t= Refresh the Package List", MessageType.None);

            EditorGUILayout.PrefixLabel("");
            EditorGUILayout.HelpBox("2nd Row Icons:\n" +
                                    "\n#\t= Multi-Asset Drop-down" +
                                    "\nReset\t= Reset main view back to defaults" +
                                    "\nTitle Bar\t= Collection/Group Drop-down" +
                                    "\nUp\t= Move Up a Collection/Group" +
                                    "\nDown\t= Move Down a Collection/Group", MessageType.None);

            EditorGUILayout.PrefixLabel("");
            EditorGUILayout.HelpBox("3rd Row Icons:\n" +
                                    "\n#\t= Assets in current list" +
                                    "\nIcons\t= In order: All, Store, User, Standard, Old" +
                                    "\n\nSelect an icon from above to quickly change to a selected collection",
                MessageType.None);

            EditorGUILayout.PrefixLabel("");
            EditorGUILayout.HelpBox("Bottom Row Foldout:\n" +
                                    "\nShows general package info for the hi-lighted asset." +
                                    "\n\nMenu Items:\n" +
                                    "\n* = 'Purchased Assets' list required to function", MessageType.None);

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.PrefixLabel("");
        }

        void ShowPreferences()
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical();

            EditorGUILayout.LabelField("Application Options");
            EditorGUI.indentLevel++;

            // Disable for Linux until proper handling for the 'Standard Assets' folder
            if (GYAExt.IsOSLinux)
                GYA.gyaVars.Prefs.isPersist = false;

            GYA.gyaVars.Prefs.autoUpdateGYA = EditorGUILayout.ToggleLeft("Auto-Update GYA", GYA.gyaVars.Prefs.autoUpdateGYA);
            //GYA.gyaVars.Prefs.promptBeforeUpdateGYA = EditorGUILayout.ToggleLeft("Prompt for Update", GYA.gyaVars.Prefs.promptBeforeUpdateGYA);
            GYA.gyaVars.Prefs.isPersist = EditorGUILayout.ToggleLeft("Persist Mode", GYA.gyaVars.Prefs.isPersist);

            GYA.gyaVars.Prefs.isSilent = EditorGUILayout.ToggleLeft("Hide Tooltips", GYA.gyaVars.Prefs.isSilent);
            bool bToolbarCollections = GYA.gyaVars.Prefs.enableToolbarCollections;
            GYA.gyaVars.Prefs.enableToolbarCollections = EditorGUILayout.ToggleLeft("Show Toolbar: Collections",
                GYA.gyaVars.Prefs.enableToolbarCollections);
            var bDarkMode = GYA.gyaVars.Prefs.forceDarkMode;
            GYA.gyaVars.Prefs.forceDarkMode = EditorGUILayout.ToggleLeft("Force Dark Mode", GYA.gyaVars.Prefs.forceDarkMode);
            EditorGUI.indentLevel--;

            EditorGUILayout.PrefixLabel("");
            EditorGUILayout.LabelField("Display Options");
            EditorGUI.indentLevel++;

            bool bHeaders = GYA.gyaVars.Prefs.enableHeaders;
            GYA.gyaVars.Prefs.enableHeaders = EditorGUILayout.ToggleLeft("Headers", GYA.gyaVars.Prefs.enableHeaders);

            // Enable/Disable Color
            var bColor = GYA.gyaVars.Prefs.enableColors;
            GYA.gyaVars.Prefs.enableColors = EditorGUILayout.ToggleLeft("Colors", GYA.gyaVars.Prefs.enableColors);

            EditorGUI.indentLevel++;

            // Color selection
#if UI_Colors
            var colorStore = EditorGUILayout.ColorField("Store", GYATexture.HexToColor(colorHexStore));
            GYA.gyaVars.Prefs.uiColor.store = colorHexStore = GYATexture.ColorToHex(colorStore);
            var colorStandard = EditorGUILayout.ColorField("Standard", GYATexture.HexToColor(colorHexStandard));
            GYA.gyaVars.Prefs.uiColor.standard = colorHexStandard = GYATexture.ColorToHex(colorStandard);
            var colorUser = EditorGUILayout.ColorField("User", GYATexture.HexToColor(colorHexUser));
            GYA.gyaVars.Prefs.uiColor.user = colorHexUser = GYATexture.ColorToHex(colorUser);
            var colorOld = EditorGUILayout.ColorField("Old", GYATexture.HexToColor(colorHexOld));
            GYA.gyaVars.Prefs.uiColor.old = colorHexOld = GYATexture.ColorToHex(colorOld);
            var colorOldToMove = EditorGUILayout.ColorField("OldToMove", GYATexture.HexToColor(colorHexOldToMove));
            GYA.gyaVars.Prefs.uiColor.oldToMove = colorHexOldToMove = GYATexture.ColorToHex(colorOldToMove);
            var colorProject = EditorGUILayout.ColorField("Project", GYATexture.HexToColor(colorHexProject));
            GYA.gyaVars.Prefs.uiColor.project = colorHexProject = GYATexture.ColorToHex(colorProject);
#endif

            EditorGUI.indentLevel--;

            // Enable/Disable Icons
            var bCollectionIcons = GYA.gyaVars.Prefs.enableCollectionTypeIcons;
            GYA.gyaVars.Prefs.enableCollectionTypeIcons =
                EditorGUILayout.ToggleLeft("Icons", GYA.gyaVars.Prefs.enableCollectionTypeIcons);

            // Enable/Disable Outline Icons
            var bAltIcon = GYA.gyaVars.Prefs.enableAltIconForOldVersions;
            GYA.gyaVars.Prefs.enableAltIconForOldVersions =
                EditorGUILayout.ToggleLeft("Alt Icons for Old Package Versions",
                    GYA.gyaVars.Prefs.enableAltIconForOldVersions);
            EditorGUI.indentLevel--;

            EditorGUILayout.PrefixLabel("");
            EditorGUILayout.LabelField("Asset Store Options");
            EditorGUI.indentLevel++;

#if ENABLE_PAL
	        GYA.gyaVars.Prefs.getPurchasedAssetsListDuringRefresh =
                EditorGUILayout.ToggleLeft("Retrieve Purchased Assets List",
                    GYA.gyaVars.Prefs.getPurchasedAssetsListDuringRefresh);
#endif

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ToggleLeft("", GYA.gyaVars.Prefs.autoPreventASOverwrite, GUILayout.Width(28));
            EditorGUI.EndDisabledGroup();

            // Enable/Disable Auto Prevent Asset Store Overwrite
            if (GUILayout.Button("Asset Overwrite Protection", GUILayout.ExpandWidth(false)))
            {
                GYA.Instance.TBPopUpCallback("AutoPreventASOverwrite");
                GUIUtility.ExitGUI();
            }
            EditorGUILayout.EndHorizontal();

            // Open (x) URLs Per Batch -
            GYA.gyaVars.Prefs.openURLInUnity =
                EditorGUILayout.ToggleLeft("Open URLs In Unity", GYA.gyaVars.Prefs.openURLInUnity);
            EditorGUILayout.PrefixLabel("");
            EditorGUILayout.LabelField("Open (x) URLs Per Batch");
            GYA.gyaVars.Prefs.urlsToOpenPerBatch =
                EditorGUILayout.IntSlider(GYA.gyaVars.Prefs.urlsToOpenPerBatch, 1, 30, GUILayout.MaxWidth(230));

            // Multi-import
            GYA.gyaVars.Prefs.multiImportOverride =
                (GYAImport.MultiImportType)EditorGUILayout.EnumPopup("Multi-Import Override:",
                    GYA.gyaVars.Prefs.multiImportOverride, GUILayout.MaxWidth(230));

            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical(GUILayout.Width(400));

            EditorGUILayout.PrefixLabel("");
            EditorGUILayout.HelpBox(
                "Auto-Update - Prompts to Update GYA if a new version has been downloaded."
                //+ "\nAuto-Update - Update GYA if a new version has been downloaded."
                //+ "\nPrompt for Update - GYA will prompt before updating if Auto-Update is enabled."
                + "\nPersist Mode - Maintains the latest version of GYA within the 'Standard Assets' folder for quickly adding GYA into a new project via 'Assets->Import Package'."
                + "\nHide Tooltips - Hide/Show tooltips."
                + "\nCollections Toolbar - Enable/Disable the Collections Toolbar"
                + "\nForce Dark Mode - Enable/Disable Dark UI where possible"
                , MessageType.None);
            EditorGUILayout.PrefixLabel("");
            EditorGUILayout.PrefixLabel("");

            EditorGUILayout.HelpBox(
                "Headers/Colors/Icons - Change appearance of the asset list." +
                "\n\nAlt Icons - Packages published with an older major Unity version will show an alternate icon."
                , MessageType.None);
            EditorGUILayout.PrefixLabel("");
            EditorGUILayout.PrefixLabel("");

            EditorGUILayout.HelpBox(
#if ENABLE_PAL
	            "Purchased Asset List (PAL) - Enables denoting deprecated assets and access to menu options marked with a (*) by downloading your Assets List."
                + "  This will be updated during 'Refresh' if GYA detects a change in the Asset Store folder."
	            + "\nNOTE: This is purely optional and NOT required for the normal use of GYA.\n\n"
	            + 
#endif
	            "Asset Overwrite Protection - Automatically rename all assets to include the version when refreshing to prevent updates from overwriting existing assets."
                + "\nThis ONLY affects official assets in the Asset Store folder."
                + "\n\nOpen URLs In Unity - Toggle between opening URLs in Unity or Browser."
                + "\nDoes not apply to 'Open URL of Selected Packages'"
                , MessageType.None);

            EditorGUILayout.HelpBox("How many URls to open when selecting 'Open URL of Selected Packages'"
                , MessageType.None);

            EditorGUILayout.HelpBox(
                "Multi Import Override - Override the method used to perform Multi-Import's."
                + "\'Default will select the best option for current version of Unity."
                + "\nGYA may override selected option if it is known to cause problems."
                , MessageType.None);

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.PrefixLabel("");

            // Update Main Window
            if (bHeaders != GYA.gyaVars.Prefs.enableHeaders)
                GYA.Instance.svMain.headerCount =
                    GYA.Instance.SVGetHeaderCount(); // Count optional lines/headers to draw

            if (bColor != GYA.gyaVars.Prefs.enableColors)
                GYA.Instance.CheckIfGUISkinHasChanged(true); // Force reload

            if (bDarkMode != GYA.gyaVars.Prefs.forceDarkMode)
                GYA.Instance.CheckIfGUISkinHasChanged(true); // Force reload

            if (
                bToolbarCollections != GYA.gyaVars.Prefs.enableToolbarCollections ||
                bHeaders != GYA.gyaVars.Prefs.enableHeaders ||
                bDarkMode != GYA.gyaVars.Prefs.forceDarkMode ||
                bColor != GYA.gyaVars.Prefs.enableColors ||
                bCollectionIcons != GYA.gyaVars.Prefs.enableCollectionTypeIcons ||
                bAltIcon != GYA.gyaVars.Prefs.enableAltIconForOldVersions
            )
            {
                GYA.Instance.Repaint();
                GYAFile.SaveGYAPrefs();
            }
        }

        // User Folders

        void ShowUserFolders()
        {
            int pathCount = GYA.gyaVars.Prefs.pathUserAssets.Count;

            //EditorGUILayout.PrefixLabel("");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();

            // change sorted assets Folder
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Sorted Assets Folder");
            // alwaysAskForSortedAssetsFolder
            var bAlwaysAskForSortedAssetsFolder = GYA.gyaVars.Prefs.alwaysAskForSortedAssetsFolder;
            GYA.gyaVars.Prefs.alwaysAskForSortedAssetsFolder = EditorGUILayout.ToggleLeft("Always Ask for Folder", GYA.gyaVars.Prefs.alwaysAskForSortedAssetsFolder);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUI.indentLevel++;
            EditorGUILayout.PrefixLabel("");
            EditorGUILayout.HelpBox(GYA.gyaVars.Prefs.pathSortedAssetsFolder, MessageType.None, true);

            if (GUILayout.Button("Change", GUILayout.ExpandWidth(false)))
            {
                string pathTemp =
                    EditorUtility.SaveFolderPanel(GYA.gyaVars.abbr + " Select Sorted Assets Folder:", null, null);
                if (pathTemp != "")
                {
                    GYA.gyaVars.Prefs.pathSortedAssetsFolder = pathTemp;
                    GYAFile.SaveGYAPrefs();
                    GUIUtility.ExitGUI();
                }
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.EndHorizontal();

            // Add new Folder
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("User Folders");
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUI.indentLevel++;
            if (GUILayout.Button("Add New Folder", GUILayout.ExpandWidth(false)))
            {
                string pathTemp =
                    EditorUtility.SaveFolderPanel(GYA.gyaVars.abbr + " Select User Assets Folder:", null, null);
                if (pathTemp != "")
                {
                    GYA.gyaVars.Prefs.pathUserAssets.Add("");
                    GYA.gyaVars.Prefs.pathUserAssets[pathCount] = pathTemp;
                    GYAFile.SaveGYAPrefs();
                    GUIUtility.ExitGUI();
                }
            }
            EditorGUILayout.PrefixLabel("");
            // Apply
            if (GUILayout.Button("Refresh User Folder", GUILayout.Width(130)))
            {
                GYAPackage.RefreshUser();
                GYA.Instance.RefreshSV();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
            EditorGUILayout.PrefixLabel("");

            // -- Begin SV

            // Linux beta error fix: NullReferenceException ??
            try
            {
                svPosition = EditorGUILayout.BeginScrollView(svPosition, false, false, GUILayout.ExpandWidth(true));
            }
            catch (Exception)
            {
            }


            for (int i = 0; i < pathCount; i++)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField((i + 1).ToString(), GUILayout.Width(40));
                // Change
                if (GUILayout.Button("Change", GUILayout.ExpandWidth(false)))
                {
                    string pathTemp = EditorUtility.SaveFolderPanel(GYA.gyaVars.abbr + " Select User Assets Folder:",
                        GYA.gyaVars.Prefs.pathUserAssets[i], "");
                    if (pathTemp != "")
                    {
                        GYA.gyaVars.Prefs.pathUserAssets[i] = pathTemp;
                        GYAFile.SaveGYAPrefs();
                        GUIUtility.ExitGUI();
                    }
                }

                // Show button if not last/empty entry
                if (GYA.gyaVars.Prefs.pathUserAssets[i] != "")
                {
                    if (GUILayout.Button("Remove", GUILayout.ExpandWidth(false)))
                    {
                        GYA.gyaVars.Prefs.pathUserAssets.RemoveAt(i);
                        GYAFile.SaveGYAPrefs();
                        continue;
                    }
                }
                EditorGUILayout.HelpBox(GYA.gyaVars.Prefs.pathUserAssets[i], MessageType.None);

                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
            }

            // Linux beta error fix: NullReferenceException ??
            try
            {
                EditorGUILayout.EndScrollView();
            }
            catch (Exception)
            {
            }

            // -- End SV

            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical(GUILayout.Width(280));

            EditorGUILayout.HelpBox("The first folder is considered the\nDEFAULT folder for 'Copy To User Folder'.",
                MessageType.Info);
            EditorGUILayout.HelpBox(
                "After adding any/all folders as desired, click the 'Refresh' button to scan the newly added folders.",
                MessageType.Info);
            EditorGUILayout.PrefixLabel("");

            EditorGUILayout.HelpBox(
                "It is *not* recommended to assign the 'Asset Store' Folder or a Sub-Folder within, as a User Folder." +
                "\n\nIt is *not* recommended to assign a User Folder within another User Folder." +
                "\n\nDoing either of these MAY result in unwanted duplicates in the list and have unforeseen consequences should you perform any File Actions on them." +
                "\n\nExample: Overlapping User folders COULD make you think you have duplicate files.  Deleting the duplicate in this case would likely have the consequence of actually deleting the original as they would be one in the same.",
                MessageType.Warning);

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.PrefixLabel("");

            if (bAlwaysAskForSortedAssetsFolder != GYA.gyaVars.Prefs.alwaysAskForSortedAssetsFolder)
            {
                GYAFile.SaveGYAPrefs();
            }
        }

        // Groups

        void ShowGroups()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();

            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("");
            // Add new Group
            if (GUILayout.Button("Add New Group", GUILayout.ExpandWidth(false)))
            {
                GYA.Instance.GroupCreate("New Group");
                GYA.Instance.BuildPrevNextList();
            }
            EditorGUILayout.PrefixLabel("");
            // Apply
            if (GUILayout.Button("Apply Changes", GUILayout.Width(100)))
            {
                GYA.Instance.BuildPrevNextList();
                GYAFile.SaveGYAGroups();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;

            // Field labels
            EditorGUILayout.PrefixLabel("");
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("", GUILayout.Width(62));
            EditorGUILayout.LabelField("Group Name:", GUILayout.Width(186));
            EditorGUILayout.LabelField("# of Assets:", GUILayout.Width(80));
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;

            // View/Edit all Groups
            svPosition = EditorGUILayout.BeginScrollView(svPosition, false, false, GUILayout.ExpandWidth(true));

            for (int i = 0; i < GYA.gyaData.Groups.Count; i++)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField((i).ToString(), GUILayout.Width(40));

                EditorGUI.BeginDisabledGroup((i == 0));
                // Remove
                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    if (EditorUtility.DisplayDialog(GYA.gyaVars.abbr + " - Remove Selected Group",
                        "Are you sure you want to REMOVE this group?\n\n" +
                        "No Assets will be deleted, only the virtual group.\n\n" +
                        GYA.gyaData.Groups[i].name
                        , "Cancel", "REMOVE"))
                    {
                        // Cancel as default - Do nothing
                    }
                    else
                    {
                        GYA.Instance.GroupDelete(i);
                        GYA.Instance.BuildPrevNextList();
                        GUIUtility.ExitGUI();
                    }
                }
                EditorGUI.EndDisabledGroup();

                // If Group then show Name & Count
                // Rename

                if (i <= GYA.gyaData.Groups.Count - 1)
                {
                    // Only allow these chars 
                    char chr = Event.current.character;
                    if (
                        (chr < 'a' || chr > 'z') && (chr < 'A' || chr > 'Z') && (chr < '0' || chr > '9') &&
                        (chr != ' ') && (chr != '(') && (chr != ')') && (chr != '-') && (chr != '+') && (chr != '_')
                        && (chr != '/') // grpsub: Allow '/' entry for submenus in groups
                    )
                    {
                        Event.current.character = '\0';
                    }
                    // Group Name
                    EditorGUI.BeginDisabledGroup((i == 0));
                    GYA.gyaData.Groups[i].name =
                        EditorGUILayout.TextField(GYA.gyaData.Groups[i].name, GUILayout.Width(200));
                    EditorGUI.EndDisabledGroup();

                    // # of Assets in Group [i]
                    EditorGUILayout.LabelField(GYA.gyaData.Groups[i].Assets.Count.ToString(), GUILayout.Width(64));
                }

                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical(GUILayout.Width(320));

            EditorGUILayout.HelpBox(
                "Accepted Characters: a-zA-Z0-9 ()-+_\nUse / to create sub-groups.\nRemoving a Group does NOT delete any assets.",
                MessageType.Info);

            EditorGUILayout.PrefixLabel("");
            EditorGUILayout.HelpBox(
                "Organize your unityPackages into Groups as needed.\n\n" +
                "You can also use a Group as a Project Group if desired, so that you know what packages and versions (see 'Version Locking' below) are used in a given project.",
                MessageType.None);

            EditorGUILayout.PrefixLabel("");
            EditorGUILayout.HelpBox(
                "'Version Locking' allows you to lock a specific version of an 'Asset Store' package to a group if required. (Example: When you know that a project requires a specific version of an asset.)\n\n" +
                "Enable/Disable 'Version Locking' for a package within a Group:\n" +
                "1) Right-Clicking on a package in the list and\n" + "2) Selecting 'Lock Version for this Group'\n\n" +
                "When viewing a Group, Right-Clicking on an asset and selecting 'Import' will either:\n" +
                "1) Import the latest version available OR\n" +
                "2) If it is 'Version Locked', it will import the assigned version.", MessageType.None);

            EditorGUILayout.PrefixLabel("");
            EditorGUILayout.HelpBox(
                "Sub-groups do not have to be in order. A Root-level group is not required and should not be made.\n\n" +
                "Only the outer most child groups will be accessible.\n\n" +
                "Usage:\nTopGroup/SubA\nTopGroup/SubB",
                MessageType.None);

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.PrefixLabel("");
        }

        // Renaming

        private T WithoutSelectAll<T>(Func<T> guiCall)
        {
            bool preventSelection = (Event.current.type == EventType.MouseDown);
            Color oldCursorColor = GUI.skin.settings.cursorColor;

            if (preventSelection)
                GUI.skin.settings.cursorColor = new Color(0, 0, 0, 0);

            T value = guiCall();

            if (preventSelection)
                GUI.skin.settings.cursorColor = oldCursorColor;

            return value;
        }

        void ShowRenaming()
        {
            string defaultVerString = String.Empty;
            GYAData.Asset verTestPackageGYA = GYAPackage.GetAssetByID(72902);

            //defaultVerString = " {version} ({unity_version}) ({version_id})"; // Default style old
            //defaultVerString = " (v.{version} r.{version_id} u.{unity_version})"; // Default style new 1 
            defaultVerString = " ({version}_{version_id}_{unity_version})"; // Default style new 2
            //defaultVerString = " {version} {version_id} {unity_version} {id} {upload_id} {pub_id} {pubDate} {fileDataCreated} {fileDateCreated}"; // Test string

            string resultDefaultVerString = GYAPackage.CustomizePackageFilenameVersionSuffix(verTestPackageGYA, defaultVerString);

            //if (string.IsNullOrEmpty(userVersionString))
            //    userVersionString = defaultVerString;
            ////GYA.gyaVars.Prefs.userVersionString = defaultVerString;

            //EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.PrefixLabel("");

            EditorGUILayout.LabelField(" ", "Custom version formatting for file renaming.");
            EditorGUILayout.PrefixLabel("");

            // Only allow these chars 
            char chr = Event.current.character;
            if (
                (chr < 'a' || chr > 'z') && (chr < 'A' || chr > 'Z') && (chr < '0' || chr > '9') &&
                (chr != ' ') && (chr != '(') && (chr != ')') && (chr != '-') && (chr != '+') && (chr != '_')
                && (chr != '{') && (chr != '}') && (chr != '@') && (chr != '.') //&& (chr != '%')
            )
            {
                Event.current.character = '\0';
            }
            userVersionString = EditorGUILayout.TextField("Version Parameters", userVersionString);
            //userVersionString = WithoutSelectAll(() => EditorGUILayout.TextField(userVersionString));

            EditorGUILayout.LabelField("Live Example", "'Grab Yer Assets" + GYAPackage.CustomizePackageFilenameVersionSuffix(verTestPackageGYA, userVersionString) + "'");


            EditorGUILayout.PrefixLabel("");
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Revert to Default", GUILayout.ExpandWidth(false)))
            {
                userVersionString = defaultVerString;
                GYA.gyaVars.Prefs.userVersionString = userVersionString;
                GYAFile.SaveGYAPrefs();
                GUIUtility.keyboardControl = 0;
            }
            EditorGUILayout.LabelField(" ", GUILayout.Width(40));
            if (GUILayout.Button("Save", GUILayout.ExpandWidth(false)))
            {
                GYA.gyaVars.Prefs.userVersionString = userVersionString;
                GYAFile.SaveGYAPrefs();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.PrefixLabel("");

            //EditorGUILayout.LabelField("WARNING:", "Please upgrade ALL projects to the latest version of GYA, otherwise");
            //EditorGUILayout.LabelField(" ", "this preference entry may be lost when running an older version.");
            //EditorGUILayout.PrefixLabel("");
            EditorGUILayout.LabelField("Warning:", "If the version string is not unique enough, duplicate filenames may result, causing a problem.");
            EditorGUILayout.LabelField(" ", "Unless you have a specific reason to change it, leave it at the default.");

            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();

            //EditorGUILayout.PrefixLabel(""); 
            EditorGUILayout.HelpBox(
                "Default string: (Do not include the brackets)\n\n" +
                "String    : '" + defaultVerString + "'\n" +
                //"Result    : [ v3.18.11.3001 (5.0.0f4) (405747)]\n" +
                //"Result    : [ (v.3.18.11.3001 r.405747 u.5.0.0f4)]\n" +
                "Result    : '" + resultDefaultVerString + "'\n" +
                //"Becomes: [Grab Yer Assets v3.18.11.3001 (5.0.0f4) (405747).unitypackage]\n" +
                //"Becomes: [Grab Yer Assets (v.3.18.11.3001 r.405747 u.5.0.0f4).unitypackage]\n" +
                "Becomes: 'Grab Yer Assets" + resultDefaultVerString + ".unitypackage'\n" +
                "\n" +
                "Available version strings:\n\n" +
                "Recommended:\t\t\t\tExample:\n" +
                "{version}\t\t- Pkg Version\t\t" + GYAPackage.CustomizePackageFilenameVersionSuffix(verTestPackageGYA, "{version}") + "\n" +
                "{version_id}\t- Version ID\t\t" + GYAPackage.CustomizePackageFilenameVersionSuffix(verTestPackageGYA, "{version_id}") + "\n" +
                "{unity_version}\t- Unity Version (*)\t\t" + GYAPackage.CustomizePackageFilenameVersionSuffix(verTestPackageGYA, "{unity_version}") + "\n" +
                "\nOptional:\n" +
                //"{title} - Pkg Title\n" +
                "{id}\t\t- Pkg ID\t\t\t" + GYAPackage.CustomizePackageFilenameVersionSuffix(verTestPackageGYA, "{id}") + "\n" +
                "{upload_id}\t- Upload ID (*)\t\t" + GYAPackage.CustomizePackageFilenameVersionSuffix(verTestPackageGYA, "{upload_id}") + "\n" +
                "{pub_id}\t\t- Publisher ID\t\t" + GYAPackage.CustomizePackageFilenameVersionSuffix(verTestPackageGYA, "{pub_id}") + "\n" +
                "{pub_date}\t- Publish Date\t\t" + GYAPackage.CustomizePackageFilenameVersionSuffix(verTestPackageGYA, "{pub_date}") + "\n" +
                //"{pub_dateTime}\t- Publish DateTime\n" +
                //"{date_updated}\t- Updated Date\n" +
                //"{date_created}\t- Creation Date (Pkg)\n" +
                "{pkg_created}\t- Creation Date (Pkg/Build)\t" + GYAPackage.CustomizePackageFilenameVersionSuffix(verTestPackageGYA, "{pkg_created}") + "\n" +
                "{file_created}\t- Creation Date (File) \t" + GYAPackage.CustomizePackageFilenameVersionSuffix(verTestPackageGYA, "{file_created}") + "\n" +
                "{sp}\t\t- Space",
                //"\n\nEvery official 'Asset Store' unitypackage seems to follow these rules:" +
                //"\nAll have version_id, most have upload_id",
                MessageType.None
            );

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUILayout.Width(280));
            //EditorGUILayout.PrefixLabel("");

            EditorGUILayout.HelpBox(
                "Select the parameters for customizing how files are renamed for all GYA related version/renaming features.\n" +
                "\nAccepted Characters: a-zA-Z0-9 ()-+_@.\n" +
                "\nTip:\n" +
                "Don't forget a space, '_', or '-' at the beginning to seperate the title from the version.\n" +
                "\nNotes:\n" +
                "The existing filename is NOT used, the package title is taken from the embedded JSON.\n" +
                "\n(* = Denotes that older pkg's may not have this)" +
                "\n{unity_version}\twould be 'Unknown'" +
                "\n{upload_id}\twould be '0'",
                MessageType.None
            );

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.PrefixLabel("");
        }

        // Maintenance

        void ShowMaintenance()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();

            // User File
            EditorGUILayout.LabelField("User File");
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("");
            if (GUILayout.Button("Backup", GUILayout.ExpandWidth(false)))
            {
                GYA.Instance.TBPopUpCallback("BackupUserFile");
                GUIUtility.ExitGUI();
            }
            if (GUILayout.Button("Restore", GUILayout.ExpandWidth(false)))
            {
                GYA.Instance.TBPopUpCallback("RestoreUserFile");
                GUIUtility.ExitGUI();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;

            // Export Asset Data

            EditorGUILayout.PrefixLabel("");
            EditorGUILayout.LabelField("Export Asset Data");
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("");
            if (GUILayout.Button("as CSV", GUILayout.ExpandWidth(false)))
            {
                GYA.Instance.TBPopUpCallback("ExportAsCSV");
                GUIUtility.ExitGUI();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;

            // Offline Mode

            EditorGUILayout.PrefixLabel("");
            EditorGUILayout.LabelField("Offline Mode");
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();
            GYA.gyaVars.Prefs.enableOfflineMode = EditorGUILayout.ToggleLeft("Enable",
                GYA.gyaVars.Prefs.enableOfflineMode, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;

            // Save Alt

            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("");
            if (GUILayout.Button("Save Alternate GYA Data File", GUILayout.ExpandWidth(false)))
            {
                string path = EditorUtility.SaveFilePanel(GYA.gyaVars.abbr + " - Save GYA Data File To:", "",
                    GYA.gyaVars.Files.Assets.fileName, "json");
                //string path = EditorUtility.SaveFolderPanel(GYA.gyaVars.abbr + " - Save GYA Data File To:", "", GYA.gyaVars.Files.Assets.fileName);

                if (path.Length != 0)
                {
                    File.Copy(GYA.gyaVars.Files.Assets.file, path);
                }
                GUIUtility.ExitGUI();
            }
            EditorGUILayout.EndHorizontal();

            // Load Alt

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("");
            if (GUILayout.Button("Load Alternate GYA Data File", GUILayout.ExpandWidth(false)))
            {
                string path = EditorUtility.OpenFilePanel(GYA.gyaVars.abbr + " Select User Assets Folder:", "", "json");

                if (path.Length != 0)
                {
                    // Auto Enable Offline Mode since we are loading a data file
                    GYA.gyaVars.Prefs.enableOfflineMode = true;
                    GYAFile.SaveGYAPrefs();

                    File.SetAttributes(GYA.gyaVars.Files.Assets.file, FileAttributes.Normal);
                    File.Copy(path, @GYA.gyaVars.Files.Assets.file, true);
                    GYAFile.LoadGYAAssets();
                    GYAPackage.RefreshProject();
                    GYA.Instance.RefreshSV();
                }
                GUIUtility.ExitGUI();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;

            // Asset Store Specific

            EditorGUILayout.PrefixLabel("");
            EditorGUILayout.LabelField("Asset Store Specific");
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("");
            if (GUILayout.Button("Clean 'Asset Store' Folder", GUILayout.ExpandWidth(false)))
            {
                GYA.Instance.TBPopUpCallback("DeleteEmptySubFolders");
                GUIUtility.ExitGUI();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("");
            if (GUILayout.Button("Remove Version from Filenames", GUILayout.ExpandWidth(false)))
            {
                GYA.Instance.TBPopUpCallback("RenameWithVersionRemovedCollection");
                GUIUtility.ExitGUI();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("");
            if (GUILayout.Button("Force Rebuild AS Folder", GUILayout.ExpandWidth(false)))
            {
                GYA.Instance.TBPopUpCallback("ForceRebuildFolderStore");
                GUIUtility.ExitGUI();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;

            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical(GUILayout.Width(400));

            EditorGUILayout.HelpBox(
                "Backup & Restore - Back or Restore the GYA Data files (Prefs & Groups) by making a copy of the files in GYA's Data Folder.",
                MessageType.None);

            EditorGUILayout.PrefixLabel("");
            EditorGUILayout.PrefixLabel("");
            EditorGUILayout.HelpBox(
                "Export Asset Data - Export the Asset List as a CSV data file for use in other programs.",
                MessageType.None);

            EditorGUILayout.PrefixLabel("");
            EditorGUILayout.PrefixLabel("");
            EditorGUILayout.HelpBox(
                "Offline Mode - This allows you to copy your '" + GYA.gyaVars.Files.Assets.fileName +
                "' over to another system that may not have your assets locally.  This way you can still browse your collection.\n\nNote: Importing and File functions will not be active in Offline Mode.",
                MessageType.None);

            EditorGUILayout.PrefixLabel("");
            EditorGUILayout.PrefixLabel("");
            EditorGUILayout.HelpBox(
                "Clean Asset Store Folder - Delete any Empty folders in the Asset Store folder."
                + "\n\nRemove Version from Filenames - Restore assets to their original filename.\nOnly affects assets within the Asset Store folder."
                + "\n\nForce Rebuild AS Folder - Rebuilds the Asset Store folder data."
                , MessageType.None);

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.PrefixLabel("");
        }

        // Status

        public string SessionData()
        {
            var kharmaToShow = "";

            // -- sessionID
            if (EditorPrefs.HasKey("kharma.remember_session"))
            {
                var kharmaRemember_session = EditorPrefs.GetString("kharma.remember_session");
                kharmaToShow += "kharmaRemember_session:\t" + (kharmaRemember_session == "1" ? true : false);
            }

            kharmaToShow += "\nkharmaSessionIDExists:\t" + EditorPrefs.HasKey("kharma.sessionid");

            var kharmaSessionID = "";
            if (EditorPrefs.HasKey("kharma.sessionid"))
            {
                kharmaSessionID = EditorPrefs.GetString("kharma.sessionid");
            }
            kharmaToShow += "\nkharmaSessionID:\t\t" + kharmaSessionID;

            var kharmaActiveSessionIDExists = (bool)GYAReflect.GetVal("UnityEditor.AssetStoreClient", "HasActiveSessionID");
            kharmaToShow += "\nkharmaActiveSessionIDExists:\t" + kharmaActiveSessionIDExists;

            var kharmaActiveSessionID = "";
            if (kharmaActiveSessionIDExists)
            {
                kharmaActiveSessionID = (string)GYAReflect.GetVal("UnityEditor.AssetStoreClient", "ActiveSessionID");
            }
            kharmaToShow += "\nkharmaActiveSessionID:\t" + kharmaActiveSessionID;

            kharmaToShow += "\nLogged in to Asset Store:\t" + (bool)GYAReflect.GetVal("UnityEditor.AssetStoreClient", "LoggedIn");

            Debug.Log(kharmaToShow);
            return kharmaToShow;
        }

        void ShowStatus()
        {
            GUIStyle infoStyle = GUI.skin.GetStyle("HelpBox");
            infoStyle.richText = true;

            var kharmaToShow = "";
#if ENABLE_PAL

            if (kharmaSessionID.Length == 0)
                kharmaSessionID = GYAFile.GetFromUnityCookies_KharmaSession();

            kharmaToShow += "The 'Purchased Assets List' (PAL) ";

            if (kharmaSessionID.Length > 0)
            {
                kharmaToShow += "should be accessible ..";
            }
            else
            {
                kharmaToShow += "is NOT currently accessible ..";
            }

            kharmaToShow += "\n\nSession Detected:\t\t" + (kharmaSessionID.Length > 0);

//#if TEST_PAL
            kharmaToShow += "\nCookie File Exists:\t\t" + File.Exists(GYAExt.PathUnityCookiesFile);
            kharmaToShow += "\nCookie File Path:\t\t" + GYAExt.PathUnityCookiesFile;
            kharmaToShow += "\nKharma SessionID:\t\t" + kharmaSessionID;
            if (EditorPrefs.HasKey("kharma.remember_session"))
            {
                var kharmaRemember_session = EditorPrefs.GetString("kharma.remember_session");
                kharmaToShow += "\nKharma Remember_session:\t" + (kharmaRemember_session == "1" ? true : false);
            }
            kharmaToShow += "\n\nPlease, DO NOT paste your SessionID when reporting an issue to the forums.";
//#endif

            kharmaToShow += "\n\nIf there is a problem retrieving the SessionID, verify that you have logged into the 'OLD' Asset Store at least once from within Unity.";

#else
	        //kharmaToShow = "The 'Purchased Assets List' (PAL) retrieval is disabled due to changes of the Unity Asset Store.";
#endif
            EditorGUILayout.BeginVertical(GUILayout.Width(680));
            EditorGUILayout.TextArea(kharmaToShow, infoStyle);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical(GUILayout.Width(200));

            Dictionary<string, string> infoToShowAll = new Dictionary<string, string>
            {
                {"gyaVars.version:", GYA.gyaVars.version},
                {"Application.unityVersion:", Application.unityVersion},
                {"SystemInfo.operatingSystem:", SystemInfo.operatingSystem},
                {"GYAExt.PathUnityApp:", Path.GetFullPath(GYAExt.PathUnityApp)},
                {"GYAExt.PathUnityStandardAssets:", GYAExt.PathUnityStandardAssets},
                {"GYAExt.PathUnityProject:", Path.GetFullPath(GYAExt.PathUnityProject)},
                {"GYAExt.PathUnityProjectAssets:", GYAExt.PathUnityProjectAssets},
                {"GYAExt.PathGYADataFiles:", GYAExt.PathGYADataFiles},
                {"gyaVars.pathOldAssetsFolder:", GYA.gyaVars.pathOldAssetsFolder},
                {"gyaVars.Files.Prefs.file:", GYA.gyaVars.Files.Prefs.file},
                {"gyaVars.Files.Groups.file:", GYA.gyaVars.Files.Groups.file},
                {"gyaVars.Files.Assets.file:", GYA.gyaVars.Files.Assets.file},
                {"gyaVars.Files.ASPackage.file:", GYA.gyaVars.Files.ASPackage.file},
                {"gyaVars.Files.ASPurchase.file:", GYA.gyaVars.Files.ASPurchase.file},
                {"GYAExt.PathUnityDataFiles:", GYAExt.PathUnityDataFiles},
                {"GYAExt.PathUnityAssetStoreActive:", GYAExt.PathUnityAssetStoreActive}
            };


            var infoToShowKey = "";
            var infoToShowValue = "";
            foreach (var line in infoToShowAll)
            {
                infoToShowKey += line.Key + "\n";
                infoToShowValue += line.Value + "\n";

                // Add an extra Return for display
                if (line.Key == "SystemInfo.operatingSystem:" ||
                    line.Key == "GYAExt.PathUnityProjectAssets:" ||
                    line.Key == "gyaVars.Files.ASPurchase.file:"
                )
                {
                    infoToShowKey += "\n";
                    infoToShowValue += "\n";
                }
            }

            EditorGUILayout.TextArea(infoToShowKey, infoStyle);

            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical(GUILayout.Width(480));

            EditorGUILayout.TextArea(infoToShowValue, infoStyle);

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            // Clipboard
            if (GUILayout.Button("Copy To Clipboard"))
            {
                GYAFile.CopyToClipboard(GYAExt.ToJson(infoToShowAll, true), true);
                GUIUtility.ExitGUI();
            }

            EditorGUILayout.PrefixLabel("");
        }
    }
}