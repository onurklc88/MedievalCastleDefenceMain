Shader "Custom/URP Wireframe"
{
    Properties
    {
        _MainTex ("Base Map", 2D) = "white" {}
        _WireColor ("Wireframe Color", Color) = (1,0,0,1)
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalRenderPipeline" }
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float4 _WireColor;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS);
                o.uv = v.uv.xy;
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                return _WireColor; // Sadece wireframe rengi
            }
            ENDHLSL
        }
    }
}
