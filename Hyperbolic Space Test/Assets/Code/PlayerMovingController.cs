using System;
using UnityEngine;

public class PlayerMovingController : MonoBehaviour
{
    public static event Action LostInFog = delegate { };
    public static event Action<Vector3> PositionChanged = delegate { };
    
    [SerializeField] private float _speed = 1f;

    [SerializeField] private float _height = 0.06f;

    [Space, SerializeField] private float _clampingDistance = 0.9992799f;
    
    private GyroVector _gyroPosition;
    private Transform _cachedTransform;

    private bool _clampDistance = false;
    private bool _inputActive = true;
    
    private void Awake()
    {
        _cachedTransform = transform;
        _gyroPosition = GyroVector.identity;
    }

    private void Start()
    {
        if (HyperMath.Curvature < 0)
        {
            _clampDistance = true;
        }
        
        Shader.SetGlobalVector(ShaderProperties.GlobalOffset,
            -_gyroPosition.position - Vector3.up*_height);
    }

    void Update()
    {
        if (_inputActive == false)
        {
            return;
        }

        bool needToUpdate = false;
        
        Vector3 translation = Vector3.zero;
        if (Input.GetKey(KeyCode.W))
        {
            translation += Vector3.forward;
            needToUpdate = true;
        }
        if (Input.GetKey(KeyCode.A))
        {
            translation += Vector3.left;
            needToUpdate = true;
        }
        if (Input.GetKey(KeyCode.S))
        {
            translation += Vector3.back;
            needToUpdate = true;
        }
        if (Input.GetKey(KeyCode.D))
        {
            translation += Vector3.right;
            needToUpdate = true;
        }
        
        if (Input.GetKey(KeyCode.Space))
        {
            _height += _speed * Time.deltaTime;
            needToUpdate = true;
        }
        if (Input.GetKey(KeyCode.LeftShift))
        {
            _height -= _speed * Time.deltaTime;
            needToUpdate = true;
        }

        if (needToUpdate == false)
        {
            return;
        }
        
        translation = _cachedTransform.TransformDirection(translation.normalized);
        
        _gyroPosition += translation * (_speed * Time.deltaTime);

        // Apply holonomy post-rotation
        _cachedTransform.rotation *= Quaternion.Inverse(_gyroPosition.gyration);
        _gyroPosition.gyration = Quaternion.identity;
        
        // For precision consistency
        _gyroPosition.position.y = 0f;

        if (_clampDistance)
        {
            if (_gyroPosition.position.magnitude > _clampingDistance)
            {
                LostInFog.Invoke();
            }

            _gyroPosition.position = Vector3.ClampMagnitude(_gyroPosition.position, _clampingDistance);
        }
        
        Shader.SetGlobalVector(ShaderProperties.GlobalOffset,
            -_gyroPosition.position - Vector3.up*_height);
        
        PositionChanged.Invoke(-_gyroPosition.position - Vector3.up*_height);
    }
}
