Shader "Assignment/Healthbar"
{
    Properties
    {
        _FullHealth("Full", Color) = (1.0, 1.0, 1.0, 1.0)
        _FullHealthTreshold("Full Health Threhold", Float) = 0.8
        _LowHealth("Low", Color) = (1.0, 1.0, 1.0, 1.0)
        _LowHealthTreshold("Low Health Threhold", Float) = 0.2
        _MissingHealth("Missing Health", Range(0, 1)) = 0
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

            float4 _FullHealth;
            float _FullHealthTreshold;
            float4 _LowHealth;
            float _LowHealthTreshold;
            float _MissingHealth;

            struct Mesh
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Interpolator
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };


            Interpolator vert (Mesh v)
            {
                Interpolator o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag (Interpolator i) : SV_Target
            {                                
                float missingHealthNormalized = saturate(_MissingHealth);                
                float missingHealthInverted = 1 - missingHealthNormalized;

                float fullHealthTresholdBonus = saturate((missingHealthInverted - _FullHealthTreshold));
                float lowHealthTresholdMalus = saturate((_LowHealthTreshold - missingHealthInverted));
                
                if(missingHealthInverted <= 0) return _LowHealth;
                
                float4 col = _FullHealth;
                if (missingHealthInverted > _FullHealthTreshold)
                {
                    col = _FullHealth;
                }
                else if (missingHealthInverted < _LowHealthTreshold)
                {
                    col = _LowHealth;
                }
                else
                {
                    col = lerp(_FullHealth, _LowHealth, missingHealthNormalized);
                }               

                clip(missingHealthInverted - i.uv.x); //kill pixel in order to not render those pixel outside scope
                return col;
            }
            ENDCG
        }
    }
}
