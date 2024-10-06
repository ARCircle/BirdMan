Shader "Custom/ThreeLevelLightingShader"
{
    Properties
    {
        _HighColor ("High Light Color", Color) = (1, 1, 1, 1) // �������������̐F
        _MidColor ("Mid Light Color", Color) = (0.5, 0.5, 0.5, 1) // �����x�̌��̐F
        _LowColor ("Low Light Color", Color) = (0.2, 0.2, 0.2, 1) // �Â������̐F
        _HighThreshold ("High Threshold", Float) = 0.7 // �������C�g��臒l
        _LowThreshold ("Low Threshold", Float) = 0.3 // �Ⴂ���C�g��臒l
        _Glossiness ("Smoothness", Range(0.0, 1.0)) = 0.5
        _Metallic ("Metallic", Range(0.0, 1.0)) = 0.0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows

        #include "UnityCG.cginc"

        struct Input
        {
            float3 worldNormal;
            float3 worldPos;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _HighColor;
        fixed4 _MidColor;
        fixed4 _LowColor;
        float _HighThreshold;
        float _LowThreshold;

        // �V�F�[�_�[�̃T�[�t�F�X�֐�
        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            // �@���𐳋K��
            float3 normalWorld = normalize(IN.worldNormal);

            // ���C�����C�g�̕������擾
            float3 lightDir = normalize(UnityWorldSpaceLightDir(IN.worldPos));

            // �@���ƃ��C�g�̕����̓��ρi���C�g�̓������j���v�Z
            float NdotL = saturate(dot(normalWorld, lightDir));

            // 3�i�K�̃��C�g�̋��x�ɂ��F�̐ݒ�
            if (NdotL > _HighThreshold)
            {
                o.Albedo = _HighColor.rgb;
            }
            else if (NdotL > _LowThreshold)
            {
                o.Albedo = _MidColor.rgb;
            }
            else
            {
                o.Albedo = _LowColor.rgb;
            }

            // ���^���b�N�Ɗ��炩���𔽉f
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
        }
        ENDCG
    }

    FallBack "Diffuse"
}
