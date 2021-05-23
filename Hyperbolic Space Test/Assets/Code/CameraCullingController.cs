using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraCullingController : MonoBehaviour
{
    [SerializeField] private float _cullingBoxWidth = 2f;
    private Camera _camera;
    
    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    void OnPreCull()
    {
        _camera.cullingMatrix = Matrix4x4.Ortho(-_cullingBoxWidth, _cullingBoxWidth, -_cullingBoxWidth, _cullingBoxWidth, 0.0001f, _cullingBoxWidth * 2f) * 
                                    Matrix4x4.Translate(Vector3.back * _cullingBoxWidth) * 
                                    _camera.worldToCameraMatrix;
    }
}
