// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SineVFX/MatCapPro/FirstLayerOpaque"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_FinalPower("Final Power", Float) = 1
		_FinalOpacity("Final Opacity", Range( 0 , 1)) = 1
		[Toggle(_FINALOPACITYDITHERENABLED_ON)] _FinalOpacityDitherEnabled("Final Opacity Dither Enabled", Float) = 1
		_MatCapTexture("MatCap Texture", 2D) = "white" {}
		_MatCapNormal("MatCap Normal", 2D) = "bump" {}
		_MatCapScale("MatCap Scale", Range( 0.9 , 1.1)) = 0.95
		_MatCapRotation("MatCap Rotation", Range( 0 , 360)) = 0
		[Toggle(_SINGLELAYERCOLORINGENABLED_ON)] _SingleLayerColoringEnabled("Single Layer Coloring Enabled", Float) = 0
		_Ramp("Ramp", 2D) = "white" {}
		_RampColorTint("Ramp Color Tint", Color) = (1,1,1,1)
		_RampTilingExp("Ramp Tiling Exp", Range( 0.2 , 4)) = 1
		_MaskGlowExp("Mask Glow Exp", Range( 0.2 , 8)) = 1
		_MaskGlowAmount("Mask Glow Amount", Range( 0 , 10)) = 0
		[Toggle(_WIREFRAMEENABLED_ON)] _WireframeEnabled("Wireframe Enabled", Float) = 0
		_WireframePower("Wireframe Power", Range( -100 , 100)) = 10
		_WireframeThickness("Wireframe Thickness", Range( 0 , 0.01)) = 10.92
		_WireframeFresnelExp("Wireframe Fresnel Exp", Range( 0.2 , 10)) = 1
		_WireframeGlowAmount("Wireframe Glow Amount", Range( 0 , 10)) = 0
		_OpacityTexture("Opacity Texture", 2D) = "white" {}
		_OpacityTextureChannel("Opacity Texture Channel", Vector) = (0,0,0,1)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "AlphaTest+0" "IsEmissive" = "true"  }
		Cull Off
		CGINCLUDE
		#include "UnityCG.cginc"
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#pragma shader_feature _SINGLELAYERCOLORINGENABLED_ON
		#pragma shader_feature _WIREFRAMEENABLED_ON
		#pragma shader_feature _FINALOPACITYDITHERENABLED_ON
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
			float4 screenPosition;
			float3 worldPos;
		};

		uniform sampler2D _MatCapTexture;
		uniform sampler2D _MatCapNormal;
		uniform float4 _MatCapNormal_ST;
		uniform float _MatCapScale;
		uniform float _MatCapRotation;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform float _WireframeThickness;
		uniform float _WireframePower;
		uniform float _WireframeFresnelExp;
		uniform sampler2D _Ramp;
		uniform float _RampTilingExp;
		uniform float4 _RampColorTint;
		uniform float _FinalPower;
		uniform float _MaskGlowExp;
		uniform float _MaskGlowAmount;
		uniform float _WireframeGlowAmount;
		uniform float _FinalOpacity;
		uniform sampler2D _OpacityTexture;
		uniform float4 _OpacityTexture_ST;
		uniform float4 _OpacityTextureChannel;
		uniform float _Cutoff = 0.5;


		inline float Dither4x4Bayer( int x, int y )
		{
			const float dither[ 16 ] = {
				 1,  9,  3, 11,
				13,  5, 15,  7,
				 4, 12,  2, 10,
				16,  8, 14,  6 };
			int r = y * 4 + x;
			return dither[r] / 16; // same # of instructions as pre-dividing due to compiler magic
		}


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float4 ase_screenPos = ComputeScreenPos( UnityObjectToClipPos( v.vertex ) );
			o.screenPosition = ase_screenPos;
		}

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			o.Normal = float3(0,0,1);
			float2 uv_MatCapNormal = i.uv_texcoord * _MatCapNormal_ST.xy + _MatCapNormal_ST.zw;
			float3 worldToViewDir2 = mul( UNITY_MATRIX_V, float4( (WorldNormalVector( i , UnpackNormal( tex2D( _MatCapNormal, uv_MatCapNormal ) ) )), 0 ) ).xyz;
			float2 appendResult5 = (float2((0.0 + (worldToViewDir2.x - -1.0) * (1.0 - 0.0) / (1.0 - -1.0)) , (0.0 + (worldToViewDir2.y - -1.0) * (1.0 - 0.0) / (1.0 - -1.0))));
			float2 temp_cast_0 = (0.5).xx;
			float cos11 = cos( (0.0 + (_MatCapRotation - 0.0) * (( -2.0 * UNITY_PI ) - 0.0) / (360.0 - 0.0)) );
			float sin11 = sin( (0.0 + (_MatCapRotation - 0.0) * (( -2.0 * UNITY_PI ) - 0.0) / (360.0 - 0.0)) );
			float2 rotator11 = mul( (float2( 0,0 ) + (((float2( -1,-1 ) + (appendResult5 - float2( 0,0 )) * (float2( 1,1 ) - float2( -1,-1 )) / (float2( 1,1 ) - float2( 0,0 )))*_MatCapScale + 0.0) - float2( -1,-1 )) * (float2( 1,1 ) - float2( 0,0 )) / (float2( 1,1 ) - float2( -1,-1 ))) - temp_cast_0 , float2x2( cos11 , -sin11 , sin11 , cos11 )) + temp_cast_0;
			float4 tex2DNode12 = tex2D( _MatCapTexture, rotator11 );
			float4 ase_screenPos = i.screenPosition;
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float eyeDepth37 = (SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy )*( _ProjectionParams.z - _ProjectionParams.y ));
			float4 appendResult41 = (float4(_WireframeThickness , 0.0 , 0.0 , 0.0));
			float eyeDepth58 = (SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ( ase_screenPosNorm + appendResult41 ).xy )*( _ProjectionParams.z - _ProjectionParams.y ));
			float4 appendResult42 = (float4(-_WireframeThickness , 0.0 , 0.0 , 0.0));
			float eyeDepth60 = (SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ( appendResult42 + ase_screenPosNorm ).xy )*( _ProjectionParams.z - _ProjectionParams.y ));
			float4 appendResult46 = (float4(0.0 , _WireframeThickness , 0.0 , 0.0));
			float eyeDepth57 = (SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ( appendResult46 + ase_screenPosNorm ).xy )*( _ProjectionParams.z - _ProjectionParams.y ));
			float4 appendResult44 = (float4(0.0 , -_WireframeThickness , 0.0 , 0.0));
			float eyeDepth59 = (SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ( appendResult44 + ase_screenPosNorm ).xy )*( _ProjectionParams.z - _ProjectionParams.y ));
			float4 appendResult48 = (float4(_WireframeThickness , _WireframeThickness , 0.0 , 0.0));
			float eyeDepth62 = (SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ( appendResult48 + ase_screenPosNorm ).xy )*( _ProjectionParams.z - _ProjectionParams.y ));
			float4 appendResult49 = (float4(-_WireframeThickness , -_WireframeThickness , 0.0 , 0.0));
			float eyeDepth65 = (SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ( ase_screenPosNorm + appendResult49 ).xy )*( _ProjectionParams.z - _ProjectionParams.y ));
			float4 appendResult52 = (float4(_WireframeThickness , -_WireframeThickness , 0.0 , 0.0));
			float eyeDepth64 = (SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ( ase_screenPosNorm + appendResult52 ).xy )*( _ProjectionParams.z - _ProjectionParams.y ));
			float4 appendResult76 = (float4(-_WireframeThickness , _WireframeThickness , 0.0 , 0.0));
			float eyeDepth63 = (SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ( ase_screenPosNorm + appendResult76 ).xy )*( _ProjectionParams.z - _ProjectionParams.y ));
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float fresnelNdotV81 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode81 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNdotV81, _WireframeFresnelExp ) );
			float clampResult79 = clamp( ( ( ( ( eyeDepth37 - eyeDepth58 ) + ( eyeDepth37 - eyeDepth60 ) + ( eyeDepth37 - eyeDepth57 ) + ( eyeDepth37 - eyeDepth59 ) ) + ( eyeDepth37 - eyeDepth62 ) + ( eyeDepth37 - eyeDepth65 ) + ( eyeDepth37 - eyeDepth64 ) + ( eyeDepth37 - eyeDepth63 ) ) * _WireframePower * fresnelNode81 ) , 0.0 , 1.0 );
			#ifdef _WIREFRAMEENABLED_ON
				float staticSwitch83 = clampResult79;
			#else
				float staticSwitch83 = 0.0;
			#endif
			float temp_output_80_0 = ( tex2DNode12.r + staticSwitch83 );
			float4 temp_cast_1 = (temp_output_80_0).xxxx;
			float2 appendResult23 = (float2(pow( temp_output_80_0 , _RampTilingExp ) , 0.0));
			#ifdef _SINGLELAYERCOLORINGENABLED_ON
				float4 staticSwitch22 = ( tex2D( _Ramp, appendResult23 ) * _RampColorTint * _FinalPower * ( ( pow( tex2DNode12.r , _MaskGlowExp ) * _MaskGlowAmount ) + 1.0 ) * ( ( staticSwitch83 * _WireframeGlowAmount ) + 1.0 ) );
			#else
				float4 staticSwitch22 = temp_cast_1;
			#endif
			o.Emission = staticSwitch22.rgb;
			o.Alpha = 1;
			float2 uv_OpacityTexture = i.uv_texcoord * _OpacityTexture_ST.xy + _OpacityTexture_ST.zw;
			float4 break96 = ( tex2D( _OpacityTexture, uv_OpacityTexture ) * _OpacityTextureChannel );
			float clampResult97 = clamp( max( max( max( break96.r , break96.g ) , break96.b ) , break96.a ) , 0.0 , 1.0 );
			float temp_output_88_0 = ( _FinalOpacity * clampResult97 );
			float2 clipScreen19 = ase_screenPosNorm.xy * _ScreenParams.xy;
			float dither19 = Dither4x4Bayer( fmod(clipScreen19.x, 4), fmod(clipScreen19.y, 4) );
			dither19 = step( dither19, temp_output_88_0 );
			#ifdef _FINALOPACITYDITHERENABLED_ON
				float staticSwitch98 = dither19;
			#else
				float staticSwitch98 = temp_output_88_0;
			#endif
			clip( staticSwitch98 - _Cutoff );
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Unlit keepalpha fullforwardshadows vertex:vertexDataFunc 

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
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float4 customPack2 : TEXCOORD2;
				float4 tSpace0 : TEXCOORD3;
				float4 tSpace1 : TEXCOORD4;
				float4 tSpace2 : TEXCOORD5;
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
				vertexDataFunc( v, customInputData );
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
				o.customPack2.xyzw = customInputData.screenPosition;
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
				surfIN.screenPosition = IN.customPack2.xyzw;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				SurfaceOutput o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutput, o )
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
Version=17001
1927;29;1906;1004;2036.958;670.9434;1;True;False
Node;AmplifyShaderEditor.RangedFloatNode;75;-4124.269,-1940.107;Float;False;Property;_WireframeThickness;Wireframe Thickness;16;0;Create;True;0;0;False;0;10.92;0.001;0;0.01;0;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;39;-3723.808,-1912.602;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;40;-3729.272,-2167.218;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;13;-4259.487,-772.6015;Float;True;Property;_MatCapNormal;MatCap Normal;5;0;Create;True;0;0;False;0;f064b075d97853246adfeb6483ef3a9e;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NegateNode;45;-3711.319,-1604.108;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;36;-3730.176,-1379.171;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;41;-3569.664,-2306.792;Float;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.DynamicAppendNode;46;-3558.8,-2030.622;Float;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ScreenPosInputsNode;43;-3641.935,-2494.067;Float;False;0;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;44;-3551.15,-1884.19;Float;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.DynamicAppendNode;42;-3563.171,-2170.497;Float;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.NegateNode;77;-3723.442,-1264.681;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldNormalVector;1;-3937.182,-765.4555;Float;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DynamicAppendNode;49;-3557.769,-1579.862;Float;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleAddOpNode;50;-3280.576,-2024.904;Float;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.DynamicAppendNode;52;-3556.422,-1431.7;Float;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleAddOpNode;53;-3285.607,-2284.145;Float;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleAddOpNode;51;-3280.143,-2161.755;Float;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleAddOpNode;47;-3291.571,-2400.516;Float;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.DynamicAppendNode;76;-3560.462,-1284.884;Float;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.DynamicAppendNode;48;-3557.769,-1733.413;Float;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TransformDirectionNode;2;-3716.39,-769.3075;Float;False;World;View;False;Fast;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ScreenDepthNode;60;-3118.413,-2339.876;Float;False;0;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenDepthNode;58;-3116.456,-2436.126;Float;False;0;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;55;-3277.607,-1728.024;Float;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleAddOpNode;54;-3272.219,-1616.229;Float;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleAddOpNode;56;-3273.566,-1505.781;Float;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ScreenDepthNode;59;-3108.579,-2130.064;Float;False;0;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;61;-3277.607,-1835.779;Float;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ScreenDepthNode;37;-3116.903,-2536.934;Float;False;0;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;3;-3426.598,-645.6905;Float;False;5;0;FLOAT;0;False;1;FLOAT;-1;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenDepthNode;57;-3117.321,-2236.063;Float;False;0;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;4;-3421.877,-818.9806;Float;False;5;0;FLOAT;0;False;1;FLOAT;-1;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenDepthNode;64;-3101.159,-1643.168;Float;False;0;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenDepthNode;63;-3102.506,-1558.311;Float;False;0;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenDepthNode;65;-3099.812,-1732.066;Float;False;0;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenDepthNode;62;-3098.465,-1822.31;Float;False;0;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;69;-2835.386,-2191.259;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;5;-3145.833,-719.7645;Float;True;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;67;-2835.386,-2289.609;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;68;-2839.02,-2491.325;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;66;-2841.357,-2388.288;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;82;-2458.675,-1166.845;Float;False;Property;_WireframeFresnelExp;Wireframe Fresnel Exp;17;0;Create;True;0;0;False;0;1;2;0.2;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;70;-2835.813,-1892.351;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;73;-2838.507,-1604.107;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;38;-2203.342,-2120.496;Float;False;4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;71;-2841.201,-1795.372;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;7;-2876.359,-868.289;Float;False;5;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT2;1,1;False;3;FLOAT2;-1,-1;False;4;FLOAT2;1,1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;6;-3190.559,-401.408;Float;False;Property;_MatCapScale;MatCap Scale;6;0;Create;True;0;0;False;0;0.95;0.95;0.9;1.1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;72;-2837.16,-1702.433;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;8;-2657.393,-620.311;Float;True;3;0;FLOAT2;0,0;False;1;FLOAT;0.93;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;34;-2232.132,-1467.422;Float;False;Property;_WireframePower;Wireframe Power;15;0;Create;True;0;0;False;0;10;100;-100;100;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;35;-2068.273,-1834.444;Float;False;5;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;81;-2140.999,-1280.914;Float;False;Standard;WorldNormal;ViewDir;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector4Node;90;-1360.786,1427.291;Float;False;Property;_OpacityTextureChannel;Opacity Texture Channel;20;0;Create;True;0;0;False;0;0,0,0,1;0,0,0,1;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;89;-1357.785,1224.291;Float;True;Property;_OpacityTexture;Opacity Texture;19;0;Create;True;0;0;False;0;None;be707501896621e43a122bb5dc187236;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PiNode;16;-2787.653,-51.29962;Float;False;1;0;FLOAT;-2;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;14;-2862.653,-255.2997;Float;False;Property;_MatCapRotation;MatCap Rotation;7;0;Create;True;0;0;False;0;0;0;0;360;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;10;-2517.706,-923.8701;Float;False;5;0;FLOAT2;0,0;False;1;FLOAT2;-1,-1;False;2;FLOAT2;1,1;False;3;FLOAT2;0,0;False;4;FLOAT2;1,1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;78;-1938.482,-1584.818;Float;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;15;-2543.652,-188.2997;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;360;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;95;-1037.786,1335.291;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;9;-2569.903,-381.4664;Float;False;Constant;_Float3;Float 3;6;0;Create;True;0;0;False;0;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;84;-1805.68,-1049.07;Float;False;Constant;_Float0;Float 0;16;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;79;-1756.746,-1570.12;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;96;-909.7855,1335.291;Float;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.RotatorNode;11;-2331.531,-424.5897;Float;True;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.StaticSwitch;83;-1594.144,-1113.613;Float;False;Property;_WireframeEnabled;Wireframe Enabled;14;0;Create;True;0;0;False;0;0;0;0;True;;Toggle;2;Key0;Key1;Create;False;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;12;-2065.635,-454.031;Float;True;Property;_MatCapTexture;MatCap Texture;4;0;Create;True;0;0;False;0;None;b9cc4c09ec58664439db66c490aa06f5;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMaxOpNode;91;-635.7856,1271.291;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;99;-1431.958,-233.9434;Float;False;Property;_RampTilingExp;Ramp Tiling Exp;11;0;Create;True;0;0;False;0;1;1;0.2;4;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;80;-1510.082,-597.1859;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;29;-1473.586,638.1913;Float;False;Property;_MaskGlowExp;Mask Glow Exp;12;0;Create;True;0;0;False;0;1;4;0.2;8;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;92;-502.7855,1356.291;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;100;-1122.958,-311.9434;Float;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;93;-357.7854,1435.291;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;85;-1370.116,1014.063;Float;False;Property;_WireframeGlowAmount;Wireframe Glow Amount;18;0;Create;True;0;0;False;0;0;0.5;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;31;-1162.887,616.0913;Float;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;30;-1369.586,757.7913;Float;False;Property;_MaskGlowAmount;Mask Glow Amount;13;0;Create;True;0;0;False;0;0;4;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;23;-1073.703,34.30792;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;17;-519.9183,703.9875;Float;False;Property;_FinalOpacity;Final Opacity;2;0;Create;True;0;0;False;0;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;86;-1089.272,927.7892;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;32;-1036.785,720.0913;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;97;-212.9469,1436.486;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;27;-793.9648,375.0335;Float;False;Property;_FinalPower;Final Power;1;0;Create;True;0;0;False;0;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;88;-215.8855,828.6906;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;87;-894.0011,964.5232;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;33;-871.2982,598.4984;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;26;-832.7026,202.3079;Float;False;Property;_RampColorTint;Ramp Color Tint;10;0;Create;True;0;0;False;0;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;24;-921.7026,9.307922;Float;True;Property;_Ramp;Ramp;9;0;Create;True;0;0;False;0;None;0edd1bcb97d343fa8d6600b71b560038;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;25;-550.7026,115.3079;Float;False;5;5;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.DitheringNode;19;-128.9182,703.9875;Float;False;0;False;3;0;FLOAT;0;False;1;SAMPLER2D;;False;2;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;22;-374.2905,-132.5049;Float;False;Property;_SingleLayerColoringEnabled;Single Layer Coloring Enabled;8;0;Create;True;0;0;False;0;0;0;1;True;;Toggle;2;Key0;Key1;Create;False;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;98;49.95865,583.4279;Float;False;Property;_FinalOpacityDitherEnabled;Final Opacity Dither Enabled;3;0;Create;True;0;0;False;0;0;1;0;True;;Toggle;2;Key0;Key1;Create;False;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;190.9433,2.93759;Float;False;True;2;Float;ASEMaterialInspector;0;0;Unlit;SineVFX/MatCapPro/FirstLayerOpaque;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Masked;0.5;True;True;0;False;TransparentCutout;;AlphaTest;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;39;0;75;0
WireConnection;40;0;75;0
WireConnection;45;0;75;0
WireConnection;36;0;75;0
WireConnection;41;0;75;0
WireConnection;46;1;75;0
WireConnection;44;1;39;0
WireConnection;42;0;40;0
WireConnection;77;0;75;0
WireConnection;1;0;13;0
WireConnection;49;0;45;0
WireConnection;49;1;45;0
WireConnection;50;0;44;0
WireConnection;50;1;43;0
WireConnection;52;0;75;0
WireConnection;52;1;36;0
WireConnection;53;0;42;0
WireConnection;53;1;43;0
WireConnection;51;0;46;0
WireConnection;51;1;43;0
WireConnection;47;0;43;0
WireConnection;47;1;41;0
WireConnection;76;0;77;0
WireConnection;76;1;75;0
WireConnection;48;0;75;0
WireConnection;48;1;75;0
WireConnection;2;0;1;0
WireConnection;60;0;53;0
WireConnection;58;0;47;0
WireConnection;55;0;43;0
WireConnection;55;1;49;0
WireConnection;54;0;43;0
WireConnection;54;1;52;0
WireConnection;56;0;43;0
WireConnection;56;1;76;0
WireConnection;59;0;50;0
WireConnection;61;0;48;0
WireConnection;61;1;43;0
WireConnection;37;0;43;0
WireConnection;3;0;2;2
WireConnection;57;0;51;0
WireConnection;4;0;2;1
WireConnection;64;0;54;0
WireConnection;63;0;56;0
WireConnection;65;0;55;0
WireConnection;62;0;61;0
WireConnection;69;0;37;0
WireConnection;69;1;59;0
WireConnection;5;0;4;0
WireConnection;5;1;3;0
WireConnection;67;0;37;0
WireConnection;67;1;57;0
WireConnection;68;0;37;0
WireConnection;68;1;58;0
WireConnection;66;0;37;0
WireConnection;66;1;60;0
WireConnection;70;0;37;0
WireConnection;70;1;62;0
WireConnection;73;0;37;0
WireConnection;73;1;63;0
WireConnection;38;0;68;0
WireConnection;38;1;66;0
WireConnection;38;2;67;0
WireConnection;38;3;69;0
WireConnection;71;0;37;0
WireConnection;71;1;65;0
WireConnection;7;0;5;0
WireConnection;72;0;37;0
WireConnection;72;1;64;0
WireConnection;8;0;7;0
WireConnection;8;1;6;0
WireConnection;35;0;38;0
WireConnection;35;1;70;0
WireConnection;35;2;71;0
WireConnection;35;3;72;0
WireConnection;35;4;73;0
WireConnection;81;3;82;0
WireConnection;10;0;8;0
WireConnection;78;0;35;0
WireConnection;78;1;34;0
WireConnection;78;2;81;0
WireConnection;15;0;14;0
WireConnection;15;4;16;0
WireConnection;95;0;89;0
WireConnection;95;1;90;0
WireConnection;79;0;78;0
WireConnection;96;0;95;0
WireConnection;11;0;10;0
WireConnection;11;1;9;0
WireConnection;11;2;15;0
WireConnection;83;1;84;0
WireConnection;83;0;79;0
WireConnection;12;1;11;0
WireConnection;91;0;96;0
WireConnection;91;1;96;1
WireConnection;80;0;12;1
WireConnection;80;1;83;0
WireConnection;92;0;91;0
WireConnection;92;1;96;2
WireConnection;100;0;80;0
WireConnection;100;1;99;0
WireConnection;93;0;92;0
WireConnection;93;1;96;3
WireConnection;31;0;12;1
WireConnection;31;1;29;0
WireConnection;23;0;100;0
WireConnection;86;0;83;0
WireConnection;86;1;85;0
WireConnection;32;0;31;0
WireConnection;32;1;30;0
WireConnection;97;0;93;0
WireConnection;88;0;17;0
WireConnection;88;1;97;0
WireConnection;87;0;86;0
WireConnection;33;0;32;0
WireConnection;24;1;23;0
WireConnection;25;0;24;0
WireConnection;25;1;26;0
WireConnection;25;2;27;0
WireConnection;25;3;33;0
WireConnection;25;4;87;0
WireConnection;19;0;88;0
WireConnection;22;1;80;0
WireConnection;22;0;25;0
WireConnection;98;1;88;0
WireConnection;98;0;19;0
WireConnection;0;2;22;0
WireConnection;0;10;98;0
ASEEND*/
//CHKSM=33D643776CD1119F836E3AF83D2F7CC100A5F0BA