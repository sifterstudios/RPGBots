// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

#ifndef FOG_VOLUME_FRAGMENT_INCLUDED
#define FOG_VOLUME_FRAGMENT_INCLUDED
#define HALF_MAX        65504.0
// Clamp HDR value within a safe range
inline half  SafeHDR(half  c) { return min(c, HALF_MAX); }
inline half2 SafeHDR(half2 c) { return min(c, HALF_MAX); }
inline half3 SafeHDR(half3 c) { return min(c, HALF_MAX); }
inline half4 SafeHDR(half4 c) { return min(c, HALF_MAX); }

float4 frag(v2f i) : SV_Target
{
	UNITY_SETUP_INSTANCE_ID(i);
	float3 ViewDir = normalize(i.LocalPos - i.LocalEyePos);
	float tmin = 0.0, tmax = 0.0;
	bool hit = IntersectBox(i.LocalEyePos, ViewDir, _BoxMin.xyz, _BoxMax.xyz, tmin, tmax);
	if (!hit)
		discard;

	//likely to resolve to a free modifier
	if (tmin < 0)
		tmin = 0;

	float4 ScreenUVs = UNITY_PROJ_COORD(i.ScreenUVs);
	float2 screenUV = ScreenUVs.xy / ScreenUVs.w;
	float Depth = 0;

#if _FOG_LOWRES_RENDERER && !ExternalDepth
	//low res
#ifdef FOG_VOLUME_STEREO_ON
	//left eye
	if (unity_CameraProjection[0][2] < 0)//lo estaba haciendo con unity_StereoEyeIndex, pero por algún motivo, no se entera
	{
		//Depth = tex2D(RT_Depth, screenUV);
		Depth = UNITY_SAMPLE_SCREENSPACE_TEXTURE(RT_Depth, screenUV);

	}
	//right eye
	else //if (unity_CameraProjection[0][2] > 0)
	{
		//Depth = tex2Dlod(RT_DepthR, float4(screenUV, 0, 0));
		Depth = UNITY_SAMPLE_SCREENSPACE_TEXTURE(RT_DepthR, screenUV);
	}
#else
		//Depth = tex2Dlod(RT_Depth, float4(screenUV, 0, 0));
		Depth = UNITY_SAMPLE_SCREENSPACE_TEXTURE(RT_Depth, screenUV);
#endif

	Depth = 1.0 / Depth;

	//#else
	//#ifdef ExternalDepth 
	//	 //injected from water asset
	//	 float Depth = tex2D(_CustomCameraDepthTexture, screenUV).r;
	//	 Depth = 1.0 / Depth;
	//
	//#else
	//full res or Scene view
	// float Depth = tex2D(_CameraDepthTexture, screenUV).r;
	//	 Depth = LinearEyeDepth(Depth);
	//#endif
#endif

#ifdef ExternalDepth 
	//injected from water asset
	Depth = tex2D(_CustomCameraDepthTexture, screenUV).r;
	Depth = 1.0 / Depth;
#else
#if !_FOG_LOWRES_RENDERER
	//full res or Scene view
	Depth = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_CameraDepthTexture, screenUV).r;
	Depth = LinearEyeDepth(Depth);
#endif
#endif
    //probando
    //Depth = tex2D(_CameraDepthTexture, screenUV).r;
	//Depth = LinearEyeDepth(Depth);
	//return float4( Depth.xxx, 1);
	Depth = length(Depth / normalize(i.ViewPos).z);
	float thickness = min(max(tmin, tmax), Depth) - min(min(tmin, tmax), Depth);
	float Fog = thickness / _Visibility;

	Fog = 1.0 - exp(-Fog);
	//return Fog;
	float4 Final = 0;
	float3 Normalized_CameraWorldDir = normalize(i.Wpos - _WorldSpaceCameraPos);

	float3 CameraLocalDir = (i.LocalPos - i.LocalEyePos);
	half InscatteringDistanceClamp = saturate(Depth / InscatteringTransitionWideness - InscatteringStartDistance);
	//  half InscatteringDistanceClamp = saturate((InscatteringTransitionWideness -Depth) / (InscatteringTransitionWideness- InscatteringStartDistance));
	half4 PointLightAccum = 0;
	float4 PointLightsFinal = 0;
#if _FOG_VOLUME_NOISE || _FOG_GRADIENT	

	half jitter = 1;
#ifdef JITTER

	
	jitter = remap_tri(nrand(ScreenUVs + frac(_Time.x)));

	jitter = lerp(1, jitter, _jitter);
	
#endif


	float4 Noise = 1;
	float3 ShadowColor = 0;
	float3 rayStart = i.LocalEyePos + ViewDir * tmin;
	float3 rayStop = i.LocalEyePos + ViewDir * tmax;
	float3 rayDir = rayStop - rayStart;
	float RayLength = length(rayDir);
	Speed *= _Time.x;
	float4 FinalNoise = 0;
	float4 Gradient = 1;
	half Contact = 1;
	half3 AmbientColor = 1;



	half DistanceFade = 0;
	half SphereDistance = 0;
	half DetailCascade0 = 0;
	half DetailCascade1 = 0;
	half DetailCascade2 = 0;
	half3 Phase = 0;
	/*half*/ PrimitiveAccum = 1;
	float3 normal = float3(0, 0, 1);
	half LambertTerm = 1;
	float3 LightTerms = 0;
	half SelfShadows = 1;
	float OpacityTerms = 0;
	half4 debugOutput = 1;
	float DirectLightingShadowStepSize = (1.0 / (float)DirectLightingShadowSteps)*_DirectionalLightingDistance;
	float3 LightVector = _LightLocalDirection;
	//LightVector.xz = LightVector.zx;
	//LightVector.z = -LightVector.z;
	//LightVector *= DirectLightingShadowStepSize;
	float DirectionalLightingAccum = 1;
	float3 DirectionalLighting = 1;
	half3 AmbientTerm = 1;
	half Lambert = 1;
	half absorption = 1;
	half LightShafts = 1;
	half4 VolumeFog = 0;
	half LighsShaftsLightVectorConstrain = VolumeSize.y / VolumeSize.x;
	float3 LightShaftsDir = L;
	LightShaftsDir.xz = LighsShaftsLightVectorConstrain * LightShaftsDir.zx;
	float4 debugIterations = float4(0,0,0,1);
	float t = 0, dt = _RayStep;

	float3 r0 = rayStart;
	float3  rd = normalize(rayDir);

#ifdef SAMPLING_METHOD_ViewAligned

	float PlaneNdotRay = dot(rd, i.SliceNormal);
	dt = _RayStep / abs(PlaneNdotRay);
	t = dt - fmod(dot(r0, i.SliceNormal), _RayStep) / PlaneNdotRay;
#endif



#ifdef JITTER
	t *= jitter;
	dt *= jitter;
#endif
	half VolumeRayDistanceTraveled = 0;
	for (int s = 1; s < STEP_COUNT && RayLength>0; s += 1, t += dt, RayLength -= dt)
	{
		dt *= 1 + s * s * s * _OptimizationFactor;//live fast and die young
		float3 pos = r0 + rd * t;
		VolumeSpaceCoords = pos;
		// added casting for ps4
		VolumeSpaceCoordsWorldSpace = mul((float3x3)unity_ObjectToWorld, (float3)VolumeSpaceCoords) + _VolumePosition;
		float3 NoiseCoordinates = VolumeSpaceCoords * (_3DNoiseScale * Stretch.rgb);
		//DistanceFade = distance(VolumeSpaceCoords, i.LocalEyePos);
		DistanceFade = distance(VolumeSpaceCoordsWorldSpace, _WorldSpaceCameraPos);//3.2.1
		DetailCascade0 = 1 - saturate(DistanceFade / DetailDistance);
		DetailCascade1 = 1 - saturate(DistanceFade / DirectLightingDistance);

#if SHADER_API_GLCORE || SHADER_API_D3D11 || SHADER_API_METAL
#if ATTEN_METHOD_1 || ATTEN_METHOD_2 || ATTEN_METHOD_3
		DetailCascade2 = 1 - saturate(DistanceFade * PointLightingDistance2Camera);
#endif
#endif
		DistanceFade = saturate(DistanceFade / FadeDistance);

		DistanceFade = 1 - DistanceFade;
		if (DistanceFade < .001) break;
		//#ifdef _COLLISION	
		if (Collisions != 0)
		{
			Contact = saturate((Depth - distance(VolumeSpaceCoords, i.LocalEyePos))*_SceneIntersectionSoftness);
			if (Contact < .01)
				break;
			//#endif
		}
#ifdef DF

		PrimitiveAccum = 0;
		half3 p = 0;

		//Additive primitives
		for (int k = 0; k < _PrimitiveCount; k++)
		{			
			if (_PrimitiveActionType(k) <= 1.0f)
			{
				p = mul(_PrimitivesTransform[k], VolumeSpaceCoords - _PrimitivePosition[k]);
				
				PrimitiveAccum = max(PrimitiveAccum, 1 - (PrimitiveShape(_PrimitiveShapeType(k), p, _PrimitiveScale[k] * .5) + Constrain));
			}			
		}

		//Subtractive primitives
		for (int n = 0; n < _PrimitiveCount; n++)
		{
			if (_PrimitiveActionType(n) > 1.0f)
			{
				p = mul(_PrimitivesTransform[n], VolumeSpaceCoords - _PrimitivePosition[n]);
				
					PrimitiveAccum = min(PrimitiveAccum, (PrimitiveShape(_PrimitiveShapeType(n), p, _PrimitiveScale[n] * .5) + Constrain));
			}
		}

		//Final adjustments
		
		PrimitiveAccum = ContrastF(PrimitiveAccum * _PrimitiveEdgeSoftener, 1);
		
#endif


#if defined(_FOG_GRADIENT)
		half2 GradientCoords = VolumeSpaceCoords.xy / (_BoxMax.xy - _BoxMin.xy) - .5f;
		GradientCoords.y *= 0.95;//correct bottom. must check in the future what's wrong with the uv at the edges
		GradientCoords.y -= 0.04;//3.1.1
								 //if(PrimitiveAccum>.99)
		if (gain>0)
			Gradient = tex2Dlod(_Gradient, half4(GradientCoords, 0, 0));

#endif	

		VerticalGrad = (VolumeSpaceCoords.y / (_BoxMax.y - _BoxMin.y) + 0.5);
#ifdef VOLUME_FOG
		if (OpacityTerms <1)
			VolumeRayDistanceTraveled++;

		float VolumeDepth01 = (float)VolumeRayDistanceTraveled / STEP_COUNT;
		float DistanceCamera2VolumeWalls = length(CameraLocalDir);
		float DistanceCamera2Center = distance(_WorldSpaceCameraPos, _VolumePosition);
		float DistanceCamera2VolumePoints = distance(_WorldSpaceCameraPos, VolumeSpaceCoordsWorldSpace);
		float VolumeDepth = min(max(tmin, tmax), DistanceCamera2VolumePoints) - min(min(tmin, tmax), DistanceCamera2VolumePoints);
		float VolumeDensity = DistanceCamera2VolumePoints - DistanceCamera2VolumeWalls;
		VolumeFog = saturate(1 - exp(-VolumeDepth / _Visibility * 5));
		//VolumeFog= saturate(1 - exp(-VolumeRayDistanceTraveled / _Visibility));


		VolumeFog.a *= Contact/**Gradient.a*/;// aquí hay que decidir si el gradiente se lo come o no

#endif

		NoiseAtten = gain;
		NoiseAtten *= DistanceFade;
		NoiseAtten *= Contact;



#ifdef HEIGHT_GRAD		
		heightGradient = half2(HeightGradient(VerticalGrad, GradMin, GradMax),
			/*3.1.10: Adding secondary gradient*/HeightGradient(VerticalGrad, GradMin2, GradMax2));
		NoiseAtten *= heightGradient.x*heightGradient.y;
#endif
#if SPHERICAL_FADE
		SphereDistance = 1 - saturate(length(VolumeSpaceCoords) / SphericalFadeDistance);
		NoiseAtten *= SphereDistance;

#endif



		//TEXTURE SAMPLERS//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#if _FOG_VOLUME_NOISE || _FOG_GRADIENT


#if Twirl_X || Twirl_Y || Twirl_Z
		float3 rotationDegree = length(NoiseCoordinates) * _Vortex + _Rotation + _RotationSpeed * _Time.x;

		NoiseCoordinates = rotate(NoiseCoordinates , rotationDegree);
#endif						
		half4 VerticalCoords = float4(VolumeSpaceCoords.zx / (_BoxMax.zx - _BoxMin.zx) - .5f, 0, 0);


#ifdef COVERAGE
		half CoverageTile = 1;
		half4 CoverageCoords = CoverageTile * float4(VolumeSpaceCoords.xz / (_BoxMax.xz - _BoxMin.xz) - .5f, 0, 0);
		half4 CoverageRGB = tex2Dlod(CoverageTex, CoverageCoords);

#ifdef HEIGHT_GRAD	
		//CoverageRGB.r *=heightGradient.y * (1+heightGradient.x * heightGradient.x);
		CoverageRGB.r *= heightGradient.y;
#endif
		half CumulonimbusTop = HeightGradient(VerticalGrad, 1.0, .5);
		half Cresta = HeightGradient(VerticalGrad, 0.7, 1)*CumulonimbusTop;
		Cresta *= 10 * CoverageRGB.r;
		NoiseAtten *= CoverageRGB.r;
		NoiseAtten += CoverageRGB.r * 4 * CumulonimbusTop + Cresta;

#endif

		if (Contact > 0 && NoiseAtten > 0 && PrimitiveAccum > _PrimitiveCutout)
		{
#if _FOG_VOLUME_NOISE && !_FOG_GRADIENT
			Noise = noise(NoiseCoordinates, DetailCascade0);
#endif


#if !_FOG_VOLUME_NOISE && _FOG_GRADIENT
			Gradient.a *= gain;
			Noise = Gradient;
#endif

#if _FOG_VOLUME_NOISE && _FOG_GRADIENT
			Noise = noise(NoiseCoordinates, DetailCascade0) * Gradient;

#endif


			if (Noise.a>0)
				Noise *= DistanceFade;

		}
		else
		{
			Noise = 0;
			Gradient.a = 0;
		}
		half absorptionFactor = lerp(1, 200, Absorption);
#ifdef ABSORPTION

		half d = Noise.a;//si se multiplica aquí añade cotraste

						 //half absorptionFactor = lerp(1, 20, Absorption);


		half Beers = exp(-d* absorptionFactor)* absorptionFactor;//la última multiplicación da contraste
		half Powder = 1 - exp(-d * 2);
		absorption = lerp(1, saturate(Beers*Powder), Absorption);
#ifdef HEIGHT_GRAD
		half HeightGradientAtten = 1 - heightGradient.x;
		HeightGradientAtten = 1.0 - exp(-HeightGradientAtten);
		absorption *= lerp(1,HeightGradientAtten, HeightAbsorption);
#endif


		//	AmbientTerm = absorption;3.2


#else				
		//AmbientTerm = 1;3.2
#endif
#if _LAMBERT_SHADING	
		if (LightShafts > 0.1)//si estamos en sombra, pos no hagas ná
		{
			//Lambert lighting
			if (Noise.a > 0 && NoiseAtten > 0 && DetailCascade1 > 0)
			{
				normal = calcNormal(NoiseCoordinates, DetailCascade0);
				//normal = normalize(VolumeSpaceCoordsWorldSpace- _VolumePosition);//sphere normals
				LambertTerm = max(0, dot(normal, normalize(-L)));

				//kind of half lambert
				LambertTerm = LambertTerm*0.5 + LambertianBias;
				LambertTerm *= LambertTerm;
				Lambert = lerp(1, LambertTerm, Noise.a*DirectLightingAmount*ContrastF(DetailCascade1 * 3, 2));
				Lambert = max(0.0, Lambert);

			}
		}
#endif
		AmbientColor = _AmbientColor.rgb;
#ifndef SHADOW_PASS
		AmbientTerm.rgb = _AmbientColor.rgb;
#endif
		half3 ProxyAmbient = 1;
		UNITY_BRANCH
			if (PROBES == 1) {
				//#ifdef PROBES

				ProxyAmbient = ShadeSHPerPixel(i.worldNormal, 0, VolumeSpaceCoordsWorldSpace);
				AmbientTerm.rgb *= ProxyAmbient/* * _AmbientColor.rgb*/;
				AmbientColor *= ProxyAmbient;


			}

		//#endif
		AmbientTerm *= absorption;//3.2
#endif
#if _SHADE
		if (Noise.a > 0)
			SelfShadows = Shadow(NoiseCoordinates, i, DetailCascade0, LightVector* ShadowShift, NoiseAtten);

#endif		
		//3.1.10

		HeightAtten = HeightGradient(VerticalGrad, _AmbientHeightAbsorptionMin, _AmbientHeightAbsorptionMax);
		HeightAtten = saturate(1.0 - exp(-HeightAtten));
		if (_AmbientHeightAbsorptionMin == -1 && _AmbientHeightAbsorptionMax == -1)//just to avoid adding one more shader variant
			HeightAtten = 1;
		//AmbientTerm *= HeightAtten;3.2 ambient shouldn't be affected by this
		SelfShadows *= HeightAtten;
		//
#if DIRECTIONAL_LIGHTING
		float DirectionalLightingSample = 0;
		//TODO	if (LightShafts > 0.1)

		if (NoiseAtten>0 && Noise.a>0)
		{
			float3 DirectionalLightingSamplingPosition = NoiseCoordinates;
			for (int s = 0; s < DirectLightingShadowSteps; s++)
			{
				DirectionalLightingSamplingPosition += LightVector*DirectLightingShadowStepSize;
				DirectionalLightingSample = noise(DirectionalLightingSamplingPosition, DetailCascade0).r;
				DirectionalLightingAccum += DirectionalLightingSample/* / DirectLightingShadowSteps*/;
			}
			DirectionalLighting = DirectionalLightingAccum;
		}
		DirectionalLighting *= Noise.a;

#endif

#if defined (_INSCATTERING)

#if ABSORPTION 
		//lets diffuse LambertTerm according to density/absorption

		//remap absorption greyscale to -1 1 range to affect anisotropy according to media density
		////////					multiply ranges of [0, 1]
		half t = (1 - Noise.a) * (InscatteringShape*0.5 + 0.5);
		//get back to [-1, 1]
		t = t * 2 - 1;

		InscatteringShape = lerp(InscatteringShape, (t), Absorption);


		//            #if ABSORPTION && VOLUME_FOG
		//						  InscatteringShape = lerp(InscatteringShape*absorption, InscatteringShape, Noise.a);
		//            #endif


#endif
		half HG = Henyey(Normalized_CameraWorldDir, L, InscatteringShape);
		HG *= InscatteringDistanceClamp * Contact;
		Phase = _InscatteringColor.rgb * _InscatteringIntensity * HG * Gradient.xyz * _LightColor.rgb;//3.2;




		Phase *= absorption;


#endif			

#ifdef HALO
		half LdotV = saturate(dot(L, Normalized_CameraWorldDir));
		LdotV = pow(LdotV, _HaloRadius);
		LdotV = 1 - exp(-LdotV);
		LdotV = ContrastF(LdotV, _HaloWidth);//franja

		LdotV = saturate(LdotV);
		LdotV -= .5;

		//#ifdef ABSORPTION
		//		half HaloDensityTerm = absorption;
		//#else
		half HaloDensityTerm = Noise.a;
		//#endif
		half mip = saturate(Noise.a * 12) * _HaloAbsorption * (1 - HaloDensityTerm);

		half4 Halo = 0;
		if (LdotV >0)
		{
			Halo = tex2Dlod(_LightHaloTexture, float4(0, LdotV, 0, mip));
			Halo.g = tex2Dlod(_LightHaloTexture, float4(0, LdotV * 1.1, 0, mip)/**_HaloOpticalDispersion*/).r;
			Halo.b = tex2Dlod(_LightHaloTexture, float4(0, LdotV * 1.2, 0, mip)/**_HaloOpticalDispersion*/).r;
			Halo.rgb *= _HaloIntensity;
			Halo.rgb *= Halo.a;
			Halo.rgb *= HaloDensityTerm  * (1.0 - HaloDensityTerm);
			Halo.rgb *= 1 - mip / 12;




			Halo.rgb *= LdotV;


		}
		else
			Halo = 0;

#endif


		OpacityTerms = Noise.a*Contact;
#if DIRECTIONAL_LIGHTING
		//Shadow Color
		DirectionalLighting /= LightExtinctionColor.rgb;

		DirectionalLighting = DirectionalLighting* DirectLightingShadowDensity;
		DirectionalLighting = exp(-DirectionalLighting);
		Phase *= DirectionalLighting;
#endif
		//half3 LightTerms =  exp(-DirectionalLighting) *  OpacityTerms;
		half3 LightTerms = OpacityTerms * AmbientTerm.rgb * DirectionalLighting;//3.2
																				//#ifdef VOLUMETRIC_SHADOWS
		UNITY_BRANCH
			if (VOLUMETRIC_SHADOWS == 1) {
				// added casting for ps4
				half2 shadowUVs = (mul((float3x3)_ShadowCameraProjection, (float3)(VolumeSpaceCoordsWorldSpace - _ShadowCameraPosition)).xy + _ShadowCameraSize) / (_ShadowCameraSize*2.0f);

#ifdef CONVOLVE_VOLUMETRIC_SHADOWS
#define _GeneralShadowOffset  0.003f	
				float pointDepth = length(_ShadowCameraPosition - VolumeSpaceCoordsWorldSpace);
				//
				half4 shadows;
				shadows.x = step(pointDepth, tex2Dlod(_ShadowTexture, float4(shadowUVs + float2(-_GeneralShadowOffset, -_GeneralShadowOffset), 0, 0)).r);
				shadows.y = step(pointDepth, tex2Dlod(_ShadowTexture, float4(shadowUVs + float2(_GeneralShadowOffset, _GeneralShadowOffset), 0, 0)).r);
				shadows.z = step(pointDepth, tex2Dlod(_ShadowTexture, float4(shadowUVs + float2(-_GeneralShadowOffset, _GeneralShadowOffset), 0, 0)).r);
				shadows.w = step(pointDepth, tex2Dlod(_ShadowTexture, float4(shadowUVs + float2(_GeneralShadowOffset, -_GeneralShadowOffset), 0, 0)).r);
				VolumeShadow = max(dot(shadows, 0.25f), 0.0f);
#else
				VolumeShadow = FrameShadow(shadowUVs, VolumeSpaceCoordsWorldSpace);
#endif
				//3.2
#if defined (_INSCATTERING)
				Phase *= VolumeShadow;
#endif
#ifdef HALO
				Halo.rgb *= VolumeShadow;
#endif
				//3.2
				//LightTerms *= lerp(_AmbientColor.rgb, 1, VolumeShadow);//previous 3.2
				//LightTerms *= lerp(AmbientTerm.rgb, 1, VolumeShadow);//3.2 test

			}

		//#endif


		//Phase *= LightTerms;//atten with shadowing//3.2 comment
		Phase *= OpacityTerms;//3.2

							  //LightTerms *= lerp(_AmbientColor.rgb, 1, absorption);//previous 3.2
							  //LightTerms *= lerp(AmbientTerm.rgb, 1, absorption);//3.2 test


#ifdef VOLUME_SHADOWS

		half4 LightMapCoords = VerticalCoords;

		LightMapCoords.xy = LightMapCoords.yx;

#ifndef LIGHT_ATTACHED
		LightMapCoords.xy += LightShaftsDir.zx*(1 - VerticalGrad);
#endif
		LightMapCoords.x = clamp(LightMapCoords.x, -1, 0);
		LightMapCoords.y = clamp(LightMapCoords.y, -1, 0);
		LightShafts = tex2Dlod(LightshaftTex, LightMapCoords).r;
		LightShafts = LightShafts*lerp(50,1,_Cutoff);
		LightShafts = 1 - saturate(LightShafts);
		//LightShafts *= _LightColor.rgb;3.2
#if defined (_INSCATTERING)
		Phase *= LightShafts;
#endif
#ifdef HALO
		Halo.rgb *= LightShafts;	// changed to rgb for ps4				
#endif

#endif



#ifdef VOLUME_SHADOWS
		LightTerms *= LightShafts;

#endif

		LightTerms *= Lambert;

#ifdef _SHADE
		float3 SelfShadowsColor = lerp(_SelfShadowColor.rgb * AmbientTerm.rgb, 1.0, SelfShadows);//3.2 replaced ambient color with AmbientTerm
		LightTerms *= SelfShadowsColor;
		Phase *= SelfShadowsColor;
#endif
		//3.1.10
		LightTerms *= HeightAtten;
		Phase *= HeightAtten;
		//


#ifdef VOLUME_FOG
		half3 VolumetricFogVolor = _FogColor.rgb;
		//#ifdef AMBIENT_AFFECTS_FOG_COLOR
		if (AMBIENT_AFFECTS_FOG_COLOR)	VolumetricFogVolor *= AmbientColor;
		//#endif
#ifdef VOLUME_SHADOWS
		LightTerms = lerp(LightTerms, VolumetricFogVolor * Gradient.rgb * (LightShafts + _AmbientColor.a), VolumeFog.a);
#else
		LightTerms = lerp(LightTerms, VolumetricFogVolor * Gradient.rgb, VolumeFog.a);
#endif



#if defined (_VOLUME_FOG_INSCATTERING)
		half VolumeFogInscatteringDistanceClamp = saturate((VolumeRayDistanceTraveled - VolumeFogInscatteringStartDistance) / VolumeFogInscatteringTransitionWideness);
		half3 VolumeFogPhase = Henyey(Normalized_CameraWorldDir, L, VolumeFogInscatteringAnisotropy);
		VolumeFogPhase *= Contact;
		VolumeFogPhase *= VolumeFogInscatteringDistanceClamp;
		VolumeFogPhase *= VolumeFogInscatteringColor.rgb * VolumeFogInscatteringIntensity * Gradient.xyz;
		VolumeFogPhase *= saturate(1 - Noise.a * VolumeFogInscatteringIntensity/*pushin' proportionaly to intensity*/);
		half3 FogInscatter = 0;

		UNITY_BRANCH
			if (FOG_TINTS_INSCATTER == 1)
				FogInscatter = VolumeFog.a *VolumetricFogVolor * VolumeFogPhase * _LightColor.rgb;
		//3.2 
			else
				FogInscatter = VolumeFog.a *VolumetricFogVolor * VolumeFogPhase * _LightColor.rgb + VolumeFog.a * VolumeFogPhase * _LightColor.rgb;

#ifdef VOLUME_SHADOWS		
		FogInscatter *= LightShafts;
#endif

		UNITY_BRANCH
			if (VOLUMETRIC_SHADOWS == 1)
				FogInscatter *= VolumeShadow;

		LightTerms += FogInscatter;
#endif
		OpacityTerms = min(OpacityTerms + VolumeFog.a, 1);
#endif


#if defined(_FOG_GRADIENT)
		LightTerms *= Gradient.rgb;
#endif	
		//#ifdef PROBES

		//LightTerms *= ProxyAmbient*5;//maybe not
		//#endif
		LightTerms += Phase;

		//Multiply by LambertTerm and color before additive stuff
		LightTerms *= _Color.rgb;
		LightTerms *= _LightExposure;//new in 3.1.1
		LightTerms += AmbientTerm*Noise.a;//multiplicando para no afectar a la niebla
#if SHADER_API_GLCORE || SHADER_API_D3D11 || SHADER_API_METAL
#if ATTEN_METHOD_1 || ATTEN_METHOD_2 || ATTEN_METHOD_3

		if (DetailCascade2>0)
		{
			for (int k = 0; k < _LightsCount; k++)
			{
				half PointLightRange = 1 - (length(_LightPositions[k].xyz - VolumeSpaceCoords) * PointLightingDistance);//range clamp
				if (PointLightRange > .99) {
					if (_LightData[k].z >= 0.0)
					{

						PointLightAccum +=
							(Noise.a + VolumeFog.a)*SpotLight(
								_LightPositions[k].xyz,
								VolumeSpaceCoords,
								_LightRange(k),
								_LightColors[k],
								_LightIntensity(k),
								_LightRotations[k],

								_LightSpotAngle(k),
								_LightColors[k].w, i) * Contact;
					}
					else
					{
						PointLightAccum +=
							(Noise.a + VolumeFog.a)*PointLight(
								_LightPositions[k].xyz,
								VolumeSpaceCoords,
								_LightRange(k),
								_LightColors[k],
								_LightIntensity(k),
								//1,
								i) * Contact;
					}
				}
			}
			half atten = saturate(PointLightAccum.a);
			if (atten > 0)
			{

				PointLightsFinal = PointLightAccum;
			}
		}


#endif
#endif


#ifdef HALO

		//LightTerms += Halo.rgb;
		//3.1.10
		LightTerms *= (1 + Halo.rgb);
#endif

#ifdef DEBUG
		if (_DebugMode == DEBUG_ITERATIONS)
		{
			debugOutput.rgb = tex2Dlod(_PerformanceLUT, float4(0, (float)s / STEP_COUNT, 0, 0)).rgb;
		}
		if (_DebugMode == DEBUG_INSCATTERING)
		{
			LightTerms = Phase;
		}

		if (_DebugMode == DEBUG_VOLUMETRIC_SHADOWS)
		{
			LightTerms = LightShafts * .05;
		}
#if VOLUME_FOG && _VOLUME_FOG_INSCATTERING
		if (_DebugMode == DEBUG_VOLUME_FOG_INSCATTER_CLAMP)
		{
			LightTerms = OpacityTerms * VolumeFogInscatteringDistanceClamp;
		}
		if (_DebugMode == DEBUG_VOLUME_FOG_PHASE)
		{
			LightTerms = OpacityTerms * FogInscatter;
		}
#endif

#endif

		OpacityTerms *= _PushAlpha;
		//FinalNoise.a = saturate(FinalNoise.a);

		FinalNoise = FinalNoise + float4(LightTerms, OpacityTerms)  * (1.0 - FinalNoise.a);


		if (FinalNoise.a > .999)break;//KILL'EM ALL if its already opaque, don't do anything else
	}
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////LOOP END
#ifdef DEBUG

	if (_DebugMode == DEBUG_INSCATTERING
		|| _DebugMode == DEBUG_VOLUMETRIC_SHADOWS
		|| _DebugMode == DEBUG_VOLUME_FOG_INSCATTER_CLAMP
		|| _DebugMode == DEBUG_VOLUME_FOG_PHASE)
	{
		debugOutput = FinalNoise;

	}

	return debugOutput;
#endif
	//return FinalNoise;
	//return float4(Contact, Contact, Contact,1);
	//return float4(NoiseAtten, NoiseAtten, NoiseAtten, 1); ;
	//FinalNoise.rgb *= _Color.rgb;

	_Color = FinalNoise;
	_InscatteringColor *= FinalNoise;


#endif				

#if _INSCATTERING && !_FOG_VOLUME_NOISE && !_FOG_GRADIENT

	float Inscattering = Henyey(Normalized_CameraWorldDir, L, InscatteringShape);
	//_InscatteringIntensity *= .05;
	Inscattering *= InscatteringDistanceClamp;
	Final = float4(_Color.rgb + _InscatteringColor.rgb * _InscatteringIntensity * Inscattering, _Color.a);

#else

	Final = _Color;
#endif	
#if ATTEN_METHOD_1 || ATTEN_METHOD_2 || ATTEN_METHOD_3

	Final.rgb += PointLightsFinal.rgb;

#endif
#ifdef ColorAdjust


	Final.rgb = lerp(Final.rgb, pow(max((Final.rgb + Offset), 0), 1 / Gamma), Final.a);

#if _TONEMAP			
	Final.rgb = ToneMap(Final.rgb, Exposure);
#endif
#endif
#if !_FOG_VOLUME_NOISE && !_FOG_GRADIENT
	Final.a *= (Fog * _Color.a);
#endif

	if (IsGammaSpace())
		Final.rgb = pow(Final.rgb, 1 / 2.2);


	Final.a = saturate(Final.a);

	Final.rgb = SafeHDR(Final.rgb);

#ifdef Enviro_Integration
	float4 EnviroFog = TransparentFog(Final, i.Wpos, screenUV, Depth);
	EnviroFog.a = EnviroFog.a * Final.a;
	Final = EnviroFog;
#endif

	//Debug the build problem
#if DF && defined(DEBUG_PRIMITIVES)
	//Final.rgb = _PrimitiveCount/1.5; 
	//Final.a = 0;
	Final.rgb = float3(0,.5, .5);
#endif

	return Final;

}
#endif