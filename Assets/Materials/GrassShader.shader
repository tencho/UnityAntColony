// 草を揺らすシェーダー
// 草メッシュは事前に揺れの影響度を頂点カラーで割り当てている
Shader "Unlit/GrassShader"
{
    Properties
    {
        _Strength ("Strength", Float) = 0.25
    }

    SubShader
    {
        Tags {
            "RenderType"="Opaque"
        }
        LOD 100
        
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

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            float _Strength;

            v2f vert (appdata v)
            {
                v2f o;
                // 頂点カラーのRGB強度に応じて左右に頂点を揺らす
                float4 vertex = v.vertex;
                vertex.x +=
                    v.color.x * sin(_Time * 30) * _Strength
                    + v.color.y * cos(_Time * 26) * _Strength
                    + v.color.z * cos(_Time * 37) * _Strength;
                o.vertex = UnityObjectToClipPos(vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return fixed4(0, 0, 0, 1);
            }
            ENDCG
        }
    }
}
