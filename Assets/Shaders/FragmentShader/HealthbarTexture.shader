Shader "Assignment/HealthbarTexture"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MaskTex("MaskTexture", 2D) = "white" {}
        _Health("Health", Range(0, 1)) = 1.0
        _ColorA("Color A", Color) = (1, 0, 0, 0)
        _ColorB("Color B", Color) = (0, 1, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }

        Pass {
            ZWrite Off
            // src * srcAlpha + dst * (1-srcAlpha)
            Blend SrcAlpha OneMinusSrcAlpha // Alpha blending

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct MeshData
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Interpolators
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _MaskTex;
            float _Health;
            float4 _ColorA, _ColorB;

            Interpolators vert (MeshData v)
            {
                Interpolators o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                
                return o;
            }

            float InverseLerp(float a, float b, float v)
            {
                return (v - a) / (b - a);
            }
            float4 frag (Interpolators i) : SV_Target
            {
                // sample the texture
                float HealthbarMask = _Health > i.uv.x;
                
                float2 healthUV = float2(_Health, i.uv.y);
                
                float4 maskCol = tex2D(_MaskTex, i.uv);
                float3 col = tex2D(_MainTex, healthUV) * maskCol.a;
    
                if (_Health < 0.2)
                {
                    float flash = cos(_Time.y * 5) * 0.15 + 0.15;
                    flash += 1;                    
                    col *= flash;
                }
               
                return float4(col, HealthbarMask * maskCol.a);
            }
            ENDCG
        }
    }
}
