// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/8pOrbital"
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

	float R10(float r) {
		float p = 2 * _Z * r;
		return _Z * exp(-p) * 27;
	}
	float R20(float r) {
		float p = _Z * r;
		return _Z * exp(-p) * (2 - p) * (2 - p) * 3;
	}
	float R21(float r) {
		float p = _Z * r;
		return _Z * exp(-p) * p * p * 27 * 27 * 9;
	}
	float R30(float r) {
		float p = 2 * _Z * r / 3;
		return _Z * exp(-p) * (6 - 6 * p + p*p) * (6 - 6 * p + p*p) / 3;
	}
	float R31(float r) {
		float p = 2 * _Z * r / 3;
		return _Z * exp(-p) * (4 - p) * (4 - p) * p * p * 27 * 27 * 3;
	}
	float R32(float r) {
		float p = 2 * _Z * r / 3;
		return _Z * exp(-p) * p * p * p * p * 27 * 27 * 27 * 256;
	}
	float R40(float r) {
		float p = _Z * r / 2;
		return _Z * exp(-p) * (24 - 36 * p + 12 * p*p - p*p*p) * (24 - 36 * p + 12 * p*p - p*p*p) / 64;
	}
	float R41(float r) {
		float p = _Z * r / 2;
		return _Z *  exp(-p) * (20 - 10 * p + p*p) * (20 - 10 * p + p*p) * p * p * 27 * 3;
	}
	float R42(float r) {
		float p = _Z * r / 2;
		return _Z * exp(-p) * (6 - p) * (6 - p) * p * p * p * p * 243 * 243 * 12;
	}
	float R43(float r) {
		float p = _Z * r / 2;
		float p2 = p * p;
		return _Z * exp(-p) * p2 * p2 * p2 * 243 * 243 * 243 * 27 * 4;
	}
	float R50(float r) {
		float p = 2 * _Z * r / 5;
		float poly = (120 - 240 * p + 120 * p*p - 20 * p*p*p + p*p*p*p);
		return _Z * exp(-p) * poly * poly / 27 / 2;
	}
	float R51(float r) {
		float p = 2 * _Z * r / 5;
		float poly = (120 - 90 * p + 18 * p * p - p * p * p) * p;
		return _Z * exp(-p) * poly * poly * 9;
	}
	float R52(float r) {
		float p = 2 * _Z * r / 5;
		float poly = (42 - 14 * p + p*p) * p * p;
		return _Z * exp(-p) * poly * poly * 243 * 243 * 6;
	}
	float R53(float r) {
		float p = 2 * _Z * r / 5;
		float poly = (8 - p)*p*p*p;
		return _Z * exp(-p) * poly * poly * 243 * 243 * 243 * 81;
	}
	float R54(float r) {
		float p = 2 * _Z * r / 5;
		float poly = p * p * p * p;
		return _Z * exp(-p) * poly * poly * 243 * 243;
	}
	float R60(float r) {
		float p = _Z * r / 3;
		float poly = (720 - 1800 * p + 1200 * p*p - 300 * p*p*p + 30 * p*p*p*p - p*p*p*p*p);
		return _Z * exp(-p) * poly * poly / 243 / 5;
	}
	float R61(float r) {
		float p = _Z * r / 3;
		float poly = p * (840 - 840 * p + 252 * p*p - 28 * p*p*p + p*p*p*p);
		return _Z * exp(-p) * poly * poly;
	}
	float R62(float r) {
		float p = _Z * r / 3;
		float poly = p*p * (336 - 168 * p + 24 * p*p - p*p*p);
		return _Z * exp(-p) * poly * poly * 243 * 243;
	}
	float R63(float r) {
		float p = _Z * r / 3;
		float poly = p * p * p * (72 - 18 * p + p*p);
		return _Z * exp(-p) * poly * poly * 243 * 243 * 243 * 18;
	}
	float R70(float r) {
		float p = 2 * _Z * r / 7;
		float p2 = p*p;
		float poly = (5040 - 15120 * p + 12600 * p2 - 4200 * p*p2 + 630 * p2*p2 - 42 * p2*p2*p + p2*p2*p2);
		return _Z * exp(-p) * poly * poly / 243 / 243;
	}
	float R71(float r) {
		float p = 2 * _Z * r / 7;
		float p2 = p*p;
		float poly = p * (6720 - 8400 * p + 3360 * p2 - 560 * p*p2 + 40 * p2*p2 - p2*p2*p);
		return _Z * exp(-p) * poly * poly / 18;
	}
	float R72(float r) {
		float p = 2 * _Z * r / 7;
		float p2 = p * p;
		float poly = p * p * (3024 - 2016 * p + 432 * p2 - 36 * p2*p + p2*p2);
		return _Z * exp(-p) * poly * poly * 243 * 27;
	}
	float R80(float r) {
		float p = _Z * r / 4;
		float p2 = p*p;
		float p4 = p2 * p2;
		float poly = 40320 - 141120 * p + 141120 * p2 - 58800 * p2*p + 11760 * p4 - 1176 * p4*p + 56 * p4*p2 - p4*p2*p;
		return _Z * exp(-p) * poly * poly / 243 / 243 / 27 / 2;
	}
	float R81(float r) {
		float p = _Z * r / 4;
		float p2 = p*p;
		float p4 = p2*p2;
		float poly = p * (60480 - 90720 * p + 45360 * p2 - 10080 * p2*p + 1080 * p4 - 54 * p4*p + p4*p2);
		return _Z * exp(-p) * poly * poly / 243 / 2;
	}
	float R90(float r) {
		float p = 2 * _Z * r / 9;
		float p2 = p*p;
		float p4 = p2 * p2;
		float poly = 362880 - 1451520 * p + 1693440 * p2 - 846720 * p2*p + 211680 * p4 - 28224 * p4*p + 2016 * p4*p2 - 72 * p4*p2*p + p4*p4;
		return _Z * exp(-p) * poly * poly / 243 / 243 / 27 / 81 / 1.0;
	}

	float s1(float x, float y, float z, float r) {
		return 1;
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
	float d1(float x, float y, float z, float r) { //xy
		float t = 3.873 * x * y / r / r;
		return t * t;
	}
	float d2(float x, float y, float z, float r) { //xz
		float t = 3.873 * x * z / r / r;
		return t * t;
	}
	float d3(float x, float y, float z, float r) { //yz
		float t = 3.873 * y * z / r / r;
		return t * t;
	}
	float d4(float x, float y, float z, float r) { //z^2
		float t = 1.118 * (-x*x - y*y + 2 * z*z) / (r * r); //1/sqrt(12)
		return t * t;
	}
	float d5(float x, float y, float z, float r) { //x^2-y^2
		float t = 1.936 * (x*x - y*y) / (r*r);
		return t * t;
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
	}

	ENDCG


	}
		FallBack "Diffuse"
}