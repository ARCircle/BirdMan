Shader "Custom/ToonOutlineShaderWithRange"
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
        _PlayerPosition ("Player Position", Vector) = (0, 0, 0, 0) // Player�̃��[���h���W
        _Range ("Display Range", Float) = 10.0 // �\���͈�
        _UseRange ("Use Range Flag", Float) = 1.0 // �\���͈͐�����g�����ǂ����̃t���O�i1:�g��, 0:�g��Ȃ��j
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            Name "Toon Pass"
            Cull Back
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            // �K�v�ȃV�F�[�_�[���C�u�������C���N���[�h
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 normal : TEXCOORD1;
            };

            // �v���p�e�B�̐錾
            float4 _BaseColorHigh;
            float4 _BaseColorMid;
            float4 _BaseColorLow;
            float _High;
            float _Low;

            float3 _PlayerPosition;
            float _Range;
            float _UseRange;

            v2f vert(appdata v)
            {
                v2f o;
                // ���[���h���W�ւ̕ϊ�
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                // �N���b�v���W�ւ̕ϊ�
                o.pos = UnityObjectToClipPos(v.vertex);
                // �@�������[���h���W�n�ɕϊ�
                o.normal = normalize(mul((float3x3)unity_ObjectToWorld, v.normal));
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // �͈̓`�F�b�N
                if (_UseRange == 1.0)
                {
                    float3 offset = i.worldPos - _PlayerPosition;
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
                    // ���s�����̏ꍇ
                    lightDir = normalize(_WorldSpaceLightPos0.xyz);
                }
                else
                {
                    // �_�����̏ꍇ�i�K�v�ɉ����ď�����ǉ��j
                    lightDir = normalize(_WorldSpaceLightPos0.xyz - i.worldPos);
                }

                // �@���ƌ����̕����̓��ς��v�Z
                float NdotL = max(dot(i.normal, lightDir), 0.0);

                // 3�i�K�̐F�ω�
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

                return fixed4(color, 1.0);
            }
            ENDCG
        }

        Pass
        {
            Name "Outline Pass"
            Cull Front
            ZWrite Off
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
                float3 worldPos : TEXCOORD0;
            };

            float4 _OutlineColor;
            float _OutlineWidth;
            float3 _PlayerPosition;
            float _Range;
            float _UseRange;

            v2f vert(appdata v)
            {
                v2f o;
                // �@�������[���h���W�n�ɕϊ�
                float3 normal = normalize(mul((float3x3)unity_ObjectToWorld, v.normal));
                // ���_���g�����ăA�E�g���C�����쐬
                float3 expandedVertex = v.vertex.xyz + normal * _OutlineWidth;
                // ���[���h���W�ւ̕ϊ�
                o.worldPos = mul(unity_ObjectToWorld, float4(expandedVertex, 1.0)).xyz;
                // �N���b�v���W�ւ̕ϊ�
                o.pos = UnityObjectToClipPos(float4(expandedVertex, 1.0));
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // �͈̓`�F�b�N
                if (_UseRange == 1.0)
                {
                    float3 offset = i.worldPos - _PlayerPosition;
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
