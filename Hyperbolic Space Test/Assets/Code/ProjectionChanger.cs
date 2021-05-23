using System;
using DG.Tweening;
using UnityEngine;

// Press Q for change projection in game
public class ProjectionChanger : MonoBehaviour
{
    [SerializeField] private float _tweeningTime = 0.5f;

    private enum ProjectionType
    {
        Poincare,
        BeltramiKlein
    }

    private ProjectionType _projectionType = ProjectionType.Poincare;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q) == false)
        {
            return;
        }

        this.DOKill();

        if (_projectionType == ProjectionType.Poincare)
        {
            _projectionType = ProjectionType.BeltramiKlein;
            DOTween.To(() => Shader.GetGlobalFloat(ShaderProperties.BeltramiKleinFactor),
                    value => Shader.SetGlobalFloat(ShaderProperties.BeltramiKleinFactor, value), 1f, _tweeningTime)
                .SetTarget(this)
                .SetEase(Ease.InOutQuad);
        }
        else if (_projectionType == ProjectionType.BeltramiKlein)
        {
            _projectionType = ProjectionType.Poincare;
            DOTween.To(() => Shader.GetGlobalFloat(ShaderProperties.BeltramiKleinFactor),
                    value => Shader.SetGlobalFloat(ShaderProperties.BeltramiKleinFactor, value), 0f, _tweeningTime)
                .SetTarget(this)
                .SetEase(Ease.InOutQuad);
        }
    }

    private void OnApplicationQuit()
    {
        Shader.SetGlobalFloat(ShaderProperties.BeltramiKleinFactor, 0f);
    }
}