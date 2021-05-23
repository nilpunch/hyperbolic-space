using System;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(MeshRenderer))]
public class ShaderController : MonoBehaviour
{
    [SerializeField] private Texture _texture = null;
    [SerializeField] private Color _color = Color.white;
    [SerializeField] private bool _randomizeColor = false;

    [Space, SerializeField] private bool _dontAffectByGlobalOffset = false;

    private MeshRenderer _meshRenderer;

    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        MeshFilter _meshFilter = GetComponent<MeshFilter>();
        
        
        if (_randomizeColor)
            _color = Random.ColorHSV(0f, 1f, 0.2f, 0.2f, 0.85f, 0.85f);
        
        _meshRenderer.material.SetTexture(ShaderProperties.MainTex, _texture);
        _meshRenderer.material.SetColor(ShaderProperties.Color, _color);
        _meshRenderer.material.SetInt(ShaderProperties.DontAffectByGlobalOffset, _dontAffectByGlobalOffset ? 1 : 0);

        _meshFilter.mesh.bounds = new Bounds(new Vector3(0, 0, 0), new Vector3(10000000, 10000000, 10000000));
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && _randomizeColor)
        {
            _color = Random.ColorHSV(0f, 1f, 0.2f, 0.2f, 0.9f, 0.9f);
            _meshRenderer.material.SetColor(ShaderProperties.Color, _color);
        }
    }
}