using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialScaling : MonoBehaviour
{


    public void scale(float scale)
    {

        Shader.SetGlobalFloat("_Target_Distance", 22 * scale);
    }

    private void OnDestroy()
    {
        Shader.SetGlobalFloat("_Target_Distance", 0);
    }


}
