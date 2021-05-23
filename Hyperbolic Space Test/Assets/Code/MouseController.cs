using System;
using System.Collections;
using UnityEngine;

public class MouseController : MonoBehaviour
{
    [SerializeField] private Transform _cameraTransform = null;
    [SerializeField] private Transform _characterTransform = null;
    [Space]
    [SerializeField] private float _sensitivityHor = 9.0f;
    [SerializeField] private float _sensitivityVert = 9.0f;
    [SerializeField] private float _minimumVert = -45.0f;
    [SerializeField] private float _maximumVert = 45.0f;
    [Space]
    [SerializeField] private bool _rotateVectrical = true;
    [SerializeField] private bool _rotateHorizontal = true;

    private float _rotationX = 0f;
    private float _rotationY = 0f;
    
    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (_rotateVectrical)
        {
            _rotationX -= Input.GetAxis("Mouse Y") * _sensitivityVert;
            _rotationX = Mathf.Clamp(_rotationX, _minimumVert, _maximumVert);
        }
        if (_rotateHorizontal)
        {
            _rotationY = Input.GetAxis("Mouse X") * _sensitivityHor;
        }

        _cameraTransform.localEulerAngles = new Vector3(_rotationX, 0f, 0f);
        _characterTransform.localEulerAngles += new Vector3(0f, _rotationY, 0f);
    }
}