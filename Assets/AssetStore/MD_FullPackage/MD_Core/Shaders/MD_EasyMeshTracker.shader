Shader "Matej Vanco/Mesh Deformation Package/MD_EasyMeshTracker" {

	Properties
	{
		[Space]
		[Header(_Use SurfaceTracking modifier for advanced settings_)]
		[Space]
		_ColorUp("Upper Color", Color) = (1,1,1,1)
	    _ColorDown("Lower Color", Color) = (1,1,1,1)
		[Space]
		_MainTex("Albedo (RGB) Texture", 2D) = "white" {}
		_MainNormal("Normal Texture", 2D) = "bump" {}
		_NormalAmount("Normal Power", Range(0.01,2)) = 0.5
		_Specular("Specular", Range(-1,1)) = 0.5
		_Emissive("Emission Intensity", Range(0,5)) = 0

	    [Header(Track Settings)]
		[Space]
		_TrackFactor("Track Depth", float) = 0.1

	    [Header(Tessalation Settings)]
		[Space]
		_Tess("Tessellation", Range(1,32)) = 4
	    _DispTex("Displacement Track", 2D) = "gray" {}
		[Space]
		_MinDistance("Min Distance", float) = 20
		_MaxDistance("Max Distance", float) = 50
	}

	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Standard addshadow fullforwardshadows vertex:vert tessellate:tessDistance
		#pragma target 5.0
        #include "Tessellation.cginc"

		sampler2D _MainTex;
		sampler2D _MainNormal;
		half4 _ColorUp;
		half4 _ColorDown;
		float _Specular;
		float _NormalAmount;
		float _Emissive;

		float4 _LocalPos;

		float _TrackFactor;

		float _MinDistance;
		float _MaxDistance;

		struct appdata 
		{
			float4 vertex : POSITION;
			float4 tangent : TANGENT;
			float3 normal : NORMAL;
			float2 texcoord : TEXCOORD0;
			float2 texcoord1 : TEXCOORD1;
			float2 texcoord2 : TEXCOORD2;
		};

		struct v2f
		{
			float4 vertex : POSITION;
			float2 texcoord : TEXCOORD0;
		};

		float _Tess;
		float4 tessDistance(appdata v0, appdata v1, appdata v2) 
		{
			float minDist = _MinDistance;
			float maxDist = _MaxDistance;
			return UnityDistanceBasedTess(v0.vertex, v1.vertex, v2.vertex, minDist, maxDist, _Tess);
		}

		sampler2D _DispTex;
		float _Displacement;

		void vert(inout appdata v)
		{
			float d = tex2Dlod(_DispTex, float4(v.texcoord.xy, 0, 0)).r * _TrackFactor;
			v.vertex.y += d;
		}

		struct Input
		{
			float2 uv_MainTex;
			float2 uv_DispTex;
		};

		void surf (Input IN, inout SurfaceOutputStandard o) 
		{
			float val = tex2Dlod(_DispTex, float4(IN.uv_DispTex, 0, 0)).r;

			float3 c = lerp(tex2D(_MainTex, IN.uv_MainTex) * _ColorUp.rgb, tex2D(_MainTex, IN.uv_MainTex) * _ColorDown.rgb,val);
			fixed3 n = UnpackNormal(tex2D(_MainNormal, IN.uv_MainTex));
			n.z = n.z / _NormalAmount;
			o.Albedo = c.rgb;
			o.Normal = normalize(n);
			o.Emission = c.rgb * _Emissive;
			o.Smoothness = _Specular;
		}
		ENDCG
	}
	FallBack "Diffuse"
}



