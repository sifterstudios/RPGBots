using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace FogVolumeUtilities
{
    public static class ExtensionMethods
    {
        public static float Remap(this float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }
        public static bool TimeSnap(int Frames)
        {
            bool refresh = true;
            if (Application.isPlaying)
            {
                refresh = Time.frameCount <= 3 || (Time.frameCount % (1 + Frames)) == 0;

                return (refresh);
            }
            else
                return true;
        }

        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp = lhs;
            lhs = rhs;
            rhs = temp;
        }
    }
    public class Rendering
    {
        public static void EnsureKeyword(Material material, string name, bool enabled)
        {
            if (enabled != material.IsKeywordEnabled(name))
            {
                if (enabled)
                    material.EnableKeyword(name);
                else
                    material.DisableKeyword(name);
            }
        }
    }
    public struct int2
    {
        public int x;
        public int y;
        public int2(int x, int y)
        {
            this.x = x; this.y = y;
        }

    }





    public static class EditorExtension
    {
        private static string[] m_LayerNames = null;
        private static int[] m_LayerMasks = null;

        static EditorExtension()
        {
            var tmpNames = new List<string>();
            var tmpMasks = new List<int>();
            for (int i = 0; i < 32; i++)
            {
                try
                {
                    var name = LayerMask.LayerToName(i);
                    if (name != "")
                    {
                        tmpNames.Add(name);
                        tmpMasks.Add(1 << i);
                    }
                }
                catch { }
            }
            m_LayerNames = tmpNames.ToArray();
            m_LayerMasks = tmpMasks.ToArray();
        }

        public static void ToggleInHierarchy(Object obj, bool visible)
        {
            #if UNITY_EDITOR
            if (visible)
                obj.hideFlags = HideFlags.None;
            else
                obj.hideFlags = HideFlags.HideInHierarchy;
            try
            {
                EditorApplication.RepaintHierarchyWindow();
                EditorApplication.DirtyHierarchyWindowSorting();
            }
            catch { }
            #endif
        }
#if UNITY_EDITOR
        public static int DrawLayerMaskField(Rect aPosition, int aMask, GUIContent aLabel)
        {
            int val = aMask;
            int maskVal = 0;
            for (int i = 0; i < m_LayerNames.Length; i++)
            {
                if (m_LayerMasks[i] != 0)
                {
                    if ((val & m_LayerMasks[i]) == m_LayerMasks[i])
                        maskVal |= 1 << i;
                }
                else if (val == 0)
                    maskVal |= 1 << i;
            }
            int newMaskVal = UnityEditor.EditorGUI.MaskField(aPosition, aLabel, maskVal, m_LayerNames);
            int changes = maskVal ^ newMaskVal;

            for (int i = 0; i < m_LayerMasks.Length; i++)
            {
                if ((changes & (1 << i)) != 0)            // has this list item changed?
                {
                    if ((newMaskVal & (1 << i)) != 0)     // has it been set?
                    {
                        if (m_LayerMasks[i] == 0)           // special case: if "0" is set, just set the val to 0
                        {
                            val = 0;
                            break;
                        }
                        else
                            val |= m_LayerMasks[i];
                    }
                    else                                  // it has been reset
                    {
                        val &= ~m_LayerMasks[i];
                    }
                }
            }
            return val;
        }
#endif
    }

}