using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalPlayer : MonoBehaviour
{
    PlayerGhost serverOwnerPlayer;

    public void setServerPlayer(PlayerGhost player)
    {
        if(serverOwnerPlayer == null)
        {
            serverOwnerPlayer = player;
            
        }
        
    }
    public PlayerGhost serverPlayer
    {
        get { return serverOwnerPlayer; }
    }



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
    public PlayerGhost player
    {
        get { return clientLocalPlayer; }
    }

    public float localPowerThreat
    {
        get { return clientLocalPlayer.power; }
    }

    public float localStunThreat
    {
        get
        {
            return clientLocalPlayer.unit.GetComponent<Posture>().remainingToStun;
        }
    }
}
