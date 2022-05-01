Shader "Unlit/VertexOffset"
{
    Properties
    {
        _ColorA("Color A", Color) = (1.0, 1.0, 1.0, 1.0)
        _ColorB("Color B", Color) = (1.0, 1.0, 1.0, 1.0)
        _ColorBegin("Color Begin", Range(0, 1)) = 0
        _ColorEnd("Color End", Range(0, 1)) = 1

        _WaveAmp("Wave Amplitude", Range(0, 0.2)) = 0.1
    }
    SubShader
    {
        Tags { 
            "RenderType"="Opaque"
        }

        Pass
        {           

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
           
            #include "UnityCG.cginc"

            #define TAU 6.28318530718

            float4 _ColorA;
            float4 _ColorB;

            float _ColorBegin;
            float _ColorEnd;

            float _WaveAmp;

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
            
            float GetRadDistance(float2 uv)
            {
                float2 uvCentered = uv * 2 - 1;
                return length(uvCentered);
            }

            Interpolator vert (VertexData v)
            {
                Interpolator o;                                        

                float wave = cos((GetRadDistance(v.uv) - _Time.y * 0.1) * TAU * 5);

                v.vertex.y = wave * _WaveAmp;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;     
                o.normal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            float InverseLerp(float a, float b, float v)
            {
                return (v - a) / (b - a);
            }
            
            float4 frag (Interpolator i) : SV_Target
            {          
               
                float radDistance = GetRadDistance(i.uv);
                float wave = cos((radDistance - _Time.y * 0.1) * TAU * 5);
                wave = wave * 0.5 + 0.5; //remap range into [0, 1]
                wave *= (1 - radDistance);
                float4 col = lerp(_ColorA, _ColorB, wave);
                return col;
            }
            ENDCG
        }
    }
}
