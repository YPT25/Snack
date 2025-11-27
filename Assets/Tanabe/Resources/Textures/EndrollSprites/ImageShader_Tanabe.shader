Shader "Custom/ImageShader_Tanabe"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Speed("Color Shift Speed", Range(0.0, 5.0)) = 1.0
        _Saturation("Saturation", Range(0.0, 2.0)) = 1.0
        _Value("Value", Range(0.0, 2.0)) = 1.0
    }

        SubShader
        {
            Tags {
                "Queue" = "Transparent"
                "IgnoreProjector" = "True"
                "RenderType" = "Transparent"
                "CanvasRenderer" = "True"
            }
            Cull Off
            ZWrite Off
            Lighting Off
            Blend SrcAlpha OneMinusSrcAlpha

            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"

                sampler2D _MainTex;
                float4 _MainTex_ST;
                float _Speed;
                float _Saturation;
                float _Value;

                struct appdata {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                    float4 color : COLOR;
                };

                struct v2f {
                    float4 pos : SV_POSITION;
                    float2 uv : TEXCOORD0;
                    float4 color : COLOR;
                };

                v2f vert(appdata v)
                {
                    v2f o;
                    o.pos = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                    o.color = v.color;
                    return o;
                }

                // Hue shift function (RGB Å® HSV Å® hue add Å® RGB)
                float3 HueShift(float3 color, float shift)
                {
                    // RGB Å® HSV
                    float maxc = max(color.r, max(color.g, color.b));
                    float minc = min(color.r, min(color.g, color.b));
                    float delta = maxc - minc;

                    float h = 0.0;
                    if (delta != 0)
                    {
                        if (maxc == color.r)
                            h = (color.g - color.b) / delta;
                        else if (maxc == color.g)
                            h = 2.0 + (color.b - color.r) / delta;
                        else
                            h = 4.0 + (color.r - color.g) / delta;

                        h /= 6.0;
                        if (h < 0.0) h += 1.0;
                    }

                    float s = (maxc == 0.0) ? 0.0 : (delta / maxc);
                    float v = maxc;

                    // Hue âÒì]
                    h = frac(h + shift);

                    // HSV Å® RGB
                    float3 rgb;
                    float i = floor(h * 6.0);
                    float f = h * 6.0 - i;
                    float p = v * (1.0 - s);
                    float q = v * (1.0 - f * s);
                    float t = v * (1.0 - (1.0 - f) * s);

                    if (i == 0) rgb = float3(v, t, p);
                    else if (i == 1) rgb = float3(q, v, p);
                    else if (i == 2) rgb = float3(p, v, t);
                    else if (i == 3) rgb = float3(p, q, v);
                    else if (i == 4) rgb = float3(t, p, v);
                    else rgb = float3(v, p, q);

                    return rgb;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    fixed4 col = tex2D(_MainTex, i.uv) * i.color;

                // hue shift amount
                float shift = _Time.y * _Speed * 0.1;

                float3 shifted = HueShift(col.rgb, shift);

                // saturation & value
                shifted *= _Value;
                shifted = lerp(float3(0.5,0.5,0.5), shifted, _Saturation);

                return float4(shifted, col.a);
            }
            ENDCG
        }
        }
}