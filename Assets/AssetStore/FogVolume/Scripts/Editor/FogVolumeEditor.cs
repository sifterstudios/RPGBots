using UnityEditor;
using UnityEngine;
using System.Collections;
using UnityEditorInternal;
using System.IO;

[CustomEditor(typeof(FogVolume))]

public class FogVolumeEditor : Editor
{
    #region Variables
    private static bool ShowInspectorTooltips
    {
        get { return EditorPrefs.GetBool("ShowInspectorTooltips", true); }
        set { EditorPrefs.SetBool("ShowInspectorTooltips", value); }
    }

    FogVolume _target;
    private const string _showCustomizationOptions = "showCustomizationOptions";

    private GUIStyle Theme1, ThemeFooter;
    #endregion
    //Texture2D[] _InspectorBackground;

    void OnEnable()
    {
        _target = (FogVolume)target;
        string m_ScriptFilePath;
        string BackgroundImagesPath;
        MonoScript ms = MonoScript.FromScriptableObject(this);
        m_ScriptFilePath = AssetDatabase.GetAssetPath(ms);

        FileInfo fi = new FileInfo(m_ScriptFilePath);
        BackgroundImagesPath = fi.Directory.ToString();
        BackgroundImagesPath= BackgroundImagesPath.Replace("\\", "/");
        BackgroundImagesPath += "/Themes/png";

        _target.BackgroundImagesPath = BackgroundImagesPath;

        int ImageIndex = 0;

            foreach (string filePath in System.IO.Directory.GetFiles(BackgroundImagesPath))
            {
                if (!filePath.Contains(".meta"))
                    // print(filePath);
                    ImageIndex++;
            }
        _target._InspectorBackground = null;
        _target._InspectorBackground = new Texture2D[ImageIndex];
            // print(ImageIndex);
            int i = 0;

            foreach (string filePath in System.IO.Directory.GetFiles(BackgroundImagesPath))
            {
                if (!filePath.Contains(".meta"))
                {

                    byte[] fileData;
                    // print(filePath);
                    fileData = File.ReadAllBytes(filePath);
                    //init
                    Texture2D tex = new Texture2D(1, 32, TextureFormat.RHalf, false, true);
                    tex.filterMode = FilterMode.Trilinear;
                    tex.LoadImage(fileData);
                _target._InspectorBackground[i] = tex;
                    i++;
                }


            }


        Theme1 = new GUIStyle();
        ThemeFooter = new GUIStyle();
        //  ThemeFooter.normal.background = MakeTex(new Color(.31f, 0.2f, .3f));
        if (EditorGUIUtility.isProSkin)
            ThemeFooter.normal.background = (Texture2D)Resources.Load("RendererInspectorBodyBlack");
        else
            ThemeFooter.normal.background = (Texture2D)Resources.Load("RendererInspectorBodyBright");
    }

    GUIContent VariableField(string VariableName, string Tooltip)
    {
        return new GUIContent(VariableName, ShowInspectorTooltips ? Tooltip : "");
    }


    public override bool RequiresConstantRepaint() { return true; }



    public override void OnInspectorGUI()
    {
        GUILayout.Space(10);
        serializedObject.Update();
        GUI.color = Color.white;
        GUILayout.BeginVertical(Theme1);
        //some info about fog type


        EditorGUI.BeginChangeCheck();
        Undo.RecordObject(target, "Fog volume parameter");
        #region Basic

        //   _target.bias = EditorGUILayout.Slider("bias", _target.bias, 0, 1);
        //_target.RenderableInSceneView Rod, lo moví pabajo, a other/rendering options
        if (_target._FogType == FogVolume.FogType.Textured)
        {
            _target.FogMainColor = EditorGUILayout.ColorField(new GUIContent("Light energy", ShowInspectorTooltips ? "Multiplies all lighting contributions" : ""), _target.FogMainColor);
            _target.bVolumeFog = EditorGUILayout.Toggle(VariableField("Volume Fog", "Fills the empty space left by noise with volumetric fog. This fog allows the use of pointlights and shadows"), _target.bVolumeFog);
        }
        else
            _target.FogMainColor = EditorGUILayout.ColorField("Color", _target.FogMainColor);

        if (!_target.EnableNoise && !_target.EnableGradient || _target.bVolumeFog)
            _target.Visibility = EditorGUILayout.FloatField(VariableField("Visibility", "Fog Visibility") , _target.Visibility);
        _target.fogVolumeScale = EditorGUILayout.Vector3Field("Size", _target.fogVolumeScale);
        if (_target._FogType == FogVolume.FogType.Textured)
        {

            if (_target.bVolumeFog)
            {
                GUILayout.BeginVertical("box");
                _target._FogColor = EditorGUILayout.ColorField("Volume Fog Color", _target._FogColor);
                _target._AmbientAffectsFogColor = EditorGUILayout.Toggle(VariableField("Apply ambient color", "Use ambient color and proxy volume to multiply this color"), _target._AmbientAffectsFogColor);
                GUILayout.EndVertical();//end box
            }
        }
        if(_target._FogType == FogVolume.FogType.Textured)//3.1.9 Hide if Uniform. Its going to be hardcoded as "Traditional"
            _target._BlendMode = (FogVolumeRenderer.BlendMode)EditorGUILayout.EnumPopup(VariableField("Blend Mode", "Choose between standard of premultiplied alpha blend"), _target._BlendMode);
        #endregion

        #region lighting
        //GUILayout.BeginHorizontal("toolbarbutton");
        //EditorGUILayout.LabelField("Lighting");
        //showLighting = EditorGUILayout.Toggle("Show", showLighting);
        //GUILayout.EndHorizontal();
        //if (_target.EnableNoise /*|| _target.EnableGradient*/)
        if (GUILayout.Button("Lighting", EditorStyles.toolbarButton))
            _target.showLighting = !_target.showLighting;

        if (_target.showLighting)
        {
            if (_target._FogType == FogVolume.FogType.Textured)
            {
                GUILayout.BeginVertical("box");

                _target._AmbientColor = EditorGUILayout.ColorField(VariableField("Ambient", "Ambient is added as first light contribution. Alpha is used for volumetric shadows ambient amount"), _target._AmbientColor);
                _target.Absorption = EditorGUILayout.Slider(VariableField("Absorption", "How much ambient light is absorbed by noise. Depends on noise density"), _target.Absorption, 0, 1);
                if (_target.useHeightGradient)
                {
                    _target.HeightAbsorption = EditorGUILayout.Slider(VariableField("Height Absorption", "Multiplies lighting contributoins with height gradient 1"), _target.HeightAbsorption, 0, 1);

                }
                GUILayout.EndVertical();
                if (_target.EnableNoise)
                {
                    string HeightAbsorptionTooltip = "Define a height gradient to attenuate lighting. To disable, set both sliders to -1";
                    EditorGUILayout.LabelField(VariableField("Height absorption gradient", HeightAbsorptionTooltip), EditorStyles.boldLabel);
                    GUILayout.BeginVertical("box");
                    _target.HeightAbsorptionMin = EditorGUILayout.Slider(VariableField("min", HeightAbsorptionTooltip), _target.HeightAbsorptionMin, -1, 1);
                    _target.HeightAbsorptionMax = EditorGUILayout.Slider(VariableField("max", HeightAbsorptionTooltip), _target.HeightAbsorptionMax, -1, 1);
                    GUILayout.EndVertical();
                }
                // if( GUILayout.Button(_target.bAbsorption.ToString()))
                //   _target.bAbsorption = !_target.bAbsorption;
                // else
                // _target.bAbsorption = false;

                //
                // Debug.Log(_target.bAbsorption);
                if (_target.Absorption == 0)
                    _target.bAbsorption = false;
                else
                    _target.bAbsorption = true;

            }
            _target.Sun = (Light)EditorGUILayout.ObjectField("Sun Light", _target.Sun, typeof(Light), true);

            if (_target.Sun != null)
            {
                if (_target._FogType == FogVolume.FogType.Textured)
                    _target._LightExposure = EditorGUILayout.Slider(VariableField("Light Exposure", "Overrides light intensity"), _target._LightExposure, 1, 5);
                if (_target.EnableNoise)
                {
                    GUILayout.BeginVertical("box");
                    _target.Lambert = EditorGUILayout.Toggle(VariableField("Lambertian", "Computes lambert lighting. Disabled by default\n uncomment //#pragma shader_feature _LAMBERT_SHADING  in FogVolume.shader to enable"), _target.Lambert);
                    if (_target.Lambert)
                    {
                        _target.DirectLightingAmount = EditorGUILayout.Slider("Amount", _target.DirectLightingAmount, .01f, 10f);
                        _target.LambertianBias = EditorGUILayout.Slider("Lambertian Bias", _target.LambertianBias, .5f, 1);
                        _target.NormalDistance = EditorGUILayout.Slider("Normal detail", _target.NormalDistance, .01f, .0001f);
                        _target.DirectLightingDistance = EditorGUILayout.FloatField("Distance", _target.DirectLightingDistance);
                    }
                    GUILayout.EndVertical();
                }
                #region VolumeFogInscattering
                if (_target._FogType == FogVolume.FogType.Textured)
                    if (_target.bVolumeFog)
                    {
                        GUILayout.BeginVertical("box");
                        _target.VolumeFogInscattering = EditorGUILayout.Toggle(VariableField("Volume Fog Inscattering", "Computes phase function in fog"), _target.VolumeFogInscattering);
                        if (_target.VolumeFogInscattering && _target.bVolumeFog)
                        {
                            _target.VolumeFogInscatteringColor = EditorGUILayout.ColorField("Color", _target.VolumeFogInscatteringColor);
                            _target.VolumeFogInscatterColorAffectedWithFogColor = EditorGUILayout.Toggle(VariableField("Tint with fog color", "Off: reacts additively\nOn: incident light is tinted with the given fog color"), _target.VolumeFogInscatterColorAffectedWithFogColor);
                            _target.VolumeFogInscatteringAnisotropy = EditorGUILayout.Slider(VariableField("Anisotropy", "Balance of forward and back scattering"), _target.VolumeFogInscatteringAnisotropy, -1, 1);
                            _target.VolumeFogInscatteringIntensity = EditorGUILayout.Slider("Intensity", _target.VolumeFogInscatteringIntensity, 0, 1);
                            _target.VolumeFogInscatteringStartDistance = EditorGUILayout.FloatField("Start Distance", _target.VolumeFogInscatteringStartDistance);
                            _target.VolumeFogInscatteringTransitionWideness = EditorGUILayout.FloatField("Transition Wideness", _target.VolumeFogInscatteringTransitionWideness);
                        }
                        GUILayout.EndVertical();
                    }

                #endregion

                #region Inscattering
               // if (_target.NoiseIntensity > 0)
               // {
                    GUILayout.BeginVertical("box");
                    _target.EnableInscattering = EditorGUILayout.Toggle(VariableField("Inscattering", "Computes phase function in noise. It is affected by absorption"), _target.EnableInscattering);
                    if (_target.EnableInscattering)
                    {

                        _target.InscatteringColor = EditorGUILayout.ColorField("Color", _target.InscatteringColor);
                        _target.InscatteringShape = EditorGUILayout.Slider(VariableField("Anisotropy", "Balance of forward and back scattering"), _target.InscatteringShape, -1, 1);
                        _target.InscatteringIntensity = EditorGUILayout.Slider("Intensity", _target.InscatteringIntensity, 0, 1);
                        _target.InscatteringStartDistance = EditorGUILayout.FloatField("Start Distance", _target.InscatteringStartDistance);
                        _target.InscatteringTransitionWideness = EditorGUILayout.FloatField("Transition Wideness", _target.InscatteringTransitionWideness);


                    }
                    GUILayout.EndVertical();

               // }
               // else
                //    _target.EnableInscattering = false;
                #endregion

                if (_target.EnableNoise && _target._NoiseVolume != null)
                {
                    GUILayout.BeginVertical("box");

                    _target._DirectionalLighting = EditorGUILayout.Toggle(VariableField("Directional Lighting", "Light is absorbed along light direction. Looks like sub-surface scattering"), _target._DirectionalLighting);

                    if (_target._DirectionalLighting)
                    {

                        _target.LightExtinctionColor = EditorGUILayout.ColorField(VariableField("Extinction Color", "The color of the light after full absorption"), _target.LightExtinctionColor);
                       // _target.DirectionalLightingClamp= EditorGUILayout.Slider(VariableField("Clamp", "Limit the maximum extiction"), _target.DirectionalLightingClamp, 1,5);
                        _target._DirectionalLightingDistance = EditorGUILayout.Slider(VariableField("Distance", "Distance between samples"), _target._DirectionalLightingDistance, 0, 2.0f);
                        _target.DirectLightingShadowDensity = EditorGUILayout.Slider(VariableField("Density", "Amount of light being absorbed"), _target.DirectLightingShadowDensity, 0, 15);
                        _target.DirectLightingShadowSteps = EditorGUILayout.IntSlider(VariableField("Iterations", "How many times to compute. Each iteration computes noise one more time causing dramatical performance hits"), _target.DirectLightingShadowSteps, 1, 5);

                    }
                    GUILayout.EndVertical();
                }
            }



            if (_target.EnableNoise && _target.Sun)
            {


                GUILayout.BeginVertical("box");
                _target._ShadeNoise = EditorGUILayout.Toggle(VariableField("Self shadow", "Computes directional shadows. \n It has high performance impact"), _target._ShadeNoise);
                if (_target._ShadeNoise)
                {

                    // _target.Shade = EditorGUILayout.Slider("Shadow intensity", _target.Shade, 0, 1);
                    _target.ShadowShift = EditorGUILayout.Slider(VariableField("Shadow distance", "Distance between samples"), _target.ShadowShift, .0f, 2);
                    _target._SelfShadowSteps = EditorGUILayout.IntSlider("Iterations", _target._SelfShadowSteps, 1, 20);
                    _target._SelfShadowColor = EditorGUILayout.ColorField(VariableField("Shadow color", "Ambient color x Shadow color"), _target._SelfShadowColor);
                }
                GUILayout.EndVertical();
            }

            //
            if (_target._FogType == FogVolume.FogType.Textured)
            {

                if (_target.FogRenderer.shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.On ||
                    _target.FogRenderer.receiveShadows == true)
                {
                    GUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField("Opacity shadow map", EditorStyles.boldLabel);
                    // _target.CastShadows = EditorGUILayout.Toggle("Cast Shadows", _target.CastShadows);//lets control it trough the renderer
                    if (_target.Sun && _target.CastShadows)//casi mejor que un droplist
                    {
                        //tempShadowCaster = _target.CastShadows;
                        _target.ShadowCameraPosition = EditorGUILayout.IntField(VariableField("Vertical Offset", "Distance of the shadow camera. Use the prefab RT Viewer to see the result"), _target.ShadowCameraPosition);
                        _target.SunAttached = EditorGUILayout.Toggle(VariableField("Attach light", "The assigned light will inherit the volume rotation. This is useful to cast fake shadows with noise. Check the scene Sunset to see this in action"), _target.SunAttached);
                        _target.ShadowColor = EditorGUILayout.ColorField("Shadow Color", _target.ShadowColor);
                        _target._ShadowCamera.textureSize = (ShadowCamera.TextureSize)EditorGUILayout.EnumPopup(VariableField("Resolution", "Resolution of the shadow map"), _target._ShadowCamera.textureSize);
                        _target.ShadowCameraSkippedFrames = EditorGUILayout.IntSlider(VariableField("Skip frames", "Skip frames to reduce costs"), _target.ShadowCameraSkippedFrames, 0, 10);
                        GUILayout.BeginVertical("box");
                        EditorGUILayout.LabelField("Convolution", EditorStyles.boldLabel);

                        _target._ShadowCamera.iterations = EditorGUILayout.IntSlider("Iterations", _target._ShadowCamera.iterations, 0, 5);
                        if (_target._ShadowCamera.iterations > 0)
                            _target._ShadowCamera.Downsampling = EditorGUILayout.IntSlider("Downsampling", _target._ShadowCamera.Downsampling, 0, 5);
                        if (_target._ShadowCamera.iterations > 1)
                            _target._ShadowCamera.blurSpread = EditorGUILayout.Slider("Radius", _target._ShadowCamera.blurSpread, 0, 1);
                        GUILayout.EndVertical();
                    }
                    else
                    {
                        if (_target.FogRenderer.receiveShadows)
                            _target.ShadowCaster = (FogVolume)EditorGUILayout.ObjectField(VariableField("Shadowmap coming from:", "Assign here the fog volume that is used to cast shadows"), _target.ShadowCaster, typeof(FogVolume), true);


                        if (_target.ShadowCaster != null)
                            _target.CastShadows = false;

                        // tempShadowCaster = _target.CastShadows;
                    }
                    if (_target.CastShadows == false && _target.FogRenderer.receiveShadows)
                    {
                        _target.UseConvolvedLightshafts = EditorGUILayout.Toggle(VariableField("Use convolved lightshaft", "You can select here if you want to use the blurred shadow"), _target.UseConvolvedLightshafts);
                        _target.ShadowCutoff = EditorGUILayout.Slider(VariableField("Shadow Cutoff", "Apply contrast to the sampled shadow"), _target.ShadowCutoff, 0.001f, 1);
                    }
                    GUILayout.EndVertical();
                }
                //_target.RT_Opacity = (RenderTexture)EditorGUILayout.ObjectField("LightMapRT", _target.RT_Opacity, typeof(RenderTexture), false);
                //_target._LightMapTex = (Texture2D)EditorGUILayout.ObjectField("LightMapTex", _target._LightMapTex, typeof(Texture2D), false);

            }
            else
                _target.CastShadows = false;

            if (_target.Sun && _target.EnableNoise)
            {
                GUILayout.BeginVertical("box");

                _target.LightHalo = EditorGUILayout.Toggle("Halo", _target.LightHalo);
                if (_target.LightHalo && _target.EnableNoise)
                {
                    _target._LightHaloTexture = (Texture2D)EditorGUILayout.ObjectField("Halo texture", _target._LightHaloTexture, typeof(Texture2D), true);
                    _target._HaloWidth = EditorGUILayout.Slider("Width", _target._HaloWidth, 0, 1);

                    _target._HaloRadius = EditorGUILayout.Slider("Radius", _target._HaloRadius, -5, 1);
                    _target._HaloAbsorption = EditorGUILayout.Slider(VariableField("Absorption", "How much is absorption affecting"), _target._HaloAbsorption, 0, 1);
                    // _target._HaloOpticalDispersion = EditorGUILayout.Slider("OpticalDispersion", _target._HaloOpticalDispersion, 0, 8);
                    _target._HaloIntensity = EditorGUILayout.Slider("Intensity", _target._HaloIntensity, 0, 1);
                }
                GUILayout.EndVertical();
            }//TODO. Halo should be always available
        }

        #endregion

        #region PointLights
        string PointLightTittle = "";
        if (SystemInfo.graphicsShaderLevel > 30)
            PointLightTittle = "Lights";
        else
            PointLightTittle = "Lights not available on this platform ";// + SystemInfo.graphicsShaderLevel;

        if (_target._FogType == FogVolume.FogType.Textured)
            if (GUILayout.Button(PointLightTittle, EditorStyles.toolbarButton))
                _target.showPointLights = !_target.showPointLights;

        if (_target.showPointLights &&
            SystemInfo.graphicsShaderLevel > 30)
        {
            _target.PointLightsActive = EditorGUILayout.Toggle("Enable", _target.PointLightsActive);
        }

        if (_target.showPointLights && _target.PointLightsActive)
            if (_target._FogType == FogVolume.FogType.Textured)
            {

                _target.PointLightsRealTimeUpdate = EditorGUILayout.Toggle(VariableField("Real-time search", "Fog Volume will search for lights in the scene hierarchy. \nEnable this for a brief moment if lights were added to the scene."), _target.PointLightsRealTimeUpdate);

                _target.PointLightBoxCheck = EditorGUILayout.Toggle(VariableField("Inside box only", "Computes only lights that are inside of the volume boundaries. Turn off to compute lights that are outside of the volume"), _target.PointLightBoxCheck);
                //GUILayout.BeginHorizon=tal("toolbarbutton");
                //EditorGUILayout.LabelField("Point lights");

                //GUILayout.EndHorizontal();
                _target._LightScatterMethod = (FogVolume.LightScatterMethod)EditorGUILayout.EnumPopup("Attenuation Method", _target._LightScatterMethod);
                _target.PointLightsIntensity = EditorGUILayout.Slider("Intensity", _target.PointLightsIntensity, 0, 50);
                _target.PointLightingDistance = Mathf.Max(1, _target.PointLightingDistance);
                _target.PointLightingDistance = EditorGUILayout.FloatField(VariableField("Range clamp", "Use this parameter to limit the spherical distance of the pointlight so that it is not computed when attenuation is zero. Use it to improve performance"), _target.PointLightingDistance);
                _target.PointLightingDistance2Camera = Mathf.Max(1, _target.PointLightingDistance2Camera);

                _target.PointLightingDistance2Camera = EditorGUILayout.FloatField(VariableField("Draw distance", "Point-light is attenuated at the given distance and turned off as soon as that distance is exceeded"), _target.PointLightingDistance2Camera);
                //_target.PointLightScreenMargin = EditorGUILayout.Slider("Discard margin", _target.PointLightScreenMargin, 0, 5);deprecated
                _target.PointLightScreenMargin = 0;


                _target.PointLightCullSizeMultiplier = EditorGUILayout.FloatField(VariableField("Light visibility", "The higher this value, the further the lights need to be offscreen in order to be discarded by the renderer."), _target.PointLightCullSizeMultiplier);

                /*
                if (!_target.PointLightsRealTimeUpdate)
                {
                    var FogLights = serializedObject.FindProperty("FogLights");
                    GUILayout.BeginVertical("box");
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(FogLights, new GUIContent
                    ("Editable array"), true);
                    EditorGUI.indentLevel--;
                    GUILayout.EndVertical();
                }

                var PointLightsList = serializedObject.FindProperty("PointLightsList");
                GUILayout.BeginVertical("box");
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(PointLightsList, new GUIContent("Currently computed:"), true);
                EditorGUI.indentLevel--;
                GUILayout.EndVertical();
                */
                int totalLightCount = _target.GetTotalLightCount();
                int visibleLightCount = _target.GetVisibleLightCount();
                EditorGUILayout.HelpBox("Contains " + totalLightCount + " lights", MessageType.None);
                EditorGUILayout.HelpBox("Rendering " + visibleLightCount + " lights", MessageType.None);

            }

        #endregion

        #region ColorManagement
        //GUILayout.BeginHorizontal("toolbarbutton");

        //_target.ColorAdjust = EditorGUILayout.Toggle("Color Settings", _target.ColorAdjust);

        //showColorTab = EditorGUILayout.Toggle("     Show            \t     ", showColorTab);
        //GUILayout.EndHorizontal();
        if (GUILayout.Button(new GUIContent("Color management", ShowInspectorTooltips? "Color postprocessing such as contrast, brightness and tonemapper":"") , EditorStyles.toolbarButton))
            _target.showColorTab = !_target.showColorTab;

        if (/*_target.ColorAdjust && */_target.showColorTab)
        {
            _target.ColorAdjust = EditorGUILayout.Toggle("Active", _target.ColorAdjust);

            _target.Offset = EditorGUILayout.Slider("Offset", _target.Offset, -.5f, .5f);
            _target.Gamma = EditorGUILayout.Slider("Gamma", _target.Gamma, .01f, 3);

            if (_target.ColorAdjust)
                _target.Tonemap = EditorGUILayout.Toggle("Tonemap", _target.Tonemap);
            if (_target.Tonemap && _target.ColorAdjust)
                _target.Exposure = EditorGUILayout.Slider("Exposure", _target.Exposure, 2.5f, 5);
        }
        #endregion

        #region Renderer

        if (_target._FogType == FogVolume.FogType.Textured)
        {
            // GUILayout.BeginHorizontal("toolbarbutton");
            // EditorGUILayout.LabelField("Volume properties");
            //showVolumeProperties = EditorGUILayout.Toggle("Show", showVolumeProperties);
            // GUILayout.EndHorizontal();

            if (GUILayout.Button("Renderer", EditorStyles.toolbarButton))
                _target.showVolumeProperties = !_target.showVolumeProperties;

            if (_target.showVolumeProperties)
            {



                _target.NoiseIntensity = EditorGUILayout.Slider(VariableField("Intensity", "Opacity of noise and gradients"), _target.NoiseIntensity, 0, 1);

                GUILayout.BeginVertical("box");
                _target.SceneCollision = EditorGUILayout.Toggle(VariableField("Scene Collision", "Textured fog will collide with environment when set to ON. _CameraDepthTexture is requierd for Scene view collisions. Enable Depth in camera script to generate the depth map required for Game view collisions. Disable if the volume is rendered in the background to avoid unneded computation"), _target.SceneCollision);
                if (_target.SceneCollision)
                    _target._SceneIntersectionSoftness = EditorGUILayout.Slider(VariableField("Softness", "Pixels that get in contact with scene elements are faded to black. Use this slider to control the softeness of this transition"), _target._SceneIntersectionSoftness, 50, .01f);
                GUILayout.EndVertical();

                _target._jitter = EditorGUILayout.Slider(VariableField("Jitter", "Apply noise to the sample position. Use this in combination with DeNoise (camera script) to conceal the gaps between samples"), _target._jitter, 0, .1f);

                _target._SamplingMethod = (FogVolume.SamplingMethod)EditorGUILayout.EnumPopup(VariableField("Sampling Method", "Choose between distance to camera or view aligned planes. Hidden by default: uncomment #pragma shader_feature  SAMPLING_METHOD_ViewAligned in FogVolume.shader"), _target._SamplingMethod);
                _target.Iterations = EditorGUILayout.IntSlider(VariableField("Max Iterations", "Set the max iterations to perform."), _target.Iterations, 10, 1000);
                _target.IterationStep = EditorGUILayout.FloatField(VariableField("Iteration step size", "Distance between iterations"), _target.IterationStep);
                _target.FadeDistance = EditorGUILayout.FloatField(VariableField("Draw distance", "Noise is faded to black at the given distance. Loop will stop then"), _target.FadeDistance);
                _target._OptimizationFactor = EditorGUILayout.Slider(VariableField("Optimization Factor", "Used to increase the distance between iterations so that we can have more quality at close distances. Far distances will be less opaque"), _target._OptimizationFactor, 0, 5e-09f);

                if (_target.EnableNoise)
                {
                    GUILayout.BeginVertical("box");

                    _target.useHeightGradient = EditorGUILayout.Toggle(VariableField("Height Gradient", "Noise opacity is multiplied with a vertical gradient"), _target.useHeightGradient);
                    if (_target.useHeightGradient)
                    {
                        GUILayout.BeginVertical("box");
                        _target.GradMin = EditorGUILayout.Slider("Grad 1 Min", _target.GradMin, -1, 1);
                        _target.GradMax = EditorGUILayout.Slider("Grad 1 Max", _target.GradMax, -1, 1);
                        GUILayout.EndVertical();
                        GUILayout.BeginVertical("box");
                        _target.GradMin2 = EditorGUILayout.Slider("Grad 2 Min", _target.GradMin2, -1, 1);
                        _target.GradMax2 = EditorGUILayout.Slider("Grad 2 Max", _target.GradMax2, -1, 1);
                        GUILayout.EndVertical();
                    }
                    EditorGUILayout.EndVertical();
                }
                if (_target.EnableNoise)
                {
                    GUILayout.BeginVertical("box");

                    _target.bSphericalFade = EditorGUILayout.Toggle(VariableField("Radius Fade", "Multiplies noise opacity with a spherical range. Disabled by default. Uncomment #pragma shader_feature SPHERICAL_FADE in FogVolume.shader to enable"), _target.bSphericalFade);
                    if (_target.bSphericalFade)
                    {


                        _target.SphericalFadeDistance = EditorGUILayout.FloatField("Distance", _target.SphericalFadeDistance);
                    }
                    EditorGUILayout.EndVertical();
                }
                _target._DebugMode = (FogVolume.DebugMode)EditorGUILayout.EnumPopup(VariableField("View mode: ", "Disabled by default. Uncomment #pragma multi_compile _ DEBUG in FogVolume.shader"), _target._DebugMode);

                //Primitives
                GUILayout.BeginHorizontal("toolbarbutton");
                _target.EnableDistanceFields = EditorGUILayout.Toggle("Enable Primitives", _target.EnableDistanceFields);

                if (_target.EnableDistanceFields && _target.GetTotalPrimitiveCount() > 0)
                    EditorGUILayout.LabelField(_target.GetVisiblePrimitiveCount() + " primitives visible | " + _target.GetTotalPrimitiveCount() + " primitives in volume");
                else
                    EditorGUILayout.LabelField("0 primitives visible | 0 primitives in volume");
                GUILayout.EndHorizontal();

                if (_target.EnableDistanceFields)
                {
                    GUILayout.BeginVertical("box");
                    _target.ShowPrimitives = EditorGUILayout.Toggle(VariableField("Show primitives", "Enables the primitive renderer component"), _target.ShowPrimitives);
                    _target.PrimitivesRealTimeUpdate = EditorGUILayout.Toggle(VariableField("Real-time search", "Searches primitives that are inside of the volume. Turn on only to refresh the list"), _target.PrimitivesRealTimeUpdate);
                    EditorGUILayout.EndVertical();
                    _target.Constrain = EditorGUILayout.Slider(VariableField("Size", "Overrides the primitive original size"), _target.Constrain, 0, -150);
                    _target._PrimitiveEdgeSoftener = EditorGUILayout.Slider(VariableField("Softness", "Softens the primitive shape"), _target._PrimitiveEdgeSoftener, 1, 100);
                    _target._PrimitiveCutout = EditorGUILayout.Slider(VariableField("Cutout", "Cuts the distance field with the primitive boundaries"), _target._PrimitiveCutout, 0, .99999f);

                    /*
                    var PrimitivesList = serializedObject.FindProperty("PrimitivesList");
                    GUILayout.BeginVertical("box");
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(PrimitivesList, true);
                    EditorGUI.indentLevel--;
                    EditorGUILayout.EndVertical();
                    */
                }

            }
        }
        #endregion

        #region Noise
        if (GUILayout.Button("Noise", EditorStyles.toolbarButton))
            _target.showNoiseProperties = !_target.showNoiseProperties;
        // GUILayout.BeginHorizontal("toolbarbutton");
        //EditorGUILayout.LabelField("");

        // showNoiseProperties = EditorGUILayout.Toggle("      Show", showNoiseProperties);
        //GUILayout.EndHorizontal();

        if (/*_target.EnableNoise && */_target.showNoiseProperties)
        {

            _target.EnableNoise = EditorGUILayout.Toggle("Active", _target.EnableNoise);
            if (_target.EnableNoise)
            {
                GUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("Common", EditorStyles.boldLabel);
                //var TexturesRect = GUILayoutUtility.GetRect(new GUIContent(""), GUIStyle.none);
                //EditorGUI.indentLevel++;
                //_target.TexturesGroup = EditorGUI.Foldout(TexturesRect, _target.TexturesGroup, "Noise textures");
                //if (_target.TexturesGroup)
                //{
                //    _target._NoiseVolume = (Texture3D)EditorGUILayout.ObjectField("Base", _target._NoiseVolume, typeof(Texture3D), false);
                //   // _target._NoiseVolume2 = (Texture3D)EditorGUILayout.ObjectField("Detail", _target._NoiseVolume2, typeof(Texture3D), false);
                //    _target.CoverageTex = (Texture2D)EditorGUILayout.ObjectField(VariableField("Coverage (Experimental)",
                //        "Input texture to add or multiply coverage. This texture is sampled in the vertical axis. Uncomment #define COVERAGE in FogVolume.shader and test scene: Ocean"), _target.CoverageTex, typeof(Texture2D), false);
                //}
                //EditorGUI.indentLevel--;
                // _target.CastShadows = tempShadowCaster;
                _target._3DNoiseScale = EditorGUILayout.FloatField(VariableField("Scale", "Global size of all the noise layers"), _target._3DNoiseScale);
                //_target.Procedural = EditorGUILayout.Toggle("Procedural", _target.Procedural);




                //
                #region Coordinates&Deformers
                GUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Coordinates", EditorStyles.boldLabel);

                _target.Speed = EditorGUILayout.Vector3Field("Scroll", _target.Speed);
                _target.Stretch = EditorGUILayout.Vector3Field("Stretch", _target.Stretch);
                GUILayout.EndVertical();

                GUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Deformers", EditorStyles.boldLabel);


                _target.Vortex = EditorGUILayout.Slider(VariableField("Swirl", "Twist the space. Very useful to break tiling"), _target.Vortex, 0, 3);
                if (_target.Vortex > 0)
                {
                    _target.RotationSpeed = EditorGUILayout.Slider("Rotation Speed", _target.RotationSpeed, 0, 10);
                    _target.rotation = EditorGUILayout.Slider("Rotation", _target.rotation, 0, 360);
                    _target._VortexAxis = (FogVolume.VortexAxis)EditorGUILayout.EnumPopup("Axis", _target._VortexAxis);
                }
                GUILayout.EndVertical();

                //
                GUILayout.EndVertical();
                //  GUILayout.EndVertical();
                #endregion
                GUILayout.BeginVertical("Box");

                EditorGUILayout.LabelField("Base layer", EditorStyles.boldLabel);
                _target.Coverage = EditorGUILayout.Slider(VariableField("Coverage", "Multiplies the base layer intensity. Lighting is affected as density increases"), _target.Coverage, 0, 5);
                _target.NoiseContrast = EditorGUILayout.Slider(VariableField("Contrast", "Apply contrast to noise. High values creates more empty space, which is good for performance"), _target.NoiseContrast, 0, 20);
                _target.NoiseDensity = EditorGUILayout.Slider(VariableField(
                    "Density", "Opacity of the final noise. This won't change the shape of the noise as Intensity does. It only affects the density / opacity of the noise"),
                    _target.NoiseDensity, 0, 20);

                _target.Octaves = EditorGUILayout.IntSlider(VariableField("Octaves", "How many layers of noise to use"), _target.Octaves, 1, 5);

                _target.BaseTiling = EditorGUILayout.FloatField(VariableField("Base Tiling", "Independent scale for this layer"), _target.BaseTiling);
                _target._BaseRelativeSpeed = EditorGUILayout.Slider(VariableField("Speed", "Independent speed multiplier for this layer"), _target._BaseRelativeSpeed, 0, 4);
                GUILayout.EndVertical();

                GUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("Detail layer", EditorStyles.boldLabel);
                _target.DetailTiling = EditorGUILayout.FloatField("Tiling", _target.DetailTiling);
                _target._DetailRelativeSpeed = EditorGUILayout.Slider("Speed", _target._DetailRelativeSpeed, 0, 20);
                _target.DetailDistance = EditorGUILayout.FloatField(VariableField("Draw Distance", "This layer is a lot more costly than the base. Limit the draw distance to incerase performance"), _target.DetailDistance);
                if (_target.DetailDistance > 0)
                {
                    _target._NoiseDetailRange = EditorGUILayout.Slider(VariableField("Trim",
                        "The detail layer is used to cut the base layer at the edges. If contrast is not too high, this will be more effective"), _target._NoiseDetailRange, 0, 1);
                    _target._DetailMaskingThreshold = EditorGUILayout.Slider(VariableField
                        ("Trim Threshold", "Detail layer is applied only at the edges of the base layer. Use this slider to control the size of this edge"),
                        _target._DetailMaskingThreshold, 1, 350);
                    //_target.DetailSamplingBaseOpacityLimit = EditorGUILayout.Slider(VariableField("Cutoff", "This is the threshold to stop the detail sampling. We don't compute detail when the base layer opacity is higher than this value"), _target.DetailSamplingBaseOpacityLimit, 0, 1);


                    if (_target.Octaves > 1)
                        _target._Curl = EditorGUILayout.Slider(VariableField("Curl", "Used to distort the edges. Check the 'Time of Day' demo to see this in action"), _target._Curl, 0, 1);
                }
                GUILayout.EndVertical();





                //_target.noise2D= (Texture2D)EditorGUILayout.ObjectField("noise2D", _target.noise2D, typeof(Texture2D), false);





            }
        }

        #endregion

        #region Gradient
        if (GUILayout.Button("Gradient", EditorStyles.toolbarButton))
            _target.showGradient = !_target.showGradient;

        if (_target.showGradient)
        {
            _target.EnableGradient = EditorGUILayout.Toggle("Active", _target.EnableGradient);

            if (_target.EnableGradient)
                _target.Gradient = (Texture2D)EditorGUILayout.ObjectField("", _target.Gradient, typeof(Texture2D), true);
        }
        #endregion



        #region Footer
        GUILayout.EndVertical();//termina estilo anterior

        EditorGUI.indentLevel++;
        if (GUILayout.Button("Other", EditorStyles.toolbarButton))
            _target.OtherTAB = !_target.OtherTAB;

        GUILayout.BeginVertical(ThemeFooter);

        if (_target.OtherTAB)
        {
            GUILayout.Space(10);
            var OtherRect = GUILayoutUtility.GetRect(new GUIContent(""), GUIStyle.none);
            _target.showOtherOptions = EditorGUI.Foldout(OtherRect, _target.showOtherOptions, "Rendering options");
            if (_target.showOtherOptions)
            {
                //EditorGUI.indentLevel++;

              //  if(!_target.CreateSurrogate)
                _target.DrawOrder = EditorGUILayout.IntField(new GUIContent("DrawOrder", "Ignored when surrogates are not created manually"), _target.DrawOrder);
                _target._PushAlpha = EditorGUILayout.Slider(new GUIContent("Push Alpha", "Compensate opacity when samples are not enough"), _target._PushAlpha, 1, 2f);
                _target._ztest = (UnityEngine.Rendering.CompareFunction)EditorGUILayout.EnumPopup("ZTest ", _target._ztest);
                if (_target.GameCameraGO != null /*&& _target._FogType == FogVolume.FogType.Textured*/)
                    if (_target.GameCameraGO.GetComponent<FogVolumeRenderer>() &&
                        _target.GameCameraGO.GetComponent<FogVolumeRenderer>()._Downsample > 0 &&
                        _target._FogType == FogVolume.FogType.Textured)
                        _target.CreateSurrogate = EditorGUILayout.Toggle(VariableField("Create Surrogate mesh",
                            "A copy of the volume mesh is created to project the result of the low-res render. This is good for preview, but usually, we have to create surrogate meshes by hand to sort them manually"), _target.CreateSurrogate);
                _target._VisibleByReflectionProbeStatic = EditorGUILayout.Toggle("Visible for static probe", _target._VisibleByReflectionProbeStatic);
                _target.RenderableInSceneView = EditorGUILayout.Toggle(VariableField("Render In Scene View", "Volume is not visible in Scene view. Please not that it won't be visible for reflection probes either"), _target.RenderableInSceneView);
                _target.ExcludeFromLowRes = EditorGUILayout.Toggle(VariableField("Exclude from low res",
                        "Exclude this fog volume from the main game camera low res process\n" +
                        "Enable and change the layer\n useful when rendering underwater fog with another camera\nA custom depth buffer can be injected uncommenting this line: #pragma multi_compile _ ExternalDepth"), _target.ExcludeFromLowRes);
                if (_target.ExcludeFromLowRes)
                    _target.InjectCustomDepthBuffer = EditorGUILayout.Toggle(VariableField("External depth buffer",
                        "Feed this Volume with a custom depth buffer with name: _CustomCameraDepthTexture\n useful when rendering underwater fog with another camera that has horizontal clip plane\nUncomment this line: #pragma multi_compile _ ExternalDepth"), _target.InjectCustomDepthBuffer);
            }

            GUILayout.Space(10);
            var DebugOptionsRect = GUILayoutUtility.GetRect(new GUIContent(""), GUIStyle.none);
            _target.showDebugOptions = EditorGUI.Foldout(DebugOptionsRect, _target.showDebugOptions, "Debug options");
            if (_target.showDebugOptions)
            {
                _target.ShowDebugGizmos =
                        EditorGUILayout.Toggle("Draw Debug Gizmos", _target.ShowDebugGizmos);
            }

                GUILayout.Space(10);
                var CustomizationOptionsRect = GUILayoutUtility.GetRect(new GUIContent(""), GUIStyle.none);
                _target.showCustomizationOptions = EditorGUI.Foldout(CustomizationOptionsRect, _target.showCustomizationOptions, "Customize look");
                if (_target.showCustomizationOptions)
                {
                    // EditorGUI.indentLevel++;
                    // _target._InspectorBackground = (Texture2D)EditorGUILayout.ObjectField("Background", _target._InspectorBackground, typeof(Texture2D), false);
                    if (EditorGUIUtility.isProSkin)
                    {
                        if (_target._InspectorBackground.Length > 0)
                            _target._InspectorBackgroundIndex =
                                    EditorGUILayout.IntSlider("Inspector background",
                                                              _target._InspectorBackgroundIndex,
                                                              0,
                                                              _target._InspectorBackground.Length -
                                                              1);
                    }
                    _target.HideWireframe = EditorGUILayout.Toggle("Scene view wireframe", _target.HideWireframe);
                    ShowInspectorTooltips = EditorGUILayout.Toggle(new GUIContent("Enable inspector tooltips", "Work in progress"), ShowInspectorTooltips);
                    // EditorGUI.indentLevel--;
                }

                GUILayout.Space(10);

            bool prevSaveMaterials = _target.SaveMaterials;
            _target.SaveMaterials =
                    EditorGUILayout.Toggle(new GUIContent("Save Materials", "Ensures that material shaders are built correctly when creating a platform specific build."),
                                           _target.SaveMaterials);
            if (prevSaveMaterials != _target.SaveMaterials && _target.SaveMaterials)
            {
                _target.RequestSavingMaterials = true;
            }
            else
            {
                _target.RequestSavingMaterials = false;
            }

        }





        if (_target._InspectorBackground.Length > 0)
            if (_target._InspectorBackground[_target._InspectorBackgroundIndex] != null && EditorGUIUtility.isProSkin)
                Theme1.normal.background = _target._InspectorBackground[_target._InspectorBackgroundIndex];
        GUILayout.EndVertical();//end footer style
        EditorGUI.indentLevel--;
        //GUI.backgroundColor = new Color(.9f, .5f, .9f);





        #endregion

        #region Info
        if (GUILayout.Button("Info", EditorStyles.toolbarButton))
            _target.generalInfo = !_target.generalInfo;

        if (_target.generalInfo)
        {
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Description",EditorStyles.boldLabel);
            _target.Description = EditorGUILayout.TextArea(_target.Description, GUILayout.MaxWidth(300));
            EditorGUILayout.LabelField("Version 3.4.3");
            EditorGUILayout.LabelField("Release date: August 2020");
            EditorGUILayout.LabelField("Fog type: " + _target._FogType.ToString());

            #region Camera

            if (_target.GameCameraGO)
            {
                EditorGUILayout.LabelField("Assigned camera: " + _target.GameCameraGO.name);
                if (GUILayout.Button("Select " + _target.GameCameraGO.name, EditorStyles.toolbarButton))
                    Selection.activeGameObject = _target.GameCameraGO;
            }
            else
                GUILayout.Button("No valid camera found", EditorStyles.toolbarButton);
            GUILayout.EndVertical();
            GUILayout.Space(10);
            #endregion



        }
        #endregion
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(target);
        }



        serializedObject.ApplyModifiedProperties();
    }

}