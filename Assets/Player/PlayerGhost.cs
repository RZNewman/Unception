using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerGhost : NetworkBehaviour
{
    public GameObject unitPre;

    public int attacksToGenerate = 4;

    [SyncVar]
    float playerPower = 1000;
    void Start()
    {
        if (isLocalPlayer)
        {
            FindObjectOfType<GlobalPlayer>().setLocalPlayer(this);
            CmdAddPlayer();
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

    [Command]
    void CmdAddPlayer()
    {

        GameObject u = Instantiate(unitPre);
        Power p = u.GetComponent<Power>();
        p.setPower(playerPower);
        p.subscribePower(syncPower);
        u.GetComponent<Reward>().setInventory(GetComponent<Inventory>());
        List<AttackBlock> attackBlocks = new List<AttackBlock>();
        for (int i = 0; i < attacksToGenerate; i++)
        {
            AttackBlock b = GenerateAttack.generate(p.power, i == 0);
            b.scales = true;
            attackBlocks.Add(b);
        }
        u.GetComponent<AbiltyList>().addAbility(attackBlocks);
        NetworkServer.Spawn(u, connectionToClient);
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
