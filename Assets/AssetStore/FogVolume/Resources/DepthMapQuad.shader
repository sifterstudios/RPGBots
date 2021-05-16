Shader "Hidden/DepthMapQuad"
{
	SubShader
	{
		Tags { "Queue" = "Transparent" "IgnoreProjector" = "True"  "RenderType" = "buuuh" }
		LOD 100
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag		

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;

			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{

				clip(-.1);
				return 1;
			}
				ENDCG
	}
	}
		SubShader
			{
				Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True"  "RenderType" = "Opaque" }
				LOD 1
				UsePass "Legacy Shaders/VertexLit/SHADOWCASTER" 
			}
	
}
