using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialScaling : MonoBehaviour
{


    public void scale(float distance)
    {

        Shader.SetGlobalFloat("_Target_Distance", distance);
        Shader.SetGlobalFloat("_Clip_Min_Percent", 0.45f);
        Shader.SetGlobalFloat("_Clip_Top_Percent", 1.50f);
        Shader.SetGlobalFloat("_Clip_Max_Percent", 0.95f);
        
        


    }

    private void OnDestroy()
    {
        Shader.SetGlobalFloat("_Target_Distance", 0);
    }


}
