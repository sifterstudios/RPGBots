// Upgrade NOTE: replaced 'UNITY_INSTANCE_ID' with 'UNITY_VERTEX_INPUT_INSTANCE_ID'

#ifndef FOG_VOLUME_COMMON_INPUTS_INCLUDED
#define FOG_VOLUME_COMMON_INPUTS_INCLUDED

uniform sampler2D			
							_Gradient, 
							_ValueNoise;
#ifdef _FOG_VOLUME_NOISE

uniform sampler3D			_NoiseVolume, _NoiseVolume2;
sampler2D CoverageTex;
#endif
float Collisions = 0;

half4 _AmbientHeightAbsorption;
#define _AmbientHeightAbsorptionMin _AmbientHeightAbsorption.x
#define _AmbientHeightAbsorptionMax _AmbientHeightAbsorption.y
#ifdef HEIGHT_GRAD
half4 _VerticalGradientParams;
#define GradMin _VerticalGradientParams.x
#define GradMax _VerticalGradientParams.y
#define GradMin2 _VerticalGradientParams.z
#define GradMax2 _VerticalGradientParams.w
#endif
#ifdef DEBUG
sampler2D _PerformanceLUT;
#endif

#ifdef _FOG_LOWRES_RENDERER
//sampler2D RT_Depth, RT_DepthR;
UNITY_DECLARE_SCREENSPACE_TEXTURE(RT_Depth);
UNITY_DECLARE_SCREENSPACE_TEXTURE(RT_DepthR);
#endif

#ifdef VOLUME_SHADOWS
sampler2D	LightshaftTex;
#endif

#ifdef HALO
	sampler2D	_LightHaloTexture;
	fixed	_HaloOpticalDispersion,
		_HaloIntensity,
		_HaloWidth,
		_HaloRadius,
		_HaloAbsorption;
#endif

#ifdef ExternalDepth	
	sampler2D _CustomCameraDepthTexture;
#else
	UNITY_DECLARE_SCREENSPACE_TEXTURE(_CameraDepthTexture);
#endif

	int STEP_COUNT = 200;
	int _ProxyVolume;
	int _AmbientAffectsFogColor;
#ifdef DF
	uniform float4x4			_PrimitivesTransform[20];
	uniform half	_PrimitiveEdgeSoftener;
	uniform float4 _PrimitivePosition[20],
		_PrimitiveScale[20], _PrimitiveData[20];
	int _PrimitiveCount = 0;

// Smaller than 1.0f -> BoxCollider.
// Larger than 1.0f and smaller than 2.0f -> SphereCollider.
#define _PrimitiveShapeType(i) _PrimitiveData[i].x 

// Smaller than 1.0f -> Additive.
// Larger than 1.0f and smaller than 2.0f -> Subtractive.
#define _PrimitiveActionType(i) _PrimitiveData[i].y


#endif

const static int MaxVisibleLights = 64;
#if SHADER_API_GLCORE || SHADER_API_D3D11 | SHADER_API_METAL
	
#if ATTEN_METHOD_1 || ATTEN_METHOD_2 || ATTEN_METHOD_3 
uniform float4 _LightData[MaxVisibleLights];

#define _LightIntensity(i) _LightData[i].x
#define _LightRange(i) _LightData[i].y
#define _LightSpotAngle(i) _LightData[i].z

uniform float4 _LightPositions[MaxVisibleLights], _LightColors[MaxVisibleLights], _LightRotations[MaxVisibleLights];
half  PointLightingDistance,
PointLightingDistance2Camera;


#endif
#endif
#ifdef _SHADE
int _SelfShadowSteps;
float4 _SelfShadowColor;
#endif
//uniform int							STEP_COUNT = 50,
							
int				_LightsCount = 0,


Octaves,
_DebugMode,
SamplingMethod,
DirectLightingShadowSteps;

#if DIRECTIONAL_LIGHTING
half DirectLightingShadowDensity;
				
			float4 LightExtinctionColor;
#endif

uniform float4				_Color,
							_FogColor,
							_InscatteringColor,
							_BoxMin,
							_BoxMax,
							
							Stretch,
							_LightColor,
							_AmbientColor,
							VolumeSize,
							VolumeFogInscatteringColor,
							_VolumePosition;

							


uniform float3				L = float3(0, 0, 1), 
							_LightLocalDirection,
							//_CameraForward,
							_SliceNormal,
							Speed = 1;
int VolumeFogInscatterColorAffectedWithFogColor = 1;
uniform half				DetailTiling,	
							_PrimitiveCutout,
							HeightAbsorption,
							_LightExposure,
							VolumeFogInscatteringIntensity,
							VolumeFogInscatteringAnisotropy,
							VolumeFogInscatteringStartDistance,
							VolumeFogInscatteringTransitionWideness,
							_PushAlpha,
							_DetailMaskingThreshold,
							//_DetailSamplingBaseOpacityLimit,
							_OptimizationFactor,
							_BaseRelativeSpeed,
							_DetailRelativeSpeed,
							
							_NoiseDetailRange,
							_Curl,
							Absorption, 
							BaseTiling, 
							_Cutoff,
							Coverage,
							NoiseDensity,
							LambertianBias,
							
							DirectLightingAmount,
							NormalDistance,
							_Vortex = 1, 
							_RotationSpeed, 
							_Rotation, 
							DirectLightingDistance,
							
							_FOV,
							_DirectionalLightingDistance,
							//GradMin, 
						//	GradMax,
							Constrain, 
							SphericalFadeDistance, 
							DetailDistance,
							_SceneIntersectionSoftness, 
							_InscatteringIntensity = 1, 
							InscatteringShape, 
							_Visibility, 
							InscatteringStartDistance = 100, 
							IntersectionThreshold,
							InscatteringTransitionWideness = 500, 
							_3DNoiseScale, 
							_RayStep, 
							gain = 1, 
							threshold = 0,
							Shade, 
							_SceneIntersectionThreshold, 
							ShadowBrightness, 
							_jitter, 
							FadeDistance, 
							Offset = 0, 
							Gamma = 1, 
							
							Exposure;


struct appdata {
	float4 vertex : POSITION;
	float2 texcoord : TEXCOORD0;

	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f
{
	float4 pos         : SV_POSITION;
	float3 Wpos        : TEXCOORD0;
	float4 ScreenUVs   : TEXCOORD1;
	float3 LocalPos    : TEXCOORD2;
	float3 ViewPos     : TEXCOORD3;
	float3 LocalEyePos : TEXCOORD4;
	float3 LightLocalDir : TEXCOORD5;
	float3 WorldEyeDir  : TEXCOORD6;
	float2 uv0 : TEXCOORD7;
	float3 SliceNormal : TEXCOORD8;	
	float3 worldNormal : TEXCOORD9;
	UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};


v2f vert(appdata i)
{
	v2f o;
	UNITY_SETUP_INSTANCE_ID(i);
	UNITY_TRANSFER_INSTANCE_ID(i, o);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
	o.pos = UnityObjectToClipPos(i.vertex);
	o.Wpos = mul((float4x4)unity_ObjectToWorld, float4(i.vertex.xyz, 1)).xyz;
	o.ScreenUVs = ComputeScreenPos(o.pos);
	o.ViewPos = UnityObjectToViewPos( float4(i.vertex.xyz, 1)).xyz;
	o.LocalPos = i.vertex.xyz;
	o.LocalEyePos = mul((float4x4)unity_WorldToObject, (float4(_WorldSpaceCameraPos, 1))).xyz;
	o.LightLocalDir = mul((float4x4)unity_WorldToObject, (float4(L.xyz, 1))).xyz;
	o.WorldEyeDir = (o.Wpos.xyz - _WorldSpaceCameraPos.xyz);
	o.uv0 = i.texcoord;
	//WIN http://answers.unity3d.com/questions/192553/camera-forward-vector-in-shader.html
	o.SliceNormal = UNITY_MATRIX_IT_MV[2].xyz;// mul((float4x4)unity_WorldToObject, _SliceNormal).xyz;
	o.worldNormal = float3(0, -1, 0);//upwards
	//o.worldNormal = UnityObjectToWorldNormal(float3(0,0 , 1));
	return o;

}
#endif