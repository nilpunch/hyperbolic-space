using UnityEngine;

public static class ShaderProperties
{
    public static readonly int MainTex = Shader.PropertyToID("_MainTex");
    public static readonly int Color = Shader.PropertyToID("_Color");
    public static readonly int DontAffectByGlobalOffset = Shader.PropertyToID("_DontAffectByGlobalOffset");

    public static readonly int GlobalOffset = Shader.PropertyToID("_GlobalOffset");
    public static readonly int GlobalCurvature = Shader.PropertyToID("_GlobalCurvature");
    public static readonly int GlobalKleinValue = Shader.PropertyToID("_GlobalKleinValue");
    
    public static readonly int BeltramiKleinFactor =  Shader.PropertyToID("_GlobalBeltramiKlein");
}