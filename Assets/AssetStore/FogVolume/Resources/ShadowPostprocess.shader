// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/Fog Volume/Shadow Postprocess" 
{
	Properties { _MainTex ("", any) = "" {} }
	CGINCLUDE
	#include "UnityCG.cginc"


	struct v2f {
		float4 pos : SV_POSITION;
		half2 uv : TEXCOORD0;
	
	};
	sampler2D _MainTex;
	half4 _MainTex_TexelSize;
	half3 ContrastF(half3 pixelColor, fixed contrast)
	{
		return saturate(((pixelColor.rgb - 0.5f) * max(contrast, 0)) + 0.5f);
	}

	half ShadowColor;
	v2f vert( appdata_img v ) {
		v2f o; 
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv = v.texcoord ; 
	
		return o;
	}
	half4 frag(v2f i) : SV_Target {
		half4 color = tex2D(_MainTex, i.uv);
		//color.a *= 0;
		//color.a = ContrastF(color.a, ShadowColor+1);
		//color.a = ContrastF(color.a ,ShadowColor*5);
		half edges = saturate(dot(1 - i.uv.r, 1 - i.uv.g) * dot(i.uv.r, i.uv.g) * 600);
		edges *= edges*edges;
		color.r = color.r + color.r*ShadowColor*50* edges;
		color.r = lerp(0, color.r,edges);
		return min(color.r,1);
	}
	ENDCG
	SubShader {
		 Pass {
			  ZTest Always Cull Off ZWrite Off

			  CGPROGRAM
			  #pragma vertex vert
			  #pragma fragment frag
			  ENDCG
		  }
	}
	Fallback off
}
