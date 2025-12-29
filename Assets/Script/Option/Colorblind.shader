Shader "Hidden/URP/Colorblind"
{
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Name "ColorblindPass"
            ZWrite Off
            ZTest Always
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            int _Mode;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings Vert (Attributes input)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                o.uv = input.uv;
                return o;
            }

            float4 Frag (Varyings i) : SV_Target
            {
                float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

                // Protanopia
                if (_Mode == 1)
                {
                    col.rgb = float3(
                        col.r * 0.56667 + col.g * 0.43333,
                        col.r * 0.55833 + col.g * 0.44167,
                        col.b
                    );
                }
                // Deuteranopia
                else if (_Mode == 2)
                {
                    col.rgb = float3(
                        col.r * 0.625 + col.g * 0.375,
                        col.r * 0.70 + col.g * 0.30,
                        col.b
                    );
                }
                // Tritanopia
                else if (_Mode == 3)
                {
                    col.rgb = float3(
                        col.r,
                        col.g * 0.95 + col.b * 0.05,
                        col.g * 0.433 + col.b * 0.567
                    );
                }

                return col;
            }
            ENDHLSL
        }
    }
}
