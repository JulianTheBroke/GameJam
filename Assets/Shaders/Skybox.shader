Shader "Custom/Skybox"
{
    Properties
    {
        _TopColor ("Top Color", Color) = (0.3, 0.5, 0.9, 1)
        _BottomColor ("Bottom Color", Color) = (0.9, 0.9, 0.85, 1)
        _Exponent ("Blend Exponent", Range(0.1, 10)) = 1.0
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _TopColor;
            fixed4 _BottomColor;
            float _Exponent;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 texcoord : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.vertex.xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 dir = normalize(i.texcoord);

                float t = saturate(dir.y * 0.5 + 0.5);
                t = pow(t, _Exponent);

                fixed3 col = lerp(_BottomColor.rgb, _TopColor.rgb, t);
                return fixed4(col, 1.0);
            }
            ENDCG
        }
    }

    Fallback Off
}
