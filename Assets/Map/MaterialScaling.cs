using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialScaling : MonoBehaviour
{
    public List<Material> materials = new List<Material>();

    public void scale(float scale)
    {
        foreach (Material mat in materials)
        {
            mat.SetFloat("_DefaultEffectRadius", 100 * scale);
            mat.SetFloat("_ConeObstructionDestroyRadius", 30 * scale);
        }
    }


}
