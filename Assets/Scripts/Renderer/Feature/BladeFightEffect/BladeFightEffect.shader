Shader "Hidden/BladeFightEffect"
{
    Properties
    {
        _MainTex ("MainTex", 2D) = "white" {}
        _Radius("Radius", Float) = 0.3
        _CenterCount("CenterCount", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _Centers[5]; // xy=中心UV坐标，zw未用

            int _CenterCount;
            float _Radius;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                return o;
            }

            // 灰度计算函数
            float3 ToGray(float3 col)
            {
                float lum = dot(col, float3(0.299, 0.587, 0.114));
                return float3(lum, lum, lum);
            }

            float4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);

                float minDist = 1000;
                for (int idx = 0; idx < _CenterCount; idx++)
                {
                    float2 center = _Centers[idx].xy;
                    float dist = distance(uv, center);
                    if (dist < minDist)
                        minDist = dist;
                }

                // 根据距离决定饱和度 (距离0->radius: 0->1)
                float sat = saturate(minDist / _Radius);

                // 饱和度线性插值：中心灰度，边缘正常
                float3 grayCol = ToGray(col.rgb);
                float3 finalCol = lerp(grayCol, col.rgb, sat);

                return float4(finalCol, col.a);
            }
            ENDHLSL
        }
    }
}
