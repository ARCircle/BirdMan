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
        _PlayerPosition ("Player Position", Vector) = (0, 0, 0, 0) // Playerのワールド座標
        _Range ("Display Range", Float) = 10.0 // 表示範囲
        _UseRange ("Use Range Flag", Float) = 1.0 // 表示範囲制御を使うかどうかのフラグ（1:使う, 0:使わない）
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

            // 必要なシェーダーライブラリをインクルード
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

            // プロパティの宣言
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
                // ワールド座標への変換
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                // クリップ座標への変換
                o.pos = UnityObjectToClipPos(v.vertex);
                // 法線をワールド座標系に変換
                o.normal = normalize(mul((float3x3)unity_ObjectToWorld, v.normal));
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 範囲チェック
                if (_UseRange == 1.0)
                {
                    float3 offset = i.worldPos - _PlayerPosition;
                    float distSq = offset.x * offset.x + offset.z * offset.z;
                    if (distSq > _Range * _Range)
                    {
                        discard; // 範囲外なら描画をスキップ
                    }
                }

                // メインライトの方向を取得
                float3 lightDir;
                if (_WorldSpaceLightPos0.w == 0.0)
                {
                    // 平行光源の場合
                    lightDir = normalize(_WorldSpaceLightPos0.xyz);
                }
                else
                {
                    // 点光源の場合（必要に応じて処理を追加）
                    lightDir = normalize(_WorldSpaceLightPos0.xyz - i.worldPos);
                }

                // 法線と光源の方向の内積を計算
                float NdotL = max(dot(i.normal, lightDir), 0.0);

                // 3段階の色変化
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
                // 法線をワールド座標系に変換
                float3 normal = normalize(mul((float3x3)unity_ObjectToWorld, v.normal));
                // 頂点を拡張してアウトラインを作成
                float3 expandedVertex = v.vertex.xyz + normal * _OutlineWidth;
                // ワールド座標への変換
                o.worldPos = mul(unity_ObjectToWorld, float4(expandedVertex, 1.0)).xyz;
                // クリップ座標への変換
                o.pos = UnityObjectToClipPos(float4(expandedVertex, 1.0));
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 範囲チェック
                if (_UseRange == 1.0)
                {
                    float3 offset = i.worldPos - _PlayerPosition;
                    float distSq = offset.x * offset.x + offset.z * offset.z;
                    if (distSq > _Range * _Range)
                    {
                        discard; // 範囲外なら描画をスキップ
                    }
                }

                return _OutlineColor;
            }
            ENDCG
        }
    }
}
