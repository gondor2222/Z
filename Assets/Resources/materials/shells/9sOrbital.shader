// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/8sOrbital"
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

	float _Z;

	half4 LightingUnlit(SurfaceOutput s, half3 lightDir, half atten) {
		return half4(s.Albedo, s.Alpha);
	}

	float R90(float r) {
		float p = 2 * _Z * r / 9;
		float p2 = p*p;
		float p4 = p2 * p2;
		float poly = 362880 - 1451520*p + 1693440*p2 - 846720*p2*p + 211680 * p4 - 28224*p4*p + 2016*p4*p2 - 72*p4*p2*p + p4*p4;
		return _Z * exp(-p) * poly * poly / 729 / 729 / 108;
	}

	float s1(float x, float y, float z, float r) {
		return 1;
	}

	float3 rgb2hsb(float3 c) {
		float4 K = float4(0, -0.3333, 0.6666, -1);
		float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
		float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));
		float d = q.x - min(q.w, q.y);
		float e = 1e-10;
		return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
	}

	float3 hsb2rgb(float3 c) {
		float3 rgb = clamp(abs(fmod(c.x * 6 + float3(0, 4, 2), 6) - 3) - 1, 0, 1);
		rgb = rgb * rgb * (3 - 2 * rgb);
		return c.z * lerp(float3(1,1,1), rgb, c.y);
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

		for (i = 0; i < 100; i++) {
			pos = IN.worldPos - 2 * proj * i / 100.0 - unity_ObjectToWorld._m03_m13_m23;
			pos4 = mul(unity_WorldToObject, float4(pos.x, pos.y, pos.z, 0));
			r = length(pos4) * 7 * 81;
			red += R90(r) * s1(pos4.x, pos4.y, pos4.z, r) / 25;
		}
		c.r = saturate(red) * (0.5 + 0.5 * cos(10 * _Time));
		c.g = saturate(red) * (0.5 + 0.5 * cos(10 * _Time - 2 * 3.14159 / 3));
		c.b = saturate(red) * (0.5 + 0.5 * cos(10 * _Time + 2 * 3.14159 / 3));


		float3 c2 = rgb2hsb(float3(c.r, c.g, c.b));

		c = hsb2rgb(float3(c2.x, 1 , 1));
		c = hsb2rgb(float3(c2.x, 1, 0.5 / sqrt(0.299*c.r + 0.587*c.g + 0.114*c.b)));
		o.Albedo = half3(c.rgb);
		o.Alpha = half(saturate(red));
	}

	ENDCG


	}
		FallBack "Diffuse"
}