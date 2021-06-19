Shader "Unlit/ParticleCustom1"
{
    Properties
    {
        _EmissionMult ("Emission multiplier", float) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Fade" }
        //Tags { "Queue"="Geometry" "RenderType"="Fade" }
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        //Blend SrcColor SrcAlpha
        //Blend OneMinusDstColor One
        
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

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = i.color * _EmissionMult;
                
                return col;
            }
            ENDCG
        }
    }
}
