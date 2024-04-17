using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorIndividual : MonoBehaviour
{
    private Renderer _renderer;
    private MaterialPropertyBlock _propBlock;
    SpriteRenderer _spriteRenderer;

    Vector2 offset;

    void Awake()
    {
        offset = new Vector2(Random.value, Random.value);
    }

    public void setColor(Color c, string colorProperty = "_BaseColor")
    {
        _propBlock = new MaterialPropertyBlock();
        _renderer = GetComponent<Renderer>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        if (_spriteRenderer)
        {
            _spriteRenderer.color = c;
        }
        else
        {
            // Get the current value of the material properties in the renderer.
            _renderer.GetPropertyBlock(_propBlock);
            // Assign our new value.
            _propBlock.SetColor(colorProperty, c);
            // Assign our new value.
            _propBlock.SetVector("_Offset", offset);
            // Apply the edited values to the renderer.
            _renderer.SetPropertyBlock(_propBlock);
        }
        
    }
}
