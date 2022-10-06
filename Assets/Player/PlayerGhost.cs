using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerGhost : NetworkBehaviour
{
    public GameObject unitPre;

    public int attacksToGenerate = 4;

    GameObject currentSelf;

    [SyncVar]
    float playerPower = 1000;
    void Start()
    {
        if (isLocalPlayer)
        {
            FindObjectOfType<GlobalPlayer>().setLocalPlayer(this);
            FindObjectOfType<MenuHandler>().clientMenu();
            if (isClientOnly)
            {
                CmdAddClient();
            }


        }
    }
    public float power
    {
        get
        {
            return playerPower;
        }
    }

    public void spawnPlayer()
    {
        CmdAddPlayer();
    }

    [Command]
    void CmdAddPlayer()
    {
        Inventory inv = GetComponent<Inventory>();

        GameObject u = Instantiate(unitPre);
        Power p = u.GetComponent<Power>();
        p.setPower(playerPower);
        p.subscribePower(syncPower);
        u.GetComponent<Reward>().setInventory(inv);
        u.GetComponent<AbiltyList>().addAbility(inv.equipped);
        NetworkServer.Spawn(u, connectionToClient);
    }

    public GameObject unit
    {
        get { return currentSelf; }
        set { currentSelf = value; }
    }

    [Server]
    void syncPower(Power p)
    {
        playerPower = p.power;
    }

    [Command]
    void CmdAddClient()
    {
        FindObjectOfType<SharedMaterials>().SyncVisuals(connectionToClient);

    }
}
