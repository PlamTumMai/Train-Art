Shader "Custom/TowerOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (0,1,0,1)
        _OutlineSize ("Outline Thickness", Range(0, 40)) = 0
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
        Blend SrcAlpha OneMinusSrcAlpha

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
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            fixed4 _OutlineColor;
            float _OutlineSize;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.uv) * i.color;

                if (_OutlineSize > 0.01 && c.a < 0.1)
                {
                    float2 offset = _MainTex_TexelSize.xy * _OutlineSize;
                    float alpha = 0;

                    alpha = max(alpha, tex2D(_MainTex, i.uv + float2(offset.x, 0)).a);
                    alpha = max(alpha, tex2D(_MainTex, i.uv - float2(offset.x, 0)).a);
                    alpha = max(alpha, tex2D(_MainTex, i.uv + float2(0, offset.y)).a);
                    alpha = max(alpha, tex2D(_MainTex, i.uv - float2(0, offset.y)).a);
                    alpha = max(alpha, tex2D(_MainTex, i.uv + float2(offset.x, offset.y) * 0.707).a);
                    alpha = max(alpha, tex2D(_MainTex, i.uv + float2(-offset.x, offset.y) * 0.707).a);
                    alpha = max(alpha, tex2D(_MainTex, i.uv + float2(offset.x, -offset.y) * 0.707).a);
                    alpha = max(alpha, tex2D(_MainTex, i.uv + float2(-offset.x, -offset.y) * 0.707).a);

                    if (alpha > 0.1)
                        return fixed4(_OutlineColor.rgb, _OutlineColor.a * alpha);
                }

                return c;
            }
            ENDCG
        }
    }
}