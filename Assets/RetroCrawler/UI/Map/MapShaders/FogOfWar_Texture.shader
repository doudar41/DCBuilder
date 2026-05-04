Shader "Custom/FogOfWar_Texture"
{
    Properties
    {
        _MainTex ("MapTexture", 2D) = "white" {}
        _VisibilityTex ("Visibility", 2D) = "black" {}
        _ExplorationTex ("Exploration", 2D) = "black" {}

        _WorldOrigin ("World Origin", Vector) = (0,0,0,0)
        _WorldSize ("World Size", Vector) = (10,0,10,0)

        _FogColor ("Fog Color", Color) = (0,0,0,1)
        _ExploredColor ("Explored Tint", Color) = (0.2,0.2,0.2,1)
    }

    SubShader
    {
        Tags { "Queue"="Transparent" }
        ZWrite Off
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _VisibilityTex;
            sampler2D _ExplorationTex;

            float4 _FogColor;
            float4 _ExploredColor;

            float4 _WorldOrigin;
            float4 _WorldSize;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 world : TEXCOORD0;
                float2 uv : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.world = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.uv = v.uv;
                return o;
            }


            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = (i.world.xz - _WorldOrigin.xz) / _WorldSize.xz;

                float visible = tex2D(_VisibilityTex, uv).r;
                float explored = tex2D(_ExplorationTex, uv).r;

                float fog = 1.0 - visible;
                fog = lerp(fog, fog * 0.5, explored);

                // fog texture ONLY (no noise, no modification)
                fixed4 fogTex = tex2D(_MainTex, uv);

                // tint it
                fogTex.rgb *= _FogColor.rgb;

                // apply fog alpha
                fogTex.a *= fog;

                return fogTex;
            }
            ENDCG
        }
    }
}