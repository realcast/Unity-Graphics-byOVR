float3 SampleSpecularBRDF(BSDFData bsdfData, float2 theSample, float3 viewWS)
{
    float roughness = PerceptualRoughnessToRoughness(bsdfData.perceptualRoughness);
    float3x3 localToWorld = GetLocalFrame(bsdfData.normalWS);

    float NdotL, NdotH, VdotH;
    float3 sampleDir;
    SampleGGXDir(theSample, viewWS, localToWorld, roughness, sampleDir, NdotL, NdotH, VdotH);
    return sampleDir;
}

#ifdef HAS_LIGHTLOOP

IndirectLighting EvaluateBSDF_RaytracedReflection(LightLoopContext lightLoopContext,
                                                  BSDFData bsdfData,
                                                  PreLightData preLightData,
                                                  float3 reflection)
{
    IndirectLighting lighting;
    ZERO_INITIALIZE(IndirectLighting, lighting);

    float3 reflectanceFactor = (float3)0.0;
    if (HasClearcoat())
    {
        reflectanceFactor = GetSSRDimmer() * bsdfData.clearcoatColor * preLightData.coatFGD;
    }
    else
    {
#if defined(_AXF_BRDF_TYPE_SVBRDF)
        reflectanceFactor = preLightData.specularFGD;
#elif defined(_AXF_BRDF_TYPE_CAR_PAINT)
        for (uint lobeIndex = 0; lobeIndex < CARPAINT2_LOBE_COUNT; lobeIndex++)
        {
            float coeff = _CarPaint2_CTCoeffs[lobeIndex];
            reflectanceFactor += coeff * GetCarPaintSpecularFGDForLobe(preLightData, lobeIndex);
        }
        // TODO_flakes ? most carpaints have coat anyway and if we're called here, we're already
        // at 2 bounces of indirect specular path (reflection).
        // See (and cf with) FitToStandardLit() that uses GetBaseSurfaceColorAndF0() and the mixFlakes option:
        // The whole carpaint model is fitted, so here we just add the flakes component.
        // With FitToStandardLit(), we need to hack-in the flakes into the diffuse color or f0,
        // so for the later we are constrained by the f0 < 1 limit, and we have to mix it in instead, but not here.
        //reflectanceFactor += preLightData.singleFlakesComponent;
#else
        // This is only possible if the AxF is a BTF type. However, there is a bunch of ifdefs do not support this third case
#endif
        reflectanceFactor *= GetSSRDimmer();
    }

    lighting.specularReflected = reflection.rgb * reflectanceFactor;
    return lighting;
}

IndirectLighting EvaluateBSDF_RaytracedRefraction(LightLoopContext lightLoopContext,
                                                  PreLightData preLightData,
                                                  float3 transmittedColor)
{
    IndirectLighting lighting;
    ZERO_INITIALIZE(IndirectLighting, lighting);
    return lighting;
}

float RecursiveRenderingReflectionPerceptualSmoothness(BSDFData bsdfData)
{
    return RoughnessToPerceptualSmoothness(GetScalarRoughness(bsdfData.roughness));
}
#endif

#if (SHADERPASS == SHADERPASS_RAYTRACING_GBUFFER)
void FitToStandardLit( BSDFData bsdfData
                        , BuiltinData builtinData
                        , uint2 positionSS
                        , out StandardBSDFData outStandardlit)
{
    outStandardlit.specularOcclusion = bsdfData.specularOcclusion;
    outStandardlit.normalWS = bsdfData.normalWS;
    outStandardlit.baseColor = bsdfData.diffuseColor;
    outStandardlit.fresnel0 = bsdfData.specularColor;
    outStandardlit.perceptualRoughness = bsdfData.perceptualRoughness;
    outStandardlit.coatMask = 0;
    outStandardlit.emissiveAndBaked = builtinData.bakeDiffuseLighting * bsdfData.specularColor * bsdfData.ambientOcclusion + builtinData.emissiveColor;
    outStandardlit.isUnlit = 0;
}
#endif
