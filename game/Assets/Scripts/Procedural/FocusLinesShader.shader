Shader "Custom/FocusLinesShaderURP"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _LineCount ("Line Count", Float) = 20
        _Speed ("Rotation Speed", Float) = 1
        _LineColor ("Line Color", Color) = (1, 1, 1, 1) // �W�����̐F
        _Offset ("Line Start Offset", Float) = 0.2 // �������S���痣���I�t�Z�b�g�l
        _LineFade ("Line Fade", Float) = 1.5 // ���̏I�[�̃t�F�[�h�A�E�g����
           _ShowFocusLines ("Show Focus Lines", Float) = 1 // �W�����̕\��/��\���𐧌䂷��t���O

    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 200

        Pass
        {
            ZTest Always Cull Off ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha // ���ߐݒ�

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float _LineCount;
            float _Speed;
            float4 _LineColor; // �W�����̐F
            float _Offset; // �W�����̒��S����̃I�t�Z�b�g
            float _LineFade; // ���̏I�[�̃t�F�[�h�A�E�g����
            float _ShowFocusLines;
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

            half4 frag (Varyings IN) : SV_Target
            {

                // UV���W���璆�S�����̃x�N�g�����擾
                float2 dir = IN.uv - float2(0.5, 0.5);

                // �������擾���A���̊J�n�I�t�Z�b�g�𔽉f
                float distance = length(dir);
                float newDistance = max(distance - _Offset, 0.0); // ���S����I�t�Z�b�g�������

                // �V����UV���W�𐳋K�����čČv�Z
                float2 newUV = normalize(dir) * newDistance + float2(0.5, 0.5);

                // �W�����̊p�x���v�Z
                float angle = atan2(newUV.y - 0.5, newUV.x - 0.5);
                float focusLine = abs(sin(angle * _LineCount + _Time.y * _Speed));

                // ���������Ȃ�قǓ����ɂ���
                float fadeFactor = saturate(distance * _LineFade);

                // ���̐F�ƃt�F�[�h���ʂ�K�p
                half4 lineColor = half4(_LineColor.rgb * fadeFactor, focusLine * fadeFactor);

                // �w�i�̃e�N�X�`�����擾
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);

                  // �W�����̃t���O���L���ȏꍇ�̂ݕ`��
                if (_ShowFocusLines > 0.5)
                {
                // �w�i�̐F�ƏW�����̐F���d�˂�
                col.rgb = lerp(col.rgb, lineColor.rgb, focusLine * fadeFactor); // �W�����̐F���d�˂�
                col.a *= focusLine * fadeFactor; // �����x��K�p
                }
              

                return col;
            }
            ENDHLSL
        }
    }
}
