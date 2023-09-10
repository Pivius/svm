
HEADER
{
	Description = "";
}

FEATURES
{
	#include "common/features.hlsl"
}

COMMON
{
	#include "common/shared.hlsl"

	#define S_UV2 1
}

struct VertexInput
{
#include "common/vertexinput.hlsl"
};

struct PixelInput
{
#include "common/pixelinput.hlsl"
};

VS
{
	#include "common/vertex.hlsl"

	PixelInput MainVs(VertexInput i)
	{
		PixelInput o = ProcessVertex(i);
		return FinalizeVertex(o);
	}
}

PS
{
	#include "sbox_pixel.fxc"
	#include "common/pixel.config.hlsl"
	#include "common/pixel.material.structs.hlsl"
	#include "common/pixel.lighting.hlsl"
	#include "common/pixel.shading.hlsl"
	#include "common/pixel.material.helpers.hlsl"
	#include "common/proceedural.hlsl"

	float3 _Color1 < UiType(Color); Default3(0, 0, 1); UiGroup("Color,1"); > ;
	float3 _Color2 < UiType(Color); Default3(0, 1, 1); UiGroup("Color,2"); > ;
	float3 _Color3 < UiType(Color); Default3(0, 1, 0); UiGroup("Color,3"); > ;
	float3 _Color4 < UiType(Color); Default3(1, 1, 0); UiGroup("Color,4"); > ;
	float3 _Color5 < UiType(Color); Default3(1, 0, 0); UiGroup("Color,5"); > ;

	float _BlueThreshold < UiType(Slider); Default1(0.1); UiGroup("Threshold,1"); > ;
	float _CyanThreshold < UiType(Slider); Default1(0.2); UiGroup("Threshold,2"); > ;
	float _GreenThreshold < UiType(Slider); Default1(0.3); UiGroup("Threshold,3"); > ;
	float _YellowThreshold < UiType(Slider); Default1(0.5); UiGroup("Threshold,4"); > ;
	float _RedThreshold < UiType(Slider); Default1(1.0); UiGroup("Threshold,5"); > ;

	float _CyanSmoothing < UiType(Slider); Default1(1); UiGroup("Smoothing,2"); > ;
	float _GreenSmoothing < UiType(Slider); Default1(2); UiGroup("Smoothing,3"); > ;
	float _YellowSmoothing < UiType(Slider); Default1(1); UiGroup("Smoothing,4"); > ;
	float _RedSmoothing < UiType(Slider); Default1(1); UiGroup("Smoothing,5"); > ;

	float4 MainPs(PixelInput i) : SV_Target0
	{
		Material m;
		m.Albedo = float3(1, 1, 1);
		m.Normal = TransformNormal(i, float3(0, 0, 1));
		m.Roughness = 1;
		m.Metalness = 1;
		m.AmbientOcclusion = 1;
		m.TintMask = 1;
		m.Opacity = 1;
		m.Emission = float3(0, 0, 0);
		m.Transmission = 1;

		float3 blue = float3(0, 0, 1);
		float3 green = float3(0, 1, 0);
		float3 cyan = green + blue;
		float3 red = float3(1, 0, 0);
		float3 yellow = red + float3(0, 1, 0);
		float3 vDirectionWs = CalculatePositionToCameraDirWs(i.vPositionWithOffsetWs + g_vCameraPositionWs);
		float fresnel = saturate(dot(m.Normal, vDirectionWs));
		float3 thresholdColor = lerp(_Color1, _Color2, smoothstep(_BlueThreshold, _CyanThreshold, fresnel * _CyanSmoothing));;

		thresholdColor = lerp(thresholdColor, _Color3, smoothstep(_CyanThreshold, _GreenThreshold, fresnel * _GreenSmoothing));
		thresholdColor = lerp(thresholdColor, _Color4, smoothstep(_GreenThreshold, _YellowThreshold, fresnel * _YellowSmoothing));
		thresholdColor = lerp(thresholdColor, _Color5, smoothstep(_YellowThreshold, _RedThreshold, fresnel * _RedSmoothing));

		m.Albedo = thresholdColor;
		m.Emission = thresholdColor;

		ShadingModelValveStandard sm;
		return FinalizePixelMaterial(i, m, sm);
	}
}
