Shader "Lights/PhongLightsAndSpecularHighlight"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Gloss("Gloss", Float) = 1.0
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

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Gloss;

            Interpolators vert (MeshData v)
            {
                Interpolators o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.wPos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            float4 frag (Interpolators i) : SV_Target
            {
                //diffuse lighting
                float3 n = normalize(i.normal); //out of surface //normalize in order to avoid artefactor from linear interpolation
                float3 l = _WorldSpaceLightPos0.xyz; //from surface to light source
                float3 diffuse = saturate(dot(n, l)) * _LightColor0.xyz;

                //specular
                float3 view = normalize(_WorldSpaceCameraPos - i.wPos); //from surface to camera
                float3 reflection = reflect(-l, n ); //light reflection
                float3 specular = saturate(dot(view, reflection));

                //adding glossyness
                specular = pow(specular, _Gloss); //specular exponent

                return float4(specular, 1.0);
            }
            ENDCG
        }
    }
}
