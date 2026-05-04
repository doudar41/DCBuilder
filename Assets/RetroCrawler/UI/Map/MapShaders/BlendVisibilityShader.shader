Shader "Custom/FogBlending"
{
     Properties
    {
        _MainTex ("Current", 2D) = "black" {}
        _PrevTex ("Previous", 2D) = "black" {}
    }

    SubShader
    {
        Pass
        {
            ZTest Always Cull Off ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _PrevTex;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed frag(v2f i) : SV_Target
            {
                float current = tex2D(_MainTex, i.uv).r;
                float previous = tex2D(_PrevTex, i.uv).r;

                float explored = max(previous, current);

                // IMPORTANT: collapse gradient → binary memory
                //explored = step(0.1, explored);

                //return saturate(explored);

                //return max(current, previous);
                float result = previous + current * (1.0 - previous);
                return result;
            }

            ENDCG
        }
    }
}