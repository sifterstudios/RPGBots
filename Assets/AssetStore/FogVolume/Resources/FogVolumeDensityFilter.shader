// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/FogVolumeDensityFilter" {

	Properties{ _MainTex("", any) = "" {} }
		CGINCLUDE
#include "UnityCG.cginc"
		struct v2f {
		float4 pos : SV_POSITION;
		half2 uv : TEXCOORD0;
		half2 taps[4] : TEXCOORD1;
	};
	sampler2D _MainTex;
	half4 _MainTex_TexelSize;
	half4 _BlurOffsets;
	half FOV_compensation;
	//half _Distortion = -0.01;
	sampler2D RT_FogVolumeConvolution, RT_FogVolume, _source;
	v2f vert(appdata_img v) {
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		
		o.uv = v.texcoord - _BlurOffsets.xy * _MainTex_TexelSize.xy; // hack, see BlurEffect.cs for the reason for this. let's make a new blur effect soon
		o.taps[0] = o.uv + _MainTex_TexelSize * _BlurOffsets.xy*FOV_compensation;
		o.taps[1] = o.uv - _MainTex_TexelSize * _BlurOffsets.xy*FOV_compensation;
		o.taps[2] = o.uv + _MainTex_TexelSize * _BlurOffsets.xy * half2(1, -1)*FOV_compensation;
		o.taps[3] = o.uv - _MainTex_TexelSize * _BlurOffsets.xy * half2(1, -1)*FOV_compensation;
		return o;
	}
	half4 frag(v2f i) : SV_Target{
		//distortion experiment
		/*half4 FV = tex2D(RT_FogVolumeConvolution, i.uv);
		half4 TexturedFogVolumes = tex2D(RT_FogVolume, i.uv);
		half Distortion = FV.a * _Distortion * TexturedFogVolumes.a + 1;

		half4 color = tex2D(_MainTex, (i.taps[0]-0.5)* Distortion + 0.5);
		color += tex2D(_MainTex, (i.taps[1] - 0.5)* Distortion + 0.5);
		color += tex2D(_MainTex, (i.taps[2] - 0.5)* Distortion + 0.5);
		color += tex2D(_MainTex, (i.taps[3] - 0.5)* Distortion + 0.5);*/

		half4 color = tex2D(_MainTex, i.taps[0]);
		color += tex2D(_MainTex, i.taps[1]);
		color += tex2D(_MainTex, i.taps[2]);
		color += tex2D(_MainTex, i.taps[3]);

		return color *.25;
		
	}
		ENDCG
		SubShader {
		Pass{
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vert
#pragma fragment frag
			ENDCG
		}
	}
	Fallback off
}
