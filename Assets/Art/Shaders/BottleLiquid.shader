Shader "Custom/BottleShaderURP"
{
    Properties
    {
        _Color("Liquid Color", Color) = (1,0.5,0.5,1)
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _LiquidHeight("Liquid Height", Range(0,1)) = 0
        _Agitation("Agitation", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        // Backface Pass
        Pass
        {
            Name "BottleBackface"
            Cull Front

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float _LiquidHeight;
                float _Agitation;
                TEXTURE2D(_MainTex);
                SAMPLER(sampler_MainTex);
                float4 _MainTex_ST;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.screenPos = ComputeScreenPos(output.positionCS);
                output.worldPos = TransformObjectToWorld(input.positionOS.xyz);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                c.rgb *= 0.8; // Dim backface

                if (input.worldPos.y < _LiquidHeight)
                {
                    float2 screenUV = input.screenPos.xy / input.screenPos.w;
                    screenUV.x *= _ScreenParams.x / _ScreenParams.y;
                    half4 liquidColor = _Color;
                    c = lerp(liquidColor, c, c.a);
                }
                return c;
            }
            ENDHLSL
        }

        // Forward Lit Pass
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float _LiquidHeight;
                float _Agitation;
                TEXTURE2D(_MainTex);
                SAMPLER(sampler_MainTex);
                float4 _MainTex_ST;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.worldPos = TransformObjectToWorld(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);

                if (input.worldPos.y < _LiquidHeight)
                {
                    half4 liquidColor = _Color;
                    c = lerp(liquidColor, c, c.a);
                }

                // Basic lighting for URP
                float3 normalWS = normalize(input.normalWS);
                Light mainLight = GetMainLight();
                half3 lighting = mainLight.color * max(0, dot(normalWS, mainLight.direction));
                c.rgb *= lighting;

                return half4(c.rgb, c.a);
            }
            ENDHLSL
        }
    }
}