using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialScaling : MonoBehaviour
{


    public void scale(float scale)
    {

        Shader.SetGlobalFloat("_Target_Distance", 22 * scale);
        Shader.SetGlobalFloat("_Clip_Min_Percent", 0.25f);
        Shader.SetGlobalFloat("_Clip_Top_Percent", 0.85f);
        Shader.SetGlobalFloat("_Clip_Max_Percent", 0.95f);
        
        


    }

    private void OnDestroy()
    {
        Shader.SetGlobalFloat("_Target_Distance", 0);
    }


}
