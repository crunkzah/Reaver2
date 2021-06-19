Shader "Unlit/Textured_withAlpha"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _AlphaCutout ("Alpha cutout", float) = 1 
        
        _AlphaMult ("Alpha mult", float) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }

        Pass
        {
            Cull Back
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha 
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            float _AlphaCutout;
            float _AlphaMult;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float4 col = tex2D(_MainTex, i.uv);
                
                if(col.a >= _AlphaCutout)
                {
                    discard;
                }
                // else
                // {
                //     col.a = _AlphaMult;
                // }
                
                //col 
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
