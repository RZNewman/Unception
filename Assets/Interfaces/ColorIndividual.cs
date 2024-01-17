using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorIndividual : MonoBehaviour
{
    private Renderer _renderer;
    private MaterialPropertyBlock _propBlock;

    void Awake()
    {
        _propBlock = new MaterialPropertyBlock();
        _renderer = GetComponent<Renderer>();
    }

    public void setColor(Color c)
    {
        // Get the current value of the material properties in the renderer.
        _renderer.GetPropertyBlock(_propBlock);
        // Assign our new value.
        _propBlock.SetColor("_BaseColor", c);
        // Assign our new value.
        _propBlock.SetVector("_Offset", new Vector2(Random.value, Random.value));
        // Apply the edited values to the renderer.
        _renderer.SetPropertyBlock(_propBlock);
    }
}
