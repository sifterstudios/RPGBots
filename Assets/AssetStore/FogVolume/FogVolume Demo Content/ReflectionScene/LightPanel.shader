// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge v1.36 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.36;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:0,lgpr:1,limd:3,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:1,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:False,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:3138,x:34105,y:32457,varname:node_3138,prsc:2|diff-7613-OUT,spec-7613-OUT,gloss-7613-OUT,emission-384-OUT,amdfl-7613-OUT,amspl-7613-OUT;n:type:ShaderForge.SFN_Color,id:7241,x:33567,y:32828,ptovrint:False,ptlb:Color,ptin:_Color,varname:_Color,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.9926471,c2:0.9926471,c3:0.9926471,c4:1;n:type:ShaderForge.SFN_TexCoord,id:2328,x:31371,y:32234,varname:node_2328,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Sin,id:9794,x:32360,y:32434,varname:node_9794,prsc:2|IN-4565-OUT;n:type:ShaderForge.SFN_RemapRange,id:6394,x:32672,y:32364,varname:node_6394,prsc:2,frmn:-1,frmx:1,tomn:0,tomx:1|IN-9794-OUT;n:type:ShaderForge.SFN_Multiply,id:4565,x:32071,y:32503,varname:node_4565,prsc:2|A-2328-V,B-7598-OUT;n:type:ShaderForge.SFN_Slider,id:7598,x:31701,y:32632,ptovrint:False,ptlb:Tubes,ptin:_Tubes,varname:_Tubes,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:1,cur:39.11111,max:50;n:type:ShaderForge.SFN_Multiply,id:6194,x:33018,y:32555,varname:node_6194,prsc:2|A-143-OUT,B-7231-OUT;n:type:ShaderForge.SFN_Multiply,id:6399,x:32530,y:32074,varname:node_6399,prsc:2|A-2328-U,B-2328-V,C-5481-OUT,D-5315-OUT;n:type:ShaderForge.SFN_OneMinus,id:5315,x:31814,y:32233,varname:node_5315,prsc:2|IN-2328-U;n:type:ShaderForge.SFN_OneMinus,id:5481,x:31858,y:32366,varname:node_5481,prsc:2|IN-2328-V;n:type:ShaderForge.SFN_Slider,id:7231,x:32534,y:32941,ptovrint:False,ptlb:Intensity,ptin:_Intensity,varname:_Intensity,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:182.0513,max:300;n:type:ShaderForge.SFN_SwitchProperty,id:9813,x:33471,y:32384,ptovrint:False,ptlb:Darken Borders,ptin:_DarkenBorders,varname:_DarkenBorders,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:False|A-6194-OUT,B-6305-OUT;n:type:ShaderForge.SFN_Multiply,id:6305,x:33186,y:32430,varname:node_6305,prsc:2|A-9877-OUT,B-6194-OUT;n:type:ShaderForge.SFN_Lerp,id:9877,x:32905,y:32129,varname:node_9877,prsc:2|A-4855-OUT,B-6399-OUT,T-6846-OUT;n:type:ShaderForge.SFN_Slider,id:6846,x:32531,y:32227,ptovrint:False,ptlb:Border darkness,ptin:_Borderdarkness,varname:_Borderdarkness,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:1;n:type:ShaderForge.SFN_Vector1,id:4855,x:32660,y:32050,varname:node_4855,prsc:2,v1:1;n:type:ShaderForge.SFN_RemapRangeAdvanced,id:143,x:32659,y:32532,varname:node_143,prsc:2|IN-9794-OUT,IMIN-3560-OUT,IMAX-9619-OUT,OMIN-1593-OUT,OMAX-9619-OUT;n:type:ShaderForge.SFN_Vector1,id:3560,x:32257,y:32599,varname:node_3560,prsc:2,v1:-1;n:type:ShaderForge.SFN_Vector1,id:9619,x:32232,y:32676,varname:node_9619,prsc:2,v1:1;n:type:ShaderForge.SFN_Slider,id:1593,x:32213,y:32795,ptovrint:False,ptlb:Tube shadow,ptin:_Tubeshadow,varname:_Tubeshadow,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.5470086,max:1;n:type:ShaderForge.SFN_Lerp,id:384,x:33823,y:32578,varname:node_384,prsc:2|A-7459-RGB,B-7241-RGB,T-9813-OUT;n:type:ShaderForge.SFN_Color,id:7459,x:33604,y:32595,ptovrint:False,ptlb:Shadow Color,ptin:_ShadowColor,varname:_ShadowColor,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_Vector1,id:7613,x:33884,y:32313,varname:node_7613,prsc:2,v1:0;proporder:7241-7598-7231-9813-6846-1593-7459;pass:END;sub:END;*/

Shader "Fog Volume/LightPanel" {
    Properties {
        _Color ("Color", Color) = (0.9926471,0.9926471,0.9926471,1)
        _Tubes ("Tubes", Range(1, 50)) = 39.11111
        _Intensity ("Intensity", Range(0, 300)) = 182.0513
        [MaterialToggle] _DarkenBorders ("Darken Borders", Float ) = 140.8175
        _Borderdarkness ("Border darkness", Range(0, 1)) = 1
        _Tubeshadow ("Tube shadow", Range(0, 1)) = 0.5470086
        _ShadowColor ("Shadow Color", Color) = (0.5,0.5,0.5,1)
    }
    SubShader {
        Tags {
            "RenderType"="Opaque"
        }
        Pass {
            Name "DEFERRED"
            Tags {
                "LightMode"="Deferred"
            }
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            //#define UNITY_PASS_DEFERRED
            #include "UnityCG.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma multi_compile ___ UNITY_HDR_ON
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 
            #pragma target 3.0
            uniform float4 _Color;
            uniform float _Tubes;
            uniform float _Intensity;
            uniform fixed _DarkenBorders;
            uniform float _Borderdarkness;
            uniform float _Tubeshadow;
            uniform float4 _ShadowColor;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos(v.vertex );
                return o;
            }
            void frag(
                VertexOutput i,
                out half4 outDiffuse : SV_Target0,
                out half4 outSpecSmoothness : SV_Target1,
                out half4 outNormal : SV_Target2,
                out half4 outEmission : SV_Target3 )
            {
                i.normalDir = normalize(i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float3 viewReflectDirection = reflect( -viewDirection, normalDirection );
////// Lighting:
                float Pi = 3.141592654;
                float InvPi = 0.31830988618;
///////// Gloss:
                float node_7613 = 0.0;
                float gloss = node_7613;
                float perceptualRoughness = 1.0 - node_7613;
                float roughness = perceptualRoughness * perceptualRoughness;
/////// GI Data:
                UnityLight light; // Dummy light
                light.color = 0;
                light.dir = half3(0,1,0);
                light.ndotl = max(0,dot(normalDirection,light.dir));
                UnityGIInput d;
                d.light = light;
                d.worldPos = i.posWorld.xyz;
                d.worldViewDir = viewDirection;
                d.atten = 1;
                Unity_GlossyEnvironmentData ugls_en_data;
                ugls_en_data.roughness = 1.0 - gloss;
                ugls_en_data.reflUVW = viewReflectDirection;
                UnityGI gi = UnityGlobalIllumination(d, 1, normalDirection, ugls_en_data );
////// Specular:
                float3 specularColor = node_7613;
                float specularMonochrome;
                float3 diffuseColor = float3(node_7613,node_7613,node_7613); // Need this for specular when using metallic
                diffuseColor = DiffuseAndSpecularFromMetallic( diffuseColor, specularColor, specularColor, specularMonochrome );
                specularMonochrome = 1.0-specularMonochrome;
                float NdotV = max(0.0,dot( normalDirection, viewDirection ));
                half grazingTerm = saturate( gloss + specularMonochrome );
                float3 indirectSpecular = (0 + float3(node_7613,node_7613,node_7613));
                indirectSpecular *= FresnelLerp (specularColor, grazingTerm, NdotV);
/////// Diffuse:
                float3 indirectDiffuse = float3(0,0,0);
                indirectDiffuse += float3(node_7613,node_7613,node_7613); // Diffuse Ambient Light
////// Emissive:
                float node_9794 = sin((i.uv0.g*_Tubes));
                float node_3560 = (-1.0);
                float node_9619 = 1.0;
                float node_6194 = ((_Tubeshadow + ( (node_9794 - node_3560) * (node_9619 - _Tubeshadow) ) / (node_9619 - node_3560))*_Intensity);
                float3 emissive = lerp(_ShadowColor.rgb,_Color.rgb,lerp( node_6194, (lerp(1.0,(i.uv0.r*i.uv0.g*(1.0 - i.uv0.g)*(1.0 - i.uv0.r)),_Borderdarkness)*node_6194), _DarkenBorders ));
/// Final Color:
                outDiffuse = half4( diffuseColor, 1 );
                outSpecSmoothness = half4( specularColor, gloss );
                outNormal = half4( normalDirection * 0.5 + 0.5, 1 );
                outEmission = half4( lerp(_ShadowColor.rgb,_Color.rgb,lerp( node_6194, (lerp(1.0,(i.uv0.r*i.uv0.g*(1.0 - i.uv0.g)*(1.0 - i.uv0.r)),_Borderdarkness)*node_6194), _DarkenBorders )), 1 );
                outEmission.rgb += indirectSpecular;
                outEmission.rgb += indirectDiffuse * diffuseColor;
                #ifndef UNITY_HDR_ON
                    outEmission.rgb = exp2(-outEmission.rgb);
                #endif
            }
            ENDCG
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            //#define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 
            #pragma target 3.0
            uniform float4 _Color;
            uniform float _Tubes;
            uniform float _Intensity;
            uniform fixed _DarkenBorders;
            uniform float _Borderdarkness;
            uniform float _Tubeshadow;
            uniform float4 _ShadowColor;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                LIGHTING_COORDS(3,4)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos(v.vertex );
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float3 viewReflectDirection = reflect( -viewDirection, normalDirection );
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
                float Pi = 3.141592654;
                float InvPi = 0.31830988618;
///////// Gloss:
                float node_7613 = 0.0;
                float gloss = node_7613;
                float perceptualRoughness = 1.0 - node_7613;
                float roughness = perceptualRoughness * perceptualRoughness;
                float specPow = exp2( gloss * 10.0 + 1.0 );
/////// GI Data:
                UnityLight light;
                #ifdef LIGHTMAP_OFF
                    light.color = lightColor;
                    light.dir = lightDirection;
                    light.ndotl = LambertTerm (normalDirection, light.dir);
                #else
                    light.color = half3(0.f, 0.f, 0.f);
                    light.ndotl = 0.0f;
                    light.dir = half3(0.f, 0.f, 0.f);
                #endif
                UnityGIInput d;
                d.light = light;
                d.worldPos = i.posWorld.xyz;
                d.worldViewDir = viewDirection;
                d.atten = attenuation;
                Unity_GlossyEnvironmentData ugls_en_data;
                ugls_en_data.roughness = 1.0 - gloss;
                ugls_en_data.reflUVW = viewReflectDirection;
                UnityGI gi = UnityGlobalIllumination(d, 1, normalDirection, ugls_en_data );
                lightDirection = gi.light.dir;
                lightColor = gi.light.color;
////// Specular:
                float NdotL = saturate(dot( normalDirection, lightDirection ));
                float LdotH = saturate(dot(lightDirection, halfDirection));
                float3 specularColor = node_7613;
                float specularMonochrome;
                float3 diffuseColor = float3(node_7613,node_7613,node_7613); // Need this for specular when using metallic
                diffuseColor = DiffuseAndSpecularFromMetallic( diffuseColor, specularColor, specularColor, specularMonochrome );
                specularMonochrome = 1.0-specularMonochrome;
                float NdotV = abs(dot( normalDirection, viewDirection ));
                float NdotH = saturate(dot( normalDirection, halfDirection ));
                float VdotH = saturate(dot( viewDirection, halfDirection ));
                float visTerm = SmithJointGGXVisibilityTerm( NdotL, NdotV, roughness );
                float normTerm = GGXTerm(NdotH, roughness);
                float specularPBL = (visTerm*normTerm) * UNITY_PI;
                #ifdef UNITY_COLORSPACE_GAMMA
                    specularPBL = sqrt(max(1e-4h, specularPBL));
                #endif
                specularPBL = max(0, specularPBL * NdotL);
                #if defined(_SPECULARHIGHLIGHTS_OFF)
                    specularPBL = 0.0;
                #endif
                half surfaceReduction;
                #ifdef UNITY_COLORSPACE_GAMMA
                    surfaceReduction = 1.0-0.28*roughness*perceptualRoughness;
                #else
                    surfaceReduction = 1.0/(roughness*roughness + 1.0);
                #endif
                specularPBL *= any(specularColor) ? 1.0 : 0.0;
                float3 directSpecular = attenColor*specularPBL*FresnelTerm(specularColor, LdotH);
                half grazingTerm = saturate( gloss + specularMonochrome );
                float3 indirectSpecular = (0 + float3(node_7613,node_7613,node_7613));
                indirectSpecular *= FresnelLerp (specularColor, grazingTerm, NdotV);
                indirectSpecular *= surfaceReduction;
                float3 specular = (directSpecular + indirectSpecular);
/////// Diffuse:
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                half fd90 = 0.5 + 2 * LdotH * LdotH * (1-gloss);
                float nlPow5 = Pow5(1-NdotL);
                float nvPow5 = Pow5(1-NdotV);
                float3 directDiffuse = ((1 +(fd90 - 1)*nlPow5) * (1 + (fd90 - 1)*nvPow5) * NdotL) * attenColor;
                float3 indirectDiffuse = float3(0,0,0);
                indirectDiffuse += UNITY_LIGHTMODEL_AMBIENT.rgb; // Ambient Light
                indirectDiffuse += float3(node_7613,node_7613,node_7613); // Diffuse Ambient Light
                float3 diffuse = (directDiffuse + indirectDiffuse) * diffuseColor;
////// Emissive:
                float node_9794 = sin((i.uv0.g*_Tubes));
                float node_3560 = (-1.0);
                float node_9619 = 1.0;
                float node_6194 = ((_Tubeshadow + ( (node_9794 - node_3560) * (node_9619 - _Tubeshadow) ) / (node_9619 - node_3560))*_Intensity);
                float3 emissive = lerp(_ShadowColor.rgb,_Color.rgb,lerp( node_6194, (lerp(1.0,(i.uv0.r*i.uv0.g*(1.0 - i.uv0.g)*(1.0 - i.uv0.r)),_Borderdarkness)*node_6194), _DarkenBorders ));
/// Final Color:
                float3 finalColor = diffuse + specular + emissive;
                return fixed4(finalColor,1);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
