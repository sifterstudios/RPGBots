Shader "Fog Volume/RT viewers/RT_Opacity"
{
	Properties{
		_MainTex("Base", 2D) = "" {}
	
	RT_Opacity("RT_Opacity", 2D) = "" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
		
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D RT_Opacity;
			float4 RT_Opacity_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, RT_Opacity);
			
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(RT_Opacity, i.uv);
			
			
				//return col;
				return 1-col.r;
			}
			ENDCG
		}
	}
}
