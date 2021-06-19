Shader "Unlit/FlowShader_1"
{
    Properties
    {
        [HDR]_MainColor ("Main color", color) = (0, 0, 0, 1)
        [HDR]_Color ("Color", color) = (0.26,0.19,0.16,0.0)
        _MainTex ("Texture", 2D) = "white" {}
        _WhiteThreshold("White Threshold", float) = 0.5
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
            // make fog work

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            
            float4 _MainColor;
            float4 _Color;
            float4 _MainTex_ST;
            float _WhiteThreshold;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                
                v.uv = v.uv + float2(0, _Time.x);
                
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 col_tex = tex2D(_MainTex, i.uv);
                
                float4 col;
                
                
                
                
                if(col_tex.r > (_WhiteThreshold) )
                {
                    col = _MainColor;
                }
                else
                    if(col_tex.r > _WhiteThreshold / 2)
                    {
                        col = _MainColor * col_tex.r;
                    }
                    else
                    {
                        col = col_tex * _Color;
                        
                    }
                
                return col;
            }
            ENDCG
        }
    }
}
