// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Effect 1"
{
	Properties
	{
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		[HDR]_Color("Color", Color) = (0,16.96657,11.4032,0)
		[HDR]_Color0("Color 0", Color) = (16.96657,0,12.45935,0)
		_Speed("Speed", Float) = 3.516824
		_Noise_speed("Noise_speed", Float) = 6.9
		_Float0("Float 0", Float) = 6.9
		_TextureSample1("Texture Sample 1", 2D) = "white" {}
		_Intensity("Intensity", Float) = -4.72
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#if defined(SHADER_API_D3D11) || defined(SHADER_API_XBOXONE) || defined(UNITY_COMPILER_HLSLCC) || defined(SHADER_API_PSSL) || (defined(SHADER_TARGET_SURFACE_ANALYSIS) && !defined(SHADER_TARGET_SURFACE_ANALYSIS_MOJOSHADER))//ASE Sampler Macros
		#define SAMPLE_TEXTURE2D(tex,samplerTex,coord) tex.Sample(samplerTex,coord)
		#else//ASE Sampling Macros
		#define SAMPLE_TEXTURE2D(tex,samplerTex,coord) tex2D(tex,coord)
		#endif//ASE Sampling Macros

		struct Input
		{
			float3 worldNormal;
			float3 viewDir;
			float2 uv_texcoord;
		};

		uniform float4 _Color;
		uniform float4 _Color0;
		uniform float _Float0;
		UNITY_DECLARE_TEX2D_NOSAMPLER(_TextureSample1);
		SamplerState sampler_TextureSample1;
		uniform float4 _TextureSample1_ST;
		UNITY_DECLARE_TEX2D_NOSAMPLER(_TextureSample0);
		SamplerState sampler_TextureSample0;
		uniform float _Speed;
		uniform float _Intensity;
		uniform float _Noise_speed;


		//https://www.shadertoy.com/view/XdXGW8
		float2 GradientNoiseDir( float2 x )
		{
			const float2 k = float2( 0.3183099, 0.3678794 );
			x = x * k + k.yx;
			return -1.0 + 2.0 * frac( 16.0 * k * frac( x.x * x.y * ( x.x + x.y ) ) );
		}
		
		float GradientNoise( float2 UV, float Scale )
		{
			float2 p = UV * Scale;
			float2 i = floor( p );
			float2 f = frac( p );
			float2 u = f * f * ( 3.0 - 2.0 * f );
			return lerp( lerp( dot( GradientNoiseDir( i + float2( 0.0, 0.0 ) ), f - float2( 0.0, 0.0 ) ),
					dot( GradientNoiseDir( i + float2( 1.0, 0.0 ) ), f - float2( 1.0, 0.0 ) ), u.x ),
					lerp( dot( GradientNoiseDir( i + float2( 0.0, 1.0 ) ), f - float2( 0.0, 1.0 ) ),
					dot( GradientNoiseDir( i + float2( 1.0, 1.0 ) ), f - float2( 1.0, 1.0 ) ), u.x ), u.y );
		}


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


		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float3 ase_worldNormal = i.worldNormal;
			float dotResult34 = dot( ase_worldNormal , i.viewDir );
			float2 temp_cast_0 = (_Float0).xx;
			float2 temp_cast_1 = (i.uv_texcoord.x).xx;
			float2 panner39 = ( 1.0 * _Time.y * temp_cast_0 + temp_cast_1);
			float gradientNoise36 = GradientNoise(panner39,4.2);
			gradientNoise36 = gradientNoise36*0.5 + 0.5;
			float4 lerpResult28 = lerp( _Color , _Color0 , ( dotResult34 * gradientNoise36 ));
			o.Emission = lerpResult28.rgb;
			float2 uv_TextureSample1 = i.uv_texcoord * _TextureSample1_ST.xy + _TextureSample1_ST.zw;
			float2 temp_cast_3 = (_Speed).xx;
			float2 panner8 = ( 0.2 * _Time.y * temp_cast_3 + float2( 0,37.12 ));
			float2 uv_TexCoord6 = i.uv_texcoord + panner8;
			float2 temp_cast_4 = (uv_TexCoord6.x).xx;
			float3 temp_cast_5 = (( SAMPLE_TEXTURE2D( _TextureSample1, sampler_TextureSample1, uv_TextureSample1 ).a * SAMPLE_TEXTURE2D( _TextureSample0, sampler_TextureSample0, temp_cast_4 ).r )).xxx;
			float temp_output_2_0_g1 = 1.67;
			float temp_output_3_0_g1 = ( 1.0 - temp_output_2_0_g1 );
			float3 appendResult7_g1 = (float3(temp_output_3_0_g1 , temp_output_3_0_g1 , temp_output_3_0_g1));
			float3 temp_cast_6 = (_Intensity).xxx;
			float2 temp_cast_7 = (_Noise_speed).xx;
			float2 panner14 = ( 1.0 * _Time.y * temp_cast_7 + i.uv_texcoord);
			float simplePerlin2D11 = snoise( panner14*4.94 );
			simplePerlin2D11 = simplePerlin2D11*0.5 + 0.5;
			o.Alpha = ( pow( ( ( temp_cast_5 * temp_output_2_0_g1 ) + appendResult7_g1 ) , temp_cast_6 ) * simplePerlin2D11 ).x;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard alpha:fade keepalpha fullforwardshadows 

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
				float3 worldNormal : TEXCOORD3;
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
				o.worldNormal = worldNormal;
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
				surfIN.viewDir = worldViewDir;
				surfIN.worldNormal = IN.worldNormal;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
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
Version=18400
0;0;1920;1019;2480.159;614.8979;1.807965;True;False
Node;AmplifyShaderEditor.RangedFloatNode;7;-2532.554,230.6413;Inherit;False;Property;_Speed;Speed;3;0;Create;True;0;0;False;0;False;3.516824;-11.79;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;8;-2297.817,212.012;Inherit;False;3;0;FLOAT2;0,37.12;False;2;FLOAT2;0,1;False;1;FLOAT;0.2;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;6;-2102.198,210.7861;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;5;-1839.722,204.0921;Inherit;True;Property;_TextureSample0;Texture Sample 0;0;0;Create;True;0;0;False;0;False;-1;faafc70e2b2260c40974bfa20bc28d39;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;23;-1790.576,-79.50697;Inherit;True;Property;_TextureSample1;Texture Sample 1;6;0;Create;True;0;0;False;0;False;-1;faafc70e2b2260c40974bfa20bc28d39;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;37;-1274.031,-34.77514;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;38;-1153.733,90.67572;Inherit;False;Property;_Float0;Float 0;5;0;Create;True;0;0;False;0;False;6.9;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;39;-988.9427,-23.42053;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;24;-1373.777,154.6026;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;15;-866.8185,894.4296;Inherit;False;Property;_Noise_speed;Noise_speed;4;0;Create;True;0;0;False;0;False;6.9;24.6;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;12;-1085.829,754.3663;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldNormalVector;33;-1839.734,-489.0583;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;32;-1837.734,-339.0583;Inherit;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DotProductOpNode;34;-1469.734,-185.0583;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;21;-911.903,265.6135;Inherit;False;Lerp White To;-1;;1;047d7c189c36a62438973bad9d37b1c2;0;2;1;FLOAT3;0,0,0;False;2;FLOAT;1.67;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PannerNode;14;-728.028,684.3334;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;27;-540.7603,305.7397;Inherit;False;Property;_Intensity;Intensity;7;0;Create;True;0;0;False;0;False;-4.72;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;13;-613.4301,903.3441;Inherit;False;Constant;_Float1;Float 1;2;0;Create;True;0;0;False;0;False;4.94;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;36;-782.2083,-19.99089;Inherit;False;Gradient;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;4.2;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;26;-324.7603,244.7397;Inherit;False;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;11;-496.2859,634.6747;Inherit;True;Simplex2D;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;1;-1106.618,-527.3903;Inherit;False;Property;_Color;Color;1;1;[HDR];Create;True;0;0;False;0;False;0,16.96657,11.4032,0;0,1.498039,1.45098,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;29;-1109.35,-346.496;Inherit;False;Property;_Color0;Color 0;2;1;[HDR];Create;True;0;0;False;0;False;16.96657,0,12.45935,0;3.177073,0,2.012701,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;35;-706.2082,-159.9909;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;16;-71.22344,241.6841;Inherit;True;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;28;-561.3856,-292.502;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;963.6872,7.513796;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;Effect 1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;5;True;True;0;False;Transparent;;Transparent;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;1;False;-1;0;False;-1;True;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;8;2;7;0
WireConnection;6;1;8;0
WireConnection;5;1;6;1
WireConnection;39;0;37;1
WireConnection;39;2;38;0
WireConnection;24;0;23;4
WireConnection;24;1;5;1
WireConnection;34;0;33;0
WireConnection;34;1;32;0
WireConnection;21;1;24;0
WireConnection;14;0;12;0
WireConnection;14;2;15;0
WireConnection;36;0;39;0
WireConnection;26;0;21;0
WireConnection;26;1;27;0
WireConnection;11;0;14;0
WireConnection;11;1;13;0
WireConnection;35;0;34;0
WireConnection;35;1;36;0
WireConnection;16;0;26;0
WireConnection;16;1;11;0
WireConnection;28;0;1;0
WireConnection;28;1;29;0
WireConnection;28;2;35;0
WireConnection;0;2;28;0
WireConnection;0;9;16;0
ASEEND*/
//CHKSM=40E52918F76044CFE5BC9277EDC8BBF30FD615E0