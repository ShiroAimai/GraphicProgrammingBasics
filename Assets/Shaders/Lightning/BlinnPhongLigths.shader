Shader "Lights/BlinnPhongLights"
{
    Properties
    {
        _RockAlbedo("Rock Albedo", 2D) = "white" {}
        [NoScaleOffset] _RockNormals("Rock Normals", 2D) = "bump" {}
        _Gloss("Gloss", Range(0, 1)) = 1.0
        _SurfaceColor("Surface Color", Color) = (1,1,1,1)
    }
        SubShader
        {
            Tags { "RenderType" = "Opaque" "Queue" = "Geometry"}

            Pass //base pass
            {
                Tags { "LightMode" = "ForwardBase"}
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #define IS_BASE_PASS;
                #include "../FGLight.cginc"

                ENDCG
            }

            Pass //add pass
            {
                Tags { "LightMode" = "ForwardAdd"}
                Blend One One //src * 1 + dst * 1 
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_fwdadd
                #include "../FGLight.cginc"

                ENDCG
            }
        }
}
