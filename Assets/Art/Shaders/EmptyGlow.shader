Shader "UI/EmptyGlow"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}

        _FillColor ("Fill Color (Center)", Color) = (1,1,1,0.3)
        _FillAlphaScale ("Fill Alpha Scale", Range(0,2)) = 1

        // 仍然保留强度与宽度
        _GlowSize ("Glow Size (Inner Outline px)", Range(0,20)) = 5
        _GlowStrength ("Glow Strength", Range(0,5)) = 1

        _InnerThreshold ("Inner Alpha Threshold", Range(0,1)) = 0.4
        _InnerFeather   ("Inner Feather", Range(0.001,0.5)) = 0.1

        _OutlineThreshold ("Outline Threshold", Range(0,1)) = 0.1
        _OutlineFeather   ("Outline Feather", Range(0.001,0.5)) = 0.1

        // —— 静态彩虹参数 ——
        _RainbowScale ("Rainbow Scale", Range(0,30)) = 10
        _RainbowAngle ("Rainbow Angle (deg)", Range(0,360)) = 0
        _RainbowSaturation ("Rainbow Saturation", Range(0,1)) = 1
        _RainbowValue ("Rainbow Value", Range(0,1)) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize; // x=1/width, y=1/height

            float4 _FillColor;
            float _FillAlphaScale;

            float _GlowSize;
            float _GlowStrength;

            float _InnerThreshold;
            float _InnerFeather;

            float _OutlineThreshold;
            float _OutlineFeather;

            float _RainbowScale;
            float _RainbowAngle;
            float _RainbowSaturation;
            float _RainbowValue;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            // HSV -> RGB（轻量版）
            float3 HSVtoRGB(float3 c)
            {
                float4 K = float4(1.0, 2.0/3.0, 1.0/3.0, 3.0);
                float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
                return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 tex = tex2D(_MainTex, i.uv);
                float  a   = tex.a;

                // 0）完全透明处不画（保证不会在图外出现任何描边）
                if (a <= 0.0001)
                    return float4(0,0,0,0);

                // 1）主体区域遮罩：中间更实，边缘更干净
                float innerMask = saturate( (a - _InnerThreshold) / max(_InnerFeather, 0.0001) );

                float4 fillCol = _FillColor;
                fillCol.a *= a * innerMask * _FillAlphaScale;

                // 2）向内描边：内部像素里找邻域最小 alpha，越靠近边界差值越大
                float2 texel = _MainTex_TexelSize.xy;
                float2 d = texel * _GlowSize;

                float aN  = tex2D(_MainTex, i.uv + float2(0,  d.y)).a;
                float aS  = tex2D(_MainTex, i.uv + float2(0, -d.y)).a;
                float aE  = tex2D(_MainTex, i.uv + float2( d.x, 0)).a;
                float aW  = tex2D(_MainTex, i.uv + float2(-d.x, 0)).a;
                float aNE = tex2D(_MainTex, i.uv + float2( d.x,  d.y)).a;
                float aNW = tex2D(_MainTex, i.uv + float2(-d.x,  d.y)).a;
                float aSE = tex2D(_MainTex, i.uv + float2( d.x, -d.y)).a;
                float aSW = tex2D(_MainTex, i.uv + float2(-d.x, -d.y)).a;

                float minNeighbor = min(min(min(aN,aS), min(aE,aW)), min(min(aNE,aNW), min(aSE,aSW)));
                float rawInnerEdge = saturate(a - minNeighbor);

                float outlineMask = saturate(
                    (rawInnerEdge - _OutlineThreshold) / max(_OutlineFeather, 0.0001)
                );

                // 只在内部（alpha较可靠的区域）画，减少脏边
                outlineMask *= innerMask;

                // 3）静态彩虹：Hue 只跟位置走（不使用 _Time）
                float rad = _RainbowAngle * 0.01745329252;
                float2 dir = float2(cos(rad), sin(rad));

                // uv 以中心为原点，让角度/缩放更直观
                float2 u = (i.uv - 0.5);

                // 沿 dir 的投影做条纹彩虹；Scale 越大条纹越密
                float hue = frac(dot(u, dir) * _RainbowScale + 0.5);

                float3 rgb = HSVtoRGB(float3(hue, _RainbowSaturation, _RainbowValue));
                float4 glowCol = float4(rgb, 1.0);
                glowCol.a *= outlineMask * _GlowStrength;

                // 4）合并
                float4 finalCol = fillCol + glowCol;
                finalCol.a = saturate(finalCol.a);
                return finalCol;
            }
            ENDCG
        }
    }
}
