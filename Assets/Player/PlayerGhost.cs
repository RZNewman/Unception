using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerGhost : NetworkBehaviour
{
    public GameObject unitPre;

    public int attacksToGenerate = 4;

    [SyncVar]
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
        if (isServer)
        {
            FindObjectOfType<GlobalPlayer>().setServerPlayer(this);
        }
    }
    public float power
    {
        get
        {
            return playerPower;
        }
    }

    public void embark(int mapIndex)
    {
        CmdEmbark(mapIndex);
    }

    [Command]
    void CmdEmbark(int mapIndex)
    {
        StartCoroutine(embarkRoutine(mapIndex));
    }

    [Server]
    IEnumerator embarkRoutine(int mapIndex)
    {
        MapGenerator gen = FindObjectOfType<MapGenerator>();
        Atlas atlas = FindObjectOfType<Atlas>();
        yield return gen.buildMap(atlas.getMap(mapIndex));

        Inventory inv = GetComponent<Inventory>();
        GameObject u = Instantiate(unitPre);
        Power p = u.GetComponent<Power>();
        p.setPower(playerPower);
        p.subscribePower(syncPower);
        u.GetComponent<Reward>().setInventory(inv);
        u.GetComponent<AbiltyList>().addAbility(inv.equipped);
        NetworkServer.Spawn(u, connectionToClient);
        currentSelf = u;

        TargetGameplayMenu(connectionToClient);
    }

    [TargetRpc]
    void TargetGameplayMenu(NetworkConnection conn)
    {
        FindObjectOfType<MenuHandler>().spawn();
    }

    public GameObject unit
    {
        get { return currentSelf; }
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
