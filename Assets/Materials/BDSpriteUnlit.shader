// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "BDSpriteUnlit"
{
	Properties
	{
		_MainTex("MainTex", 2D) = "white" {}
		[Toggle(_USEOUTLINE_ON)] _UseOutline("UseOutline", Float) = 1
		_OutlineThickness("OutlineThickness", Range( -0.1 , 0.1)) = 0
		_OutlineColor("OutlineColor", Color) = (0,0,0,0)
		[Toggle(_VISUALIZEKEYMASK_ON)] _VisualizeKeyMask("VisualizeKeyMask", Float) = 0
		_KeyColor("KeyColor", Color) = (0,0,0,0)
		_MaskRange("MaskRange", Range( -1 , 1.5)) = 0
		_MaskFuzziness("MaskFuzziness", Range( 0 , 2)) = 0
		_HSVShift("HSV Shift", Vector) = (0,0,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" }
		Cull Off
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#pragma shader_feature_local _USEOUTLINE_ON
		#pragma shader_feature_local _VISUALIZEKEYMASK_ON
		struct Input
		{
			float2 uv_texcoord;
		};

		struct SurfaceOutputCustomLightingCustom
		{
			half3 Albedo;
			half3 Normal;
			half3 Emission;
			half Metallic;
			half Smoothness;
			half Occlusion;
			half Alpha;
			Input SurfInput;
			UnityGIInput GIData;
		};

		uniform sampler2D _MainTex;
		uniform float4 _MainTex_ST;
		uniform float3 _HSVShift;
		uniform float4 _KeyColor;
		uniform float _MaskRange;
		uniform float _MaskFuzziness;
		uniform float4 _OutlineColor;
		uniform float _OutlineThickness;


		float3 HSVToRGB( float3 c )
		{
			float4 K = float4( 1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0 );
			float3 p = abs( frac( c.xxx + K.xyz ) * 6.0 - K.www );
			return c.z * lerp( K.xxx, saturate( p - K.xxx ), c.y );
		}


		float3 RGBToHSV(float3 c)
		{
			float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
			float4 p = lerp( float4( c.bg, K.wz ), float4( c.gb, K.xy ), step( c.b, c.g ) );
			float4 q = lerp( float4( p.xyw, c.r ), float4( c.r, p.yzx ), step( p.x, c.r ) );
			float d = q.x - min( q.w, q.y );
			float e = 1.0e-10;
			return float3( abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
		}

		inline half4 LightingStandardCustomLighting( inout SurfaceOutputCustomLightingCustom s, half3 viewDir, UnityGI gi )
		{
			UnityGIInput data = s.GIData;
			Input i = s.SurfInput;
			half4 c = 0;
			float2 uv_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			float4 tex2DNode39 = tex2D( _MainTex, uv_MainTex );
			float4 MainTextureSample223 = tex2DNode39;
			float3 hsvTorgb188 = RGBToHSV( MainTextureSample223.rgb );
			float3 hsvTorgb192 = HSVToRGB( float3(( _HSVShift.x + hsvTorgb188.x ),( hsvTorgb188.y + _HSVShift.y ),( hsvTorgb188.z + _HSVShift.z )) );
			float temp_output_217_0 = saturate( ( 1.0 - saturate( ( 1.0 - ( ( distance( _KeyColor.rgb , hsvTorgb188 ) - _MaskRange ) / max( _MaskFuzziness , 1E-05 ) ) ) ) ) );
			float4 lerpResult191 = lerp( tex2DNode39 , float4( hsvTorgb192 , 0.0 ) , temp_output_217_0);
			#ifdef _VISUALIZEKEYMASK_ON
				float4 staticSwitch215 = float4( ( temp_output_217_0 * hsvTorgb192 ) , 0.0 );
			#else
				float4 staticSwitch215 = lerpResult191;
			#endif
			float4 TintedAlbedo177 = staticSwitch215;
			float4 break46 = TintedAlbedo177;
			float MainTexAlpha183 = tex2DNode39.a;
			float4 appendResult47 = (float4(break46.r , break46.g , break46.b , MainTexAlpha183));
			float4 lerpResult28 = lerp( ( _OutlineColor * 1.0 ) , TintedAlbedo177 , MainTexAlpha183);
			float4 break44 = lerpResult28;
			float2 appendResult17 = (float2(_OutlineThickness , 0.0));
			float2 uv_TexCoord13 = i.uv_texcoord + appendResult17;
			float2 appendResult19 = (float2(-_OutlineThickness , 0.0));
			float2 uv_TexCoord14 = i.uv_texcoord + appendResult19;
			float2 appendResult21 = (float2(0.0 , _OutlineThickness));
			float2 uv_TexCoord15 = i.uv_texcoord + appendResult21;
			float2 appendResult22 = (float2(0.0 , -_OutlineThickness));
			float2 uv_TexCoord16 = i.uv_texcoord + appendResult22;
			float temp_output_31_0 = saturate( ( tex2D( _MainTex, uv_TexCoord13 ).a + tex2D( _MainTex, uv_TexCoord14 ).a + tex2D( _MainTex, uv_TexCoord15 ).a + tex2D( _MainTex, uv_TexCoord16 ).a ) );
			float4 appendResult45 = (float4(break44.r , break44.g , break44.b , temp_output_31_0));
			#ifdef _USEOUTLINE_ON
				float4 staticSwitch40 = appendResult45;
			#else
				float4 staticSwitch40 = appendResult47;
			#endif
			float4 break42 = staticSwitch40;
			float3 appendResult43 = (float3(break42.x , break42.y , break42.z));
			c.rgb = ( float3( 1,1,1 ) * appendResult43 );
			c.a = break42.w;
			return c;
		}

		inline void LightingStandardCustomLighting_GI( inout SurfaceOutputCustomLightingCustom s, UnityGIInput data, inout UnityGI gi )
		{
			s.GIData = data;
		}

		void surf( Input i , inout SurfaceOutputCustomLightingCustom o )
		{
			o.SurfInput = i;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf StandardCustomLighting alpha:fade keepalpha fullforwardshadows 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				SurfaceOutputCustomLightingCustom o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputCustomLightingCustom, o )
				surf( surfIN, o );
				UnityGI gi;
				UNITY_INITIALIZE_OUTPUT( UnityGI, gi );
				o.Alpha = LightingStandardCustomLighting( o, worldViewDir, gi ).a;
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
				clip( alphaRef - 0.01 );
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18800
1670;73;1769;875;1221.409;1457.269;1.028966;True;False
Node;AmplifyShaderEditor.CommentaryNode;50;-1464.075,-684.2654;Inherit;False;1248.661;521.3542;Tinted Main Tex;7;9;174;183;38;37;39;223;MainTex;1,1,1,1;0;0
Node;AmplifyShaderEditor.TexturePropertyNode;9;-1447.398,-650.9823;Inherit;True;Property;_MainTex;MainTex;0;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.RegisterLocalVarNode;174;-1209.467,-632.1663;Inherit;False;MainTexture;-1;True;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.SamplerNode;39;-963.8245,-634.2654;Inherit;True;Property;_TextureSample4;Texture Sample 4;0;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;225;-2126.407,-132.8688;Inherit;False;1973.041;713.4917;Comment;18;186;177;215;224;191;217;192;193;214;220;219;222;184;189;185;188;187;190;Color Tint;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;223;-475.6307,-250.9553;Inherit;False;MainTextureSample;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;187;-1954.225,119.9347;Inherit;False;223;MainTextureSample;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;185;-2076.407,347.9306;Inherit;False;Property;_KeyColor;KeyColor;12;0;Create;True;0;0;0;False;0;False;0,0,0,0;1,0.9590606,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RGBToHSVNode;188;-1674.725,75.7347;Inherit;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;189;-1824.378,424.0894;Inherit;False;Property;_MaskRange;MaskRange;13;0;Create;True;0;0;0;False;0;False;0;0;-1;1.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;190;-1819.537,503.8742;Inherit;False;Property;_MaskFuzziness;MaskFuzziness;14;0;Create;True;0;0;0;False;0;False;0;0;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;184;-1414.244,396.2712;Inherit;False;Color Mask;-1;;1;eec747d987850564c95bde0e5a6d1867;0;4;1;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;5;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;222;-1657.334,-82.86885;Inherit;False;Property;_HSVShift;HSV Shift;15;0;Create;True;0;0;0;False;0;False;0,0,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.CommentaryNode;35;-2439.001,618.7758;Inherit;False;2861.65;907.7782;Comment;25;175;53;32;54;11;31;27;25;23;24;26;14;15;13;16;19;22;21;17;20;18;10;178;179;198;4 Tex samples with offset UVS to get the outline;1,1,1,1;0;0
Node;AmplifyShaderEditor.OneMinusNode;214;-1172.789,422.6635;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;220;-1325.406,129.2261;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;219;-1323.352,35.36023;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;193;-1317.266,-53.58625;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.HSVToRGBNode;192;-1170.297,34.57737;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SaturateNode;217;-1013.323,425.7679;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;10;-2389.001,668.7758;Inherit;False;Property;_OutlineThickness;OutlineThickness;3;0;Create;True;0;0;0;False;0;False;0;0.0063;-0.1;0.1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;191;-847.6064,85.09525;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;224;-838.66,377.4875;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;18;-2287.597,768.8754;Inherit;False;Constant;_Float0;Float 0;3;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;20;-2152.398,852.0754;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;215;-647.6046,79.8411;Inherit;False;Property;_VisualizeKeyMask;VisualizeKeyMask;11;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.DynamicAppendNode;21;-2027.598,1026.276;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;22;-2013.299,1166.675;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;17;-2048.397,692.1754;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;19;-2027.599,875.4755;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ColorNode;11;-1146.904,691.4955;Inherit;False;Property;_OutlineColor;OutlineColor;4;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;175;-1859.66,1268.693;Inherit;False;174;MainTexture;1;0;OBJECT;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;177;-355.9576,79.61852;Inherit;False;TintedAlbedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;15;-1768.898,948.9257;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;183;-428.2354,-448.8365;Inherit;False;MainTexAlpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;208;-960.512,918.4341;Inherit;False;Constant;_Float6;Float 6;15;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;16;-1771.498,1090.626;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;14;-1774.097,815.6754;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;13;-1771.497,673.9757;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;32;-862.2318,737.8632;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;24;-1449.097,896.2744;Inherit;True;Property;_TextureSample1;Texture Sample 1;0;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;25;-1443.899,1093.875;Inherit;True;Property;_TextureSample2;Texture Sample 2;0;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;23;-1458.199,698.6748;Inherit;True;Property;_TextureSample0;Texture Sample 0;0;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;178;-595.2018,815.4108;Inherit;False;177;TintedAlbedo;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;54;-281.6468,691.5991;Inherit;False;232;209;Layer Tex ontop of outline;1;28;;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;179;-592.6022,894.7101;Inherit;False;183;MainTexAlpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;26;-1446.499,1312.275;Inherit;True;Property;_TextureSample3;Texture Sample 3;0;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;28;-231.6467,741.5992;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;27;-1081.015,1074.305;Inherit;False;4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;53;-10.85981,708.8279;Inherit;False;378.3669;308.7531;Comment;2;44;45;Combine channels for switch;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;52;-95.18158,-606.1744;Inherit;False;426.2059;298.9026;Comment;2;46;47;Combine Chanells for switch;1,1,1,1;0;0
Node;AmplifyShaderEditor.BreakToComponentsNode;44;39.14019,758.8279;Inherit;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SaturateNode;31;-938.956,1078.37;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;46;-45.18152,-556.1744;Inherit;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.DynamicAppendNode;45;206.507,834.5809;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.DynamicAppendNode;47;170.0244,-490.2718;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.StaticSwitch;40;470.9038,-330.5001;Inherit;False;Property;_UseOutline;UseOutline;2;0;Create;True;0;0;0;False;0;False;0;1;1;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;FLOAT4;0,0,0,0;False;0;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;3;FLOAT4;0,0,0,0;False;4;FLOAT4;0,0,0,0;False;5;FLOAT4;0,0,0,0;False;6;FLOAT4;0,0,0,0;False;7;FLOAT4;0,0,0,0;False;8;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.CommentaryNode;171;-1667.963,-1518.283;Inherit;False;2813.909;794.3931;Comment;14;164;161;159;156;151;149;162;199;201;204;209;205;226;227;Custom Lighting;1,1,1,1;0;0
Node;AmplifyShaderEditor.BreakToComponentsNode;42;749.1448,-298.3302;Inherit;False;FLOAT4;1;0;FLOAT4;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.CommentaryNode;151;-929.3005,-1399.272;Inherit;False;812;304;Comment;5;163;158;157;155;153;Attenuation and Ambient;1,1,1,1;0;0
Node;AmplifyShaderEditor.DynamicAppendNode;43;911.2438,-374.2074;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;90;-2762.002,1570.304;Inherit;False;2050.657;792.0215;Comment;19;61;58;59;60;70;81;80;56;57;75;77;78;83;87;82;86;85;88;89;ShimmerTesting;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;149;-1613.282,-1393.339;Inherit;False;540.401;320.6003;Comment;3;154;152;150;N . L;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleAddOpNode;158;-433.3004,-1239.271;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LightColorNode;157;-817.3005,-1351.271;Inherit;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;159;-675.2078,-941.3745;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;61;-2712.002,1746.268;Inherit;False;Property;_TimeScale;TimeScale;6;0;Create;True;0;0;0;False;0;False;0;2.24;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.IndirectDiffuseLighting;155;-664.3005,-1251.271;Inherit;False;Tangent;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;205;313.21,-928.3033;Inherit;False;Constant;_Float4;Float 4;16;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;37;-880.1332,-391.9524;Inherit;False;Property;_Tint;Tint;1;0;Create;True;0;0;0;False;0;False;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;164;-291.2078,-941.3745;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;161;-483.2078,-941.3745;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;154;-1213.282,-1281.339;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LightColorNode;162;-681.7282,-843.7778;Inherit;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;211;-1025.118,-937.953;Inherit;False;Constant;_Float2;Float 2;15;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;201;255.7318,-827.9788;Inherit;False;183;MainTexAlpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;199;674.6633,-927.7108;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.WorldNormalVector;152;-1537.597,-1352.505;Inherit;False;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;150;-1549.282,-1215.289;Inherit;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.Vector2Node;56;-1712.369,2198.324;Inherit;False;Property;_ShimmerFrequency;ShimmerFrequency;7;0;Create;True;0;0;0;False;0;False;1,1;-5.1,16.6;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.StaticSwitch;209;371.5644,-1185.421;Inherit;False;Property;_UNITY_PASS_FORWARDADD;UNITY_PASS_FORWARDADD;15;0;Create;True;0;0;0;False;0;False;0;0;0;False;UNITY_PASS_FORWARDADD;Toggle;2;Key0;Key1;Fetch;False;True;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.FogAndAmbientColorsNode;226;-430.1346,-1045.683;Inherit;False;UNITY_LIGHTMODEL_AMBIENT;0;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;204;474.8166,-824.9426;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;156;-828.2078,-986.3748;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;89;-917.6386,1662.974;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;173;1573.795,-495.1903;Inherit;False;2;2;0;FLOAT3;1,1,1;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SaturateNode;82;-881.1412,2083.907;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;59;-2536.402,1862.168;Inherit;False;Property;_ShimmerSpeed;ShimmerSpeed;8;0;Create;True;0;0;0;False;0;False;1,1;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.OneMinusNode;85;-1282.397,1949.622;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;88;-1264.541,1682.158;Inherit;False;Property;_ShimmerColor;ShimmerColor;5;0;Create;True;0;0;0;False;0;False;0,0,0,0;1,0.7577934,0.1839623,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;60;-2330.102,1772.668;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;38;-642.6157,-631.2685;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SinOpNode;75;-1451.89,1812.12;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;81;-2197.112,2004.362;Inherit;False;Property;_ShimmerAngle;ShimmerAngle;9;0;Create;True;0;0;0;False;0;False;0;0.58;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LightAttenuation;153;-886.9006,-1204.271;Inherit;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;78;-1143.386,2114.841;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;198;-605.2576,1124.225;Inherit;False;OutlineMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;87;-1208.86,2027.956;Inherit;False;Property;_Float1;Float 1;10;0;Create;True;0;0;0;False;0;False;0;1.47;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;57;-1612.969,1799.125;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RelayNode;172;1408.092,-552.5701;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RotatorNode;80;-1906.112,1874.362;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;86;-1018.622,1946.425;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;83;-929.1,2208.6;Inherit;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.RGBToHSVNode;186;-1836.927,248.872;Inherit;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TextureCoordinatesNode;70;-2188.889,1762.72;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleTimeNode;58;-2537.702,1769.468;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;227;-84.40188,-1002.466;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;77;-1313.686,2104.441;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;1,1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;163;-273.3004,-1351.271;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1774.84,-278.3481;Float;False;True;-1;2;ASEMaterialInspector;0;0;CustomLighting;BDSpriteUnlit;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;True;0;False;Transparent;;Transparent;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;174;0;9;0
WireConnection;39;0;174;0
WireConnection;223;0;39;0
WireConnection;188;0;187;0
WireConnection;184;1;188;0
WireConnection;184;3;185;0
WireConnection;184;4;189;0
WireConnection;184;5;190;0
WireConnection;214;0;184;0
WireConnection;220;0;188;3
WireConnection;220;1;222;3
WireConnection;219;0;188;2
WireConnection;219;1;222;2
WireConnection;193;0;222;1
WireConnection;193;1;188;1
WireConnection;192;0;193;0
WireConnection;192;1;219;0
WireConnection;192;2;220;0
WireConnection;217;0;214;0
WireConnection;191;0;39;0
WireConnection;191;1;192;0
WireConnection;191;2;217;0
WireConnection;224;0;217;0
WireConnection;224;1;192;0
WireConnection;20;0;10;0
WireConnection;215;1;191;0
WireConnection;215;0;224;0
WireConnection;21;0;18;0
WireConnection;21;1;10;0
WireConnection;22;0;18;0
WireConnection;22;1;20;0
WireConnection;17;0;10;0
WireConnection;17;1;18;0
WireConnection;19;0;20;0
WireConnection;19;1;18;0
WireConnection;177;0;215;0
WireConnection;15;1;21;0
WireConnection;183;0;39;4
WireConnection;16;1;22;0
WireConnection;14;1;19;0
WireConnection;13;1;17;0
WireConnection;32;0;11;0
WireConnection;32;1;208;0
WireConnection;24;0;175;0
WireConnection;24;1;14;0
WireConnection;25;0;175;0
WireConnection;25;1;15;0
WireConnection;23;0;175;0
WireConnection;23;1;13;0
WireConnection;26;0;175;0
WireConnection;26;1;16;0
WireConnection;28;0;32;0
WireConnection;28;1;178;0
WireConnection;28;2;179;0
WireConnection;27;0;23;4
WireConnection;27;1;24;4
WireConnection;27;2;25;4
WireConnection;27;3;26;4
WireConnection;44;0;28;0
WireConnection;31;0;27;0
WireConnection;46;0;177;0
WireConnection;45;0;44;0
WireConnection;45;1;44;1
WireConnection;45;2;44;2
WireConnection;45;3;31;0
WireConnection;47;0;46;0
WireConnection;47;1;46;1
WireConnection;47;2;46;2
WireConnection;47;3;183;0
WireConnection;40;1;47;0
WireConnection;40;0;45;0
WireConnection;42;0;40;0
WireConnection;43;0;42;0
WireConnection;43;1;42;1
WireConnection;43;2;42;2
WireConnection;158;0;155;0
WireConnection;158;1;153;0
WireConnection;159;0;156;0
WireConnection;164;0;161;0
WireConnection;164;1;162;0
WireConnection;161;0;159;0
WireConnection;154;0;152;0
WireConnection;154;1;150;0
WireConnection;199;0;209;0
WireConnection;199;1;205;0
WireConnection;199;2;204;0
WireConnection;209;1;227;0
WireConnection;209;0;163;0
WireConnection;204;0;201;0
WireConnection;156;0;211;0
WireConnection;156;1;154;0
WireConnection;89;1;88;0
WireConnection;89;2;86;0
WireConnection;173;1;43;0
WireConnection;82;0;83;0
WireConnection;60;0;58;0
WireConnection;60;1;59;0
WireConnection;38;0;39;0
WireConnection;38;1;37;0
WireConnection;75;0;57;0
WireConnection;78;0;77;0
WireConnection;198;0;31;0
WireConnection;57;0;80;0
WireConnection;57;1;56;0
WireConnection;172;0;205;0
WireConnection;80;0;70;0
WireConnection;80;2;81;0
WireConnection;86;0;82;0
WireConnection;86;1;87;0
WireConnection;83;0;78;0
WireConnection;186;0;185;0
WireConnection;70;1;60;0
WireConnection;58;0;61;0
WireConnection;227;0;226;0
WireConnection;227;1;164;0
WireConnection;77;0;75;0
WireConnection;163;0;157;0
WireConnection;163;1;158;0
WireConnection;0;9;42;3
WireConnection;0;13;173;0
ASEEND*/
//CHKSM=C00E1159CC15A96324773562ABAD57E70DCC74A9