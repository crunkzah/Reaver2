Shader "Unlit/UnlitColorHDR_audioBands"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Emission ("Emission", float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };
            
            float4 _Color;
            float _Emission;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 col = _Color * _Emission;
                
                return col;
            }
            ENDCG
        }
    }
}
