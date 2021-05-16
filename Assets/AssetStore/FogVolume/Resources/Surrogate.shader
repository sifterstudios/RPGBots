// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//Shader "Hidden/LowResReplace"
Shader "Hidden/Fog Volume/Surrogate"
{
	SubShader{
		Tags{
		"Queue" = "Transparent" "IgnoreProjector" = "True"
		//	"RenderType" = "Opaque" 
		}

		LOD 100
		//Blend One OneMinusSrcAlpha
		//	Blend SrcAlpha OneMinusSrcAlpha
		Blend[_SrcBlend] OneMinusSrcAlpha //One OneMinusSrcAlpha works best with noise
		Fog{ Mode Off }
		Cull Front
		Lighting Off
		ZWrite Off
		ZTest [_ztest]
		//ZTest LEqual
		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#pragma shader_feature _EDITOR_WINDOW
			#define red float4(1, 0,0,1)
			#define green float4(0, 1,0,1)
			int _SrcBlend, _ztest;
			//UNITY_DECLARE_SCREENSPACE_TEXTURE(RT_FogVolume);
			//UNITY_DECLARE_SCREENSPACE_TEXTURE(RT_FogVolumeR);
			sampler2D RT_FogVolume;
			sampler2D RT_FogVolumeR;
	
			struct appdata_t {
				float4 vertex : POSITION;
				float2 texcoord: TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID //Insert
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				float4 projPos : TEXCOORD0;
				float2 uv : TEXCOORD1;
				UNITY_VERTEX_OUTPUT_STEREO //Insert
			};

			v2f vert(appdata_t v)
			{

				
				#if UNITY_UV_STARTS_AT_TOP
				float scale = -1.0;
				#else
				float scale = 1.0;
				#endif

				v2f o;
				UNITY_SETUP_INSTANCE_ID(v); //Insert
				UNITY_INITIALIZE_OUTPUT(v2f, o); //Insert
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); //Insert
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.projPos = ComputeScreenPos(o.vertex);
				o.uv = v.texcoord;
				return o;
			}

			half4 frag(v2f i) : SV_Target
			{
						 
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				float4 coords = 0, FV;
				coords = UNITY_PROJ_COORD(i.projPos);
				float2 screenUV = coords.xy / coords.w;
			#if UNITY_SINGLE_PASS_STEREO
				float4 scaleOffset = unity_StereoScaleOffset[unity_StereoEyeIndex];
				screenUV = (screenUV - scaleOffset.zw) / scaleOffset.xy;
				
			#endif
				if (unity_StereoEyeIndex == 0)
				{
					FV = tex2D(RT_FogVolume, screenUV)/**red*/;
					
				}
				else
				{
					FV = tex2D(RT_FogVolumeR, screenUV)/**green*/;
				}
				
	#ifdef _EDITOR_WINDOW
			FV = 0;
		//	FV= float4(1, 0, 0, 1)*FV;
	#endif
	//return UNITY_SAMPLE_SCREENSPACE_TEXTURE(RT_FogVolume, i.uv)/**red*/;
			return FV;
			return float4(1, 0, 0, 1)*FV;
			}
		ENDCG
		}
	}
}