
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

	PixelInput MainVs( VertexInput i )
	{
		PixelInput o = ProcessVertex( i );
		return FinalizeVertex( o );
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


	float4 MainPs( PixelInput i ) : SV_Target0
	{
		Material m;
		m.Albedo = float3( 1, 1, 1 );
		m.Normal = TransformNormal( i, float3( 0, 0, 1 ) );
		m.Roughness = 1;
		m.Metalness = 0;
		m.AmbientOcclusion = 1;
		m.TintMask = 1;
		m.Opacity = 1;
		m.Emission = float3( 0, 0, 0 );
		m.Transmission = 1;

		float4 local0 = float4( 0.013528103, 0.013528103, 0.1213873, 1 );
		float4 local1 = float4( 0.3256755, 0.34822816, 0.517341, 1 );
		float2 local2 = i.vTextureCoords.xy * float2( 1, 1 );
		float4 local3 = float4( local2.xy, 0, 0 ) - i.vPositionSs;
		float local4 = length( local3 );
		float local5 = local4 / 5000;
		float local6 = max( local5, 0 );
		float local7 = min( local6, 1 );
		float4 local8 = lerp( local0, local1, local7 );
		float local9 = 0.5;
		float4 local10 = float4( 0, 0, 0, 1 );

		m.Albedo = local8.xyz;
		m.Emission = local8.xyz;
		m.Metalness = local9;
		m.AmbientOcclusion = local10.x;

		ShadingModelValveStandard sm;
		return FinalizePixelMaterial( i, m, sm );
	}
}
