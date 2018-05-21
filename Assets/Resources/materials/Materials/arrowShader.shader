// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/arrowShader"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
		Lighting On
		ZWrite Off
		Blend One One

		CGPROGRAM
#pragma surface surf Unlit alpha
#include "UnityCG.cginc"
		struct Input
	{
		float3 worldPos;
		float3 viewDir;
		float4 uv : TEXCOORD0;
	};

	half3 _Color;

	half4 LightingUnlit(SurfaceOutput s, half3 lightDir, half atten) {
		return half4(s.Albedo, s.Alpha);
	}


	float s1(float x, float y, float z, float r) {
		return 1;
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

		c.r = _Color.r;
		c.g = _Color.g;
		c.b = _Color.b;
		o.Albedo = half3(c.rgb);
		o.Alpha = half(0.3f);
	}

	ENDCG


	}
		FallBack "Diffuse"
}