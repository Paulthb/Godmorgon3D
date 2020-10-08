// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Reflect"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 1
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		[HDR]_Color("Color", Color) = (0,0,0,0)
		_Speed("Speed", Float) = 0
		_Off("Off", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Overlay+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		Blend SrcAlpha OneMinusSrcAlpha
		
		AlphaToMask On
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
			float2 uv_texcoord;
		};

		uniform float4 _Color;
		UNITY_DECLARE_TEX2D_NOSAMPLER(_TextureSample0);
		uniform float _Speed;
		UNITY_DECLARE_TEX2D_NOSAMPLER(_Sampler6088);
		SamplerState sampler_Sampler6088;
		SamplerState sampler_TextureSample0;
		uniform float _Off;
		uniform float _Cutoff = 1;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			o.Emission = _Color.rgb;
			o.Alpha = 1;
			float mulTime95 = _Time.y * _Speed;
			float2 temp_output_1_0_g17 = float2( 1.64,0 );
			float2 appendResult10_g17 = (float2(( (temp_output_1_0_g17).x * i.uv_texcoord.x ) , ( i.uv_texcoord.y * (temp_output_1_0_g17).y )));
			float2 temp_output_11_0_g17 = float2( 3.19,0 );
			float2 panner18_g17 = ( ( (temp_output_11_0_g17).x * _Time.y ) * float2( 1,0 ) + i.uv_texcoord);
			float2 panner19_g17 = ( ( _Time.y * (temp_output_11_0_g17).y ) * float2( 0,1 ) + i.uv_texcoord);
			float2 appendResult24_g17 = (float2((panner18_g17).x , (panner19_g17).y));
			float2 temp_output_47_0_g17 = float2( 0,0 );
			float2 uv_TexCoord78_g17 = i.uv_texcoord * float2( 2,2 );
			float2 temp_output_31_0_g17 = ( uv_TexCoord78_g17 - float2( 1,1 ) );
			float2 appendResult39_g17 = (float2(frac( ( atan2( (temp_output_31_0_g17).x , (temp_output_31_0_g17).y ) / 6.28318548202515 ) ) , length( temp_output_31_0_g17 )));
			float2 panner54_g17 = ( ( (temp_output_47_0_g17).x * _Time.y ) * float2( 1,0 ) + appendResult39_g17);
			float2 panner55_g17 = ( ( _Time.y * (temp_output_47_0_g17).y ) * float2( 0,1 ) + appendResult39_g17);
			float2 appendResult58_g17 = (float2((panner54_g17).x , (panner55_g17).y));
			float2 smoothstepResult92 = smoothstep( float2( -1,-2 ) , float2( 2.46,1.84 ) , ( ( (SAMPLE_TEXTURE2D( _Sampler6088, sampler_Sampler6088, ( appendResult10_g17 + appendResult24_g17 ) )).rg * 0.0 ) + ( float2( 1,1 ) * appendResult58_g17 ) ));
			float2 panner106 = ( mulTime95 * float2( 0,-0.69 ) + smoothstepResult92);
			float2 uv_TexCoord89 = i.uv_texcoord * float2( -0.25,1 ) + panner106;
			float temp_output_1_0_g18 = SAMPLE_TEXTURE2D( _TextureSample0, sampler_TextureSample0, uv_TexCoord89 ).r;
			float smoothstepResult101 = smoothstep( 0.0 , 1.0 , ( ( 1.14 - temp_output_1_0_g18 ) / ( 0.18 - temp_output_1_0_g18 ) ));
			float clampResult102 = clamp( smoothstepResult101 , 0.0 , _Off );
			clip( clampResult102 - _Cutoff );
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			AlphaToMask Off
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
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
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
269;58;1132;380;2022.045;-216.9444;2.141122;True;False
Node;AmplifyShaderEditor.RangedFloatNode;94;-2015.916,784.5162;Inherit;False;Property;_Speed;Speed;3;0;Create;True;0;0;False;0;False;0;3.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;88;-2239.92,459.2317;Inherit;False;RadialUVDistortion;-1;;17;051d65e7699b41a4c800363fd0e822b2;0;7;60;SAMPLER2D;_Sampler6088;False;1;FLOAT2;1.64,0;False;11;FLOAT2;3.19,0;False;65;FLOAT;0;False;68;FLOAT2;1,1;False;47;FLOAT2;0,0;False;29;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SmoothstepOpNode;92;-1733.787,523.0591;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;-1,-2;False;2;FLOAT2;2.46,1.84;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleTimeNode;95;-1845.103,807.8257;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;106;-1496.339,557.4299;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,-0.69;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;89;-1223.402,431.1335;Inherit;True;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;-0.25,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;99;-826.2186,729.8251;Inherit;False;Constant;_Float0;Float 0;4;0;Create;True;0;0;False;0;False;0.18;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;100;-616.2186,772.8251;Inherit;False;Constant;_Float1;Float 1;4;0;Create;True;0;0;False;0;False;1.14;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;86;-959.2974,453.1216;Inherit;True;Property;_TextureSample0;Texture Sample 0;1;0;Create;True;0;0;False;0;False;-1;None;803941f5e2cb249498c9f95f90b0d415;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;98;-583.1547,356.5124;Inherit;True;Inverse Lerp;-1;;18;09cbe79402f023141a4dc1fddd4c9511;0;3;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;103;439.6544,688.5432;Inherit;False;Property;_Off;Off;4;0;Create;True;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;101;326.6848,375.386;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;108;-108.7466,182.3966;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;109;-418.8466,23.26628;Inherit;True;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;93;251.8133,-110.693;Inherit;False;Property;_Color;Color;2;1;[HDR];Create;True;0;0;False;0;False;0,0,0,0;1.717647,1.717647,1.717647,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;90;-2803.925,457.8734;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;102;642.6543,369.5431;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;87;-2522.893,453.0539;Inherit;True;Simplex2D;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldNormalVector;107;-791.6933,-56.15121;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TextureCoordinatesNode;110;-720.4949,170.7346;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PosVertexDataNode;111;-578.748,-124.5455;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;944.6533,69.17523;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;Reflect;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;1;True;True;0;True;Transparent;;Overlay;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;True;0;0;False;-1;-1;0;False;-1;0;0;0;False;1;False;-1;0;False;-1;True;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;92;0;88;0
WireConnection;95;0;94;0
WireConnection;106;0;92;0
WireConnection;106;1;95;0
WireConnection;89;1;106;0
WireConnection;86;1;89;0
WireConnection;98;1;86;0
WireConnection;98;2;99;0
WireConnection;98;3;100;0
WireConnection;101;0;98;0
WireConnection;108;1;109;0
WireConnection;109;0;111;3
WireConnection;109;1;110;0
WireConnection;102;0;101;0
WireConnection;102;2;103;0
WireConnection;87;0;90;0
WireConnection;0;2;93;0
WireConnection;0;10;102;0
ASEEND*/
//CHKSM=6D7BD02183573627AD2E78C7F1ACB77E9E5E0C6C