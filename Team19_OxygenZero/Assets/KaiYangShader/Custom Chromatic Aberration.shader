Shader "Custom Post-Processing/ChromaticAberration"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Intensity ("Intensity", Range(0, 1)) = 0.1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float _Intensity;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                
                // Offset for RGB channels
                float2 offset = _Intensity * (uv - 0.5);

                float4 color;
                color.r = tex2D(_MainTex, uv + offset).r;
                color.g = tex2D(_MainTex, uv).g;
                color.b = tex2D(_MainTex, uv - offset).b;
                color.a = 1.0;

                return color;
            }
            ENDHLSL
        }
    }
}


