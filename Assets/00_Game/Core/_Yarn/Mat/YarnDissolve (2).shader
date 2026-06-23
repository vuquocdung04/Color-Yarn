Shader "Custom/YarnDissolve"
{
    Properties
    {
        _BaseMap ("Base Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)

        _NoiseMap ("Noise Texture", 2D) = "gray" {}
        _NoiseScale ("Noise Scale", Float) = 8

        _DissolveAmount ("Dissolve Amount", Range(0,1)) = 0
        _NoiseStrength ("Noise Jitter", Range(0,1)) = 0.22
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        LOD 100

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            Cull Off   // render ca 2 mat (front + back)

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                // localY normalized 0 (bottom) -> 1 (top) of the object's bounds,
                // ta se tinh theo object-space y de biet huong doc
                float  vertY       : TEXCOORD1;
            };

            TEXTURE2D(_BaseMap);   SAMPLER(sampler_BaseMap);
            TEXTURE2D(_NoiseMap);  SAMPLER(sampler_NoiseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float  _NoiseScale;
                float  _DissolveAmount;
                float  _NoiseStrength;
            CBUFFER_END

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);

                // dung object-space Y de xac dinh huong doc cua vat the
                // map tu [-0.5, 0.5] (unit cube/sphere) -> [0,1]; chinh neu mesh khac
                OUT.vertY = IN.positionOS.y + 0.5;
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                half4 baseCol = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;

                // noise de mep tan tu nhien (khong phang li)
                float noise = SAMPLE_TEXTURE2D(_NoiseMap, sampler_NoiseMap, IN.uv * _NoiseScale).r;

                // HUONG: top-to-duoi. vertY=1 o dinh, =0 o day.
                // muon tan tu TREN xuong => dinh phai bi xoa truoc.
                // gradient cao (gan 1) = bi tan som => lay (1 - vertY)
                float directional = (1.0 - IN.vertY);

                // tron huong + chut noise cho mep rang cua huu co
                float threshold = directional * (1.0 - _NoiseStrength) + noise * _NoiseStrength;

                // cut: pixel co threshold < DissolveAmount thi bi xoa
                float diff = threshold - _DissolveAmount;
                clip(diff);   // diff < 0 => discard

                return half4(baseCol.rgb, 1.0);
            }
            ENDHLSL
        }
    }
}
