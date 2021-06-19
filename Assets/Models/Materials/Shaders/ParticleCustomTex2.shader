Shader "Unlit/ParticleCustomTex2"
{
    Properties
    {
        _MainTex_ST ("Texture", 2D) = "white" {}
        _EmissionMult ("Emission multiplier", float) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Fade" }
        //Tags { "Queue"="Geometry" "RenderType"="Fade" }
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        
        Cull Back
        
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            //#pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                //UNITY_FOG_COORDS(1)
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            
            
            float _EmissionMult;
            sampler2D _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                //fixed4 col = i.color * _EmissionMult;
                fixed4 col = tex2D(_MainTex_ST, i.uv);
                col = col * i.color * _EmissionMult;
                
                return col;
            }
            ENDCG
        }
    }
}
