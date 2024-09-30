Shader "Unlit/Plane"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ShadowPower("影の強さ", Range(0, 1)) = 0.8
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
                // シャドウ座標を追加
                float4 shadowCoord  : TEXCOORD2;

            };

            sampler2D _MainTex;

            CBUFFER_START(UnityPerMaterial)         

            float4 _MainTex_ST;
            float _ShadowPower;
            CBUFFER_END

            v2f vert(appdata input) // 頂点シェーダ
            {
                v2f output= (v2f)0;                
                output.positionWS.xyz = input.vertex;
                output.positionCS =  TransformObjectToHClip(input.vertex);
                output.uv=input.uv*_MainTex_ST.zw;
                return output; 
            }

            Light GetMainLightShadowMap(float4 shadowCoord)
            {
                // メインライトの情報を取得
                Light light = GetMainLight();

                /// RealTimeShadowの計算 ///
                // メインライトのシャドウパラメータを取得
                half4 shadowParams = GetMainLightShadowParams();
                // シャドウの強度を取得
                half shadowStrength = shadowParams.x;
                // シャドウサンプリングデータを取得
                ShadowSamplingData shadowSamplingData = GetMainLightShadowSamplingData();

                half attenuation;
                // シャドウマップテクスチャからシャドウの減衰を計算
                attenuation = SAMPLE_TEXTURE2D_SHADOW(_MainLightShadowmapTexture, sampler_MainLightShadowmapTexture, shadowCoord.xyz);
                // フィルタリングされたシャドウマップからシャドウの減衰を計算
                attenuation = SampleShadowmapFiltered(TEXTURE2D_SHADOW_ARGS(_MainLightShadowmapTexture, sampler_MainLightShadowmapTexture), shadowCoord, shadowSamplingData);
                // シャドウの強度に応じて減衰を補間
                attenuation = LerpWhiteTo(attenuation, shadowStrength);

                // シャドウが遠すぎる場合は減衰を1.0に、そうでない場合は計算した減衰を使用
                half shadowAttenuation = BEYOND_SHADOW_FAR(shadowCoord) ? 1.0 : attenuation;                ///

                // ライトのシャドウ減衰を設定
                light.shadowAttenuation = shadowAttenuation;

                // 計算したライト情報を返す
                return light;
            }     

            half4 frag(v2f input) : SV_Target // フラグメントシェーダ
            {
                float4 color = tex2D(_MainTex, input.uv);

                // ワールド座標をシャドウ座標に変換
                half4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                                
                // メインライトとそのシャドウを取得
                Light mainLight = GetMainLightShadowMap(shadowCoord);
                
                // シャドウの色を計算。シャドウの強度に応じて色を補間
                half3 shadowColor =  lerp(float4(1-_ShadowPower.xxx,1), float4(1,1,1,1),  mainLight.shadowAttenuation);                
                
                // 最終的な色にシャドウの色を乗算
                color.rgb *= shadowColor;                

                return color; // 色を返す
            }
            ENDHLSL
        }
    }
}
