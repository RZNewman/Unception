using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialScaling : MonoBehaviour
{
    float cloudTextureDist = 0;

    float cameraMult = 0;
    float camDist = 0;
    static readonly float playerDistBase = 2000;
    float playerDist = playerDistBase;
    public void game()
    {

        //Shader.SetGlobalFloat("_Target_Distance", distance);
        cameraMult = 1;
        playerDist = 100;
        setMaterialDistance();
        Shader.SetGlobalFloat("_Clip_Min_Percent", 0.25f);
        Shader.SetGlobalFloat("_Clip_Top_Percent", 0.95f);
        Shader.SetGlobalFloat("_Clip_Max_Percent", 0.95f);
        




    }
    public void none()
    {
        cameraMult = 0;
        playerDist = playerDistBase;
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
        float[] distances = new float[32];
        distances[LayerMask.NameToLayer("Players")] = playerDist;
        Camera.main.layerCullDistances = distances;
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
