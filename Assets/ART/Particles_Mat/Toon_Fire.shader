// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Toon_Fire"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		_Outer_Fire("Outer_Fire", Float) = 0.9
		_Inner_Fire("Inner_Fire", Float) = 0.84
		_Float3("_Speed", Float) = -0.29
		[HDR]_Color0("Color 0", Color) = (4,0.6836113,0,0)
		[HDR]_Color1("Color 1", Color) = (0.8117647,0.1668144,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
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

		uniform float4 _Color0;
		UNITY_DECLARE_TEX2D_NOSAMPLER(_TextureSample0);
		SamplerState sampler_TextureSample0;
		uniform float4 _TextureSample0_ST;
		uniform float _Float3;
		uniform float _Outer_Fire;
		uniform float _Inner_Fire;
		uniform float4 _Color1;
		uniform float _Cutoff = 0.5;


		float3 mod2D289( float3 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float2 mod2D289( float2 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float3 permute( float3 x ) { return mod2D289( ( ( x * 34.0 ) + 1.0 ) * x ); }

		float snoise( float2 v )
		{
			const float4 C = float4( 0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439 );
			float2 i = floor( v + dot( v, C.yy ) );
			float2 x0 = v - i + dot( i, C.xx );
			float2 i1;
			i1 = ( x0.x > x0.y ) ? float2( 1.0, 0.0 ) : float2( 0.0, 1.0 );
			float4 x12 = x0.xyxy + C.xxzz;
			x12.xy -= i1;
			i = mod2D289( i );
			float3 p = permute( permute( i.y + float3( 0.0, i1.y, 1.0 ) ) + i.x + float3( 0.0, i1.x, 1.0 ) );
			float3 m = max( 0.5 - float3( dot( x0, x0 ), dot( x12.xy, x12.xy ), dot( x12.zw, x12.zw ) ), 0.0 );
			m = m * m;
			m = m * m;
			float3 x = 2.0 * frac( p * C.www ) - 1.0;
			float3 h = abs( x ) - 0.5;
			float3 ox = floor( x + 0.5 );
			float3 a0 = x - ox;
			m *= 1.79284291400159 - 0.85373472095314 * ( a0 * a0 + h * h );
			float3 g;
			g.x = a0.x * x0.x + h.x * x0.y;
			g.yz = a0.yz * x12.xz + h.yz * x12.yw;
			return 130.0 * dot( m, g );
		}


		float2 voronoihash18( float2 p )
		{
			
			p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
			return frac( sin( p ) *43758.5453);
		}


		float voronoi18( float2 v, float time, inout float2 id, inout float2 mr, float smoothness )
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
			 		float2 o = voronoihash18( n + g );
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
			float2 uv_TextureSample0 = i.uv_texcoord * _TextureSample0_ST.xy + _TextureSample0_ST.zw;
			float4 tex2DNode13 = SAMPLE_TEXTURE2D( _TextureSample0, sampler_TextureSample0, uv_TextureSample0 );
			float2 appendResult23 = (float2(0.13 , _Float3));
			float2 uv_TexCoord3 = i.uv_texcoord * float2( 1,0.75 );
			float2 temp_output_8_0 = ( uv_TexCoord3 * 1.6 );
			float2 panner21 = ( 1.0 * _Time.y * appendResult23 + temp_output_8_0);
			float simplePerlin2D20 = snoise( panner21*4.92 );
			float time18 = 0.0;
			float2 appendResult12 = (float2(0.0 , _Float3));
			float2 panner7 = ( 1.0 * _Time.y * appendResult12 + temp_output_8_0);
			float2 coords18 = panner7 * 4.06;
			float2 id18 = 0;
			float2 uv18 = 0;
			float voroi18 = voronoi18( coords18, time18, id18, uv18, 0 );
			float temp_output_17_0 = ( 1.0 - ( pow( tex2DNode13.r , 1.11 ) * ( ( tex2DNode13.r * 0.19 ) + ( simplePerlin2D20 * voroi18 ) ) ) );
			float temp_output_29_0 = step( temp_output_17_0 , _Inner_Fire );
			float4 temp_output_36_0 = ( ( _Color0 * ( step( temp_output_17_0 , _Outer_Fire ) - temp_output_29_0 ) ) + ( _Color1 * temp_output_29_0 ) );
			o.Emission = temp_output_36_0.rgb;
			o.Alpha = 1;
			clip( temp_output_36_0.r - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18400
0;6;1920;1013;1143.382;483.6105;1;True;True
Node;AmplifyShaderEditor.RangedFloatNode;24;-2426.342,-175.7495;Inherit;False;Property;_Float3;_Speed;4;0;Create;True;0;0;False;0;False;-0.29;-0.76;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;9;-2420.936,189.3708;Inherit;False;Constant;_Float1;Float 1;0;0;Create;True;0;0;False;0;False;1.6;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;3;-2533.461,-48.81311;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,0.75;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;23;-2181.193,-195.1537;Inherit;False;FLOAT2;4;0;FLOAT;0.13;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;12;-2059.563,431.0371;Inherit;True;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;8;-2109.935,114.3709;Inherit;True;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;7;-1797.535,302.1709;Inherit;True;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;21;-1911.94,-227.5146;Inherit;True;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;20;-1580.277,-205.2614;Inherit;True;Simplex2D;False;False;2;0;FLOAT2;0,0;False;1;FLOAT;4.92;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;13;-891.5864,-327.4757;Inherit;True;Property;_TextureSample0;Texture Sample 0;1;0;Create;True;0;0;False;0;False;-1;2181bd441a6d261469d5fa4f8a5febc6;b4c9dd42b792ba546839d69c294c3cef;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VoronoiNode;18;-1457.004,113.7152;Inherit;True;0;0;1;0;1;False;1;False;False;4;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;4.06;False;3;FLOAT;0;False;3;FLOAT;0;FLOAT2;1;FLOAT2;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;22;-1091.637,67.42316;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;27;-463.1362,-151.2009;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.19;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;26;-331.7813,84.92007;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;28;-217.1362,-167.2009;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1.11;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;14;4.18212,22.85541;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;30;209.6269,407.8646;Inherit;False;Property;_Inner_Fire;Inner_Fire;3;0;Create;True;0;0;False;0;False;0.84;0.805;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;6;168.566,-204.2254;Inherit;False;Property;_Outer_Fire;Outer_Fire;2;0;Create;True;0;0;False;0;False;0.9;0.875;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;17;234.6682,27.99309;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;29;530.6269,276.8646;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;5;566.0369,-121.7023;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;31;858.5217,-103.4705;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;35;845.6336,589.2706;Inherit;False;Property;_Color1;Color 1;6;1;[HDR];Create;True;0;0;False;0;False;0.8117647,0.1668144,0,0;1.62353,0.4477948,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;34;890.291,-361.6638;Inherit;False;Property;_Color0;Color 0;5;1;[HDR];Create;True;0;0;False;0;False;4,0.6836113,0,0;2.118547,0.7002554,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;32;1078.522,264.5295;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;33;1114.835,-103.6212;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;36;1387.269,93.12662;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1739.927,-147.8481;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;Toon_Fire;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;True;0;True;TransparentCutout;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;True;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;23;1;24;0
WireConnection;12;1;24;0
WireConnection;8;0;3;0
WireConnection;8;1;9;0
WireConnection;7;0;8;0
WireConnection;7;2;12;0
WireConnection;21;0;8;0
WireConnection;21;2;23;0
WireConnection;20;0;21;0
WireConnection;18;0;7;0
WireConnection;22;0;20;0
WireConnection;22;1;18;0
WireConnection;27;0;13;1
WireConnection;26;0;27;0
WireConnection;26;1;22;0
WireConnection;28;0;13;1
WireConnection;14;0;28;0
WireConnection;14;1;26;0
WireConnection;17;0;14;0
WireConnection;29;0;17;0
WireConnection;29;1;30;0
WireConnection;5;0;17;0
WireConnection;5;1;6;0
WireConnection;31;0;5;0
WireConnection;31;1;29;0
WireConnection;32;0;35;0
WireConnection;32;1;29;0
WireConnection;33;0;34;0
WireConnection;33;1;31;0
WireConnection;36;0;33;0
WireConnection;36;1;32;0
WireConnection;0;2;36;0
WireConnection;0;10;36;0
ASEEND*/
//CHKSM=5A36CACE5C7EE12AF8BA7F4F29505BBA32467552