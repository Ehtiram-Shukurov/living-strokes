Shader "Custom/LivingStroke_GPU"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _MotionTex ("Motion Texture (Float)", 2D) = "black" {}
        _Speed ("Playback Speed", Float) = 0.2
        _WaveDensity ("Wave Density", Float) = 1.0
        _Amplitude ("Gesture Amplitude", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float _Speed;
                float _WaveDensity;
                float _Amplitude;
            CBUFFER_END

            TEXTURE2D(_MotionTex);
            SAMPLER(sampler_MotionTex);

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                float timeOffset = _Time.y * _Speed;
                float samplePoint = frac(timeOffset - (IN.uv.x * _WaveDensity));

                float4 rawOffset = SAMPLE_TEXTURE2D_LOD(_MotionTex, sampler_MotionTex, float2(samplePoint, 0.5), 0);

                float envelope = 0.5 + sin(IN.uv.x * 3.14159) * 0.5;
                
                float3 animatedPos = IN.positionOS.xyz + (rawOffset.xyz * envelope * _Amplitude);

                OUT.positionHCS = TransformObjectToHClip(animatedPos);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
                return _BaseColor;
            }
            ENDHLSL
        }
    }
}