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
            float _MetaballRadius; // メタボールの半径
            float _Threshold;      // フィールド値の閾値
            float _MetaballCount;  // メタボールの数
            float4 _MetaballPositions[10]; // 最大10個のメタボール位置を格納する配列

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

            // 距離に基づくメタボールのフィールド値計算
            float MetaballField(float3 position)
            {
                float field = 0.0;
                for (int i = 0; i < _MetaballCount; i++)
                {
                    float3 metaballPos = _MetaballPositions[i].xyz;
                    float dist = length(position - metaballPos);  // メタボールとの距離
                    float influence = 1.0 - (dist / _MetaballRadius); // メタボールの影響範囲
                    field += max(0.0, influence * influence); // 貢献度を加算
                }
                return field;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                // 画面上の各ピクセルにおける位置
                float3 worldPos = float3(IN.uv * 2.0 - 1.0, 0.0); // UVからワールド座標へ

                // メタボールのフィールド値を計算
                float field = MetaballField(worldPos);

                // 閾値を超えるかどうかで色を決定（メタボール内部は白、外部は透明）
                if (field > _Threshold)
                {
                    return half4(1, 1, 1, 1); // メタボール内部は白
                }
                else
                {
                    return half4(0, 0, 0, 0); // メタボール外部は透明
                }
            }
            ENDHLSL
        }
    }
}
