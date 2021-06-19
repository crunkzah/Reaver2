Shader "Unlit/BulletShader1"
{
    Properties
    {
        [HDR] _Color1  ("Color1", color) = (1,1,1,1)
        [HDR] _Color2  ("Color2", color) = (0.5, 0.5, 1, 1)
        _Freq ("Frequency", float) = 2
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Opaque" }
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                // float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            float4 _Color1;
            float4 _Color2;
            float _Freq;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // _SinTime
                // float sinTime = 0.5 + sin(_Freq * _Time.y)*0.5;
                
                // float4 col = lerp(_Color1, _Color2, sinTime);
                
                float4 col = _Color1;
                
                return col;
            }
            ENDCG
        }
    }
}
