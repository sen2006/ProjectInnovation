Shader "Custom/InstancedColorShader"
{
    Properties
    {
        _BaseColor ("Color", Color) = (1,1,1,1) // Default color (not used in instancing)
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
                UNITY_VERTEX_INPUT_INSTANCE_ID // Ensure instancing is enabled
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
                UNITY_VERTEX_INPUT_INSTANCE_ID // Pass instance ID to fragment
            };

            // ✅ Define per-instance property for color
            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor) // Instanced color
            UNITY_INSTANCING_BUFFER_END(Props)

            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v); // ✅ Correct way to set instance ID
                UNITY_TRANSFER_INSTANCE_ID(v, o); // ✅ Pass to fragment
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i); // ✅ Correct way to set instance ID in fragment
                return UNITY_ACCESS_INSTANCED_PROP(Props, _BaseColor); // ✅ Apply per-instance color
            }
            ENDCG
        }
    }
}

