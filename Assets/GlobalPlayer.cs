using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalPlayer : MonoBehaviour
{
    public static GlobalPlayer gPlay;
    private void Start()
    {
        gPlay = this;
    }
    PlayerGhost serverOwnerPlayer;

    public void setServerPlayer(PlayerGhost player)
    {
        if (serverOwnerPlayer == null)
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
            return clientLocalPlayer && clientLocalPlayer.unit ? clientLocalPlayer.unit.GetComponent<Posture>().remainingToStun : int.MaxValue;
        }
    }

    public bool localInCombat
    {
        get
        {
            return clientLocalPlayer && clientLocalPlayer.unit && clientLocalPlayer.unit.GetComponent<Combat>().inCombat;
        }
    }


    [Client]
    public void clientPlayerGroveLeave()
    {
        clientLocalPlayer.GroveLeave();
    }

    [Server]
    public static void shutdown()
    {

        NetworkServer.Shutdown();
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
        if (NetworkClient.isConnected)
        {
            NetworkClient.Disconnect();
        }
    }

    public static void startHost()
    {
        FindObjectOfType<NetworkManager>().StartHost();
    }
    public static void startClient()
    {
        FindObjectOfType<NetworkManager>().StartClient();
    }
}
