Shader "Custom/Circle" {
	 Properties {
         _MainTex ("Base (RGB)", 2D) = "white" {}
         _Radius("Radius", Float) = -2.0
         _SourceX("Right Limit: World Pos X", Float) = 2.0
         _SourceY("Top Limit: World Pos Y", Float) = -2.0
		 _Alpha("Transparent",range(0.0,1.0)) = 1.0

		 _HM("Highlight / Magnifier",Range(0,1)) = 0

     }
     SubShader {
         Lighting Off
         AlphaTest Greater 0.5
              
         Tags { 
              "Queue"="Transparent" 
              "IgnoreProjector"="True" 
              "RenderType"="Transparent" 
         }
          
         ZWrite Off
         Blend One OneMinusSrcAlpha
         LOD 200

         CGPROGRAM
		 
		 //#pragma vert vertex
         #pragma surface surf NoLighting alpha 
         #include "UnityCG.cginc"
      
         fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten){
             fixed4 c;
             c.rgb = s.Albedo;
             c.a = s.Alpha;
             return c;
         }
      
		 sampler2D _MainTex;
 
         struct Input {
             float2 uv_MainTex;
             float3 worldPos;
         };

		 float _Radius;
		 float _SourceX;
		 float _SourceY;
		 float _Alpha;
		 int _HM;

         void surf (Input IN, inout SurfaceOutput o) {
			 
			 float x = pow(pow(IN.worldPos.x - _SourceX,2) + pow(IN.worldPos.y -_SourceY,2),0.5f);
		
			 if (_HM == 0) {
				 if (x < _Radius) {
					 clip(-1.0);
				 }
			 }
			 else {
				 if (x > _Radius) {
					 clip(-1.0);
				 }
			 }
 
             half4 c = tex2D (_MainTex, IN.uv_MainTex);
             o.Albedo = c.rgb;
             o.Alpha = _Alpha;
         }
         ENDCG
     }
     FallBack "Diffuse"
  }