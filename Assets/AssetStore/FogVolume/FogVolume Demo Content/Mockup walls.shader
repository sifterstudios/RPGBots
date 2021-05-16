// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge v1.33 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.33;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:1,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:False,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:3138,x:33205,y:32671,varname:node_3138,prsc:2|emission-7241-RGB,alpha-4411-OUT;n:type:ShaderForge.SFN_Color,id:7241,x:32218,y:32626,ptovrint:False,ptlb:Color,ptin:_Color,varname:_Color,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:1,c3:1,c4:1;n:type:ShaderForge.SFN_DepthBlend,id:9093,x:32065,y:32824,varname:node_9093,prsc:2|DIST-2293-OUT;n:type:ShaderForge.SFN_TexCoord,id:6495,x:31623,y:32903,varname:node_6495,prsc:2,uv:0;n:type:ShaderForge.SFN_Multiply,id:4411,x:32314,y:32927,varname:node_4411,prsc:2|A-9093-OUT,B-7352-OUT;n:type:ShaderForge.SFN_Multiply,id:7594,x:31853,y:32963,varname:node_7594,prsc:2|A-6495-V,B-6495-V;n:type:ShaderForge.SFN_Vector1,id:2293,x:31821,y:32807,varname:node_2293,prsc:2,v1:0.5;n:type:ShaderForge.SFN_OneMinus,id:7352,x:32065,y:32963,varname:node_7352,prsc:2|IN-6495-V;n:type:ShaderForge.SFN_Exp,id:8329,x:32740,y:32929,varname:node_8329,prsc:2,et:0|IN-729-OUT;n:type:ShaderForge.SFN_OneMinus,id:7457,x:32988,y:32915,varname:node_7457,prsc:2|IN-8329-OUT;n:type:ShaderForge.SFN_Negate,id:729,x:32462,y:32954,varname:node_729,prsc:2|IN-4411-OUT;proporder:7241;pass:END;sub:END;*/

Shader "Fog Volume/Mockup walls" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Front
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            //#define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal d3d11_9x 
            #pragma target 3.0
            uniform sampler2D _CameraDepthTexture;
            uniform float4 _Color;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 projPos : TEXCOORD1;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.pos = UnityObjectToClipPos(v.vertex );
                o.projPos = ComputeScreenPos (o.pos);
                COMPUTE_EYEDEPTH(o.projPos.z);
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                float sceneZ = max(0,LinearEyeDepth (UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)))) - _ProjectionParams.g);
                float partZ = max(0,i.projPos.z - _ProjectionParams.g);
////// Lighting:
////// Emissive:
                float3 emissive = _Color.rgb;
                float3 finalColor = emissive;
                float node_4411 = (saturate((sceneZ-partZ)/0.5)*(1.0 - i.uv0.g));
                return fixed4(finalColor,node_4411);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
