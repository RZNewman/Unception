using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SharedMaterials : MonoBehaviour
{
    public Shader shader;
    List<Material> materials = new List<Material>();

    public Material addMaterial(Color c)
    {
        Material m = new Material(shader);
        m.SetColor(Shader.PropertyToID("_Color"), c);
        materials.Add(m);
        return m;
    }

}
