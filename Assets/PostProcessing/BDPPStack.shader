// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "BD/ColorPostProcess"
{
	Properties
	{
		_PalletPosturize("PalletPosturize", Vector) = (1,1,1,0)
		_PixelScale("PixelScale", Float) = 1

	}

	SubShader
	{
		LOD 0

		Cull Off
		ZWrite Off
		ZTest Always
		
		Pass
		{
			CGPROGRAM

			

			#pragma vertex Vert
			#pragma fragment Frag
			#pragma target 3.0

			#include "UnityCG.cginc"
			#include "UnityShaderVariables.cginc"

		
			struct ASEAttributesDefault
			{
				float3 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				
			};

			struct ASEVaryingsDefault
			{
				float4 vertex : SV_POSITION;
				float2 texcoord : TEXCOORD0;
				float2 texcoordStereo : TEXCOORD1;
			#if STEREO_INSTANCING_ENABLED
				uint stereoTargetEyeIndex : SV_RenderTargetArrayIndex;
			#endif
				
			};

			uniform sampler2D _MainTex;
			uniform half4 _MainTex_TexelSize;
			uniform half4 _MainTex_ST;
			
			uniform float _PixelScale;
			uniform float3 _PalletPosturize;


			
			float2 TransformTriangleVertexToUV (float2 vertex)
			{
				float2 uv = (vertex + 1.0) * 0.5;
				return uv;
			}

			ASEVaryingsDefault Vert( ASEAttributesDefault v  )
			{
				ASEVaryingsDefault o;
				o.vertex = float4(v.vertex.xy, 0.0, 1.0);
				o.texcoord = TransformTriangleVertexToUV (v.vertex.xy);
#if UNITY_UV_STARTS_AT_TOP
				o.texcoord = o.texcoord * float2(1.0, -1.0) + float2(0.0, 1.0);
#endif
				o.texcoordStereo = TransformStereoScreenSpaceTex (o.texcoord, 1.0);

				v.texcoord = o.texcoordStereo;
				float4 ase_ppsScreenPosVertexNorm = float4(o.texcoordStereo,0,1);

				

				return o;
			}

			float4 Frag (ASEVaryingsDefault i  ) : SV_Target
			{
				float4 ase_ppsScreenPosFragNorm = float4(i.texcoordStereo,0,1);

				float2 texCoord9 = i.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
				float pixelWidth8 =  1.0f / ( _ScreenParams.x * ( 1.0 / _PixelScale ) );
				float pixelHeight8 = 1.0f / ( _ScreenParams.y * ( 1.0 / _PixelScale ) );
				half2 pixelateduv8 = half2((int)(texCoord9.x / pixelWidth8) * pixelWidth8, (int)(texCoord9.y / pixelHeight8) * pixelHeight8);
				float4 tex2DNode7 = tex2D( _MainTex, pixelateduv8 );
				float3 appendResult34 = (float3(( ceil( ( tex2DNode7.r * _PalletPosturize.x ) ) / _PalletPosturize.x ) , ( ceil( ( tex2DNode7.g * _PalletPosturize.y ) ) / _PalletPosturize.y ) , ( ceil( ( tex2DNode7.b * _PalletPosturize.z ) ) / _PalletPosturize.z )));
				

				float4 color = float4( appendResult34 , 0.0 );
				
				return color;
			}
			ENDCG
		}
	}
	CustomEditor "ASEMaterialInspector"
	
	
}
/*ASEBEGIN
Version=18800
714;304;2009;916;2131.553;365.7594;1.3;True;False
Node;AmplifyShaderEditor.CommentaryNode;18;-1843.378,28.05183;Inherit;False;1012.296;501.9933;Comment;8;11;9;12;14;15;17;16;8;Pixelate;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;15;-1793.378,388.5451;Inherit;False;Property;_PixelScale;PixelScale;1;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;17;-1633.481,395.0451;Inherit;False;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RelayNode;16;-1506.08,393.7458;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenParams;11;-1541.088,199.6517;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;14;-1268.173,336.5454;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;12;-1286.373,194.845;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;9;-1340.883,78.05183;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCPixelate;8;-1047.082,180.7518;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TemplateShaderPropertyNode;6;-801.9754,9.552186;Inherit;False;0;0;_MainTex;Pass;True;0;5;SAMPLER2D;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;7;-518.5786,6.952109;Inherit;True;Property;_TextureSample0;Texture Sample 0;1;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector3Node;19;-490.0663,303.0793;Inherit;False;Property;_PalletPosturize;PalletPosturize;0;0;Create;True;0;0;0;False;0;False;1,1,1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;25;-114.5011,282.2909;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;29;-121.0341,453.8907;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;32;-87.23409,574.7906;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CeilOpNode;33;102.4664,591.6907;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CeilOpNode;30;68.66637,470.7907;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CeilOpNode;26;75.19932,299.1909;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;27;276.6995,312.191;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;31;303.9664,604.6908;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;28;270.1664,483.7908;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenColorNode;4;-735.6761,-188.0473;Inherit;False;Global;_GrabScreen0;Grab Screen 0;2;0;Create;True;0;0;0;False;0;False;Object;-1;False;False;1;0;FLOAT2;0,0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;2;-90.79315,103.3821;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.DynamicAppendNode;34;528.899,383.6905;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;5;222.9,-20.40001;Float;False;True;-1;2;ASEMaterialInspector;0;2;BD/ColorPostProcess;32139be9c1eb75640a847f011acf3bcf;True;SubShader 0 Pass 0;0;0;SubShader 0 Pass 0;1;False;False;False;False;False;False;False;False;False;True;2;False;-1;False;False;False;False;False;True;2;False;-1;True;7;False;-1;False;False;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;0;;0;0;Standard;0;0;1;True;False;;False;0
WireConnection;17;1;15;0
WireConnection;16;0;17;0
WireConnection;14;0;11;2
WireConnection;14;1;16;0
WireConnection;12;0;11;1
WireConnection;12;1;16;0
WireConnection;8;0;9;0
WireConnection;8;1;12;0
WireConnection;8;2;14;0
WireConnection;7;0;6;0
WireConnection;7;1;8;0
WireConnection;25;0;7;1
WireConnection;25;1;19;1
WireConnection;29;0;7;2
WireConnection;29;1;19;2
WireConnection;32;0;7;3
WireConnection;32;1;19;3
WireConnection;33;0;32;0
WireConnection;30;0;29;0
WireConnection;26;0;25;0
WireConnection;27;0;26;0
WireConnection;27;1;19;1
WireConnection;31;0;33;0
WireConnection;31;1;19;3
WireConnection;28;0;30;0
WireConnection;28;1;19;2
WireConnection;2;0;7;0
WireConnection;34;0;27;0
WireConnection;34;1;28;0
WireConnection;34;2;31;0
WireConnection;5;0;34;0
ASEEND*/
//CHKSM=89DB545422A7CBA35FBD64C632E74D6F83AFCF4A