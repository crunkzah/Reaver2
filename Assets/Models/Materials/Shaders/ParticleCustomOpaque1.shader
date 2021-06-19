Shader "Unlit/ParticleCustomOpaque1"
{
    Properties
    {
        _EmissionMult ("Emission multiplier", float) = 1
    }
    SubShader
    {
        
        Tags { "Queue"="Geometry" "RenderType"="Opaque" }
        ZWrite On
        //Blend SrcAlpha OneMinusSrcAlpha
        
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
