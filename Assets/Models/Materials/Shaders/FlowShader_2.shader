Shader "Unlit/FlowShader_2"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [HDR]_Color("Foam Color", Color) = (1,1,1,1)
        Scale("Scroll speed", float) = 0.075
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite On
            
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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float Scale;
            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                // o.uv += _Time.x;
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                
                float2 uv1 = i.uv;
                
                uv1.x += _Time.x;
                uv1.y += _Time.x;
                
                float2 uv2 = i.uv;
                
                uv2.x -= _Time.x;
                uv2.y -= _Time.x;
                
                
                float4 col, noise1, noise2;
                noise1 = tex2D(_MainTex, uv1 * Scale);
                
                noise2 = tex2D(_MainTex, uv2 * Scale);
                
                float2 uv_main = noise1 + noise2;
                
                // col = tex2D(_MainTex, i.uv) + noise1 + noise2;
                col = tex2D(_MainTex, uv_main) * _Color;// + noise1 + noise2;
                
                return col;
            }
            ENDCG
        }
    }
}
