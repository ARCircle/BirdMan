Shader "Custom/URPToonOutlineShaderWithRange"
{
    Properties
    {
        _BaseColorHigh ("High Light Color", Color) = (1, 1, 1, 1)
        _BaseColorMid ("Mid Light Color", Color) = (0.5, 0.5, 0.5, 1)
        _BaseColorLow ("Low Light Color", Color) = (0.2, 0.2, 0.2, 1)
        _High ("High", Float) = 0.6
        _Low ("Low", Float) = 0.3

        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineWidth ("Outline Width", Float) = 0.05
        _PlayerPosition ("Player Position", Vector) = (0, 0, 0) // Player�̃��[���h���W
        _Range ("Display Range", Float) = 10.0 // �\���͈�
        _UseRange ("Use Range Flag", Float) = 1.0 // �\���͈͐�����g�����ǂ����̃t���O�i1:�g��, 0:�g��Ȃ��j
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

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColorHigh;
                float4 _BaseColorMid;
                float4 _BaseColorLow;
                float3 _PlayerPosition;
                float _High;
                float _Low;

                float _Range;
                float _UseRange; // �t���O��ǉ�
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS : NORMALWS;
                float2 uv : TEXCOORD0;
                float3 positionWS : POSITIONWS;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionWS = TransformObjectToWorld(IN.positionOS);
                OUT.positionHCS = TransformWorldToHClip(OUT.positionWS);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // �t���O��1�̏ꍇ�ɂ̂ݔ͈͂��`�F�b�N
                if (_UseRange == 1.0)
                {
                    float3 offset = IN.positionWS - _PlayerPosition;
                      if (offset.x > _Range || offset.z> _Range)
                    {
                        discard; // �͈͊O�Ȃ�`����X�L�b�v
                    }
                }

                // ���C�����C�g���擾
                Light mainLight = GetMainLight();
                float3 lightDir = normalize(mainLight.direction);

                // �@���𐳋K��
                float3 normal = normalize(IN.normalWS);

                // �@���ƌ����̕����̓��ς��v�Z
                float NdotL = max(dot(normal, lightDir), 0.0);

                // 3�i�K�̐F�ω��� step �Ŏ���
                float3 color;
                if (NdotL > _High)
                {
                    color = _BaseColorHigh.rgb;
                }
                else if (NdotL > _Low)
                {
                    color = _BaseColorMid.rgb;
                }
                else
                {
                    color = _BaseColorLow.rgb;
                }

                return float4(color, 1.0);
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
                float _UseRange; // �t���O��ǉ�
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
                // �t���O��1�̏ꍇ�ɂ̂ݔ͈͂��`�F�b�N
                if (_UseRange == 1.0)
                {
                    float3 offset = IN.positionWS - _PlayerPosition;
                      if (offset.x > _Range || offset.z> _Range)
                    {
                        discard; // �͈͊O�Ȃ�`����X�L�b�v
                    }
                }

                return _OutlineColor;
            }
            ENDHLSL
        }
    }
}
