using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialScaling : MonoBehaviour
{


    public void game(float distance)
    {

        Shader.SetGlobalFloat("_Target_Distance", distance);
        Shader.SetGlobalFloat("_Clip_Min_Percent", 0.25f);
        Shader.SetGlobalFloat("_Clip_Top_Percent", 0.95f);
        Shader.SetGlobalFloat("_Clip_Max_Percent", 0.95f);
        
        


    }
    public void none()
    {

        Shader.SetGlobalFloat("_Target_Distance", 0);




    }

    private void OnDestroy()
    {
        Shader.SetGlobalFloat("_Target_Distance", 0);
    }


}
