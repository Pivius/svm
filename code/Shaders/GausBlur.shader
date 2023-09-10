HEADER
{
	Description = "Gaussean blur";
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
	#define E 2.71828182846

	RenderState(DepthWriteEnable, false);
	RenderState(DepthEnable, false);

	CreateTexture2D(g_tColorBuffer) < Attribute("ColorBuffer");      SrgbRead(true); Filter(MIN_MAG_LINEAR_MIP_POINT); AddressU(MIRROR); AddressV(MIRROR); > ;
	CreateTexture2D(g_tDepthBuffer) < Attribute("DepthTexture");     SrgbRead(false); Filter(MIN_MAG_MIP_POINT); > ;


	float _BlurAmount < UiType(Slider); Default1(0.007); UiGroup("Blur,1"); > ;

	float3 GaussianBlur(Texture2D tColorBuffer, SamplerState sSampler, float2 vTexCoords, float2 flSize)
	{
		float fl2PI = 6.28318530718f;
		float flDirections = 16.0f;
		float flQuality = 4.0f;
		float flTaps = 1.0f;

		float3 vColor = Tex2DS(tColorBuffer, sSampler, vTexCoords).rgb;

		[unroll]
		for (float d = 0.0; d < fl2PI; d += fl2PI / flDirections)
		{
			[unroll]
			for (float j = 1.0 / flQuality; j <= 1.0; j += 1.0 / flQuality)
			{
				flTaps += 1;
				vColor += Tex2DS(tColorBuffer, sSampler, vTexCoords + float2(cos(d), sin(d)) * flSize * j).rgb;
			}
		}
		return vColor / flTaps;
	}

	SamplerState _Sampler < Filter(LINEAR); AddressU(WRAP); AddressV(WRAP); > ;

	float4 MainPs(PixelInput i) : SV_Target0
	{
		return float4(GaussianBlur(g_tColorBuffer, _Sampler, i.uv.xy, float2(_BlurAmount, _BlurAmount)), 0.0f);
	}
}
