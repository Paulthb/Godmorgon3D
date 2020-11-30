// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Fire_action"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_Speed("Speed", Float) = 0.2
		_Angle_Speed("Angle_Speed", Float) = 1.35
		[HDR]_Color0("Color 0", Color) = (0.8666667,0.3548591,0,0)
		[HDR]_Color1("Color 1", Color) = (1,0.06844188,0,0)
		_Opa("_Opa", Float) = 0
		_Intensity("Intensity", Float) = 0
		_Scale("_Scale", Float) = 1.07
		_Emissive_Int("Emissive_Int", Float) = 9.2
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		_X_Step("X_Step", Float) = 0.6
		_Y_Step("Y_Step", Float) = -1.76
		_Z_Step("Z_Step", Float) = -0.89
		_Scale2("_Scale 2", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		Blend SrcAlpha OneMinusSrcAlpha
		
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#if defined(SHADER_API_D3D11) || defined(SHADER_API_XBOXONE) || defined(UNITY_COMPILER_HLSLCC) || defined(SHADER_API_PSSL) || (defined(SHADER_TARGET_SURFACE_ANALYSIS) && !defined(SHADER_TARGET_SURFACE_ANALYSIS_MOJOSHADER))//ASE Sampler Macros
		#define SAMPLE_TEXTURE2D(tex,samplerTex,coord) tex.Sample(samplerTex,coord)
		#else//ASE Sampling Macros
		#define SAMPLE_TEXTURE2D(tex,samplerTex,coord) tex2D(tex,coord)
		#endif//ASE Sampling Macros

		#pragma surface surf Standard keepalpha addshadow fullforwardshadows exclude_path:deferred 
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
		};

		uniform float _Emissive_Int;
		uniform float _Scale;
		uniform float _Angle_Speed;
		uniform float _Speed;
		uniform float4 _Color0;
		uniform float4 _Color1;
		uniform float _Scale2;
		uniform float _X_Step;
		uniform float _Y_Step;
		uniform float _Z_Step;
		uniform float _Intensity;
		UNITY_DECLARE_TEX2D_NOSAMPLER(_TextureSample0);
		SamplerState sampler_TextureSample0;
		uniform float4 _TextureSample0_ST;
		uniform float _Opa;
		uniform float _Cutoff = 0.5;


		float2 voronoihash22( float2 p )
		{
			
			p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
			return frac( sin( p ) *43758.5453);
		}


		float voronoi22( float2 v, float time, inout float2 id, inout float2 mr, float smoothness )
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
			 		float2 o = voronoihash22( n + g );
					o = ( sin( time + o * 6.2831 ) * 0.5 + 0.5 ); float2 r = f - g - o;
					float d = 0.707 * sqrt(dot( r, r ));
			 //		if( d<F1 ) {
			 //			F2 = F1;
			 			float h = smoothstep(0.0, 1.0, 0.5 + 0.5 * (F1 - d) / smoothness); F1 = lerp(F1, d, h) - smoothness * h * (1.0 - h);mg = g; mr = r; id = o;
			 //		} else if( d<F2 ) {
			 //			F2 = d;
			 //		}
			 	}
			}
			return F1;
		}


		float2 voronoihash1( float2 p )
		{
			
			p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
			return frac( sin( p ) *43758.5453);
		}


		float voronoi1( float2 v, float time, inout float2 id, inout float2 mr, float smoothness )
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
			 		float2 o = voronoihash1( n + g );
					o = ( sin( time + o * 6.2831 ) * 0.5 + 0.5 ); float2 r = f - g - o;
					float d = 0.707 * sqrt(dot( r, r ));
			 //		if( d<F1 ) {
			 //			F2 = F1;
			 			float h = smoothstep(0.0, 1.0, 0.5 + 0.5 * (F1 - d) / smoothness); F1 = lerp(F1, d, h) - smoothness * h * (1.0 - h);mg = g; mr = r; id = o;
			 //		} else if( d<F2 ) {
			 //			F2 = d;
			 //		}
			 	}
			}
			return F1;
		}


		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 temp_cast_0 = (_Angle_Speed).xx;
			float2 panner8 = ( 1.0 * _Time.y * temp_cast_0 + i.uv_texcoord);
			float time22 = panner8.x;
			float voronoiSmooth0 = 0.06;
			float2 temp_cast_2 = (_Speed).xx;
			float3 ase_worldPos = i.worldPos;
			float4 appendResult79 = (float4(ase_worldPos.x , ase_worldPos.y , 0.0 , 0.0));
			float2 panner24 = ( 1.0 * _Time.y * temp_cast_2 + appendResult79.xy);
			float2 coords22 = panner24 * _Scale;
			float2 id22 = 0;
			float2 uv22 = 0;
			float fade22 = 0.5;
			float voroi22 = 0;
			float rest22 = 0;
			for( int it22 = 0; it22 <6; it22++ ){
			voroi22 += fade22 * voronoi22( coords22, time22, id22, uv22, voronoiSmooth0 );
			rest22 += fade22;
			coords22 *= 2;
			fade22 *= 0.5;
			}//Voronoi22
			voroi22 /= rest22;
			float clampResult33 = clamp( ( 1.0 - voroi22 ) , 0.13 , 0.71 );
			float time1 = panner8.x;
			float2 temp_cast_5 = (1.82).xx;
			float2 panner6 = ( _Time.x * temp_cast_5 + i.uv_texcoord);
			float2 coords1 = panner6 * _Scale2;
			float2 id1 = 0;
			float2 uv1 = 0;
			float fade1 = 0.5;
			float voroi1 = 0;
			float rest1 = 0;
			for( int it1 = 0; it1 <6; it1++ ){
			voroi1 += fade1 * voronoi1( coords1, time1, id1, uv1, voronoiSmooth0 );
			rest1 += fade1;
			coords1 *= 2;
			fade1 *= 0.5;
			}//Voronoi1
			voroi1 /= rest1;
			float smoothstepResult15 = smoothstep( 0.37 , 6.32 , saturate( ( 1.0 - voroi1 ) ));
			float4 lerpResult18 = lerp( ( pow( clampResult33 , 9.79 ) * _Color0 ) , _Color1 , ( 4.07 * smoothstepResult15 ));
			float3 appendResult89 = (float3(_X_Step , _Y_Step , _Z_Step));
			float4 temp_output_82_0 = step( ( _Emissive_Int * lerpResult18 ) , float4( appendResult89 , 0.0 ) );
			float4 temp_cast_7 = (pow( smoothstepResult15 , _Intensity )).xxxx;
			o.Emission = saturate( ( temp_output_82_0 - temp_cast_7 ) ).rgb;
			o.Alpha = 1;
			float2 uv_TextureSample0 = i.uv_texcoord * _TextureSample0_ST.xy + _TextureSample0_ST.zw;
			float saferPower76 = max( SAMPLE_TEXTURE2D( _TextureSample0, sampler_TextureSample0, uv_TextureSample0 ).r , 0.0001 );
			clip( ( pow( saferPower76 , 14.78 ) * pow( temp_output_82_0.r , _Opa ) ) - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18400
0;18;1920;1001;2800.3;915.423;1.665179;True;True
Node;AmplifyShaderEditor.WorldPosInputsNode;78;-2587.712,-1008.853;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TimeNode;69;-2057.327,-23.04529;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;70;-2042.542,-107.7232;Inherit;False;Constant;_Panner_Speed2;_Panner_Speed 2;9;0;Create;True;0;0;False;0;False;1.82;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;26;-2010.94,-304.1612;Inherit;False;Property;_Angle_Speed;Angle_Speed;2;0;Create;True;0;0;False;0;False;1.35;3.35;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;5;-2607.843,-420.9559;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;79;-2335.67,-942.7831;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;25;-2452.297,-712.2908;Inherit;False;Property;_Speed;Speed;1;0;Create;True;0;0;False;0;False;0.2;-0.25;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;8;-1830.316,-370.1111;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;34;-1820.986,-463.9383;Inherit;False;Constant;_Smoothness;_Smoothness;5;0;Create;True;0;0;False;0;False;0.06;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;90;-1696.085,180.7051;Inherit;False;Property;_Scale2;_Scale 2;13;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;65;-1908.838,-533.4008;Inherit;False;Property;_Scale;_Scale;7;0;Create;True;0;0;False;0;False;1.07;1.17;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;24;-2072.814,-757.9949;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;6;-1826.439,-168.3871;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.VoronoiNode;22;-1503.692,-555.0985;Inherit;True;0;1;1;0;6;False;1;False;True;4;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;2.98;False;3;FLOAT;0;False;3;FLOAT;0;FLOAT2;1;FLOAT2;2
Node;AmplifyShaderEditor.VoronoiNode;1;-1511.726,-242.5217;Inherit;True;0;1;1;0;6;False;1;False;True;4;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;2.66;False;3;FLOAT;0;False;3;FLOAT;0;FLOAT2;1;FLOAT2;2
Node;AmplifyShaderEditor.OneMinusNode;9;-1216.004,-215.6128;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;31;-1169.859,-528.0699;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;13;-895.0039,106.3872;Inherit;False;Constant;_Float1;Float 1;0;0;Create;True;0;0;False;0;False;0.37;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;11;-995.0039,-212.6128;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;14;-867.0039,243.3872;Inherit;False;Constant;_Float2;Float 2;0;0;Create;True;0;0;False;0;False;6.32;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;33;-883.3008,-476.9697;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0.13;False;2;FLOAT;0.71;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;19;-753.8395,-983.7706;Inherit;False;Property;_Color0;Color 0;3;1;[HDR];Create;True;0;0;False;0;False;0.8666667,0.3548591,0,0;1.733333,0.3283307,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;35;-481.389,-470.6601;Inherit;True;False;2;0;FLOAT;0;False;1;FLOAT;9.79;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;15;-383.1457,-164.9116;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;72;70.73657,-275.3221;Inherit;False;2;2;0;FLOAT;4.07;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;21;-129.4466,-774.8647;Inherit;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;20;-754.9423,-797.4002;Inherit;False;Property;_Color1;Color 1;4;1;[HDR];Create;True;0;0;False;0;False;1,0.06844188,0,0;18.67833,0.6633681,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;86;213.478,535.8495;Inherit;False;Property;_Y_Step;Y_Step;11;0;Create;True;0;0;False;0;False;-1.76;0.16;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;66;420.4586,-628.8038;Inherit;False;Property;_Emissive_Int;Emissive_Int;8;0;Create;True;0;0;False;0;False;9.2;13.15;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;18;294.2413,-454.6422;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;85;211.6774,259.2899;Inherit;True;Property;_X_Step;X_Step;10;0;Create;True;0;0;False;0;False;0.6;5.98;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;87;196.6789,630.6772;Inherit;False;Property;_Z_Step;Z_Step;12;0;Create;True;0;0;False;0;False;-0.89;-0.97;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;89;594.1614,280.9753;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;53;587.5339,-504.5948;Inherit;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;62;2.344238,205.4192;Inherit;False;Property;_Intensity;Intensity;6;0;Create;True;0;0;False;0;False;0;0.4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;82;889.5517,-442.3578;Inherit;True;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.BreakToComponentsNode;40;1555.985,-191.3086;Inherit;True;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.PowerNode;59;137.0304,-112.8284;Inherit;True;False;2;0;FLOAT;0;False;1;FLOAT;0.52;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;49;1556.026,242.24;Inherit;False;Property;_Opa;_Opa;5;0;Create;True;0;0;False;0;False;0;0.31;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;73;2052.557,-309.0934;Inherit;True;Property;_TextureSample0;Texture Sample 0;9;0;Create;True;0;0;False;0;False;-1;None;c9f48a1b5122cfd408c053f3df91cc79;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleSubtractOpNode;55;1098.175,-111.4525;Inherit;True;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.PowerNode;48;1879.007,-117.2818;Inherit;True;False;2;0;FLOAT;0;False;1;FLOAT;46.65;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;76;2410.658,-272.4638;Inherit;True;True;2;0;FLOAT;0;False;1;FLOAT;14.78;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;41;1642.342,-450.6588;Inherit;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.NegateNode;61;2472.208,127.9074;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;75;2765.6,-141.4175;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;58;3051.004,-372.5113;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;Fire_action;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;True;0;True;TransparentCutout;;Transparent;ForwardOnly;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;True;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;79;0;78;1
WireConnection;79;1;78;2
WireConnection;8;0;5;0
WireConnection;8;2;26;0
WireConnection;24;0;79;0
WireConnection;24;2;25;0
WireConnection;6;0;5;0
WireConnection;6;2;70;0
WireConnection;6;1;69;1
WireConnection;22;0;24;0
WireConnection;22;1;8;0
WireConnection;22;2;65;0
WireConnection;22;3;34;0
WireConnection;1;0;6;0
WireConnection;1;1;8;0
WireConnection;1;2;90;0
WireConnection;1;3;34;0
WireConnection;9;0;1;0
WireConnection;31;0;22;0
WireConnection;11;0;9;0
WireConnection;33;0;31;0
WireConnection;35;0;33;0
WireConnection;15;0;11;0
WireConnection;15;1;13;0
WireConnection;15;2;14;0
WireConnection;72;1;15;0
WireConnection;21;0;35;0
WireConnection;21;1;19;0
WireConnection;18;0;21;0
WireConnection;18;1;20;0
WireConnection;18;2;72;0
WireConnection;89;0;85;0
WireConnection;89;1;86;0
WireConnection;89;2;87;0
WireConnection;53;0;66;0
WireConnection;53;1;18;0
WireConnection;82;0;53;0
WireConnection;82;1;89;0
WireConnection;40;0;82;0
WireConnection;59;0;15;0
WireConnection;59;1;62;0
WireConnection;55;0;82;0
WireConnection;55;1;59;0
WireConnection;48;0;40;0
WireConnection;48;1;49;0
WireConnection;76;0;73;1
WireConnection;41;0;55;0
WireConnection;75;0;76;0
WireConnection;75;1;48;0
WireConnection;58;2;41;0
WireConnection;58;10;75;0
ASEEND*/
//CHKSM=C3A05972D5720D43D95B52DA7C039C16B80042E7