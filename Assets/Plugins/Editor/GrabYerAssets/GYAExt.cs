#if (UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5)
#define UNITY_3
#endif

#if (UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7)
#define UNITY_4
#endif

#define EnableZiosEditorThemeTweaks

using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using GYAInternal.Json;
using GYAInternal.Json.Linq;
using System.Text.RegularExpressions;

namespace XeirGYA
{
    public static class StringExtension
    {
        static readonly Regex re = new Regex(@"\{([^\}]+)\}", RegexOptions.Compiled);
        public static string FormatPlaceholder(this string str, Dictionary<string, string> fields)
        {
            if (fields == null)
                return str;

            return re.Replace(str, delegate (Match match)
            {
                return fields[match.Groups[1].Value];
            });

        }
        //return str.Replace("{tag}", "tagvalue");
    }

    // GYA Extensions
    public static class GYAExt
    {
        internal static ActiveOS activeOS = ActiveOS.Unknown;
        internal enum ActiveOS
        {
            Unknown,
            Windows,
            Mac,
            Linux
        }

        // Invoke
        internal static BindingFlags _flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public |
                                              BindingFlags.Static;

        public static object Invoke(object p_target, string p_method, params object[] p_args)
        {
            Type t = p_target.GetType();
            MethodInfo mi = t.GetMethod(p_method, _flags);
            return mi == null ? null : mi.Invoke(p_target, p_args);
        }

        static System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();

        public static void StopwatchStart()
        {
            stopWatch.Start();
        }

        public static void StopwatchStop()
        {
            stopWatch.Stop();
        }

        public static string StopwatchElapsed(bool consoleOutput = true, bool cumulative = false)
        {
            if (!cumulative)
                stopWatch.Stop();

            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}.{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds / 10);

            if (!cumulative)
                stopWatch.Reset();
            if (consoleOutput)
                Log("Elapsed Time: " + elapsedTime);
            elapsedTime = "Elapsed Time: " + elapsedTime;

            return elapsedTime;
        }

        public static string setSize(String _s, int _num)
        {
            var temp_s = _s;
            if (_s.Length >= _num)
                return _s;
            for (int i = 0; i < _num - _s.Length; i++)
                temp_s += " ";
            return temp_s;
        }

        // Return False if 0 , True if NOT 0
        public static bool ToBool(this int x)
        {
            //return (x != 0 ? true : false);
            return Convert.ToBoolean(x);
        }

        // Return True if 1
        public static bool IsTrue(this int x)
        {
            //return (x == 1 ? true : false);

            //bool y = Convert.ToBoolean(x);
            return (Convert.ToBoolean(x));
        }

        // Return True if 0
        public static bool IsFalse(this int x)
        {
            //return (x == 0 ? true : false);

            //bool y = Convert.ToBoolean(x);
            return (!Convert.ToBoolean(x));
        }

        // Decimal to Hex
        public static string DecToHex(int decValue)
        {
            return string.Format("{0:x}", decValue);
        }

        // Hex to Decimal
        public static long HexToDec(string hexValue)
        {
            return Convert.ToInt64(hexValue, 16);
        }

        //byte[] data = FromHex("47-61-74-65-77-61-79-53-65-72-76-65-72");
        //string s = Encoding.ASCII.GetString(data);
        public static byte[] FromHex(string hexValue)
        {
            hexValue = hexValue.Replace("-", "");
            byte[] raw = new byte[hexValue.Length / 2];
            for (int i = 0; i < raw.Length; i++)
            {
                raw[i] = Convert.ToByte(hexValue.Substring(i * 2, 2), 16);
            }
            return raw;
        }

        public static string GetLine(string text, int lineNo)
        {
            string[] lines = text.Replace("\r", "").Split('\n');
            return lines.Length >= lineNo ? lines[lineNo - 1] : "";
        }

        // Get string Between first/last of other strings
        public static string Between(this string value, string a, string b)
        {
            int posA = value.IndexOf(a, StringComparison.InvariantCultureIgnoreCase);
            int posB = value.LastIndexOf(b, StringComparison.InvariantCultureIgnoreCase);
            if (posA == -1)
            {
                return "";
            }
            if (posB == -1)
            {
                return "";
            }
            int adjustedPosA = posA + a.Length;
            if (adjustedPosA >= posB)
            {
                return "";
            }
            return value.Substring(adjustedPosA, posB - adjustedPosA);
        }

        public static string GetUntilOrEmpty(this string text, string stopAt = "-")
        {
            if (!string.IsNullOrEmpty(text))
            {
                int charLocation = text.IndexOf(stopAt, StringComparison.Ordinal);

                if (charLocation > 0)
                {
                    return text.Substring(0, charLocation);
                }
            }
            return String.Empty;
        }

        // Get first string Before another string
        public static string Before(this string value, string a)
        {
            int posA = value.IndexOf(a, StringComparison.InvariantCultureIgnoreCase);
            if (posA == -1)
            {
                return "";
            }
            return value.Substring(0, posA);
        }

        // Get last string After another string
        public static string After(this string value, string a)
        {
            int posA = value.LastIndexOf(a, StringComparison.InvariantCultureIgnoreCase);
            if (posA == -1)
            {
                return "";
            }
            int adjustedPosA = posA + a.Length;
            if (adjustedPosA >= value.Length)
            {
                return "";
            }
            return value.Substring(adjustedPosA);
        }

        // Return Digits or Alpha only
        public static string DigitsOnly(string pString)
        {
            System.Text.RegularExpressions.Regex text = new System.Text.RegularExpressions.Regex(@"[^\d]");
            return text.Replace(pString, "");
        }

        public static string AlphaOnly(string pString)
        {
            System.Text.RegularExpressions.Regex text = new System.Text.RegularExpressions.Regex(@"[^a-zA-Z]");
            return text.Replace(pString, "");
        }

        // Check if an Int is null
        public static bool IsIntNull(int? pInt)
        {
            return (pInt == null);
        }

        public static bool IsInt(object pVal)
        {
            int n;
            bool isNumeric = int.TryParse(pVal.ToString(), out n);
            return isNumeric;
        }

        public static int IntOrZero(object pVal)
        {
            return IsInt(pVal) ? Convert.ToInt32(pVal.ToString()) : 0;
        }

        // returns: "2016-04-13T13:18:10+00:00" style
        public static string DateAsISO8601(string pDateTime)
        {
            DateTime utcDate = DateTime.SpecifyKind(Convert.ToDateTime(pDateTime), DateTimeKind.Utc);
            return DateAsISO8601(utcDate);
        }

        // returns: "2016-04-13T13:18:10+00:00" style
        public static string DateAsISO8601(DateTime pDateTime)
        {
            CultureInfo dateCulture = CultureInfo.InvariantCulture;
            DateTimeStyles dateStyle = DateTimeStyles.AssumeUniversal;

            DateTimeOffset utcDate = DateTimeOffset.Parse(pDateTime.ToString(CultureInfo.InvariantCulture), dateCulture, dateStyle);
            return DateAsISO8601(utcDate);
        }

        // returns: "2016-04-13T13:18:10+00:00" style
        public static string DateAsISO8601(DateTimeOffset pDateTime)
        {
            CultureInfo dateCulture = CultureInfo.InvariantCulture;
            return pDateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", dateCulture);
        }

        //Return only the Date portion "2016-04-13" of "2016-04-13T13:18:10+00:00"
        public static string DateOnlyFromISO8601(string pDateTime)
        {
            string result = String.Empty;

            if (!string.IsNullOrEmpty(pDateTime) && pDateTime.Length > 9)
            {
                //return pDateTime.Split('T')[0];
                //string result = pDateTime.Substring(0, Math.Max(pDateTime.IndexOf('T'), 0));
                result = pDateTime.Substring(0, 10);
                result = GYAFile.GetSafeFilename(result, "-");
            }

            return result;
        }

        ////Return only the Time portion "13:18:10" of "2016-04-13T13:18:10+00:00"
        //public static string TimeOnlyFromISO8601(string pDateTime)
        //{
        //    pDateTime = pDateTime.Split('T')[1];
        //    return pDateTime.Split('+')[0];
        //}

        public static DateTimeOffset DateStringAsDTO(string pDateTime)
        {
            CultureInfo dateCulture = CultureInfo.InvariantCulture;
            DateTimeStyles dateStyle = DateTimeStyles.AssumeUniversal;

            DateTime utcDate = DateTime.SpecifyKind(Convert.ToDateTime(pDateTime), DateTimeKind.Utc);
            DateTimeOffset utcDate2 = DateTimeOffset.Parse(utcDate.ToString(CultureInfo.InvariantCulture), dateCulture, dateStyle);

            return utcDate2.ToUniversalTime();
        }

        // Check for valid date, return MinValue if not valid
        public static string ValidOrMinDate(DateTimeOffset pDateTime)
        {
            return ValidOrMinDate(pDateTime.ToString());
        }

        public static string ValidOrMinDate(string pDateTime)
        {
            DateTime tmpDate;
            if (!DateTime.TryParse(pDateTime, out tmpDate))
            {
                tmpDate = DateTime.MinValue;
            }
            return tmpDate.ToString(CultureInfo.InvariantCulture);
        }


        public static Type[] GetAllDerivedTypes(this AppDomain aAppDomain, Type aType)
        {
            var assemblies = aAppDomain.GetAssemblies();
            return (from assembly in assemblies from type in assembly.GetTypes() where type.IsSubclassOf(aType) select type).ToArray();
        }

        public static Rect GetEditorMainWindowPos()
        {
            var containerWinType = AppDomain.CurrentDomain.GetAllDerivedTypes(typeof(ScriptableObject))
                .FirstOrDefault(t => t.Name == "ContainerWindow");
            if (containerWinType == null)
                throw new MissingMemberException(
                    "Can't find internal type ContainerWindow. Maybe something has changed inside Unity");
            var showModeField = containerWinType.GetField("m_ShowMode",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var positionProperty = containerWinType.GetProperty("position",
                BindingFlags.Public | BindingFlags.Instance);
            if (showModeField == null || positionProperty == null)
                throw new MissingFieldException(
                    "Can't find internal fields 'm_ShowMode' or 'position'. Maybe something has changed inside Unity");
            var windows = Resources.FindObjectsOfTypeAll(containerWinType);
            foreach (var win in windows)
            {
                var showmode = (int)showModeField.GetValue(win);
                if (showmode == 4) // main window
                {
                    var pos = (Rect)positionProperty.GetValue(win, null);
                    return pos;
                }
            }
            throw new NotSupportedException(
                "Can't find internal main window. Maybe something has changed inside Unity");
        }

        public static void CenterOnMainWin(this UnityEditor.EditorWindow aWin)
        {
            var main = GetEditorMainWindowPos();
            var pos = aWin.position;
            float w = (main.width - pos.width) * 0.5f;
            float h = (main.height - pos.height) * 0.5f;
            pos.x = main.x + w;
            pos.y = main.y + h;
            aWin.position = pos;
        }

        // Print properties of an object to the console as json
        public static void LogAsJson(object obj, bool header = true)
        {
            try
            {
                String dText;
                if (obj == null)
                {
                    dText = header ? "Properties of -- NULL\n" : "";
                }
                else
                {
                    dText = header ? string.Format("Properties of -- {0}\n", obj.GetType()) : "";
                    dText += JsonConvert.SerializeObject(obj, Formatting.Indented);
                }
                Debug.Log(dText);
            }
            catch (Exception ex)
            {
                LogWarning("LogAsJson: " + ex.Message);
            }
        }

        public static string ToJson(object obj, bool formatted = false)
        {
            try
            {
                if (obj == null)
                    return null;

                return JsonConvert.SerializeObject(obj, (formatted ? Formatting.Indented : Formatting.None));
            }
            catch (Exception ex)
            {
                LogWarning("ToJson: " + ex.Message);
                return null;
            }
        }

        public static JToken RemoveFields(this JToken token, string[] fields)
        {
            JContainer container = token as JContainer;
            if (container == null)
                return token;

            List<JToken> removeList = new List<JToken>();
            foreach (JToken el in container.Children())
            {
                JProperty p = el as JProperty;
                if (p != null && fields.Contains(p.Name))
                    removeList.Add(el);

                el.RemoveFields(fields);
            }

            foreach (JToken el in removeList)
                el.Remove();

            return token;
        }

        // Simple Formatted Log for GYA
        internal static void Log(string pString, string pString2 = null, bool indent = true)
        {
            if (pString == null)
                pString = "null";

            if (pString2 == null)
                Debug.Log(GYA.gyaVars.abbr + " - " + pString + "\n");
            else
                Debug.Log(GYA.gyaVars.abbr + " - " + pString + (indent ? NewLineIndent() : "\n\n") +
                                      pString2);
        }

        internal static void LogWarning(string pString, string pString2 = null, bool indent = true)
        {
            if (pString == null)
                pString = "null";

            if (pString2 == null)
                Debug.LogWarning(GYA.gyaVars.abbr + " - " + pString + "\n");
            else
                Debug.LogWarning(GYA.gyaVars.abbr + " - " + pString + (indent ? NewLineIndent() : "\n\n") +
                                             pString2);
        }

        internal static void LogError(string pString, string pString2 = null, bool indent = true)
        {
            if (pString == null)
                pString = "null";

            if (pString2 == null)
                Debug.LogError(GYA.gyaVars.abbr + " - " + pString + "\n");
            else
                Debug.LogError(GYA.gyaVars.abbr + " - " + pString + (indent ? NewLineIndent() : "\n\n") +
                                           pString2);
        }

        internal static string NewLineIndent()
        {
            return "\n" + Indent(11);
        }

        public static string Indent(int count)
        {
            return "".PadLeft(count);
        }

        public static object TryParseJSON(string jsonString)
        {
            try
            {
                var o = JObject.Parse(jsonString);
                if (o != null)
                {
                    return o;
                }
            }
            catch (Exception) { }
            return false;
        }

        // Return path modified for platform
        public static string PathFixedForOS(string source)
        {
            char directorySeparatorDefault = '/'; // OS X, iOS, *nix, Android, etc
            char directorySeparatorWindows = '\\'; // Windows
            source = IsOSWin ? source.Replace(directorySeparatorDefault, directorySeparatorWindows) : source.Replace(directorySeparatorWindows, directorySeparatorDefault);
            return source;
        }

        public static string NullToEmpty(string pString)
        {
            return string.IsNullOrEmpty(pString) ? string.Empty : pString;
        }

        // Set Rect values, avoid CS1612: Cannot modify a value type return value
        public static Rect SetRect(this Rect sourceRect, float? pX = null, float? pY = null, float? pWidth = null,
            float? pHeight = null)
        {
            sourceRect.Set(
                pX ?? sourceRect.x,
                pY ?? sourceRect.y,
                pWidth ?? sourceRect.width,
                pHeight ?? sourceRect.height
            );
            return sourceRect;
        }

        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            if (source == null || toCheck == null)
                return false;
            return source.IndexOf(toCheck, comp) >= 0;
        }

        public static bool In<T>(this T source, params T[] list)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            return list.Contains(source);
        }

        // Prev for enum
        public static T Prev<T>(this T src) where T : struct
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException(String.Format("Argumnent {0} is not an Enum", typeof(T).FullName));

            T[] Arr = (T[])Enum.GetValues(src.GetType());
            int cnt = Array.IndexOf<T>(Arr, src) - 1;
            return (cnt == -1) ? Arr[Arr.Length - 1] : Arr[cnt];
        }

        // Next for enum
        public static T Next<T>(this T src) where T : struct
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException(String.Format("Argumnent {0} is not an Enum", typeof(T).FullName));

            T[] Arr = (T[])Enum.GetValues(src.GetType());
            int cnt = Array.IndexOf<T>(Arr, src) + 1;
            return (cnt == Arr.Length) ? Arr[0] : Arr[cnt];
        }

        // Return the count of an enumerable
        public static int CountEnum(IEnumerable enumerable)
        {
            return (from object item in enumerable select item).Count();
        }

        // For Casting to Enum
        public static T ParseEnum<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        // Open Asset Store page for specific asset
        //Use urlOverride to force url without processing
        public static void OpenAssetURL(int assetID, string urlOverride = null)
        {
            OpenAssetURL(assetID, false, urlOverride);
        }

        // Open Asset Store page for specific asset
        //OLD: https://www.assetstore.unity3d.com/#/content/72902
        //NEW: https://assetstore.unity.com/packages/slug/72902

        //return e.startsWith("com.unity3d.kharma:content") ? e.replace("com.unity3d.kharma:content","/packages/slug") :
        //e.startsWith("com.unity3d.kharma:list") ? e.replace("com.unity3d.kharma:list","/lists") :
        //e.startsWith("com.unity3d.kharma:publisher") ? e.replace("com.unity3d.kharma:publisher","/publishers") :
        //e.startsWith("com.unity3d.kharma:download") ? "/account/downloads" :
        //e.indexOf("content")>-1 ? e.replace("content","/packages/slug") : null

        public static void OpenAssetURL(int pID, bool openURLInUnity = false, string urlOverride = null)
        {
            //string openURLSite = "https://www.assetstore.unity3d.com/#";
            string openURLSite = "https://assetstore.unity.com";
            string openURL = string.Empty;

            if (!string.IsNullOrEmpty(urlOverride))
            {
                openURLSite = urlOverride;
                openURL = pID.ToString();
            }

            // NOTE: The first click does not load the asset page IF the AS window was not already open
            // Open in Unity STILL requires "content/id"
            if (openURLInUnity)
            {
                if (string.IsNullOrEmpty(urlOverride))
                {
                    //openURL = "com.unity3d.kharma:" + openURL;
                    //openURL = "packages/slug/" + pID; // NO
                    openURL = "content/" + pID;
                }

                // Open in Unity's Asset Store Window
                UnityEditorInternal.AssetStore.Open(openURL);
                //GYAReflect.MI_Invoke("UnityEditor.AssetStoreWindow", "OpenURL", openURL); // Alt
            }
            // Open in browser
            else
            {
                if (string.IsNullOrEmpty(urlOverride))
                {
                    //openURL = "content/" + pID;
                    openURL = "/packages/slug/" + pID;
                    openURL = openURLSite + openURL;
                }

                if (IsOSWin)
                {
                    System.Diagnostics.Process.Start(openURL);
                }
                if (IsOSMac)
                {
                    System.Diagnostics.Process.Start("open", openURL);
                }
                if (IsOSLinux)
                {
                    //System.Diagnostics.Process.Start ("open", openURL);
                    System.Diagnostics.Process.Start(openURL);
                }
            }
            //GYAExt.Log(openURL);

        }

        //OLD:	https://www.assetstore.unity3d.com/#/search/page=1/sortby=popularity/query=publisher:6144
        //var link = "https://www.assetstore.unity3d.com/#/search/page=1/sortby=popularity/query=publisher:";
        //NEW:	https://assetstore.unity.com/publishers/6144
        //var link = "https://assetstore.unity.com/publishers/";

        // NOTE: Unable to open Publisher in Unity
        public static void OpenPublisherURL(int pID, bool openURLInUnity = false, string urlOverride = null)
        {
            string openURLSite = "https://assetstore.unity.com";
            string openURL = string.Empty;

            //gyaVars.Prefs.openURLInUnity

            if (!string.IsNullOrEmpty(urlOverride))
            {
                openURLSite = urlOverride;
                openURL = pID.ToString();
            }

            // NOTE: The first click does not load the asset page IF the AS window was not already open
            if (openURLInUnity)
            {
                if (string.IsNullOrEmpty(urlOverride))
                {
                    //openURL = "publishers/" + pID;
                    openURL = "com.unity3d.kharma:publisher/" + pID;
                }

                // Open in Unity's Asset Store Window
                UnityEditorInternal.AssetStore.Open(openURL);
                //GYAReflect.MI_Invoke("UnityEditor.AssetStoreWindow", "OpenURL", openURL); // Alt
            }
            // Open in browser
            else
            {
                if (string.IsNullOrEmpty(urlOverride))
                {
                    openURL = "/publishers/" + pID;
                    openURL = openURLSite + openURL;
                }

                if (IsOSWin)
                {
                    System.Diagnostics.Process.Start(openURL);
                }
                if (IsOSMac)
                {
                    System.Diagnostics.Process.Start("open", openURL);
                }
                if (IsOSLinux)
                {
                    //System.Diagnostics.Process.Start ("open", openURL);
                    System.Diagnostics.Process.Start(openURL);
                }
            }
            //GYAExt.Log(openURL);

        }

        // Open in window passed folder name, optionally strip the filename
        public static void ShellOpenFolder(string @folder, bool stripName = false)
        {
            folder = PathFixedForOS(folder);
            if (stripName)
                folder = Path.GetDirectoryName(folder);

            if (folder != null && Directory.Exists(folder))
            {
                if (IsOSWin)
                {
                    folder = "\"" + folder + "\"";
                    System.Diagnostics.Process.Start(@folder);
                }
                if (IsOSMac)
                {
                    folder = "\"" + folder + "\"";
                    System.Diagnostics.Process.Start("open", @folder);
                }
                if (IsOSLinux)
                {
                    folder = folder + "/.";
                    EditorUtility.RevealInFinder(@folder);
                }
            }
        }

        // Running Pro version?
        public static bool IsPro
        {
            get
            {
                if (GYA.gyaVars.Prefs.forceDarkMode)
                    return true;
                else
                    return UnityEditorInternal.InternalEditorUtility.HasPro();
            }
        }

        // Using Pro skin?
        public static bool IsProSkin
        {
            get
            {
                if (GYA.gyaVars.Prefs.forceDarkMode)
                {
                    return true;
                }
                else
                {

#if EnableZiosEditorThemeTweaks
                    return (EditorGUIUtility.isProSkin || IsZiosEditorThemeDark);
#else
		        return (EditorGUIUtility.isProSkin);
#endif
                }
            }
        }

        // Using Zios Dark Theme? .. ZiosEditorThemeIsDark
        public static bool IsZiosEditorThemeDark
        {
            get { return (EditorPrefs.GetBool("EditorTheme-Dark")); }
        }

        public static void AssignIsOS()
        {
            activeOS = ActiveOS.Unknown;
            if (SystemInfo.operatingSystem.IndexOf("Windows", StringComparison.InvariantCultureIgnoreCase) != -1)
                activeOS = ActiveOS.Windows;
            if (SystemInfo.operatingSystem.IndexOf("Mac OS", StringComparison.InvariantCultureIgnoreCase) != -1)
                activeOS = ActiveOS.Mac;
            if (SystemInfo.operatingSystem.IndexOf("Linux", StringComparison.InvariantCultureIgnoreCase) != -1)
                activeOS = ActiveOS.Linux;
        }

        // Is current OS Mac
        public static bool IsOSMac
        {
            get
            {
                if (activeOS == ActiveOS.Unknown)
                    AssignIsOS();
                return activeOS == ActiveOS.Mac;
            }
        }

        // Is current OS Windows
        public static bool IsOSWin
        {
            get
            {
                if (activeOS == ActiveOS.Unknown)
                    AssignIsOS();
                return activeOS == ActiveOS.Windows;
            }
        }

        // Is current OS Linux
        public static bool IsOSLinux
        {
            get
            {
                if (activeOS == ActiveOS.Unknown)
                    AssignIsOS();
                return activeOS == ActiveOS.Linux;
            }
        }

        // Is mouse over gui component & asset window
        public static bool IsMouseOver(Rect item)
        {
            return Event.current.type == EventType.Repaint && item.Contains(Event.current.mousePosition);
        }

        // Return the Folder: Unity App
        public static string PathUnityApp
        {
            get { return Path.GetDirectoryName(EditorApplication.applicationPath); }
        }

        // Return the Folder: Unity Project Assets
        public static string PathUnityProjectAssets
        {
            get { return Path.GetFullPath(Path.Combine(PathUnityProject, "Assets")); }
        }

        // Return the Folder: Unity Project
        public static string PathUnityProject
        {
            get { return Path.GetDirectoryName(Application.dataPath); }
        }

        // Return the Unity Folder: Standard Assets with Path
        public static string PathUnityStandardAssets
        {
            get { return Path.GetFullPath(Path.Combine(PathUnityApp, FolderUnityStandardAssets)); }
        }

        // Return the Unity Folder: Standard Assets without Path
        public static string FolderUnityStandardAssets
        {
            get
            {
                // Unity 5 Asset Folder
                string standardAssetsPath = "Standard Assets";

#if UNITY_3 || UNITY_4 // Pre Unity 5 Standard Assets folder
				standardAssetsPath = "Standard Packages";
#endif
                return standardAssetsPath;
            }
        }

        public static string FileInGYADataFiles(string pFile)
        {
            return Path.Combine(PathGYADataFiles, pFile);
        }

        public static string PathGYADataFiles
        {
            get
            {
                const string dataPath = "Grab Yer Assets";
                return Path.Combine(PathUnityDataFiles, dataPath);
            }
        }

        // Return the Unity Folder: Asset Store
        public static string FolderUnityAssetStore
        {
            get { return "Asset Store"; }
        }

        // Return the correct Unity Folder: Asset Store for the actively running version
        public static string FolderUnityAssetStoreActive
        {
            get
            {
                // System specific asset folder
                string folderPath = FolderUnityAssetStore; // Default for Unity 3/4 AS Folder

                if (!(GYAVersion.GetUnityVersionMajor == 3 || GYAVersion.GetUnityVersionMajor == 4))
                {
                    folderPath += "-5.x";
                }

                return folderPath;
            }
        }

        // Return the correct Path of the Unity Folder: Asset Store for the actively running version
        public static string PathUnityAssetStoreActive
        {
            get
            {
                return Path.Combine(PathUnityDataFiles, FolderUnityAssetStoreActive);
            }
        }

        // Return the Unity cookie path - Browser
        public static string PathUnityCookiesFile
        {
            get
            {
                string cookiePath = string.Empty;
                if (IsOSWin)
                {
                    // PathUnityDataFiles will not work as Cookies is in the LocalLow folder
                    // C:\Users\{USER}\AppData\LocalLow\Unity\Browser\Cookies\Cookies
                    cookiePath =
                        PathFixedForOS(Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                                .Replace("AppData\\Roaming", "AppData"), "LocalLow/Unity/Browser/Cookies/Cookies"));
                }

                if (IsOSMac)
                {
                    cookiePath = Path.Combine(GYAExt.PathUnityDataFiles, "Browser/Cookies/Cookies");
                }

                if (IsOSLinux)
                {
                    cookiePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "unity3d/Unity/Browser/Cookies/Cookies");
                }
                return cookiePath;
            }
        }

        // Return the Unity cookie path - WebViewProfile - Untested
        public static string PathUnityCookiesFileAlt
        {
            get
            {
                string cookiePath = string.Empty;
                if (IsOSWin)
                {
                    // PathUnityDataFiles will not work as Cookies is in the LocalLow folder
                    // C:\Users\{USER}\AppData\LocalLow\Unity\Browser\Cookies\Cookies
                    cookiePath =
                        PathFixedForOS(Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                                .Replace("AppData\\Roaming", "AppData"), "LocalLow/Unity/WebViewProfile/cookies.sqlite"));
                }

                if (IsOSMac)
                {
                    cookiePath = Path.Combine(GYAExt.PathUnityDataFiles, "WebViewProfile/cookies.sqlite");
                }

                if (IsOSLinux)
                {
                    cookiePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "unity3d/Unity/WebViewProfile/cookies.sqlite");
                }
                return cookiePath;
            }
        }

        // Return the Unity Folder: Asset Store Parent Folder
        public static string PathUnityDataFiles
        {
            get
            {
                // System specific asset path:
                // Windows:	%AppData%\Unity
                // Mac:	~/Library/Unity
                // Linux:	~/.local/share/unity3d

                if (IsOSWin)
                {
                    return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Unity");
                }
                if (IsOSMac)
                {
                    return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library/Unity");
                }
                if (IsOSLinux)
                {
                    return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "unity3d");
                }
                return "";
            }
        }

        // Return the Unity Folder: Asset Store
        public static string PathUnityAssetStore
        {
            get { return Path.Combine(PathUnityDataFiles, FolderUnityAssetStore); }
        }

        // Convert bytes to KB/MB/GB
        public static string BytesToKB(this int fileSizeBytes)
        {
            double fs = fileSizeBytes;
            return fs.BytesToKB();
        }

        public static string BytesToKB(this float fileSizeBytes)
        {
            double fs = fileSizeBytes;
            return fs.BytesToKB();
        }

        public static string BytesToKB(this double fileSizeBytes)
        {
            // Get filesize of asset
            string[] sizes = { "KB", "MB", "GB" };
            int order = 0;
            fileSizeBytes = fileSizeBytes / 1024;

            while (fileSizeBytes >= 1024 && order + 1 < sizes.Length)
            {
                order++;
                fileSizeBytes = fileSizeBytes / 1024;
            }
            // Format fileSize string
            return String.Format("{0:0.00} {1}", fileSizeBytes, sizes[order]);
        }

        internal static string intToSizeString(int inValue)
        {
            if (inValue < 0)
            {
                return "unknown";
            }
            float num = inValue;
            var array = new string[]
            {
                "TB",
                "GB",
                "MB",
                "KB",
                "Bytes"
            };
            int num2 = array.Length - 1;
            while (num > 1000 && num2 >= 0)
            {
                num /= 1000;
                num2--;
            }
            return num2 < 0 ? "<error>" : string.Format("{0:#.##} {1}", num, array[num2]);
        }

        // Return the byte range for the size header
        public static string GetByteRangeHeader(double pkgSize)
        {
            string headerText = string.Empty;
            int kb = 1024;
            pkgSize = pkgSize / 1024;

            if (pkgSize > kb * 1000)
                headerText = "1 GB+";
            if (pkgSize > kb * 500 && pkgSize < kb * 1000)
                headerText = "500 MB - < 1 GB";
            if (pkgSize > kb * 250 && pkgSize < kb * 500)
                headerText = "250 MB - < 500 MB";
            if (pkgSize > kb * 100 && pkgSize < kb * 250)
                headerText = "100 MB - < 250 MB";
            if (pkgSize > kb * 50 && pkgSize < kb * 100)
                headerText = "50 MB - < 100 MB";
            if (pkgSize > kb * 10 && pkgSize < kb * 50)
                headerText = "10 MB - < 50 MB";
            if (pkgSize > kb * 1 && pkgSize < kb * 10)
                headerText = "1 MB - < 10 MB";
            if (pkgSize < kb * 1)
                headerText = "< 1 MB";

            return headerText;
        }

        public static Color WithAlpha(this Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }
    }
}
