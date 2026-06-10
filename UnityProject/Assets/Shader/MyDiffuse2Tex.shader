Shader "ZYQ/Mobile_Diffuse2Tex"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Tex1Color("Tex 1 Color", Color) = (1, 1, 1, 1)
        _MainTex2 ("Base 2 (RGB)", 2D) = "white" {}
        _MultiParam("Multiply", Range(0, 3)) = 1.5
        _AddParam("Add", Color) = (0.1, 0.1, 0.1, 0.1)
        _Tex2Light("Tex 2 Light", Range(0.1, 1)) = 0.5
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }
        LOD 150

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_MainTex2);
            SAMPLER(sampler_MainTex2);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _MainTex2_ST;
                half4 _Tex1Color;
                half _MultiParam;
                half4 _AddParam;
                half _Tex2Light;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv1 : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
                half3 normalWS : TEXCOORD2;
                float3 positionWS : TEXCOORD3;
                half fogFactor : TEXCOORD4;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv1 = TRANSFORM_TEX(input.uv, _MainTex);
                output.uv2 = TRANSFORM_TEX(input.uv, _MainTex2);
                output.fogFactor = ComputeFogFactor(positionInputs.positionCS.z);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 c1 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv1) * _Tex1Color;
                half4 c2 = SAMPLE_TEXTURE2D(_MainTex2, sampler_MainTex2, input.uv2);
                half4 c = c2.a > 0 ? (c1 + c2) * _Tex2Light : c1;

                half3 normalWS = normalize(input.normalWS);
                Light mainLight = GetMainLight(TransformWorldToShadowCoord(input.positionWS));
                half ndotl = saturate(dot(normalWS, mainLight.direction));
                half3 ambient = SampleSH(normalWS);
                half3 lighting = ambient + mainLight.color * ndotl * mainLight.shadowAttenuation;
                half3 color = (c.rgb * _MultiParam + _AddParam.rgb) * lighting;

                color = MixFog(color, input.fogFactor);
                return half4(color, c.a);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
    }
}
