Shader "Lights/Lighting_RTR"
{
    Properties
    {
       _ColorCool("Color Cool", Color) = (0, 0, 0.55, 1)
       _ColorWarm("Color Warm", Color) = (0.3, 0.3, 0, 1)
       _ColorHighlight("Highlight", Color) = (2, 2, 2, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            float3 _ColorCool, _ColorWarm, _ColorHighlight;
            
             struct MeshData
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct Interpolators
            {
                float2 uv : TEXCOORD0;
                float3 normal : TEXCOORD1;
                float3 wPos : TEXCOORD2;
                float4 vertex : SV_POSITION;
            };                     

            Interpolators vert(MeshData v)
            {
                Interpolators o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;//TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.wPos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            float3 FunctionUnlit(float3 normal)
            {
                return 0.5 * _ColorCool;
            }

            float3 FunctionLit(float3 n, float3 l, float3 v)
            {
                float3 reflection = reflect(-l, n); //light reflection
                float3 specularFactor = saturate(100.0 * dot(v, reflection) - 97.0);
                return lerp(_ColorWarm, _ColorHighlight, specularFactor);
            }

            float4 frag(Interpolators i) : SV_Target
            {
                //diffuse lighting
                float3 n = normalize(i.normal); //out of surface //normalize in order to avoid artefactor from linear interpolation
                float3 l = _WorldSpaceLightPos0.xyz; //from surface to light source
                float3 diffuse = saturate(dot(n, l)) * _LightColor0.xyz;
                float3 view = normalize(_WorldSpaceCameraPos - i.wPos); //from surface to camera

                float4 OutColor = float4(FunctionUnlit(n), 1.0);                
                OutColor.xyz += diffuse * FunctionLit(n, l, view);

                return OutColor;
            }
            ENDCG
        }
    }
}
