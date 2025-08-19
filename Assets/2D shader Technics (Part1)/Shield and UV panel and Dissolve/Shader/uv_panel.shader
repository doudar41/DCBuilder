/*
VERY IMPORTANT:
-> click on the Texture
-> Set Texture Type to Sprite (2D and UI)
-> Set Wrap Mode to Repeat
*/

Shader "Custom/UV-Panel"
{
	Properties
	{
		_Color("Color Overlay",Color) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}
	    _Transparency("Alpha",Range(0,1)) = 1
		_Speed("Speed",Range(0.1,10)) = 1

	}
	SubShader
	{
		ZWrite OFF // csak Transparent objektumnal legyen OFF
        Tags 
		{
			"Queue"="Transparent" 
			"RenderType"="Transparent" 
			"IgnoreProjector"="True" 
		}
		LOD 100
			Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _Speed;
			float _Transparency;
			float4 _Color;

			v2f vert (appdata v)
			{
				v2f o;
				o.uv = TRANSFORM_TEX(v.uv, _MainTex).xy;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv.x += _Time[1]*_Speed;
				return o;
			}

			fixed4 frag (v2f i) :  COLOR
			{
			   fixed4 mainTex = tex2D(_MainTex, i.uv);
			   mainTex.a = _Transparency;
			   return mainTex * _Color;
			}

			ENDCG
		}
	}
}
