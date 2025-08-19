Shader "Custom/Light" {
    Properties {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Base (RGB)", 2D) = "white" {}
		_SpecColor ("Spec Color", Color) = (1,1,1,1)
    }
   
    SubShader {
        Blend SrcAlpha One
		//Blend SrcAlpha OneMinusSrcAlpha

        ZWrite OFF // csak Transparent objektumnal legyen OFF
        Tags 
		{
			"Queue"="Transparent" 
			"RenderType"="Transparent" 
			"IgnoreProjector"="True" 
		}
        ColorMask RGB
        // Vertex lights
        Pass {
            Tags {"LightMode" = "Vertex"}

            Lighting ON
			SeparateSpecular On // hogy a spectacular szin feluliras mukodjon
			Cull Back // nem rendereli aminek a normalja nem felenk mutat
			
			/*Fog{
				Mode Linear
				Color (0,0,0,0)
				Range 0.0,1.1
			}*/

			Material {
                Diffuse [_Color]
				// Ambient [_Color] a korulotte is vilagit
				Specular [_SpecColor]
            }
			
            SetTexture [_MainTex] {
                constantColor [_Color] 
                Combine texture * primary DOUBLE, texture * constant
            }
        }
    }
   
    Fallback "VertexLit", 2
   
}