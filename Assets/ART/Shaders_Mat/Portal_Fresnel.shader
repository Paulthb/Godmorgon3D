// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Portal_Fresnel"
{
	Properties
	{
		[HDR]_Color("Color", Color) = (0,1.592157,2,0)
		_Scale("Scale", Float) = 1.05
		_Power("Power", Float) = 5.06
		_Smoothness("Smoothness", Float) = 0.25
		_Speed("Speed", Float) = 0
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
		struct Input
		{
			float3 worldPos;
			float3 worldNormal;
		};

		uniform float4 _Color;
		uniform float _Smoothness;
		uniform float _Speed;
		uniform float _Scale;
		uniform float _Power;


		float2 voronoihash10( float2 p )
		{
			
			p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
			return frac( sin( p ) *43758.5453);
		}


		float voronoi10( float2 v, float time, inout float2 id, inout float2 mr, float smoothness )
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
			 		float2 o = voronoihash10( n + g );
					o = ( sin( time + o * 6.2831 ) * 0.5 + 0.5 ); float2 r = f - g - o;
					float d = 0.5 * dot( r, r );
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
			float time10 = 0.0;
			float voronoiSmooth0 = _Smoothness;
			float2 temp_cast_0 = (_Speed).xx;
			float3 ase_worldPos = i.worldPos;
			float2 panner6 = ( 1.0 * _Time.y * temp_cast_0 + ase_worldPos.xy);
			float2 coords10 = panner6 * 4.79;
			float2 id10 = 0;
			float2 uv10 = 0;
			float fade10 = 0.5;
			float voroi10 = 0;
			float rest10 = 0;
			for( int it10 = 0; it10 <2; it10++ ){
			voroi10 += fade10 * voronoi10( coords10, time10, id10, uv10, voronoiSmooth0 );
			rest10 += fade10;
			coords10 *= 2;
			fade10 *= 0.5;
			}//Voronoi10
			voroi10 /= rest10;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = i.worldNormal;
			float fresnelNdotV2 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode2 = ( 0.0 + _Scale * pow( 1.0 - fresnelNdotV2, _Power ) );
			float4 lerpResult7 = lerp( _Color , float4( 0,0,0,0 ) , ( 1.0 - ( voroi10 * fresnelNode2 ) ));
			o.Emission = lerpResult7.rgb;
			o.Alpha = fresnelNode2;
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
				float3 worldPos : TEXCOORD1;
				float3 worldNormal : TEXCOORD2;
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
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.worldNormal = worldNormal;
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
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
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
0;0;1920;1019;4786.501;1735.803;3.41726;True;True
Node;AmplifyShaderEditor.RangedFloatNode;14;-1464.779,33.38029;Inherit;False;Property;_Speed;Speed;4;0;Create;True;0;0;False;0;False;0;0.31;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;16;-1623.522,-193.7943;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;5;-1280,288;Inherit;False;Property;_Scale;Scale;1;0;Create;True;0;0;False;0;False;1.05;11.72;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;4;-1280,384;Inherit;False;Property;_Power;Power;2;0;Create;True;0;0;False;0;False;5.06;5.8;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;6;-1224.662,-69.71642;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;13;-1239.845,98.10723;Inherit;False;Property;_Smoothness;Smoothness;3;0;Create;True;0;0;False;0;False;0.25;0.34;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;2;-935.9627,178.3261;Inherit;True;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.VoronoiNode;10;-963.514,-97.53527;Inherit;True;0;0;1;0;2;False;1;False;True;4;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;4.79;False;3;FLOAT;0;False;3;FLOAT;0;FLOAT2;1;FLOAT2;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;9;-656.6344,0.2990105;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;1;-949.2281,-318.584;Inherit;False;Property;_Color;Color;0;1;[HDR];Create;True;0;0;False;0;False;0,1.592157,2,0;0,0.5553691,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;8;-472.0333,6.86972;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;7;-335.4568,-268.8183;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.NormalVertexDataNode;15;-1703.078,-339.9679;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;337.3147,13.15458;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;Portal_Fresnel;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;5;True;True;0;False;Transparent;;Transparent;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;1;False;-1;0;False;-1;True;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;6;0;16;0
WireConnection;6;2;14;0
WireConnection;2;2;5;0
WireConnection;2;3;4;0
WireConnection;10;0;6;0
WireConnection;10;3;13;0
WireConnection;9;0;10;0
WireConnection;9;1;2;0
WireConnection;8;0;9;0
WireConnection;7;0;1;0
WireConnection;7;2;8;0
WireConnection;0;2;7;0
WireConnection;0;9;2;0
ASEEND*/
//CHKSM=1757151CE479E5CC6AE172F5EDAF7B674DD27857