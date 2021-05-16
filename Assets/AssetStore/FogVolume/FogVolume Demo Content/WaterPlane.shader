// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge v1.36 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.36;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:1,spmd:0,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:False,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.01027249,fgcg:0.2571382,fgcb:0.2794118,fgca:1,fgde:0.85,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:3138,x:39103,y:32740,varname:node_3138,prsc:2|diff-8234-OUT,spec-1036-OUT,gloss-4729-OUT,normal-1964-OUT,emission-6069-OUT,alpha-3237-OUT;n:type:ShaderForge.SFN_Color,id:7241,x:37623,y:33113,ptovrint:False,ptlb:Color,ptin:_Color,varname:_Color,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.07843138,c2:0.3921569,c3:0.7843137,c4:1;n:type:ShaderForge.SFN_Tex2d,id:2459,x:32090,y:33099,varname:_Layers,prsc:2,tex:fb6566c21f717904f83743a5a76dd0b0,ntxv:0,isnm:False|UVIN-6492-UVOUT,MIP-6073-OUT,TEX-4819-TEX;n:type:ShaderForge.SFN_FragmentPosition,id:6633,x:30687,y:32701,varname:node_6633,prsc:2;n:type:ShaderForge.SFN_ComponentMask,id:3244,x:30844,y:32834,varname:node_3244,prsc:2,cc1:0,cc2:2,cc3:-1,cc4:-1|IN-6633-XYZ;n:type:ShaderForge.SFN_Divide,id:8317,x:31258,y:32986,varname:node_8317,prsc:2|A-3244-OUT,B-1364-OUT;n:type:ShaderForge.SFN_Slider,id:1364,x:30537,y:33113,ptovrint:False,ptlb:Scale,ptin:_Scale,varname:_Scale,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:1,cur:1,max:200;n:type:ShaderForge.SFN_Multiply,id:1038,x:31835,y:33208,varname:node_1038,prsc:2|A-1090-UVOUT,B-4511-OUT;n:type:ShaderForge.SFN_Vector1,id:4511,x:31628,y:33301,varname:node_4511,prsc:2,v1:1.1;n:type:ShaderForge.SFN_Tex2dAsset,id:4819,x:31562,y:32755,ptovrint:False,ptlb:node_4819,ptin:_node_4819,varname:_node_4819,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:fb6566c21f717904f83743a5a76dd0b0,ntxv:3,isnm:True;n:type:ShaderForge.SFN_Tex2d,id:8684,x:32078,y:33283,varname:_node_2312,prsc:2,tex:fb6566c21f717904f83743a5a76dd0b0,ntxv:0,isnm:False|UVIN-1038-OUT,MIP-6073-OUT,TEX-4819-TEX;n:type:ShaderForge.SFN_Slider,id:3005,x:37774,y:32821,ptovrint:False,ptlb:Specular,ptin:_Specular,varname:_Specular,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.7948718,max:1;n:type:ShaderForge.SFN_Slider,id:6924,x:31805,y:34964,ptovrint:False,ptlb:Gloss,ptin:_Gloss,varname:_Gloss,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.8205128,max:1;n:type:ShaderForge.SFN_Tex2d,id:9650,x:30604,y:31814,ptovrint:False,ptlb:NormalMap blend mask,ptin:_NormalMapblendmask,varname:_NormalMapblendmask,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:e80c3c84ea861404d8a427db8b7abf04,ntxv:0,isnm:False|UVIN-7490-UVOUT;n:type:ShaderForge.SFN_Lerp,id:6208,x:32553,y:33126,varname:node_6208,prsc:2|A-2459-RGB,B-8684-RGB,T-1683-OUT;n:type:ShaderForge.SFN_TexCoord,id:7490,x:30354,y:31784,varname:node_7490,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Multiply,id:9333,x:31287,y:31584,varname:node_9333,prsc:2|A-3786-OUT,B-9650-R,C-9650-R;n:type:ShaderForge.SFN_Vector1,id:3786,x:31141,y:31771,varname:node_3786,prsc:2,v1:4;n:type:ShaderForge.SFN_Clamp01,id:1683,x:31498,y:31629,varname:node_1683,prsc:2|IN-9333-OUT;n:type:ShaderForge.SFN_Multiply,id:9374,x:31261,y:31891,varname:node_9374,prsc:2|A-929-OUT,B-9650-G,C-9650-G;n:type:ShaderForge.SFN_Vector1,id:929,x:31115,y:32077,varname:node_929,prsc:2,v1:4;n:type:ShaderForge.SFN_Clamp01,id:8889,x:31472,y:31936,varname:node_8889,prsc:2|IN-9374-OUT;n:type:ShaderForge.SFN_Multiply,id:6727,x:31858,y:33486,varname:node_6727,prsc:2|A-1038-OUT,B-2650-OUT;n:type:ShaderForge.SFN_Vector1,id:2650,x:31651,y:33579,varname:node_2650,prsc:2,v1:1.1;n:type:ShaderForge.SFN_Tex2d,id:8030,x:32101,y:33561,varname:_node_9753,prsc:2,tex:fb6566c21f717904f83743a5a76dd0b0,ntxv:0,isnm:False|UVIN-6727-OUT,MIP-6073-OUT,TEX-4819-TEX;n:type:ShaderForge.SFN_Lerp,id:9181,x:32774,y:33232,varname:node_9181,prsc:2|A-6208-OUT,B-8030-RGB,T-8889-OUT;n:type:ShaderForge.SFN_Lerp,id:8916,x:33153,y:33342,varname:node_8916,prsc:2|A-7688-OUT,B-9181-OUT,T-4855-OUT;n:type:ShaderForge.SFN_Slider,id:981,x:32503,y:33709,ptovrint:False,ptlb:Normal,ptin:_Normal,varname:_Normal,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:1;n:type:ShaderForge.SFN_Multiply,id:1964,x:33564,y:33382,varname:node_1964,prsc:2|A-8916-OUT,B-1979-RGB;n:type:ShaderForge.SFN_Color,id:1979,x:33335,y:33517,ptovrint:False,ptlb:NormalMult,ptin:_NormalMult,varname:_NormalMult,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:0.9310346,c3:0,c4:1;n:type:ShaderForge.SFN_Panner,id:1090,x:31447,y:33157,varname:node_1090,prsc:2,spu:1,spv:1|UVIN-8317-OUT,DIST-2890-OUT;n:type:ShaderForge.SFN_Vector1,id:8432,x:31260,y:33301,varname:node_8432,prsc:2,v1:1;n:type:ShaderForge.SFN_Panner,id:6492,x:31579,y:32996,varname:node_6492,prsc:2,spu:-1,spv:1|UVIN-8317-OUT,DIST-2890-OUT;n:type:ShaderForge.SFN_Multiply,id:2890,x:30928,y:33195,varname:node_2890,prsc:2|A-9632-OUT,B-7600-TSL;n:type:ShaderForge.SFN_Slider,id:9632,x:30534,y:33347,ptovrint:False,ptlb:Pan,ptin:_Pan,varname:_Pan,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.1111111,max:1;n:type:ShaderForge.SFN_Time,id:7600,x:30569,y:33479,varname:node_7600,prsc:2;n:type:ShaderForge.SFN_Distance,id:7731,x:34120,y:33458,varname:node_7731,prsc:2|A-1595-XYZ,B-8737-XYZ;n:type:ShaderForge.SFN_FragmentPosition,id:1595,x:33788,y:33493,varname:node_1595,prsc:2;n:type:ShaderForge.SFN_ViewPosition,id:8737,x:33775,y:33415,varname:node_8737,prsc:2;n:type:ShaderForge.SFN_Divide,id:5029,x:34504,y:33491,varname:node_5029,prsc:2|A-7731-OUT,B-9964-OUT;n:type:ShaderForge.SFN_Slider,id:9964,x:34169,y:33379,ptovrint:False,ptlb:Fade distance,ptin:_Fadedistance,varname:_Fadedistance,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:1,cur:1,max:50000;n:type:ShaderForge.SFN_Negate,id:9436,x:35004,y:33291,varname:node_9436,prsc:2|IN-5029-OUT;n:type:ShaderForge.SFN_Exp,id:3532,x:35293,y:33365,varname:node_3532,prsc:2,et:0|IN-9436-OUT;n:type:ShaderForge.SFN_Distance,id:5124,x:30792,y:33954,varname:node_5124,prsc:2|A-9441-XYZ,B-6966-XYZ;n:type:ShaderForge.SFN_FragmentPosition,id:9441,x:30460,y:33989,varname:node_9441,prsc:2;n:type:ShaderForge.SFN_ViewPosition,id:6966,x:30556,y:34186,varname:node_6966,prsc:2;n:type:ShaderForge.SFN_Divide,id:5432,x:31176,y:33987,varname:node_5432,prsc:2|A-5124-OUT,B-5501-OUT;n:type:ShaderForge.SFN_Slider,id:5501,x:30829,y:34143,ptovrint:False,ptlb:MIPS,ptin:_MIPS,varname:_MIPS,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:1,cur:1,max:50000;n:type:ShaderForge.SFN_Clamp01,id:3502,x:31417,y:34009,varname:node_3502,prsc:2|IN-5432-OUT;n:type:ShaderForge.SFN_Multiply,id:6073,x:31591,y:34025,varname:node_6073,prsc:2|A-3502-OUT,B-938-OUT;n:type:ShaderForge.SFN_Vector1,id:938,x:31343,y:34223,varname:node_938,prsc:2,v1:5;n:type:ShaderForge.SFN_TexCoord,id:1220,x:31963,y:34640,varname:node_1220,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Tex2d,id:7372,x:32213,y:34670,ptovrint:True,ptlb:Gloss,ptin:_GlossMap,varname:_GlossMap,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:e80c3c84ea861404d8a427db8b7abf04,ntxv:0,isnm:False|UVIN-1220-UVOUT;n:type:ShaderForge.SFN_Multiply,id:4729,x:32570,y:34927,varname:node_4729,prsc:2|A-7372-R,B-6924-OUT;n:type:ShaderForge.SFN_Multiply,id:4855,x:33024,y:33559,varname:node_4855,prsc:2|A-981-OUT,B-3015-OUT;n:type:ShaderForge.SFN_Vector1,id:8234,x:37395,y:32843,varname:node_8234,prsc:2,v1:0;n:type:ShaderForge.SFN_Dot,id:7435,x:36817,y:33375,varname:node_7435,prsc:2,dt:1|A-889-OUT,B-855-OUT;n:type:ShaderForge.SFN_NormalVector,id:8689,x:36220,y:33461,prsc:2,pt:True;n:type:ShaderForge.SFN_ViewVector,id:889,x:36349,y:33841,varname:node_889,prsc:2;n:type:ShaderForge.SFN_HalfVector,id:855,x:36073,y:33728,varname:node_855,prsc:2;n:type:ShaderForge.SFN_Color,id:6982,x:37209,y:33821,ptovrint:False,ptlb:Backcatter,ptin:_Backcatter,varname:_Backcatter,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.0227076,c2:0.5684106,c3:0.6176471,c4:1;n:type:ShaderForge.SFN_Multiply,id:7498,x:37402,y:33540,varname:node_7498,prsc:2|A-9773-OUT,B-6982-RGB;n:type:ShaderForge.SFN_OneMinus,id:6155,x:37020,y:33505,varname:node_6155,prsc:2|IN-7435-OUT;n:type:ShaderForge.SFN_Power,id:9773,x:37209,y:33578,varname:node_9773,prsc:2|VAL-6155-OUT,EXP-4172-OUT;n:type:ShaderForge.SFN_Slider,id:4172,x:36878,y:33732,ptovrint:False,ptlb:Backcatter pow,ptin:_Backcatterpow,varname:_Backcatterpow,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:1,cur:30.78636,max:60;n:type:ShaderForge.SFN_Add,id:6069,x:37903,y:33164,varname:node_6069,prsc:2|A-7241-RGB,B-7498-OUT;n:type:ShaderForge.SFN_Fresnel,id:4083,x:38124,y:32932,varname:node_4083,prsc:2|NRM-8689-OUT,EXP-2221-OUT;n:type:ShaderForge.SFN_Vector1,id:2221,x:37908,y:32991,varname:node_2221,prsc:2,v1:5;n:type:ShaderForge.SFN_Add,id:6690,x:38652,y:32718,varname:node_6690,prsc:2|A-4083-OUT,B-7492-OUT;n:type:ShaderForge.SFN_Multiply,id:1036,x:38412,y:32928,varname:node_1036,prsc:2|A-3005-OUT,B-2638-OUT,C-6690-OUT;n:type:ShaderForge.SFN_Vector1,id:2638,x:38199,y:33123,varname:node_2638,prsc:2,v1:30;n:type:ShaderForge.SFN_FragmentPosition,id:6553,x:31963,y:33989,varname:node_6553,prsc:2;n:type:ShaderForge.SFN_Distance,id:6570,x:32225,y:34001,varname:node_6570,prsc:2|A-6553-XYZ,B-5179-XYZ;n:type:ShaderForge.SFN_ViewPosition,id:5179,x:31957,y:34151,varname:node_5179,prsc:2;n:type:ShaderForge.SFN_Divide,id:3685,x:32558,y:34072,varname:node_3685,prsc:2|A-6570-OUT,B-5149-OUT;n:type:ShaderForge.SFN_Slider,id:5149,x:32131,y:34237,ptovrint:False,ptlb:Normal fade dist,ptin:_Normalfadedist,varname:_Normalfadedist,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:1,cur:1,max:12000;n:type:ShaderForge.SFN_Clamp01,id:174,x:32767,y:34066,varname:node_174,prsc:2|IN-3685-OUT;n:type:ShaderForge.SFN_Vector3,id:7688,x:32844,y:33417,varname:node_7688,prsc:2,v1:0,v2:0,v3:1;n:type:ShaderForge.SFN_OneMinus,id:3015,x:32899,y:33915,varname:node_3015,prsc:2|IN-174-OUT;n:type:ShaderForge.SFN_Vector1,id:7492,x:38391,y:32758,varname:node_7492,prsc:2,v1:0.02;n:type:ShaderForge.SFN_DepthBlend,id:8337,x:38661,y:33107,varname:node_8337,prsc:2;n:type:ShaderForge.SFN_Multiply,id:3237,x:38862,y:33025,varname:node_3237,prsc:2|A-3532-OUT,B-8337-OUT;n:type:ShaderForge.SFN_LightAttenuation,id:4128,x:38444,y:33323,varname:node_4128,prsc:2;proporder:7241-1364-4819-3005-6924-9650-981-1979-9632-9964-5501-7372-6982-4172-5149;pass:END;sub:END;*/

Shader "Fog Volume/WaterPlane" {
    Properties {
        _Color ("Color", Color) = (0.07843138,0.3921569,0.7843137,1)
        _Scale ("Scale", Range(1, 200)) = 1
        _node_4819 ("node_4819", 2D) = "bump" {}
        _Specular ("Specular", Range(0, 1)) = 0.7948718
        _Gloss ("Gloss", Range(0, 1)) = 0.8205128
        _NormalMapblendmask ("NormalMap blend mask", 2D) = "white" {}
        _Normal ("Normal", Range(0, 1)) = 1
        _NormalMult ("NormalMult", Color) = (1,0.9310346,0,1)
        _Pan ("Pan", Range(0, 1)) = 0.1111111
        _Fadedistance ("Fade distance", Range(1, 50000)) = 1
        _MIPS ("MIPS", Range(1, 50000)) = 1
        _GlossMap ("Gloss", 2D) = "white" {}
        _Backcatter ("Backcatter", Color) = (0.0227076,0.5684106,0.6176471,1)
        _Backcatterpow ("Backcatter pow", Range(1, 60)) = 30.78636
        _Normalfadedist ("Normal fade dist", Range(1, 12000)) = 1
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
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            //#define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform float4 _LightColor0;
            uniform sampler2D _CameraDepthTexture;
            uniform float4 _TimeEditor;
            uniform float4 _Color;
            uniform float _Scale;
            uniform sampler2D _node_4819; uniform float4 _node_4819_ST;
            uniform float _Specular;
            uniform float _Gloss;
            uniform sampler2D _NormalMapblendmask; uniform float4 _NormalMapblendmask_ST;
            uniform float _Normal;
            uniform float4 _NormalMult;
            uniform float _Pan;
            uniform float _Fadedistance;
            uniform float _MIPS;
            uniform sampler2D _GlossMap; uniform float4 _GlossMap_ST;
            uniform float4 _Backcatter;
            uniform float _Backcatterpow;
            uniform float _Normalfadedist;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                float3 tangentDir : TEXCOORD3;
                float3 bitangentDir : TEXCOORD4;
                float4 projPos : TEXCOORD5;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos(v.vertex );
                o.projPos = ComputeScreenPos (o.pos);
                COMPUTE_EYEDEPTH(o.projPos.z);
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float4 node_7600 = _Time + _TimeEditor;
                float node_2890 = (_Pan*node_7600.r);
                float2 node_8317 = (i.posWorld.rgb.rb/_Scale);
                float2 node_6492 = (node_8317+node_2890*float2(-1,1));
                float node_6073 = (saturate((distance(i.posWorld.rgb,_WorldSpaceCameraPos)/_MIPS))*5.0);
                float3 _Layers = UnpackNormal(tex2Dlod(_node_4819,float4(TRANSFORM_TEX(node_6492, _node_4819),0.0,node_6073)));
                float2 node_1038 = ((node_8317+node_2890*float2(1,1))*1.1);
                float3 _node_2312 = UnpackNormal(tex2Dlod(_node_4819,float4(TRANSFORM_TEX(node_1038, _node_4819),0.0,node_6073)));
                float4 _NormalMapblendmask_var = tex2D(_NormalMapblendmask,TRANSFORM_TEX(i.uv0, _NormalMapblendmask));
                float2 node_6727 = (node_1038*1.1);
                float3 _node_9753 = UnpackNormal(tex2Dlod(_node_4819,float4(TRANSFORM_TEX(node_6727, _node_4819),0.0,node_6073)));
                float3 normalLocal = (lerp(float3(0,0,1),lerp(lerp(_Layers.rgb,_node_2312.rgb,saturate((4.0*_NormalMapblendmask_var.r*_NormalMapblendmask_var.r))),_node_9753.rgb,saturate((4.0*_NormalMapblendmask_var.g*_NormalMapblendmask_var.g))),(_Normal*(1.0 - saturate((distance(i.posWorld.rgb,_WorldSpaceCameraPos)/_Normalfadedist)))))*_NormalMult.rgb);
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float sceneZ = max(0,LinearEyeDepth (UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)))) - _ProjectionParams.g);
                float partZ = max(0,i.projPos.z - _ProjectionParams.g);
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = 1;
                float3 attenColor = attenuation * _LightColor0.xyz;
///////// Gloss:
                float4 _GlossMap_var = tex2D(_GlossMap,TRANSFORM_TEX(i.uv0, _GlossMap));
                float gloss = (_GlossMap_var.r*_Gloss);
                float specPow = exp2( gloss * 10.0 + 1.0 );
////// Specular:
                float NdotL = saturate(dot( normalDirection, lightDirection ));
                float node_1036 = (_Specular*30.0*(pow(1.0-max(0,dot(normalDirection, viewDirection)),5.0)+0.02));
                float3 specularColor = float3(node_1036,node_1036,node_1036);
                float3 directSpecular = attenColor * pow(max(0,dot(halfDirection,normalDirection)),specPow)*specularColor;
                float3 specular = directSpecular;
/////// Diffuse:
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                float3 directDiffuse = max( 0.0, NdotL) * attenColor;
                float3 indirectDiffuse = float3(0,0,0);
                indirectDiffuse += UNITY_LIGHTMODEL_AMBIENT.rgb; // Ambient Light
                float node_8234 = 0.0;
                float3 diffuseColor = float3(node_8234,node_8234,node_8234);
                float3 diffuse = (directDiffuse + indirectDiffuse) * diffuseColor;
////// Emissive:
                float3 emissive = (_Color.rgb+(pow((1.0 - max(0,dot(viewDirection,halfDirection))),_Backcatterpow)*_Backcatter.rgb));
/// Final Color:
                float3 finalColor = diffuse + specular + emissive;
                return fixed4(finalColor,(exp((-1*(distance(i.posWorld.rgb,_WorldSpaceCameraPos)/_Fadedistance)))*saturate((sceneZ-partZ))));
            }
            ENDCG
        }
        Pass {
            Name "FORWARD_DELTA"
            Tags {
                "LightMode"="ForwardAdd"
            }
            Blend One One
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            //#define UNITY_PASS_FORWARDADD
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #pragma multi_compile_fwdadd
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform float4 _LightColor0;
            uniform sampler2D _CameraDepthTexture;
            uniform float4 _TimeEditor;
            uniform float4 _Color;
            uniform float _Scale;
            uniform sampler2D _node_4819; uniform float4 _node_4819_ST;
            uniform float _Specular;
            uniform float _Gloss;
            uniform sampler2D _NormalMapblendmask; uniform float4 _NormalMapblendmask_ST;
            uniform float _Normal;
            uniform float4 _NormalMult;
            uniform float _Pan;
            uniform float _Fadedistance;
            uniform float _MIPS;
            uniform sampler2D _GlossMap; uniform float4 _GlossMap_ST;
            uniform float4 _Backcatter;
            uniform float _Backcatterpow;
            uniform float _Normalfadedist;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                float3 tangentDir : TEXCOORD3;
                float3 bitangentDir : TEXCOORD4;
                float4 projPos : TEXCOORD5;
                LIGHTING_COORDS(6,7)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos(v.vertex );
                o.projPos = ComputeScreenPos (o.pos);
                COMPUTE_EYEDEPTH(o.projPos.z);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float4 node_7600 = _Time + _TimeEditor;
                float node_2890 = (_Pan*node_7600.r);
                float2 node_8317 = (i.posWorld.rgb.rb/_Scale);
                float2 node_6492 = (node_8317+node_2890*float2(-1,1));
                float node_6073 = (saturate((distance(i.posWorld.rgb,_WorldSpaceCameraPos)/_MIPS))*5.0);
                float3 _Layers = UnpackNormal(tex2Dlod(_node_4819,float4(TRANSFORM_TEX(node_6492, _node_4819),0.0,node_6073)));
                float2 node_1038 = ((node_8317+node_2890*float2(1,1))*1.1);
                float3 _node_2312 = UnpackNormal(tex2Dlod(_node_4819,float4(TRANSFORM_TEX(node_1038, _node_4819),0.0,node_6073)));
                float4 _NormalMapblendmask_var = tex2D(_NormalMapblendmask,TRANSFORM_TEX(i.uv0, _NormalMapblendmask));
                float2 node_6727 = (node_1038*1.1);
                float3 _node_9753 = UnpackNormal(tex2Dlod(_node_4819,float4(TRANSFORM_TEX(node_6727, _node_4819),0.0,node_6073)));
                float3 normalLocal = (lerp(float3(0,0,1),lerp(lerp(_Layers.rgb,_node_2312.rgb,saturate((4.0*_NormalMapblendmask_var.r*_NormalMapblendmask_var.r))),_node_9753.rgb,saturate((4.0*_NormalMapblendmask_var.g*_NormalMapblendmask_var.g))),(_Normal*(1.0 - saturate((distance(i.posWorld.rgb,_WorldSpaceCameraPos)/_Normalfadedist)))))*_NormalMult.rgb);
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float sceneZ = max(0,LinearEyeDepth (UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)))) - _ProjectionParams.g);
                float partZ = max(0,i.projPos.z - _ProjectionParams.g);
                float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
///////// Gloss:
                float4 _GlossMap_var = tex2D(_GlossMap,TRANSFORM_TEX(i.uv0, _GlossMap));
                float gloss = (_GlossMap_var.r*_Gloss);
                float specPow = exp2( gloss * 10.0 + 1.0 );
////// Specular:
                float NdotL = saturate(dot( normalDirection, lightDirection ));
                float node_1036 = (_Specular*30.0*(pow(1.0-max(0,dot(normalDirection, viewDirection)),5.0)+0.02));
                float3 specularColor = float3(node_1036,node_1036,node_1036);
                float3 directSpecular = attenColor * pow(max(0,dot(halfDirection,normalDirection)),specPow)*specularColor;
                float3 specular = directSpecular;
/////// Diffuse:
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                float3 directDiffuse = max( 0.0, NdotL) * attenColor;
                float node_8234 = 0.0;
                float3 diffuseColor = float3(node_8234,node_8234,node_8234);
                float3 diffuse = directDiffuse * diffuseColor;
/// Final Color:
                float3 finalColor = diffuse + specular;
                return fixed4(finalColor * (exp((-1*(distance(i.posWorld.rgb,_WorldSpaceCameraPos)/_Fadedistance)))*saturate((sceneZ-partZ))),0);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
