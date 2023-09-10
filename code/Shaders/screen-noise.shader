HEADER
{
	Description = "Screen noise";
	DevShader = true;
}

MODES
{
	Default();
	VrForward();
}

FEATURES
{
}

COMMON
{
	#include "postprocess/shared.hlsl"
}

struct VertexInput
{
	float3 vertex : POSITION < Semantic(PosXyz); > ;
	float2 uv : TEXCOORD0 < Semantic(LowPrecisionUv); > ;
};

struct PixelInput
{
	float2 uv : TEXCOORD0;

	// VS only
#if ( PROGRAM == VFX_PROGRAM_VS )
	float4 vPositionPs		: SV_Position;
#endif

	// PS only
#if ( ( PROGRAM == VFX_PROGRAM_PS ) )
	float4 vPositionSs		: SV_ScreenPosition;
#endif
};

VS
{
	float CalculateProjectionDepthFromViewDepth(float flViewDepth)
	{
		float flZScale = g_vInvProjRow3.z;
		float flZTran = g_vInvProjRow3.w;
		return (1.0 / flViewDepth - flZTran) / flZScale;
	}

	PixelInput MainVs(VertexInput i)
	{
		PixelInput o;
		o.vPositionPs = float4(i.vertex.xyz, 1.0f);
		//o.vPositionPs.z = RemapValClamped(CalculateProjectionDepthFromViewDepth(flDOFFocusPlane), 0.0, 1.0,  g_flViewportMinZ, g_flViewportMaxZ);
		o.uv = i.uv;
		return o;
	}
}

PS
{
	#include "postprocess/common.hlsl"
	#include "msaa_offsets.fxc"
	#include "common/msaa.hlsl"
	#define PI 3.14159265359

	RenderState(DepthWriteEnable, false);
	RenderState(DepthEnable, false);

	CreateTexture2D(g_tColorBuffer) < Attribute("ColorBuffer");      SrgbRead(true); Filter(MIN_MAG_LINEAR_MIP_POINT); AddressU(MIRROR); AddressV(MIRROR); > ;
	CreateTexture2D(g_tDepthBuffer) < Attribute("DepthTexture");     SrgbRead(false); Filter(MIN_MAG_MIP_POINT); > ;
	CreateTexture2D(g_tNoiseTexture) < Attribute("NoiseTexture");    SrgbRead(true); Filter(MIN_MAG_LINEAR_MIP_POINT); AddressU(MIRROR); AddressV(MIRROR); > ;

	float _NoiseScale < UiType(Slider); Default1(10.0); UiGroup("Noise,1"); > ;
	float _NoiseSpeed < UiType(Slider); Default1(2.0); UiGroup("Noise,2"); > ;
	float _NoiseIntensity < UiType(Slider); Default1(0.1); UiGroup("Noise,3"); > ;
	float _NoiseOpacity < UiType(Slider); Default1(0.1); UiGroup("Noise,4"); > ;

	float4 MainPs(PixelInput i) : SV_Target0
	{
		float2 noiseUV = float2(i.uv.x * _NoiseScale, i.uv.y * _NoiseScale) + float2(g_flTime * _NoiseSpeed, g_flTime * _NoiseSpeed);
		//float noise = Tex2D(g_tNoiseTexture, noiseUV).r;

		//float color = (noise * _NoiseIntensity + (1.0 - _NoiseIntensity));

		float2 uv = float2(_NoiseSpeed * sin(g_flTime * 0.5), _NoiseSpeed * cos(g_flTime * 0.5));
		float4 color = Tex2D(g_tColorBuffer, i.uv);
		float4 noise = Tex2D(g_tNoiseTexture, (noiseUV * 3.5) + uv);

		color += noise * _NoiseOpacity;
		return color ;
	}
}
