Shader "Custom/WallGlitch"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GlitchIntensity ("Glitch Intensity", Range(0, 1)) = 0.5
        _GlitchSpeed ("Glitch Speed", Range(0, 5)) = 1.0
        _GlitchAmount ("Glitch Amount", Range(0, 1)) = 0.5
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
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float _GlitchIntensity;
            float _GlitchSpeed;
            float _GlitchAmount;

            v2f vert(appdata v)
            {
                v2f o;
                o.position = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;

                // Add UV distortion for glitch effect
                o.uv.x += sin(_Time.y * _GlitchSpeed + o.uv.y * 10.0) * _GlitchIntensity;
                o.uv.y += cos(_Time.y * _GlitchSpeed + o.uv.x * 10.0) * _GlitchIntensity;

                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                half4 color = tex2D(_MainTex, i.uv);

                // Add horizontal glitch lines
                float glitchLine = frac(_Time.y * _GlitchSpeed) * _GlitchAmount;
                if (glitchLine > 0.8)
                {
                    color.rgb += half3(0.3, 0.1, 0.2);
                }

                return color;
            }
            ENDHLSL
        }
    }
}
