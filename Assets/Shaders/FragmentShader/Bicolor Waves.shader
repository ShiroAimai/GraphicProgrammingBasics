Shader "Unlit/BicolorWaves"
{
    Properties
    {
        _ColorA("Color A", Color) = (1.0, 1.0, 1.0, 1.0)
        _ColorB("Color B", Color) = (1.0, 1.0, 1.0, 1.0)
        _ColorBegin("Color Begin", Range(0, 1)) = 0
        _ColorEnd("Color End", Range(0, 1)) = 1
    }
    SubShader
    {
        Tags { 
            "RenderType"="Opaque"
        }

        Pass
        {

            Blend One One //additive blending

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
           
            #include "UnityCG.cginc"

            #define TAU 6.28318530718

            float4 _ColorA;
            float4 _ColorB;

            float _ColorBegin;
            float _ColorEnd;

            struct VertexData
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;    
                float3 normal : NORMAL;
            };

            struct Interpolator
            {
                float2 uv : TEXCOORD0;                  
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD1;
            };

            Interpolator vert (VertexData v)
            {
                Interpolator o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;     
                o.normal = v.normal;
                return o;
            }

            float InverseLerp(float a, float b, float v)
            {
                return (v - a) / (b - a);
            }
            
            float4 frag (Interpolator i) : SV_Target
            {                                 
                float xOffset = cos(i.uv.y * TAU * 4 - _Time.y) * 0.05; //offset in  uv space
                float t = (i.uv.x + xOffset) * TAU * 4;
                t = cos(t) * 0.5 + 0.5; //remap Y range from [-1, 1] to [0, 1]

                t *= i.uv.y; //gives a vertical shade from bottom (0) to top (1)

                float colorInterp = saturate(InverseLerp(_ColorBegin, _ColorEnd, i.uv.y));
                float4 colorGradient = lerp(_ColorA, _ColorB, colorInterp);


                return t * colorGradient;
            }
            ENDCG
        }
    }
}
