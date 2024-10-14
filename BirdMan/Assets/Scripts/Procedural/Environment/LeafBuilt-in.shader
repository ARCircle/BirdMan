Shader "Custom/LeafShaderWithNoiseAndRange"
{
    Properties
    {
        _HighColor ("High Light Color", Color) = (1, 1, 1, 1) // 光が強い部分の色
        _MidColor ("Mid Light Color", Color) = (0.5, 0.5, 0.5, 1) // 中程度の光の色
        _LowColor ("Low Light Color", Color) = (0.2, 0.2, 0.2, 1) // 暗い部分の色
        _HighThreshold ("High Threshold", Range(0,1)) = 0.7 // 高いライトの閾値
        _LowThreshold ("Low Threshold", Range(0,1)) = 0.3 // 低いライトの閾値
        _Glossiness ("Smoothness", Range(0.0, 1.0)) = 0.5 // 滑らかさ
        _Metallic ("Metallic", Range(0.0, 1.0)) = 0.0 // メタリック度

        _NoiseScale ("Noise Scale", Float) = 0.1 // ノイズのスケール
        _NoiseStrength ("Noise Strength", Float) = 0.2 // ノイズの強さ
        _LeafPower ("Leaf Power", Float) = 1.0 // リーフの明るさ

        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1) // アウトラインの色
        _OutlineWidth ("Outline Width", Float) = 0.01 // アウトラインの幅

        _PlayerPosition ("Player Position", Vector) = (0, 0, 0) // プレイヤーの座標
        _Range ("Display Range", Float) = 10.0 // 表示範囲
        _UseRange ("Use Range Flag", Float) = 1.0 // 範囲制限のフラグ
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }

        // メインのPass（Surface Shader）
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert addshadow

        #include "UnityCG.cginc"

        struct Input
        {
            float2 uv_MainTex;
            float3 worldNormal;
            float3 worldPos;
        };

        // プロパティ
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

        // ノイズ生成関数
        float GenerateNoise(float3 pos)
        {
            return frac(sin(dot(pos, float3(12.9898, 78.233, 37.719))) * 43758.5453);
        }

        // 頂点シェーダー
        void vert(inout appdata_full v)
        {
            // ノイズによる頂点変形
            float noise = GenerateNoise(v.vertex.xyz * _NoiseScale);
            v.vertex.xyz += v.normal * noise * _NoiseStrength;
        }

        // サーフェイスシェーダー
        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            // 範囲チェック
            if (_UseRange == 1.0)
            {
                float3 offset = IN.worldPos - _PlayerPosition;
                float distSq = offset.x * offset.x + offset.z * offset.z;
                if (distSq > _Range * _Range)
                {
                    discard; // 範囲外ならピクセルを描画しない
                }
            }

            // 法線を正規化
            float3 normalWorld = normalize(IN.worldNormal);

            // メインライトの方向を取得
            float3 lightDir = normalize(UnityWorldSpaceLightDir(IN.worldPos));

            // 法線とライトの方向の内積（ライトの当たり具合）を計算
            float NdotL = saturate(dot(normalWorld, lightDir));

            // 3段階のライトの強度による色の設定
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

            // 明るさの適用
            o.Albedo *= _LeafPower;

            // メタリックと滑らかさを反映
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
        }
        ENDCG

        // アウトラインパス
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

            // ノイズ生成関数
            float GenerateNoise(float3 pos)
            {
                return frac(sin(dot(pos, float3(12.9898, 78.233, 37.719))) * 43758.5453);
            }

            void vert(in appdata v, out v2f o)
            {
                UNITY_INITIALIZE_OUTPUT(v2f, o);

                // ノイズによる頂点変形
                float noise = GenerateNoise(v.vertex.xyz * _NoiseScale);
                float3 displacedPosition = v.vertex.xyz + v.normal * noise * _NoiseStrength;

                // 法線をワールド空間に変換
                float3 normalWS = UnityObjectToWorldNormal(v.normal);

                // アウトラインのための頂点拡張
                displacedPosition += normalWS * _OutlineWidth;

                // クリップ空間への変換
                o.pos = UnityObjectToClipPos(float4(displacedPosition, 1.0));

                // ワールド座標を取得
                o.worldPos = mul(unity_ObjectToWorld, float4(displacedPosition, 1.0)).xyz;
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
    FallBack "Diffuse"
}
