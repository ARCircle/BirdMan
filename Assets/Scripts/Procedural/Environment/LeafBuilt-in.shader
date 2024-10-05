Shader "Custom/LeafShaderWithNoiseAndRange"
{
    Properties
    {
        _BaseColorHigh ("High Light Color", Color) = (1, 1, 1, 1)
        _BaseColorMid ("Mid Light Color", Color) = (0.5, 0.5, 0.5, 1)
        _BaseColorLow ("Low Light Color", Color) = (0.2, 0.2, 0.2, 1)
        _High ("High", Float) = 0.6
        _Low ("Low", Float) = 0.3

        _NoiseScale ("Noise Scale", Float) = 0.1
        _NoiseStrength ("Noise Strength", Float) = 0.2
        _LeafPower ("Leaf Power", Float) = 1.0
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineWidth ("Outline Width", Float) = 0.01
        _PlayerPosition ("Player Position", Vector) = (0, 0, 0, 0)
        _Range ("Display Range", Float) = 10.0
        _UseRange ("Use Range Flag", Float) = 1.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            Name "ForwardLit"
            Cull Back
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float2 uv : TEXCOORD1;
                float3 displacedPositionWS : TEXCOORD2;
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

            // �m�C�Y�����֐�
            float GenerateNoise(float3 pos)
            {
                return frac(sin(dot(pos, float3(12.9898, 78.233, 37.719))) * 43758.5453);
            }

            v2f vert(appdata v)
            {
                v2f o;

                // �@�������Ƀm�C�Y��������
                float noise = GenerateNoise(v.vertex.xyz * _NoiseScale);
                float3 displacedPosition = v.vertex.xyz + v.normal * noise * _NoiseStrength;

                // �N���b�v���W�ւ̕ϊ�
                o.pos = UnityObjectToClipPos(float4(displacedPosition, 1.0));

                // ���[���h��Ԃ̖@��
                o.normalWS = normalize(mul((float3x3)unity_ObjectToWorld, v.normal));

                o.uv = v.uv;

                // �ό`��̃��[���h���W
                o.displacedPositionWS = mul(unity_ObjectToWorld, float4(displacedPosition, 1.0)).xyz;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // �͈̓`�F�b�N
                if (_UseRange == 1.0)
                {
                    float3 offset = i.displacedPositionWS - _PlayerPosition;
                    float distSq = offset.x * offset.x + offset.z * offset.z;
                    if (distSq > _Range * _Range)
                    {
                        discard; // �͈͊O�Ȃ�`����X�L�b�v
                    }
                }

                // ���C�����C�g�̕������擾
                float3 lightDir;
                if (_WorldSpaceLightPos0.w == 0.0)
                {
                    // ���s����
                    lightDir = normalize(_WorldSpaceLightPos0.xyz);
                }
                else
                {
                    // �_�����i�K�v�ɉ����ď�����ǉ��j
                    lightDir = normalize(_WorldSpaceLightPos0.xyz - i.displacedPositionWS);
                }

                // �@���ƌ����̕����̓��ς��v�Z
                float NdotL = max(dot(i.normalWS, lightDir), 0.0);

                // 3�i�K�̐F�ω�������
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

                return fixed4(color * _LeafPower, 1.0);
            }
            ENDCG
        }

        Pass
        {
            Name "Outline Pass"
            Cull Front
            ZTest Less
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 displacedPositionWS : TEXCOORD0;
            };

            float4 _OutlineColor;
            float _OutlineWidth;
            float _NoiseScale;
            float _NoiseStrength;
            float3 _PlayerPosition;
            float _Range;
            float _UseRange;

            // �m�C�Y�����֐�
            float GenerateNoise(float3 pos)
            {
                return frac(sin(dot(pos, float3(12.9898, 78.233, 37.719))) * 43758.5453);
            }

            v2f vert(appdata v)
            {
                v2f o;

                // �@�������Ƀm�C�Y��������
                float noise = GenerateNoise(v.vertex.xyz * _NoiseScale);
                float3 displacedPosition = v.vertex.xyz + v.normal * noise * _NoiseStrength;

                // �@�������[���h��Ԃɕϊ�
                float3 normalWS = normalize(mul((float3x3)unity_ObjectToWorld, v.normal));

                // �A�E�g���C���̂��߂ɒ��_���g��
                float3 expandedPosition = displacedPosition + normalWS * _OutlineWidth;

                // �N���b�v���W�ւ̕ϊ�
                o.pos = UnityObjectToClipPos(float4(expandedPosition, 1.0));

                // �ό`��̃��[���h���W
                o.displacedPositionWS = mul(unity_ObjectToWorld, float4(displacedPosition, 1.0)).xyz;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // �͈̓`�F�b�N
                if (_UseRange == 1.0)
                {
                    float3 offset = i.displacedPositionWS - _PlayerPosition;
                    float distSq = offset.x * offset.x + offset.z * offset.z;
                    if (distSq > _Range * _Range)
                    {
                        discard; // �͈͊O�Ȃ�`����X�L�b�v
                    }
                }

                return _OutlineColor;
            }
            ENDCG
        }
    }
}
