Shader "Unlit/UnlitTextured_Rim"
{
    Properties
    {
        _MainTex ("Diffuse Texture", 2D) = "white" {}
        [HDR]_Color ("Color", Color) = (1,1,1,1)
		//_SpecColor( "Specular Color", Color ) = ( 1.0, 1.0, 1.0, 1.0 )
		//_Shininess( "Shininess", float ) = 10
		_RimColor( "Rim Color", Color ) = ( 1.0, 1.0, 1.0, 1.0 )
		_RimPower( "Rim Power", Range( 0.1, 10.0 )) = 3.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        //LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            float4 _Color;
            sampler2D _MainTex;
			float4 _MainTex_ST;
			//float4 _SpecColor;
			//float _Shininess;
			float4 _RimColor;
			float _RimPower;
            
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };
            

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv) * _Color;
                //float4 col = _Color;
                
                return col;
            }
            ENDCG
        }
    }
}
