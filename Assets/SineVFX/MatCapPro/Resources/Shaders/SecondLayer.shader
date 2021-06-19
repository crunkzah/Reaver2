// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SineVFX/MatCapPro/SecondLayer"
{
	Properties
	{
		_FinalPower("Final Power", Float) = 1
		_FinalOpacity("Final Opacity", Range( 0 , 1)) = 1
		[Toggle(_FINALOPACITYDITHERENABLED_ON)] _FinalOpacityDitherEnabled("Final Opacity Dither Enabled", Float) = 0
		[Toggle(_EDGESONLYMODEENABLED_ON)] _EdgesOnlyModeEnabled("Edges Only Mode Enabled", Float) = 0
		_Ramp("Ramp", 2D) = "white" {}
		[HDR] _RampColorTint("Ramp Color Tint", Color) = (1,1,1,1)
		_RampTilingExp("Ramp Tiling Exp", Range( 0.2 , 4)) = 1
		_EdgeDetectionThickness("Edge Detection Thickness", Float) = 1
		_MaskGlowExp("Mask Glow Exp", Range( 0.2 , 8)) = 1
		_MaskGlowAmount("Mask Glow Amount", Range( 0 , 10)) = 0
		_EdgeDetectionExp("Edge Detection Exp", Range( 1 , 4)) = 1
		_EdgeGlowAmount("Edge Glow Amount", Range( 0 , 10)) = 0
		_EdgeMaskPower("Edge Mask Power", Range( -100 , 100)) = 20
		_EdgeFresnelExp("Edge Fresnel Exp", Range( 0.2 , 10)) = 2
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
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Off
		GrabPass{ }
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#pragma shader_feature _WIREFRAMEENABLED_ON
		#pragma shader_feature _EDGESONLYMODEENABLED_ON
		#pragma shader_feature _FINALOPACITYDITHERENABLED_ON
		#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
		#define ASE_DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex);
		#else
		#define ASE_DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex)
		#endif
		struct Input
		{
			float4 screenPosition;
			float3 worldPos;
			float3 worldNormal;
			float2 uv_texcoord;
		};

		uniform float _EdgeDetectionThickness;
		uniform sampler2D _Ramp;
		ASE_DECLARE_SCREENSPACE_TEXTURE( _GrabTexture )
		uniform float _EdgeDetectionExp;
		uniform float _EdgeMaskPower;
		uniform float _EdgeFresnelExp;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform float _WireframeThickness;
		uniform float _WireframePower;
		uniform float _WireframeFresnelExp;
		uniform float _RampTilingExp;
		uniform float _EdgeGlowAmount;
		uniform float4 _RampColorTint;
		uniform float _FinalPower;
		uniform float _MaskGlowExp;
		uniform float _MaskGlowAmount;
		uniform float _WireframeGlowAmount;
		uniform float _FinalOpacity;
		uniform sampler2D _OpacityTexture;
		uniform float4 _OpacityTexture_ST;
		uniform float4 _OpacityTextureChannel;


		float distortuv( float2 vsposition , float2 dir , float et )
		{
			float2 correctuv = (vsposition.xy + dir * et) / _ScreenParams.xy;
			float4 imagef = tex2D(_GrabTexture, correctuv.xy);
			imagef = 0.2126*imagef.r + 0.7152*imagef.g + 0.0722*imagef.b;
			return imagef;
		}


		inline float4 ASE_ComputeGrabScreenPos( float4 pos )
		{
			#if UNITY_UV_STARTS_AT_TOP
			float scale = -1.0;
			#else
			float scale = 1.0;
			#endif
			float4 o = pos;
			o.y = pos.w * 0.5f;
			o.y = ( pos.y - o.y ) * _ProjectionParams.x * scale + o.y;
			return o;
		}


		float MyCustomExpression( float2 pos )
		{
			float uu = 0.0;
			float vv = 0.0;
			uu -= distortuv(pos, float2(-1.0,-1.0), _EdgeDetectionThickness);
			uu -= 2.0 * distortuv(pos, float2(-1.0,0.0), _EdgeDetectionThickness);
			uu -= distortuv(pos, float2(-1.0,1.0), _EdgeDetectionThickness);
			uu += distortuv(pos, float2(1.0,-1.0), _EdgeDetectionThickness);
			uu += 2.0 * distortuv(pos, float2(1.0,0.0), _EdgeDetectionThickness);
			uu += distortuv(pos, float2(1.0,1.0), _EdgeDetectionThickness);
			vv -= distortuv(pos, float2(-1.0,-1.0), _EdgeDetectionThickness);
			vv -= 2.0 * distortuv(pos, float2(0.0,-1.0), _EdgeDetectionThickness);
			vv -= distortuv(pos, float2(1.0,-1.0), _EdgeDetectionThickness);
			vv += distortuv(pos, float2(-1.0,1.0), _EdgeDetectionThickness);
			vv += 2.0 * distortuv(pos, float2(0.0,1.0), _EdgeDetectionThickness);
			vv += distortuv(pos, float2(1.0,1.0), _EdgeDetectionThickness);
			float output = saturate((uu*uu + vv*vv));
			return output;
		}


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
			float4 ase_screenPos = i.screenPosition;
			float4 ase_grabScreenPos = ASE_ComputeGrabScreenPos( ase_screenPos );
			float4 ase_grabScreenPosNorm = ase_grabScreenPos / ase_grabScreenPos.w;
			float4 screenColor38 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_GrabTexture,ase_grabScreenPosNorm.xy);
			float2 pos36 = ( ase_grabScreenPosNorm * _ScreenParams ).xy;
			float localMyCustomExpression36 = MyCustomExpression( pos36 );
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = i.worldNormal;
			float fresnelNdotV50 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode50 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNdotV50, _EdgeFresnelExp ) );
			float clampResult40 = clamp( ( pow( localMyCustomExpression36 , _EdgeDetectionExp ) * _EdgeMaskPower * fresnelNode50 ) , -1.0 , 1.0 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float eyeDepth100 = (SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy )*( _ProjectionParams.z - _ProjectionParams.y ));
			float4 appendResult91 = (float4(_WireframeThickness , 0.0 , 0.0 , 0.0));
			float eyeDepth104 = (SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ( ase_screenPosNorm + appendResult91 ).xy )*( _ProjectionParams.z - _ProjectionParams.y ));
			float4 appendResult90 = (float4(-_WireframeThickness , 0.0 , 0.0 , 0.0));
			float eyeDepth107 = (SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ( appendResult90 + ase_screenPosNorm ).xy )*( _ProjectionParams.z - _ProjectionParams.y ));
			float4 appendResult84 = (float4(0.0 , _WireframeThickness , 0.0 , 0.0));
			float eyeDepth106 = (SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ( appendResult84 + ase_screenPosNorm ).xy )*( _ProjectionParams.z - _ProjectionParams.y ));
			float4 appendResult85 = (float4(0.0 , -_WireframeThickness , 0.0 , 0.0));
			float eyeDepth105 = (SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ( appendResult85 + ase_screenPosNorm ).xy )*( _ProjectionParams.z - _ProjectionParams.y ));
			float4 appendResult94 = (float4(_WireframeThickness , _WireframeThickness , 0.0 , 0.0));
			float eyeDepth114 = (SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ( appendResult94 + ase_screenPosNorm ).xy )*( _ProjectionParams.z - _ProjectionParams.y ));
			float4 appendResult96 = (float4(-_WireframeThickness , -_WireframeThickness , 0.0 , 0.0));
			float eyeDepth112 = (SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ( ase_screenPosNorm + appendResult96 ).xy )*( _ProjectionParams.z - _ProjectionParams.y ));
			float4 appendResult95 = (float4(_WireframeThickness , -_WireframeThickness , 0.0 , 0.0));
			float eyeDepth115 = (SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ( ase_screenPosNorm + appendResult95 ).xy )*( _ProjectionParams.z - _ProjectionParams.y ));
			float4 appendResult93 = (float4(-_WireframeThickness , _WireframeThickness , 0.0 , 0.0));
			float eyeDepth109 = (SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ( ase_screenPosNorm + appendResult93 ).xy )*( _ProjectionParams.z - _ProjectionParams.y ));
			float clampResult135 = clamp( ( ( ( eyeDepth100 - eyeDepth104 ) + ( eyeDepth100 - eyeDepth107 ) + ( eyeDepth100 - eyeDepth106 ) + ( eyeDepth100 - eyeDepth105 ) ) + ( eyeDepth100 - eyeDepth114 ) + ( eyeDepth100 - eyeDepth112 ) + ( eyeDepth100 - eyeDepth115 ) + ( eyeDepth100 - eyeDepth109 ) ) , 0.0 , 1.0 );
			float fresnelNdotV125 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode125 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNdotV125, _WireframeFresnelExp ) );
			float clampResult127 = clamp( ( clampResult135 * _WireframePower * fresnelNode125 ) , -1.0 , 1.0 );
			#ifdef _WIREFRAMEENABLED_ON
				float staticSwitch128 = clampResult127;
			#else
				float staticSwitch128 = 0.0;
			#endif
			float clampResult42 = clamp( ( screenColor38.r + clampResult40 + staticSwitch128 ) , 0.0 , 1.0 );
			float2 appendResult49 = (float2(pow( clampResult42 , _RampTilingExp ) , 0.0));
			float clampResult152 = clamp( ( ( clampResult40 * _EdgeGlowAmount ) + 1.0 ) , 0.0 , 100.0 );
			float clampResult153 = clamp( ( ( staticSwitch128 * _WireframeGlowAmount ) + 1.0 ) , 0.0 , 100.0 );
			o.Emission = ( tex2D( _Ramp, appendResult49 ) * clampResult152 * _RampColorTint * _FinalPower * ( ( pow( screenColor38.r , _MaskGlowExp ) * _MaskGlowAmount ) + 1.0 ) * clampResult153 ).rgb;
			#ifdef _EDGESONLYMODEENABLED_ON
				float staticSwitch149 = clampResult40;
			#else
				float staticSwitch149 = 1.0;
			#endif
			float2 uv_OpacityTexture = i.uv_texcoord * _OpacityTexture_ST.xy + _OpacityTexture_ST.zw;
			float4 break139 = ( tex2D( _OpacityTexture, uv_OpacityTexture ) * _OpacityTextureChannel );
			float clampResult143 = clamp( max( max( max( break139.r , break139.g ) , break139.b ) , break139.a ) , 0.0 , 1.0 );
			float clampResult147 = clamp( ( _FinalOpacity * clampResult143 ) , 0.0 , 1.0 );
			float2 clipScreen55 = ase_screenPosNorm.xy * _ScreenParams.xy;
			float dither55 = Dither4x4Bayer( fmod(clipScreen55.x, 4), fmod(clipScreen55.y, 4) );
			float clampResult148 = clamp( ( _FinalOpacity * clampResult143 ) , 0.0 , 1.0 );
			dither55 = step( dither55, clampResult148 );
			#ifdef _FINALOPACITYDITHERENABLED_ON
				float staticSwitch57 = dither55;
			#else
				float staticSwitch57 = clampResult147;
			#endif
			o.Alpha = ( staticSwitch149 * staticSwitch57 );
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Unlit alpha:fade keepalpha fullforwardshadows vertex:vertexDataFunc 

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
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float4 customPack1 : TEXCOORD1;
				float2 customPack2 : TEXCOORD2;
				float3 worldPos : TEXCOORD3;
				float3 worldNormal : TEXCOORD4;
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
				o.worldNormal = worldNormal;
				o.customPack1.xyzw = customInputData.screenPosition;
				o.customPack2.xy = customInputData.uv_texcoord;
				o.customPack2.xy = v.texcoord;
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
				surfIN.screenPosition = IN.customPack1.xyzw;
				surfIN.uv_texcoord = IN.customPack2.xy;
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = IN.worldNormal;
				SurfaceOutput o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutput, o )
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
Version=17001
1927;29;1906;1004;3175.11;985.5544;1;True;False
Node;AmplifyShaderEditor.RangedFloatNode;81;-4215.011,-2358.871;Float;False;Property;_WireframeThickness;Wireframe Thickness;17;0;Create;True;0;0;False;0;10.92;0.001;0;0.01;0;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;83;-3820.014,-2585.982;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;82;-3814.55,-2331.366;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;84;-3649.542,-2449.386;Float;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.DynamicAppendNode;85;-3641.892,-2302.954;Float;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.NegateNode;86;-3814.184,-1683.445;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;87;-3820.918,-1797.935;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;88;-3802.061,-2022.872;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenPosInputsNode;89;-3732.677,-2912.831;Float;False;0;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;90;-3653.913,-2589.261;Float;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.DynamicAppendNode;91;-3660.406,-2725.556;Float;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.DynamicAppendNode;96;-3648.511,-1998.626;Float;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.DynamicAppendNode;95;-3647.164,-1850.464;Float;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.DynamicAppendNode;94;-3648.511,-2152.177;Float;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.DynamicAppendNode;93;-3651.204,-1703.648;Float;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleAddOpNode;92;-3382.313,-2819.28;Float;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleAddOpNode;97;-3371.318,-2443.668;Float;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleAddOpNode;98;-3376.349,-2702.909;Float;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleAddOpNode;99;-3370.885,-2580.519;Float;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleAddOpNode;101;-3368.349,-2254.543;Float;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleAddOpNode;102;-3368.349,-2146.788;Float;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleAddOpNode;103;-3364.308,-1924.545;Float;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ScreenDepthNode;100;-3207.645,-2955.698;Float;False;0;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenDepthNode;104;-3207.198,-2854.89;Float;False;0;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenDepthNode;105;-3199.321,-2548.828;Float;False;0;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenDepthNode;106;-3208.063,-2654.827;Float;False;0;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenDepthNode;107;-3209.155,-2758.64;Float;False;0;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;108;-3362.961,-2034.993;Float;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ScreenDepthNode;109;-3193.248,-1977.075;Float;False;0;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenDepthNode;115;-3191.901,-2061.932;Float;False;0;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;110;-2929.762,-2910.089;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;111;-2926.128,-2610.023;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenDepthNode;112;-3190.554,-2150.83;Float;False;0;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;113;-2932.099,-2807.052;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;116;-2926.128,-2708.373;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenDepthNode;114;-3189.207,-2241.074;Float;False;0;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GrabScreenPosition;48;-2790.767,-687.4645;Float;False;0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleSubtractOpNode;117;-2929.249,-2022.871;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;119;-2927.902,-2121.197;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenParams;39;-2677.915,-487.3761;Float;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector4Node;136;-2146.11,1177.792;Float;False;Property;_OpacityTextureChannel;Opacity Texture Channel;21;0;Create;True;0;0;False;0;0,0,0,1;0,0,0,1;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleSubtractOpNode;120;-2926.555,-2311.115;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;137;-2143.109,974.7921;Float;True;Property;_OpacityTexture;Opacity Texture;20;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleSubtractOpNode;118;-2931.943,-2214.136;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;121;-2294.084,-2539.26;Float;False;4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;138;-1823.11,1085.792;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;126;-2785.417,-1553.609;Float;False;Property;_WireframeFresnelExp;Wireframe Fresnel Exp;18;0;Create;True;0;0;False;0;1;0.2;0.2;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;32;-2419.947,-567.0855;Float;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleAddOpNode;122;-2159.015,-2253.208;Float;False;5;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;124;-2322.874,-1886.186;Float;False;Property;_WireframePower;Wireframe Power;16;0;Create;True;0;0;False;0;10;100;-100;100;0;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;139;-1695.11,1085.792;Float;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.CustomExpressionNode;36;-2262.271,-562.9215;Float;False;float uu = 0.0@$float vv = 0.0@$$uu -= distortuv(pos, float2(-1.0,-1.0), _EdgeDetectionThickness)@$uu -= 2.0 * distortuv(pos, float2(-1.0,0.0), _EdgeDetectionThickness)@$uu -= distortuv(pos, float2(-1.0,1.0), _EdgeDetectionThickness)@$uu += distortuv(pos, float2(1.0,-1.0), _EdgeDetectionThickness)@$uu += 2.0 * distortuv(pos, float2(1.0,0.0), _EdgeDetectionThickness)@$uu += distortuv(pos, float2(1.0,1.0), _EdgeDetectionThickness)@$vv -= distortuv(pos, float2(-1.0,-1.0), _EdgeDetectionThickness)@$vv -= 2.0 * distortuv(pos, float2(0.0,-1.0), _EdgeDetectionThickness)@$vv -= distortuv(pos, float2(1.0,-1.0), _EdgeDetectionThickness)@$vv += distortuv(pos, float2(-1.0,1.0), _EdgeDetectionThickness)@$vv += 2.0 * distortuv(pos, float2(0.0,1.0), _EdgeDetectionThickness)@$vv += distortuv(pos, float2(1.0,1.0), _EdgeDetectionThickness)@$$float output = saturate((uu*uu + vv*vv))@$$return output@;1;False;1;True;pos;FLOAT2;0,0;In;;Float;False;My Custom Expression;False;False;0;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;51;-2322.108,-891.4071;Float;False;Property;_EdgeDetectionExp;Edge Detection Exp;11;0;Create;True;0;0;False;0;1;2;1;4;0;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;125;-2467.741,-1667.678;Float;True;Standard;WorldNormal;ViewDir;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;135;-1981.966,-2205.378;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;44;-2297.22,106.4527;Float;False;Property;_EdgeFresnelExp;Edge Fresnel Exp;14;0;Create;True;0;0;False;0;2;4;0.2;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;140;-1421.11,1021.792;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;37;-2050.921,-402.1373;Float;False;Property;_EdgeMaskPower;Edge Mask Power;13;0;Create;True;0;0;False;0;20;0.5;-100;100;0;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;50;-1992.674,-43.23819;Float;True;Standard;WorldNormal;ViewDir;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;41;-1972.615,-754.5521;Float;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;123;-2029.224,-2003.582;Float;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;141;-1288.11,1106.792;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;43;-1686.77,-453.0898;Float;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;129;-1787.845,-1544.86;Float;False;Constant;_Float3;Float 3;16;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;127;-1846.488,-1988.884;Float;False;3;0;FLOAT;0;False;1;FLOAT;-1;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GrabScreenPosition;47;-1917.793,-1078.972;Float;False;0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ScreenColorNode;38;-1678.181,-1082.255;Float;False;Global;_GrabScreen1;Grab Screen 1;8;0;Create;True;0;0;False;0;Object;-1;False;False;1;0;FLOAT2;0,0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMaxOpNode;142;-1143.11,1185.792;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;40;-1542.095,-454.8669;Float;False;3;0;FLOAT;0;False;1;FLOAT;-1;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;128;-1596.053,-1639.777;Float;False;Property;_WireframeEnabled;Wireframe Enabled;15;0;Create;True;0;0;False;0;0;0;1;True;;Toggle;2;Key0;Key1;Create;False;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;17;-1040.044,860.6466;Float;False;Property;_FinalOpacity;Final Opacity;1;0;Create;True;0;0;False;0;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;30;-1146.9,-742.9018;Float;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;143;-998.2712,1186.987;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;156;-1124.3,-869.5369;Float;False;Property;_RampTilingExp;Ramp Tiling Exp;7;0;Create;True;0;0;False;0;1;2.33;0.2;4;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;80;-1470.869,44.36096;Float;False;Property;_MaskGlowExp;Mask Glow Exp;9;0;Create;True;0;0;False;0;1;1;0.2;8;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;42;-1013.708,-740.199;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;29;-1460.285,-88.53113;Float;False;Property;_EdgeGlowAmount;Edge Glow Amount;12;0;Create;True;0;0;False;0;0;0;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;145;-664.4892,901.5757;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;132;-1356.271,449.4044;Float;False;Property;_WireframeGlowAmount;Wireframe Glow Amount;19;0;Create;True;0;0;False;0;0;1;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;78;-1366.869,163.9609;Float;False;Property;_MaskGlowAmount;Mask Glow Amount;10;0;Create;True;0;0;False;0;0;0;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;46;-1127.881,-188.1254;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;144;-627.2604,1051.699;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;131;-1075.427,363.1304;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;77;-1160.17,22.26097;Float;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;155;-765.8466,-740.1615;Float;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;148;-515.4609,903.5791;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;33;-937.7499,-242.4481;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;49;-996.2366,-474.4093;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;150;-326.3675,494.301;Float;False;Constant;_Float0;Float 0;21;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DitheringNode;55;-416.3497,713.9327;Float;False;0;False;3;0;FLOAT;0;False;1;SAMPLER2D;;False;2;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;147;-432.4609,1060.579;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;76;-1034.068,126.261;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;130;-880.1565,399.8644;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;57;-176.8245,815.9179;Float;False;Property;_FinalOpacityDitherEnabled;Final Opacity Dither Enabled;3;0;Create;True;0;0;False;0;0;0;1;True;;Toggle;2;Key0;Key1;Create;False;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;79;-883.2693,-23.23907;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;152;-735.0938,-219.4558;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;100;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;153;-719.3429,466.9627;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;100;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;27;-729.8708,365.6248;Float;False;Property;_FinalPower;Final Power;0;0;Create;True;0;0;False;0;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;26;-771.1932,170.1586;Float;False;Property;_RampColorTint;Ramp Color Tint;6;0;Create;True;0;0;False;0;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;24;-838.4108,-496.1455;Float;True;Property;_Ramp;Ramp;5;0;Create;True;0;0;False;0;None;7d356691fc2a479d9c6fc3646ed80697;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StaticSwitch;149;-132.8024,507.1978;Float;False;Property;_EdgesOnlyModeEnabled;Edges Only Mode Enabled;4;0;Create;True;0;0;False;0;0;0;0;True;;Toggle;2;Key0;Key1;Create;False;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;45;-373.6285,-331.3239;Float;False;6;6;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.CustomExpressionNode;133;-2265.418,-711.5486;Float;False;float2 correctuv = (vsposition.xy + dir * et) / _ScreenParams.xy@$float4 imagef = tex2D(_GrabTexture, correctuv.xy)@$imagef = 0.2126*imagef.r + 0.7152*imagef.g + 0.0722*imagef.b@$return imagef@;1;False;3;True;vsposition;FLOAT2;0,0;In;;Float;False;True;dir;FLOAT2;0,0;In;;Float;False;True;et;FLOAT;0;In;;Float;False;distortuv;False;True;0;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;134;-2340.986,-805.1993;Float;False;Property;_EdgeDetectionThickness;Edge Detection Thickness;8;0;Create;True;0;0;True;0;1;2.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;151;212.0377,653.7679;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;146;-1045.542,758.2673;Float;False;Property;_FinalOpacityMaskPower;Final Opacity Mask Power;2;0;Create;True;0;0;False;0;0;1;1;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;345.3564,4.573638;Float;False;True;2;Float;ASEMaterialInspector;0;0;Unlit;SineVFX/MatCapPro/SecondLayer;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;True;0;False;Transparent;;Transparent;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;83;0;81;0
WireConnection;82;0;81;0
WireConnection;84;1;81;0
WireConnection;85;1;82;0
WireConnection;86;0;81;0
WireConnection;87;0;81;0
WireConnection;88;0;81;0
WireConnection;90;0;83;0
WireConnection;91;0;81;0
WireConnection;96;0;88;0
WireConnection;96;1;88;0
WireConnection;95;0;81;0
WireConnection;95;1;87;0
WireConnection;94;0;81;0
WireConnection;94;1;81;0
WireConnection;93;0;86;0
WireConnection;93;1;81;0
WireConnection;92;0;89;0
WireConnection;92;1;91;0
WireConnection;97;0;85;0
WireConnection;97;1;89;0
WireConnection;98;0;90;0
WireConnection;98;1;89;0
WireConnection;99;0;84;0
WireConnection;99;1;89;0
WireConnection;101;0;94;0
WireConnection;101;1;89;0
WireConnection;102;0;89;0
WireConnection;102;1;96;0
WireConnection;103;0;89;0
WireConnection;103;1;93;0
WireConnection;100;0;89;0
WireConnection;104;0;92;0
WireConnection;105;0;97;0
WireConnection;106;0;99;0
WireConnection;107;0;98;0
WireConnection;108;0;89;0
WireConnection;108;1;95;0
WireConnection;109;0;103;0
WireConnection;115;0;108;0
WireConnection;110;0;100;0
WireConnection;110;1;104;0
WireConnection;111;0;100;0
WireConnection;111;1;105;0
WireConnection;112;0;102;0
WireConnection;113;0;100;0
WireConnection;113;1;107;0
WireConnection;116;0;100;0
WireConnection;116;1;106;0
WireConnection;114;0;101;0
WireConnection;117;0;100;0
WireConnection;117;1;109;0
WireConnection;119;0;100;0
WireConnection;119;1;115;0
WireConnection;120;0;100;0
WireConnection;120;1;114;0
WireConnection;118;0;100;0
WireConnection;118;1;112;0
WireConnection;121;0;110;0
WireConnection;121;1;113;0
WireConnection;121;2;116;0
WireConnection;121;3;111;0
WireConnection;138;0;137;0
WireConnection;138;1;136;0
WireConnection;32;0;48;0
WireConnection;32;1;39;0
WireConnection;122;0;121;0
WireConnection;122;1;120;0
WireConnection;122;2;118;0
WireConnection;122;3;119;0
WireConnection;122;4;117;0
WireConnection;139;0;138;0
WireConnection;36;0;32;0
WireConnection;125;3;126;0
WireConnection;135;0;122;0
WireConnection;140;0;139;0
WireConnection;140;1;139;1
WireConnection;50;3;44;0
WireConnection;41;0;36;0
WireConnection;41;1;51;0
WireConnection;123;0;135;0
WireConnection;123;1;124;0
WireConnection;123;2;125;0
WireConnection;141;0;140;0
WireConnection;141;1;139;2
WireConnection;43;0;41;0
WireConnection;43;1;37;0
WireConnection;43;2;50;0
WireConnection;127;0;123;0
WireConnection;38;0;47;0
WireConnection;142;0;141;0
WireConnection;142;1;139;3
WireConnection;40;0;43;0
WireConnection;128;1;129;0
WireConnection;128;0;127;0
WireConnection;30;0;38;1
WireConnection;30;1;40;0
WireConnection;30;2;128;0
WireConnection;143;0;142;0
WireConnection;42;0;30;0
WireConnection;145;0;17;0
WireConnection;145;1;143;0
WireConnection;46;0;40;0
WireConnection;46;1;29;0
WireConnection;144;0;17;0
WireConnection;144;1;143;0
WireConnection;131;0;128;0
WireConnection;131;1;132;0
WireConnection;77;0;38;1
WireConnection;77;1;80;0
WireConnection;155;0;42;0
WireConnection;155;1;156;0
WireConnection;148;0;145;0
WireConnection;33;0;46;0
WireConnection;49;0;155;0
WireConnection;55;0;148;0
WireConnection;147;0;144;0
WireConnection;76;0;77;0
WireConnection;76;1;78;0
WireConnection;130;0;131;0
WireConnection;57;1;147;0
WireConnection;57;0;55;0
WireConnection;79;0;76;0
WireConnection;152;0;33;0
WireConnection;153;0;130;0
WireConnection;24;1;49;0
WireConnection;149;1;150;0
WireConnection;149;0;40;0
WireConnection;45;0;24;0
WireConnection;45;1;152;0
WireConnection;45;2;26;0
WireConnection;45;3;27;0
WireConnection;45;4;79;0
WireConnection;45;5;153;0
WireConnection;151;0;149;0
WireConnection;151;1;57;0
WireConnection;0;2;45;0
WireConnection;0;9;151;0
ASEEND*/
//CHKSM=A2684BA1B9885A343CDA571E56D4B69A55649AD1