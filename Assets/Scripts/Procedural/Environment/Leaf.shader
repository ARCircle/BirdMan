Shader "Custom/LeafShaderWithNoiseAndRange"
{
    Properties
    {
        _BaseColorHigh ("High Light Color", Color) = (1, 1, 1, 1) // �������������̐F
        _BaseColorMid ("Mid Light Color", Color) = (0.5, 0.5, 0.5, 1) // �����x�̌��̐F
        _BaseColorLow ("Low Light Color", Color) = (0.2, 0.2, 0.2, 1) // �Â������̐F
        _High ("High", Float) = 0.6
        _Low ("Low", Float) = 0.3

        _NoiseScale ("Noise Scale", Float) = 0.1 // �m�C�Y�̃X�P�[��
        _NoiseStrength ("Noise Strength", Float) = 0.2 // �m�C�Y�̋���
        _LeafPower ("Leaf Power", Float) = 1.0 // ���[�t�̃p���[�i���邳�j
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1) // �A�E�g���C���̐F
        _OutlineWidth ("Outline Width", Float) = 0.01 // �A�E�g���C���̕�
        _PlayerPosition ("Player Position", Vector) = (0, 0, 0) // Player�̃��[���h���W
        _Range ("Display Range", Float) = 10.0 // �\���͈�
        _UseRange ("Use Range Flag", Float) = 1.0 // �͈͐����̗L��/����
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalRenderPipeline" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

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
                float3 displacedPositionWS : TEXCOORD1; // �ό`��̃��[���h���W
            };

            sampler2D _MainTex;
            float4 _BaseColorHigh;
            float4 _BaseColorMid;
            float4 _BaseColorLow;
            float _High;
            float _Low;
            float _NoiseScale;
            float _NoiseStrength;
            float _LeafPower;
            float3 _PlayerPosition;
            float _Range;
            float _UseRange;

            float GenerateNoise(float3 pos)
            {
                return frac(sin(dot(pos.xyz, float3(12.9898, 78.233, 37.719))) * 43758.5453);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                // �@�������Ƀm�C�Y��������
                float noise = GenerateNoise(IN.positionOS.xyz * _NoiseScale);
                float3 displacedPosition = IN.positionOS.xyz + IN.normalOS * noise * _NoiseStrength;

                OUT.positionHCS = TransformObjectToHClip(float4(displacedPosition, 1.0));
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv = IN.uv;
                OUT.displacedPositionWS = TransformObjectToWorld(float4(displacedPosition, 1.0)).xyz;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // �t���O���L���ȏꍇ�̂݁A�͈͂��`�F�b�N
                if (_UseRange == 1.0)
                {
                    float3 offset = IN.displacedPositionWS - _PlayerPosition;
                      if (offset.x > _Range || offset.z> _Range)
                    {
                        discard; // �͈͊O�Ȃ�`����X�L�b�v
                    }
                }

                // ���C�����C�g�̕������擾
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

                return float4(color, 1.0) * _LeafPower;
            }
            ENDHLSL
        }

        Pass
        {
            Name "Outline Pass"
            Cull Front
            ZTest Less
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 displacedPositionWS : TEXCOORD0; // �ό`��̃��[���h���W
            };

            float4 _OutlineColor;
            float _OutlineWidth;
            float _NoiseScale;
            float _NoiseStrength;
            float3 _PlayerPosition;
            float _Range;
            float _UseRange;

            float GenerateNoise(float3 pos)
            {
                return frac(sin(dot(pos.xyz, float3(12.9898, 78.233, 37.719))) * 43758.5453);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                // �@�������Ƀm�C�Y��������
                float noise = GenerateNoise(IN.positionOS.xyz * _NoiseScale);
                float3 displacedPosition = IN.positionOS.xyz + IN.normalOS * noise * _NoiseStrength;

                float3 normalOS = normalize(IN.normalOS);
                float3 expandedPosOS = displacedPosition + normalOS * _OutlineWidth;
                OUT.positionHCS = TransformObjectToHClip(float4(expandedPosOS, 1.0));
                OUT.displacedPositionWS = TransformObjectToWorld(float4(displacedPosition, 1.0)).xyz;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // �t���O���L���ȏꍇ�̂݁A�͈͂��`�F�b�N
                if (_UseRange == 1.0)
                {
                    float3 offset = IN.displacedPositionWS - _PlayerPosition;
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
