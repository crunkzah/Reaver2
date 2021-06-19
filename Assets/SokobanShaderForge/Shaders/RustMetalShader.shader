Shader "Custom/RustMetalShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _RustMap("BumpMap", 2D) = "white" {}
        _RustColor("Rust color", Color) = (1, 0.5, 0, 1)
        //_RustThreshold("Rust threshold", Range(0,1)) = 0
        _BumpMap("BumpMap", 2D) = "bump" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        BumpThreshold("Bump threshold", Range(0,1)) = 0
        BumpStrength("Bumo strength", Range(0,1)) = 1
        EmissionStr("Emission Strength", Range(0, 25)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _BumpMap;
        sampler2D _RustMap;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_BumpMap;
            float2 uv_RustMap;
        };

        half _Glossiness;
        half _Metallic;
        float BumpThreshold;
        float BumpStrength;
        float EmissionStr;
        float _RustThreshold;
        float4 _RustColor;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)
        
        float when_gt(float x, float y) 
        {
             return max(sign(x - y), 0.0);
        }
        
        float when_lt(float x, float y) 
        {
             return max(sign(y - x), 0.0);
        }
        
        float when_le(float x, float y) 
        {
            return 1.0 - when_gt(x, y);
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            float4 rust = tex2D(_RustMap, IN.uv_RustMap + _Time.x * 0.2);
            _RustThreshold = 0.5 + sin(3.14 * _Time.y) * 0.5;
            
            
            
            c = when_lt(rust.r, _RustThreshold) * c  + when_gt(rust.r, _RustThreshold) * rust * _RustColor * EmissionStr;
            
            // if(rust.r >= _RustThreshold)
            // {
            //     c = rust * _RustColor * EmissionStr;
            // }
                
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
            
            half4 bumpTexel = tex2D(_BumpMap, IN.uv_BumpMap);
            
            o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap) * BumpStrength);
        }
        ENDCG
    }
    FallBack "Diffuse"
}
