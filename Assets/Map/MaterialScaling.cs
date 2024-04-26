using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialScaling : MonoBehaviour
{
    float cloudTextureDist = 0;

    float cameraMult = 0;
    float camDist = 0;
    public void game()
    {

        //Shader.SetGlobalFloat("_Target_Distance", distance);
        cameraMult = 1;
        setMaterialDistance();
        Shader.SetGlobalFloat("_Clip_Min_Percent", 0.25f);
        Shader.SetGlobalFloat("_Clip_Top_Percent", 0.95f);
        Shader.SetGlobalFloat("_Clip_Max_Percent", 0.95f);
        
        


    }
    public void none()
    {
        cameraMult = 0;
        setMaterialDistance();
        //Shader.SetGlobalFloat("_Target_Distance", 0);




    }
    public void setCameraDistance(float dist)
    {
        camDist = dist;
        setMaterialDistance();
    }

    void setMaterialDistance()
    {
        Shader.SetGlobalFloat("_Target_Distance", camDist * cameraMult);
    }

    public void  addDistance(float d)
    {
        cloudTextureDist += d;
        Shader.SetGlobalFloat("_DistanceTraveled", cloudTextureDist
            );
    }

    private void OnDestroy()
    {
        Shader.SetGlobalFloat("_Target_Distance", 0);       
        Shader.SetGlobalFloat("_DistanceTraveled", 0);
    }


}
