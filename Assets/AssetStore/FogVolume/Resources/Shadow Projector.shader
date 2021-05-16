// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Fog Volume/Shadow Projector"
{
	
	Properties
	{
		[NoScaleOffset]_MainTex("ShadowMap", 2D) = "white" {}
		displacePower("light Displace intensity", range(.01,.3)) = 0.079
			_ShadowColor("ShadowColor", Color)=(0,0,0,0)
			_Cutoff("Alpha cutoff", Range(0,1)) = 0.01//TODO: Publish
	}
		SubShader
	{ Tags{ 
			"Queue" = "AlphaTest" 
		
			"IgnoreProjector" = "True" 
			"RenderType" = "NeverRenderMe"
		}
		Pass
	{
		Fog{ Mode Off } 
	
		Blend DstColor OneMinusSrcAlpha
		Cull front
		ZWrite Off
		ZTest Always
		Lighting Off
		CGPROGRAM
		#pragma target 3.0
		#pragma vertex vert
		#pragma fragment frag


#include "UnityCG.cginc"

		struct v2f
	{
		float4 pos : SV_POSITION;
		half2 uv : TEXCOORD0;
		float4 screenUV : TEXCOORD1;
		float3 ray : TEXCOORD2;
		half3 orientation : TEXCOORD3;
	};

	v2f vert(float3 v : POSITION)
	{
		v2f o;
		o.pos = UnityObjectToClipPos(float4(v,1));
		o.uv = v.xz + 0.5;
		o.screenUV = ComputeScreenPos(o.pos);
		//o.ray = mul(UNITY_MATRIX_MV, float4(v,1)).xyz * float3(-1,-1,1);
		//5.6:
		o.ray = UnityObjectToViewPos(float4(v, 1) ).xyz* float3(-1, -1, 1);
		o.orientation = mul((float3x3)unity_ObjectToWorld, float3(0,1,0));
		return o;
	}
	
	uniform float3 L;
	sampler2D _MainTex, RT_OpacityBlur;
	//sampler2D_float _CameraDepthTexture;
	sampler2D _CameraDepthTexture;
	half displacePower;
	half _Cutoff;
	half4 _ShadowColor;
	fixed4 frag(v2f i) : COLOR0
	{
		i.ray = i.ray * (_ProjectionParams.z / i.ray.z);
		float2 uv = i.screenUV.xy / i.screenUV.w;
		// read depth and reconstruct world position
		//float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv);
		float depth = tex2D(_CameraDepthTexture, uv);
		depth = Linear01Depth(depth);
		float4 vpos = float4(i.ray * depth,1);
		float3 wpos = mul(unity_CameraToWorld, vpos).xyz;
		float3 opos = mul(unity_WorldToObject, float4(wpos,1)).xyz;
		clip(float3(0.5,0.5,0.5) - abs(opos.xyz));
		i.uv = opos.xz + 0.5;	
		i.uv = i.uv + displacePower*L.xz;
		fixed4 col = tex2D(RT_OpacityBlur, i.uv).r;
		clip(col.a - _Cutoff);
		col.rgb = _ShadowColor*( col.a);
		//return float4(0,0,0,1);
		return float4(col.rgb, col.a);
	}
		ENDCG
	}

	}

		Fallback Off
}
