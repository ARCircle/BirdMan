Shader "Custom/LeafShaderWithNoiseAndRange"
{
    Properties
    {
        _HighColor ("High Light Color", Color) = (1, 1, 1, 1) // �������������̐F
        _MidColor ("Mid Light Color", Color) = (0.5, 0.5, 0.5, 1) // �����x�̌��̐F
        _LowColor ("Low Light Color", Color) = (0.2, 0.2, 0.2, 1) // �Â������̐F
        _HighThreshold ("High Threshold", Range(0,1)) = 0.7 // �������C�g��臒l
        _LowThreshold ("Low Threshold", Range(0,1)) = 0.3 // �Ⴂ���C�g��臒l
        _Glossiness ("Smoothness", Range(0.0, 1.0)) = 0.5 // ���炩��
        _Metallic ("Metallic", Range(0.0, 1.0)) = 0.0 // ���^���b�N�x

        _NoiseScale ("Noise Scale", Float) = 0.1 // �m�C�Y�̃X�P�[��
        _NoiseStrength ("Noise Strength", Float) = 0.2 // �m�C�Y�̋���
        _LeafPower ("Leaf Power", Float) = 1.0 // ���[�t�̖��邳

        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1) // �A�E�g���C���̐F
        _OutlineWidth ("Outline Width", Float) = 0.01 // �A�E�g���C���̕�

        _PlayerPosition ("Player Position", Vector) = (0, 0, 0) // �v���C���[�̍��W
        _Range ("Display Range", Float) = 10.0 // �\���͈�
        _UseRange ("Use Range Flag", Float) = 1.0 // �͈͐����̃t���O
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }

        // ���C����Pass�iSurface Shader�j
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert addshadow

        #include "UnityCG.cginc"

        struct Input
        {
            float2 uv_MainTex;
            float3 worldNormal;
            float3 worldPos;
        };

        // �v���p�e�B
        fixed4 _HighColor;
        fixed4 _MidColor;
        fixed4 _LowColor;
        float _HighThreshold;
        float _LowThreshold;
        half _Glossiness;
        half _Metallic;

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

        // ���_�V�F�[�_�[
        void vert(inout appdata_full v)
        {
            // �m�C�Y�ɂ�钸�_�ό`
            float noise = GenerateNoise(v.vertex.xyz * _NoiseScale);
            v.vertex.xyz += v.normal * noise * _NoiseStrength;
        }

        // �T�[�t�F�C�X�V�F�[�_�[
        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            // �͈̓`�F�b�N
            if (_UseRange == 1.0)
            {
                float3 offset = IN.worldPos - _PlayerPosition;
                float distSq = offset.x * offset.x + offset.z * offset.z;
                if (distSq > _Range * _Range)
                {
                    discard; // �͈͊O�Ȃ�s�N�Z����`�悵�Ȃ�
                }
            }

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

            // ���邳�̓K�p
            o.Albedo *= _LeafPower;

            // ���^���b�N�Ɗ��炩���𔽉f
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
        }
        ENDCG

        // �A�E�g���C���p�X
        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "Always" }
            Cull Front
            ZTest LEqual
            ZWrite Off
            ColorMask RGB

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _OutlineColor;
            float _OutlineWidth;

            float _NoiseScale;
            float _NoiseStrength;
            float3 _PlayerPosition;
            float _Range;
            float _UseRange;

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

            // �m�C�Y�����֐�
            float GenerateNoise(float3 pos)
            {
                return frac(sin(dot(pos, float3(12.9898, 78.233, 37.719))) * 43758.5453);
            }

            void vert(in appdata v, out v2f o)
            {
                UNITY_INITIALIZE_OUTPUT(v2f, o);

                // �m�C�Y�ɂ�钸�_�ό`
                float noise = GenerateNoise(v.vertex.xyz * _NoiseScale);
                float3 displacedPosition = v.vertex.xyz + v.normal * noise * _NoiseStrength;

                // �@�������[���h��Ԃɕϊ�
                float3 normalWS = UnityObjectToWorldNormal(v.normal);

                // �A�E�g���C���̂��߂̒��_�g��
                displacedPosition += normalWS * _OutlineWidth;

                // �N���b�v��Ԃւ̕ϊ�
                o.pos = UnityObjectToClipPos(float4(displacedPosition, 1.0));

                // ���[���h���W���擾
                o.worldPos = mul(unity_ObjectToWorld, float4(displacedPosition, 1.0)).xyz;
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
    FallBack "Diffuse"
}
