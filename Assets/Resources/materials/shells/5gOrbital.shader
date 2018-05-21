// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/5gOrbital"
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

	float R54(float r) {
		float p = 2 * _Z * r / 5;
		float poly = p * p * p * p;
		return _Z * exp(-p) * poly * poly;
	}

	float g1(float x, float y, float z, float r) { //xy(x^2-y^2)
		float t = 8.874*x*y*(x*x-y*y) / (r*r*r*r);
		return t * t * 4;
	}
	float g2(float x, float y, float z, float r) { //zy^3
		float t = 6.275*(3*x*x - y*y)*y*z / (r*r*r*r);
		return t * t * 5;
	}
	float g3(float x, float y, float z, float r) { //z^2xy
		float t = 3.354*x*y*(7*z*z-r*r) / (r*r*r*r);
		return 4 * t * t;
	}
	float g4(float x, float y, float z, float r) { //z^3y
		float t = y*z*(7*z*z-3*r*r) / (r*r*r*r);
		return 15* t * t;
	}
	float g5(float x, float y, float z, float r) { //z^4
		float t = (35*z*z/r*z*z/r-30*z*z+3*r*r) / (r*r);
		return t / 1.5 *  t;
	}
	float g6(float x, float y, float z, float r) { //z^3x
		float t = x*z*(7*z*z-3*r*r) / (r*r*r*r);
		return t * t * 25;
	}
	float g7(float x, float y, float z, float r) { //z^2xy
		float t = 1.677*(x*x-y*y)*(7*z*z-r*r) / (r*r*r*r);
		return t * t * 5;
	}
	float g8(float x, float y, float z, float r) { //zx^3
		float t = (x*x - 3 * y*y)*x*z / (r*r*r*r);
		return t * t * 1.5e2;
	}
	float g9(float x, float y, float z, float r) { //x^4+y^4
		float t = (x*x*(x*x - 3 * y*y) - y*y*(3 * x*x - y*y)) / (r*r*r*r);
		return t * t * 2e1;
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
		float w[9] = { 0,0,0,0,0,0,0,0,0 };
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
			w[0] += 8e-7 * R54(r) * g1(pos4.x * 175, pos4.y * 175, pos4.z * 175, r);
			w[1] += 8e-7 * R54(r) * g2(pos4.x * 175, pos4.y * 175, pos4.z * 175, r);
			w[2] += 8e-7 * R54(r) * g3(pos4.x * 175, pos4.y * 175, pos4.z * 175, r);
			w[3] += 8e-7 * R54(r) * g4(pos4.x * 175, pos4.y * 175, pos4.z * 175, r);
			w[4] += 8e-7 * R54(r) * g5(pos4.x * 175, pos4.y * 175, pos4.z * 175, r);
			w[5] += 8e-7 * R54(r) * g6(pos4.x * 175, pos4.y * 175, pos4.z * 175, r);
			w[6] += 8e-7 * R54(r) * g7(pos4.x * 175, pos4.y * 175, pos4.z * 175, r);
			w[7] += 8e-7 * R54(r) * g8(pos4.x * 175, pos4.y * 175, pos4.z * 175, r);
			w[8] += 8e-7 * R54(r) * g9(pos4.x * 175, pos4.y * 175, pos4.z * 175, r);
		}


		c1 = float3(0.5 + 0.5 * cos(10 * _Time.x),
			0.5 + 0.5 * cos(10 * _Time.x - 2 * 3.14159 / 3),
			0.5 + 0.5 * cos(10 * _Time.x + 2 * 3.14159 / 3));

		float c3[9] = {
			clamp(cos(70 * _Time.x                  ) * 3 - 2,0,1),
			clamp(cos(70 * _Time.x + 2 * 3.14159 / 9) * 3 - 2,0,1),
			clamp(cos(70 * _Time.x + 4 * 3.14159 / 9) * 3 - 2,0,1),
			clamp(cos(70 * _Time.x + 6 * 3.14159 / 9) * 3 - 2,0,1),
			clamp(cos(70 * _Time.x + 8 * 3.14159 / 9) * 3 - 2,0,1),
			clamp(cos(70 * _Time.x + 10 * 3.14159 / 9) * 3 - 2,0,1),
			clamp(cos(70 * _Time.x + 12 * 3.14159 / 9) * 3 - 2,0,1),
			clamp(cos(70 * _Time.x + 14 * 3.14159 / 9) * 3 - 2,0,1),
			clamp(cos(70 * _Time.x + 16 * 3.14159 / 9) * 3 - 2,0,1)
		};
		float3 c2 = rgb2hsb(c1);
		c = hsb2rgb(float3(c2.x, 1, 1));
		c = hsb2rgb(float3(c2.x, 1, 0.5 / sqrt(0.299*c.r + 0.587*c.g + 0.114*c.b)));
		o.Albedo = half3(c.rgb);
		o.Alpha = 0;
		for (i = 0; i < 9; i++) {
			o.Alpha += w[i] * c3[i];
		}
	}

	ENDCG


	}
		FallBack "Diffuse"
}