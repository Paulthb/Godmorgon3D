// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "CARD_USING"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		_Scale("_Scale", Float) = 5.57
		_Angle_Speed("_Angle_Speed", Float) = 1
		_Max("_Max", Float) = 0.09
		_Color0("Color 0", Color) = (0,0,0,0)
		_Color1("Color 1", Color) = (0.9433962,0.187385,0,0)
		_Opa("_Opa", Float) = 25.84
		_OOF("OOF", Float) = 110.43
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "Transparent+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#if defined(SHADER_API_D3D11) || defined(SHADER_API_XBOXONE) || defined(UNITY_COMPILER_HLSLCC) || defined(SHADER_API_PSSL) || (defined(SHADER_TARGET_SURFACE_ANALYSIS) && !defined(SHADER_TARGET_SURFACE_ANALYSIS_MOJOSHADER))//ASE Sampler Macros
		#define SAMPLE_TEXTURE2D(tex,samplerTex,coord) tex.Sample(samplerTex,coord)
		#else//ASE Sampling Macros
		#define SAMPLE_TEXTURE2D(tex,samplerTex,coord) tex2D(tex,coord)
		#endif//ASE Sampling Macros

		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float _Max;
		UNITY_DECLARE_TEX2D_NOSAMPLER(_TextureSample0);
		SamplerState sampler_TextureSample0;
		uniform float _Scale;
		uniform float _Angle_Speed;
		uniform float4 _Color1;
		uniform float4 _Color0;
		uniform float _Opa;
		uniform float _OOF;
		uniform float _Cutoff = 0.5;


		float2 voronoihash14( float2 p )
		{
			
			p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
			return frac( sin( p ) *43758.5453);
		}


		float voronoi14( float2 v, float time, inout float2 id, inout float2 mr, float smoothness )
		{
			float2 n = floor( v );
			float2 f = frac( v );
			float F1 = 8.0;
			float F2 = 8.0; float2 mg = 0;
			for ( int j = -1; j <= 1; j++ )
			{
				for ( int i = -1; i <= 1; i++ )
			 	{
			 		float2 g = float2( i, j );
			 		float2 o = voronoihash14( n + g );
					o = ( sin( time + o * 6.2831 ) * 0.5 + 0.5 ); float2 r = f - g - o;
					float d = 0.5 * dot( r, r );
			 		if( d<F1 ) {
			 			F2 = F1;
			 			F1 = d; mg = g; mr = r; id = o;
			 		} else if( d<F2 ) {
			 			F2 = d;
			 		}
			 	}
			}
			return F1;
		}


		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 temp_cast_0 = (0.0).xxxx;
			float4 temp_cast_1 = (_Max).xxxx;
			float2 uv_TexCoord2 = i.uv_texcoord * float2( 1.4,1 ) + float2( -0.2,0 );
			float2 temp_cast_2 = (_Angle_Speed).xx;
			float2 panner18 = ( 1.0 * _Time.y * temp_cast_2 + i.uv_texcoord);
			float time14 = panner18.x;
			float2 panner15 = ( 1.0 * _Time.y * float2( 0,0 ) + i.uv_texcoord);
			float2 coords14 = panner15 * _Scale;
			float2 id14 = 0;
			float2 uv14 = 0;
			float fade14 = 0.5;
			float voroi14 = 0;
			float rest14 = 0;
			for( int it14 = 0; it14 <7; it14++ ){
			voroi14 += fade14 * voronoi14( coords14, time14, id14, uv14, 0 );
			rest14 += fade14;
			coords14 *= 2;
			fade14 *= 0.5;
			}//Voronoi14
			voroi14 /= rest14;
			float4 smoothstepResult21 = smoothstep( temp_cast_0 , temp_cast_1 , ( SAMPLE_TEXTURE2D( _TextureSample0, sampler_TextureSample0, uv_TexCoord2 ) * voroi14 ));
			float4 temp_output_27_0 = ( smoothstepResult21 - ( 1.0 - smoothstepResult21 ) );
			o.Emission = ( ( temp_output_27_0 * _Color1 ) + ( _Color0 * temp_output_27_0 ) ).rgb;
			o.Alpha = 1;
			float4 temp_cast_5 = (_Opa).xxxx;
			float4 temp_cast_6 = (_OOF).xxxx;
			clip( step( pow( temp_output_27_0 , temp_cast_5 ) , temp_cast_6 ).r - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18400
0;0;1920;1019;-667.1911;218.0924;1.197973;True;True
Node;AmplifyShaderEditor.TextureCoordinatesNode;16;-1210.29,275.5685;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;19;-1037.842,551.0498;Inherit;False;Property;_Angle_Speed;_Angle_Speed;3;0;Create;True;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;18;-846.6251,359.8307;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;17;-616.4705,478.4004;Inherit;False;Property;_Scale;_Scale;2;0;Create;True;0;0;False;0;False;5.57;6.08;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;2;-850.1372,-102.1958;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1.4,1;False;1;FLOAT2;-0.2,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;15;-807.7948,123.4447;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;1;-509.3329,-117.7478;Inherit;True;Property;_TextureSample0;Texture Sample 0;1;0;Create;True;0;0;False;0;False;-1;6dfdf98b0b57c7e4e9ace5d91029a019;6dfdf98b0b57c7e4e9ace5d91029a019;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VoronoiNode;14;-430.1779,290.7858;Inherit;True;0;0;1;0;7;False;1;False;False;4;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;3;FLOAT;0;FLOAT2;1;FLOAT2;2
Node;AmplifyShaderEditor.RangedFloatNode;23;-25.76193,837.1346;Inherit;False;Property;_Max;_Max;4;0;Create;True;0;0;False;0;False;0.09;0.13;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;22;-25.76193,725.1346;Inherit;False;Constant;_Float2;Float 2;2;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;20;-80,128;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SmoothstepOpNode;21;195.8031,250.387;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;28;408,118;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;32;713.5443,632.622;Inherit;False;Property;_Color1;Color 1;6;0;Create;True;0;0;False;0;False;0.9433962,0.187385,0,0;1,0.6034184,0,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleSubtractOpNode;27;619.7389,288.8202;Inherit;True;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;26;108.1633,-238.7225;Inherit;False;Property;_Color0;Color 0;5;0;Create;True;0;0;False;0;False;0,0,0,0;1,0.07166468,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;35;1222.026,550.9302;Inherit;False;Property;_Opa;_Opa;7;0;Create;True;0;0;False;0;False;25.84;0.23;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;25;858.869,-143.8822;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.PowerNode;37;1368.831,323.5487;Inherit;True;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;39;1666.769,705.9382;Inherit;False;Property;_OOF;OOF;8;0;Create;True;0;0;False;0;False;110.43;0.28;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;30;1003.698,213.4519;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;40;1631.769,467.9382;Inherit;False;Constant;_Float0;Float 0;8;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;29;1284.368,-56.47322;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StepOpNode;41;1867.56,340.1628;Inherit;True;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;2214.968,-73.50713;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;CARD_USING;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;True;0;True;TransparentCutout;;Transparent;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;True;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;18;0;16;0
WireConnection;18;2;19;0
WireConnection;15;0;16;0
WireConnection;1;1;2;0
WireConnection;14;0;15;0
WireConnection;14;1;18;0
WireConnection;14;2;17;0
WireConnection;20;0;1;0
WireConnection;20;1;14;0
WireConnection;21;0;20;0
WireConnection;21;1;22;0
WireConnection;21;2;23;0
WireConnection;28;0;21;0
WireConnection;27;0;21;0
WireConnection;27;1;28;0
WireConnection;25;0;26;0
WireConnection;25;1;27;0
WireConnection;37;0;27;0
WireConnection;37;1;35;0
WireConnection;30;0;27;0
WireConnection;30;1;32;0
WireConnection;29;0;30;0
WireConnection;29;1;25;0
WireConnection;41;0;37;0
WireConnection;41;1;39;0
WireConnection;0;2;29;0
WireConnection;0;10;41;0
ASEEND*/
//CHKSM=47D18DEA8830D6DC948B1CE25935E698E87983F2