// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Fat_Brick"
{
	Properties
	{
		_ASEOutlineWidth( "Outline Width", Float ) = 0.005
		_ASEOutlineColor( "Outline Color", Color ) = (0.1641906,0.1448469,0.3301887,0)
		_ToonRamp("Toon Ramp", 2D) = "white" {}
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		_GlobalTint("Global Tint", Color) = (0.6132076,0.6132076,0.6132076,1)
		_RimOffset("Rim Offset", Float) = 1
		_Rimpower("Rim power", Range( 0 , 1)) = 0
		_RimTint("Rim Tint", Color) = (0.4211463,0.8457575,0.9811321,1)
		_Specsmth("Spec smth", Range( 0 , 1)) = 0.9768803
		_min("min", Float) = 0
		_max("max", Float) = 0
		_Specintensity("Spec intensity", Range( 0 , 1)) = 0.5529412
		_Specmap("Spec map", 2D) = "white" {}
		_Speccolor("Spec color", Color) = (1,1,1,1)
		_Emissive_Intensity("Emissive_Intensity", Range( -4 , 0)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ }
		Cull Front
		CGPROGRAM
		#pragma target 3.0
		#pragma surface outlineSurf Outline nofog  keepalpha noshadow noambient novertexlights nolightmap nodynlightmap nodirlightmap nometa noforwardadd vertex:outlineVertexDataFunc 
		
		float4 _ASEOutlineColor;
		float _ASEOutlineWidth;
		void outlineVertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			v.vertex.xyz += ( v.normal * _ASEOutlineWidth );
		}
		inline half4 LightingOutline( SurfaceOutput s, half3 lightDir, half atten ) { return half4 ( 0,0,0, s.Alpha); }
		void outlineSurf( Input i, inout SurfaceOutput o )
		{
			o.Emission = _ASEOutlineColor.rgb;
			o.Alpha = 1;
		}
		ENDCG
		

		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IgnoreProjector" = "True" }
		Cull Back
		Blend One Zero , One One
		
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "UnityCG.cginc"
		#include "UnityShaderVariables.cginc"
		#include "Lighting.cginc"
		#pragma target 4.6
		#if defined(SHADER_API_D3D11) || defined(SHADER_API_XBOXONE) || defined(UNITY_COMPILER_HLSLCC) || defined(SHADER_API_PSSL) || (defined(SHADER_TARGET_SURFACE_ANALYSIS) && !defined(SHADER_TARGET_SURFACE_ANALYSIS_MOJOSHADER))//ASE Sampler Macros
		#define SAMPLE_TEXTURE2D(tex,samplerTex,coord) tex.Sample(samplerTex,coord)
		#else//ASE Sampling Macros
		#define SAMPLE_TEXTURE2D(tex,samplerTex,coord) tex2D(tex,coord)
		#endif//ASE Sampling Macros

		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float3 worldNormal;
			INTERNAL_DATA
			float2 uv_texcoord;
			float3 worldPos;
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

		uniform float _Emissive_Intensity;
		uniform float4 _GlobalTint;
		UNITY_DECLARE_TEX2D_NOSAMPLER(_ToonRamp);
		UNITY_DECLARE_TEX2D_NOSAMPLER(_TextureSample0);
		uniform float4 _TextureSample0_ST;
		SamplerState sampler_TextureSample0;
		SamplerState sampler_ToonRamp;
		uniform float _RimOffset;
		uniform float _Rimpower;
		uniform float4 _RimTint;
		uniform float _min;
		uniform float _max;
		uniform float _Specsmth;
		UNITY_DECLARE_TEX2D_NOSAMPLER(_Specmap);
		uniform float4 _Specmap_ST;
		SamplerState sampler_Specmap;
		uniform float4 _Speccolor;
		uniform float _Specintensity;

		inline half4 LightingStandardCustomLighting( inout SurfaceOutputCustomLightingCustom s, half3 viewDir, UnityGI gi )
		{
			UnityGIInput data = s.GIData;
			Input i = s.SurfInput;
			half4 c = 0;
			#ifdef UNITY_PASS_FORWARDBASE
			float ase_lightAtten = data.atten;
			if( _LightColor0.a == 0)
			ase_lightAtten = 0;
			#else
			float3 ase_lightAttenRGB = gi.light.color / ( ( _LightColor0.rgb ) + 0.000001 );
			float ase_lightAtten = max( max( ase_lightAttenRGB.r, ase_lightAttenRGB.g ), ase_lightAttenRGB.b );
			#endif
			#if defined(HANDLE_SHADOWS_BLENDING_IN_GI)
			half bakedAtten = UnitySampleBakedOcclusion(data.lightmapUV.xy, data.worldPos);
			float zDist = dot(_WorldSpaceCameraPos - data.worldPos, UNITY_MATRIX_V[2].xyz);
			float fadeDist = UnityComputeShadowFadeDistance(data.worldPos, zDist);
			ase_lightAtten = UnityMixRealtimeAndBakedShadows(data.atten, bakedAtten, UnityComputeShadowFade(fadeDist));
			#endif
			float4 Albedo28 = _GlobalTint;
			float2 uv_TextureSample0 = i.uv_texcoord * _TextureSample0_ST.xy + _TextureSample0_ST.zw;
			float4 normal22 = SAMPLE_TEXTURE2D( _TextureSample0, sampler_TextureSample0, uv_TextureSample0 );
			float3 ase_worldPos = i.worldPos;
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			float3 ase_worldlightDir = 0;
			#else //aseld
			float3 ase_worldlightDir = Unity_SafeNormalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			#endif //aseld
			float dotResult4 = dot( normalize( (WorldNormalVector( i , normal22.rgb )) ) , ase_worldlightDir );
			float normal_lightDir9 = dotResult4;
			float2 temp_cast_2 = ((normal_lightDir9*0.5 + 0.5)).xx;
			float4 Shadow16 = ( Albedo28 * SAMPLE_TEXTURE2D( _ToonRamp, sampler_ToonRamp, temp_cast_2 ) );
			#if defined(LIGHTMAP_ON) && ( UNITY_VERSION < 560 || ( defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK) && defined(SHADOWS_SCREEN) ) )//aselc
			float4 ase_lightColor = 0;
			#else //aselc
			float4 ase_lightColor = _LightColor0;
			#endif //aselc
			UnityGI gi36 = gi;
			float3 diffNorm36 = WorldNormalVector( i , normal22.rgb );
			gi36 = UnityGI_Base( data, 1, diffNorm36 );
			float3 indirectDiffuse36 = gi36.indirect.diffuse + diffNorm36 * 0.0001;
			float4 Lighting35 = ( Shadow16 * ( ase_lightColor * float4( ( indirectDiffuse36 + ase_lightAtten ) , 0.0 ) * ase_lightColor.a ) );
			float3 ase_worldViewDir = Unity_SafeNormalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float dotResult7 = dot( normalize( (WorldNormalVector( i , normal22.rgb )) ) , ase_worldViewDir );
			float normal_viewdir10 = dotResult7;
			float4 Rim51 = ( saturate( ( pow( ( 1.0 - saturate( ( _RimOffset + normal_viewdir10 ) ) ) , _Rimpower ) * ( normal_lightDir9 * ase_lightAtten ) ) ) * ( ase_lightColor * _RimTint ) );
			float dotResult70 = dot( ( ase_worldViewDir + _WorldSpaceLightPos0.xyz ) , normalize( (WorldNormalVector( i , normal22.rgb )) ) );
			float smoothstepResult75 = smoothstep( _min , _max , pow( dotResult70 , _Specsmth ));
			float2 uv_Specmap = i.uv_texcoord * _Specmap_ST.xy + _Specmap_ST.zw;
			float4 lerpResult88 = lerp( _Speccolor , ase_lightColor , 0.5);
			float4 Spec80 = ( ( ( smoothstepResult75 * ( SAMPLE_TEXTURE2D( _Specmap, sampler_Specmap, uv_Specmap ) * lerpResult88 ) ) * _Specintensity ) * ase_lightAtten );
			c.rgb = ( ( Lighting35 + Rim51 ) + Spec80 ).rgb;
			c.a = 1;
			return c;
		}

		inline void LightingStandardCustomLighting_GI( inout SurfaceOutputCustomLightingCustom s, UnityGIInput data, inout UnityGI gi )
		{
			s.GIData = data;
		}

		void surf( Input i , inout SurfaceOutputCustomLightingCustom o )
		{
			o.SurfInput = i;
			o.Normal = float3(0,0,1);
			float4 color92 = IsGammaSpace() ? float4(1,0.6627451,0,1) : float4(1,0.3967553,0,1);
			o.Albedo = ( color92 + _Emissive_Intensity ).rgb;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf StandardCustomLighting keepalpha fullforwardshadows exclude_path:deferred 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 4.6
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
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
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
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
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
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				SurfaceOutputCustomLightingCustom o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputCustomLightingCustom, o )
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
355;144;1920;1013;2604.097;-737.7763;2.492233;True;True
Node;AmplifyShaderEditor.CommentaryNode;40;-1563.207,-220.2479;Inherit;False;578;280;Comment;2;21;22;Normal;1,1,1,1;0;0
Node;AmplifyShaderEditor.SamplerNode;21;-1513.207,-170.2479;Inherit;True;Property;_TextureSample0;Texture Sample 0;1;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;22;-1209.207,-170.2479;Inherit;False;normal;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;24;-1042.547,149.8141;Inherit;False;22;normal;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;12;-1022.492,-674.0256;Inherit;False;912.4115;353.6499;Comment;5;23;9;4;5;3;Normal.Light;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;13;-837.9117,97.93362;Inherit;False;728.3055;417.9462;Comment;4;8;6;7;10;Normal.ViewDir;1,1,1,1;0;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;8;-759.6647,327.8802;Inherit;False;World;True;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldNormalVector;6;-787.9118,147.9338;Inherit;False;True;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;23;-989.9199,-627.335;Inherit;False;22;normal;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.WorldNormalVector;3;-794.7584,-634.3249;Inherit;False;True;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DotProductOpNode;7;-502.205,258.7383;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;5;-808.7517,-468.4752;Inherit;False;True;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DotProductOpNode;4;-527.6335,-525.8171;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;64;-700.9219,1455.57;Inherit;False;1849.826;563.6787;Comment;17;45;47;48;50;49;56;53;61;54;62;58;59;60;46;44;57;51;Rim Light;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;10;-334.6071,266.1097;Float;False;normal_viewdir;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;9;-368,-512;Float;False;normal_lightDir;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;91;-803.2884,2270;Inherit;False;2748.922;1171.428;Comment;23;65;69;67;66;68;89;70;87;86;71;82;72;73;88;74;75;90;83;76;77;79;78;80;Spec;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;20;8.783274,112.2448;Inherit;False;1101.53;607.5712;Comment;7;16;15;31;30;18;14;19;Shadow;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;29;39.18895,-1012.016;Inherit;False;916.507;479.0635;;4;27;25;28;26;Albedo;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;44;-650.9219,1602.081;Inherit;False;10;normal_viewdir;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;46;-597.7232,1508.39;Inherit;False;Property;_RimOffset;Rim Offset;4;0;Create;True;0;0;False;0;False;1;0.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;69;-753.2884,2631.525;Inherit;False;22;normal;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;14;58.78326,200.1772;Inherit;False;9;normal_lightDir;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;45;-397.671,1506.78;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;26;142.3593,-962.0162;Inherit;False;Property;_GlobalTint;Global Tint;3;0;Create;True;0;0;False;0;False;0.6132076,0.6132076,0.6132076,1;0.1411765,0.2142088,0.3294118,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;19;51.20016,292.1364;Inherit;False;Constant;_Float0;Float 0;1;0;Create;True;0;0;False;0;False;0.5;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldSpaceLightPos;67;-720.1563,2487.604;Inherit;False;0;3;FLOAT4;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;65;-656,2320;Inherit;False;World;True;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ScaleAndOffsetNode;18;244.9855,250.4482;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;28;731.6959,-821.8295;Float;False;Albedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;47;-224.3171,1510.708;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldNormalVector;68;-554.495,2621.169;Inherit;False;True;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.CommentaryNode;42;-820.5149,811.3237;Inherit;False;1398.169;530.5688;Comment;9;33;32;41;34;35;38;36;39;37;Lighting;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleAddOpNode;66;-379.5197,2393.385;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;15;419.8538,359.844;Inherit;True;Property;_ToonRamp;Toon Ramp;0;0;Create;True;0;0;False;0;False;-1;a3ffb7da37751a04583a248626cbc961;a3ffb7da37751a04583a248626cbc961;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;58;-170.8524,1727.438;Inherit;False;9;normal_lightDir;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;70;-188.2926,2496.793;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;31;480.4264,199.2001;Inherit;False;28;Albedo;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.LightAttenuation;59;-169.9288,1832.343;Inherit;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;48;-61.59402,1509.547;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;50;-185.6027,1610.992;Inherit;False;Property;_Rimpower;Rim power;5;0;Create;True;0;0;False;0;False;0;0.262;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LightColorNode;87;501.3389,3189.405;Inherit;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;71;-188.6694,2620.765;Inherit;False;Property;_Specsmth;Spec smth;7;0;Create;True;0;0;False;0;False;0.9768803;0.565;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;39;-770.5149,1127.753;Inherit;False;22;normal;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;86;443.8972,2991.062;Inherit;False;Property;_Speccolor;Spec color;12;0;Create;True;0;0;False;0;False;1,1,1,1;0,2.193451E-05,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;89;394.0713,3325.428;Inherit;False;Constant;_SpecTransi;Spec Transi;13;0;Create;True;0;0;False;0;False;0.5;0.29;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;88;678.3987,2978.885;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;73;160.5668,2658.175;Inherit;False;Property;_min;min;8;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LightAttenuation;37;-557.8147,1230.893;Inherit;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;30;720.7555,204.4616;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.PowerNode;49;123.7759,1505.57;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;60;51.90611,1771.022;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;72;143.5745,2522.698;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.IndirectDiffuseLighting;36;-601.9929,1128.296;Inherit;False;Tangent;1;0;FLOAT3;0,0,1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;74;165.305,2741.41;Inherit;False;Property;_max;max;9;0;Create;True;0;0;False;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;82;470.2626,2734.489;Inherit;True;Property;_Specmap;Spec map;11;0;Create;True;0;0;False;0;False;-1;f856fb062a80b1e4baa1bbc3a4809dd4;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;61;354.245,1506.782;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;56;436.7028,1807.249;Inherit;False;Property;_RimTint;Rim Tint;6;0;Create;True;0;0;False;0;False;0.4211463,0.8457575,0.9811321,1;0.6313726,0.321253,0.1333333,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;90;812.913,2825.643;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;38;-225.3944,1108.696;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;16;901.3673,252.6668;Inherit;False;Shadow;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LightColorNode;33;-289.0705,941.3237;Inherit;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.LightColorNode;53;452.2566,1686.631;Inherit;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.SmoothstepOpNode;75;384.5667,2529.175;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;54;716.1953,1772.473;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;32;-220.0705,861.3237;Inherit;False;16;Shadow;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;41;-49.46997,1009.543;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;83;913.7441,2545.235;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;76;1042.554,2662.853;Inherit;False;Property;_Specintensity;Spec intensity;10;0;Create;True;0;0;False;0;False;0.5529412;0.188;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;62;555.0514,1508.32;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;77;1295.385,2538.485;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;57;756.7255,1517.891;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LightAttenuation;79;1338.549,2333.99;Inherit;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;34;145.4641,903.3456;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;35;353.6537,915.6754;Inherit;False;Lighting;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;51;924.9044,1510.509;Inherit;False;Rim;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;78;1536.696,2520.223;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;17;1475.921,165.4304;Inherit;False;35;Lighting;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;52;1486.843,269.3901;Inherit;False;51;Rim;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;80;1721.634,2536.096;Inherit;False;Spec;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;84;1673.361,195.5518;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;81;1630.552,396.9723;Inherit;False;80;Spec;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;94;1411.59,-69.87686;Inherit;False;Property;_Emissive_Intensity;Emissive_Intensity;13;0;Create;True;0;0;False;0;False;0;0;-4;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;92;1418.499,-257.1446;Inherit;False;Constant;_Emissive;Emissive;13;0;Create;True;0;0;False;0;False;1,0.6627451,0,1;0.6603774,0.3879115,0,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;96;1768.628,76.40112;Inherit;False;Property;_Opcaity;Opcaity;14;0;Create;True;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;95;1777.042,-203.7138;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;27;467.9741,-765.4628;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;25;89.18895,-762.9526;Inherit;True;Property;_Albedo;Albedo;2;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;85;1805.219,279.6153;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;2;2068.732,-70.28638;Float;False;True;-1;6;ASEMaterialInspector;0;0;CustomLighting;Fat_Brick;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;ForwardOnly;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;35.3;10;25;False;0.73;True;0;5;False;-1;10;False;-1;4;1;False;-1;1;False;-1;0;False;-1;0;False;-1;0;True;0.005;0.1641906,0.1448469,0.3301887,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;True;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;22;0;21;0
WireConnection;6;0;24;0
WireConnection;3;0;23;0
WireConnection;7;0;6;0
WireConnection;7;1;8;0
WireConnection;4;0;3;0
WireConnection;4;1;5;0
WireConnection;10;0;7;0
WireConnection;9;0;4;0
WireConnection;45;0;46;0
WireConnection;45;1;44;0
WireConnection;18;0;14;0
WireConnection;18;1;19;0
WireConnection;18;2;19;0
WireConnection;28;0;26;0
WireConnection;47;0;45;0
WireConnection;68;0;69;0
WireConnection;66;0;65;0
WireConnection;66;1;67;1
WireConnection;15;1;18;0
WireConnection;70;0;66;0
WireConnection;70;1;68;0
WireConnection;48;0;47;0
WireConnection;88;0;86;0
WireConnection;88;1;87;0
WireConnection;88;2;89;0
WireConnection;30;0;31;0
WireConnection;30;1;15;0
WireConnection;49;0;48;0
WireConnection;49;1;50;0
WireConnection;60;0;58;0
WireConnection;60;1;59;0
WireConnection;72;0;70;0
WireConnection;72;1;71;0
WireConnection;36;0;39;0
WireConnection;61;0;49;0
WireConnection;61;1;60;0
WireConnection;90;0;82;0
WireConnection;90;1;88;0
WireConnection;38;0;36;0
WireConnection;38;1;37;0
WireConnection;16;0;30;0
WireConnection;75;0;72;0
WireConnection;75;1;73;0
WireConnection;75;2;74;0
WireConnection;54;0;53;0
WireConnection;54;1;56;0
WireConnection;41;0;33;0
WireConnection;41;1;38;0
WireConnection;41;2;33;2
WireConnection;83;0;75;0
WireConnection;83;1;90;0
WireConnection;62;0;61;0
WireConnection;77;0;83;0
WireConnection;77;1;76;0
WireConnection;57;0;62;0
WireConnection;57;1;54;0
WireConnection;34;0;32;0
WireConnection;34;1;41;0
WireConnection;35;0;34;0
WireConnection;51;0;57;0
WireConnection;78;0;77;0
WireConnection;78;1;79;0
WireConnection;80;0;78;0
WireConnection;84;0;17;0
WireConnection;84;1;52;0
WireConnection;95;0;92;0
WireConnection;95;1;94;0
WireConnection;27;1;25;0
WireConnection;85;0;84;0
WireConnection;85;1;81;0
WireConnection;2;0;95;0
WireConnection;2;13;85;0
ASEEND*/
//CHKSM=94DBF766E3BB196B244E9654A3DBCD15C0943D7B