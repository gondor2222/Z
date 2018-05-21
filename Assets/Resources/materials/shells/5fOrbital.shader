// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/5fOrbital"
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

	float R53(float r) {
		float p = 2 * _Z * r / 5;
		float poly = (8-p)*p*p*p;
		return _Z * exp(-p) * poly * poly * 3;
	}

	float f1(float x, float y, float z, float r) { //y(3x^2-y^2)
		float t = 2.09165*(3 * x*x - y*y)*y / (r*r*r);
		return t * t;
	}
	float f2(float x, float y, float z, float r) { //xyz
		float t = 10.2470*x*y*z / (r*r*r); //sqrt(24)
		return t * t;
	}
	float f3(float x, float y, float z, float r) { //yz^2
		float t = 1.6202*y*(4 * z*z - x*x - y*y) / (r*r*r); //sqrt(3/5)
		return t * t;
	}
	float f4(float x, float y, float z, float r) { //z^3
		float t = 1.3229*z*(2 * z*z - 3 * x*x - 3 * y*y) / (r*r*r); //sqrt(2/5)
		return t * t;
	}
	float f5(float x, float y, float z, float r) { //xz^2
		float t = 1.6202*x*(4 * z*z - x*x - y*y) / (r*r*r); //sqrt(3/5)
		return t * t;
	}
	float f6(float x, float y, float z, float r) { //z(x^2-y^2)
		float t = 5.123*(x*x - y*y)*z / (r*r*r); //sqrt(6)
		return t * t;
	}
	float f7(float x, float y, float z, float r) { //x(x^2-3y^2)
		float t = 2.092*(x*x - 3 * y*y)*x / (r*r*r);
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
		float w[7] = { 0,0,0,0,0,0, 0 };
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
			r = length(pos4) * 7 * 25;
			w[0] += 2e8 * R53(r) * f1(pos4.x, pos4.y, pos4.z, r);
			w[1] += 2e8 * R53(r) * f2(pos4.x, pos4.y, pos4.z, r);
			w[2] += 2e8 * R53(r) * f3(pos4.x, pos4.y, pos4.z, r);
			w[3] += 2e8 * R53(r) * f4(pos4.x, pos4.y, pos4.z, r);
			w[4] += 2e8 * R53(r) * f5(pos4.x, pos4.y, pos4.z, r);
			w[5] += 2e8 * R53(r) * f6(pos4.x, pos4.y, pos4.z, r);
			w[6] += 2e8 * R53(r) * f7(pos4.x, pos4.y, pos4.z, r);
		}


		c1 = float3(0.5 + 0.5 * cos(10 * _Time.x),
			0.5 + 0.5 * cos(10 * _Time.x - 2 * 3.14159 / 3),
			0.5 + 0.5 * cos(10 * _Time.x + 2 * 3.14159 / 3));

		float c3[7] = {
			clamp(cos(50 * _Time.x),0,1),
			clamp(cos(50 * _Time.x - 2 * 3.14159 / 7),0,1),
			clamp(cos(50 * _Time.x - 4 * 3.14159 / 7),0,1),
			clamp(cos(50 * _Time.x - 6 * 3.14159 / 7),0,1),
			clamp(cos(50 * _Time.x + 2 * 3.14159 / 7),0,1),
			clamp(cos(50 * _Time.x + 4 * 3.14159 / 7),0,1),
			clamp(cos(50 * _Time.x + 6 * 3.14159 / 7),0,1)
		};
		float3 c2 = rgb2hsb(c1);
		c = hsb2rgb(float3(c2.x, 1, 1));
		c = hsb2rgb(float3(c2.x, 1, 0.5 / sqrt(0.299*c.r + 0.587*c.g + 0.114*c.b)));
		o.Albedo = half3(c.rgb);
		o.Alpha = 0;
		for (i = 0; i < 7; i++) {
			o.Alpha += w[i] * c3[i];
		}
	}

	ENDCG


	}
		FallBack "Diffuse"
}