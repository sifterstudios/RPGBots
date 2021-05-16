﻿// Errors if instancing is enabled - as we may not overwrite unity_ObjectToWorld which is an array...
// So comment #pragma multi_compile_instancing in the cg shader parts seems to fix this.

Shader "AdvancedTerrainGrass/Grass Array DepthPrepass Shader" {
	Properties {

		[Space(8)]
		[Header(For the terrain engine only)]
		[NoScaleOffset] _MainTex 					("Albedo Tex (RGB) Alpha (A)", 2D) = "white" {}
		
		[Header(Actually used texture array)]
		[NoScaleOffset] _MainTexArray 				("Albedo Array (RGB) Alpha (A)", 2DArray) = "white" {}
		_Layers									 	("Number of Layers - 1 (int)", Float) = 1
		[KeywordEnum(Random,BySize,SoftMerge)] _MixMode("Texture Mix Mode", Float) = 0
		_SizeThreshold								("    Size Threshold", Range(0,1)) = 0.5

		_Cutoff 									("Alpha cutoff", Range(0,1)) = 0.5

		[Space(8)]
		[HideInInspector] _MinMaxScales 			("MinMaxScale Factors", Vector) = (1,1,1,1)
		_HealthyColor 								("Healthy Color (RGB) Bending (A)", Color) = (1,1,1,1)
		_DryColor 									("Dry Color (RGB) Bending (A)", Color) = (1,1,1,1)

		[Header(Lighting)]
		[Space(8)]
		[Toggle(_NORMAL)] _SampleNormal 			("Use NormalBuffer", Float) = 0
		_NormalBend									("Bend Normal", Range(0,1)) = 0.5
		[Toggle(_SPECULARHIGHLIGHTS_OFF)] _Spec 	("Enable specular highlights", Float) = 0
		[NoScaleOffset]_SpecTexArray 				("    Trans (R) Spec Mask (G) Smoothness (B)", 2DArray) = "black" {}

		[Space(8)]
		_TransStrength 								("Translucency Strength", Range(0, 1)) = 1.0
// 		Grass has no TranslucencyPower
//		_TransPower ("TransPower", Range(0, 1)) = 0.8

		[Header(Two Step Culling)]
        [Space(8)]
		_Clip 										("Clip Threshold", Range(0.0, 1.0)) = 0.3
		[Toggle(_PARALLAXMAP)] _EnableDebug 		("    Enable Debug", Float) = 0
		_DebugColor									("    Debug Color", Color) = (1,0,0,1)
		[Enum(XYZ,0,XY,1)]
        _ScaleMode                          		("Scale Mode", Float) = 0

		[Header(Wind)]
		[Space(8)]
		_WindMultiplier 							("Strength Main (X) Jitter (Y)", Vector) = (1, 0.5, 0, 0)
		[Toggle(_METALLICGLOSSMAP)] _SamplePivot 	("Sample Wind at Pivot", Float) = 0
		_WindLOD 									("Wind LOD (int)", Float) = 0
	}

	SubShader {
		Tags { 
			"Queue" = "Geometry+200"
			"IgnoreProjector" = "True"
			"RenderType" = "ATGrassArray"
		//	In order to adjust the wind settings on single instances:
			"DisableBatching"="True"
		}
		LOD 200
		Cull Off
	
	//	Instead of using clip in the surface shader
		ZTest Equal

	//	Depth Only Prepass
		Pass {
			Name "DEFERRED"
			Tags { "LightMode" = "Deferred" "Queue" = "Geometry+199" }

            ColorMask 0
            Cull Off
            ZWrite On
            ZTest LEqual

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#pragma target 4.5
//	Shader supports only procedural instancing
//          #pragma multi_compile_instancing

            // Wind sample mode
			#pragma shader_feature _METALLICGLOSSMAP
			// Array Mix Mode
			#pragma shader_feature _MIXMODE_RANDOM _MIXMODE_BYSIZE _MIXMODE_SOFTMERGE
			
			// Instancing support
			void dummy() {}
			// dummy is needed by Metal
			#define UNITY_PROCEDURAL_INSTANCING_ENABLED
			#define UNITY_INSTANCING_PROCEDURAL_FUNC dummy
			#define UNITY_ASSUME_UNIFORM_SCALING
			#include "UnityCG.cginc"

			struct appdata_grassinstanced_depth
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				fixed4 color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float3 texcoord : TEXCOORD0;  // texcoord.z = texture layer
				float4 vertex : SV_POSITION;
			};

			StructuredBuffer<float4x4> GrassMatrixBuffer;

			float3 terrainNormal;
			float TextureLayer;
			float InstanceScale;

			float _Clip;
			float _WindLOD;
			float _NormalBend;
			half2 _MinMaxScales;
			fixed4 _HealthyColor;
			fixed4 _DryColor;

			half _Layers;

			half _ScaleMode;

			//	Generalized custom CBUFFER
			CBUFFER_START(AtgGrass)
				float4 _AtgWindDirSize;
				float4 _AtgWindStrengthMultipliers;
				float4 _AtgSinTime;
				float4 _AtgGrassFadeProps;
				float4 _AtgGrassShadowFadeProps;

				float3 _AtgSurfaceCameraPosition;
			CBUFFER_END
			sampler2D _AtgWindRT;
			float4 _AtgTerrainShiftSurface;

			#define GRASSUSESTEXTUREARRAYS
			#define DEPTHONLY

			#include "Includes/GrassInstancedIndirect_Vertex.cginc"

            //v2f vert (appdata_full v, uint instanceID : SV_InstanceID)
            v2f vert (appdata_grassinstanced_depth v) {
                
            //	UNITY_SETUP_INSTANCE_ID(v)
            //	We do not use the standard macro here
            	UnitySetupInstanceID(UNITY_GET_INSTANCE_ID(v));
                v2f o;
                
                float4x4 data = GrassMatrixBuffer[unity_InstanceID]; //[instanceID];
				unity_ObjectToWorld = data;

			//	Handle Floating Origin
				float3 shift = _AtgTerrainShiftSurface.xyz * _AtgTerrainShiftSurface.w; // w = 0 when compute / 1 when no compute
				unity_ObjectToWorld[0].w -= shift.x;
				unity_ObjectToWorld[1].w -= shift.y;
				unity_ObjectToWorld[2].w -= shift.z;

			//	Restore matrix as it could contain layer data here!
				InstanceScale = frac(unity_ObjectToWorld[3].w);
				TextureLayer = unity_ObjectToWorld[3].w - InstanceScale;
				InstanceScale *= 100.0f;
				#if defined(_NORMAL)
					terrainNormal = unity_ObjectToWorld[3].xyz;
				#endif
				unity_ObjectToWorld[3] = float4(0, 0, 0, 1.0f);

			// 	Not correct but good enough to get the wind direction in objectspace
				unity_WorldToObject = unity_ObjectToWorld;
				unity_WorldToObject._14_24_34 = 1.0f / unity_WorldToObject._14_24_34;
				unity_WorldToObject._11_22_33 *= -1;

			//	Set o.texcoord before calling vertgrass() as the texture layer is stored in v.texcoord.x
				o.texcoord.xy = v.texcoord.xy;
				vertgrass(v, terrainNormal, InstanceScale);

				#if defined(_MIXMODE_SOFTMERGE)
					o.texcoord.z = TextureLayer;
				#else
					o.texcoord.z = v.texcoord.x;
				#endif

                o.vertex = UnityObjectToClipPos(v.vertex);
                
                return o;
            }

            UNITY_DECLARE_TEX2DARRAY(_MainTexArray);
			fixed _Cutoff;
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 c = UNITY_SAMPLE_TEX2DARRAY(_MainTexArray, float3(i.texcoord.xy, i.texcoord.z ));
				clip(c.a - _Cutoff);
				return c;
			}
			ENDCG

		}


	//	Shadow Caster Pass
		Pass {
			Tags {"LightMode"="ShadowCaster"}

            Cull Off
            ZWrite On
            ZTest LEqual

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_shadowcaster

			#pragma target 4.5
//	Shader supports only procedural instancing
//            #pragma multi_compile_instancing

            // Wind sample mode
			#pragma shader_feature _METALLICGLOSSMAP
			// Array Mix Mode
			#pragma shader_feature _MIXMODE_RANDOM _MIXMODE_BYSIZE _MIXMODE_SOFTMERGE
			
			// Instancing support
			void dummy() {}
			// dummy is needed by Metal
			#define UNITY_PROCEDURAL_INSTANCING_ENABLED
			#define UNITY_INSTANCING_PROCEDURAL_FUNC dummy
			#define UNITY_ASSUME_UNIFORM_SCALING
			#include "UnityCG.cginc"

			struct appdata_grassinstanced_depth
			{
				float4 vertex : POSITION;
				//float3 normal : NORMAL;
				float2 texcoord : TEXCOORD0;
				fixed4 color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				V2F_SHADOW_CASTER;
				float3 texcoord : TEXCOORD1;  // texcoord.z = texture layer
			};

			StructuredBuffer<float4x4> GrassMatrixBuffer;

			float3 terrainNormal;
			float TextureLayer;
			float InstanceScale;

			float _Clip;
			float _WindLOD;
			float _NormalBend;
			half2 _MinMaxScales;
			fixed4 _HealthyColor;
			fixed4 _DryColor;

			half _Layers;

			half _ScaleMode;

			//	Generalized custom CBUFFER
			CBUFFER_START(AtgGrass)
				float4 _AtgWindDirSize;
				float4 _AtgWindStrengthMultipliers;
				float4 _AtgSinTime;
				float4 _AtgGrassFadeProps;
				float4 _AtgGrassShadowFadeProps;

				float3 _AtgSurfaceCameraPosition;
			CBUFFER_END
			sampler2D _AtgWindRT;
			float4 _AtgTerrainShiftSurface;

			#define GRASSUSESTEXTUREARRAYS
			#define DEPTHONLY
			#ifndef UNITY_PASS_SHADOWCASTER
				#define UNITY_PASS_SHADOWCASTER
			#endif

			#include "Includes/GrassInstancedIndirect_Vertex.cginc"

            //v2f vert (appdata_full v, uint instanceID : SV_InstanceID)
            v2f vert (appdata_grassinstanced_depth v) {
                
            //	UNITY_SETUP_INSTANCE_ID(v)
            //	We do not use the standard macro here
            	UnitySetupInstanceID(UNITY_GET_INSTANCE_ID(v));
                v2f o;
                
                float4x4 data = GrassMatrixBuffer[unity_InstanceID]; //[instanceID];
				unity_ObjectToWorld = data;

			//	Handle Floating Origin
				float3 shift = _AtgTerrainShiftSurface.xyz * _AtgTerrainShiftSurface.w; // w = 0 when compute / 1 when no compute
				unity_ObjectToWorld[0].w -= shift.x;
				unity_ObjectToWorld[1].w -= shift.y;
				unity_ObjectToWorld[2].w -= shift.z;

			//	Restore matrix as it could contain layer data here!
				InstanceScale = frac(unity_ObjectToWorld[3].w);
				TextureLayer = unity_ObjectToWorld[3].w - InstanceScale;
				InstanceScale *= 100.0f;
				#if defined(_NORMAL)
					terrainNormal = unity_ObjectToWorld[3].xyz;
				#endif
				unity_ObjectToWorld[3] = float4(0, 0, 0, 1.0f);

			// 	Not correct but good enough to get the wind direction in objectspace
				unity_WorldToObject = unity_ObjectToWorld;
				unity_WorldToObject._14_24_34 = 1.0f / unity_WorldToObject._14_24_34;
				unity_WorldToObject._11_22_33 *= -1;

			//	Set o.texcoord before calling vertgrass() as the texture layer is stored in v.texcoord.x
				o.texcoord.xy = v.texcoord.xy;
				vertgrass(v, terrainNormal, InstanceScale);

				#if defined(_MIXMODE_SOFTMERGE)
				o.texcoord.z = TextureLayer;
				#else
                o.texcoord.z = v.texcoord.x;
				#endif
                //TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                TRANSFER_SHADOW_CASTER(o)
                
                return o;
            }

            UNITY_DECLARE_TEX2DARRAY(_MainTexArray);
			fixed _Cutoff;
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 c = UNITY_SAMPLE_TEX2DARRAY(_MainTexArray, float3(i.texcoord.xy, i.texcoord.z ));
				clip(c.a - _Cutoff);
				SHADOW_CASTER_FRAGMENT(i)
			}
			ENDCG

		}


		CGPROGRAM
// noshadowmask does not fix the problem with baked shadows in deferred
// removing nolightmap does	
		#pragma surface surf ATGSpecular vertex:vertgrass nodynlightmap nolppv nometa
// nolightmap
		#pragma target 3.5

//	Shader supports only procedural instancing
//		#pragma multi_compile_instancing
		
		// Specular Highlights
		#pragma shader_feature _SPECULARHIGHLIGHTS_OFF
		// Debug
		#pragma shader_feature _PARALLAXMAP
		// Wind sample mode
		#pragma shader_feature _METALLICGLOSSMAP
		// Array Mix Mode
		#pragma shader_feature _MIXMODE_RANDOM _MIXMODE_BYSIZE _MIXMODE_SOFTMERGE
		// NormalBuffer
		#pragma shader_feature _NORMAL

	//	assumeuniformscaling --> so we do not need the proper WorldToObject matrix for the normals
		#pragma instancing_options assumeuniformscaling procedural:setup
		#define ISGRASS
		#include "Includes/AtgPBSLighting.cginc"

	//	Inputs for vertex shader	
		float _Clip;
		float _WindLOD;
		#if defined(_MIXMODE_BYSIZE)
			float _SizeThreshold;
		#endif
		#if defined(_PARALLAXMAP)
			fixed4 _DebugColor;
		#endif
		half2 _MinMaxScales;
		fixed4 _HealthyColor;
		fixed4 _DryColor;
float3 terrainNormal;
float TextureLayer;
float InstanceScale;
		half _NormalBend;
		half _Layers;

		half _ScaleMode;

	//	Include all general inputs and vertex functions
		#define GRASSUSESTEXTUREARRAYS
		#include "Includes/GrassInstancedIndirect_Inputs.cginc"
		#include "Includes/GrassInstancedIndirect_Vertex.cginc"

	//	Inputs for the pixelshader
		UNITY_DECLARE_TEX2DARRAY(_MainTexArray);
		float4 _MainTexArray_TexelSize;
		UNITY_DECLARE_TEX2DARRAY(_SpecTexArray);
		half _TransPower;
		half _TransStrength;
		fixed4 _Color;
		half _Glossiness;
		fixed _Cutoff;

		void surf (Input IN, inout SurfaceOutputATGSpecular o) {
			#if defined(_MIXMODE_SOFTMERGE)
				fixed4 c = UNITY_SAMPLE_TEX2DARRAY(_MainTexArray, float3(IN.uv_MainTexArray, TextureLayer ));
			#else
				fixed4 c = UNITY_SAMPLE_TEX2DARRAY(_MainTexArray, float3(IN.uv_MainTexArray, IN.layer ));
			#endif

			// NOTE: no clip here!

			#if defined(_SPECULARHIGHLIGHTS_OFF)
				#if defined(_MIXMODE_SOFTMERGE)
					half3 rest = UNITY_SAMPLE_TEX2DARRAY(_SpecTexArray, float3(IN.uv_MainTexArray, TextureLayer ));
				#else
					half3 rest = UNITY_SAMPLE_TEX2DARRAY(_SpecTexArray, float3(IN.uv_MainTexArray, IN.layer ));
				#endif
			#endif
			o.Albedo = c.rgb * IN.color;
			o.Alpha = c.a;
			o.Occlusion = IN.occ;
			//o.Specular = 0;
			#if defined(_SPECULARHIGHLIGHTS_OFF)
				o.Smoothness = rest.b * IN.scale;
				o.Translucency = _TransStrength * rest.r * IN.scale;
			//	Grass does not have any specPower (fixed value) but we write out the spec mask here
				o.TranslucencyPower = rest.g;
			//	Enable grass spec lighting
				o.Specular = half3(1,0,0);
			#else
				o.Smoothness = 0;
				o.Translucency = _TransStrength * o.Albedo.g;
			//	Enable grass trans lighting
				o.Specular = half3(1, 0, 0);
			#endif
		}
		ENDCG
	}
	Fallback "AdvancedTerrainGrass/Grass Array Shader"
}