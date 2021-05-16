Shader "Hidden/FogVolume"
{
	Properties
	{
		[hideininspector]_SrcBlend("__src", Float) = 1.0
		[hideininspector]_NoiseVolume("_NoiseVolume", 3D)= "white" {}
		[hideininspector]_NoiseVolume2("_NoiseVolume2", 3D) = "white" {}
		[hideininspector]_Gradient("_Gradient", 2D) = "white" {}
		[hideininspector]CoverageTex("CoverageTex", 2D) = "grey" {}
		//[hideininspector]_ShadowTexture("_ShadowTexture", 2D) = "white" {}
	}
		CGINCLUDE
		//#define COVERAGE
		//#define EARTH_CLOUD_STYLE
		//#define VOLUMETRIC_SHADOWS _VolumetricShadowsEnabled
		//#define CONVOLVE_VOLUMETRIC_SHADOWS
		//custom depth input
		//#define ExternalDepth
		#define AMBIENT_AFFECTS_FOG_COLOR _AmbientAffectsFogColor
		//#define DEBUG_PRIMITIVES
		#define VOLUMETRIC_SHADOWS _VolumetricShadowsEnabled
		#define FOG_TINTS_INSCATTER VolumeFogInscatterColorAffectedWithFogColor
		#include "UnityCG.cginc" 
		#include "CommonInputs.cginc"
		#include "Integrations.cginc"
		#define PROBES _ProxyVolume

		half NoiseAtten = 1;
		int _SrcBlend;
		int _ztest;

		#include "FogVolumeCommon.cginc"
		#define DEBUG_ITERATIONS 1
		#define DEBUG_INSCATTERING 2
		#define DEBUG_VOLUMETRIC_SHADOWS 3
		#define DEBUG_VOLUME_FOG_INSCATTER_CLAMP 4
		#define DEBUG_VOLUME_FOG_PHASE 5
		#include "FogVolumeFragment.cginc"
		

		ENDCG

	//normal pass
	SubShader
	{ 
		
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True"  "RenderType" = "None" }
		LOD 600
		//Blend SrcAlpha OneMinusSrcAlpha
			//Blend One OneMinusSrcAlpha 
			Blend[_SrcBlend] OneMinusSrcAlpha 

		Fog{ Mode Off }
		Cull Front
		Lighting Off
		ZWrite Off
		ZTest [_ztest]

	Pass 
	{
		

	CGPROGRAM
	//#pragma multi_compile_local _ ExternalDepth
	#pragma multi_compile _ _FOG_LOWRES_RENDERER 			
	#pragma shader_feature _INSCATTERING  
	#pragma shader_feature VOLUME_FOG
	#pragma shader_feature _VOLUME_FOG_INSCATTERING  
	#pragma shader_feature _FOG_GRADIENT  
	#pragma shader_feature _FOG_VOLUME_NOISE    
	// #pragma shader_feature _COLLISION            
	//#pragma multi_compile_local _ DEBUG
	//#pragma shader_feature  SAMPLING_METHOD_ViewAligned
	#pragma shader_feature HEIGHT_GRAD
	//#pragma shader_feature _TONEMAP	
	#pragma shader_feature JITTER
	#pragma shader_feature ColorAdjust	
	#pragma shader_feature	ABSORPTION			
	#pragma multi_compile_local _ Twirl_X Twirl_Y Twirl_Z
	#pragma shader_feature _SHADE
	#pragma shader_feature DF
	#pragma shader_feature DIRECTIONAL_LIGHTING 
	#pragma multi_compile_local _ ATTEN_METHOD_1 ATTEN_METHOD_2 ATTEN_METHOD_3
	#pragma shader_feature SPHERICAL_FADE
	//#pragma shader_feature _LAMBERT_SHADING
	#pragma multi_compile_local _ VOLUME_SHADOWS
	#pragma shader_feature LIGHT_ATTACHED
	#pragma shader_feature HALO
	//Unity define for stereo is not working. Had to do it manually
	#pragma multi_compile_local _ FOG_VOLUME_STEREO_ON 
	#pragma exclude_renderers d3d9
	//#pragma only_renderers d3d11
	#pragma vertex vert
	#pragma fragment frag	

	#pragma target 3.0

	ENDCG
	}

	}
	
//opacity pass . Only for shadow rt
		SubShader
		{
			Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True"  "RenderType" = "FogVolume_ShadowCaster" }
			LOD 100
			Blend SrcAlpha OneMinusSrcAlpha

			Fog{ Mode Off }
			Cull Front
			Lighting Off
			ZWrite Off
			ZTest Always

			Pass 
			{
			Fog{ Mode Off }			

			CGPROGRAM
			#define SHADOW_PASS
			#pragma shader_feature _FOG_GRADIENT  
			#pragma shader_feature _FOG_VOLUME_NOISE    
			// #pragma shader_feature _COLLISION            
			#pragma shader_feature  SAMPLING_METHOD_ViewAligned
			#pragma shader_feature HEIGHT_GRAD
			#pragma multi_compile_local Twirl_X Twirl_Y Twirl_Z
			#pragma shader_feature DF
			//#pragma multi_compile_local SHADOW_PASS
			#pragma shader_feature SPHERICAL_FADE
			#pragma exclude_renderers d3d9
			//#pragma only_renderers d3d11
			#pragma glsl
			#pragma vertex vert
			#pragma fragment frag	

			#pragma target 3.0
			ENDCG
			}

		}
}
