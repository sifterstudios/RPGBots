#if UNITY_EDITOR
using UnityEngine;

using UnityEditor;

using System.Reflection;


public class FogVolumeCreator : Editor
{
    [MenuItem("GameObject/Create Other/Fog Volume")]
    [MenuItem("Fog Volume/Create Fog Volume")]
    public static void CreateFogVolume()
    {
        var FogVolume = new GameObject();

        //Icon stuff
        var Icon = Resources.Load("FogVolumeIcon") as Texture;

        //Icon.hideFlags = HideFlags.HideAndDontSave;
        var editorGUI = typeof(EditorGUIUtility);
        var bindingFlags = BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic;
        var args = new object[]
        {
            FogVolume,
            Icon
        };
        editorGUI.InvokeMember("SetIconForObject", bindingFlags, null, null, args);

        FogVolume.name = "Fog Volume";
        FogVolume.AddComponent<MeshRenderer>();
        FogVolume.AddComponent<FogVolume>();
        FogVolume.GetComponent<Renderer>().shadowCastingMode =
                UnityEngine.Rendering.ShadowCastingMode.Off;
        FogVolume.GetComponent<Renderer>().receiveShadows = false;
        FogVolume.GetComponent<Renderer>().reflectionProbeUsage =
                UnityEngine.Rendering.ReflectionProbeUsage.Off;
        FogVolume.GetComponent<Renderer>().lightProbeUsage =
                UnityEngine.Rendering.LightProbeUsage.Off;
        Selection.activeObject = FogVolume;
        if (SceneView.currentDrawingSceneView)
        {
            SceneView.currentDrawingSceneView.MoveToView(FogVolume.transform);
        }
    }


    //=============================================================================================
    // L I G H T S
    //=============================================================================================

    [MenuItem("GameObject/Create Other/Fog Volume Light/Fog Volume Point Light")]
    [MenuItem("Fog Volume/Create Light/Fog Volume Point Light")]
    public static void CreateFogVolumePointLight()
    {
        var fogVolumeLight = new GameObject("FogVolumePointLight");
        var light = fogVolumeLight.AddComponent<FogVolumeLight>();
        light.IsPointLight = true;
        light.IsAddedToNormalLight = false;
    }

    [MenuItem("GameObject/Create Other/Fog Volume Light/Fog Volume Spot Light")]
    [MenuItem("Fog Volume/Create Light/Fog Volume Spot Light")]
    public static void CreateFogVolumeSpotLight()
    {
        var fogVolumeLight = new GameObject("FogVolumeSpotLight");
        var light = fogVolumeLight.AddComponent<FogVolumeLight>();
        light.IsPointLight = false;
        light.IsAddedToNormalLight = false;
    }

    [MenuItem("Fog Volume/Create Light/Fog Volume Directional Light")]
    public static void AutoCreateFogVolumeDirectionalLight()
    {
        var lights = FindObjectsOfType<Light>();
        for (var i = 0; i < lights.Length; i++)
        {
            var light = lights[i];
            if (light.type == LightType.Directional)
            {
                if (light.GetComponent<FogVolumeDirectionalLight>() == null)
                {
                    var dirLight = light.gameObject.AddComponent<FogVolumeDirectionalLight>();
                    var fogVolumes = FindObjectsOfType<FogVolume>();
                    dirLight._TargetFogVolumes = new FogVolume[fogVolumes.Length];
                    for (var k = 0; k < fogVolumes.Length; k++)
                    {
                        dirLight._TargetFogVolumes[k] = fogVolumes[k];
                    }
                }
            }
        }
    }

    [MenuItem("Fog Volume/Create Light/Fog Volume Directional Light", true)]
    public static bool EnableCreateFogVolumeDirectionalLight()
    {
        return FindObjectOfType<FogVolumeDirectionalLight>() == null &&
               FindObjectOfType<FogVolume>() != null;
    }
    [MenuItem("GameObject/Create Other/Fog Volume Light/Normal Point Light")]
    [MenuItem("Fog Volume/Create Light/Normal Point Light")]
    public static void CreateNormalPointLight()
    {
        var normalPointLight = new GameObject("Point light");
        var light = normalPointLight.AddComponent<Light>();
        light.type = LightType.Point;
        var fvLight = normalPointLight.AddComponent<FogVolumeLight>();
        fvLight.IsPointLight = true;
        fvLight.IsAddedToNormalLight = true;
    }

    [MenuItem("GameObject/Create Other/Fog Volume Light/Normal Spot Light")]
    [MenuItem("Fog Volume/Create Light/Normal Spot Light")]
    public static void CreateNormalSpotLight()
    {
        var normalPointLight = new GameObject("Spot light");
        var light = normalPointLight.AddComponent<Light>();
        light.type = LightType.Spot;
        var fvLight = normalPointLight.AddComponent<FogVolumeLight>();
        fvLight.IsPointLight = false;
        fvLight.IsAddedToNormalLight = true;
    }


    //=============================================================================================
    // P R I M I T I V E S
    //=============================================================================================

    [MenuItem("Fog Volume/Create Primitive/Fog Volume Primitive Box")]
    [MenuItem("GameObject/Create Other/Fog Volume Primitive/Box")]
    static public void CreateFogVolumePrimitiveBox()
    {
        GameObject FogVolumePrimitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //Icon stuff
        //Texture Icon = Resources.Load("FogVolumePrimitiveIcon") as Texture;
        //Icon.hideFlags = HideFlags.HideAndDontSave;
        // var editorGUI = typeof(EditorGUIUtility);
        // var bindingFlags = BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic;
        //var args = new object[] { FogVolumePrimitive, Icon };
        //editorGUI.InvokeMember("SetIconForObject", bindingFlags, null, null, args);

        FogVolumePrimitive.name = "Fog Volume Primitive Box";

        FogVolumePrimitive.GetComponent<BoxCollider>().isTrigger = true;

        FogVolumePrimitive.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        FogVolumePrimitive.GetComponent<Renderer>().receiveShadows = false;
        FogVolumePrimitive.GetComponent<Renderer>().reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
        FogVolumePrimitive.GetComponent<Renderer>().lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        Selection.activeObject = FogVolumePrimitive;
        if (UnityEditor.SceneView.currentDrawingSceneView) UnityEditor.SceneView.currentDrawingSceneView.MoveToView(FogVolumePrimitive.transform);
        FogVolumePrimitive.AddComponent<FogVolumePrimitive>();
    }


    [MenuItem("Fog Volume/Create Primitive/Fog Volume Primitive Sphere")]
    [MenuItem("GameObject/Create Other/Fog Volume Primitive/Sphere")]
    static public void CreateFogVolumePrimitiveSphere()
    {
        GameObject FogVolumePrimitive = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //Icon stuff
        //Texture Icon = Resources.Load("FogVolumePrimitiveIcon") as Texture;
        //Icon.hideFlags = HideFlags.HideAndDontSave;
        // var editorGUI = typeof(EditorGUIUtility);
        // var bindingFlags = BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic;
        //var args = new object[] { FogVolumePrimitive, Icon };
        //editorGUI.InvokeMember("SetIconForObject", bindingFlags, null, null, args);

        FogVolumePrimitive.name = "Fog Volume Primitive Sphere";

        FogVolumePrimitive.GetComponent<SphereCollider>().isTrigger = true;

        FogVolumePrimitive.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        FogVolumePrimitive.GetComponent<Renderer>().receiveShadows = false;
        FogVolumePrimitive.GetComponent<Renderer>().reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
        FogVolumePrimitive.GetComponent<Renderer>().lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        Selection.activeObject = FogVolumePrimitive;
        if (UnityEditor.SceneView.currentDrawingSceneView) UnityEditor.SceneView.currentDrawingSceneView.MoveToView(FogVolumePrimitive.transform);
        FogVolumePrimitive.AddComponent<FogVolumePrimitive>();
    }
}
#endif
