using System;
using UnityEngine;

public class WorldSettings : MonoBehaviour
{
    [SerializeField] private int _quadsPerVertex = 5;

    private void Awake()
    {
        HyperMath.SetTileType(_quadsPerVertex);
        
        Shader.SetGlobalFloat(ShaderProperties.GlobalCurvature, HyperMath.Curvature);
        Shader.SetGlobalFloat(ShaderProperties.GlobalKleinValue, HyperMath.KleinValue);
    }
    
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) ;
        //Debug.Break();
    }

    private void OnValidate()
    {
        HyperMath.SetTileType(_quadsPerVertex);
        
        Shader.SetGlobalFloat(ShaderProperties.GlobalCurvature, HyperMath.Curvature);
        Shader.SetGlobalFloat(ShaderProperties.GlobalKleinValue, HyperMath.KleinValue);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(Vector3.zero, 1f);
    }
    
    private void OnApplicationQuit()
    {
        Shader.SetGlobalVector(ShaderProperties.GlobalOffset, Vector4.zero);
    }
}