using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace XeirGYA
{
    public static class GYATexture
    {
        // Textures
        internal static Texture2D iconPrev;

        internal static Texture2D iconNext;
        internal static Texture2D iconFavorite;
        internal static Texture2D iconDamaged;
        internal static Texture2D iconMenu;
        internal static Texture2D iconRefresh;
        internal static Texture2D iconReset;
        internal static Texture2D iconResetX;
        internal static Texture2D iconCategory;
        internal static Texture2D iconCategoryX;
        internal static Texture2D iconPublisher;
        internal static Texture2D iconPublisherX;
        internal static Texture2D iconStore;
        internal static Texture2D iconUser;
        internal static Texture2D iconStandard;
        internal static Texture2D iconOld;
        internal static Texture2D iconProject;
        internal static Texture2D iconStoreAlt;
        internal static Texture2D iconUserAlt;
        internal static Texture2D iconStandardAlt;
        internal static Texture2D iconOldAlt;
        internal static Texture2D iconBlank;
        internal static Texture2D iconAll;
        internal static Texture2D iconOptions;
        internal static Texture2D iconUnity;
        internal static Texture2D iconLock;
        internal static Texture2D iconDownload;

        internal static Texture2D texUnityDark;
        internal static Texture2D texUnityLight;
        internal static Texture2D texTransparent;
        internal static Texture2D texDivider;
        internal static Texture2D texSelector;

        internal static Dictionary<int, Texture2D> dictIcons = new Dictionary<int, Texture2D>();

        // Load textures
        internal static void LoadTextures()
        {
            iconAll = GetIconAll();
            iconOptions = GetIconOptions();
            iconUnity = GetIconUnity();
            iconLock = GetIconLock();
            iconDownload = GetIconDownload();
            iconPrev = GetIconPrev();
            iconNext = GetIconNext();
            iconFavorite = GetIconFavorite();
            iconDamaged = GetIconDamaged();
            iconMenu = GetIconMenu();
            iconRefresh = GetIconRefresh();
            iconReset = GetIconReset();
            iconResetX = GetIconResetX();
            iconCategory = GetIconCategory();
            iconCategoryX = GetIconCategoryX();
            iconPublisher = GetIconPublisher();
            iconPublisherX = GetIconPublisherX();

            // Default Icons
            iconStore = GetIconStore();
            iconUser = GetIconUser();
            iconStandard = GetIconStandard();
            iconOld = GetIconOld();
            iconStoreAlt = GetIconStoreAlt();
            iconUserAlt = GetIconUserAlt();
            iconStandardAlt = GetIconStandardAlt();
            iconOldAlt = GetIconOldAlt();
            iconProject = GetIconProject();
            iconBlank = GetIconBlank();
            texUnityDark = GetTexUnityDark();
            texUnityLight = GetTexUnityLight();
            texTransparent = GetTexTransparent();
            texDivider = GetTexDivider();
            texSelector = GetTexSelector();
        }

        // Load Texture2D from file
        internal static void LoadTexture(string filepath, ref Texture2D texture)
        {
            if (!texture)
            {
                texture = new Texture2D(128, 128);
            }
            byte[] array = null;
            try
            {
                array = File.ReadAllBytes(filepath);
            }
            catch (Exception)
            {
            }

            if (filepath == string.Empty || array == null || !texture.LoadImage(array))
            {
                Color[] pixels = texture.GetPixels();
                for (int i = 0; i < pixels.Length; i++)
                {
                    pixels[i] = new Color(0.5f, 0.5f, 0.5f, 0f);
                }
                texture.SetPixels(pixels);
                texture.Apply();
            }
        }

        // Return Unity Skin as texture
        public static Texture2D GetUnitySkinTexture()
        {
            if (GYAExt.IsProSkin)
                return texUnityDark;
            return texUnityLight;
        }

        // Return Unity Skin as Color
        public static Color GetUnitySkinColor()
        {
            Color skinColor = GYAExt.IsProSkin
                ? (Color) new Color32(56, 56, 56, 255)
                : (Color) new Color32(194, 194, 194, 255);
            return skinColor;
        }

		// Changed after 3.17.8.2902
        // Unity Dark Texture
        internal static Texture2D GetTexUnityDark()
        {
            return ShowColor(new Color32(56, 56, 56, 255));
        }

        // Unity Light Texture
        internal static Texture2D GetTexUnityLight()
        {
	        return ShowColor (new Color32 (194, 194, 194, 255));
        }

        public static Texture2D LoadTextureFromFile(string filename)
        {
            Texture2D texture = new Texture2D(2048, 2048);

            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            byte[] imageData = new byte[fs.Length];
            fs.Read(imageData, 0, (int) fs.Length);
            texture.LoadImage(imageData);

            return texture;
        }

		public static Color32 ToColor(int HexVal)
		{
			byte R = (byte)((HexVal >> 16) & 0xFF);
			byte G = (byte)((HexVal >> 8) & 0xFF);
			byte B = (byte)((HexVal) & 0xFF);
			return new Color32(R, G, B, 255);
		}

		public static string ToHex(this Color color)
		{
			Color32 c = color;
			var hex = string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", c.r, c.g, c.b, c.a);
			return hex;
		}

        // Note that Color32 and Color implictly convert to each other. You may pass a Color object to this method without first casting it.
        public static string ColorToHex(Color32 color)
        {
            string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
            return hex;
        }

        public static Color HexToColor(string hex)
        {
            try
            {
                hex = hex.Replace("0x", ""); //in case the string is formatted 0xFFFFFF
                hex = hex.Replace("#", ""); //in case the string is formatted #FFFFFF
                byte a = 255; //assume fully visible unless specified in hex
                byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                //Only use alpha if the string has enough characters
                if (hex.Length == 8)
                {
                    a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
                }
                return new Color32(r, g, b, a);
            }
            catch
            {
                return Color.black;
            }
        }

        public static bool IsValidHexColor(string inputColor)
        {
            if (Regex.Match(inputColor, "^#(?:[0-9a-fA-F]{3}){1,2}$").Success)
                return true;
            return false;
        }

        internal static Color TransformHSV(
            Color color, // color to transform
            float H, // hue shift (in degrees)
            float S, // saturation multiplier (scalar)
            float V // value multiplier (scalar)
        )
        {
            float VSU = V * S * Mathf.Cos(H * Mathf.PI / 180);
            float VSW = V * S * Mathf.Sin(H * Mathf.PI / 180);

            Color ret = new Color
            {
                r = (.299f * V + .701f * VSU + .168f * VSW) * color.r
                    + (.587f * V - .587f * VSU + .330f * VSW) * color.g
                    + (.114f * V - .114f * VSU - .497f * VSW) * color.b,
                g = (.299f * V - .299f * VSU - .328f * VSW) * color.r
                    + (.587f * V + .413f * VSU + .035f * VSW) * color.g
                    + (.114f * V - .114f * VSU + .292f * VSW) * color.b,
                b = (.299f * V - .3f * VSU + 1.25f * VSW) * color.r
                    + (.587f * V - .588f * VSU - 1.05f * VSW) * color.g
                    + (.114f * V + .886f * VSU - .203f * VSW) * color.b,
                a = 1f
            };
            if (ret.r < 0)
            {
                ret.r = 0;
            }
            if (ret.g < 0)
            {
                ret.g = 0;
            }
            if (ret.b < 0)
            {
                ret.b = 0;
            }
            return ret;
        }

        // Return blended color
        internal static Color BlendColor(Color sourceColor, Color tintColor)
        {
            Color finalColor = sourceColor * (1 - tintColor.a) + tintColor * tintColor.a;
            return finalColor;
        }

        internal static Color CombineColors(params Color[] aColors)
        {
            Color result = new Color(0, 0, 0, 0);
            result = aColors.Aggregate(result, (current, c) => current + c);

            result /= aColors.Length;
            return result;
        }

        public static Texture2D CombineTextures(Texture2D aBaseTexture, Texture2D aToCopyTexture)
        {
            int aWidth = aBaseTexture.width;
            int aHeight = aBaseTexture.height;
            Texture2D aReturnTexture = new Texture2D(aWidth, aHeight, TextureFormat.RGBA32, false);

            Color[] aBaseTexturePixels = aBaseTexture.GetPixels();
            Color[] aCopyTexturePixels = aToCopyTexture.GetPixels();
            Color[] aColorList = new Color[aBaseTexturePixels.Length];
            int aPixelLength = aBaseTexturePixels.Length;

            for (int p = 0; p < aPixelLength; p++)
            {
                aColorList[p] = Color.Lerp(aBaseTexturePixels[p], aCopyTexturePixels[p], aCopyTexturePixels[p].a);
            }

            aReturnTexture.SetPixels(aColorList);
            aReturnTexture.Apply(false);

            return aReturnTexture;
        }

        internal static Texture2D addTextures(Texture2D texture1, Texture2D texture2)
        {
            Color[] color1 = texture1.GetPixels();
            Color[] color2 = texture2.GetPixels();

            for (int i = 0; i < color1.Length; i++)
            {
                Color.Lerp(color1[i], color2[i], color2[i].a);
            }
            texture1.SetPixels(color1);
            return texture1;
        }

        internal static Texture2D TintTexture(Texture2D tintTexture, Color tintColor)
        {
            Texture2D texture = tintTexture;
            int colorCount = 1;
            Color[] colors = new Color[colorCount];
            colors[0] = tintColor;

            int mipCount = Mathf.Min(colorCount, texture.mipmapCount);

            // Tint mip levels
            for (int mip = 0; mip < mipCount; ++mip)
            {
                Color[] texColors = texture.GetPixels(mip);
                for (int i = 0; i < texColors.Length; ++i)
                {
                    if (texColors[i].a >= 0.5f) // Do not process transparent
                    {
                        texColors[i] = BlendColor(colors[mip], texColors[i]); // Blended
                    }
                }
                texture.SetPixels(texColors, mip);
            }
            texture.Apply(false);

            return texture;
        }

        internal static Texture2D InvertColors(Texture2D invertedTexture)
        {
            for (int m = 0; m < invertedTexture.mipmapCount; m++)
            {
                Color[] c = invertedTexture.GetPixels(m);
                for (int i = 0; i < c.Length; i++)
                {
                    c[i].r = 1 - c[i].r;
                    c[i].g = 1 - c[i].g;
                    c[i].b = 1 - c[i].b;
                }
                invertedTexture.SetPixels(c, m);
            }
            invertedTexture.Apply();
            return invertedTexture;
        }

        // Create Texture2D from base64 string
        internal static Texture2D BuildTexture(string base64Text, int x, int y)
        {
            Texture2D texture = new Texture2D(x, y) {hideFlags = HideFlags.HideAndDontSave};
            texture.LoadImage(Convert.FromBase64String(base64Text));
            texture.Apply();
            return texture;
        }

        internal static Texture2D ScaleImage(Texture2D source, int w, int h)
        {
            if (source.width % 4 != 0)
            {
                return null;
            }
            Texture2D texture2D = new Texture2D(w, h, TextureFormat.RGB24, false, true);
            Color[] pixels = texture2D.GetPixels(0);
            double num = 1 / (double) w;
            double num2 = 1 / (double) h;
            double num3 = 0;
            double num4 = 0;
            int num5 = 0;
            for (int i = 0; i < h; i++)
            {
                int j = 0;
                while (j < w)
                {
                    pixels[num5] = source.GetPixelBilinear((float) num3, (float) num4);
                    num3 += num;
                    j++;
                    num5++;
                }
                num3 = 0;
                num4 += num2;
            }
            texture2D.SetPixels(pixels, 0);
            texture2D.Apply();
            return texture2D;
        }

        internal static Texture GetIconByInstanceID(int instanceID)
        {
            Texture result = null;
            if (instanceID != 0)
            {
                string assetPath = AssetDatabase.GetAssetPath(instanceID);
                result = AssetDatabase.GetCachedIcon(assetPath);
            }
            return result;
        }

        // Create Texture2D from Color32
        internal static Texture2D ShowColor(Color32 color)
        {
            return ShowColor((Color) color);
        }

        // Create Texture2D from Color
        internal static Texture2D ShowColor(Color color)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            texture.hideFlags = HideFlags.HideAndDontSave;
            return texture;
        }

        // SV Header bar color
        internal static Texture2D GetTexDivider()
        {
            if (GYAExt.IsProSkin)
            {
                return ShowColor(new Color32(70, 70, 70, 255));
            }
            return ShowColor(new Color32(154, 154, 154, 255));
        }

        // SV Selection bar color
        internal static Texture2D GetTexSelector()
        {
            if (GYAExt.IsProSkin)
            {
                return ShowColor (new Color32 (62, 95, 150, 255));
            }
            return ShowColor(new Color32(62, 125, 231, 255));
        }

        // Lock Icon
        public static Texture2D GetIconLock()
        {
            string texString =
                "iVBORw0KGgoAAAANSUhEUgAAAAwAAAAQCAMAAAAVv241AAAABGdBTUEAALGPC/xhBQAAAAFzUkdCAK7OHOkAAADAUExURQAAANm7WOfDS9OqVcfHyBYTAfHOXzYwIRIPB5uen9WuY1NPK8KuUuvJQC0tJZSQde/Tc5+ci9y8a7e1s3ZyXMzMzNy8eXx2YuPh4szMyrCwsN3b25uZmdy5ddKlNgAAANGmPbySG8ueG8TExMLAwJ+fn6Ojo5mWi9PS0+nDMuzFZ+3EcvPUgPLVjO/Meue9Ot6vIfbel4BsK0c/I+bDde3FTMnIyOHf4OK1LBobH15TL+7He7m4ueu9W1FPQ3dsT9PhMMcAAAAodFJOUwD5/fTJSf4oYiL0OtD9EbL8xfNBal3zWeJNE/bR8vQX9ej1oKB2oU1QTqBmAAAApElEQVQI103LRxaCQBRE0Q/YtAKiBBVQVDCcDiTJ5v3vyj7owDeqOygAUeD7E/i18LrOW3y3s243m8faGXAk5wBO5DBgRMRh0o7+QAboM8tdrVxrpgOYhjpPL5e5PDVMwJKapIk6pVTCgO0sqauS5o0tsMy4dL1RmS0FQibnVdmweyig5H3xerOaawLbouizlPPnXiCK472iaco2wrBDaDyE0O4Dt38QTqmcum0AAAAASUVORK5CYII=";
            return BuildTexture(texString, 16, 16);
        }

        // Download Icon
        public static Texture2D GetIconDownload()
        {
            string texString =
                "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQBAMAAADt3eJSAAAABGdBTUEAALGPC/xhBQAAAAFzUkdCAK7OHOkAAAAkUExURQAAADPMMzPLMzLMMjPMMzPLMzLMMi3SLTLMMjLMMjLMMjPMM7YK1g0AAAALdFJOUwAj6ePXhroR/Hrine15wwAAAERJREFUCNdjYGDgDA2dwAACTLt3K5DNKHPx2L27xSWdgdF6NxBsFmBgEAYxDIFyICGQAFjIEKyL0RoiABRaCKEZGEECAGHWHSGG6Q54AAAAAElFTkSuQmCC";
            return BuildTexture(texString, 14, 14);
        }

        // Transparent Texture
        internal static Texture2D GetTexTransparent()
        {
            string texString =
                "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABAQMAAAAl21bKAAAABGdBTUEAALGPC/xhBQAAAAFzUkdCAK7OHOkAAAADUExURQAAAKd6PdoAAAABdFJOUwBA5thmAAAACklEQVQI12NgAAAAAgAB4iG8MwAAAABJRU5ErkJggg==";
            return BuildTexture(texString, 1, 1);
        }

        // Icons

        public static Texture2D GetIconUnity()
        {
            string texString =
                "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgBAMAAACBVGfHAAAABGdBTUEAALGPC/xhBQAAAAFzUkdCAK7OHOkAAAAwUExURQAAABQUFBQUFBMTExMTExISEhMTExMTExQUFBMTExQUFBMTExMTExMTExISEhQUFK2li5UAAAAPdFJOUwBx5O45RbsZJ5rMB4X3UiOBiA4AAADJSURBVCjPY2DABxoFULilx+wTkLhbxPT//1dH8CvX/geCrwiB+f//f7L//wfOZ7n/OSfU/v83GFdQbM1zBs7/uZ8bIAJO/003MDDEfur6/wCiQP+TAwMD9/qPnP8nQBWoAEnG/6v5/q+GKPgcAKSK/j/gsLeAmgCizn/bwJ3/EWICSAGH/RWg4D2wgm+CQCAGMjAeiFn1/0PApwIGhqoGLAIYWjANxbAW02FAJaooTsf0HIb3MQMIIwixBDJmNGBEFGZUYolsCAAAJ82NNPCA0TwAAAAASUVORK5CYII=";
            return BuildTexture(texString, 16, 16);
        }

        public static Texture2D GetIconAll()
        {
            string texString =
                "iVBORw0KGgoAAAANSUhEUgAAABIAAAASCAYAAABWzo5XAAAACXBIWXMAAAAAAAAAAQCEeRdzAAAANklEQVR4nGNgoCbwa/f6j4xh4mFBB/8jY5h4n9PM/8h4JBg0+MDgC6PBZ9DgA6R6bSQaRA0AALrMJjBfyG+yAAAAAElFTkSuQmCC";
            return BuildTexture(texString, 16, 16);
        }

        public static Texture2D GetIconFavorite()
        {
            string texString =
                "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAACXBIWXMAAAAAAAAAAQCEeRdzAAAAoUlEQVR4nGNgoCWY2e3TDcJkG3B9l955ECZLc215ROTf67L/QBjEJst2mAFEuSI1NdkQhEG2gfwN0wzDIDGQHEwdiua1s532vj6pcg9dEy4MUgvSg2I7spMJYZBaDFcQawhWzcQaglczcnjgMgDF37gAIRfg1QxyHnJswFIicujj9QJIEqYQFO+wOAexYQbjNQCUUED+xKYIJAaSIytZ4wMAqPMgInrKGhsAAAAASUVORK5CYII=";
            return BuildTexture(texString, 16, 16);
        }

        public static Texture2D GetIconDamaged()
        {
            string texString =
                "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAACXBIWXMAAAAAAAAAAQCEeRdzAAAAdUlEQVR4nGNgoDaQMzGxNNDQ+IsNg+SI0ryRmeH/HRYWFAwSw2sIsuZPDNgxTkNgmkE2IWv4PH06GBM0BJfN2AxANgTFAHTbSTYA5gJkg8g2AIZhBiCLkWQANkwbF+BKQNgMIJgWsKVCslIjNkxSfiArM5EDAAI4MrBjTDv7AAAAAElFTkSuQmCC";
            return BuildTexture(texString, 16, 16);
        }

        public static Texture2D GetIconMenu()
        {
            string texString =
                "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAACXBIWXMAAAAAAAAAAQCEeRdzAAAAmklEQVR4nLVT2w3AIAh0jM7VhfxR4yTu4hzdo2kvgQSxPvgoycWgcMCpzv1lMcbyorLvvT8AC8GVUrpDCCf5Ff6QBIFIojUjWAPns4qfSZpgOga3uSIhlCYZzCqwCOKuu4aABZMYiSqFnbZuIhiMkBcj5I5Ej8KC6b1OA0oulmvsrpMeUF08pOp2TQqGatz69n9ANTmn+TPt2gMobC2mcoAhKgAAAABJRU5ErkJggg==";
            return BuildTexture(texString, 16, 16);
        }

        public static Texture2D GetIconOptions()
        {
            string texString =
                "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAACXBIWXMAAAAAAAAAAQCEeRdzAAAAHklEQVR4nGNgGF6gtbX1P7GYNgYMPBgNg9EwIBcAABSFqJ0ebpurAAAAAElFTkSuQmCC";
            return BuildTexture(texString, 16, 16);
        }

        public static Texture2D GetIconRefresh()
        {
            string texString =
                "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAACXBIWXMAAAAAAAAAAQCEeRdzAAAAkklEQVR4nL1S2wnAMAjMEp2oa2QVv/LYKCt1jbYBhSMqEgoNCAmedxc1pT9OrZXW2CJorV2991uilHK6ShHBvLvFDBiRAyVERAcC5nslEAET8yazy855dMm4rOyrhEFkCn0mmGOBLqsmQi+GO0rstOUC1c1RogtxIluHyuEiIdCKcI0n+7o4YttV9oj4C3mrcPc8KRv6C/ztygwAAAAASUVORK5CYII=";
            return BuildTexture(texString, 16, 16);
        }

        public static Texture2D GetIconReset()
        {
            string texString =
                "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAACXBIWXMAAAAAAAAAAQCEeRdzAAAAw0lEQVR4nK1TyQ3DMAxzh8gyXaVz+G3AxzZeypM0IRAijKz0kfQh+CJpS6JDjPHlRc75XUr5IDC/wgW7UWttW4zW2lcDezi7FEgpLSQSzBeoKEZgJwEBTLd4F5wEQCJ5B3YG88ecZL2IAoOqSuKaT+Zc8QFgVQRIn8462EKDA27AIRYEWQGAbRuVMwkADBGt/k8BmwIrTgN556cUtIjsgGcu06WjiAoQ101e2NPp0s6jjcZIw5LFSN010l+s/Pgz3f3OK3is08sKNK6mAAAAAElFTkSuQmCC";
            return BuildTexture(texString, 16, 16);
        }

        public static Texture2D GetIconResetX()
        {
            string texString =
                "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAACXBIWXMAAAAAAAAAAQCEeRdzAAAA20lEQVR4nJWTsQ0CQQwEj1bogPxDREZCAQSQAAESGTSAEDESLdACIUVQAjmCBtAG89o3PiSC1et8613b5y/X+bKX4TKd9R01XomBw3Z3Pi5Wzwy6qwrIJZJP681EiKJeUSvwyyUz6AjgQLK+QBVkrcEt7g7RS/QzSc4vuryNxq97M9w7CTAHj4mrHOW2Al5qfJUooHNVgKn79OMepAK0wMQpnel7ZZ0WFNCBgAvF5UIEfvsKSnoMmjeoiQhw4bSLJEUEantPMu5fqxyrcPhdusrRJUPW2pfAv7/zB8SGyw2blSfqAAAAAElFTkSuQmCC";
            return BuildTexture(texString, 16, 16);
        }

        public static Texture2D GetIconCategory()
        {
            string texString =
                "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAACXBIWXMAAAAAAAAAAQCEeRdzAAAANElEQVR4nGNgGLSgtbX1Py5MFYMpspksVyAbgtUAUmwm2QB0jcPAALyhTFMD8AKKDRgwAAB/QO2XSnnZJAAAAABJRU5ErkJggg==";
            return BuildTexture(texString, 16, 16);
        }

        public static Texture2D GetIconCategoryX()
        {
            string texString =
                "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAACXBIWXMAAAAAAAAAAQCEeRdzAAAAa0lEQVR4nGNgGLRgQmLKf1yYKgZTZDNZrlgREvkfhGFsnAqwYWQ1OA3Y5hP0Hx+GqUGmqWvAQVff//gwTA0yjQJOOXr+x4dhanCG8mVrl//4MEwNTgMeGtv9R8cgDTA2To34DEDGBA0YMAAABXHYCXZIbC8AAAAASUVORK5CYII=";
            return BuildTexture(texString, 16, 16);
        }

        public static Texture2D GetIconBars()
        {
            string texString =
                "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAACXBIWXMAAAAAAAAAAQCEeRdzAAAAHklEQVR4nGNgGF6gtbX1P7GYNgYMPBgNg9EwIBcAABSFqJ0ebpurAAAAAElFTkSuQmCC";
            return BuildTexture(texString, 16, 16);
        }

        public static Texture2D GetIconPublisher()
        {
            string texString =
                "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAACXBIWXMAAAAAAAAAAQCEeRdzAAAAl0lEQVR4nLWS2w3AIAhFHcWB3IYV9N8ZnIiJbCWBlNIQaZqSEB/JPVzBlP6MWutoreHKSesbYV6CQkl7Pk8CRsXYez8oWaghEBKz7WkgsAOAFmuI2P8EkKe4AGrazsE2yIUFSHWZSgQylBPkuyx/wu0DVy9mEsiVQT/zAZGqXg/4HjXEs+ymgVxfOyI2kHszo2KVJTTSSJxNt0HEVoQXdQAAAABJRU5ErkJggg==";
            return BuildTexture(texString, 16, 16);
        }

        public static Texture2D GetIconPublisherX()
        {
            string texString =
                "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAACXBIWXMAAAAAAAAAAQCEeRdzAAAAoUlEQVR4nGNgoCWYHh2/GYSnxSb8B9FEa5wbHKmw2TsgAYRBbBAm2hCQYpitMIxsCMhQkjQjGwJzFU4DQJLYNCM7n6AB27wD/2PDIANgXiHbAELhh9MQmO2wWCFoCEgDTPM5G9djIDGQRhAbJIYzHGChDFMIMwA59GEuxDAEpPChke1/fBjmGpBmEJ8kzeiGwGgwIFYzDGMEJjkGEI5PIgEAxvYdqgbJ9hgAAAAASUVORK5CYII=";
            return BuildTexture(texString, 16, 16);
        }

        public static Texture2D GetIconPrev()
        {
            string texString =
                "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAACXBIWXMAAAAAAAAAAQCEeRdzAAAAQUlEQVR4nGOor69nogQz4JMsKSn5AMJkGQDTTMgQrAaga8ZnCIYBuDTjMoSBFM3YDGEgVTO6IdQPg1EDho0BpGAAkkjHIpn4bfwAAAAASUVORK5CYII=";
            return BuildTexture(texString, 16, 16);
        }

        public static Texture2D GetIconNext()
        {
            string texString =
                "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAACXBIWXMAAAAAAAAAAQCEeRdzAAAAPUlEQVR4nGOor69nogQzYBMsKSn5gA2PGjD8DcCliZBhDMTYjM8lGF4gRTPtAhGbISRlJnRD8KnBawAxGAALscciHIQIOQAAAABJRU5ErkJggg==";
            return BuildTexture(texString, 16, 16);
        }

        // Collection Icons

        public static Texture2D GetIconProject()
        {
            string texString =
                "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAACXBIWXMAAAAAAAAAAQCEeRdzAAAAXklEQVR4nGNgoCaoqKj4jw8TZcC2bdswMEycoCH4DFixYgUY4zUEJLlx40YMTLRXcBmAbhheA9atXYcXEzRg9arVeDFBA2CBhQvT3oBlS5fhxbR3AcXRSAzGacCAAADrfmPeZSzDvAAAAABJRU5ErkJggg==";
            return BuildTexture(texString, 16, 16);
        }

        public static Texture2D GetIconStandard()
        {
            string texString =
                "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAACXBIWXMAAAAAAAAAAQCEeRdzAAAAXElEQVR4nGNgGJ5gbcLW/9O95oPxkvA1/2HiIDZMHKQGpwEwRegKkQ0GYZINWJe4DbsBu/MOwwWR2ciGoNuOYgC6BLGYegYQ8gJBA/AF4sAYgC3EscUMTgOGFgAAs/EXN7319fgAAAAASUVORK5CYII=";
            return BuildTexture(texString, 16, 16);
        }

        public static Texture2D GetIconUser()
        {
            string texString =
                "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAACXBIWXMAAAAAAAAAAQCEeRdzAAAAS0lEQVR4nGNgoBWYOfP2/6ioo2AMYpOtmSxD0DXD8EgygOJARDeEZM1kgwMHXv7H5nxkl4DUEHQyIYzhJVI0YzWEVM0Y0UqxAZQAAK6GFKg/0uadAAAAAElFTkSuQmCC";
            return BuildTexture(texString, 16, 16);
        }

        public static Texture2D GetIconStore()
        {
            string texString =
                "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAACXBIWXMAAAAAAAAAAQCEeRdzAAAAbklEQVR4nGNgwAEaNzf+j58fD8YgNi51eDWiY7wG4dNI0CBiNaJjuAH5K/P/k+qCCXsmIAzwafD5D8KEDIJpDO0PBWMMA3AZhK6RoAHIBmHTSLQBuDSOGoDFAFhiItYAlESEDpANIkkjNoOI0QgAV6aub7ojLnoAAAAASUVORK5CYII=";
            return BuildTexture(texString, 16, 16);
        }

        public static Texture2D GetIconOld()
        {
            string texString =
                "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAACXBIWXMAAAAAAAAAAQCEeRdzAAAAUUlEQVR4nGNgoAXYo6Hxf6K8PAoGiZGlkWiDiNGM1xBiNcMw2bZjdQVMcKWqGlEYwxXIBsybOx8vpr0BZHmB4kCkOBqpkpAoTsqEDCJKIzkAAKhnQADNgHe4AAAAAElFTkSuQmCC";
            return BuildTexture(texString, 16, 16);
        }

        public static Texture2D GetIconStandardAlt()
        {
            string texString =
                "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAACXBIWXMAAAAAAAAAAQCEeRdzAAAAZ0lEQVR4nGNgGJ6gzqC1oc6w7T8I1xq0PoCJg9gwcZAa3AZAFUANOIBkwAFkObwGIBuE7jJkNRhORlEEEwfRSGxMA3AowmY4uisJGkDIm3i9QLQBJEkSawA+bxCORmQv4cJEhNMQAQD7JZC/o9mXgQAAAABJRU5ErkJggg==";
            return BuildTexture(texString, 16, 16);
        }

        public static Texture2D GetIconUserAlt()
        {
            string texString =
                "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAACXBIWXMAAAAAAAAAAQCEeRdzAAAAY0lEQVR4nGNgoBWIijrcEBV19D8EH24gUzOIRrBJMODo/8jIwwdgfBAbJEaSAcg2wlxBXwMo9AKFgYhqCBmaKQIg22B+RsYQMTwuIeRkvPKkBBRWtaQGFEa6QPcvsZhYC/ECAHpIq/MdBYx9AAAAAElFTkSuQmCC";
            return BuildTexture(texString, 16, 16);
        }

        public static Texture2D GetIconStoreAlt()
        {
            string texString =
                "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAACXBIWXMAAAAAAAAAAQCEeRdzAAAAoklEQVR4nGNgwAHi5sY1xM+P/w/CIDYudTg1xs2LOwBigzGQTdAgdI3Y5HEa5NPo8w8kQawrQWpDJ4T+QxjQ4PM/f2U+Qf/CXAlSG9of+h/FABAd0hvSgM0gZI0gNSAxrAbAALJB6BphAK8BMIBNI0kGoCgaAQZAcQMhA/KW5zWAxEP6Qh6gSIA0oxuEbABMI1gzjpjBMAikmGiN6ACkmBiNAGZBxTZ5IdfJAAAAAElFTkSuQmCC";
            return BuildTexture(texString, 16, 16);
        }

        public static Texture2D GetIconOldAlt()
        {
            string texString =
                "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAACXBIWXMAAAAAAAAAAQCEeRdzAAAAYUlEQVR4nGNgoAXol5NrmCgv/x8Zg8RI0oisAZc4Vs34bMKrhlhnwgwhLIgHYFhGdCDhshDGOX36dMOZM2f+48MgNch6qGcARV6gOBBJcQVOyyhOSMgKyErKuAwiSSM5AAB5eawN69BdUwAAAABJRU5ErkJggg==";
            return BuildTexture(texString, 16, 16);
        }

        public static Texture2D GetIconBlank()
        {
            string texString =
                "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABAQMAAAAl21bKAAAABGdBTUEAALGPC/xhBQAAAAFzUkdCAK7OHOkAAAADUExURQAAAKd6PdoAAAABdFJOUwBA5thmAAAACklEQVQI12NgAAAAAgAB4iG8MwAAAABJRU5ErkJggg==";
            return BuildTexture(texString, 16, 16);
        }
    }
}