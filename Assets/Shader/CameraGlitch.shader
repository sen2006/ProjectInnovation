Shader "Custom/CameraGlitch"
{
    Properties
    {
        _GlitchStrength ("Glitch Strength", Range(0, 1)) = 0.5
        _MainTex ("Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Overlay" }
        Pass
        {
            Name "FullscreenGlitch"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

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
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float _GlitchStrength;

            v2f vert(appdata v)
            {
                v2f o;
                o.position = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                float2 glitchUV = i.uv;

                // Wavy distortion effect
                glitchUV.x += sin(_Time.y * 10.0 + i.uv.y * 20.0) * _GlitchStrength * 0.2;
                glitchUV.y += cos(_Time.y * 5.0 + i.uv.x * 15.0) * _GlitchStrength * 0.2;

                half4 color = tex2D(_MainTex, glitchUV);
                return color;
            }
            ENDHLSL
        }
    }
}
