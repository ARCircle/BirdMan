Shader "Custom/FocusLinesShaderURP"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _LineCount ("Line Count", Float) = 20
        _Speed ("Rotation Speed", Float) = 1
        _LineColor ("Line Color", Color) = (1, 1, 1, 1) // 集中線の色
        _Offset ("Line Start Offset", Float) = 0.2 // 線が中心から離れるオフセット値
        _LineFade ("Line Fade", Float) = 1.5 // 線の終端のフェードアウト効果
           _ShowFocusLines ("Show Focus Lines", Float) = 1 // 集中線の表示/非表示を制御するフラグ

    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 200

        Pass
        {
            ZTest Always Cull Off ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha // 透過設定

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float _LineCount;
            float _Speed;
            float4 _LineColor; // 集中線の色
            float _Offset; // 集中線の中心からのオフセット
            float _LineFade; // 線の終端のフェードアウト効果
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

                // UV座標から中心方向のベクトルを取得
                float2 dir = IN.uv - float2(0.5, 0.5);

                // 距離を取得し、線の開始オフセットを反映
                float distance = length(dir);
                float newDistance = max(distance - _Offset, 0.0); // 中心からオフセット分離れる

                // 新しいUV座標を正規化して再計算
                float2 newUV = normalize(dir) * newDistance + float2(0.5, 0.5);

                // 集中線の角度を計算
                float angle = atan2(newUV.y - 0.5, newUV.x - 0.5);
                float focusLine = abs(sin(angle * _LineCount + _Time.y * _Speed));

                // 線が遠くなるほど透明にする
                float fadeFactor = saturate(distance * _LineFade);

                // 線の色とフェード効果を適用
                half4 lineColor = half4(_LineColor.rgb * fadeFactor, focusLine * fadeFactor);

                // 背景のテクスチャを取得
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);

                  // 集中線のフラグが有効な場合のみ描画
                if (_ShowFocusLines > 0.5)
                {
                // 背景の色と集中線の色を重ねる
                col.rgb = lerp(col.rgb, lineColor.rgb, focusLine * fadeFactor); // 集中線の色を重ねる
                col.a *= focusLine * fadeFactor; // 透明度を適用
                }
              

                return col;
            }
            ENDHLSL
        }
    }
}
