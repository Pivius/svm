HEADER
{
	Description = "Night vision";
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

	RenderState(DepthWriteEnable, false);
	RenderState(DepthEnable, false);

	CreateTexture2D(g_tColorBuffer) < Attribute("ColorBuffer");      SrgbRead(true); Filter(MIN_MAG_LINEAR_MIP_POINT); AddressU(MIRROR); AddressV(MIRROR); > ;
	CreateTexture2D(g_tDepthBuffer) < Attribute("DepthTexture");     SrgbRead(false); Filter(MIN_MAG_MIP_POINT); > ;

	float FetchDepth(float2 vTexCoord, float flProjectedDepth)
	{
		flProjectedDepth = RemapValClamped(flProjectedDepth, g_flViewportMinZ, g_flViewportMaxZ, 0, 1);

		float flZScale = g_vInvProjRow3.z;
		float flZTran = g_vInvProjRow3.w;

		float flDepthRelativeToRayLength = 1.0 / ((flProjectedDepth * flZScale + flZTran));

		return flDepthRelativeToRayLength;
	}

	float4 MainPs(PixelInput i) : SV_Target0
	{
		float4 tex = Tex2D(g_tDepthBuffer, i.uv.xy);
		float centerDepth = FetchDepth(i.uv.xy, tex.r);

		// float3 flProjectedDepth = Tex2D( g_tDepthBuffer, screenUv);

		float4 color = Tex2D(g_tColorBuffer, i.uv);
		float luminance = Luminance(color.rgb);

		color.rgb = dot(color.rgb, float3(0.57, 0.57, 0.57));

		float4 deferredDiffuse = RemapValClamped(centerDepth, g_flViewportMinZ, 50000, 0.0, 3.0);

		color.rgb += deferredDiffuse.rgb * (luminance + 0.8);
		color.rb = max(color.r - 0.02, 0);
		return color;
	}
}
