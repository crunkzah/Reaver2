// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/Triplanar_Rim"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Tiling ("Tiling", Float) = 1.0
        _OcclusionMap("Occlusion", 2D) = "white" {}
        _RimColor( "Rim Color", Color ) = ( 1.0, 1.0, 1.0, 1.0 )
		_RimPower( "Rim Power", Range( 0.1, 10.0 )) = 3.0
    }
    SubShader
    {
        Pass
        {
            // Tags
			// { 
			// 	"LightMode" = "ForwardBase" // allows shadow rec/cast, lighting
			// }
            //ZTest Less
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            

            struct v2f
            {
                half3 objNormal : TEXCOORD0;
                float3 coords : TEXCOORD1;
                float2 uv : TEXCOORD2;
                float3 viewDir : TEXCOORD3;
                float4 pos : SV_POSITION;
            };

            float _Tiling;

            v2f vert (float4 pos : POSITION, float3 normal : NORMAL, float2 uv : TEXCOORD0, float4 viewDir : TEXCOORD3)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(pos);
                o.coords = pos.xyz * _Tiling;
                o.objNormal = normal;
                o.uv = uv;
                o.viewDir = normalize(WorldSpaceViewDir(viewDir));
                
                return o;
            }

            sampler2D _MainTex;
            sampler2D _OcclusionMap;
            float4 _RimColor;
			float _RimPower;
            
            float4 frag (v2f i) : SV_Target
            {
                // use absolute value of normal as texture weights
                half3 blend = abs(i.objNormal);
                // make sure the weights sum up to 1 (divide by sum of x+y+z)
                blend /= dot(blend,1.0);
                // read the three texture projections, for x,y,z axes
                float4 cx = tex2D(_MainTex, i.coords.yz);
                float4 cy = tex2D(_MainTex, i.coords.xz);
                float4 cz = tex2D(_MainTex, i.coords.xy);
                // blend the textures based on weights
                float4 c = cx * blend.x + cy * blend.y + cz * blend.z;
                
                c *= tex2D(_OcclusionMap, i.uv);
                
                
                //float rim = 1 - saturate(dot(normalize(i.viewDir), i.objNormal));
                //c.xyz *= 2 * _RimColor.rgb * pow(rim, _RimPower);
                
                return c;
            }
            ENDCG
        }
    }
}