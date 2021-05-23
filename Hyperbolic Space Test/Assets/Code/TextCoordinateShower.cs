using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextCoordinateShower : MonoBehaviour
{
    [SerializeField] private SymbolicCoordinate _coordinateProvider = null;
    [SerializeField] private TMPro.TextMeshPro _textMeshPro = null;

    private void Start()
    {
        _textMeshPro.text = _coordinateProvider.SymbolicCoord.ToUpper();
    }
}
