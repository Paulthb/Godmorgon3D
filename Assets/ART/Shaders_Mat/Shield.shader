// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Shield"
{
	Properties
	{
		[HDR]_Color("Color", Color) = (0,16.96657,11.4032,0)
		_TextureSample1("Texture Sample 1", 2D) = "white" {}
		[HDR]_Color0("Color 0", Color) = (16.96657,0,12.45935,0)
		_Speedwhite("Speed white", Range( -6 , 3)) = 0
		_Powerint("Power int", Range( 0 , 6)) = 0
		_Intensity("Intensity", Range( -6 , 3)) = 0
		_FresnelPower("Fresnel Power", Range( 0 , 3)) = 0
		_FresnelScale("Fresnel Scale", Range( 0 , 0.5)) = 0
		_Offsetspeed("Offset speed", Float) = 0.37
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		Blend SrcAlpha OneMinusSrcAlpha
		
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
			float3 worldPos;
		};

		uniform float4 _Color;
		uniform float4 _Color0;
		UNITY_DECLARE_TEX2D_NOSAMPLER(_TextureSample1);
		uniform float _Offsetspeed;
		uniform float _Speedwhite;
		SamplerState sampler_TextureSample1;
		uniform float _Powerint;
		uniform float _Intensity;
		uniform float _FresnelScale;
		uniform float _FresnelPower;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float3 ase_worldNormal = i.worldNormal;
			float dotResult71 = dot( ase_worldNormal , i.viewDir );
			float4 lerpResult75 = lerp( _Color , _Color0 , dotResult71);
			o.Emission = lerpResult75.rgb;
			float4 temp_cast_1 = (0.0).xxxx;
			float4 temp_cast_2 = (1.0).xxxx;
			float2 temp_cast_3 = (_Offsetspeed).xx;
			float2 temp_cast_4 = (_Speedwhite).xx;
			float2 panner45 = ( 1.0 * _Time.y * temp_cast_4 + float2( 0,0 ));
			float2 uv_TexCoord46 = i.uv_texcoord * temp_cast_3 + panner45;
			float2 temp_cast_5 = (uv_TexCoord46.y).xx;
			float4 smoothstepResult59 = smoothstep( temp_cast_1 , temp_cast_2 , SAMPLE_TEXTURE2D( _TextureSample1, sampler_TextureSample1, temp_cast_5 ));
			float4 temp_cast_6 = (_Powerint).xxxx;
			float temp_output_2_0_g1 = _Intensity;
			float temp_output_3_0_g1 = ( 1.0 - temp_output_2_0_g1 );
			float3 appendResult7_g1 = (float3(temp_output_3_0_g1 , temp_output_3_0_g1 , temp_output_3_0_g1));
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float fresnelNdotV37 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode37 = ( 0.0 + _FresnelScale * pow( 1.0 - fresnelNdotV37, _FresnelPower ) );
			o.Alpha = ( ( ( pow( smoothstepResult59 , temp_cast_6 ).rgb * temp_output_2_0_g1 ) + appendResult7_g1 ) * fresnelNode37 ).x;
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
				surfIN.worldPos = worldPos;
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
0;12;1920;1007;5172.117;1721.001;3.06879;True;False
Node;AmplifyShaderEditor.RangedFloatNode;44;-2637.199,896.1976;Inherit;False;Property;_Speedwhite;Speed white;5;0;Create;True;0;0;False;0;False;0;-1.02;-6;3;0;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;45;-2222.628,911.3068;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;80;-2381.21,744.1555;Inherit;False;Property;_Offsetspeed;Offset speed;10;0;Create;True;0;0;False;0;False;0.37;1.18;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;46;-2022.894,734.4536;Inherit;True;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BreakToComponentsNode;47;-1793.125,779.9235;Inherit;False;FLOAT;1;0;FLOAT;0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.RangedFloatNode;66;-1398.455,1112.417;Inherit;False;Constant;_Float1;Float 1;5;0;Create;True;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;48;-1548.19,748.8054;Inherit;True;Property;_TextureSample1;Texture Sample 1;3;0;Create;True;0;0;False;0;False;-1;None;803941f5e2cb249498c9f95f90b0d415;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;65;-1431.3,994.4426;Inherit;False;Constant;_Float0;Float 0;5;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;64;-947.1375,1081.064;Inherit;False;Property;_Powerint;Power int;6;0;Create;True;0;0;False;0;False;0;2.35;0;6;0;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;59;-1117.51,807.4804;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.PowerNode;63;-613.8145,874.7285;Inherit;False;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;77;-1690.928,375.1679;Inherit;False;Property;_FresnelScale;Fresnel Scale;9;0;Create;True;0;0;False;0;False;0;0.288;0;0.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;69;-1809.333,-637.4408;Inherit;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldNormalVector;70;-1811.333,-787.4407;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;68;-508.4252,1072.78;Inherit;False;Property;_Intensity;Intensity;7;0;Create;True;0;0;False;0;False;0;-0.94;-6;3;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;76;-1682.293,485.5807;Inherit;False;Property;_FresnelPower;Fresnel Power;8;0;Create;True;0;0;False;0;False;0;3;0;3;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;72;-1080.949,-644.8785;Inherit;False;Property;_Color0;Color 0;4;1;[HDR];Create;True;0;0;False;0;False;16.96657,0,12.45935,0;15.27921,22.6517,21.44586,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FresnelNode;37;-1248.577,334.9543;Inherit;True;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;71;-1441.333,-483.4405;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;67;-306.9415,798.361;Inherit;False;Lerp White To;-1;;1;047d7c189c36a62438973bad9d37b1c2;0;2;1;FLOAT3;0,0,0;False;2;FLOAT;0.93;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ColorNode;74;-1078.217,-825.7728;Inherit;False;Property;_Color;Color;2;1;[HDR];Create;True;0;0;False;0;False;0,16.96657,11.4032,0;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;79;-3375.672,-269.6339;Inherit;False;2834.202;505.1274;Comment;10;85;86;83;81;60;24;29;26;38;31;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SmoothstepOpNode;60;-1822.631,-35.29935;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;1,1,1,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;24;-2258.738,-152.0013;Inherit;True;Property;_TextureSample0;Texture Sample 0;1;0;Create;True;0;0;False;0;False;-1;None;803941f5e2cb249498c9f95f90b0d415;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BreakToComponentsNode;29;-2485.186,-174.164;Inherit;False;FLOAT;1;0;FLOAT;0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.TextureCoordinatesNode;26;-2714.956,-219.6339;Inherit;True;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;38;-2978.397,-50.53156;Inherit;True;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;31;-3325.672,-58.41486;Inherit;False;Constant;_Speedblack;Speed black;2;0;Create;True;0;0;False;0;False;0.9712894;0.316;0;3;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;36;-158.0428,400.9114;Inherit;True;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;75;-532.9829,-590.8845;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RotatorNode;81;-1554.45,-32.23265;Inherit;True;3;0;FLOAT2;0,0;False;1;FLOAT2;0.93,-0.83;False;2;FLOAT;1.39;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DesaturateOpNode;86;-1280.831,-28.59295;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT;-4.38;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ColorNode;83;-1244.252,-224.4723;Inherit;False;Constant;_Color1;Color 1;11;0;Create;True;0;0;False;0;False;1,1,1,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;85;-1001.815,-31.28864;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;19;432.4247,26.69288;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;Shield;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;True;0;True;TransparentCutout;;Transparent;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;True;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;45;2;44;0
WireConnection;46;0;80;0
WireConnection;46;1;45;0
WireConnection;47;0;46;2
WireConnection;48;1;47;0
WireConnection;59;0;48;0
WireConnection;59;1;65;0
WireConnection;59;2;66;0
WireConnection;63;0;59;0
WireConnection;63;1;64;0
WireConnection;37;2;77;0
WireConnection;37;3;76;0
WireConnection;71;0;70;0
WireConnection;71;1;69;0
WireConnection;67;1;63;0
WireConnection;67;2;68;0
WireConnection;60;0;24;0
WireConnection;24;1;29;0
WireConnection;29;0;26;2
WireConnection;26;1;38;0
WireConnection;38;2;31;0
WireConnection;36;0;67;0
WireConnection;36;1;37;0
WireConnection;75;0;74;0
WireConnection;75;1;72;0
WireConnection;75;2;71;0
WireConnection;81;0;60;0
WireConnection;86;0;81;0
WireConnection;85;0;83;0
WireConnection;85;1;86;0
WireConnection;19;2;75;0
WireConnection;19;9;36;0
ASEEND*/
//CHKSM=52F1F409E24EA229D879005624A489ADD3D01E7D