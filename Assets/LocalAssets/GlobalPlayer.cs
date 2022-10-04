using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalPlayer : MonoBehaviour
{
    PlayerGhost clientLocalPlayer;
    public void setLocalPlayer(PlayerGhost player)
    {
        clientLocalPlayer = player;
    }
    public bool isSet
    {
        get
        {
            return clientLocalPlayer;
        }
    }

    public float localPower
    {
        get { return clientLocalPlayer.power; }
    }

    public void setLocalUnit(GameObject u)
    {
        clientLocalPlayer.unit = u;
    }
}
