Shader "Custom/ToonOutlineWithHeightColorLoopAndShadowsAndRange"
{
    Properties
    {
        _Color1 ("Color 1", Color) = (0, 1, 0, 1)
        _Color2 ("Color 2", Color) = (1, 0, 0, 1)
        _Color3 ("Color 3", Color) = (1, 1, 0, 1)
        _Color4 ("Color 4", Color) = (0, 0, 1, 1)
        _Color5 ("Color 5", Color) = (0, 1, 1, 1)
        _Color6 ("Color 6", Color) = (1, 0, 1, 1)
        _Color7 ("Color 7", Color) = (0.5, 0.5, 0, 1)
        _Color8 ("Color 8", Color) = (0.5, 0, 0.5, 1)
        _Color9 ("Color 9", Color) = (0, 0.5, 0.5, 1)
        _HeightInterval1 ("Height Interval 1", Float) = 1.0
        _HeightBorder1 ("Height Border 1", Float) = 3.0
        _HeightInterval2 ("Height Interval 2", Float) = 1.0
        _HeightBorder2 ("Height Border 2", Float) = 1.0
        _HeightInterval3 ("Height Interval 3", Float) = 1.0
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineWidth ("Outline Width", Float) = 0.05
        _ShadowPower("Shadow Power", Range(0, 1)) = 0.8
        _PlayerPosition ("Player Position", Vector) = (0, 0, 0, 0)
        _Range ("Display Range", Float) = 10.0
        _UseRange ("Use Range Flag", Float) = 1.0
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

            // プロパティの宣言
            float4 _Color1, _Color2, _Color3;
            float4 _Color4, _Color5, _Color6;
            float4 _Color7, _Color8, _Color9;
            float _HeightInterval1, _HeightBorder1;
            float _HeightInterval2, _HeightBorder2;
            float _HeightInterval3;
            float _ShadowPower;
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
                float3 normal : TEXCOORD1;
                float4 shadowCoord : TEXCOORD2;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.normal = normalize(mul((float3x3)unity_ObjectToWorld, v.normal));

                // シャドウ座標の計算
                #ifdef USE_SHADOWS
                o.shadowCoord = ComputeShadowCoord(o.pos);
                #endif

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

                // 高さによる色の決定
                float yPos = i.worldPos.y;
                float3 baseColor;

                if (yPos >= _HeightBorder1)
                {
                    float modYPos = fmod(yPos, _HeightInterval1 * 3);
                    if (modYPos <= _HeightInterval1)
                    {
                        baseColor = _Color1.rgb;
                    }
                    else if (modYPos <= _HeightInterval1 * 2.0)
                    {
                        baseColor = _Color2.rgb;
                    }
                    else
                    {
                        baseColor = _Color3.rgb;
                    }
                }
                else if (yPos >= _HeightBorder2)
                {
                    float modYPos = fmod(yPos - _HeightBorder2, _HeightInterval2 * 3);
                    if (modYPos <= _HeightInterval2)
                    {
                        baseColor = _Color4.rgb;
                    }
                    else if (modYPos <= _HeightInterval2 * 2.0)
                    {
                        baseColor = _Color5.rgb;
                    }
                    else
                    {
                        baseColor = _Color6.rgb;
                    }
                }
                else
                {
                    float modYPos = fmod(yPos - _HeightBorder2, _HeightInterval3 * 3);
                    if (modYPos <= _HeightInterval3)
                    {
                        baseColor = _Color7.rgb;
                    }
                    else if (modYPos <= _HeightInterval3 * 2.0)
                    {
                        baseColor = _Color8.rgb;
                    }
                    else
                    {
                        baseColor = _Color9.rgb;
                    }
                }

                // ライティング計算
                float3 lightDir;
                float atten = 1.0;

                // メインライトの取得
                if (_WorldSpaceLightPos0.w == 0.0)
                {
                    // 平行光源
                    lightDir = normalize(_WorldSpaceLightPos0.xyz);
                }
                else
                {
                    // 点光源
                    lightDir = normalize(_WorldSpaceLightPos0.xyz - i.worldPos);
                    //float range = _LightRange0;
                    float range = 1;
                    float dist = length(_WorldSpaceLightPos0.xyz - i.worldPos);
                    atten = saturate(1.0 - dist / range);
                }

                float NdotL = max(dot(i.normal, lightDir), 0.0);

                // シャドウ計算
                float shadowAttenuation = 1.0;
                #ifdef USE_SHADOWS
                shadowAttenuation = UnitySampleShadowmap(i.shadowCoord);
                #endif

                // 影の強度を適用
               //baseColor *= lerp(1.0, shadowAttenuation, _ShadowPower) * NdotL * atten;

                return fixed4(baseColor, 1.0);
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

            float4 _OutlineColor;
            float _OutlineWidth;
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

            v2f vert(appdata v)
            {
                v2f o;
                float3 normalWorld = normalize(mul((float3x3)unity_ObjectToWorld, v.normal));
                float3 expandedVertex = v.vertex.xyz + (normalWorld * _OutlineWidth);
                o.worldPos = mul(unity_ObjectToWorld, float4(expandedVertex, 1.0)).xyz;
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
                        discard; // 範囲外ならアウトラインも描画しない
                    }
                }

                return _OutlineColor;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
