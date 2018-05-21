// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/7pOrbital"
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

	float R71(float r) {
		float p = 2 * _Z * r / 7;
		float p2 = p*p;
		float poly = p * (6720 - 8400*p + 3360*p2 - 560*p*p2 + 40*p2*p2 - p2*p2*p);
		return _Z * exp(-p) * poly * poly / 18;
	}

	float p1(float x, float y, float z, float r) {
		float t = 1.732 * x / r;
		return t * t;
	}
	float p2(float x, float y, float z, float r) {
		float t = 1.732 * y / r;
		return t * t;
	}
	float p3(float x, float y, float z, float r) {
		float t = 1.732 * z / r;
		return t * t;
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
		float3 w1 = float3(0,0,0);
		float3 c1;
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
			r = length(pos4) * 7 * 49;
			w1 += 1.5 * float3(R71(r) * p1(pos4.x, pos4.y, pos4.z, r),
				R71(r) * p2(pos4.x, pos4.y, pos4.z, r),
				R71(r) * p3(pos4.x, pos4.y, pos4.z, r)
				)
				;
		}


		c1 = float3(0.5 + 0.5 * cos(10 * _Time.x),
			0.5 + 0.5 * cos(10 * _Time.x - 2 * 3.14159 / 3),
			0.5 + 0.5 * cos(10 * _Time.x + 2 * 3.14159 / 3));

		float3 c3 = float3(
			clamp(cos(30 * _Time.x)*0.7+0.3,0,1),
			clamp(cos(30 * _Time.x - 2 * 3.14159 / 3)*0.7+0.3,0,1),
			clamp(cos(30 * _Time.x + 2 * 3.14159 / 3)*0.7+0.3,0,1));
		float3 c2 = rgb2hsb(c1);
		float3 alph = c3 * w1;
		c = hsb2rgb(float3(c2.x, 1, 1));
		c = hsb2rgb(float3(c2.x, 1, 0.5 / sqrt(0.299*c.r + 0.587*c.g + 0.114*c.b)));
		o.Albedo = half3(c.rgb);
		o.Alpha = half(saturate(w1.x * c3.x + w1.y * c3.y + w1.z * c3.z));
	}

	ENDCG


	}
		FallBack "Diffuse"
}