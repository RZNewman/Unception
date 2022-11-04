using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartPicker : MonoBehaviour
{
    public List<GameObject> parts;

    public GameObject selectedPart;
    
    public int partCount()
    {
        return parts.Count;
    }
    public void pickPart(int i, Color[] colors)
    {
        if(i < parts.Count)
        {
            selectedPart.SetActive(false);
            parts[i].SetActive(true);
            setPartColor(parts[i], colors);
        }
        else
        {
            Debug.LogError("Model index out of range");
        }
    }

    void setPartColor(GameObject part, Color[] colors)
    {
        MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
        Renderer renderer = part.GetComponent<Renderer>();

        int startIndex = 0;
        if (renderer.material.name.Contains("02"))
        {
            startIndex += 8;
        }

        // Get the current value of the material properties in the renderer.
        renderer.GetPropertyBlock(propBlock);
        // Assign our new value.

        for(int i = 0; i< 8; i++)
        {
            propBlock.SetColor("_Color0"+i, colors[i+startIndex]);
        }
        




        // Apply the edited values to the renderer.
        renderer.SetPropertyBlock(propBlock);
    }

}
