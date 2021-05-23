using System;
using UnityEditor;
using UnityEngine;

public class SymbolicCoordinate : MonoBehaviour
{
    [SerializeField] private string _symbolicCoord = String.Empty;
    [SerializeField] private bool _isRoot = false;
    
    private Vector3? _originalPosition = null;

    public Vector3 HyperbolicTileOffset { get; private set; }
    public string SymbolicCoord => _symbolicCoord;

    private void Awake()
    {
        _originalPosition = transform.position;
        if (_isRoot)
        {
            ApplyTillingTransform();
        }
    }

    public void SetSymbolicCoordinate(string symbolicCoord)
    {
        _symbolicCoord = symbolicCoord;
        ApplyTillingTransform();
    }

    [ContextMenu("Scale Root Tile")]
    private void ScaleRootTile()
    {
        transform.localScale = HyperMath.CurvatureTan(HyperMath.CellWidth) * Vector3.one;
    }
    
    private void ApplyTillingTransform()
    {
        HyperbolicTileOffset = HyperMath.TileCoordToGyroVector(SymbolicCoord).position;

        if (_isRoot)
        {
            transform.localScale = HyperMath.CurvatureTan(HyperMath.CellWidth) * Vector3.one;

            foreach (var symbolicCoordinate in GetComponentsInChildren<SymbolicCoordinate>())
            {
                if (symbolicCoordinate != this)
                {
                    symbolicCoordinate.SetSymbolicCoordinate(SymbolicCoord);
                }
            }
            return;
        }
        
        if (_originalPosition.HasValue == false)
        {
            _originalPosition = transform.position;
        }

        Vector3 originalPlanarOffset = new Vector3(_originalPosition.Value.x, 0f, _originalPosition.Value.z);
        Vector3 originalHeightHeight = new Vector3(0f, _originalPosition.Value.y, 0f);
        GyroVector gyroCoord = HyperMath.TileCoordToGyroVector(SymbolicCoord);

        if (HyperMath.Curvature < 0f)
        {
            transform.position = HyperMath.PoincareToKlein((gyroCoord + HyperMath.KleinToPoincare(originalPlanarOffset)).position);
        }
        else if (HyperMath.Curvature > 0f)
        {
            transform.position = (gyroCoord + originalPlanarOffset).position;
        }
        
        transform.position += originalHeightHeight;
        transform.rotation = Quaternion.Inverse(gyroCoord.gyration) * transform.rotation;
    }
}