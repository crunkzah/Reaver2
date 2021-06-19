// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SineVFX/PrototypeGridSprite"
{
	Properties
	{
		_SpriteTexture("Sprite Texture", 2D) = "black" {}
		_TexturePower("Texture Power", Range( 0 , 1)) = 1
		_TextureColorTint("Texture Color Tint", Color) = (1,1,1,1)
		_Metallic("Metallic", Range( 0 , 1)) = 0
		_Smoothness("Smoothness", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IgnoreProjector" = "True" }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float4 _TextureColorTint;
		uniform sampler2D _SpriteTexture;
		uniform float4 _SpriteTexture_ST;
		uniform float _TexturePower;
		uniform float _Metallic;
		uniform float _Smoothness;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_SpriteTexture = i.uv_texcoord * _SpriteTexture_ST.xy + _SpriteTexture_ST.zw;
			float4 clampResult71 = clamp( ( _TextureColorTint + ( tex2D( _SpriteTexture, uv_SpriteTexture ).r * _TexturePower ) ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
			o.Albedo = clampResult71.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Smoothness;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=16902
7;29;1906;1004;2539.177;459.337;1.147281;True;False
Node;AmplifyShaderEditor.TexturePropertyNode;4;-1963.626,48.18908;Float;True;Property;_SpriteTexture;Sprite Texture;0;0;Create;True;0;0;False;0;None;None;False;black;Auto;Texture2D;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.SamplerNode;74;-1651.49,46.42749;Float;True;Property;_TextureSample0;Texture Sample 0;13;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;73;-1621.58,248.6694;Float;False;Property;_TexturePower;Texture Power;1;0;Create;True;0;0;False;0;1;0.055;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;72;-1301.78,130.1694;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;9;-1369.683,-124.5587;Float;False;Property;_TextureColorTint;Texture Color Tint;2;0;Create;True;0;0;False;0;1,1,1,1;1,0,0,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;70;-1052.079,-37.83064;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.DistanceOpNode;61;-1681.048,1386.903;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;60;-1967.031,1302.749;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.NegateNode;63;-1523.406,1387.293;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenParams;27;-2746.766,-1249.311;Float;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;17;-1174.552,-831.5797;Float;False;Property;_GradientExp;Gradient Exp;5;0;Create;True;0;0;False;0;0;0.75;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;7;-258.587,836.2813;Float;False;Property;_Metallic;Metallic;3;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GrabScreenPosition;11;-2978.834,-1052.397;Float;False;0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;37;-1296.478,-1055.813;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;71;-901.9938,-39.58424;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,1;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;39;-896.9641,-1224.799;Float;False;Property;_GradientColorTwo;Gradient Color Two;7;0;Create;True;0;0;False;0;1,1,1,1;0.2352941,0.2352941,0.2352941,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;16;-914.5522,-1056.58;Float;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;67;-976.2481,1391.399;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;36;-1103.525,-1056.518;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;59;-1948.709,1465.782;Float;False;Constant;_Vector1;Vector 1;11;0;Create;True;0;0;False;0;0,0,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;66;-937.671,1608.925;Float;False;Property;_OpacityMaskPower;Opacity Mask Power;10;0;Create;True;0;0;False;0;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;34;-1474.475,-1057.861;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;29;-2568.665,-1054.21;Float;False;FLOAT4;1;0;FLOAT4;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.RangedFloatNode;35;-1780.602,-794.6179;Float;False;Property;_GradientLength;Gradient Length;6;0;Create;True;0;0;False;0;1;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;68;-645.1809,1397.85;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;28;-1960.858,-1058.91;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;30;-2240.301,-1128.303;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;75;-1738.253,-200.6931;Float;True;Property;_MainTex;MainTex;11;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;40;-470.9641,-1206.509;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;62;-1596.774,1513.802;Float;False;Property;_OpacityMaskDistance;Opacity Mask Distance;9;0;Create;True;0;0;False;0;25;1000;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;64;-1368.627,1386.708;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;33;-1636.97,-1059.95;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;65;-1216.836,1387.07;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;8;-258.2775,911.2813;Float;False;Property;_Smoothness;Smoothness;4;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;38;-906.3201,-1401.013;Float;False;Property;_GradientColorOne;Gradient Color One;8;0;Create;True;0;0;False;0;0,0,0,1;0,0,0,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LengthOpNode;14;-1813.845,-1059.663;Float;True;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;13;-2751.52,-1052.854;Float;False;5;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT4;1,1,1,1;False;3;FLOAT4;-1,-1,-1,-1;False;4;FLOAT4;1,1,1,1;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;31;-2551.304,-1229.303;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;147,-60;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;SineVFX/PrototypeGridSprite;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;74;0;4;0
WireConnection;72;0;74;1
WireConnection;72;1;73;0
WireConnection;70;0;9;0
WireConnection;70;1;72;0
WireConnection;61;0;60;0
WireConnection;61;1;59;0
WireConnection;63;0;61;0
WireConnection;37;0;34;0
WireConnection;37;2;35;0
WireConnection;71;0;70;0
WireConnection;16;0;36;0
WireConnection;16;1;17;0
WireConnection;67;0;65;0
WireConnection;36;0;37;0
WireConnection;34;0;33;0
WireConnection;34;1;35;0
WireConnection;29;0;13;0
WireConnection;68;0;67;0
WireConnection;68;1;66;0
WireConnection;28;0;30;0
WireConnection;28;1;29;1
WireConnection;30;0;31;0
WireConnection;30;1;29;0
WireConnection;40;0;38;0
WireConnection;40;1;39;0
WireConnection;40;2;16;0
WireConnection;64;0;63;0
WireConnection;64;1;62;0
WireConnection;33;0;14;0
WireConnection;65;0;64;0
WireConnection;65;2;62;0
WireConnection;14;0;28;0
WireConnection;13;0;11;0
WireConnection;31;0;27;1
WireConnection;31;1;27;2
WireConnection;0;0;71;0
WireConnection;0;3;7;0
WireConnection;0;4;8;0
ASEEND*/
//CHKSM=2FACA2D0703E1AE6D7910823ED6F866D07F92E1F