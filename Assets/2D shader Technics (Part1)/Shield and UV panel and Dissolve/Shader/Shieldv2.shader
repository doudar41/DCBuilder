/*
VERY IMPORTANT:
-> click on the Texture
-> Set Texture Type to Sprite (2D and UI)
-> Set Wrap Mode to Repeat

This is just a noisy effect I like :)
*/

Shader "Custom/Shieldv2"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	_NoiseTex("Texture", 2D) = "white" {}
	_BurnGradient("Gradient Pic",2D) = "white" {}
	_DissolveValue("Dissolve", Range(0.0,1.0)) = 1.0
		_GradientAdjust("Gradient Level", Range(0,5)) = 1

	}
		SubShader
	{
		ZWrite OFF // csak Transparent objektumnal legyen OFF
		Tags
	{
		"Queue" = "Transparent"
		"RenderType" = "Transparent"
		"IgnoreProjector" = "True"
	}
		LOD 100
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
	{
		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
		// make fog work
#pragma multi_compile_fog

#include "UnityCG.cginc"

		struct appdata
	{
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
	};

	struct v2f
	{
		float2 uv : TEXCOORD0;
		//UNITY_FOG_COORDS(1)
		float4 vertex : SV_POSITION;
	};

	sampler2D _MainTex;
	float4 _MainTex_ST;
	sampler2D _NoiseTex;
	float _DissolveValue;
	sampler2D _BurnGradient;
	float _GradientAdjust;

	v2f vert(appdata v)
	{
		v2f o;
		o.uv = TRANSFORM_TEX(v.uv, _MainTex).xy;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv.y += _Time[1];
		//o.vertex.y += sin(_Time.w);

		float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

		o.vertex.y *= sin(worldPos.x * floor(sin(_Time[1]) / cos(_Time[2])) ) * sin(_Time[3]) ;
		return o;
	}

	fixed4 frag(v2f i) : COLOR
	{
		fixed4 mainTex = tex2D(_MainTex, i.uv);
	fixed noiseVal = tex2D(_NoiseTex, i.uv).r;

	fixed d = (2.0 * _DissolveValue + noiseVal) - 1.0;

	fixed overOne = saturate(d * _GradientAdjust); // 0 es 1 kozé esik clamp(0,1);
	fixed4 burn = tex2D(_BurnGradient, float2(overOne, 0.5));

	mainTex *= (burn);
	if (overOne<0.5)
		mainTex.a = overOne;

	return mainTex;
	}

		ENDCG
	}
	}
}
