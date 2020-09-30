// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Card"
{
	Properties
	{
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		_TextureSample1("Texture Sample 1", 2D) = "white" {}
		_TextureSample2("Texture Sample 2", 2D) = "white" {}
		_Offset("Offset", Range( 0 , 10)) = 3
		_TextureSample3("Texture Sample 3", 2D) = "white" {}
		_TextureSample4("Texture Sample 4", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}

	}
	
	SubShader
	{
		
		
		Tags { "RenderType"="Opaque" }
	LOD 100

		CGINCLUDE
		#pragma target 3.0
		ENDCG
		Blend Off
		AlphaToMask Off
		Cull Back
		ColorMask RGBA
		ZWrite On
		ZTest LEqual
		Offset 0 , 0
		
		
		
		Pass
		{
			Name "Unlit"
			Tags { "LightMode"="ForwardBase" }
			CGPROGRAM

			

			#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
			//only defining to not throw compilation error over Unity 5.5
			#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
			#endif
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"
			#include "UnityShaderVariables.cginc"
			#if defined(SHADER_API_D3D11) || defined(SHADER_API_XBOXONE) || defined(UNITY_COMPILER_HLSLCC) || defined(SHADER_API_PSSL) || (defined(SHADER_TARGET_SURFACE_ANALYSIS) && !defined(SHADER_TARGET_SURFACE_ANALYSIS_MOJOSHADER))//ASE Sampler Macros
			#define SAMPLE_TEXTURE2D(tex,samplerTex,coord) tex.Sample(samplerTex,coord)
			#else//ASE Sampling Macros
			#define SAMPLE_TEXTURE2D(tex,samplerTex,coord) tex2D(tex,coord)
			#endif//ASE Sampling Macros
			


			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				float3 worldPos : TEXCOORD0;
				#endif
				float4 ase_texcoord1 : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			UNITY_DECLARE_TEX2D_NOSAMPLER(_TextureSample0);
			uniform float4 _TextureSample0_ST;
			SamplerState sampler_linear_repeat;
			UNITY_DECLARE_TEX2D_NOSAMPLER(_TextureSample1);
			SamplerState sampler_TextureSample1;
			uniform float _Offset;
			UNITY_DECLARE_TEX2D_NOSAMPLER(_TextureSample3);
			SamplerState sampler_TextureSample3;
			UNITY_DECLARE_TEX2D_NOSAMPLER(_TextureSample4);
			SamplerState sampler_TextureSample4;
			UNITY_DECLARE_TEX2D_NOSAMPLER(_TextureSample2);
			SamplerState sampler_TextureSample2;
			SamplerState sampler_TextureSample0;
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
			

			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				o.ase_texcoord1.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord1.zw = 0;
				float3 vertexValue = float3(0, 0, 0);
				#if ASE_ABSOLUTE_VERTEX_POS
				vertexValue = v.vertex.xyz;
				#endif
				vertexValue = vertexValue;
				#if ASE_ABSOLUTE_VERTEX_POS
				v.vertex.xyz = vertexValue;
				#else
				v.vertex.xyz += vertexValue;
				#endif
				o.vertex = UnityObjectToClipPos(v.vertex);

				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				#endif
				return o;
			}
			
			fixed4 frag (v2f i , half ase_vface : VFACE) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				fixed4 finalColor;
				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				float3 WorldPosition = i.worldPos;
				#endif
				float2 uv_TextureSample0 = i.ase_texcoord1.xy * _TextureSample0_ST.xy + _TextureSample0_ST.zw;
				float4 tex2DNode1 = SAMPLE_TEXTURE2D( _TextureSample0, sampler_linear_repeat, uv_TextureSample0 );
				float temp_output_5_0 = ( _Offset * 0.01 );
				float4 appendResult9 = (float4(temp_output_5_0 , 0.0 , 0.0 , 0.0));
				float2 texCoord20 = i.ase_texcoord1.xy * float2( 1,1 ) + appendResult9.xy;
				float4 appendResult15 = (float4(0.0 , temp_output_5_0 , 0.0 , 0.0));
				float2 texCoord19 = i.ase_texcoord1.xy * float2( 1,1 ) + appendResult15.xy;
				float2 texCoord18 = i.ase_texcoord1.xy * float2( 1,1 ) + -appendResult9.xy;
				float2 texCoord22 = i.ase_texcoord1.xy * float2( 1,1 ) + -appendResult15.xy;
				float clampResult27 = clamp( ( ( SAMPLE_TEXTURE2D( _TextureSample1, sampler_TextureSample1, texCoord20 ).a + SAMPLE_TEXTURE2D( _TextureSample3, sampler_TextureSample3, texCoord19 ).a ) + ( SAMPLE_TEXTURE2D( _TextureSample4, sampler_TextureSample4, texCoord18 ).a + SAMPLE_TEXTURE2D( _TextureSample2, sampler_TextureSample2, texCoord22 ).a ) ) , 0.0 , 1.0 );
				float4 color45 = IsGammaSpace() ? float4(0.1113715,1,0,0) : float4(0.0118791,1,0,0);
				float4 appendResult34 = (float4(0.0 , -( 0.5 * _Time.y ) , 0.0 , 0.0));
				float2 texCoord35 = i.ase_texcoord1.xy * float2( 1,1 ) + appendResult34.xy;
				float simplePerlin2D36 = snoise( texCoord35*10.0 );
				simplePerlin2D36 = simplePerlin2D36*0.5 + 0.5;
				float4 color42 = IsGammaSpace() ? float4(1,0,0,0) : float4(1,0,0,0);
				float switchResult47 = (((ase_vface>0)?(simplePerlin2D36):(( 1.0 - simplePerlin2D36 ))));
				float4 appendResult57 = (float4(0.0 , -( 0.5 * _Time.y ) , 0.0 , 0.0));
				float2 texCoord52 = i.ase_texcoord1.xy * float2( 1,1 ) + appendResult57.xy;
				float simplePerlin2D56 = snoise( texCoord52*10.0 );
				simplePerlin2D56 = simplePerlin2D56*0.5 + 0.5;
				float4 color61 = IsGammaSpace() ? float4(1,0.1020897,0,0) : float4(1,0.0103502,0,0);
				float4 temp_cast_6 = (1.26).xxxx;
				
				
				finalColor = ( tex2DNode1 + ( ( clampResult27 - tex2DNode1.a ) * ( ( color45 * simplePerlin2D36 ) + ( color42 * switchResult47 ) + pow( ( simplePerlin2D56 * color61 ) , temp_cast_6 ) ) ) );
				return finalColor;
			}
			ENDCG
		}
	}
	CustomEditor "ASEMaterialInspector"
	
	
}
/*ASEBEGIN
Version=18400
7;274;1920;758;355.1128;-67.22278;1.6;True;False
Node;AmplifyShaderEditor.RangedFloatNode;8;-2214.634,581.9003;Inherit;False;Property;_Offset;Offset;3;0;Create;True;0;0;False;0;False;3;3;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;7;-2111.575,684.3081;Inherit;False;Constant;_Float0;Float 0;2;0;Create;True;0;0;False;0;False;0.01;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;30;-1980.124,2152.684;Inherit;False;Constant;_Float2;Float 2;5;0;Create;True;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TimeNode;38;-2000.963,2295.97;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;5;-1913.34,627.9316;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TimeNode;53;-846.2482,2680.63;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;55;-897.1334,2552.133;Inherit;False;Constant;_Float1;Float 1;6;0;Create;True;0;0;False;0;False;0.5;0;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;31;-1780.056,2206.341;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;15;-1636.626,752.6187;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.DynamicAppendNode;9;-1617.836,407.1636;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;54;-564.0165,2591.478;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;33;-1601.641,2216.336;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;64;-383.8523,2583.113;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;34;-1334.432,2127.189;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.NegateNode;21;-1635.993,1455.815;Inherit;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.NegateNode;11;-1696.902,1098.599;Inherit;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;22;-1437.423,1416.953;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;57;-189.0342,2521.606;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;18;-1429.866,1073.263;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;20;-1415.875,412.0344;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;19;-1398.918,771.4372;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;35;-1138.831,2016.198;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;3;-1108.696,316.0332;Inherit;True;Property;_TextureSample1;Texture Sample 1;1;0;Create;True;0;0;False;0;False;-1;9f5fe3894d05934429d526c3ca3f9661;9f5fe3894d05934429d526c3ca3f9661;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;14;-1080.978,1053.675;Inherit;True;Property;_TextureSample4;Texture Sample 4;5;0;Create;True;0;0;False;0;False;-1;9f5fe3894d05934429d526c3ca3f9661;9f5fe3894d05934429d526c3ca3f9661;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;52;5.564119,2420.279;Inherit;True;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NoiseGeneratorNode;36;-852.7814,1904.449;Inherit;True;Simplex2D;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;10;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;13;-1113.354,753.5698;Inherit;True;Property;_TextureSample3;Texture Sample 3;4;0;Create;True;0;0;False;0;False;-1;9f5fe3894d05934429d526c3ca3f9661;9f5fe3894d05934429d526c3ca3f9661;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;12;-1045.136,1394.201;Inherit;True;Property;_TextureSample2;Texture Sample 2;2;0;Create;True;0;0;False;0;False;-1;9f5fe3894d05934429d526c3ca3f9661;9f5fe3894d05934429d526c3ca3f9661;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NoiseGeneratorNode;56;331.3042,2405.205;Inherit;True;Simplex2D;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;10;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;40;-528.4257,2063.242;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;61;526.5444,2788.313;Inherit;False;Constant;_Color2;Color 2;6;0;Create;True;0;0;False;0;False;1,0.1020897,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;24;-701.8215,625.6379;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;23;-675.0983,1235.958;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;62;757.2341,2354.064;Inherit;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SwitchByFaceNode;47;-359.3706,2012.888;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;59;497.4104,2096.361;Inherit;False;Constant;_Float3;Float 3;6;0;Create;True;0;0;False;0;False;1.26;0;-2;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;42;-323.1469,1830.554;Inherit;False;Constant;_Color0;Color 0;5;0;Create;True;0;0;False;0;False;1,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;45;-541.2744,1684.291;Inherit;False;Constant;_Color1;Color 1;5;0;Create;True;0;0;False;0;False;0.1113715,1,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;26;-441.6997,822.7308;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerStateNode;2;-1433.88,-8.408436;Inherit;False;0;0;0;1;-1;1;0;SAMPLER2D;;False;1;SAMPLERSTATE;0
Node;AmplifyShaderEditor.PowerNode;58;865.6218,2093.67;Inherit;False;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;1;-1166.22,-122.8436;Inherit;True;Property;_TextureSample0;Texture Sample 0;0;0;Create;True;0;0;False;0;False;-1;9f5fe3894d05934429d526c3ca3f9661;9f5fe3894d05934429d526c3ca3f9661;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;44;-95.52423,1677.769;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;43;-121.1637,2024.915;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;27;-189.5597,613.3182;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;51;826.7035,1612.02;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;28;55.13101,399.1677;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;29;1096.483,702.3168;Inherit;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;49;1339.828,466.1514;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;1708.825,469.4737;Float;False;True;-1;2;ASEMaterialInspector;100;1;Card;0770190933193b94aaa3065e307002fa;True;Unlit;0;0;Unlit;2;True;0;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;True;0;False;-1;0;False;-1;False;False;False;False;False;False;True;0;False;-1;True;0;False;-1;True;True;True;True;True;0;False;-1;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;RenderType=Opaque=RenderType;True;2;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=ForwardBase;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;1;0;1;True;False;;True;0
WireConnection;5;0;8;0
WireConnection;5;1;7;0
WireConnection;31;0;30;0
WireConnection;31;1;38;2
WireConnection;15;1;5;0
WireConnection;9;0;5;0
WireConnection;54;0;55;0
WireConnection;54;1;53;2
WireConnection;33;0;31;0
WireConnection;64;0;54;0
WireConnection;34;1;33;0
WireConnection;21;0;15;0
WireConnection;11;0;9;0
WireConnection;22;1;21;0
WireConnection;57;1;64;0
WireConnection;18;1;11;0
WireConnection;20;1;9;0
WireConnection;19;1;15;0
WireConnection;35;1;34;0
WireConnection;3;1;20;0
WireConnection;14;1;18;0
WireConnection;52;1;57;0
WireConnection;36;0;35;0
WireConnection;13;1;19;0
WireConnection;12;1;22;0
WireConnection;56;0;52;0
WireConnection;40;0;36;0
WireConnection;24;0;3;4
WireConnection;24;1;13;4
WireConnection;23;0;14;4
WireConnection;23;1;12;4
WireConnection;62;0;56;0
WireConnection;62;1;61;0
WireConnection;47;0;36;0
WireConnection;47;1;40;0
WireConnection;26;0;24;0
WireConnection;26;1;23;0
WireConnection;58;0;62;0
WireConnection;58;1;59;0
WireConnection;1;7;2;0
WireConnection;44;0;45;0
WireConnection;44;1;36;0
WireConnection;43;0;42;0
WireConnection;43;1;47;0
WireConnection;27;0;26;0
WireConnection;51;0;44;0
WireConnection;51;1;43;0
WireConnection;51;2;58;0
WireConnection;28;0;27;0
WireConnection;28;1;1;4
WireConnection;29;0;28;0
WireConnection;29;1;51;0
WireConnection;49;0;1;0
WireConnection;49;1;29;0
WireConnection;0;0;49;0
ASEEND*/
//CHKSM=F219AF7843D823B05B634D65B294016E7EF2A704