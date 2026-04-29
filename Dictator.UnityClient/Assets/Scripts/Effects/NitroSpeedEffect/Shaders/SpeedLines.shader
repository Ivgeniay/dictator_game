Shader "Custom/SpeedLines"
{
    Properties
    {
        _Intensity ("Intensity", Range(0, 1)) = 0
        _LineCount ("Line Count", Range(20, 200)) = 80
        _LineSpeed ("Line Speed", Range(0, 5)) = 2
        _LineWidth ("Line Width", Range(0.001, 0.05)) = 0.01
        _InnerRadius ("Inner Radius", Range(0, 0.5)) = 0.15
        _OuterFade ("Outer Fade", Range(0, 1)) = 0.8
        _Color ("Color", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest Always
        Cull Off

        Pass
        {
            Name "SpeedLines"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _Intensity;
                float _LineCount;
                float _LineSpeed;
                float _LineWidth;
                float _InnerRadius;
                float _OuterFade;
                float4 _Color;
            CBUFFER_END

            float Random(float seed)
            {
                return frac(sin(seed * 127.1 + 311.7) * 43758.5453);
            }

            half4 Frag(Varyings IN) : SV_Target
            {
                half4 cameraColor = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, IN.texcoord);

                float2 uv = IN.texcoord - 0.5;
                uv.x *= _ScreenParams.x / _ScreenParams.y;

                float dist = length(uv);
                float angle = atan2(uv.y, uv.x);

                float normalizedAngle = (angle / (PI * 2.0)) + 0.5;
                float lineIndex = floor(normalizedAngle * _LineCount);
                float lineRandom = Random(lineIndex);

                float lineCenter = (lineIndex + 0.5) / _LineCount;
                float angleDiff = abs(normalizedAngle - lineCenter);
                angleDiff = min(angleDiff, 1.0 - angleDiff);

                float lineAlpha = 1.0 - smoothstep(0.0, _LineWidth, angleDiff * (PI * 2.0));

                float timeOffset = lineRandom * 100.0;
                float animatedDist = frac(dist - _Time.y * _LineSpeed * (0.5 + lineRandom * 0.5) + timeOffset);
                float radialAlpha = 1.0 - smoothstep(0.0, 0.6, animatedDist);

                float innerMask = smoothstep(_InnerRadius * 0.5, _InnerRadius, dist);
                float outerMask = 1.0 - smoothstep(_OuterFade, 1.0, dist);

                float finalAlpha = lineAlpha * radialAlpha * innerMask * outerMask;
                finalAlpha *= _Intensity;
                finalAlpha *= (0.5 + lineRandom * 0.5);

                half4 linesColor = half4(_Color.rgb, finalAlpha * _Color.a);
                return lerp(cameraColor, linesColor, linesColor.a);
            }
            ENDHLSL
        }
    }
}