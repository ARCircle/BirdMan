Shader "Custom/MetaballShader"
{
    Properties
    {
        _MetaballRadius ("Metaball Radius", Float) = 0.5
        _Threshold ("Threshold", Float) = 1.0
        _MainTex ("Base Texture", 2D) = "white" {}
        _MetaballCount ("Metaball Count", Float) = 3
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            ZTest Always
            ZWrite Off
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // Metaball data
            float _MetaballRadius; // ���^�{�[���̔��a
            float _Threshold;      // �t�B�[���h�l��臒l
            float _MetaballCount;  // ���^�{�[���̐�
            float4 _MetaballPositions[10]; // �ő�10�̃��^�{�[���ʒu���i�[����z��

            // Base texture
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionHCS : SV_POSITION;
            };

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
                OUT.uv = IN.uv;
                return OUT;
            }

            // �����Ɋ�Â����^�{�[���̃t�B�[���h�l�v�Z
            float MetaballField(float3 position)
            {
                float field = 0.0;
                for (int i = 0; i < _MetaballCount; i++)
                {
                    float3 metaballPos = _MetaballPositions[i].xyz;
                    float dist = length(position - metaballPos);  // ���^�{�[���Ƃ̋���
                    float influence = 1.0 - (dist / _MetaballRadius); // ���^�{�[���̉e���͈�
                    field += max(0.0, influence * influence); // �v���x�����Z
                }
                return field;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                // ��ʏ�̊e�s�N�Z���ɂ�����ʒu
                float3 worldPos = float3(IN.uv * 2.0 - 1.0, 0.0); // UV���烏�[���h���W��

                // ���^�{�[���̃t�B�[���h�l���v�Z
                float field = MetaballField(worldPos);

                // 臒l�𒴂��邩�ǂ����ŐF������i���^�{�[�������͔��A�O���͓����j
                if (field > _Threshold)
                {
                    return half4(1, 1, 1, 1); // ���^�{�[�������͔�
                }
                else
                {
                    return half4(0, 0, 0, 0); // ���^�{�[���O���͓���
                }
            }
            ENDHLSL
        }
    }
}
