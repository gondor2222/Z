Shader "materials/fOrbital"
{
	Properties
	{
		_Transparency("Transparency", Range(0.05, 1.0)) = 0.1
		_MainTex("Texture", 2D) = "white" {}
		_Phase("Phase", Range(0, 6.283)) = 0
	}
	SubShader
	{
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
		Lighting On
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		CGPROGRAM
		#pragma surface surf Unlit alpha
		#include "UnityCG.cginc"
		struct Input
		{
			float2 uv_MainTex;
			float3 worldNormal;
			half3 viewDir;
		};

		half _Transparency;
		half _Phase;

		half4 LightingUnlit(SurfaceOutput s, half3 lightDir, half atten) {
			return half4(s.Albedo, s.Alpha);
		}

		void surf(Input IN, inout SurfaceOutput o)
		{
			half3 c = 0;
			half theta = IN.uv_MainTex.y * 3.14159;
			half phi = IN.uv_MainTex.x * 6.2832 - 3.14159;
			half st = sin(theta);
			half ct = cos(theta);
			half cp = cos(phi);
			half sp = sin(phi);
			half r = (4 * ct*ct - st*st)*st*cp;
			half g = (4 * ct*ct - st*st)*st*sp;
			half b = sp*sp*sp*cos(3 * phi);

			c.r = 0.7 + 0.3 * r * cos(150 * _Time + _Phase);
			c.g = 0.7 + 0.3 * g * cos(150 * _Time + 3.142 * 2 / 3 + _Phase);
			c.b = 0.7 + 0.3 * b * cos(150 * _Time + 3.142 * 4 / 3 + _Phase);
			o.Albedo = c.rgb;
			float dotProduct = clamp(dot(normalize(IN.viewDir), IN.worldNormal), 0, 1);
			dotProduct = dotProduct;
			o.Alpha = _Transparency * dotProduct * dotProduct;
		}
		ENDCG
	}
	FallBack "Diffuse"
}