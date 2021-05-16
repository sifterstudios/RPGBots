using UnityEditor;

namespace MD_PluginEditor
{
    public class MD_StandardDeformer_Editor : MD_MaterialEditorUtilities
    {
        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            ps(10);
            phb("MD Plugin - Mesh Deformer 1.0 [March 2021]");
            ps(10);
            pl("Essentials");
            pv();
            ppDrawProperty(materialEditor, properties, "_Cull");
            ppDrawProperty(materialEditor, properties, "_Color");
            pv();
            ppDrawProperty(materialEditor, properties, "_MainTex", true);
            materialEditor.TextureScaleOffsetProperty(FindProperty("_MainTex", properties));
            pve();
            pv();
            ppDrawProperty(materialEditor, properties, "_MainNormal", true);
            materialEditor.TextureScaleOffsetProperty(FindProperty("_MainNormal", properties));
            ppDrawProperty(materialEditor, properties, "_Normal");
            pve();
            pv();
            ppDrawProperty(materialEditor, properties, "_Specular");
            ppDrawProperty(materialEditor, properties, "_MainMetallic", true);
            materialEditor.TextureScaleOffsetProperty(FindProperty("_MainMetallic", properties));
            ppDrawProperty(materialEditor, properties, "_Metallic");
            pve();
            pv();
            ppDrawProperty(materialEditor, properties, "_MainEmission", true);
            materialEditor.TextureScaleOffsetProperty(FindProperty("_MainEmission", properties));
            ppDrawProperty(materialEditor, properties, "_Emission");
            pve();
            pve();

            ps(15);
            pl("Deformers");
            pv();
            ppDrawProperty(materialEditor, properties, "_DEFAnim", "Deformer Animation Type");
            ppDrawProperty(materialEditor, properties, "_DEFDirection");
            ppDrawProperty(materialEditor, properties, "_DEFFrequency");
            pv();
            ppDrawProperty(materialEditor, properties, "_DEFEdges");
            ppDrawProperty(materialEditor, properties, "_DEFEdgesAmount");
            ppDrawProperty(materialEditor, properties, "_DEFExtrusion");
            pve();
            pve();
            ps();
            pl("Deformer Additional Properties");
            pv();
            ppDrawProperty(materialEditor, properties, "_DEFAbsolute");
            ppDrawProperty(materialEditor, properties, "_DEFFract");
            if (ppCompareProperty(materialEditor, "_DEFFract", 1))
                ppDrawProperty(materialEditor, properties, "_DEFFractValue");
            pve();
            ps();
            pl("Clipping");
            pv();
            ppDrawProperty(materialEditor, properties, "_DEFClipping");
            if (ppCompareProperty(materialEditor, "_DEFClipping", 1))
            {
                ppDrawProperty(materialEditor, properties, "_DEFClipType");
                ppDrawProperty(materialEditor, properties, "_DEFClipTile");
                ppDrawProperty(materialEditor, properties, "_DEFClipSize");
                ppDrawProperty(materialEditor, properties, "_DEFAnimateClip");
                if (ppCompareProperty(materialEditor, "_DEFAnimateClip", 1))
                    ppDrawProperty(materialEditor, properties, "_DEFClipAnimSpeed");
            }
            pve();
            ps();
            pl("Noise");
            pv();
            ppDrawProperty(materialEditor, properties, "_DEFNoise");
            if (ppCompareProperty(materialEditor, "_DEFNoise", 1))
            {
                ppDrawProperty(materialEditor, properties, "_DEFNoiseDirection");
                ppDrawProperty(materialEditor, properties, "_DEFNoiseSpeed");
            }
            pve();
           
            ps(40);
        }
    }
}