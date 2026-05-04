Shader "Custom/FogBrush_Texture"
{
    Properties
    {
        _BrushTex ("Brush Texture", 2D) = "white" {}
        _Center ("Center UV", Vector) = (0.5,0.5,0,0)
        _Size ("Size UV", Vector) = (0.2, 0.2, 0, 0)
        _Opacity ("Opacity", Float) = 1
    }

    SubShader
    {
        Tags { "Queue"="Transparent" }
        ZWrite Off
        ZTest Always
        Cull Off

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _BrushTex;

            float2 _Center;
            float2 _Size;
            float _Opacity;

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

            float4 frag(v2f i) : SV_Target
            {
                float2 uv = (i.uv - _Center) / _Size + 0.5;
                // 3. shape mask (rectangle falloff)
                //float2 d = abs(uv - 0.5) * 2.0;
                //float dist = max(d.x, d.y);

                //float shape = smoothstep(1.0, 0.0, dist);
                //shape *= shape; 
                //return shape * _Opacity;
                float2 inside = step(0.0, uv) * step(uv, 1.0);
                float mask = inside.x * inside.y;

                float4 tex = tex2D(_BrushTex, uv);
                //if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1) return 0;

                return tex*mask * _Opacity;
            }

            ENDCG
        }
    }
}