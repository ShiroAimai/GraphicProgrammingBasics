#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "AutoLight.cginc"

struct MeshData
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
    float3 normal : NORMAL;
    float4 tangent : TANGENT; //xyz tangent direction, w tangent sign
};

struct Interpolators
{
    float2 uv : TEXCOORD0;
    float3 normal : TEXCOORD1;
    float3 tangent : TEXCOORD2;
    float3 bitangent : TEXCOORD3;
    float3 wPos : TEXCOORD4;
    float4 vertex : SV_POSITION;
    LIGHTING_COORDS(5, 6)
};

sampler2D _RockAlbedo;
float4 _RockAlbedo_ST;
sampler2D _RockNormals;
float4 _RockNormals_ST;
float _Gloss;
float4 _SurfaceColor;

Interpolators vert(MeshData v)
{
    Interpolators o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.uv = TRANSFORM_TEX(v.uv, _RockAlbedo);
    o.normal = UnityObjectToWorldNormal(v.normal);
    o.tangent = UnityObjectToWorldDir(v.tangent.xyz);
    o.bitangent = normalize(cross(o.normal, o.tangent));
    o.bitangent *= (v.tangent.w * unity_WorldTransformParams.w); //correct if we have flipped UV
    o.wPos = mul(unity_ObjectToWorld, v.vertex);
    TRANSFER_VERTEX_TO_FRAGMENT(o);
    return o;
}

float4 frag(Interpolators i) : SV_Target
{
    float3 rock = tex2D(_RockAlbedo, i.uv).rgb;

    float3 surfaceColor = rock * _SurfaceColor.rgb;

    float3 tangentSpaceNormal = UnpackNormal(tex2D(_RockNormals, i.uv)); //from -1 to +1
   
    float3x3 mtxTangToWorld = {
        i.tangent.x, i.bitangent.x, i.normal.x,
        i.tangent.y, i.bitangent.y, i.normal.y,
        i.tangent.z, i.bitangent.z, i.normal.z
    };

    float3 n = mul(mtxTangToWorld, tangentSpaceNormal);
    //diffuse lighting
    //float3 n = normalize(i.normal); //out of surface //normalize in order to avoid artefactor from linear interpolation
    float3 l = normalize(UnityWorldSpaceLightDir(i.wPos));
    
    float attenuation = LIGHT_ATTENUATION(i);
    float3 lambert = saturate(dot(n, l));

    float3 diffuseLight = (lambert * attenuation) *_LightColor0.xyz;

    //specular
    float3 view = normalize(_WorldSpaceCameraPos - i.wPos); //from surface to camera
    float3 halfViewLight = normalize(l + view);
    float specular = saturate(dot(halfViewLight, n)) * (lambert > 0); // blinn-phong

    //adding glossyness
    float GlossExp = exp2(_Gloss * 11) + 2;
    specular = pow(specular, GlossExp) * _Gloss * attenuation; //specular exponent
    
    float3 specularLight = specular * _LightColor0.xyz;

    return float4(diffuseLight * surfaceColor + specularLight, 1.0);
}