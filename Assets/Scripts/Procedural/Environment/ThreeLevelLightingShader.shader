Shader "Custom/ThreeLevelLightingShader"
{
    Properties
    {
        _HighColor ("High Light Color", Color) = (1, 1, 1, 1) // 光が強い部分の色
        _MidColor ("Mid Light Color", Color) = (0.5, 0.5, 0.5, 1) // 中程度の光の色
        _LowColor ("Low Light Color", Color) = (0.2, 0.2, 0.2, 1) // 暗い部分の色
        _HighThreshold ("High Threshold", Float) = 0.7 // 高いライトの閾値
        _LowThreshold ("Low Threshold", Float) = 0.3 // 低いライトの閾値
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

        // シェーダーのサーフェス関数
        void surf(Input IN, inout SurfaceOutputStandard o)
        {
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

            // メタリックと滑らかさを反映
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
        }
        ENDCG
    }

    FallBack "Diffuse"
}
