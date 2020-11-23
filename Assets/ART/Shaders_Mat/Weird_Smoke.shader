// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Weird_Smoke"
{
	Properties
	{
		_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Particle Texture", 2D) = "white" {}
		_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
		_Scale("Scale", Float) = 0
		_Speed("Speed", Float) = 0
		_zefzefze("zefzefze", Float) = 0

	}


	Category 
	{
		SubShader
		{
		LOD 0

			Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMask RGB
			Cull Off
			Lighting Off 
			ZWrite Off
			ZTest LEqual
			
			Pass {
			
				CGPROGRAM
				
				#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
				#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
				#endif
				
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 2.0
				#pragma multi_compile_instancing
				#pragma multi_compile_particles
				#pragma multi_compile_fog
				#include "UnityShaderVariables.cginc"
				#define ASE_NEEDS_FRAG_COLOR


				#include "UnityCG.cginc"

				struct appdata_t 
				{
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float4 texcoord : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
					
				};

				struct v2f 
				{
					float4 vertex : SV_POSITION;
					fixed4 color : COLOR;
					float4 texcoord : TEXCOORD0;
					UNITY_FOG_COORDS(1)
					#ifdef SOFTPARTICLES_ON
					float4 projPos : TEXCOORD2;
					#endif
					UNITY_VERTEX_INPUT_INSTANCE_ID
					UNITY_VERTEX_OUTPUT_STEREO
					float4 ase_texcoord3 : TEXCOORD3;
				};
				
				
				#if UNITY_VERSION >= 560
				UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
				#else
				uniform sampler2D_float _CameraDepthTexture;
				#endif

				//Don't delete this comment
				// uniform sampler2D_float _CameraDepthTexture;

				uniform sampler2D _MainTex;
				uniform fixed4 _TintColor;
				uniform float4 _MainTex_ST;
				uniform float _InvFade;
				uniform float _Scale;
				uniform float _Speed;
				uniform float _zefzefze;
						float2 voronoihash2( float2 p )
						{
							p = p - 20 * floor( p / 20 );
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
				


				v2f vert ( appdata_t v  )
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					UNITY_TRANSFER_INSTANCE_ID(v, o);
					float3 ase_worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
					o.ase_texcoord3.xyz = ase_worldPos;
					
					
					//setting value to unused interpolator channels and avoid initialization warnings
					o.ase_texcoord3.w = 0;

					v.vertex.xyz +=  float3( 0, 0, 0 ) ;
					o.vertex = UnityObjectToClipPos(v.vertex);
					#ifdef SOFTPARTICLES_ON
						o.projPos = ComputeScreenPos (o.vertex);
						COMPUTE_EYEDEPTH(o.projPos.z);
					#endif
					o.color = v.color;
					o.texcoord = v.texcoord;
					UNITY_TRANSFER_FOG(o,o.vertex);
					return o;
				}

				fixed4 frag ( v2f i  ) : SV_Target
				{
					UNITY_SETUP_INSTANCE_ID( i );
					UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( i );

					#ifdef SOFTPARTICLES_ON
						float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
						float partZ = i.projPos.z;
						float fade = saturate (_InvFade * (sceneZ-partZ));
						i.color.a *= fade;
					#endif

					float time2 = _Time.z;
					float voronoiSmooth0 = 0.44;
					float2 temp_cast_0 = (_Speed).xx;
					float2 texCoord4 = i.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
					float2 panner7 = ( _Time.y * temp_cast_0 + texCoord4);
					float2 coords2 = panner7 * _Scale;
					float2 id2 = 0;
					float2 uv2 = 0;
					float fade2 = 0.5;
					float voroi2 = 0;
					float rest2 = 0;
					for( int it2 = 0; it2 <5; it2++ ){
					voroi2 += fade2 * voronoi2( coords2, time2, id2, uv2, voronoiSmooth0 );
					rest2 += fade2;
					coords2 *= 2;
					fade2 *= 0.5;
					}//Voronoi2
					voroi2 /= rest2;
					float3 ase_worldPos = i.ase_texcoord3.xyz;
					float3 temp_output_5_0_g1 = ( ( ase_worldPos - float3( 0,0,0 ) ) / 0.0 );
					float dotResult8_g1 = dot( temp_output_5_0_g1 , temp_output_5_0_g1 );
					float smoothstepResult55 = smoothstep( 0.0 , 2.0 , pow( saturate( dotResult8_g1 ) , 26.3 ));
					

					fixed4 col = ( ( i.color * voroi2 ) * ( pow( saturate( ( 1.0 - voroi2 ) ) , _zefzefze ) * smoothstepResult55 ) );
					UNITY_APPLY_FOG(i.fogCoord, col);
					return col;
				}
				ENDCG 
			}
		}	
	}
	CustomEditor "ASEMaterialInspector"
	
	
}
/*ASEBEGIN
Version=18400
0;0;1920;1019;1610.759;946.442;1.995018;True;True
Node;AmplifyShaderEditor.TextureCoordinatesNode;4;-1172.175,-129.2243;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;8;-1149.12,11.48331;Inherit;False;Property;_Speed;Speed;1;0;Create;True;0;0;False;0;False;0;0.19;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TimeNode;11;-1176.462,134.8737;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;7;-920.8347,-131.1835;Inherit;True;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;6;-841.1754,149.7937;Inherit;False;Property;_Scale;Scale;0;0;Create;True;0;0;False;0;False;0;4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;12;-730.7874,326.0939;Inherit;False;Constant;_Float0;Float 0;2;0;Create;True;0;0;False;0;False;0.44;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.VoronoiNode;2;-546.1754,-66.22433;Inherit;True;0;0;1;0;5;True;20;False;True;4;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;3;FLOAT;0;FLOAT2;1;FLOAT2;2
Node;AmplifyShaderEditor.OneMinusNode;51;-93.85962,27.89249;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;61;122.7222,-51.43478;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;53;287.0009,223.8922;Inherit;False;SphereMask;-1;;1;988803ee12caf5f4690caee3c8c4a5bb;0;3;15;FLOAT3;0,0,0;False;14;FLOAT;0;False;12;FLOAT;26.3;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;48;86.44907,208.2968;Inherit;False;Property;_zefzefze;zefzefze;3;0;Create;True;0;0;False;0;False;0;-10;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;49;-208.0513,-507.0179;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;47;436.1458,-57.44528;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;55;672.3651,187.4469;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;13;48,-336;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;54;797.3299,-107.8016;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;63;1028.843,-144.9755;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;14;-529.5302,-339.9546;Inherit;False;Property;_Color0;Color 0;2;0;Create;True;0;0;False;0;False;0.6642621,0.1722143,0.8490566,0;0,0.8345323,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;52;1611.281,-103.254;Float;False;True;-1;2;ASEMaterialInspector;0;7;Weird_Smoke;0b6a9f8b4f707c74ca64c0be8e590de0;True;SubShader 0 Pass 0;0;0;SubShader 0 Pass 0;2;True;2;5;False;-1;10;False;-1;0;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;True;2;False;-1;True;True;True;True;False;0;False;-1;False;False;False;False;True;2;False;-1;True;3;False;-1;False;True;4;Queue=Transparent=Queue=0;IgnoreProjector=True;RenderType=Transparent=RenderType;PreviewType=Plane;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;0;0;;0;0;Standard;0;0;1;True;False;;True;0
WireConnection;7;0;4;0
WireConnection;7;2;8;0
WireConnection;7;1;11;2
WireConnection;2;0;7;0
WireConnection;2;1;11;3
WireConnection;2;2;6;0
WireConnection;2;3;12;0
WireConnection;51;0;2;0
WireConnection;61;0;51;0
WireConnection;47;0;61;0
WireConnection;47;1;48;0
WireConnection;55;0;53;0
WireConnection;13;0;49;0
WireConnection;13;1;2;0
WireConnection;54;0;47;0
WireConnection;54;1;55;0
WireConnection;63;0;13;0
WireConnection;63;1;54;0
WireConnection;52;0;63;0
ASEEND*/
//CHKSM=EF7FC1B56B2AE9A5CA1B07F7AC5B7E5DF6639D4D