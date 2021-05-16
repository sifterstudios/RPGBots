// Upgrade NOTE: removed variant '__' where variant LOD_FADE_PERCENTAGE is used.

Shader "Hidden/FogVolumeDirectionalLight"
{
	Properties
	{
		_Cutoff("Alpha cutoff", Range(0,1)) = 0.5
		_MainTex("Base (RGB)", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		//_FogVolumeShadowMapEdgeSoftness("Edge size", Range(1, 100))=100
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque" }

		Pass
		{
			CGPROGRAM
			#define EDGE_FIX
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			uniform sampler2D 	_MainTex;
					float4 		_ShadowCameraPosition;
			uniform float 		_Cutoff = 1, _FogVolumeShadowMapEdgeSoftness;
			
			struct v2f
			{
				float4 	pos 		: SV_POSITION;
				float 	depth 		: DEPTH;
				float2 	uv 			: TEXCOORD0;
				float4 	screenUV 	: TEXCOORD1;
			};

			v2f vert(appdata_base v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.depth = length(mul(unity_ObjectToWorld, v.vertex).xyz - _ShadowCameraPosition.xyz);
				o.uv = v.texcoord.xy;
				o.screenUV = ComputeScreenPos(o.pos);
				return o;
			}
			
			float4 frag(v2f i) : COLOR
			{
				float2 uv = i.screenUV.xy / i.screenUV.w;
				float d = i.depth;
	#ifdef EDGE_FIX
				half edges = saturate(dot(1-uv.r, 1-uv.g) * dot(uv.r, uv.g)*600);
				d = lerp(10000, d, edges);		
	#endif
				half fade = saturate(dot(1 - uv.r, 1 - uv.g) * dot(uv.r, uv.g)*_FogVolumeShadowMapEdgeSoftness);
				fade *= fade*fade;
				return float4(d, fade, 0.0f, 1);
			}
			ENDCG
		}
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout" }

		Pass
		{
			CGPROGRAM
				#define EDGE_FIX
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"

			uniform sampler2D 	_MainTex;
					float4 		_ShadowCameraPosition;
			uniform float 		_Cutoff = 1, _FogVolumeShadowMapEdgeSoftness;
					float4 		_Color;

			struct v2f
			{
				float4 pos 		: SV_POSITION;
				float depth 	: DEPTH;
				float2 uv 		: TEXCOORD0;
				float4 screenUV : TEXCOORD1;
			};

			v2f vert(appdata_base v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.depth = length(mul(unity_ObjectToWorld, v.vertex).xyz - _ShadowCameraPosition.xyz);
				o.uv = v.texcoord.xy;
				o.screenUV = ComputeScreenPos(o.pos);
				return o;
			}

			float4 frag(v2f i) : COLOR
			{
				half4 c = tex2D(_MainTex, i.uv)*_Color;
				float2 uv = i.screenUV.xy / i.screenUV.w;
				clip(c.a - _Cutoff);
				float d = i.depth;
				#ifdef EDGE_FIX

				half edges = saturate(dot(1 - uv.r, 1 - uv.g) * dot(uv.r, uv.g) * 600);

				d = lerp(10000, d, edges);
				#endif

				half fade = saturate(dot(1 - uv.r, 1 - uv.g) * dot(uv.r, uv.g)*_FogVolumeShadowMapEdgeSoftness);
				fade *= fade*fade;
				return float4(d, fade, 0.0f, 1);
			}
			ENDCG
		}
	}

	SubShader
	{
		Tags
		{
			"Queue" 			= "Geometry"
			"IgnoreProjector" 	= "True"
			"RenderType" 		= "SpeedTree"
			"DisableBatching" 	= "LODFading"
		}

		Cull[_Cull]
		Fog{ Mode Off }

		CGPROGRAM
		uniform sampler2D _MainTex;
				float4 /*_ShadowCameraPosition,*/ _Color;
		uniform float _Cutoff = 1, _FogVolumeShadowMapEdgeSoftness;
				#pragma surface surf Lambert vertex:SpeedTreeVert nolightmap
				#pragma target 3.0
				#pragma multi_compile  LOD_FADE_PERCENTAGE LOD_FADE_CROSSFADE
				#pragma shader_feature GEOM_TYPE_BRANCH GEOM_TYPE_BRANCH_DETAIL GEOM_TYPE_FROND GEOM_TYPE_LEAF GEOM_TYPE_MESH
				#define ENABLE_WIND
				#define SPEEDTREE_ALPHATEST
				#define EDGE_FIX
				#include "SpeedTreeCommonDepth.cginc"

		void surf(Input i, inout SurfaceOutput OUT)
		{
			float2 uv = i.screenUV.xy / i.screenUV.w;
			float d = i.ShadowDepth;
			#ifdef EDGE_FIX
				half edges = saturate(dot(1 - uv.r, 1 - uv.g) * dot(uv.r, uv.g) * 600);
				d = lerp(10000, d, edges);
			#endif
			half fade = saturate(dot(1 - uv.r, 1 - uv.g) * dot(uv.r, uv.g)*_FogVolumeShadowMapEdgeSoftness);
			fade *= fade*fade;

			OUT.Emission = float3(d, fade, 0.0f);

			SpeedTreeFragOut o;
			SpeedTreeFrag(i, o);
			SPEEDTREE_COPY_FRAG(OUT, o)
		}
		ENDCG
	}
}