// Upgrade NOTE: removed variant '__' where variant LOD_FADE_PERCENTAGE is used.

Shader "Hidden/Fog Volume/Depth" 
{
	Properties
	{
		_MainTex("Main Texture", 2D) = "white" {}
		_Cutoff("Alpha cutoff", Range(0,1)) = 0.5
		_Color("Color", Color) = (1,1,1,1)
		[MaterialEnum(Off,0,Front,1,Back,2)] _Cull("Cull", Int) = 2
		[MaterialEnum(None,0,Fastest,1,Fast,2,Better,3,Best,4,Palm,5)] _WindQuality("Wind Quality", Range(0,5)) = 0
	}

	CGINCLUDE
	#pragma target 3.0
	#include "UnityCG.cginc"
	sampler2D _CameraDepthTexture;
	uniform float _Cutoff;
	uniform sampler2D _MainTex;
	float4 _Color;
	int _Cull;
	struct v2f
	{
		float4 vertex : SV_POSITION;
		float2 uv : TEXCOORD0;
		half depth:TEXCOORD1;
		UNITY_VERTEX_OUTPUT_STEREO
	};

	v2f vert(appdata_full v)
	{
		v2f o;
		UNITY_SETUP_INSTANCE_ID(v);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = v.texcoord.xy;
		//o.depth = COMPUTE_DEPTH_01;
		//3.2.1
		o.depth = -(UnityObjectToViewPos(v.vertex).z)/** _ProjectionParams.w*/;
		return o;
	}

	half4 fragOpaque(v2f i) : COLOR
	{
		float d =1.0/(i.depth);
		//float d = Linear01Depth(i.depth);
		//float d= 1.0 / (_ZBufferParams.x * i.depth + _ZBufferParams.y);
		//#if defined(UNITY_REVERSED_Z)
		//d = 1.0f - d;
		//#endif
		return d;	
	}

	half4 fragTransparentCutout(v2f i) : COLOR
	{
		half4 c = tex2D(_MainTex, i.uv)*_Color;
		clip(c.a - _Cutoff);
		float d = 1.0/(i.depth);
//		#if defined(UNITY_REVERSED_Z)
//			i.depth = 1.0f - i.depth;
//		#endif
		return (d);
	}
	ENDCG

	SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		Pass
		{
			Fog{ Mode Off }
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragOpaque
			ENDCG
		}
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout" }
		Pass
		{
			Fog{ Mode Off }
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragTransparentCutout
			ENDCG
		}
	}


	SubShader
	{
		Tags{ "RenderType" = "TreeLeaf" }
		Pass
		{
			Fog{ Mode Off }
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragTransparentCutout
			ENDCG
		}
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Geometry"
			"IgnoreProjector" = "True"
			"RenderType" = "SpeedTree"
			"DisableBatching" = "LODFading"
		}
		//Pass{
			Cull[_Cull]
			Fog{ Mode Off }
			CGPROGRAM

			#pragma surface surf Lambert vertex:SpeedTreeVert nolightmap
			#pragma target 3.0
			#pragma multi_compile  LOD_FADE_PERCENTAGE LOD_FADE_CROSSFADE
			#pragma shader_feature GEOM_TYPE_BRANCH GEOM_TYPE_BRANCH_DETAIL GEOM_TYPE_FROND GEOM_TYPE_LEAF GEOM_TYPE_MESH
			#define ENABLE_WIND
			#define SPEEDTREE_ALPHATEST
			#include "SpeedTreeCommonDepth.cginc"

			void surf(Input IN, inout SurfaceOutput OUT)
			{
				float d= 1.0 / (IN.depth);
	//			/*#if defined(UNITY_REVERSED_Z)
	//			d = 1.0f - d;
	//			#endif*/
				OUT.Emission = d;
				SpeedTreeFragOut o;
				SpeedTreeFrag(IN, o);
				SPEEDTREE_COPY_FRAG(OUT, o)
			}
			ENDCG
		//}
	}
}
