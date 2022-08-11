using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalPlayer : MonoBehaviour
{
    GameObject clientLocalPlayer;

    float power;
    public void setLocalPlayer(GameObject player)
    {
        clientLocalPlayer = player;
        clientLocalPlayer.GetComponent<Power>().subscribePower(setLocalPower);
    }



    public void setLocalPower(Power p)
    {
        power = p.power;
    }
    public float localPower
    {
        get { return power; }
    }
}
