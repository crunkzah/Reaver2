Shader "Custom/RimLightingNonLit"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        //_MainTex ("Albedo (RGB)", 2D) = "white" {}
        [HDR] _RimColor ("Rim Color", Color) = (0.26,0.19,0.16,0.0)
        _RimPower ("Rim Power", Range(0.5,16.0)) = 3.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        

        
        //#pragma surface surf Standard fullforwardshadows
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                //UNITY_FOG_COORDS(1)
                float4 vertex : POSITION;
                fixed4 color : COLOR;
            };

            float4 _MainTex_ST;
            float _EmissionMult;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 col = i.color * _EmissionMult;
                
                return col;
            }
            ENDCG
        }
    }
}
