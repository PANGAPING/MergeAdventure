Shader "UI/EmptyGlow"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}

        _FillColor ("Fill Color (Center)", Color) = (1,1,1,0.3)
        _FillAlphaScale ("Fill Alpha Scale", Range(0,2)) = 1

        _GlowColor ("Glow Color", Color) = (0.6,0,1,1)
        _GlowSize ("Glow Size", Range(0,20)) = 5
        _GlowStrength ("Glow Strength", Range(0,5)) = 1

        _InnerThreshold ("Inner Alpha Threshold", Range(0,1)) = 0.4
        _InnerFeather   ("Inner Feather", Range(0.001,0.5)) = 0.1

        _OutlineThreshold ("Outline Threshold", Range(0,1)) = 0.1
        _OutlineFeather   ("Outline Feather", Range(0.001,0.5)) = 0.1
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

            float4 _FillColor;
            float _FillAlphaScale;

            float4 _GlowColor;
            float _GlowSize;
            float _GlowStrength;

            float _InnerThreshold;
            float _InnerFeather;

            float _OutlineThreshold;
            float _OutlineFeather;

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

            float4 frag (v2f i) : SV_Target
            {
                float4 tex = tex2D(_MainTex, i.uv);
                float  a   = tex.a;

                //---------------------------------
                // 1）主体区域遮罩：让中间更“实”，边缘更干净
                //---------------------------------
                // 内部遮罩：a 大于阈值才认为是“主体”
                float innerMask = saturate( (a - _InnerThreshold) / max(_InnerFeather, 0.0001) );

                float4 fillCol = _FillColor;
                // fillAlpha = 贴图 alpha × 内部遮罩 × 可调强度
                fillCol.a *= a * innerMask * _FillAlphaScale;

                //---------------------------------
                // 2）做一次简单的 alpha 模糊，拿到“扩散后的范围”
                //---------------------------------
                float blurA = 0;
                int sampleCount = 0;

                // 小核采样，GlowSize 控制范围
                [unroll]
                for (int x = -2; x <= 2; x++)
                {
                    [unroll]
                    for (int y = -2; y <= 2; y++)
                    {
                        float2 offset = float2(x, y) / 100.0 * _GlowSize;
                        float sa = tex2D(_MainTex, i.uv + offset).a;
                        blurA += sa;
                        sampleCount++;
                    }
                }

                blurA /= sampleCount;  // 归一化

                //---------------------------------
                // 3）只要“模糊范围 - 主体范围” → 得到边缘区（描边）
                //---------------------------------
                float rawOutline = saturate(blurA - innerMask);   // 只保留超出主体的部分

                // 再做一次阈值 + feather，让描边可以调软硬
                float outlineMask = saturate(
                    (rawOutline - _OutlineThreshold) / max(_OutlineFeather, 0.0001)
                );

                //---------------------------------
                // 4）描边颜色（真正的“外发光”部分）
                //---------------------------------
                float4 glowCol = _GlowColor;
                glowCol.a *= outlineMask * _GlowStrength;

                //---------------------------------
                // 5）中心 + 外发光 合并
                //---------------------------------
                float4 finalCol = fillCol + glowCol;

                // 避免 RGB 有值但 alpha 为 0
                finalCol.a = saturate(finalCol.a);

                return finalCol;
            }
            ENDCG
        }
    }
}
