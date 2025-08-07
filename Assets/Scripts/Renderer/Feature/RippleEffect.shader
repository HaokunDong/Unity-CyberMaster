Shader "Hidden/RippleEffect"
{
    Properties
    {
        _MainTex ("MainTex", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
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

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

            float4 _RippleCenters[5];   // xy: uv坐标, z: 启动时间, w: 强度
            float _RippleCount;
            float _TimeNow;

            // 新增参数
            float _MaxRadius = 1.2;        // 最大扩散半径（UV空间单位，1是满屏对角线的长度）
            float _RippleSpeed = 3.0;      // 扩散速度（单位UV/s）
            float _Duration = 0.7;         // 持续时间（秒）

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv = v.uv;
                return o;
            }

            // 计算单个涟漪扰动（根据当前时间，距离，强度，半径，持续时间）
            float ripple(float2 uv, float2 center, float age, float strength)
            {
                // 当前涟漪半径
                float currentRadius = age * _RippleSpeed;

                // 如果超过最大半径或持续时间，扰动为0
                if (age > _Duration || currentRadius > _MaxRadius)
                    return 0;

                float dist = distance(uv, center);

                // 计算距离和半径差，做一个宽度为0.05的波纹带
                float diff = dist - currentRadius;

                // 只有在波纹带附近才有扰动，波纹带宽度可以调
                float width = 0.05;

                if (abs(diff) > width)
                    return 0;

                // 计算波纹强度，使用余弦实现缓和波峰
                float rippleStrength = cos(diff / width * 3.14159) * strength;

                // 加一个衰减，越远衰减越强
                rippleStrength *= smoothstep(_MaxRadius, 0, dist);

                // 振幅乘以缩放
                float amplitude = 0.02; // 可调节整体振幅大小

                return rippleStrength * amplitude;
            }

            float2 distort(float2 uv)
            {
                float2 offset = float2(0, 0);
                for (int i = 0; i < _RippleCount; i++) {
                    float2 center = _RippleCenters[i].xy;
                    float startTime = _RippleCenters[i].z;
                    float strength = _RippleCenters[i].w;

                    float age = _TimeNow - startTime;

                    float r = ripple(uv, center, age, strength);

                    if (r != 0)
                    {
                        // 根据中心点方向偏移UV
                        offset += normalize(uv - center) * r;
                    }
                }
                return uv + offset;
            }

            float4 frag(v2f i) : SV_Target
            {
                float2 distortedUV = distort(i.uv);
                return tex2D(_MainTex, distortedUV);
            }

            ENDHLSL
        }
    }
    FallBack Off
}
