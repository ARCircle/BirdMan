Shader "Unlit/Plane"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ShadowPower("�e�̋���", Range(0, 1)) = 0.8
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"          
        }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                float4 shadowCoord : TEXCOORD1;
            };

            struct v2f
            {
                float4 positionCS               : SV_POSITION;
                float3 positionWS               : TEXCOORD0;
                
                float2 uv : TEXCOORD1;
                // �V���h�E���W��ǉ�
                float4 shadowCoord  : TEXCOORD2;

            };

            sampler2D _MainTex;

            CBUFFER_START(UnityPerMaterial)         

            float4 _MainTex_ST;
            float _ShadowPower;
            CBUFFER_END

            v2f vert(appdata input) // ���_�V�F�[�_
            {
                v2f output= (v2f)0;                
                output.positionWS.xyz = input.vertex;
                output.positionCS =  TransformObjectToHClip(input.vertex);
                output.uv=input.uv*_MainTex_ST.zw;
                return output; 
            }

            Light GetMainLightShadowMap(float4 shadowCoord)
            {
                // ���C�����C�g�̏����擾
                Light light = GetMainLight();

                /// RealTimeShadow�̌v�Z ///
                // ���C�����C�g�̃V���h�E�p�����[�^���擾
                half4 shadowParams = GetMainLightShadowParams();
                // �V���h�E�̋��x���擾
                half shadowStrength = shadowParams.x;
                // �V���h�E�T���v�����O�f�[�^���擾
                ShadowSamplingData shadowSamplingData = GetMainLightShadowSamplingData();

                half attenuation;
                // �V���h�E�}�b�v�e�N�X�`������V���h�E�̌������v�Z
                attenuation = SAMPLE_TEXTURE2D_SHADOW(_MainLightShadowmapTexture, sampler_MainLightShadowmapTexture, shadowCoord.xyz);
                // �t�B���^�����O���ꂽ�V���h�E�}�b�v����V���h�E�̌������v�Z
                attenuation = SampleShadowmapFiltered(TEXTURE2D_SHADOW_ARGS(_MainLightShadowmapTexture, sampler_MainLightShadowmapTexture), shadowCoord, shadowSamplingData);
                // �V���h�E�̋��x�ɉ����Č�������
                attenuation = LerpWhiteTo(attenuation, shadowStrength);

                // �V���h�E����������ꍇ�͌�����1.0�ɁA�����łȂ��ꍇ�͌v�Z�����������g�p
                half shadowAttenuation = BEYOND_SHADOW_FAR(shadowCoord) ? 1.0 : attenuation;                ///

                // ���C�g�̃V���h�E������ݒ�
                light.shadowAttenuation = shadowAttenuation;

                // �v�Z�������C�g����Ԃ�
                return light;
            }     

            half4 frag(v2f input) : SV_Target // �t���O�����g�V�F�[�_
            {
                float4 color = tex2D(_MainTex, input.uv);

                // ���[���h���W���V���h�E���W�ɕϊ�
                half4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                                
                // ���C�����C�g�Ƃ��̃V���h�E���擾
                Light mainLight = GetMainLightShadowMap(shadowCoord);
                
                // �V���h�E�̐F���v�Z�B�V���h�E�̋��x�ɉ����ĐF����
                half3 shadowColor =  lerp(float4(1-_ShadowPower.xxx,1), float4(1,1,1,1),  mainLight.shadowAttenuation);                
                
                // �ŏI�I�ȐF�ɃV���h�E�̐F����Z
                color.rgb *= shadowColor;                

                return color; // �F��Ԃ�
            }
            ENDHLSL
        }
    }
}
