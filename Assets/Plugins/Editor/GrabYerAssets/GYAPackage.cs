//	Grab Yer Assets
//	Copyright Frederic Bell, 2014

using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GYAInternal.Json;
using System.Net;

using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace XeirGYA
{
    [ExecuteInEditMode]
    public partial class GYAPackage : MonoBehaviour
    {
        public static int countMarkedToImport = 0; // Asset count marked in list for processing
        internal static bool refreshDetectedUpdate = false; // Refresh detected Assets added/removed from Index
        internal static bool refreshDetectedUpdateOnStore = false; // Refresh detected Assets added/removed from Index

        internal static string CustomizePackageFilenameVersionSuffix(GYAData.Asset package, string userVersionString)
        {
            userVersionString = userVersionString.Replace("{unity_version}", GetUnityVersionOrUnknown(package.unity_version));
            userVersionString = userVersionString.Replace("{id}", package.id.ToString());
            userVersionString = userVersionString.Replace("{version}", GYAFile.GetSafeFilename(package.version));
            userVersionString = userVersionString.Replace("{version_id}", package.version_id.ToString());
            userVersionString = userVersionString.Replace("{upload_id}", package.upload_id.ToString());
            userVersionString = userVersionString.Replace("{pub_id}", package.publisher.id.ToString());

            //userVersionString = userVersionString.Replace("{pubDate}", package.pubdate);
            userVersionString = userVersionString.Replace("{pub_date}", GYAExt.DateOnlyFromISO8601(package.pubDateToDateTime.ToString()));
            //userVersionString = userVersionString.Replace("{date_updated}", GYAExt.DateOnlyFromISO8601(package.dateUpdated.ToString()));
            //userVersionString = userVersionString.Replace("{date_created}", GYAExt.DateOnlyFromISO8601(package.dateCreated.ToString()));
            userVersionString = userVersionString.Replace("{pkg_created}", GYAExt.DateOnlyFromISO8601(package.fileDataCreated.ToString()));
            userVersionString = userVersionString.Replace("{file_created}", GYAExt.DateOnlyFromISO8601(package.fileDateCreated.ToString()));

            userVersionString = userVersionString.Replace("{sp}", " ");

            return userVersionString;
        }

        internal static string GetUnityVersionOrUnknown(string unityVersion)
        {
            if (unityVersion.Length == 0)
            {
                unityVersion = "Unknown";
            }
            return unityVersion;
        }

        // Has SessionID
        internal static bool HasSessionID()
        {
            bool kharmaSessionIDExists = EditorPrefs.HasKey("kharma.sessionid");
            return kharmaSessionIDExists;
        }

        // Get SessionID
        internal static string GetSessionID()
        {
            var kharmaSessionID = "";
            var kharmaSessionIDExists = EditorPrefs.HasKey("kharma.sessionid");
            if (kharmaSessionIDExists)
            {
                kharmaSessionID = EditorPrefs.GetString("kharma.sessionid");
            }
            return kharmaSessionID;
        }

        // Has ActiveSessionID
        internal static bool HasActiveSessionID()
        {
            bool kharmaActiveSessionIDExists = (bool)GYAReflect.MI_Invoke("UnityEditor.AssetStoreContext",
                "SessionHasString", "kharma.active_sessionid");
            return kharmaActiveSessionIDExists;
        }

        // Get ActiveSessionID
        internal static string GetActiveSessionID()
        {
            var kharmaActiveSessionID = "";
            var kharmaActiveSessionIDExists = (bool)GYAReflect.MI_Invoke("UnityEditor.AssetStoreContext",
                "SessionHasString", "kharma.active_sessionid");
            if (kharmaActiveSessionIDExists)
            {
                kharmaActiveSessionID = (string)GYAReflect.MI_Invoke("UnityEditor.AssetStoreContext",
                    "SessionGetString", "kharma.active_sessionid");
            }
            return kharmaActiveSessionID;
        }

        // Check for and update/save ActiveSessionID if required
        internal static string UpdateActiveSessionID()
        {
            //var kharmaSessionID = GetSessionID();
            //var kharmaActiveSessionID = GetActiveSessionID();

            // If not empty and has changed, update in Prefs

            return "";
        }

        // Open multiple URL's in the browser
        internal static void BrowseSelectedPackages(object pObj)
        {
            string dialogTitle = "Browse Asset Store";
            int assetCounter = 0;

            foreach (var asset in GYA.gyaData.Assets)
            {
                if (!asset.isMarked)
                    continue;

                if ((assetCounter > 0)
                    && ((assetCounter % GYA.gyaVars.Prefs.urlsToOpenPerBatch) == 0))
                {
                    if (!EditorUtility.DisplayDialog(dialogTitle,
                        string.Format("{0} URLs have already been opened.\nDo you want more?", assetCounter),
                        "Continue", "Cancel"))
                        break;
                }

                assetCounter++;
                if (asset.link.id > 0)
                {
                    string openURL = string.Format("https://www.assetstore.unity3d.com/#/{0}/{1}", asset.link.type,
                        asset.link.id);
                    Application.OpenURL(openURL);
                }
            }

            if (assetCounter <= 0)
                EditorUtility.DisplayDialog(dialogTitle,
                    "There is nothing to browse!\nFirst select one or more packages.", "OK");
        }

        // --

        // Scan Store assets folder for unitypackages
        internal static string RefreshStore(bool showRefresh = true)
        {
            List<String> pathsTmp = new List<String> { GYAExt.PathUnityDataFiles };
            var pathsToProcess = pathsTmp.ToArray();

            return RefreshCollection(GYA.svCollection.Store, pathsToProcess, showRefresh);
        }

        // Scan User assets folder(s) for unitypackages
        internal static string RefreshUser(bool showRefresh = true)
        {
            var pathsToProcess = GYA.gyaVars.Prefs.pathUserAssets.ToArray();
            return RefreshCollection(GYA.svCollection.User, pathsToProcess, showRefresh);
        }

        // Scan standard assets folder for unitypackages
        internal static string RefreshStandard(bool showRefresh = true)
        {
            return RefreshCollection(GYA.svCollection.Standard, GYAExt.PathUnityStandardAssets, showRefresh);
        }

        // Scan project folder for unitypackages
        internal static string RefreshProject(bool showRefresh = true)
        {
            return RefreshCollection(GYA.svCollection.Project, GYAExt.PathUnityProjectAssets, showRefresh);
        }

        // Scan old folder for unitypackages
        internal static string RefreshOld(bool showRefresh = true)
        {
            return RefreshCollection(GYA.svCollection.Old, GYA.gyaVars.pathOldAssetsFolder, showRefresh);
        }

        // --

        // Figure out the Old Assets of pkgData.Store including what is in the Old Folder
        internal static void BuildOldAssetsList()
        {
            // Store results
            List<GYAData.Asset> packagesTemp = new List<GYAData.Asset>(GYA.gyaData.Assets);

            try
            {
                // Old files ONLY selected from: AS/AS5/OLD
                packagesTemp.RemoveAll(x => x.isExported);
                packagesTemp.RemoveAll(x => x.collection == GYA.svCollection.Standard);
                packagesTemp.RemoveAll(x => x.collection == GYA.svCollection.User);
                packagesTemp.RemoveAll(x => x.collection == GYA.svCollection.Project);

                if (packagesTemp.Any())
                {
                    // Sort by id then version_id, descending
                    packagesTemp.Sort((x, y) =>
                    {
                        int compare = -x.id.CompareTo(y.id);
                        if (compare != 0)
                            return compare;

                        compare = -x.version_id.CompareTo(y.version_id);
                        if (compare != 0)
                            return compare;

                        return x.id.CompareTo(y.id);
                    });

                    int tmpPkgID = packagesTemp[0].id;
                    int tmpPkgVID = packagesTemp[0].version_id;
                    foreach (GYAData.Asset package in packagesTemp)
                    {
                        var isPrimaryPkg = true;
                        isPrimaryPkg = package.id != tmpPkgID;

                        // Check for primary package
                        if (package.id == tmpPkgID && package.version_id < tmpPkgVID &&
                            package.collection == GYA.svCollection.Store && isPrimaryPkg == false)
                        {
                            package.isOldToMove = true;
                        }
                        tmpPkgID = package.id;
                        // Only update if this is the primary package for a given PkgID
                        if (isPrimaryPkg)
                            tmpPkgVID = package.version_id;
                    }
                }
            }
            catch (Exception ex)
            {
                GYA.Instance.ErrorStateSet(GYA.ErrorCode.Error);
                GYAExt.LogError(GYA.gyaVars.abbr + " - Processing JSON Failed: " + ex.Message);
            }
            packagesTemp = null;
        }

        // Process Auto Consolidation Move/Delete if required
        // return value determines if Refresh quits processing due to enableOfflineMode
        internal static bool RefreshPreProcess()
        {
            // Offline Mode
            if (GYA.gyaVars.Prefs.enableOfflineMode)
            {
                // Offline mode enabled, do not refresh package list
                GYAExt.Log("Offline Mode is currently ENABLED.  Exisitng data file used.");
                if (!GYAFile.LoadGYAAssets())
                {
                    GYAExt.LogWarning("Data file missing.",
                        "Please select file to load or disable Offline Mode via 'Control Center->Maintanence'.");
                }

                GYA.Instance.RefreshSV();
                return true;
            }

            return false;
        }

        // Count files in a given folder
        internal static int CountAssetsInFolder(string folder, bool includeSubFolders = true)
        {
            int fileCount = 0;
            if (Directory.Exists(folder))
            {
                fileCount = includeSubFolders ? Directory.GetFiles(folder, "*.unity?ackage", SearchOption.AllDirectories).Length : Directory.GetFiles(folder, "*.unity?ackage", SearchOption.TopDirectoryOnly).Length;
            }
            return fileCount;
        }

        internal static int GetAssetCountFromFolder(string folder)
        {
            DirectoryInfo directory = new DirectoryInfo(folder);

            if (directory.Exists)
            {
                FileInfo[] files = directory.GetFiles("*.unity?ackage", SearchOption.AllDirectories)
                    .Where(fi => (fi.Attributes & FileAttributes.Hidden) == 0).ToArray();
                return files.Length;
            }
            return 0;
        }

        // Tally file size of dupes
        internal static string CalcFolderSize(List<GYAData.Asset> packageCalc)
        {
            double fileSizeTotal = packageCalc.Aggregate<GYAData.Asset, double>(0, (current, package) => current + package.fileSize);
            return fileSizeTotal.BytesToKB();
        }

        // Populate counts and size data
        internal static void TallyAssets()
        {
            GYA.gyaVars.FilesCount.all = GYA.gyaData.Assets.Count;
            GYA.gyaVars.FilesCount.store = GYA.gyaData.Assets.FindAll(x => x.collection == GYA.svCollection.Store)
                .Count;
            GYA.gyaVars.FilesCount.user = GYA.gyaData.Assets.FindAll(x => x.collection == GYA.svCollection.User).Count;
            GYA.gyaVars.FilesCount.standard = GYA.gyaData.Assets.FindAll(x => x.collection == GYA.svCollection.Standard)
                .Count;
            GYA.gyaVars.FilesCount.old = GYA.gyaData.Assets.FindAll(x => x.collection == GYA.svCollection.Old).Count;
            GYA.gyaVars.FilesCount.oldToMove = GYA.gyaData.Assets.FindAll(x => x.isOldToMove).Count;
            GYA.gyaVars.FilesCount.project = GYA.gyaData.Assets.FindAll(x => x.collection == GYA.svCollection.Project)
                .Count;

            GYA.gyaVars.FilesSize.all = GYA.gyaData.Assets.Sum(item => item.fileSize);
            GYA.gyaVars.FilesSize.store = GYA.gyaData.Assets.FindAll(x => x.collection == GYA.svCollection.Store)
                .Sum(item => item.fileSize);
            GYA.gyaVars.FilesSize.user = GYA.gyaData.Assets.FindAll(x => x.collection == GYA.svCollection.User)
                .Sum(item => item.fileSize);
            GYA.gyaVars.FilesSize.standard = GYA.gyaData.Assets.FindAll(x => x.collection == GYA.svCollection.Standard)
                .Sum(item => item.fileSize);
            GYA.gyaVars.FilesSize.old = GYA.gyaData.Assets.FindAll(x => x.collection == GYA.svCollection.Old)
                .Sum(item => item.fileSize);
        }

        // Count instances of package ID, ignore "Non Asset Store packages"
        internal static int CountDuplicatesOfID(int id)
        {
            List<GYAData.Asset> pkgResults = GYA.gyaData.Assets.FindAll(x => x.id == id && !x.isExported);
            return (pkgResults.Count);
        }

        // Return the true asset name of an asset IF the title is blank, often the filename is 'unknown'
        internal static string GetAssetNameFromOldIDIfExist(int assetID, int assetVersionID = 0)
        {
            string assetName = "unknown";
            var pkgResults = GYA.gyaData.Assets.FindAll(x => x.id == assetID);
            pkgResults.Sort((x, y) => -x.version_id.CompareTo(y.version_id));

            if (pkgResults.Any())
            {
                foreach (GYAData.Asset package in pkgResults)
                {
                    if (!package.title.StartsWith("unknown", StringComparison.InvariantCultureIgnoreCase))
                    {
                        assetName = package.title;
                        break;
                    }
                    if (package.version_id == assetVersionID)
                        assetName = Path.GetFileNameWithoutExtension(package.filePath);
                }
            }
            else
            {
                assetName = GYA.gyaData.Assets[0].title;
            }

            return assetName;
        }

        // FindAssetByFullName
        internal static GYAData.Asset GetAssetByFullName(string fullName)
        {
            GYAData.Asset pkgResults = GYA.gyaData.Assets.Find(x => String.Equals(x.filePath, fullName, StringComparison.InvariantCultureIgnoreCase));
            return (pkgResults);
        }

        internal static GYAData.Asset GetAssetByID(int assetID)
        {
            GYAData.Asset pkgResults = GYA.gyaData.Assets.Find(x => x.id == assetID && !x.isExported);
            return (pkgResults);
        }

        //internal static ASPurchased.Item GetASPurchasedByID (int assetID)
        internal static ASPurchased.Result GetASPurchasedByID(int assetID)
        {
            if (GYA.asPurchased.results != null && GYA.asPurchased.results.Count > 0)
            {
                var result = GYA.asPurchased.results.Find(x => x.id == assetID);
                return result;
            }
            return null; // No asPurchased data to process
        }

        // Get info from unitypackage
        internal static string GetPackageInfoFromFile(string assetPath, GYA.svCollection collection)
        {
            FileInfo fi = new FileInfo(assetPath);

            return GetPackageInfoFromFile(fi, collection);
        }

        internal static string GetPackageInfoFromFile(FileInfo fi, GYA.svCollection collection,
            bool reportPkgErrors = false)
        {
            string infoJSON = "";
            // Path & File Name
            string fileFullName = fi.FullName;
            fileFullName = fileFullName.Replace('\\', '/');

            string fileAssetName = Path.GetFileNameWithoutExtension(fileFullName);

            // Remove invalid characters from title, this is in case it is used as a filename later
            //fileAssetName = GYAFile.ReturnValidFile(fileAssetName);

            bool hasValidHeader = false;
            bool isDamaged = false;
            DateTimeOffset mTimeStamp = default(DateTimeOffset);

            // If file exists, process it
            if (File.Exists(fileFullName))
            {
                // Make sure the file is at least long enough to test for data
                if (fi.Length >= 32)
                {
                    mTimeStamp = new DateTimeOffset(fi.CreationTimeUtc);
                    try
                    {
                        // Finds the length of the Info contained in the package and populates a string
                        using (FileStream fs = new FileStream(fileFullName, FileMode.Open, FileAccess.Read))
                        {
                            using (BinaryReader br = new BinaryReader(fs, System.Text.Encoding.UTF8))
                            {
                                var headerBytes = br.ReadBytes(16);
                                string headerString = BitConverter.ToString(headerBytes, 0, 16);
                                headerString = headerString.Replace("-", ""); // Remove "-" dividers
                                string uID = headerString.Substring(0, 4);
                                string uFlag = headerString.Substring(6, 2);
                                string hexData =
                                    headerString.Substring(22, 2) + headerString.Substring(20, 2); // Length of Data
                                int lenData =
                                    int.Parse(hexData,
                                        System.Globalization.NumberStyles.HexNumber); // Length of Data (Converted)
                                string uSubID = headerString.Substring(24, 4);
                                string hexJSON =
                                    headerString.Substring(30, 2) +
                                    headerString.Substring(28, 2); // Length of JSON String
                                int lenJSON =
                                    int.Parse(hexJSON,
                                        System.Globalization.NumberStyles
                                            .HexNumber); // Length of JSON String (Converted)

                                // Change collection to Old if asset is in pathOldAssetsFolder
                                if (fileFullName.Replace('/', '\\')
                                    .Contains(GYA.gyaVars.pathOldAssetsFolder.Replace('/', '\\'),
                                        StringComparison.OrdinalIgnoreCase))
                                {
                                    collection = GYA.svCollection.Old;
                                }

                                // Check if .unitypackage is valid to process
                                if (uID == "1F8B") // 31,139
                                {
                                    mTimeStamp = DateTimeOffset.Parse("1970-01-01Z")
                                        .AddSeconds(headerBytes[4] + (headerBytes[5] << 8) + (headerBytes[6] << 16) +
                                                    (headerBytes[7] << 24));

                                    // Valid GZip header
                                    if (uFlag == "04" && uSubID == "4124") // 65, 36
                                    {
                                        // Process Asset Store package
                                        if (lenData == (lenJSON + 4))
                                        {
                                            // Validate json size
                                            infoJSON = new string(br.ReadChars(lenJSON)); // <- Info String

                                            // Begin Validation Check - infoJSON
                                            try
                                            {
#pragma warning disable 0168, 219
                                                // Convert raw text to json
                                                var checkIfValid =
                                                    JsonConvert.DeserializeObject<GYAData.Asset>(infoJSON);
                                                if (checkIfValid != null)
                                                {
                                                }
#pragma warning restore 0168, 219
                                                // TODO: Rework the remaining code to use AssetIndexAddEntry() to build the Collection
                                                // While it works, is from the early days of GYA

                                                // Remove last char '}' for adding following elements
                                                infoJSON = infoJSON.Remove(infoJSON.Length - 1);

                                                // Fix missing/damaged field(s), ie- missing title

                                                // Blank title
                                                if (infoJSON.Contains("\"title\":\"\"",
                                                    StringComparison.OrdinalIgnoreCase))
                                                {
                                                    isDamaged = true;
                                                    if (reportPkgErrors)
                                                    {
                                                        string infoJSONRaw = infoJSON;
                                                        GYAExt.LogWarning("Blank package title: " + fileFullName,
                                                            "Header: (" + headerString + ")" + "\nJSON: (" +
                                                            infoJSONRaw + ")");
                                                    }

                                                    // Add missing title
                                                    //string newTitle = fileAssetName;
                                                    string newTitle = "Blank";

                                                    infoJSON = infoJSON.Replace(
                                                        ",\"title\":\"\"",
                                                        ",\"title\":\"" + newTitle + "\""
                                                    );
                                                }

                                                // "Untitled" title - I do not know why Unity pkg's have started appearing this way,
                                                // but this will force the use of the filename as the title
                                                // This will NOT update the data already in the list view however

                                                // Disabled due to problem with pkg version renaming.
                                                // As there is no true title, the version is constantly appended
                                                // example: Awesome Cartoon Airplanes v10 (400f7) (168854) v10 (400f7) (168854) v10 (400f7) (168854)
                                                // (10_168854_400f7) (10_168854_400f7) (10_168854_400f7) (10_168854_400f7) (10_168854_400f7)
                                                // (10_168854_400f7) (10_168854_400f7) (1.0_168854_4.0.0f7)

                                                if (infoJSON.Contains("\"title\":\"Untitled\"",
                                                    StringComparison.OrdinalIgnoreCase))
                                                {
                                                    isDamaged = true;
                                                    if (reportPkgErrors)
                                                    {
                                                        string infoJSONRaw = infoJSON;
                                                        GYAExt.LogWarning("'Untitled' package title: " + fileFullName,
                                                            "Header: (" + headerString + ")" + "\nJSON: (" +
                                                            infoJSONRaw + ")");
                                                    }

                                                    //// Add missing title
                                                    //string newTitle = fileAssetName;
                                                    //infoJSON = infoJSON.Replace(
                                                    //    ",\"title\":\"Untitled\"",
                                                    //    ",\"title\":\"" + newTitle + "\""
                                                    //);
                                                }

                                                // Missing title
                                                if (!infoJSON.Contains("\"title\":\"",
                                                    StringComparison.OrdinalIgnoreCase))
                                                {
                                                    isDamaged = true;
                                                    // Add missing title
                                                    infoJSON += ",\"title\":\"" + fileAssetName + "\"";
                                                    //infoJSON += ",\"title\":\"Missing\"";

                                                    if (reportPkgErrors)
                                                    {
                                                        GYAExt.LogWarning("Missing package title: " + fileFullName,
                                                            "Header: (" + headerString + ")" + "\nJSON: (" + infoJSON +
                                                            ")");
                                                    }
                                                }
                                                // Missing unity_version
                                                if (!infoJSON.Contains("\"unity_version\":\"",
                                                    StringComparison.OrdinalIgnoreCase))
                                                {
                                                    // Add missing title
                                                    infoJSON += ",\"unity_version\":\"\"";
                                                }
                                                // Missing unity_version
                                                if (!infoJSON.Contains("\"upload_id\":\"",
                                                    StringComparison.OrdinalIgnoreCase))
                                                {
                                                    // Add missing title
                                                    infoJSON += ",\"upload_id\":\"0\"";
                                                }

                                                infoJSON += ",\"filePath\":\"" + fileFullName + "\"";
                                                infoJSON += ",\"fileSize\":\"" + fi.Length + "\"";
                                                infoJSON += ",\"fileDataCreated\":\"" +
                                                            GYAExt.DateAsISO8601(mTimeStamp) + "\"";

                                                // For checking date validation
                                                infoJSON += ",\"fileDateCreated\":\"" +
                                                            GYAExt.DateAsISO8601(fi.CreationTimeUtc) + "\"";
                                                infoJSON += ",\"isExported\":\"" + false + "\"";
                                                infoJSON += ",\"isDamaged\":\"" + isDamaged + "\"";
                                                infoJSON += ",\"collection\":\"" + collection + "\"}";
                                                hasValidHeader = true; // Header is valid!
                                            }
                                            catch (Exception)
                                            {
                                                if (reportPkgErrors)
                                                    GYAExt.LogWarning(
                                                        "Damaged UnityPackage (JSON Corrupt): " + fileFullName,
                                                        "Header: (" + headerString + ")" + "\nJSON: (" + infoJSON +
                                                        ")");
                                            }
                                            // End Validation check
                                        }
                                        else
                                        {
                                            isDamaged = true;
                                            if (reportPkgErrors)
                                                GYAExt.LogWarning(
                                                    "Damaged UnityPackage (JSON Length Mismatch): " + fileFullName,
                                                    "Header: (" + headerString + ")");
                                        }
                                    }
                                }
                                else
                                {
                                    isDamaged = true;
                                    if (reportPkgErrors)
                                        GYAExt.LogWarning("Damaged UnityPackage (Missing GZip Header):" + fileFullName,
                                            "Header: (" + headerString + ")");
                                } // End extracting Asset Store JSON data from file
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        isDamaged = true;
                        GYAExt.LogWarning("Damaged UnityPackage (Exception): " + fileFullName,
                            "Header: (" + infoJSON + ")\n" + ex.Message);
                    }
                }
                else
                {
                    isDamaged = true;
                    if (reportPkgErrors)
                        GYAExt.LogWarning("Damaged UnityPackage (File Length Too Short): " + fileFullName,
                            "File Size in Bytes: " + fi.Length);
                }
            }
            else
            {
                isDamaged = true;
                GYAExt.LogWarning("Missing UnityPackage: " + fileFullName, "This could be due to invalid path characters in the filename.  Please fix from the OS and re-scan.");
            }

            if (hasValidHeader)
            {
                return infoJSON;
            }

            // Process Exported file - Build required info
            string defaultString = String.Empty;
            int defaultInt = 0;

            infoJSON = "{\"link\":{\"type\":\"" + defaultString + "\",\"id\":\"" + defaultInt + "\"}," +
                       "\"unity_version\":\"" + defaultString + "\",\"pubdate\":\"" + defaultString +
                       "\",\"version\":\"" + defaultString + "\",\"upload_id\":\"" + defaultInt +
                       "\",\"version_id\":\"" + defaultString + "\"," + "\"category\":{\"label\":\"" + defaultString +
                       "\",\"id\":\"" + defaultInt + "\"},\"id\":\"" + defaultInt + "\"," + "\"title\":\"" +
                       fileAssetName + "\",\"publisher\":{\"label\":\"" + defaultString + "\",\"id\":\"" + defaultInt +
                       "\"}";

            infoJSON += ",\"filePath\":\"" + fileFullName + "\"";
            infoJSON += ",\"fileSize\":\"" + fi.Length + "\"";
            infoJSON += ",\"fileDataCreated\":\"" + GYAExt.DateAsISO8601(mTimeStamp) + "\"";
            infoJSON += ",\"fileDateCreated\":\"" + GYAExt.DateAsISO8601(fi.CreationTimeUtc) + "\"";
            infoJSON += ",\"isExported\":\"" + true + "\"";
            infoJSON += ",\"isDamaged\":\"" + isDamaged + "\"";
            infoJSON += ",\"collection\":\"" + collection + "\"}";

            return infoJSON;
        }


        // Grab a copy of the local data used by the Asset Store (Icon, and other basic data)
        internal static void GetPackagesInfoFromUnity()
        {
            // Package List - Downloaded AS Packages
            GYACoroutine.start(GYAReflect.UASGet_AssetStoreContext_GetPackageList());
        }

        public static bool MyRemoteCertificateValidationCallback(System.Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            bool isOk = true;
            // If there are errors in the certificate chain, look at each error to determine the cause.
            if (sslPolicyErrors != SslPolicyErrors.None)
            {
                for (int i = 0; i < chain.ChainStatus.Length; i++)
                {
                    if (chain.ChainStatus[i].Status != X509ChainStatusFlags.RevocationStatusUnknown)
                    {
                        chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                        chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                        chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
                        chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                        bool chainIsValid = chain.Build((X509Certificate2)certificate);
                        if (!chainIsValid)
                        {
                            isOk = false;
                        }
                    }
                }
            }

            if (!isOk)
                Debug.Log("userCertificateValidationCallback: " + (isOk ? "SUCCESS " : "FAILED ") + ((SslPolicyErrors)sslPolicyErrors).ToString());

            return isOk;
        }


        //// For MONO ssl decryption failed
        //public static string PostString(string url, string data)
        //{
        //    Process p = null;
        //    try
        //    {
        //        var psi = new ProcessStartInfo
        //        {
        //            FileName = "curl",
        //            Arguments = string.Format("-k {0} --data \"{1}\"", url, data),
        //            RedirectStandardOutput = true,
        //            UseShellExecute = false,
        //            CreateNoWindow = false,
        //        };

        //        p = Process.Start(psi);

        //        return p.StandardOutput.ReadToEnd();
        //    }
        //    finally
        //    {
        //        if (p != null && p.HasExited == false)
        //            p.Kill();
        //    }
        //}


        public static void Download_UAS_Asset(int id)
        {
            GYAExt.Log("Download_UAS_Asset: " + id);

        }

        // client_id=asset_store
        public static void GetSessionFromUnity()
        {
            //string data = "client_id=asset_store&response_type=code"; //replace <value>
            //byte[] dataStream = System.Text.Encoding.UTF8.GetBytes(data);

            //string urlPath = "https://api.unity.com/v1/oauth2/authorize";
            //string request2 = urlPath + "";
            //WebRequest webRequest = WebRequest.Create(request2);
            //webRequest.Method = "POST";
            //webRequest.ContentType = "application/x-www-form-urlencoded";
            //webRequest.ContentLength = dataStream.Length;
            //ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;
            //Stream newStream = webRequest.GetRequestStream();
            //// Send the data.
            //newStream.Write(dataStream, 0, dataStream.Length);
            //newStream.Close();
            //WebResponse webResponse = webRequest.GetResponse();
            //GYAExt.Log("FinishWebLogin Result: " + webResponse.GetHashCode() + " - " + webResponse.Headers.ToString() + " - " + webResponse);
            //return;


            //var pURL = "https://www.assetstore.unity3d.com/api/en-US/account/downloads/search.json";
            var pURL = "https://www.assetstore.unity3d.com/auth/whoami";
            //var pURL = "https://api.unity.com/v1/oauth2/authorize?client_id=asset_store&response_type=code";
            //var pURL = "https://api.unity.com/v1/oauth2/authorize";

            // FAILS: GYA - GetSessionFromUnity (WebException): Error: SendFailure (Error writing request: The authentication or decryption has failed.)
            //var pURL = "https://api.unity.com/v1/oauth2/authorize?client_id=asset_store&response_type=code&redirect_uri=https%3A%2F%2Fwww.assetstore.unity3d.com%2Fauth%2Fwhoami";

            var postData = string.Empty;
            //var postData = "client_id=asset_store&response_type=code";
            //byte[] dataStream = System.Text.Encoding.UTF8.GetBytes(postData);

            var request = (HttpWebRequest)WebRequest.Create(new Uri(pURL));

            request.Method = "POST";
            //request.Accept = "application/json";
            request.Accept = "*/*";
            request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.None;
            request.ContentLength = postData.Length;
            //request.ContentLength = dataStream.Length;
            request.Proxy = null;

            //request.Headers.Add("Authorization", "Bearer " + "");
            //request.Headers.Add("Cookie", "kharma_explicitly_logged_out=0; kharma_intro=1; kharma_session=" + GYA.gyaVars.Prefs.kharmaSession);
            request.Headers.Add("Cookie", "SERVERID=varnish01; kharma_explicitly_logged_out=0; kharma_intro=1");
            //request.Headers.Add("Connection", "keep-alive");
            request.Headers.Add("X-Kharma-Version", "0"); // 67a6844 95bf5c5
            request.Headers.Add("X-Requested-With", "UnityAssetStore");
            //request.Headers.Add("X-Unity-Session", "26c4202eb475d02864b40827dfff11a14657aa41");
            request.KeepAlive = true;
            request.Referer = "https://www.assetstore.unity3d.com/en/";

            //request.Headers.Add("client_id", "asset_store");
            //request.Headers.Add("Host", "api.unity.com"); // ArgumentException: This header must be modified with the appropiate property.
            //request.Host = "api.unity.com"; // CS1061: Type `System.Net.HttpWebRequest' does not contain a definition for `Host'
            //request.Headers["Host"] = "api.unity.com"; // ArgumentException: restricted header

            //Change Host header
            //request.changeSysTemHeader("Host", "api.unity.com");
            request.changeSystemHeader("Host", "www.assetstore.unity3d.com");
            request.changeReflectionField("hostChanged", true);

            // Added to fix: Error getting response stream (Write: The authentication or decryption has failed.): SendFailure
            ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;

            try
            {
                // Request
                using (var reqStream = request.GetRequestStream())
                {
                    // Shouldn't be required as there is nothing to send
                    using (var writer = new StreamWriter(reqStream))
                    {
                        writer.Write(postData);
                        //writer.Write(dataStream);
                        //writer.Write(dataStream, 0, dataStream.Length);
                    }
                }
            }
            catch (WebException ex)
            {
                GYAExt.Log("GetSessionFromUnity (WebException): " + ex.Message);
                return;
            }
            catch (Exception ex)
            {
                GYAExt.Log("GetSessionFromUnity (Exception): " + ex.Message);
                return;
            }

            // Result
            request.BeginGetResponse(new AsyncCallback(FinishWebLogin), request);
        }


        public static void FinishWebLogin(IAsyncResult result)
        {
            var formatJSON = true;
            try
            {
                using (var response = (result.AsyncState as HttpWebRequest).EndGetResponse(result) as HttpWebResponse)
                using (var streamResponse = response.GetResponseStream())
                using (var streamRead = new StreamReader(streamResponse, System.Text.Encoding.UTF8))
                {
                    var responseString = streamRead.ReadToEnd();
                    var success = response.StatusCode == HttpStatusCode.OK;

                    GYAExt.Log("FinishWebLogin Result: " + success + " - " + responseString);

                    // Save File
                    if (success && !string.IsNullOrEmpty(responseString))
                    {
                        var pFile = GYAExt.PathGYADataFiles + "/AS Login.json";
                        if (!formatJSON)
                            GYAFile.SaveFile(pFile, responseString, true);
                        else
                            GYAFile.SaveFileJson(pFile, responseString, true);
                    }
                }
            }
            catch (WebException ex)
            {
                using (WebResponse response = ex.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    GYAExt.Log("FinishWebLogin Error code: " + httpResponse.StatusCode, ex.Message);
                    using (var streamReader = new StreamReader(response.GetResponseStream()))
                        GYAExt.Log(streamReader.ReadToEnd());
                }
            }
        }


        // Grab a copy of the Users Purchased assets list from the Asset Store
        // https://www.assetstore.unity3d.com/en/#!/account/downloads/search=#PACKAGES
        public static void DownloadPALFromUnity()
        {
            var pURL = "https://www.assetstore.unity3d.com/api/en-US/account/downloads/search.json";

            var postData = string.Empty;
            var request = (HttpWebRequest)WebRequest.Create(new Uri(pURL));
            request.Method = "POST";
            request.Accept = "*/*";
            request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.ContentLength = postData.Length;
            request.Proxy = null;

            request.Headers.Add("Cookie", "kharma_explicitly_logged_out=0; kharma_intro=1; kharma_session=" + GYA.gyaVars.Prefs.kharmaSession);
            request.Headers.Add("X-Kharma-Version", "5.6.0-r89264"); // TODO: using default ver, gen version from active editor
            request.Headers.Add("X-Requested-With", "UnityAssetStore");

            // Added to fix: Error getting response stream (Write: The authentication or decryption has failed.): SendFailure
            ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;

            try
            {
                // Request
                using (var reqStream = request.GetRequestStream())
                {
                    // Shouldn't be required as there is nothing to send
                    using (var writer = new StreamWriter(reqStream))
                    {
                        writer.Write(postData);
                    }
                }
            }
            catch (WebException ex)
            {
                GYAExt.Log("DownloadPALFromUnity (WebException): " + ex.Message);
                return;
            }
            catch (Exception ex)
            {
                GYAExt.Log("DownloadPALFromUnity (Exception): " + ex.Message);
                return;
            }

            // Result
            request.BeginGetResponse(new AsyncCallback(FinishWebRequest), request);
        }

        //NOT USED
        public static void StartWebRequest(string url)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.BeginGetResponse(new AsyncCallback(FinishWebRequest), request);
            }
            catch (WebException ex)
            {
                GYAExt.Log("StartWebRequest: " + ex.Message);
            }
        }

        public static void FinishWebRequest(IAsyncResult result)
        {
            var formatJSON = true;
            try
            {
                using (var response = (result.AsyncState as HttpWebRequest).EndGetResponse(result) as HttpWebResponse)
                using (var streamResponse = response.GetResponseStream())
                using (var streamRead = new StreamReader(streamResponse, System.Text.Encoding.UTF8))
                {
                    var responseString = streamRead.ReadToEnd();
                    var success = response.StatusCode == HttpStatusCode.OK;

                    //GYAExt.Log("FinishWebRequest Result: " + success + " - " + responseString);

                    // Save File
                    if (success && !string.IsNullOrEmpty(responseString))
                    {
                        var pFile = GYA.gyaVars.Files.ASPurchase.file;
                        if (!formatJSON)
                            GYAFile.SaveFile(pFile, responseString, true);
                        else
                            GYAFile.SaveFileJson(pFile, responseString, true);
                        ProcessDownloadedFile(pFile, responseString);
                    }
                }
            }
            catch (WebException ex)
            {
                using (WebResponse response = ex.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    GYAExt.Log("FinishWebRequest Error code: " + httpResponse.StatusCode, ex.Message);
                    using (var streamReader = new StreamReader(response.GetResponseStream()))
                        GYAExt.Log(streamReader.ReadToEnd());
                }
            }
        }

        public static void ProcessDownloadedFile(string pFile, string wwwText)
        {
            try
            {
                if (pFile == GYA.gyaVars.Files.ASPurchase.file)
                {
                    GYA.asPurchased = JsonConvert.DeserializeObject<ASPurchased>(wwwText);
                    GYA.Instance.RefreshSV();
                    GYAExt.Log("Unity 'Purchased Assets List' downloaded: ( Assets: " + GYA.asPurchased.results.Count +
                               " ) ( Size: " + wwwText.Length.BytesToKB() + " )");
                }
            }
            catch (Exception ex)
            {
                GYAExt.LogWarning("ProcessDownloadedFile (Failure): " + pFile, ex.Message);
            }
        }

        // Refresh All Collections scanning for file add/remove
        internal static void RefreshAllCollections(bool forceUpdate = false)
        {
            if (RefreshPreProcess())
                return; // Perform Pre Refresh routines - Exit if enableOfflineMode

            // Force rescan of damaged or untitled assets
            AssetForceRemoveAndRescan();

            // Auto Rename AS assets if enabled
            if (GYA.gyaVars.Prefs.autoPreventASOverwrite && GYA.gyaVars.FilesCount.store > 0)
            {
                GYAFile.RenameWithVersionCollection(false);
            }

            GYAExt.StopwatchStart();

            string scanResults = String.Empty;
            string collResults = "Scan Results Per Collection:\n";
            collResults += RefreshProject();
            collResults += RefreshStandard();
            collResults += RefreshOld();
            collResults += RefreshStore();
            collResults += RefreshUser();

            EditorUtility.DisplayProgressBar(
                string.Format("{0} Refreshing {1}", GYA.gyaVars.abbr, "..."), "Rebuilding Asset Index ...", 1f
            );

            GYA.Instance.RefreshSV(false);

            //// If index changes detected, process and save, saves a little time by not saving the file if not needed
            //if (!File.Exists(GYA.gyaVars.Files.Assets.file) || refreshDetectedUpdate || forceUpdate)
            //    GYAFile.SaveGYAAssets();
            GYAFile.SaveGYAAssets();

            // -- BEGIN Native/Remote Package Data collection

            var optionResult = String.Empty;

            
#if ENABLE_PAL
	        var tFile = Path.GetFullPath(Path.Combine(GYAExt.PathGYADataFiles, "AS Purchased.json"));
	        bool getPurchEnabledButMissing = GYA.gyaVars.Prefs.getPurchasedAssetsListDuringRefresh && !File.Exists(tFile);
            
            if ((GYA.gyaVars.Prefs.getPurchasedAssetsListDuringRefresh && refreshDetectedUpdateOnStore) || getPurchEnabledButMissing || forceUpdate)
            {
                // Check if SessionID is valid
                GYA.gyaVars.Prefs.kharmaSession = GYAFile.GetFromUnityCookies_KharmaSession();

                if (GYA.gyaVars.Prefs.kharmaSession == "")
                {
                    //optionResult += "Unable to retrieve the PAL at this time ...";
                }
                else
                {
                    DownloadPALFromUnity(); // Package List - Downloaded AS Packages
                    optionResult += "Updating PAL in the background ...";
                }
            }
#endif

            if (!string.IsNullOrEmpty(optionResult))
                optionResult += "\n";

            refreshDetectedUpdate = false;
            refreshDetectedUpdateOnStore = false;

            // -- END Native/Remote Package Data collection

            string swEnd = GYAExt.StopwatchElapsed(false);

            scanResults = "Packages Refreshed: " + swEnd + " - All:   " + GYA.gyaVars.FilesCount.all +
                          "   Store:   " + GYA.gyaVars.FilesCount.store + "   User:   " + GYA.gyaVars.FilesCount.user +
                          "   " + GYAExt.FolderUnityStandardAssets + ":   " + GYA.gyaVars.FilesCount.standard +
                          "   Old:  " + GYA.gyaVars.FilesCount.old + "   Old To Consolidate:  " +
                          GYA.gyaVars.FilesCount.oldToMove + "   Project:  " + GYA.gyaVars.FilesCount.project;

            // Refresh results & per collection results - Silent? Still show if downloading data
            //if ((!GYA.gyaVars.Prefs.isSilent) || (GYA.gyaVars.Prefs.isSilent && !string.IsNullOrEmpty(optionResult)))
            GYAExt.Log(scanResults, optionResult + "\n" + collResults);

            EditorUtility.ClearProgressBar();

            // Auto-Update GYA if enabled
            if (GYA.gyaVars.Prefs.autoUpdateGYA && GYA.gyaVars.FilesCount.store > 0)
            {
                GYA.Instance.UpdateToLatestVersionOfGYA();
            }
        }

        // Remove bad entries to force a rescan if needed
        internal static void AssetForceRemoveAndRescan()
        {
            // Remove old entries as some recent assets had a title of Untitled and would show up as such in the list, those now use the filename as the title
            //GYA.gyaData.Assets.RemoveAll(x => x.title == "Untitled");
            // Remove any entries that may have been scanned from the Unity/cache folder as those are handled by Unity
            GYA.gyaData.Assets.RemoveAll(x => x.filePath.Contains(GYAExt.PathFixedForOS(Path.Combine(GYAExt.PathUnityDataFiles, "cache"))));
        }

        // Remove entry from collection, checks on filePath & collection
        internal static void AssetIndexRemoveEntry(String assetFile, GYA.svCollection collection)
        {
            GYA.gyaData.Assets.RemoveAll(x => x.filePath == assetFile && x.collection == collection);
        }

        // Remove all entries from collection
        internal static void AssetIndexRemoveEntryAll(GYA.svCollection collection)
        {
            GYA.gyaData.Assets.RemoveAll(x => x.collection == collection);
        }

        // Add entry to gyaData
        internal static void AssetIndexAddEntry(String assetFile, GYA.svCollection collection)
        {
            string jsonText = GetPackageInfoFromFile(assetFile, collection);

            if (jsonText.Length != 0)
            {
                jsonText = "{\"Assets\":[" + jsonText + "]}";

                GYAData scanData = new GYAData();
                scanData = JsonConvert.DeserializeObject<GYAData>(jsonText);

                IEnumerable<GYAData.Asset> scanIE = from package in scanData.Assets select package;
                GYA.gyaData.Assets.AddRange(scanIE);
            }
        }

        // Refresh specified Collection scanning for file add/remove
        internal static string RefreshCollection(GYA.svCollection pCollection, String assetPath,
            bool showRefresh = true)
        {
            return RefreshCollection(pCollection, new[] { assetPath }, showRefresh);
        }

        internal static string RefreshCollection(GYA.svCollection pCollection, String[] assetPath,
            bool showRefresh = true)
        {
            var toScan = new[]
            {
                "Gathering File Info", "Assets In Collection", "Calculating Files To Skip", "Removing outdated entries",
                "Calculating Files To Scan"
            };
            using (
                var progressBar = new GYA.ProgressBar(
                    string.Format("{0} Refreshing {1}", GYA.gyaVars.abbr, pCollection),
                    toScan.Length,
                    1,
                    stepNumber => pCollection + " - " + toScan[stepNumber] + " ...",
                    showRefresh
                )
            )
            {
                Dictionary<string, string> fi = new Dictionary<string, string>();
                Dictionary<string, string> assetCollection = new Dictionary<string, string>();
                HashSet<string> filesToSkip = new HashSet<string>();
                HashSet<string> assetsToRemove = new HashSet<string>();
                List<string> filesToScan = new List<string>();
                DirectoryInfo di;
                IEnumerable<FileInfo> fi2 = Enumerable.Empty<FileInfo>();
                IEnumerable<FileInfo> fiTmp = Enumerable.Empty<FileInfo>();
                IEnumerable<FileInfo> fiTmp2 = Enumerable.Empty<FileInfo>();

                // -- Folders/sub-folders to scan for unitypackages
                progressBar.Update(0);
                foreach (var dir in assetPath)
                {
                    if (!Directory.Exists(dir))
                        continue; // Skip folder if not exist

                    di = new DirectoryInfo(dir);

                    try
                    {
                        //If scanning PathUnityDataFiles, ignore PathGYADataFiles
                        if (pCollection == GYA.svCollection.Store)
                        {
                            //fiTmp = di.GetFiles("*.unity?ackage", SearchOption.AllDirectories).Where(x => !x.DirectoryName.Contains(GYAExt.PathFixedForOS(GYAExt.PathGYADataFiles)));
                            fiTmp = di.GetFiles("*.unity?ackage", SearchOption.AllDirectories)
                                      .Where(x => x.DirectoryName.Contains(GYAExt.PathFixedForOS(Path.Combine(GYAExt.PathUnityDataFiles, "Asset Store"))));
                        }
                        else
                        {
                            fiTmp = di.GetFiles("*.unity?ackage", SearchOption.AllDirectories);
                        }
                    }
                    catch (System.Exception e)
                    {
                        GYAExt.LogWarning("Please check your folder structure as there was an issue detected while scanning the folder:", e.ToString());
                    }

                    fiTmp2 = fi2.Concat(fiTmp);
                    fi2 = fiTmp2;
                }
                fiTmp2 = null;
                fiTmp = null;
                di = null;

                fi = fi2.ToDictionaryIgnoreDupes(a => a.FullName, b => GYAExt.DateAsISO8601(b.CreationTimeUtc));
                fi2 = null;

                // -- Assets to check by collection
                progressBar.Update(1);
                assetCollection = GYA.gyaData.Assets.FindAll(x => x.collection == pCollection)
                    .ToDictionary(a => a.filePath, b => GYAExt.DateAsISO8601(b.fileDateCreated));

                // -- Calculate Files to Skip that don't require updating
                progressBar.Update(2, " (Out of " + fi.Count + ")");
                filesToSkip = fi.Where(x => assetCollection.Any(y => y.Key == x.Key && x.Value == y.Value))
                    .Select(x => x.Key).ToHashSet();

                // -- Removes assets from index for files that no longer exist
                progressBar.Update(3, " (Out of " + assetCollection.Count + ")");
                if (assetCollection.Count != filesToSkip.Count)
                {
                    assetsToRemove = assetCollection.Where(x => filesToSkip.All(y => y != x.Key))
                        .Select(x => x.Key).ToHashSet();

                    if (assetsToRemove.Count > 0)
                        GYA.gyaData.Assets.RemoveAll(x => assetsToRemove.Contains(x.filePath));
                }

                // -- Add files that are new/changed
                progressBar.Update(4);
                if (fi.Count != filesToSkip.Count)
                {
                    filesToScan = fi.Where(x => filesToSkip.All(y => y != x.Key)).Select(x => x.Key).ToList();

                    using (
                            var progressBar2 = new GYA.ProgressBar(
                                string.Format("{0} Refreshing {1}", GYA.gyaVars.abbr, pCollection),
                                filesToScan.Count,
                                80,
                                stepNumber => pCollection + " - " +
                                                filesToScan[stepNumber]
                                                    .Substring(
                                                        Path.GetDirectoryName(filesToScan[stepNumber]).Length + 1),
                                showRefresh
                            )
                        )
                        // Process unitypackage files
                        for (int i = 0; i < filesToScan.Count; ++i)
                        {
                            progressBar2.Update(i);
                            AssetIndexAddEntry(filesToScan[i], pCollection);
                        }
                }

                EditorUtility.ClearProgressBar();

                // If add/remove detected
                if (assetsToRemove.Count > 0 || filesToScan.Count > 0)
                    refreshDetectedUpdate = true;
                // If add/remove detected ONLY for Store
                if (pCollection == GYA.svCollection.Store && (assetsToRemove.Count > 0 || filesToScan.Count > 0))
                    refreshDetectedUpdateOnStore = true;

                int padWidth = 20;
                string results = pCollection.ToString().PadRight(padWidth) +
                    ("\tLast Index: " + assetCollection.Count).PadRight(padWidth) +
                    ("\tFiles: " + fi.Count).PadRight(padWidth) +
                    ("\tSkip: " + filesToSkip.Count).PadRight(padWidth) +
                    ("\tRemove: " + assetsToRemove.Count).PadRight(padWidth) +
                    ("\tAdd: " + filesToScan.Count).PadRight(padWidth) + "\n";

                return showRefresh ? results : String.Empty;
            }
        }
    }

    public static class EnumerableExtensions
    {
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
        {
            return source.ToHashSet<T>(null);
        }

        public static HashSet<T> ToHashSet<T>(
            this IEnumerable<T> source, IEqualityComparer<T> comparer)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            return new HashSet<T>(source, comparer);
        }

        public static Dictionary<K, V> ToDictionaryIgnoreDupes<TSource, K, V>(
            this IEnumerable<TSource> source,
            Func<TSource, K> keySelector,
            Func<TSource, V> valueSelector)
        {
            Dictionary<K, V> output = new Dictionary<K, V>();
            foreach (TSource item in source)
            {
                //ignores future duplicates, comment above and uncomment below to change behavior
                K key = keySelector(item);
                if (!output.ContainsKey(key))
                {
                    output.Add(key, valueSelector(item));
                }
            }
            return output;
        }
    }
}
