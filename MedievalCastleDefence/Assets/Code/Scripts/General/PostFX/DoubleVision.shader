Shader "Custom/DoubleVision"
{
    Properties
    {
        _MainTex("Base (RGB)", 2D) = "white" {}
        _Offset("Offset", Float) = 0.02
        _Intensity("Intensity", Range(0, 1)) = 0.5
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

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

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float _Offset;
            float _Intensity;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
{
    // Ana renk
    half4 mainColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
    
    // Ofsetli renk (tüm ekrana uygula)
    float2 offsetUV = IN.uv + float2(_Offset, 0);
    half4 offsetColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, offsetUV);
    
    // MERKEZ DÜZELTME: Tüm ekraný etkilemek için centerFactor'ý kaldýrýn
    half4 finalColor = lerp(mainColor, offsetColor, _Intensity);
    return finalColor;
}
            ENDHLSL
        }
    }
}