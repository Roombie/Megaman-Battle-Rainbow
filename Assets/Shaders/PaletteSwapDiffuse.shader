Shader "Custom/PaletteSwapDiffuse"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "white" {}
        _Palette("Palette Texture", 2D) = "white" {}
    }
        SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Lighting On
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _Palette;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float4 originalColor = tex2D(_MainTex, i.uv);

                // Find the corresponding color in the palette
                float2 paletteUV = float2(originalColor.r, 0.5);
                float4 newColor = tex2D(_Palette, paletteUV);

                // Return the new color
                return float4(newColor.rgb, originalColor.a);
            }
            ENDCG
        }
    }
        FallBack "Sprites/Default"
}
