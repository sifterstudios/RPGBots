#if GAIA_PRESENT && UNITY_EDITOR

using UnityEngine;
using System;
using UnityEditor;
using UnityEngine.Rendering;

namespace Gaia.GX.FogVolume3
{
    /// <summary>
    /// Fog Volume 3 creator for Gaia.
    /// </summary>
    public class FogVolumeGaiaIntegration : MonoBehaviour
    {
        #region Generic informational methods

        /// <summary>
        /// Returns the publisher name if provided. 
        /// This will override the publisher name in the namespace ie Gaia.GX.PublisherName
        /// </summary>
        /// <returns>Publisher name</returns>
        public static string GetPublisherName()
        {
            return "David Miranda";
        }

        /// <summary>
        /// Returns the package name if provided
        /// This will override the package name in the class name ie public class PackageName.
        /// </summary>
        /// <returns>Package name</returns>
        public static string GetPackageName()
        {
            return "Fog Volume 3";
        }

        #endregion

        #region Methods exposed by Gaia as buttons must be prefixed with GX_       

        public static void GX_About()
        {
            EditorUtility.DisplayDialog("About Fog Volume 3", "This integration adds Fog Volume 3 to your scene. After adding your Fog Volume components adjust their Y positions to better suit your scene. Also pay attention to the Fog Volume settings on your main camera. For example changing falloff will reduce the blur applied to distant clouds.", "OK");
        }

        public static void GX_Setup_AddGroundFog()
        {
            //Pick colour of main light
            GameObject goLight = GameObject.Find("Directional Light");
            Light mainLight = null;
            if (goLight != null)
            {
                mainLight = goLight.GetComponent<Light>();
            }
            else
            {
                mainLight = GameObject.FindObjectOfType<Light>();
            }
            Color mainLightColor = Color.white;
            if (mainLight != null)
            {
                mainLightColor = mainLight.color;
            }

            //First make sure its not already in scene
            GameObject fvGroundFog = GameObject.Find("Fog Volume [Ground Fog]");
            if (fvGroundFog == null)
            {
                fvGroundFog = new GameObject();
                fvGroundFog.name = "Fog Volume [Ground Fog]";
                fvGroundFog.AddComponent<MeshRenderer>();
                fvGroundFog.AddComponent<FogVolume>();
                fvGroundFog.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                fvGroundFog.GetComponent<Renderer>().receiveShadows = false;
                fvGroundFog.GetComponent<Renderer>().reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
                fvGroundFog.GetComponent<Renderer>().lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            }

            //Adjust its position and size
            FogVolume fvVolume = fvGroundFog.GetComponent<FogVolume>();
            if (fvVolume != null)
            {
                GaiaSceneInfo info = GaiaSceneInfo.GetSceneInfo();

                Debug.Log(info.m_seaLevel);

                fvVolume.transform.position = new Vector3(info.m_sceneBounds.center.x, info.m_seaLevel + 0.01f + (info.m_sceneBounds.extents.y / 4f), info.m_sceneBounds.center.z );//   info.m_sceneBounds.center;
                fvVolume.fogVolumeScale = new Vector3(info.m_sceneBounds.size.x * 3, info.m_sceneBounds.extents.y / 2f, info.m_sceneBounds.size.z * 3);

                //And adjust camera far clip as well
                float maxClip = Math.Max(info.m_sceneBounds.size.x, info.m_sceneBounds.size.z) * 3f;
                Camera mainCamera = Camera.main;
                if (mainCamera != null)
                {
                    if (mainCamera.farClipPlane < maxClip)
                    {
                        mainCamera.farClipPlane = maxClip + 200f;
                    }
                }

                fvVolume.FogMainColor = new Color(53f/255f, 76f/255f, 114f/255f);
                //fvVolume.Visibility = maxClip;
                fvVolume.Visibility = 800f;
                fvVolume.EnableInscattering = true;
                fvVolume.InscatteringColor = Color.Lerp(mainLightColor, Color.black, 0.8f);
                fvVolume.VolumeFogInscatteringAnisotropy = 0.59f;
                fvVolume.InscatteringIntensity = 0.07f;
                fvVolume.InscatteringStartDistance = 5f;
                fvVolume.InscatteringTransitionWideness = 300f;

                //Other
                fvVolume.DrawOrder = 3;
                fvVolume._PushAlpha = 1.0025f;
                fvVolume._ztest = CompareFunction.Always;
            }
        }

        public static void GX_Setup_AddClouds()
        {
            //Pick colour of main light
            Color mainLightColor = Color.white;
            GameObject goLight = GameObject.Find("Directional Light");
            Light mainLight;
            if (goLight != null)
            {
                mainLight = goLight.GetComponent<Light>();
            }
            else
            {
                mainLight = GameObject.FindObjectOfType<Light>();
            }
            if (mainLight != null)
            {
                mainLightColor = mainLight.color;
            }

            //Get the main camera
            Camera mainCamera = Camera.main;

            //First make sure its not already in scene - if it isnt then add it
            FogVolume fvVolume;
            GameObject goClouds = GameObject.Find("Fog Volume [Clouds]");
            if (goClouds == null)
            {
                goClouds = new GameObject();
                goClouds.name = "Fog Volume [Clouds]";
                goClouds.AddComponent<MeshRenderer>();
                fvVolume = goClouds.AddComponent<FogVolume>();
                goClouds.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                goClouds.GetComponent<Renderer>().receiveShadows = false;
                goClouds.GetComponent<Renderer>().reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
                goClouds.GetComponent<Renderer>().lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;

                //Create the horizon
                GameObject goHorizon = GameObject.CreatePrimitive(PrimitiveType.Plane);
                goHorizon.name = "Horizon";
                goHorizon.transform.parent = goClouds.transform;
                goHorizon.transform.localPosition = new Vector3(0f, -79f, 0f);
                goHorizon.GetComponent<MeshRenderer>().enabled = false;
                goHorizon.GetComponent<MeshCollider>().enabled = false;


                //Create the priority script
                FogVolumePriority fvPriority = goClouds.AddComponent<FogVolumePriority>();
                fvPriority.GameCamera = mainCamera;
                fvPriority.FogOrderCameraAbove = 4;
                fvPriority.FogOrderCameraBelow = -1;
                fvPriority.thisFog = fvVolume;
                fvPriority.Horizon = goHorizon;
            }

            //Adjust its position and size
            fvVolume = goClouds.GetComponent<FogVolume>();
            if (fvVolume != null)
            {
                GaiaSceneInfo info = GaiaSceneInfo.GetSceneInfo();

                //Location and scale
                fvVolume.transform.position = new Vector3(info.m_sceneBounds.center.x, info.m_seaLevel + 200f, info.m_sceneBounds.center.z);//   info.m_sceneBounds.center;
                fvVolume.fogVolumeScale = new Vector3(info.m_sceneBounds.size.x * 3, 100f, info.m_sceneBounds.size.z * 3);

                //Camera far clip
                float maxClip = Math.Max(info.m_sceneBounds.size.x, info.m_sceneBounds.size.z) * 3f;
                if (mainCamera != null)
                {
                    if (mainCamera.farClipPlane < maxClip)
                    {
                        mainCamera.farClipPlane = maxClip + 200f;
                    }
                }

                //Fog type and blend mode
                fvVolume._FogType = FogVolume.FogType.Textured;
                fvVolume._BlendMode = FogVolumeRenderer.BlendMode.PremultipliedTransparency;

                //Lighting
                fvVolume._AmbientColor = Color.Lerp(mainLightColor, Color.black, 0.1f);
                fvVolume.useHeightGradient = true;
                fvVolume.Absorption = 0.8f;
                fvVolume.HeightAbsorption = 0.185f;
                fvVolume.bAbsorption = true;

                fvVolume.EnableInscattering = true;
                fvVolume.InscatteringColor = mainLightColor;
                fvVolume.InscatteringShape = 0.05f;
                fvVolume.InscatteringIntensity = 0.882f;
                fvVolume.InscatteringStartDistance = 0f;
                fvVolume.InscatteringTransitionWideness = 1f;

                fvVolume._DirectionalLighting = true;
                fvVolume.LightExtinctionColor = Color.Lerp(mainLightColor, Color.black, 0.8f);
                fvVolume._DirectionalLightingDistance = 0.0008f;
                fvVolume.DirectLightingShadowDensity = 6f;
                fvVolume.DirectLightingShadowSteps = 1;

                //Renderer
                fvVolume.NoiseIntensity = 1f;
                fvVolume.SceneCollision = false; //Faster i suppose ?
                fvVolume.Iterations = 500;
                fvVolume.IterationStep = 100;
                fvVolume._OptimizationFactor = 0.0000005f;

                fvVolume.GradMin = 0.19f;
                fvVolume.GradMax = 0.06f;
                fvVolume.GradMin2 = -0.25f;
                fvVolume.GradMax2 = 0.21f;

                //Noise
                fvVolume.EnableNoise = true;
                fvVolume._3DNoiseScale = 0.15f;
                fvVolume.Speed = new Vector4(0.49f, 0f, 0f, 0f);
                fvVolume.Vortex = 0.47f;
                fvVolume.RotationSpeed = 0f;
                fvVolume.rotation = 324f;
                fvVolume._VortexAxis = FogVolume.VortexAxis.Y;
                fvVolume.Coverage = 2.44f;
                fvVolume.NoiseContrast = 12.9f;
                fvVolume.NoiseDensity = 0.2f;
                fvVolume.Octaves = 3;
                fvVolume.BaseTiling = 150f;
                fvVolume._BaseRelativeSpeed = 0.85f;
                fvVolume.DetailTiling = 285.3f;
                fvVolume._DetailRelativeSpeed = 16.6f;
                fvVolume.DetailDistance = 5000f;
                fvVolume._NoiseDetailRange = 0.337f;
                fvVolume._DetailMaskingThreshold = 8f;
                fvVolume._Curl = 0.364f;

                //Other
                fvVolume.DrawOrder = 4;
                fvVolume._ztest = CompareFunction.LessEqual;
                fvVolume.CreateSurrogate = true;
            }
        }

        public static void GX_Setup_AddPostEffects()
        {
            //Update renderer settings to dampen things down a bit for newcomers
            FogVolumeRenderer fvRenderer = GameObject.FindObjectOfType<FogVolumeRenderer>();
            if (fvRenderer != null)
            {
                fvRenderer._Downsample = 3;
                fvRenderer._BlendMode = FogVolumeRenderer.BlendMode.PremultipliedTransparency;
                fvRenderer.GenerateDepth = false;
            }

            //Add screen if missing
            FogVolumeScreen fvScreen = GameObject.FindObjectOfType<FogVolumeScreen>();
            if (fvScreen == null && Camera.main != null)
            {
                fvScreen = Camera.main.gameObject.AddComponent<FogVolumeScreen>();
                fvScreen.Downsample = 3;
                fvScreen.iterations = 3;
                fvScreen.blurSpread = 0.2f;
            }
        }

        public static void GX_Quality_Low()
        {
            //What about ground fog and cloud settings ?
            GameObject goGroundFog = GameObject.Find("Fog Volume [Ground Fog]");
            if (goGroundFog != null)
            {
                FogVolume fvGroundFog = goGroundFog.GetComponent<FogVolume>();
                if (fvGroundFog != null)
                {
                    //Make adjustments
                }
            }
            GameObject goClouds = GameObject.Find("Fog Volume [Clouds]");
            if (goClouds != null)
            {
                FogVolume fvClouds = goClouds.GetComponent<FogVolume>();
                if (fvClouds != null)
                {
                    //Make adjustments
                }
            }

            //Update renderer settings
            FogVolumeRenderer fvRenderer = GameObject.FindObjectOfType<FogVolumeRenderer>();
            if (fvRenderer != null)
            {
                fvRenderer._Downsample = 8;
                fvRenderer._BlendMode = FogVolumeRenderer.BlendMode.PremultipliedTransparency;
            }

            //Update screen settings
            FogVolumeScreen fvScreen = GameObject.FindObjectOfType<FogVolumeScreen>();
            if (fvScreen != null)
            {
                
            }
        }

        public static void GX_Quality_Medium()
        {
            //What about ground fog and cloud settings ?


            //Update renderer settings
            FogVolumeRenderer fvRenderer = GameObject.FindObjectOfType<FogVolumeRenderer>();
            if (fvRenderer != null)
            {
                fvRenderer._Downsample = 4;
            }

            //Update screen settings
            FogVolumeScreen fvScreen = GameObject.FindObjectOfType<FogVolumeScreen>();
            if (fvScreen != null)
            {

            }
        }

        public static void GX_Quality_High()
        {
            //What about ground fog and cloud settings ?

            //Update renderer settings
            FogVolumeRenderer fvRenderer = GameObject.FindObjectOfType<FogVolumeRenderer>();
            if (fvRenderer != null)
            {
                fvRenderer._Downsample = 2;
            }

            //Update screen settings
            FogVolumeScreen fvScreen = GameObject.FindObjectOfType<FogVolumeScreen>();
            if (fvScreen != null)
            {

            }
        }

        public static void GX_Quality_Epic()
        {
            //What about ground fog and cloud settings ?



            //Update renderer settings
            FogVolumeRenderer fvRenderer = GameObject.FindObjectOfType<FogVolumeRenderer>();
            if (fvRenderer != null)
            {
                fvRenderer._Downsample = 0;

            }

            //Update screen settings
            FogVolumeScreen fvScreen = GameObject.FindObjectOfType<FogVolumeScreen>();
            if (fvScreen != null)
            {

            }
        }

        #endregion
    }
}

#endif