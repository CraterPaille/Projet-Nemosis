Shader "Custom/ColorblindCorrection"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ColorblindMode ("Colorblind Mode", Int) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            int _ColorblindMode;

            fixed3 ApplyProtanopia(fixed3 c)
            {
                // Matrices inspirées de l’article
                // https://www.alanzucconi.com/2015/12/16/color-blindness/
                fixed3x3 m = fixed3x3(
                    0.567, 0.433, 0.000,
                    0.558, 0.442, 0.000,
                    0.000, 0.242, 0.758
                );
                return mul(m, c);
            }

            fixed3 ApplyDeuteranopia(fixed3 c)
            {
                fixed3x3 m = fixed3x3(
                    0.625, 0.375, 0.000,
                    0.700, 0.300, 0.000,
                    0.000, 0.300, 0.700
                );
                return mul(m, c);
            }

            fixed3 ApplyTritanopia(fixed3 c)
            {
                fixed3x3 m = fixed3x3(
                    0.950, 0.050, 0.000,
                    0.000, 0.433, 0.567,
                    0.000, 0.475, 0.525
                );
                return mul(m, c);
            }

            fixed4 frag(v2f_img i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed3 rgb = col.rgb;

                if (_ColorblindMode == 1)       // Protanopia
                    rgb = ApplyProtanopia(rgb);
                else if (_ColorblindMode == 2)  // Deuteranopia
                    rgb = ApplyDeuteranopia(rgb);
                else if (_ColorblindMode == 3)  // Tritanopia
                    rgb = ApplyTritanopia(rgb);
                // 0 = None -> rgb inchangé

                col.rgb = saturate(rgb);
                return col;
            }
            ENDCG
        }
    }
}