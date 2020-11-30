// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Portal_Void"
{
	Properties
	{
		_Voro_Speed("Voro_Speed", Float) = 0.05
		_Voro_Scale("Voro_Scale", Float) = 5.86
		_Voro_Angle("Voro_Angle", Float) = 1.5
		_Smoothness_Threshold("Smoothness_Threshold", Float) = 0.58
		_Color0("Color 0", Color) = (0,0.03998869,0.08627451,0)
		_Color1("Color 1", Color) = (0.6083254,0.5319509,0.8113208,0)
		_Color3("Color 3", Color) = (0,0.005931692,0.02745098,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Off
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Unlit keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float3 worldPos;
			float2 uv_texcoord;
		};

		uniform float4 _Color3;
		uniform float _Voro_Scale;
		uniform float _Voro_Angle;
		uniform float _Smoothness_Threshold;
		uniform float _Voro_Speed;
		uniform float4 _Color0;
		uniform float4 _Color1;


		float2 voronoihash2( float2 p )
		{
			
			p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
			return frac( sin( p ) *43758.5453);
		}


		float voronoi2( float2 v, float time, inout float2 id, inout float2 mr, float smoothness )
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
			 		float2 o = voronoihash2( n + g );
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


		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float2 temp_cast_0 = (_Voro_Angle).xx;
			float3 ase_worldPos = i.worldPos;
			float2 temp_cast_1 = (ase_worldPos.y).xx;
			float2 uv_TexCoord4 = i.uv_texcoord * temp_cast_1;
			float2 panner7 = ( 1.0 * _Time.y * temp_cast_0 + uv_TexCoord4);
			float time2 = panner7.x;
			float voronoiSmooth0 = _Smoothness_Threshold;
			float2 temp_cast_3 = (_Voro_Speed).xx;
			float2 panner6 = ( 1.0 * _Time.y * temp_cast_3 + uv_TexCoord4);
			float2 coords2 = panner6 * _Voro_Scale;
			float2 id2 = 0;
			float2 uv2 = 0;
			float fade2 = 0.5;
			float voroi2 = 0;
			float rest2 = 0;
			for( int it2 = 0; it2 <3; it2++ ){
			voroi2 += fade2 * voronoi2( coords2, time2, id2, uv2, voronoiSmooth0 );
			rest2 += fade2;
			coords2 *= 2;
			fade2 *= 0.5;
			}//Voronoi2
			voroi2 /= rest2;
			float4 lerpResult14 = lerp( _Color0 , _Color1 , voroi2);
			float4 smoothstepResult33 = smoothstep( float4( 0.08664116,0.3113208,0.2825901,0 ) , float4( 0.3962264,0.3962264,0.3962264,0 ) , ( ( _Color3 * voroi2 ) + lerpResult14 ));
			o.Emission = smoothstepResult33.rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18400
0;194;1920;817;1960.267;994.9471;1.668605;True;True
Node;AmplifyShaderEditor.WorldPosInputsNode;18;-2172.168,-317.9846;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;8;-1744,-16;Inherit;False;Property;_Voro_Angle;Voro_Angle;2;0;Create;True;0;0;False;0;False;1.5;1.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;5;-1744,-240;Inherit;False;Property;_Voro_Speed;Voro_Speed;0;0;Create;True;0;0;False;0;False;0.05;0.05;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;4;-1798.263,-146.6315;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;7;-1408,-80;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;9;-1408,256;Inherit;False;Property;_Smoothness_Threshold;Smoothness_Threshold;3;0;Create;True;0;0;False;0;False;0.58;0.58;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;3;-1408,160;Inherit;False;Property;_Voro_Scale;Voro_Scale;1;0;Create;True;0;0;False;0;False;5.86;5.86;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;6;-1408,-208;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.VoronoiNode;2;-1119.407,-96.54855;Inherit;True;0;0;1;0;3;False;1;False;True;4;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;3;FLOAT;0;FLOAT2;1;FLOAT2;2
Node;AmplifyShaderEditor.ColorNode;21;-659.9865,-666.7369;Inherit;False;Property;_Color3;Color 3;6;0;Create;True;0;0;False;0;False;0,0.005931692,0.02745098,0;0.4258707,0.9622642,0.1770203,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WireNode;31;-371.5925,-11.92169;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;13;-1056,-352;Inherit;False;Property;_Color1;Color 1;5;0;Create;True;0;0;False;0;False;0.6083254,0.5319509,0.8113208,0;1,1,1,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;11;-1057.406,-526.8828;Inherit;False;Property;_Color0;Color 0;4;0;Create;True;0;0;False;0;False;0,0.03998869,0.08627451,0;0,0.1309173,0.2735849,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;30;-359.6337,-509.0411;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;14;-738.5458,-440.9871;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;32;-99.35464,-357.3495;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;19;-1948.168,-199.9846;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SmoothstepOpNode;33;178.0875,-376.0599;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0.08664116,0.3113208,0.2825901,0;False;2;COLOR;0.3962264,0.3962264,0.3962264,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;532.7131,-399.7836;Float;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;Portal_Void;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;True;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;4;0;18;2
WireConnection;7;0;4;0
WireConnection;7;2;8;0
WireConnection;6;0;4;0
WireConnection;6;2;5;0
WireConnection;2;0;6;0
WireConnection;2;1;7;0
WireConnection;2;2;3;0
WireConnection;2;3;9;0
WireConnection;31;0;2;0
WireConnection;30;0;21;0
WireConnection;30;1;31;0
WireConnection;14;0;11;0
WireConnection;14;1;13;0
WireConnection;14;2;2;0
WireConnection;32;0;30;0
WireConnection;32;1;14;0
WireConnection;33;0;32;0
WireConnection;0;2;33;0
ASEEND*/
//CHKSM=B0E82D877F24E29BAE84C569AE72CC1A286672A2