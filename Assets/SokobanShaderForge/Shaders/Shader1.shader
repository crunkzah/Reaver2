// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Shader1"
{
    Properties
    {
        Tint ("Tint", Color) = (1, 1, 1, 1)
    }
    
    Subshader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag   
            
            #include "UnityCG.cginc"
            
            float4 Tint;
            
            struct Interpolators
            {
                float4 position : SV_POSITION;
                float3 localPosition : TEXCOORD0;
            };
            
            Interpolators vert(float4 position : POSITION)
            {
                Interpolators i;
                
                i.localPosition = position.xyz;
                i.position = UnityObjectToClipPos(position);
                
                return i;
            }
            
            float4 frag(Interpolators i) : SV_TARGET
            {
                return float4(i.localPosition, 1);
            }
            
            ENDCG
        }
    }
}
