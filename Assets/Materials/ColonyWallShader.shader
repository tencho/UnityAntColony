// コロニーの内壁を描画するシェーダー
Shader "Sprites/ColonyWall"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
        _WallTex ("Wall Texture", 2D) = "white" {}
        _Blur ("Blur Size", Float) = 0.25
        _AlphaThreshold ("Alpha Threshold", Range(0, 1)) = 0.3
        [Toggle]_ShowBackground ("Show Background", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f {
                float4 vertex : SV_POSITION;
                float2 uvColony : TEXCOORD0;
                float2 uvWall : TEXCOORD1;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            
            sampler2D _WallTex;
            float4 _WallTex_ST;
            
            float4 _Color;
            float4 _RendererColor;
            float _Blur;
            float _AlphaThreshold;
            float _ShowBackground;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uvColony = TRANSFORM_TEX(v.uv, _MainTex);
                o.uvWall = TRANSFORM_TEX(v.uv, _WallTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 掘られているピクセルをぼかして閾値以下の領域を内壁にする
                fixed4 col = fixed4(0, 0, 0, 0);
                for (int ix = -5; ix <= 5; ix++)
                {
                    for (int iy = -5; iy <= 5; iy++)
                    {
                        col += tex2D(_MainTex, i.uvColony + fixed2(_MainTex_TexelSize.x * ix * _Blur, _MainTex_TexelSize.y * iy * _Blur));
                    }
                }
                col /= 121;
                if (col.x >= _AlphaThreshold)
                {
                    // 内壁の閾値より内側をグラデーションにして影っぽくする
                    half shadow = saturate((col.x - _AlphaThreshold) / (1 - _AlphaThreshold));
                    fixed4 shadowColor = fixed4(shadow, shadow, shadow, 1);
                    return tex2D(_WallTex, i.uvWall) * _Color * shadowColor;
                }
                else
                {
                    return tex2D(_WallTex, i.uvWall) * _ShowBackground;
                }
            }
            ENDCG
        }
    }
}
