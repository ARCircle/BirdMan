Shader "Custom/URPToonOutlineWithHeightColorLoopAndShadowsAndRange"
{
    Properties
    {
        _Color1 ("Color 1", Color) = (0, 1, 0, 1)
        _Color2 ("Color 2", Color) = (1, 0, 0, 1)
        _Color3 ("Color 3", Color) = (1, 1, 0, 1)
        _Color4 ("Color 4", Color) = (0, 0, 1, 1)
        _Color5 ("Color 5", Color) = (0, 1, 1, 1)
        _Color6 ("Color 6", Color) = (1, 0, 1, 1)
        _Color7 ("Color 7", Color) = (0.5, 0.5, 0, 1)
        _Color8 ("Color 8", Color) = (0.5, 0, 0.5, 1)
        _Color9 ("Color 9", Color) = (0, 0.5, 0.5, 1)
        _HeightInterval1 ("Height Interval 1", Float) = 1.0
        _HeightBorder1 ("Height Border 1", Float) = 3.0
        _HeightInterval2 ("Height Interval 2", Float) = 1.0
        _HeightBorder2 ("Height Border 2", Float) = 1.0
        _HeightInterval3 ("Height Interval 3", Float) = 1.0
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineWidth ("Outline Width", Float) = 0.05
        _ShadowPower("影の強さ", Range(0, 1)) = 0.8
        _PlayerPosition ("Player Position", Vector) = (0, 0, 0)
        _Range ("Display Range", Float) = 10.0
        _UseRange ("Use Range Flag", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalRenderPipeline" }

        Pass
        {
            Name "Toon Pass"
            Tags { "LightMode" = "UniversalForward" }
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _Color1, _Color2, _Color3;
                float4 _Color4, _Color5, _Color6;
                float4 _Color7, _Color8, _Color9;
                float _HeightInterval1, _HeightBorder1;
                float _HeightInterval2, _HeightBorder2;
                float _HeightInterval3;
                float _ShadowPower;
                float3 _PlayerPosition;
                float _Range;
                float _UseRange;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 positionWS : POSITIONWS;
                float3 viewDirWS : TEXCOORD1;
                float4 shadowCoord : TEXCOORD2;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                OUT.positionWS = TransformObjectToWorld(IN.positionOS);
                OUT.positionHCS = TransformWorldToHClip(OUT.positionWS);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.viewDirWS = GetCameraPositionWS() - OUT.positionWS;

                #if defined(_MAIN_LIGHT_SHADOWS)
                VertexPositionInputs vertexInput = GetVertexPositionInputs(IN.positionOS);
                OUT.shadowCoord = GetMainLightShadowCoord(vertexInput);
                #endif

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // フラグが1の場合、範囲内に収まっているか確認
                if (_UseRange == 1.0)
                {
                    float3 offset = IN.positionWS - _PlayerPosition;
                    if (abs(offset.x) > _Range || abs(offset.z) > _Range)
                    {
                        discard; // 範囲外なら描画をスキップ
                    }
                }

                // 高さによる色の決定
                float yPos = IN.positionWS.y;
                float3 baseColor;

                if (yPos >= _HeightBorder1)
                {
                    float modYPos = fmod(yPos, _HeightInterval1 * 3);
                    if (modYPos <= _HeightInterval1)
                    {
                        baseColor = _Color1.rgb;
                    }
                    else if (modYPos <= _HeightInterval1 * 2.0)
                    {
                        baseColor = _Color2.rgb;
                    }
                    else
                    {
                        baseColor = _Color3.rgb;
                    }
                }
                else if (yPos >= _HeightBorder2)
                {
                    float modYPos = fmod(yPos - _HeightBorder2, _HeightInterval2 * 3);
                    if (modYPos <= _HeightInterval2)
                    {
                        baseColor = _Color4.rgb;
                    }
                    else if (modYPos <= _HeightInterval2 * 2.0)
                    {
                        baseColor = _Color5.rgb;
                    }
                    else
                    {
                        baseColor = _Color6.rgb;
                    }
                }
                else
                {
                    float modYPos = fmod(yPos - _HeightBorder2, _HeightInterval3 * 3);
                    if (modYPos <= _HeightInterval3)
                    {
                        baseColor = _Color7.rgb;
                    }
                    else if (modYPos <= _HeightInterval3 * 2.0)
                    {
                        baseColor = _Color8.rgb;
                    }
                    else
                    {
                        baseColor = _Color9.rgb;
                    }
                }

                // 影の適用
                half shadowAttenuation = 1.0;
                #if defined(_MAIN_LIGHT_SHADOWS)
                shadowAttenuation = SAMPLE_SHADOW_ATTENUATION(IN.shadowCoord);
                #endif

                // 影の強度に基づいて色を補間
                baseColor *= lerp(1, shadowAttenuation, _ShadowPower);

                return float4(baseColor, 1.0);
            }
            ENDHLSL
        }

        Pass
        {
            Name "Outline Pass"
            Cull Front
            ZWrite Off
            ZTest Less

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _OutlineColor;
                float _OutlineWidth;
                float3 _PlayerPosition;
                float _Range;
                float _UseRange;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : POSITIONWS;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 normalOS = normalize(IN.normalOS);
                float3 expandedPosOS = IN.positionOS.xyz + normalOS * _OutlineWidth;
                OUT.positionWS = TransformObjectToWorld(float4(expandedPosOS, 1.0)).xyz;
                OUT.positionHCS = TransformWorldToHClip(OUT.positionWS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // フラグが1の場合、範囲内に収まっているか確認
                if (_UseRange == 1.0)
                {
                    float3 offset = IN.positionWS - _PlayerPosition;
                    if (abs(offset.x) > _Range || abs(offset.z) > _Range)
                    {
                        discard; // 範囲外ならアウトラインも描画しない
                    }
                }

                return _OutlineColor;
            }
            ENDHLSL
        }
    }
}
