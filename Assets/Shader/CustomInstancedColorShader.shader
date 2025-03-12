Shader "Custom/InstancedColorShader"
{
    Properties
    {
        _BaseColor ("Color", Color) = (1,1,1,1) // Default color (not used in instancing)
        _Metallic ("Metallic", Range(0,1)) = 0 // Metallic value
        _MainTex ("Texture", 2D) = "white" {} // Texture property
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID // Ensure instancing is enabled
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
                UNITY_VERTEX_INPUT_INSTANCE_ID // Pass instance ID to fragment
            };

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor) // Instanced color
                UNITY_DEFINE_INSTANCED_PROP(float, _Metallic)   // Instanced metallic
            UNITY_INSTANCING_BUFFER_END(Props)

            sampler2D _MainTex;

            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v); // Correct way to set instance ID
                UNITY_TRANSFER_INSTANCE_ID(v, o); // Pass to fragment
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i); // Correct way to set instance ID in fragment
                float4 color = UNITY_ACCESS_INSTANCED_PROP(Props, _BaseColor); // Apply per-instance color
                float metallic = UNITY_ACCESS_INSTANCED_PROP(Props, _Metallic); // Get per-instance metallic
                float smoothness = metallic; // Smoothness is 0 when metallic is 0, and 1 when metallic is 1
                float4 texColor = tex2D(_MainTex, i.uv); // Sample texture
                return texColor * color; // Combine texture and color
            }
            ENDCG
        }
    }
}



