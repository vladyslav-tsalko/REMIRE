Shader "Sprite/PanningDiffuse"
{
    Properties
    {
        _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
        _Speed("Speed", Float) = 0.0
        [Toggle(PIXELSNAP_ON)] PixelSnap("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
            "RenderPipeline" = "UniversalPipeline"
        }

        Cull Off
        ZWrite Off
        Blend One OneMinusSrcAlpha
        Lighting Off

        Pass
        {
            Name "SpriteLit"
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float _Speed;
                float4 _RendererColor;
                float4 _Flip;
                TEXTURE2D(_MainTex);
                SAMPLER(sampler_MainTex);
                TEXTURE2D(_AlphaTex);
                SAMPLER(sampler_AlphaTex);
                float _EnableExternalAlpha;
            CBUFFER_END

            // Helper function to flip sprite
            float4 UnityFlipSprite(float4 pos, float4 flip)
            {
                return float4(pos.x * flip.x, pos.y * flip.y, pos.z, pos.w);
            }

            // Pixel snap function
            float4 UnityPixelSnap(float4 pos)
            {
                float2 hpc = _ScreenParams.xy * 0.5;
                pos.xy = floor(pos.xy * hpc + 0.5) / hpc;
                return pos;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(UnityFlipSprite(input.positionOS, _Flip));
                #ifdef PIXELSNAP_ON
                output.positionCS = UnityPixelSnap(output.positionCS);
                #endif
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color * _Color * _RendererColor;
                return output;
            }

            half4 SampleSpriteTexture(float2 uv)
            {
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                if (_EnableExternalAlpha)
                {
                    color.a = SAMPLE_TEXTURE2D(_AlphaTex, sampler_AlphaTex, uv).r;
                }
                return color;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Apply panning effect
                input.uv.x -= _Time.y * _Speed;
                half4 c = SampleSpriteTexture(input.uv) * input.color;
                return half4(c.rgb * c.a, c.a);
            }
            ENDHLSL
        }
    }

    Fallback "Universal Render Pipeline/2D/Sprite-Lit-Default"
}