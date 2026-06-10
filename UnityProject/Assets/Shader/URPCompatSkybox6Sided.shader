Shader "ZYQ/URP/Skybox 6 Sided"
{
    Properties
    {
        _Tint ("Tint Color", Color) = (.5, .5, .5, .5)
        _Exposure ("Exposure", Range(0, 8)) = 1
        _Rotation ("Rotation", Range(0, 360)) = 0
        [NoScaleOffset] _FrontTex ("Front [+Z]", 2D) = "grey" {}
        [NoScaleOffset] _BackTex ("Back [-Z]", 2D) = "grey" {}
        [NoScaleOffset] _LeftTex ("Left [+X]", 2D) = "grey" {}
        [NoScaleOffset] _RightTex ("Right [-X]", 2D) = "grey" {}
        [NoScaleOffset] _UpTex ("Up [+Y]", 2D) = "grey" {}
        [NoScaleOffset] _DownTex ("Down [-Y]", 2D) = "grey" {}
    }

    SubShader
    {
        Tags { "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" "RenderPipeline" = "UniversalPipeline" }
        Cull Off
        ZWrite Off

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            half4 _Tint;
            half _Exposure;
            float _Rotation;
        CBUFFER_END

        struct Attributes
        {
            float4 positionOS : POSITION;
            float2 uv : TEXCOORD0;
        };

        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float2 uv : TEXCOORD0;
        };

        Varyings vert(Attributes input)
        {
            Varyings output;
            output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
            output.uv = input.uv;
            return output;
        }

        half4 ApplySkyTint(half4 color)
        {
            color.rgb *= _Tint.rgb * unity_ColorSpaceDouble.rgb * _Exposure;
            return color;
        }
        ENDHLSL

        Pass
        {
            Name "Front"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            TEXTURE2D(_FrontTex); SAMPLER(sampler_FrontTex);
            half4 frag(Varyings input) : SV_Target { return ApplySkyTint(SAMPLE_TEXTURE2D(_FrontTex, sampler_FrontTex, input.uv)); }
            ENDHLSL
        }

        Pass
        {
            Name "Back"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            TEXTURE2D(_BackTex); SAMPLER(sampler_BackTex);
            half4 frag(Varyings input) : SV_Target { return ApplySkyTint(SAMPLE_TEXTURE2D(_BackTex, sampler_BackTex, input.uv)); }
            ENDHLSL
        }

        Pass
        {
            Name "Left"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            TEXTURE2D(_LeftTex); SAMPLER(sampler_LeftTex);
            half4 frag(Varyings input) : SV_Target { return ApplySkyTint(SAMPLE_TEXTURE2D(_LeftTex, sampler_LeftTex, input.uv)); }
            ENDHLSL
        }

        Pass
        {
            Name "Right"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            TEXTURE2D(_RightTex); SAMPLER(sampler_RightTex);
            half4 frag(Varyings input) : SV_Target { return ApplySkyTint(SAMPLE_TEXTURE2D(_RightTex, sampler_RightTex, input.uv)); }
            ENDHLSL
        }

        Pass
        {
            Name "Up"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            TEXTURE2D(_UpTex); SAMPLER(sampler_UpTex);
            half4 frag(Varyings input) : SV_Target { return ApplySkyTint(SAMPLE_TEXTURE2D(_UpTex, sampler_UpTex, input.uv)); }
            ENDHLSL
        }

        Pass
        {
            Name "Down"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            TEXTURE2D(_DownTex); SAMPLER(sampler_DownTex);
            half4 frag(Varyings input) : SV_Target { return ApplySkyTint(SAMPLE_TEXTURE2D(_DownTex, sampler_DownTex, input.uv)); }
            ENDHLSL
        }
    }
}
