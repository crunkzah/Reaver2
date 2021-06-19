Shader "Custom/RimLighting"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        [HDR] _RimColor ("Rim Color", Color) = (0.26,0.19,0.16,0.0)
        _RimPower ("Rim Power", Range(0.5,16.0)) = 3.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 viewDir;
            float3 worldNormal;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        float4 _RimColor;
        float _RimPower;
        
        
        UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            
            
            float4 c = _Color;
            o.Albedo = c.rgb;
            
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            
            o.Alpha = 1;
            float rim = 1 - saturate(dot (normalize(IN.viewDir), IN.worldNormal));
            
            o.Emission = _RimColor.rgb * pow(rim, _RimPower) * (1 + 0.5 * sin(3 * _Time.z));
            
        }
        ENDCG
    }
    FallBack "Diffuse"
}
