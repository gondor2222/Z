// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/VelocityShader"
{
	Properties
	{
		_Z("Z", Float) = 1
	}
		SubShader
	{
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
		Lighting On
		ZWrite Off
		Blend One Zero

		CGPROGRAM
#pragma surface surf Unlit alpha
#include "UnityCG.cginc"
		struct Input
	{
		float3 worldPos;
		float3 viewDir;
		float4 uv : TEXCOORD0;
	};

	float _Z;

	half4 LightingUnlit(SurfaceOutput s, half3 lightDir, half atten) {
		return half4(s.Albedo, s.Alpha);
	}

	void surf(Input IN, inout SurfaceOutput o)
	{

		float3 c = 0;
		float red = 0;
		float3 radius = IN.worldPos - unity_ObjectToWorld._m03_m13_m23;
		float3 proj = 2 * dot(radius, normalize(IN.viewDir)) * normalize(IN.viewDir);
		float3 pos;
		float4 pos4;
		float normalized = length(proj);
		float r;
		int i;

		float4 pos4start = mul(unity_WorldToObject, float4(IN.worldPos.x, IN.worldPos.y, IN.worldPos.z, 0));

		for (i = 0; i < 100; i++) {
			pos = IN.worldPos - 2 * proj * i / 100.0 - unity_ObjectToWorld._m03_m13_m23;
			pos4 = mul(unity_WorldToObject, float4(pos.x, pos.y, pos.z, 0));
			r = length(pos4);
			red += sin(r*30 - 150*_Time) / 5 / exp(20 * r * r);
		}

		o.Albedo = half3(1,1,1);
		o.Alpha = half(saturate(red));
	}

	ENDCG


	}
		FallBack "Diffuse"
}