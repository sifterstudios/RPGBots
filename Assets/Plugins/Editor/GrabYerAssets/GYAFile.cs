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

//#define UseSQLite
#if UseSQLite
using GYAInternal.SQLite.Net;
#endif

using UnityEditor;
using UnityEngine;
using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32.SafeHandles;
using System.ComponentModel;
using System.Runtime.InteropServices;
using GYAInternal.Json;
using GYAInternal.Json.Linq;
using System.Text.RegularExpressions;

namespace XeirGYA
{
    public partial class GYAFile
    {
        public static bool IsSQLiteDatabase(string pathToFile)
        {
            bool result = false;
            if (File.Exists(pathToFile))
            {
                using (FileStream stream = new FileStream(pathToFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    byte[] header = new byte[16];
                    for (int i = 0; i < 16; i++)
                        header[i] = (byte)stream.ReadByte();
                    result = System.Text.Encoding.UTF8.GetString(header).Contains("SQLite format 3");
                    stream.Close();
                }
            }
            return result;
        }

        //Basic cookies class for Unity Cookies file
        public class Cookies
        {
            public string name { get; set; }
            public string value { get; set; }
        }

#if UseSQLite

        //SQLite access to Unity Cookies file (sqlite)
        public static string GetFromUnityCookies_KharmaSession()
        {
            var stringToFind = "kharma_session";

            string cookiePath = GYAExt.PathUnityCookiesFile;

            FileInfo fi = new FileInfo(cookiePath);
            string fileFullName = fi.FullName;
            fileFullName = GYAExt.PathFixedForOS(fileFullName);

            if (File.Exists(fileFullName) && IsSQLiteDatabase(fileFullName))
            {
                var conn = new SQLiteConnection(fileFullName);
                var query = conn.Query<Cookies>("select * from cookies where name = ?", stringToFind);

                if (query.Count() > 0)
                {
                    foreach (var row in query)
                    {
                        //GYAExt.Log("Row: " + query.Count());
                        return row.value;
                    }
                }

                conn.Close();
            }

            return ""; // Not found
        }

#else
		
	    // No longer called as PAL retrieval no longer works with the updated Asset Store
        //Brute force READ ONLY from Unity Cookies file (sqlite)
        //This is not pretty or clean, but it does the trick
        //and avoids loading sqlite to access just one field
        public static string GetFromUnityCookies_KharmaSession()
        {
            var stringToFind = "kharma_session";
            var stringLength = 86;

            string cookiePath = GYAExt.PathUnityCookiesFile;

            FileInfo fi = new FileInfo(cookiePath);
            string fileFullName = fi.FullName;
            fileFullName = GYAExt.PathFixedForOS(fileFullName);

            List<string> foundStrings = new List<string>();

            try
            {
                if (File.Exists(fileFullName) && IsSQLiteDatabase(fileFullName))
                {
                    byte[] bStringToFind = Encoding.UTF8.GetBytes(stringToFind);

                    using (FileStream fs = File.Open(fileFullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        int byteRead;

                        do
                        {
                            byteRead = fs.ReadByte();
                            if (byteRead == -1) continue;
                            bool found = true;
                            foreach (byte t in bStringToFind)
                            {
                                if ((byte)byteRead == t)
                                {
                                    byteRead = fs.ReadByte();
                                    if (byteRead != -1) continue;
                                    found = false;
                                    break;
                                }

                                found = false;
                                break;
                            }
                            if (found)
                            {
                                fs.Seek(-1, SeekOrigin.Current);

                                byte[] bytes = new byte[86];
                                int position = 0;

                                int nbBytesRead = fs.Read(bytes, position, stringLength);
                                if (nbBytesRead == 0)
                                {
                                    found = false;
                                    break;
                                }

                                foundStrings.Add(Encoding.UTF8.GetString(bytes, 0, stringLength));

                                // Check for generic session
                                if (foundStrings[0].StartsWith("26c4202eb475d02864b40827dfff11a14657aa41"))
                                    return "";

                                return foundStrings[0]; // Return 1st entry
                            }
                        } while (byteRead != -1);
                    }
                }
            }
            catch (Exception e)
            {
                GYAExt.LogWarning("GetDataFromUnityCookies - Reading file: " + cookiePath, "" + e.Message);
            }
            return ""; // Not found
        }

#endif

        // Copy to clipboard, default as JSON
        internal static void CopyToClipboard(object obj, bool asString = false)
        {
            TextEditor te = new TextEditor();

            if (asString) // as String, no quotes
            {
#if UNITY_5_2_OR_OLDER
                te.content = new GUIContent(obj.ToString());
#else
                te.text = obj.ToString();
#endif
            }
            else // as JSON
            {
#if UNITY_5_2_OR_OLDER
                te.content = new GUIContent(GYAExt.ToJson(obj, true));
#else
                te.text = GYAExt.ToJson(obj, true);
#endif
            }

            te.SelectAll();
            te.Copy();
        }

        internal static Dictionary<string, string> GetGUIDsAndPathsForAllAssets()
        {
            // Gather GUID & Path for All Assets
            var assets = AssetDatabase.GetAllAssetPaths().ToList();
            Dictionary<string, string> guidList = assets.ToDictionary(asset => AssetDatabase.AssetPathToGUID(asset));
            CopyToClipboard(guidList);
            return guidList;
        }

        // Extract a fragment "property" from JSON and return it as a structure <T>
        internal static T DeserializeJSONPropertyAs<T>(string pJsonText, string pPropertyName)
        {
            var root = JObject.Parse(pJsonText);
            var serializer = new JsonSerializer();
            return serializer.Deserialize<T>(root[pPropertyName].CreateReader());
        }

        internal static long GetDirectorySize(FileInfo[] files)
        {
            return files.Sum(f => f.Length);
        }

        internal static long GetDirectorySize(string path, string searchPattern = "*.*")
        {
            // Get array of all file names.
            string[] files = Directory.GetFiles(path, searchPattern);

            // Calculate total bytes of all files in a loop.
            return files.Select(f => new FileInfo(f)).Select(fi => fi.Length).Sum();
        }

        // ConvertGYAv2Tov3
        internal static void ConvertGYAv2Tov3()
        {
            // Original conversion v2 to v3 (pre and post 3.19)
            string jsonFileUserOLD = Path.Combine(GYAExt.PathGYADataFiles, "GYA Settings.json");

            if ((!File.Exists(GYA.gyaVars.Files.Prefs.file)) && File.Exists(jsonFileUserOLD))
            {
                GYAExt.LogWarning("Converting Settings & Groups to v3.",
                    "Please upgrade All projects that use GYA to v3.");

                File.SetAttributes(jsonFileUserOLD, FileAttributes.Normal);
                string jsonText = File.ReadAllText(jsonFileUserOLD);

                // -- Perform Settings changes here
                JObject json = JObject.Parse(jsonText);
                var root = json["Settings"];
                string pathUserAssets = (string)root["pathUserAssets"];
                JArray pathUserAssetsList = (JArray)root["pathUserAssetsList"];

                // Update fields
                pathUserAssetsList.Insert(0, pathUserAssets); // Merge pathUserAssets
                root["pathUserAssets"].Replace(""); // Clear field
                root["pathUserAssetsList"] = pathUserAssetsList; // Replace with merged data

                // remove: "pathUserAssets": "", & rename pathUserAssetsList
                var prefsText = json.ToString().Replace("\"pathUserAssets\": \"\",", "");
                prefsText = prefsText.Replace("\"pathUserAssetsList\"", "\"pathUserAssets\"");

                // Extract 'settings' fragment
                GYA.gyaVars.Prefs = DeserializeJSONPropertyAs<GYAPrefs>(prefsText, "Settings");
                SaveGYAPrefs();

                // Extract 'groups' fragment
                GYA.gyaData.Groups = DeserializeJSONPropertyAs<List<GYAData.Group>>(prefsText, "Group");
                SaveGYAGroups();

                // -- Backup v2 file, leave original in case other projects have not been upgraded yet
                string targetFile = jsonFileUserOLD.Replace(".json", ".v2");
                if (!File.Exists(targetFile))
                    File.Copy(jsonFileUserOLD, targetFile);

                GYAExt.Log("Old Settings backed up with extension '.v2'.",
                    "Next 'Refresh' will update your asset collection to v3 if required.");
            }

            // Extended conversion to handle pref changes from v3 to 3.19.x.x
            string jsonFilePrefsOLD = Path.Combine(GYAExt.PathGYADataFiles, "GYA Prefs.json");

            if ((!File.Exists(GYA.gyaVars.Files.Prefs.file)) && File.Exists(jsonFilePrefsOLD))
            {
                GYAExt.LogWarning("Converting Prefs to v3 (3.19.x.x).",
                    "Please upgrade All projects that use GYA to v3.19.x.x or newer.");

                File.SetAttributes(jsonFilePrefsOLD, FileAttributes.Normal);

                // -- Copy pre-v3.19 to v3 file, leave original in case other projects have not been upgraded yet
                if (!File.Exists(GYA.gyaVars.Files.Prefs.file))
                    File.Copy(jsonFilePrefsOLD, GYA.gyaVars.Files.Prefs.file);

                //// -- Backup v3 file, leave original in case other projects have not been upgraded yet
                //string targetFile = jsonFilePrefsOLD.Replace(".json", ".v3");
                //if (!File.Exists(targetFile))
                //    File.Copy(jsonFilePrefsOLD, targetFile);

                //GYAExt.Log("v3 (pre-v3.19) Prefs backed up with extension '.v3'.");
            }
        }

        internal static bool IsPALValid(string stringPAL)
        {
            // Search for "\"name\": \"Packages\"," .. OR .. "\"items\": [", if found then PAL is pre-May 15th 2017

            if (stringPAL.Contains("\"name\": \"Packages\",") && stringPAL.Contains("\"items\": ["))
            {
                GYAExt.Log("'Purchased Assets List' has invalid structure.  Deleting.",
                    "Refresh to download current PAL.");
                File.Delete(GYA.gyaVars.Files.ASPurchase.file);
                GYA.gyaVars.Files.ASPurchase.fileExists = false;

                return false;
            }

            return true;
        }

        // Load AS Packages
        internal static bool LoadASPackages()
        {
            if (!File.Exists(GYA.gyaVars.Files.ASPackage.file))
            {
                GYA.gyaVars.Files.ASPackage.fileExists = false;
                return false;
            }

            GYA.gyaVars.Files.ASPackage.fileExists = true;
            string jsonText = File.ReadAllText(GYA.gyaVars.Files.ASPackage.file);
            GYA.asPackages = JsonConvert.DeserializeObject<ASPackageList>(jsonText);

            return true;
        }

        // SaveGYAGroups 
        internal static void SaveGYAUserData(bool append = false)
        {
            // Check for dupes before saving?! or is it fixed with recent change

            // Save AS and non-AS notes
            List<GYAAssetInfo.AssetInfo> assetInfo = GYA.gyaData.Assets.Where(y => y.AssetInfo.id > 0 || y.AssetInfo.filePath.Length > 0).Select(x => x.AssetInfo).Distinct().ToList();

            TextWriter writer = null;
            var objectToWrite = assetInfo;

            try
            {
                if (File.Exists(GYA.gyaVars.Files.AssetInfo.file))
                    File.SetAttributes(GYA.gyaVars.Files.AssetInfo.file, FileAttributes.Normal);

                var contentsToWriteToFile = JsonConvert.SerializeObject(objectToWrite, Formatting.Indented);
                writer = new StreamWriter(GYA.gyaVars.Files.AssetInfo.file, append);
                writer.Write(contentsToWriteToFile);
            }
            catch (Exception ex)
            {
                GYAExt.LogWarning("SaveGYAUserData Error: ", ex.Message);
                GYA.Instance.ErrorStateSet(GYA.ErrorCode.Error);
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }
        }

        internal static bool LoadGYAUserData()
        {
            if (!File.Exists(GYA.gyaVars.Files.AssetInfo.file))
            {
                GYA.gyaVars.Files.AssetInfo.fileExists = false;
                return false;
            }

            var assetInfo = new List<GYAAssetInfo.AssetInfo>();

            GYA.gyaVars.Files.AssetInfo.fileExists = true;
            string jsonText = File.ReadAllText(GYA.gyaVars.Files.AssetInfo.file);
            assetInfo = JsonConvert.DeserializeObject<List<GYAAssetInfo.AssetInfo>>(jsonText);

            // Place notes with assets
            if (assetInfo != null && assetInfo.Count > 0)
            {
                foreach (var t in assetInfo)
                {
                    if (t.id != 0)
                        // For asset store files id > 0, compare via id
                        GYA.gyaData.Assets.FindAll(x => x.id == t.id).ForEach(y => y.AssetInfo = t);
                    else
                        // For non-asset store files, compare via filePath
                        GYA.gyaData.Assets.FindAll(x => x.id == t.id && x.filePath == t.filePath).ForEach(y => y.AssetInfo = t);
                }
            }

            return true;
        }

        // Load AS Purchased
        internal static bool LoadASPurchased()
        {
            if (!File.Exists(GYA.gyaVars.Files.ASPurchase.file))
            {
                GYA.gyaVars.Files.ASPurchase.fileExists = false;
                return false;
            }

            try
            {
                string jsonText = File.ReadAllText(GYA.gyaVars.Files.ASPurchase.file);

                // Add code to detect if PAL has changed, if it has then delete outdated file
                if (IsPALValid(jsonText))
                {
                    GYA.asPurchased = JsonConvert.DeserializeObject<ASPurchased>(jsonText);
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                GYAExt.LogWarning("LoadASPurchased - Deleting file: " + GYA.gyaVars.Files.ASPurchase.file,
                    "It will be re-created during the next 'Refresh' if required.\n\n" + e.Message);

                if (File.Exists(GYA.gyaVars.Files.ASPurchase.file))
                    File.Delete(GYA.gyaVars.Files.ASPurchase.file);

                return false;
            }

            GYA.gyaVars.Files.ASPurchase.fileExists = true;
            return true;
        }

        // Load GYA Settings
        internal static bool LoadGYAPrefs()
        {
            if (!File.Exists(GYA.gyaVars.Files.Prefs.file))
            {
                SaveGYAPrefs();
            }
            try
            {
                GYA.gyaVars.Files.Prefs.fileExists = true;
                string jsonText = File.ReadAllText(GYA.gyaVars.Files.Prefs.file);
                GYA.gyaVars.Prefs = JsonConvert.DeserializeObject<GYAPrefs>(jsonText);
            }
            catch (Exception e)
            {
                GYAExt.LogWarning("LoadGYAPrefs - Error loading User Prefs: ", e.Message);
                GYA.Instance.ErrorStateSet(GYA.ErrorCode.Error);
                return false;
            }

            return true;
        }

        // Load GYA Groups
        internal static bool LoadGYAGroups()
        {
            // Check if exists
            if (!File.Exists(GYA.gyaVars.Files.Groups.file))
            {
                GYA.Instance.GroupCreate("Favorites");
                SaveGYAGroups();
            }

            GYA.gyaVars.Files.Groups.fileExists = true;
            string jsonText = File.ReadAllText(GYA.gyaVars.Files.Groups.file);
            GYA.gyaData.Groups = JsonConvert.DeserializeObject<List<GYAData.Group>>(jsonText);

            return true;
        }

        // Create or over-write the json file for user info
        internal static void SaveGYAPrefs(bool append = false)
        {
            // Make any last minute changes to User data prior to saving
            GYA.gyaData.version = GYA.gyaVars.version;

            TextWriter writer = null;
            GYAPrefs objectToWrite = GYA.gyaVars.Prefs;

            try
            {
                if (File.Exists(GYA.gyaVars.Files.Prefs.file))
                    File.SetAttributes(GYA.gyaVars.Files.Prefs.file, FileAttributes.Normal);

                var contentsToWriteToFile = JsonConvert.SerializeObject(objectToWrite, Formatting.Indented);
                writer = new StreamWriter(GYA.gyaVars.Files.Prefs.file, append);
                writer.Write(contentsToWriteToFile);
            }
            catch (Exception ex)
            {
                GYAExt.LogWarning("SaveGYAPrefs Error: ", ex.Message);
                GYA.Instance.ErrorStateSet(GYA.ErrorCode.Error);
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }
        }

        // Create or over-write the json file for user Groups
        internal static void SaveGYAGroups(bool append = false)
        {
            // Make any last minute changes to User data prior to saving
            GYA.gyaData.version = GYA.gyaVars.version;

            TextWriter writer = null;
            List<GYAData.Group> objectToWrite = GYA.gyaData.Groups;

            try
            {
                if (File.Exists(GYA.gyaVars.Files.Groups.file))
                    File.SetAttributes(GYA.gyaVars.Files.Groups.file, FileAttributes.Normal);

                var contentsToWriteToFile = JsonConvert.SerializeObject(objectToWrite, Formatting.Indented);
                writer = new StreamWriter(GYA.gyaVars.Files.Groups.file, append);
                writer.Write(contentsToWriteToFile);
            }
            catch (Exception ex)
            {
                GYAExt.LogWarning("SaveGYAGroups Error: ", ex.Message);
                GYA.Instance.ErrorStateSet(GYA.ErrorCode.Error);
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }
        }

        // Load the package data from file: Grab Yer Assets.json
        public static bool LoadGYAAssets()
        {
            GYA.gyaVars.Files.Assets.fileExists = true;
            string jsonText = File.ReadAllText(GYA.gyaVars.Files.Assets.file);

            try
            {
                GYA.gyaData = JsonConvert.DeserializeObject<GYAData>(jsonText);
                LoadGYAGroups();

                // verify JSON
                if (GYA.Instance.JSONObjectsAreNotNULL)
                {
                    // Post-proc json

                    GYA.gyaData.Assets.Sort((x, y) => -x.version_id.CompareTo(y.version_id));

                    foreach (GYAData.Asset t in GYA.gyaData.Assets)
                    {
                        // Check for damaged asset
                        if (t.isDamaged)
                        {
                            if (t.title
                                .StartsWith("unknown", StringComparison.InvariantCultureIgnoreCase))
                            {
                                var newTitle = GYAPackage.GetAssetNameFromOldIDIfExist(t.id, t.version_id);
                                t.title = newTitle;
                            }
                        }
                    }


                    // Calculate the old assets
                    //GYAPackage.BuildOldAssetsList();
                    GYA.Instance.RefreshSV();

                    // Check if loaded file is same major version, if not, save current version
                    if (!isAssetsFileSameVersion())
                    {
                        SaveGYAAssets();
                    }
                }

                //ProcessASPurchased();
            }
            catch (Exception ex)
            {
                GYAExt.LogError("LoadGYAAssets: ", ex.Message);
                GYA.Instance.ErrorStateSet(GYA.ErrorCode.ErrorStep2);
            }

            return true;
        }

        // Process asPurchased - check for deprecated assets, etc.
        public static void ProcessASPurchased()
        {
            if (GYA.asPurchased.results != null && GYA.asPurchased.results.Count > 0)
            {
                foreach (GYAData.Asset t in GYA.gyaData.Assets)
                {
                    var vTmp = GYA.asPurchased.results.Find(x => x.id == t.id);
                    if (vTmp != null) // version_id's match
                    {
                        t.Purchased = vTmp;
                        t.isInPurchasedList = true;
                        t.isDeprecated = (vTmp.status.ToLower() == "deprecated");
                        if (vTmp.purchased_at != null)
                        {
                            t.datePurchased = GYAExt.DateStringAsDTO(vTmp.purchased_at.ToString());
                        }
                        else
                        {
                            var tempDateTime = Convert.ToDateTime(DateTime.MinValue);
                            tempDateTime = DateTime.SpecifyKind(tempDateTime, DateTimeKind.Utc);
                            t.datePurchased = tempDateTime;
                        }

                        if (vTmp.created_at != null)
                        {
                            t.dateCreated = GYAExt.DateStringAsDTO(vTmp.created_at.ToString());
                        }
                        else
                        {
                            var tempDateTime = Convert.ToDateTime(DateTime.MinValue);
                            tempDateTime = DateTime.SpecifyKind(tempDateTime, DateTimeKind.Utc);
                            t.dateCreated = tempDateTime;
                        }

                        if (vTmp.updated_at != null)
                        {
                            t.dateUpdated = GYAExt.DateStringAsDTO(vTmp.updated_at.ToString());
                        }
                        else
                        {
                            var tempDateTime = Convert.ToDateTime(DateTime.MinValue);
                            tempDateTime = DateTime.SpecifyKind(tempDateTime, DateTimeKind.Utc);
                            t.dateUpdated = tempDateTime;
                        }
                        t.icon = vTmp.icon;
                    }
                }
            }
        }

        public static bool isAssetsFileSameVersion()
        {
            bool isSameMajorVersion = GYA.gyaData.version[0] == GYA.gyaVars.version[0];
            return isSameMajorVersion;
        }

        // Return JSON As Formatted JSON
        public static string JsonAsFormatted(string json, bool asIndented = true)
        {
            try
            {
                using (var stringReader = new StringReader(json))
                using (var stringWriter = new StringWriter())
                {
                    var jsonReader = new JsonTextReader(stringReader);
                    var jsonWriter = new JsonTextWriter(stringWriter)
                    {
                        Formatting = (asIndented ? Formatting.Indented : Formatting.None),
                        Culture = System.Globalization.CultureInfo.InvariantCulture,
                        DateFormatHandling = DateFormatHandling.IsoDateFormat,
                        DateTimeZoneHandling = DateTimeZoneHandling.Utc
                    };

                    jsonWriter.WriteToken(jsonReader);
                    return stringWriter.ToString();
                }
            }
            catch (Exception ex)
            {
                GYAExt.LogWarning("JsonAsFormatted: ", ex.Message);
                return null;
            }
        }

        internal static void SaveFile(string pFile, string pText, bool pAddGYADataPath = false)
        {
            if (pAddGYADataPath)
            {
                // Default to GYA Data folder
                pFile = Path.GetFullPath(Path.Combine(GYAExt.PathGYADataFiles, pFile));
            }

            try
            {
                if (File.Exists(pFile))
                {
                    File.SetAttributes(pFile, FileAttributes.Normal);
                    File.Delete(pFile);
                }

                using (StreamWriter sw = new StreamWriter(pFile))
                {
                    sw.Write(pText);
                }
            }
            catch (Exception ex)
            {
                GYAExt.LogWarning("SaveFile: " + pFile, ex.Message, false);
            }
        }

        // Multi-stage save: <file>.tmp, to JsonAsFormatted(x), to <file>
        internal static void SaveFileJson(string pFile, string pText, bool pAddGYADataPath = false)
        {
            string pFileTmp = pFile + ".tmp";

            // Save as <file>.tmp
            SaveFile(pFileTmp, pText, pAddGYADataPath);

            // Save file as <file>
            var pFormatted = JsonAsFormatted(pText);
            if (pFormatted != null)
            {
                SaveFile(pFile, pFormatted, pAddGYADataPath);
                // Delete tmp file
                if (File.Exists(pFileTmp))
                    File.Delete(pFileTmp);
            }
            else
            {
                GYAExt.LogWarning("SaveFileJson - File may be invalid: " + pFileTmp);
            }
        }

        // Create or over-write the json file for package info
        // Blank = save directly from gyaData.Assets
        public static void SaveGYAAssets(string jsonToWrite = "")
        {
            GYA.gyaData.version = GYA.gyaVars.version; // Update the version for the file

            TextWriter writer = null;
            string contentsToWriteToFile = String.Empty;

            try
            {
                if (File.Exists(GYA.gyaVars.Files.Assets.file))
                    File.SetAttributes(GYA.gyaVars.Files.Assets.file, FileAttributes.Normal);

                if (jsonToWrite.Length == 0)
                {
                    // Save just the Assets
                    var gyaTmp = new GYAData
                    {
                        version = GYA.gyaData.version,
                        Assets = GYA.gyaData.Assets
                    };

                    contentsToWriteToFile = JsonConvert.SerializeObject(gyaTmp, Formatting.Indented);
                    gyaTmp = null;
                }
                else
                {
                    var parsedJSON = JsonConvert.DeserializeObject(jsonToWrite);
                    contentsToWriteToFile = JsonConvert.SerializeObject(parsedJSON, Formatting.Indented);
                }

                writer = new StreamWriter(GYA.gyaVars.Files.Assets.file, false);
                writer.Write(contentsToWriteToFile);
            }
            catch (Exception ex)
            {
                GYAExt.LogWarning("SaveGYAAssets Error: ", ex.Message);
                GYA.Instance.ErrorStateSet(GYA.ErrorCode.Error);
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }
        }

        // Backup the "Grab Yer Assets User.json" file
        internal static void BackupUserFiles()
        {
            string backupExt = "bak";
            string jsonFilePrefsBackup = Path.ChangeExtension(GYA.gyaVars.Files.Prefs.file, backupExt);
            string jsonFileGroupsBackup = Path.ChangeExtension(GYA.gyaVars.Files.Groups.file, backupExt);

            GYAExt.Log(
                "Backing up as:\t" + Path.GetFileName(jsonFilePrefsBackup) + " & " +
                Path.GetFileName(jsonFileGroupsBackup), "\tTo:\t" + Path.GetDirectoryName(jsonFilePrefsBackup)
            );

            // If data files exists load it, else create it
            if (Directory.Exists(GYAExt.PathGYADataFiles))
            {
                // Prefs
                if (File.Exists(GYA.gyaVars.Files.Prefs.file))
                {
                    File.SetAttributes(GYA.gyaVars.Files.Prefs.file, FileAttributes.Normal);
                    if (File.Exists(jsonFilePrefsBackup))
                        File.SetAttributes(jsonFilePrefsBackup, FileAttributes.Normal);

                    File.Copy(GYA.gyaVars.Files.Prefs.file, jsonFilePrefsBackup, true);
                }
                else
                {
                    GYAExt.LogWarning("User file not found: " + GYA.gyaVars.Files.Prefs.file);
                }

                // Groups
                if (File.Exists(GYA.gyaVars.Files.Groups.file))
                {
                    File.SetAttributes(GYA.gyaVars.Files.Groups.file, FileAttributes.Normal);
                    if (File.Exists(jsonFileGroupsBackup))
                        File.SetAttributes(jsonFileGroupsBackup, FileAttributes.Normal);

                    File.Copy(GYA.gyaVars.Files.Groups.file, jsonFileGroupsBackup, true);
                }
                else
                {
                    GYAExt.LogWarning("User file not found: " + GYA.gyaVars.Files.Groups.file);
                }
            }
        }

        // Restore the "Grab Yer Assets User.json" file
        internal static void RestoreUserFiles()
        {
            string backupExt = "bak";
            string jsonFilePrefsBackup = Path.ChangeExtension(GYA.gyaVars.Files.Prefs.file, backupExt);
            string jsonFileGroupsBackup = Path.ChangeExtension(GYA.gyaVars.Files.Groups.file, backupExt);

            GYAExt.Log(
                "Restoring:\t" + Path.GetFileName(jsonFilePrefsBackup) + " & " + Path.GetFileName(jsonFileGroupsBackup),
                "From:\t" + Path.GetDirectoryName(jsonFilePrefsBackup));

            // Prefs - If data files exists load it, else create it
            if (Directory.Exists(GYAExt.PathGYADataFiles))
            {
                if (File.Exists(jsonFilePrefsBackup))
                {
                    File.SetAttributes(jsonFilePrefsBackup, FileAttributes.Normal);
                    if (File.Exists(GYA.gyaVars.Files.Prefs.file))
                        File.SetAttributes(GYA.gyaVars.Files.Prefs.file, FileAttributes.Normal);

                    File.Copy(jsonFilePrefsBackup, GYA.gyaVars.Files.Prefs.file, true);
                }
                else
                {
                    GYAExt.LogWarning("User backup file not found: " + jsonFilePrefsBackup);
                }
            }

            // Groups - If data files exists load it, else create it
            if (Directory.Exists(GYAExt.PathGYADataFiles))
            {
                if (File.Exists(jsonFileGroupsBackup))
                {
                    File.SetAttributes(jsonFileGroupsBackup, FileAttributes.Normal);
                    if (File.Exists(GYA.gyaVars.Files.Groups.file))
                        File.SetAttributes(GYA.gyaVars.Files.Groups.file, FileAttributes.Normal);

                    File.Copy(jsonFileGroupsBackup, GYA.gyaVars.Files.Groups.file, true);
                }
                else
                {
                    GYAExt.LogWarning("User backup file not found: " + jsonFileGroupsBackup);
                }
            }
        }


        // Convert 0 to EMPTY field for CSV export
        internal static string CSVZeroToEmpty(int pInt)
        {
            return pInt == 0 ? "" : pInt.ToString();
        }

        internal static string WrapCSVCell(object obj)
        {
            bool mustQuote = true;
            string str = obj.ToString();

            if (mustQuote)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("\"");

                foreach (char nextChar in str)
                {
                    sb.Append(nextChar);
                    if (nextChar == '"')
                        sb.Append("\"");
                }

                sb.Append("\"");
                return sb.ToString();
            }
            return str;
        }

        // Save Embedded Asset Info as CSV
        internal static void SaveAsCSV(List<GYAData.Asset> pkgList, string path)
        {
            var result = new StringBuilder();
            var csvFile = new List<string>();
            TextWriter writer = null;

            csvFile.Add(
                "\"icon\",\"title\",\"link\",\"id\",\"pubdate\",\"version\",\"version_id\",\"unity_version\",\"category_label\",\"category_id\",\"publisher_label\",\"publisher_id\",\"filePath\",\"fileSize\",\"isExported\",\"collection\"");

            foreach (GYAData.Asset item in pkgList)
            {
                string assetURL = string.Empty;
                string assetIcon = string.Empty;

                // Ignore exported (non-Asset Store packages)
                if (!item.isExported)
                {
                    // Works for Google Sheets
                    // Numbers/Excel may encode # to %23 when clicking/sending the link to the browser
                    //assetURL = "https://www.assetstore.unity3d.com/#/" + item.link.type + "/" + item.link.id;
                    assetURL = "https://assetstore.unity.com/packages/slug/" + item.link.id;
                    assetURL = "=HYPERLINK(\"" + assetURL + "\", \"link\")";

                    if (!string.IsNullOrEmpty(item.icon))
                    {
                        assetIcon = "=IMAGE(\"http:";
                        assetIcon += item.icon;
                        assetIcon += "\")";
                    }
                }

                csvFile.Add(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15}",
                    WrapCSVCell(assetIcon),
                    WrapCSVCell(item.title),
                    WrapCSVCell(assetURL),
                    WrapCSVCell(CSVZeroToEmpty(item.id)),
                    WrapCSVCell(item.pubdate),
                    WrapCSVCell(item.version),
                    WrapCSVCell(CSVZeroToEmpty(item.version_id)),
                    WrapCSVCell(item.unity_version),
                    WrapCSVCell(item.category.label),
                    WrapCSVCell(CSVZeroToEmpty(item.category.id)),
                    WrapCSVCell(item.publisher.label),
                    WrapCSVCell(CSVZeroToEmpty(item.publisher.id)),
                    WrapCSVCell(item.filePath),
                    WrapCSVCell(item.fileSize),
                    WrapCSVCell(item.isExported),
                    WrapCSVCell(item.collection)
                ));
            }

            foreach (string line in csvFile)
            {
                result.AppendLine(line);
            }

            try
            {
                writer = new StreamWriter(path);
                writer.Write(result);
            }
            catch (Exception ex)
            {
                GYAExt.LogWarning("Exporting Error:", ex.Message);
            }
            finally
            {
                if (writer != null)
                    writer.Close();

                GYAExt.Log("Exported Asset List as: " + path);
            }
        }

        // Version for GYAStore - embed asset icon url for google sheets
        internal static void SaveAsCSVGroup(string path)
        {
            var result = new StringBuilder();
            var csvFile = new List<string>();
            TextWriter writer = null;

            csvFile.Add("\"icon\",\"title\",\"link\",\"version\",\"category_label\"");

            foreach (GYAData.Asset item in GYA.grpData[GYA.showGroup])
            {
                string assetURL = string.Empty;
                string assetIcon = string.Empty;

                // Asset Icon
                // Ignore exported (non-Asset Store packages)
                if (!item.isExported)
                {
                    // Retrieve icon url
                    if (!string.IsNullOrEmpty(item.icon))
                    {
                        assetIcon = "=IMAGE(\"http:";
                        assetIcon += item.icon;
                        assetIcon += "\")";
                    }

                    // Works for Google Sheets, Libre Office
                    // Numbers/Excel may encode # to %23 when clicking/sending the link to the browser

                    // Asset Link
                    //assetURL = "https://www.assetstore.unity3d.com/#/" + item.link.type + "/" + item.link.id;
                    assetURL = "https://assetstore.unity.com/packages/slug/" + item.link.id;
                    assetURL = "=HYPERLINK(\"" + assetURL + "\", \"link\")";
                }

                csvFile.Add(string.Format("{0},{1},{2},{3},{4}",
                    WrapCSVCell(assetIcon),
                    WrapCSVCell(item.title),
                    WrapCSVCell(assetURL),
                    WrapCSVCell(item.version),
                    WrapCSVCell(item.category.label)
                ));
            }

            foreach (string line in csvFile)
            {
                result.AppendLine(line);
            }

            try
            {
                writer = new StreamWriter(path);
                writer.Write(result);
            }
            catch (Exception ex)
            {
                GYAExt.LogWarning("Exporting Error: ", ex.Message);
            }
            finally
            {
                if (writer != null)
                {
                    writer.Close();
                }
                GYAExt.Log("Exported Group List as: " + path);
            }
        }

        internal static void SaveListAsCSV(string path)
        {
            var result = new StringBuilder();
            var csvFile = new List<string>();
            TextWriter writer = null;

            csvFile.Add("\"icon\",\"title\",\"link\",\"version\",\"category_label\"");

            List<GYAData.Asset> Assets = GYA.svData;

            foreach (GYAData.Asset item in Assets)
            {
                string assetURL = string.Empty;
                string assetIcon = string.Empty;

                // Asset Icon
                // Ignore exported (non-Asset Store packages)
                if (!item.isExported)
                {
                    // Retrieve icon url
                    if (!string.IsNullOrEmpty(item.icon))
                    {
                        assetIcon = "=IMAGE(\"http:";
                        assetIcon += item.icon;
                        assetIcon += "\")";
                    }

                    // Works for Google Sheets, Libre Office
                    // Numbers/Excel may encode # to %23 when clicking/sending the link to the browser

                    // Asset Link
                    //assetURL = "https://www.assetstore.unity3d.com/#/" + item.link.type + "/" + item.link.id;
                    assetURL = "https://assetstore.unity.com/packages/slug/" + item.link.id;
                    assetURL = "=HYPERLINK(\"" + assetURL + "\", \"link\")";
                }

                csvFile.Add(string.Format("{0},{1},{2},{3},{4}",
                    WrapCSVCell(assetIcon),
                    WrapCSVCell(item.title),
                    WrapCSVCell(assetURL),
                    WrapCSVCell(item.version),
                    WrapCSVCell(item.category.label)
                ));
            }

            foreach (string line in csvFile)
            {
                result.AppendLine(line);
            }

            try
            {
                writer = new StreamWriter(path);
                writer.Write(result);
            }
            catch (Exception ex)
            {
                GYAExt.LogWarning("Exporting Error: ", ex.Message);
            }
            finally
            {
                if (writer != null)
                {
                    writer.Close();
                }
                GYAExt.Log("Exported Group List as: " + path);
            }
        }

        // Move Contents from one folder to another accounting for symlinks, Create if required
        internal static int MoveFolderContents(string moveFrom, string moveTo, bool overwrite = false)
        {
            int count = 0;
            int countTotal = 0;
            string result = string.Empty;

            if (moveFrom != null && moveTo != null)
            {
                if (!Directory.Exists(moveFrom))
                {
                    // Folder missing, exit
                    GYAExt.Log("Source folder does NOT exist: " + moveFrom);
                }
                else
                {
                    if (!Directory.Exists(moveTo))
                        CreateFolder(moveTo);

                    DirectoryInfo directory = new DirectoryInfo(moveFrom);

                    if (directory.Exists)
                    {
                        FileInfo[] files = directory.GetFiles("*.unity?ackage", SearchOption.AllDirectories)
                            .Where(fi => (fi.Attributes & FileAttributes.Hidden) == 0).ToArray();

                        int filenameStartIndex = (directory.FullName.Length + 1);
                        using (
                            var progressBar = new GYA.ProgressBar(
                                string.Format("{0} Copying {1}", GYA.gyaVars.abbr, directory.FullName),
                                files.Length,
                                80,
                                stepNumber => files[stepNumber].FullName.Substring(filenameStartIndex)
                            )
                        )

                            for (int i = 0; i < files.Length; ++i)
                            {
                                var fileTo = Path.Combine(moveTo, files[i].Name);
                                progressBar.Update(i);
                                try
                                {
                                    File.SetAttributes(files[i].FullName, FileAttributes.Normal);
                                    countTotal += 1;
                                    if (File.Exists(fileTo) && !overwrite)
                                    {
                                        result += "EXISTS:\t" + files[i].FullName + "\n\tTo: " + fileTo + "\n";
                                    }
                                    else
                                    {
                                        if (IsSymLink(moveFrom) || IsSymLink(moveTo))
                                        {
                                            File.Copy(files[i].FullName, fileTo, overwrite);
                                            result += "Copied:\t" + files[i].FullName + "\n\tTo: " + fileTo + "\n";
                                        }
                                        else
                                        {
                                            if (overwrite)
                                            {
                                                File.Delete(fileTo);
                                                File.Move(files[i].FullName, fileTo);
                                            }
                                            else
                                            {
                                                File.Move(files[i].FullName, fileTo);
                                            }
                                            result += "Moved:\t" + files[i].FullName + "\n\tTo: " + fileTo + "\n";
                                        }
                                        // If successful, delete the old file
                                        if (File.Exists(fileTo))
                                        {
                                            count += 1;
                                            File.Delete(files[i].FullName);
                                        }
                                    }
                                }
                                catch (IOException ex)
                                {
                                    GYAExt.LogWarning("Error Moving: " + files[i].FullName,
                                        "To: " + fileTo + "\n\n" + ex.Message);
                                    result += "ERROR:\t" + files[i].FullName + "\n\tTo: " + fileTo + "\n";
                                }
                            }
                        if (countTotal > 0)
                        {
                            result = "Copied " + count + " of " + countTotal + "\n\n" + result;
                            GYAExt.Log(result);
                            GYAExt.Log("Once you have verified that your Old Assets have been moved without error,",
                                "you can use 'Menu->Maintenance->Clean up Outdated GYA Support Files' to cleanup the outdated files/folders.");
                            GYAPackage.RefreshAllCollections();
                        }
                    }
                }
            }
            return count;
        }

        // Create folder if not exist
        internal static void CreateFolder(string folder, bool silent = false)
        {
            try
            {
                if (!Directory.Exists(folder))
                {
                    // Default way
                    Directory.CreateDirectory(folder);

                    if (!silent)
                    {
                        GYAExt.Log("Created Folder:\t'" + folder + "'");
                    }
                }
            }
            catch (Exception ex)
            {
                GYAExt.LogError("Error Creating Folder:\t'" + folder + "'", ex.Message);
            }
        }

        internal static bool OldAssetNeedsToMove(List<GYAData.Asset> packages, int oldLine)
        {
            bool needsToMove = !(Path.GetFullPath(Path.GetDirectoryName(packages[oldLine].filePath)) ==
                                 GYA.gyaVars.pathOldAssetsFolder);
            return needsToMove;
        }

        internal static void DeleteAllFilesInOldAssetsFolder(bool needsRefresh = true)
        {
            string filesInfo = String.Empty;
            int filesDeleted = 0;
            List<GYAData.Asset> packageDelete = GYA.gyaData.Assets.FindAll(x => x.collection == GYA.svCollection.Old &&
                x.filePath.Contains(
                    GYA.gyaVars.pathOldAssetsFolder,
                    StringComparison
                    .OrdinalIgnoreCase));

            foreach (GYAData.Asset t in packageDelete)
            {
                // Delete asset
                var fileData = DeleteAsset(t);
                filesDeleted = filesDeleted + fileData.Key;
                filesInfo = filesInfo + fileData.Value.Split('\n')[1] + "\n";
            }
            if (filesDeleted > 0)
            {
                GYAExt.Log("( " + filesDeleted + " ) package(s) deleted from the Old Assets folder.",
                    filesInfo);
            }

            if (needsRefresh)
            {
                GYAPackage.RefreshOld(false);
                GYAPackage.RefreshStore(false);
                GYA.Instance.RefreshSV();
                SaveGYAAssets();
            }
        }

        // Delete the selected assets
        internal static void DeleteAssetMultiple(bool needsRefresh = true)
        {
            string filesInfo = String.Empty;
            int filesDeleted = 0;
            List<GYAData.Asset> packageDelete = GYA.gyaData.Assets.FindAll(x => x.isMarked);

            // Check all assets
            foreach (GYAData.Asset t in packageDelete)
            {
                // Is asset marked
                if (t.isMarked)
                {
                    // Delete asset
                    var fileData = DeleteAsset(t);
                    filesDeleted = filesDeleted + fileData.Key;
                    filesInfo = filesInfo + fileData.Value.Split('\n')[1] + "\n";
                }
            }
            if (filesDeleted > 0)
                GYAExt.Log("( " + filesDeleted + " ) package(s) deleted.", filesInfo);

            // Make sure list is up-to-date
            if (needsRefresh)
                GYAPackage.RefreshAllCollections();
        }

        // Move assets to the passed folder - defaults to old assets
        internal static KeyValuePair<int, string> DeleteAsset(GYAData.Asset packageMove)
        {
            string moveInfo = String.Empty;
            int filesDeleted = 0;
            string fileToDelete = Path.GetFullPath(packageMove.filePath);

            // Does file already exist at destination?
            if (File.Exists(fileToDelete))
            {
                // Yes it is
                moveInfo = moveInfo + "Deleting: " + packageMove.title + "\n" + "Path: " + fileToDelete + "\n";

                try
                {
                    // Delete the file
                    File.Delete(fileToDelete);

                    // Verification
                    if (File.Exists(fileToDelete))
                    {
                        GYAExt.LogWarning("Error: File not Deleted:", moveInfo);
                    }
                    else
                    {
                        GYA.gyaData.Assets.Remove(packageMove);
                        filesDeleted += 1;
                    }
                }
                catch (IOException ex)
                {
                    GYAExt.LogWarning("Error: File Delete Failed:", moveInfo + "\n\n" + ex.Message);
                }
            }
            else
            {
                GYAExt.Log("Unable to delete - File doesn't exist: " + fileToDelete);
            }
            return new KeyValuePair<int, string>(filesDeleted, moveInfo);
        }

        // Return full version suffix to append if official AS pkg: v<Asset Version> (<Unity Version>) (<version_id>)
        internal static string GetAssetVersionStringToAppend(GYAData.Asset packageName, bool returnOldVerStyle = false)
        {

            if (!packageName.isExported)
                return GYAPackage.CustomizePackageFilenameVersionSuffix(packageName, GYA.gyaVars.Prefs.userVersionString);
            return String.Empty;

            //string appendString = string.Empty;
            //string verString = packageName.version;
            //string uniString = packageName.unity_version;
            //string vidString = packageName.version_id.ToString();

            //if (!packageName.isExported)
            //{
            //    // Check for missing or blank tags
            //    if (string.IsNullOrEmpty(verString))
            //        verString = "";
            //    if (string.IsNullOrEmpty(uniString))
            //        uniString = "";
            //    if (string.IsNullOrEmpty(vidString))
            //        vidString = "";

            //    // Asset Version string
            //    if (verString.Length > 0)
            //        appendString = " v" + verString;

            //    // Unity Version string
            //    if (uniString.Length > 0)
            //        appendString = appendString + " (" + uniString + ")";

            //    // Version ID string
            //    if (!returnOldVerStyle)
            //    {
            //        if (vidString.Length > 0)
            //            appendString = appendString + " (" + vidString + ")";
            //    }
            //}

            //return appendString;
        }

        internal static string GetTitleVersionAppended(GYAData.Asset packageName, bool addExtension = false)
        {
            // Make asset title the filename
            string filename = GetTitleCleaned(packageName);
            string ext = ".unitypackage";

            // Create full version suffix to append
            string verString = GetAssetVersionStringToAppend(packageName);

            // Add version
            if (!filename.Contains(verString))
                filename = filename + verString;

            // Add extension if requested
            if (addExtension && !filename.EndsWith(ext, StringComparison.Ordinal))
                filename = filename + ext;

            return filename;
        }

        //returns: cleaned title + ext
        internal static string GetTitleAsFilename(GYAData.Asset packageName)
        {
            return GetTitleCleaned(packageName) + ".unitypackage";
        }

        // handles differently for official/exported AS packages
        internal static string GetTitleCleaned(GYAData.Asset packageName)
        {
            string filename = string.Empty;

            // Official AS pkg
            if (!packageName.isExported)
            {
                filename = RemoveInvalidCharsUnityTitleAsFilename(packageName.title);
            }
            // Exported pkg, may have characters not covered by Unity's InvalidPathCharsRegExp
            else
            {
                filename = RemoveInvalidCharsOSFilename(packageName.title);
            }

            return filename;
        }

        // Remove invalid chars
        internal static string RemoveInvalidCharsUnityTitleAsFilename(string packageName, bool cleanWhitespace = true)
        {
            var InvalidPathCharsRegExp = new Regex("[^a-zA-Z0-9() _-]");
            //return InvalidPathCharsRegExp.Replace(packageName, "");

            packageName = InvalidPathCharsRegExp.Replace(packageName, "");

            // Fix double spaces after cleaning and trim leading/trailing spaces just in case !!
            if (cleanWhitespace)
                packageName = packageName.Trim().Replace("  ", " ");

            return packageName;
        }

        // Remove invalid chars
        private static string RemoveInvalidCharsOSFilename(string filename)
        {
            //Remove any path info, only deal with filename and extension
            filename = Path.GetFileName(filename);

            System.Collections.Generic.HashSet<char> validChars = GetInvalidFileNameChars().ToHashSet();
            StringBuilder sb = new StringBuilder();
            string s = filename;

            for (int i = 0; i < s.Length; i++)
                if (!validChars.Contains(s[i]))
                    sb.Append(s[i]);

            return sb.ToString();
        }

        // Remove invalid chars
        private static string RemoveInvalidCharsOSPath(string filepath)
        {
            string invalidChars = new string(GetInvalidPathChars());
            var validChars = filepath.Where(x => !invalidChars.Contains(x)).ToArray();
            return new string(validChars);
        }

        // Same as RemoveInvalidCharsOSFilename except replaces invalid chars with '_'
        public static string GetSafeFilename(string filename, string replacementChar = "_")
        {
            return string.Join(replacementChar, filename.Split(GetInvalidFileNameChars()));
        }

        // Build AS Style path for pkg at target folder
        internal static string BuildAssetStorePathForPackage(GYAData.Asset pkg, string targetPath, bool includePublisher = false, bool includeCategory = true)
        {
            if (includePublisher)
            {
                //Add publisher name to path to fully recreate AS structure
                var pubPath = RemoveInvalidCharsUnityTitleAsFilename(pkg.publisher.label);
                targetPath = Path.Combine(targetPath, pubPath);
            }

            if (includeCategory)
            {
                // RemoveInvalidCharsUnityTitleAsFilename will maintain Unity's pathing format for assets
                var categoryPath = RemoveInvalidCharsUnityTitleAsFilename(pkg.category.label);
                targetPath = Path.Combine(targetPath, categoryPath);
            }

            return targetPath;
        }

        // Move assets to folder - copyOverride forces a copy instead of move
        public static KeyValuePair<int, string> MoveAssetToPath(GYAData.Asset packageMove, string pathMoveTo = null,
        bool copyOverride = false, bool quietMode = false, bool appendVer = true, bool appendCategory = false, bool removeVer = false)
        {
            string moveInfo = String.Empty;
            int filesMoved = 0;

            //string filename = Path.GetFileNameWithoutExtension(packageMove.filePath);
            string filename = Path.GetFileName(packageMove.filePath);
            string pathMoveFrom = GYAExt.PathFixedForOS(Path.GetFullPath(packageMove.filePath));

            // Path to move to
            if (string.IsNullOrEmpty(pathMoveTo))
            {
                GYAExt.Log("Target path is null: " + pathMoveTo, "File was NOT copied !!");
                return new KeyValuePair<int, string>(filesMoved, moveInfo);
            }

            // If Official AS pkg
            if (!packageMove.isExported)
            {
                // Add asset category to move path
                if (appendCategory)
                {
                    // Leave at false for default usage, append cat without pub
                    pathMoveTo = BuildAssetStorePathForPackage(packageMove, pathMoveTo, false);
                }

                // appendCategory with appendVer can be used to rebuild the AS folder structure
                // by forcing appendversion it makes sure that pkg's aren't duplicated due to a filename diff
                // then remove version info from files if desired

                // Add version
                if (appendVer)
                {
                    //if (packageMove.title != "Untitled")
                    if (!packageMove.isDamaged)
                        filename = GetTitleVersionAppended(packageMove, true);
                }
                // Remove version
                if (removeVer)
                {
                    filename = GetTitleAsFilename(packageMove);
                }
            }

            // Check for invalid chars - Path
            pathMoveTo = RemoveInvalidCharsOSPath(pathMoveTo);
            pathMoveTo = Path.GetFullPath(Path.Combine(pathMoveTo, filename));

            // Does file already exist at destination?
            if (File.Exists(pathMoveTo))
            {
                // Yes it is, do nothing
                if (!quietMode)
                    GYAExt.Log("File already exists: " + pathMoveTo, "File was NOT copied !!");
            }
            else
            {
                // Create folder if required
                if (pathMoveTo != null && !Directory.Exists(Path.GetDirectoryName(pathMoveTo)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(pathMoveTo));
                }

                // No, it's not
                string copyMoveTxt = "Move";
                if (copyOverride)
                    copyMoveTxt = "Copy";

                moveInfo = moveInfo + copyMoveTxt + ": " + pathMoveFrom + "\n" +
                           "To: " + pathMoveTo + "\n";

                try
                {
                    // Move the file
                    try
                    {
                        if (File.Exists(pathMoveFrom))
                            File.SetAttributes(pathMoveFrom, FileAttributes.Normal);

                        if (copyOverride)
                        {
                            // Copy file
                            if (pathMoveTo != null) File.Copy(pathMoveFrom, pathMoveTo);
                        }
                        else
                        {
                            // Move file
                            if (IsSymLink(pathMoveFrom) || IsSymLink(pathMoveTo))
                            {
                                // Must Copy/Delete because of symlink
                                File.Copy(pathMoveFrom, pathMoveTo);
                                if (File.Exists(pathMoveTo))
                                    File.Delete(pathMoveFrom);
                            }
                            else
                            {
                                // Can Move file
                                File.Move(pathMoveFrom, pathMoveTo);
                            }
                        }
                    }
                    catch (IOException ex)
                    {
                        GYAExt.LogWarning(
                            "Error " + (copyOverride ? "Copying: " : "Moving: ") + pathMoveFrom + " to " +
                            pathMoveTo, ex.Message, false);
                    }

                    // Verification
                    if (File.Exists(pathMoveTo))
                    {
                        filesMoved += 1;
                    }
                    else
                    {
                        GYAExt.LogWarning("Error: File " + copyMoveTxt + " - Unable to locate file at the target path:",
                            moveInfo);
                    }
                }
                catch (IOException ex)
                {
                    GYAExt.LogWarning("Error: File " + copyMoveTxt + " - Failed:", moveInfo + "\n\n" + ex.Message);
                }
            }
            return new KeyValuePair<int, string>(filesMoved, moveInfo);
        }

        // Delete empty folders recursively & handle .DS_Store files
        internal static void DeleteEmptySubFolders(string startLocation)
        {
            if (Directory.Exists(startLocation))
            {
                foreach (var directory in Directory.GetDirectories(startLocation))
                {
                    try
                    {
                        // If exists ds_store, delete it if it's the only file in folder
                        if (Directory.GetFileSystemEntries(directory, ".DS_Store").Length == 1)
                        {
                            File.Delete(Path.Combine(directory, ".DS_Store"));
                        }

                        DeleteEmptySubFolders(directory);
                        if (Directory.GetFileSystemEntries(directory).Length == 0)
                        {
                            Directory.Delete(directory, false);
                            GYAExt.Log("Deleted Empty Sub Folders from the Asset Store folder: " + startLocation);
                        }
                    }
                    catch (IOException ex)
                    {
                        GYAExt.LogWarning("Error: DeleteEmptySubFolders Failed: " + directory, ex.Message, false);
                    }
                }
            }
        }

        // Copy/Move marked assets to the prescribed folder
        internal static void CopyToSelected(string path, bool appendCategory = false, bool moveAssets = false)
        {
            int filesMoved = 0;
            string filesInfo = String.Empty;

            // Check all old assets
            int stepNumber = 0;
            GYAData.Asset packageToCopy = null;
            using (
                var progressBar = new GYA.ProgressBar(
                    string.Format("{0} Copying Selected Package(s)", GYA.gyaVars.abbr),
                    GYAImport.CountToImport(),
                    0,
                    _ => Path.GetFileName(packageToCopy.filePath)
                )
            )
                foreach (GYAData.Asset t in GYA.gyaData.Assets)
                {
                    // Is asset not in the old asset folder
                    if (t.isMarked)
                    {
                        packageToCopy = t;
                        progressBar.Update(stepNumber++);

                        // copy (or move) w/ category structure
                        var fileData = MoveAssetToPath(t, path, !moveAssets, false, false, appendCategory);
                        filesMoved = filesMoved + fileData.Key;
                        filesInfo = filesInfo + fileData.Value;
                    }
                }
            if (filesMoved > 0)
                GYAExt.Log("( " + filesMoved + " ) package(s) copied to: " + path, filesInfo, false);
        }

        // Rename asset to include version
        internal static void RenameWithVersion(object package)
        {
            GYAData.Asset pObject = (GYAData.Asset)package;
            RenameWithVersion(pObject, true);
        }

        // Rename asset to include version - right click in SV
        internal static void RenameWithVersion(GYAData.Asset package, bool showResults = true)
        {
            int filesMoved = 0;
            string filesInfo = String.Empty;

            if (!package.isExported)
            {
                // rename w/ ver
                var fileData = MoveAssetToPath(package, Path.GetDirectoryName(package.filePath), false, false, true);
                filesMoved = filesMoved + fileData.Key;
                filesInfo = filesInfo + fileData.Value;
            }

            if (showResults && filesMoved > 0)
            {
                GYAExt.Log(GYA.gyaVars.abbr + " - Package renamed: - Please 'Refresh' when you are done renaming files.", filesInfo, false);
            }
        }

        // Rename asset to include version
        internal static void RenameWithoutVersion(object package)
        {
            GYAData.Asset pObject = (GYAData.Asset)package;
            RenameWithoutVersion(pObject, true);
        }

        // Rename asset to include version - right click in SV
        internal static void RenameWithoutVersion(GYAData.Asset package, bool showResults = true)
        {
            int filesMoved = 0;
            string filesInfo = String.Empty;

            if (!package.isExported)
            {
                // rename w/ ver
                var fileData = MoveAssetToPath(package, Path.GetDirectoryName(package.filePath), false, false, false, false, true);
                filesMoved = filesMoved + fileData.Key;
                filesInfo = filesInfo + fileData.Value;
            }

            if (showResults && filesMoved > 0)
            {
                GYAExt.Log("Package renamed: - Please 'Refresh' when you are done renaming files.", filesInfo, false);
            }
        }

        // Rename selected to include version
        internal static void RenameWithVersionSelected()
        {
            int filesMoved = 0;
            string filesInfo = String.Empty;

            // Check all selected
            foreach (GYAData.Asset t in GYA.gyaData.Assets)
            {
                if (t.isMarked)
                {
                    // rename w/ ver
                    var fileData = MoveAssetToPath(t, Path.GetDirectoryName(t.filePath), false, true, true);
                    filesMoved = filesMoved + fileData.Key;
                    filesInfo = filesInfo + fileData.Value;
                }
            }
            if (filesMoved > 0)
            {
                GYAExt.Log("( " + filesMoved + " ) package(s) renamed.", filesInfo, false);
                // Make sure list is up-to-date
                //GYAPackage.RefreshAllCollections();
                GYAPackage.RefreshStore();
                GYA.Instance.RefreshSV();
            }
        }

        // Called when autoPreventASOverwrite is enabled
        // Rename AS assets to include version (Only affects assets within the AS folder)
        internal static void RenameWithVersionCollection(bool preventRefresh = false)
        {
            int filesMoved = 0;
            string filesInfo = String.Empty;

            // Check assets within the AS folder
            foreach (GYAData.Asset t in GYA.gyaData.Assets)
            {
                if (t.collection == GYA.svCollection.Store)
                {
                    // Rename
                    var fileData = MoveAssetToPath(t, Path.GetDirectoryName(t.filePath), false, true, true);
                    filesMoved = filesMoved + fileData.Key;
                    filesInfo = filesInfo + fileData.Value;
                }
            }
            if (filesMoved > 0)
            {
                GYAExt.Log("( " + filesMoved + " ) package(s) protected.", filesInfo, false);

                // Make sure list is up-to-date
                if (!preventRefresh)
                {
                    //GYAPackage.RefreshAllCollections();
                    GYAPackage.RefreshStore();
                    GYA.Instance.RefreshSV();
                }
            }
        }

        // Not Used - likely to be conflicts if more then 1 ver of asset in AS folder
        // Rename AS assets to remove version (Only affects assets within the AS folder)
        internal static void RenameWithVersionRemovedCollection()
        {
            int filesMoved = 0;
            string filesInfo = String.Empty;

            // Check assets within the AS folder
            foreach (GYAData.Asset t in GYA.gyaData.Assets)
            {
                if (t.collection == GYA.svCollection.Store)
                {
                    // Rename w/ ver removed
                    var fileData = MoveAssetToPath(t, Path.GetDirectoryName(t.filePath), false, true, false, false, true);
                    filesMoved = filesMoved + fileData.Key;
                    filesInfo = filesInfo + fileData.Value;
                }
            }
            if (filesMoved > 0)
            {
                GYAExt.Log("( " + filesMoved + " ) package(s) renamed with version removed.", filesInfo, false);
            }
            // Make sure list is up-to-date
            //GYAPackage.RefreshAllCollections();
            GYAPackage.RefreshStore();
            GYA.Instance.RefreshSV();
        }

        // Move old assets to the prescribed folder
        internal static void OldAssetsMove(bool needsRefresh = true)
        {
            int filesMoved = 0;
            string filesInfo = String.Empty;

            // Check all old assets
            foreach (GYAData.Asset t in GYA.gyaData.Assets)
            {
                if (t.isOldToMove)
                {
                    // Move to old
                    var fileData = MoveAssetToPath(t, GYA.gyaVars.pathOldAssetsFolder);
                    filesMoved = filesMoved + fileData.Key;
                    filesInfo = filesInfo + fileData.Value;
                }
            }
            if (filesMoved > 0)
            {
                GYAExt.Log("( " + filesMoved + " ) package(s) moved to the Old Assets Folder.", filesInfo, false);
            }
            // Make sure list is up-to-date
            if (needsRefresh)
            {
                //GYAPackage.RefreshAllCollections();
                GYAPackage.RefreshStore(false);
                GYAPackage.RefreshOld(false);
                GYA.Instance.RefreshSV();
            }
        }

        [DllImport("UnityEditor")]
        internal static extern void MoveAssetToTrash(string path);
        internal const int FILE_SHARE_READ = 1;
        internal const int FILE_SHARE_WRITE = 2;
        internal const int CREATION_DISPOSITION_OPEN_EXISTING = 3;
        internal const int FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;

        [DllImport("kernel32.dll", EntryPoint = "GetFinalPathNameByHandleW", CharSet = CharSet.Unicode,
            SetLastError = true)]
        public static extern int GetFinalPathNameByHandle(IntPtr handle, [In, Out] StringBuilder path, int bufLen,
            int flags);

        [DllImport("kernel32.dll", EntryPoint = "CreateFileW", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern SafeFileHandle CreateFile(string lpFileName, int dwDesiredAccess, int dwShareMode,
            IntPtr SecurityAttributes, int dwCreationDisposition, int dwFlagsAndAttributes, IntPtr hTemplateFile);

        // TRUE if a symlink/reparse point
        public static bool IsSymLink(string path)
        {
            bool pathBool = false; // Default is NOT a symlink
            try
            {
                if (File.Exists(path) || Directory.Exists(path))
                {
                    if ((File.GetAttributes(path) & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint)
                        pathBool = true;
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                GYAExt.LogWarning("IsSymLink UnauthorizedAccessException:", ex.Message);
            }
            catch (Exception ex)
            {
                GYAExt.LogWarning("IsSymLink Exception:", ex.Message);
            }
            return pathBool;
        }

        public static FileAttributes RemoveFileAttribute(FileAttributes attributes, FileAttributes attributesToRemove)
        {
            return attributes & ~attributesToRemove;
        }

        ////not used
        //public static bool IsValidFileName(string filename)
        //{
        //    string text = RemoveInvalidCharsFromFileName(filename, false);
        //    return text == filename && !string.IsNullOrEmpty(text);
        //}

        ////called by: IsValidFileName
        //public static string RemoveInvalidCharsFromFileName(string filename, bool logIfInvalidChars = false)
        //{
        //    if (string.IsNullOrEmpty(filename))
        //        return filename;

        //    filename = filename.Trim();
        //    if (string.IsNullOrEmpty(filename))
        //        return filename;

        //    string text = new string(GetInvalidFileNameChars());
        //    string text2 = string.Empty;
        //    bool flag = false;
        //    string text3 = filename;
        //    foreach (char c in text3)
        //    {
        //        if (text.IndexOf(c) == -1)
        //        {
        //            text2 += c;
        //        }
        //        else
        //        {
        //            flag = true;
        //        }
        //    }
        //    if (flag && logIfInvalidChars)
        //    {
        //        string displayStringOfInvalidCharsOfFileName =
        //            GetDisplayStringOfInvalidCharsOfFileName(filename);
        //        if (displayStringOfInvalidCharsOfFileName.Length > 0)
        //        {
        //            GYAExt.LogWarning("A filename cannot contain the following character(s): " +
        //                              displayStringOfInvalidCharsOfFileName);
        //        }
        //    }
        //    return text2;
        //}

        ////called by: RemoveInvalidCharsFromFileName
        //public static string GetDisplayStringOfInvalidCharsOfFileName(string filename)
        //{
        //    if (string.IsNullOrEmpty(filename))
        //        return string.Empty;

        //    string text = new string(GetInvalidFileNameChars());
        //    string text2 = string.Empty;
        //    foreach (char c in filename)
        //    {
        //        if (text.IndexOf(c) >= 0 && text2.IndexOf(c) == -1)
        //        {
        //            if (text2.Length > 0)
        //                text2 += " ";

        //            text2 += c;
        //        }
        //    }
        //    return text2;
        //}

        //As of.NET 4.7.2, Path.GetInvalidFileNameChars() reports the following 41 'bad' characters.
        //
        //0x0000    0      '\0'   |    0x000d   13      '\r'   |    0x001b   27  '\u001b'
        //0x0001    1  '\u0001'   |    0x000e   14  '\u000e'   |    0x001c   28  '\u001c'
        //0x0002    2  '\u0002'   |    0x000f   15  '\u000f'   |    0x001d   29  '\u001d'
        //0x0003    3  '\u0003'   |    0x0010   16  '\u0010'   |    0x001e   30  '\u001e'
        //0x0004    4  '\u0004'   |    0x0011   17  '\u0011'   |    0x001f   31  '\u001f'
        //0x0005    5  '\u0005'   |    0x0012   18  '\u0012'   |    0x0022   34       '"'
        //0x0006    6  '\u0006'   |    0x0013   19  '\u0013'   |    0x002a   42       '*'
        //0x0007    7      '\a'   |    0x0014   20  '\u0014'   |    0x002f   47       '/'
        //0x0008    8      '\b'   |    0x0015   21  '\u0015'   |    0x003a   58       ':'
        //0x0009    9      '\t'   |    0x0016   22  '\u0016'   |    0x003c   60       '<'
        //0x000a   10      '\n'   |    0x0017   23  '\u0017'   |    0x003e   62       '>'
        //0x000b   11      '\v'   |    0x0018   24  '\u0018'   |    0x003f   63       '?'
        //0x000c   12      '\f'   |    0x0019   25  '\u0019'   |    0x005c   92      '\\'
        //                        |    0x001a   26  '\u001a'   |    0x007c  124       '|'

        public static char[] GetInvalidFileNameChars()
        {
            if (GYAExt.IsOSWin)
            {
                return new[]
                {
                    '\0',
                    '\u0001', '\u0002', '\u0003', '\u0004', '\u0005', '\u0006',
                    '\a', '\b', '\t', '\n', '\v', '\f', '\r',
                    '\u000e', '\u000f', '\u0010', '\u0011', '\u0012', '\u0013', '\u0014', '\u0015', '\u0016',
                    '\u0017', '\u0018', '\u0019', '\u001a', '\u001b', '\u001c', '\u001d', '\u001e', '\u001f',
                    '"', '*', '/', ':', '<', '>', '?', '\\', '|'
                };
            }

            return new[]
            {
                '\0', '/', // default
                '*', ':', '\\', '|'
            };
        }

        public static char[] GetInvalidPathChars()
        {
            if (GYAExt.IsOSWin)
            {
                return new[]
                {
                    '\0',
                    '\u0001', '\u0002', '\u0003', '\u0004', '\u0005', '\u0006',
                    '\a', '\b', '\t', '\n', '\v', '\f', '\r',
                    '\u000e', '\u000f', '\u0010', '\u0011', '\u0012', '\u0013', '\u0014', '\u0015', '\u0016',
                    '\u0017', '\u0018', '\u0019', '\u001a', '\u001b', '\u001c', '\u001d', '\u001e', '\u001f',
                    '"', '<', '>', '|'
                };
            }

            return new[]
            {
                '\0' // default
            };
        }

        internal static void CopyFileIfExists(string src, string dst, bool overwrite)
        {
            if (File.Exists(src))
                UnityFileCopy(src, dst, overwrite);
        }

        internal static void CreateOrCleanDirectory(string dir)
        {
            if (Directory.Exists(dir))
                Directory.Delete(dir, true);

            Directory.CreateDirectory(dir);
        }

        internal static void MoveFileIfExists(string src, string dst)
        {
            if (File.Exists(src))
            {
                FileUtil.DeleteFileOrDirectory(dst);
                FileUtil.MoveFileOrDirectory(src, dst);
            }
        }

        internal static string NiceWinPath(string unityPath)
        {
            return (Application.platform != RuntimePlatform.WindowsEditor) ? unityPath.Replace("\\", "/") : unityPath.Replace("/", "\\");
        }

        internal static string RemovePathPrefix(string fullPath, string prefix)
        {
            string[] array = fullPath.Split(Path.DirectorySeparatorChar);
            string[] array2 = prefix.Split(Path.DirectorySeparatorChar);

            int num = 0;
            if (array[0] == string.Empty)
                num = 1;

            while (num < array.Length && num < array2.Length && array[num] == array2[num])
            {
                num++;
            }
            if (num == array.Length)
                return string.Empty;

            char directorySeparatorChar = Path.DirectorySeparatorChar;
            return string.Join(directorySeparatorChar.ToString(), array, num, array.Length - num);
        }

        internal static void ReplaceTextInFile(string path, params string[] input)
        {
            path = NiceWinPath(path);
            string[] array = File.ReadAllLines(path);
            for (int i = 0; i < input.Length; i += 2)
            {
                for (int j = 0; j < array.Length; j++)
                {
                    array[j] = array[j].Replace(input[i], input[i + 1]);
                }
            }
            File.WriteAllLines(path, array);
        }

        internal static void UnityFileCopy(string from, string to)
        {
            UnityFileCopy(from, to, false);
        }

        internal static void UnityFileCopy(string from, string to, bool overwrite)
        {
            File.Copy(NiceWinPath(from), NiceWinPath(to), overwrite);
        }

        internal static bool Regex_ReplaceTextInFile(string path, params string[] input)
        {
            bool result = false;
            path = NiceWinPath(path);
            string[] array = File.ReadAllLines(path);
            for (int i = 0; i < input.Length; i += 2)
            {
                for (int j = 0; j < array.Length; j++)
                {
                    string text = array[j];
                    array[j] = System.Text.RegularExpressions.Regex.Replace(text, input[i], input[i + 1]);
                    if (text != array[j])
                    {
                        result = true;
                    }
                }
            }
            File.WriteAllLines(path, array);
            return result;
        }
    }
}
