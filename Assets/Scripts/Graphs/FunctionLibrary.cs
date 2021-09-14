using UnityEngine;

using static UnityEngine.Mathf;
public static class FunctionLibrary
{

    public delegate Vector3 Function(float u, float v, float t);
    
    public enum Functions
    {
        Wave,
        MultiWave,
        //MultiWaveXZ,
        //MorphingWave,
        Ripple,
        Sphere,
        Torus
    }
    private static Function[] functions = { Wave, MultiWave,/*MultiWaveXZ, MorphingWave,*/ Ripple, Sphere, Torus};

    public static int FunctionCount => functions.Length;

    public static Function GetFunc(Functions name) => functions[(int)name];

    public static Functions GetNextFunctionName (Functions name) =>(int)name < functions.Length - 1 ? name + 1 : 0; 
    
    public static Functions GetRandomFunctionNameOtherThan (Functions name) {
        var choice = (Functions)Random.Range(1, functions.Length);
        return choice == name ? 0 : choice;
    }
    public static Vector3 Wave(float u, float v, float t)
    {
        Vector3 p;
        p.x = u;
        p.z = v;
        p.y = Sin(PI * (u + v + t));
        return p;
    }
    
    public static Vector3 MultiWave (float u,float v,  float t)
    {
        Vector3 p;
        p.x = u;
        p.z = v;
        p.y = Sin(PI * (u + t));
        p.y += Sin(2f * PI * (u + (v + t))) * 0.5f;
        p.y *= (2f / 3f);
        
        return p;
    }
    
    public static Vector3 MultiWaveXZ (float u,float v,  float t) {
        Vector3 p;
        p.x = u;
        p.z = v;
        p.y = Sin(PI * (u + t));
        p.y  += Sin(2f * PI * (u + (v + t))) * 0.5f;
        p.y  += Sin(PI * (u + v + 0.25f * t));
        p.y *= (1f / 2.5f);
        return p;
    }
    
    public static Vector3 MorphingWave (float u,float v,  float t)
    {
        Vector3 p;
        p.x = u;
        p.z = v;
        p.y = Sin(PI * (u + 0.5f * t));
        p.y += Sin(2f * PI * (u + t)) * 0.5f;
        p.y *= (2f / 3f);
        return p;
    }
    
    
    public static Vector3 Ripple (float u, float v, float t)
    {
        Vector3 p;
        p.x = u;
        p.z = v;
        float d = Sqrt(u * u + v * v);
        p.y = Sin(PI * (4f * d - t));
        p.y /= (1f + 10f * d);
        return p;
    }

    public static Vector3 Sphere(float u, float v, float t)
    {
        float r = 0.5f + 0.5f * Sin(PI * (v + t));
        float s = r * Cos(0.5f * PI * v);
        Vector3 p;
        p.x = Sin(PI * u) * s;
        p.y =  r * Sin(0.5f * PI * v);
        p.z = Cos(PI * u) * s;
        return p;
    }
    
    public static Vector3 Torus (float u, float v, float t) {
        float r1 = 0.7f + 0.1f * Sin(PI * (6f * u + 0.5f * t));
        //float r2 = 0.15f + 0.05f * Sin(PI *(8f * u + 4f * v + 2f * t));
        float r2 = 0.25f;
        float s = r1 + r2 * Cos(PI * v);
        Vector3 p;
        p.x = s * Sin(PI * u);
        p.y = r2 * Sin(PI * v);
        p.z = s * Cos(PI * u);
        return p;
    }

    public static Vector3 Morph(
        float u, float v, float t, Function from, Function to, float progress
    )
    {
        return Vector3.LerpUnclamped(
            from(u, v, t), to(u, v, t), SmoothStep(0f, 1f, progress)
        );
    }
}
