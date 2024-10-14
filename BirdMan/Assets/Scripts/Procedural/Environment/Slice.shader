Shader "Custom/SliceShaderURP" {
    Properties {
        _BaseColor("Base Color", Color) = (1.0, 1.0, 1.0, 1.0)
    }
    SubShader {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
        Cull Off

        Pass {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes {
                float4 positionOS : POSITION;
            };

            struct Varyings {
                float4 positionHCS : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            float4 _BaseColor;

            Varyings vert(Attributes IN) {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
                OUT.worldPos = TransformObjectToWorld(IN.positionOS).xyz;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target {
                // Apply slicing effect based on the Y position of the world coordinate
                if (frac(IN.worldPos.y * 15) - 0.5 < 0) {
                    discard; // Discard the fragment if it's below the slicing threshold
                }

                // Set base color for the object
                return half4(_BaseColor.rgb, 1.0);
            }

            ENDHLSL
        }
    }
    FallBack "Hidden/InternalErrorShader"
}
