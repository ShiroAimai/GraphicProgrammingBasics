Shader "Basics/TextureTest"
{
    Properties
    {
        _MainTex ("Moss Texture", 2D) = "white" {}
        _RockTex("Rock Texture", 2D) = "white" {}
        _PatternTex("Pattern Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
           
            #include "UnityCG.cginc"

            struct Mesh
            {
                float4 vertex : POSITION;               
                float2 uv : TEXCOORD0;                
            };

            struct Interp
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 vertexWorld : TEXCOORD1;
            };

            sampler2D _MainTex;
            sampler2D _RockTex;
            sampler2D _PatternTex;

            Interp vert (Mesh v)
            {
                Interp o;
                o.vertexWorld = mul(unity_ObjectToWorld, v.vertex);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (Interp i) : SV_Target
            {
                
                // sample the texture
                float4 moss = tex2D(_MainTex, i.vertexWorld.xz);
                float4 rock = tex2D(_RockTex, i.vertexWorld.xz);
                float pattern = tex2D(_PatternTex, i.uv).x;
                
                float4 finalCol = lerp(rock, moss, pattern);

                return finalCol;
            }
            ENDCG
        }
    }
}
