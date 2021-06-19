Shader "Custom/WeaponModelShader"
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
        Tags {
             "RenderType"="Opaque" 
             }
        LOD 200
        

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        //#pragma surface surf Standard fullforwardshadows
        //#pragma surface surf NoLighting fullforwardshadows
        #pragma surface surf NoLighting noambient
        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0
        
        float4 LightingNoLighting(SurfaceOutput s, float3 lightDir, float atten) 
        {
             return float4(s.Albedo, s.Alpha);
        }
        
        // float4 LightingNoLighting(SurfaceOutput s, float3 lightDir, float atten)
        // {
        //     float4 c;
        //     c.rgb = s.Albedo; 
        //     c.a = s.Alpha;
            
        //     //c.rgb = lightDir;
        //     return c;
        // }
        
        

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 viewDir;
            float3 worldNormal;
        };

        half _Glossiness;
        half _Metallic;
        float4 _Color;
        float4 _RimColor;
        float _RimPower;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutput o)
        {
            // Albedo comes from a texture tinted by color
            float4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            // o.Metallic = _Metallic;
            // o.Smoothness = _Glossiness;
            o.Alpha = c.a;
            
            float rim = 1 - saturate(dot(normalize(IN.viewDir), IN.worldNormal));
            
            o.Emission = _RimColor.rgb * pow (rim, _RimPower);
            
        }
        
        
        
        ENDCG
    }
    FallBack "Diffuse"
}
