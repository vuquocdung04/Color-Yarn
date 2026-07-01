Shader "Custom/YarnDissolve"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _BumpMap ("Normal Map", 2D) = "bump" {}
        _BumpScale ("Normal Scale", Float) = 1
        _NoiseMap ("Noise Texture", 2D) = "gray" {}
        _NoiseScale ("Noise Scale", Float) = 8
        _DissolveAmount ("Dissolve Amount", Range(0,1)) = 0
        _NoiseStrength ("Noise Jitter", Range(0,1)) = 0.22
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 tangentOS  : TANGENT;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float  vertY       : TEXCOORD1;
                float3 normalWS    : TEXCOORD2;
                float3 tangentWS   : TEXCOORD3;
                float3 bitangentWS : TEXCOORD4;
                float3 positionWS  : TEXCOORD5;
            };

            TEXTURE2D(_BumpMap);   SAMPLER(sampler_BumpMap);
            TEXTURE2D(_NoiseMap);  SAMPLER(sampler_NoiseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _BumpMap_ST;
                float  _BumpScale;
                float  _NoiseScale;
                float  _DissolveAmount;
                float  _NoiseStrength;
            CBUFFER_END

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs pos = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs   nrm = GetVertexNormalInputs(IN.normalOS, IN.tangentOS);
                OUT.positionHCS = pos.positionCS;
                OUT.positionWS  = pos.positionWS;
                OUT.normalWS    = nrm.normalWS;
                OUT.tangentWS   = nrm.tangentWS;
                OUT.bitangentWS = nrm.bitangentWS;
                OUT.uv = TRANSFORM_TEX(IN.uv, _BumpMap);
                OUT.vertY = IN.positionOS.y + 0.5;
                return OUT;
            }

            half4 frag (Varyings IN, FRONT_FACE_TYPE facing : FRONT_FACE_SEMANTIC) : SV_Target
            {
                half4 baseCol = _BaseColor;

                float noise = SAMPLE_TEXTURE2D(_NoiseMap, sampler_NoiseMap, IN.uv * _NoiseScale).r;
                float directional = (1.0 - IN.vertY);
                float threshold = directional * (1.0 - _NoiseStrength) + noise * _NoiseStrength;
                clip(threshold - _DissolveAmount);

                float3 normalTS = UnpackNormalScale(
                    SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, IN.uv), _BumpScale);

                float3x3 tbn = float3x3(
                    normalize(IN.tangentWS),
                    normalize(IN.bitangentWS),
                    normalize(IN.normalWS));

                float3 n = normalize(mul(normalTS, tbn));
                n *= IS_FRONT_VFACE(facing, 1.0, -1.0);

                float4 shadowCoord = TransformWorldToShadowCoord(IN.positionWS);
                Light mainLight = GetMainLight(shadowCoord);

                float ndotl = saturate(dot(n, mainLight.direction));
                float shadow = mainLight.shadowAttenuation;
                float3 lit = mainLight.color * (ndotl * shadow);

                float3 ambient = SampleSH(n);

                float3 color = baseCol.rgb * (lit + ambient);
                return half4(color, 1.0);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            ZWrite On
            ZTest LEqual
            Cull Off

            HLSLPROGRAM
            #pragma vertex vertShadow
            #pragma fragment fragShadow

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            TEXTURE2D(_NoiseMap);  SAMPLER(sampler_NoiseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _BumpMap_ST;
                float  _BumpScale;
                float  _NoiseScale;
                float  _DissolveAmount;
                float  _NoiseStrength;
            CBUFFER_END

            float3 _LightDirection;

            struct AttributesS { float4 positionOS : POSITION; float3 normalOS : NORMAL; float2 uv : TEXCOORD0; };
            struct VaryingsS   { float4 positionHCS : SV_POSITION; float2 uv : TEXCOORD0; float vertY : TEXCOORD1; };

            VaryingsS vertShadow (AttributesS IN)
            {
                VaryingsS OUT;
                float3 posWS = TransformObjectToWorld(IN.positionOS.xyz);
                float3 nrmWS = TransformObjectToWorldNormal(IN.normalOS);
                float4 hclip = TransformWorldToHClip(ApplyShadowBias(posWS, nrmWS, _LightDirection));
                #if UNITY_REVERSED_Z
                    hclip.z = min(hclip.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    hclip.z = max(hclip.z, UNITY_NEAR_CLIP_VALUE);
                #endif
                OUT.positionHCS = hclip;
                OUT.uv = IN.uv;
                OUT.vertY = IN.positionOS.y + 0.5;
                return OUT;
            }

            half4 fragShadow (VaryingsS IN) : SV_Target
            {
                float noise = SAMPLE_TEXTURE2D(_NoiseMap, sampler_NoiseMap, IN.uv * _NoiseScale).r;
                float directional = (1.0 - IN.vertY);
                float threshold = directional * (1.0 - _NoiseStrength) + noise * _NoiseStrength;
                clip(threshold - _DissolveAmount);
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode"="DepthOnly" }
            ZWrite On
            ColorMask 0
            Cull Off

            HLSLPROGRAM
            #pragma vertex vertDepth
            #pragma fragment fragDepth

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_NoiseMap);  SAMPLER(sampler_NoiseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _BumpMap_ST;
                float  _BumpScale;
                float  _NoiseScale;
                float  _DissolveAmount;
                float  _NoiseStrength;
            CBUFFER_END

            struct AttributesD { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct VaryingsD   { float4 positionHCS : SV_POSITION; float2 uv : TEXCOORD0; float vertY : TEXCOORD1; };

            VaryingsD vertDepth (AttributesD IN)
            {
                VaryingsD OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.vertY = IN.positionOS.y + 0.5;
                return OUT;
            }

            half4 fragDepth (VaryingsD IN) : SV_Target
            {
                float noise = SAMPLE_TEXTURE2D(_NoiseMap, sampler_NoiseMap, IN.uv * _NoiseScale).r;
                float directional = (1.0 - IN.vertY);
                float threshold = directional * (1.0 - _NoiseStrength) + noise * _NoiseStrength;
                clip(threshold - _DissolveAmount);
                return 0;
            }
            ENDHLSL
        }
    }
}