using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class LocalPlayer : NetworkBehaviour
{
    public GameObject localCameraPre;
    public GameObject localClickPre;
    public int attacksToGenerate = 2;
    // Start is called before the first frame update
    void Start()
    {
        if (isLocalPlayer)
        {
            Instantiate(localCameraPre, transform);
            Instantiate(localClickPre, transform);
            GameObject.FindGameObjectWithTag("LocalCanvas").GetComponent<UnitUiReference>().setTarget(gameObject);
            FindObjectOfType<GlobalPlayer>().setLocalPlayer(gameObject);
            gameObject.GetComponentInChildren<UnitUiReference>().gameObject.SetActive(false);
            CmdPlayerObject();
            if (isClientOnly)
            {
                CmdAddClient();
            }

        }
    }
    [Command]
    void CmdPlayerObject()
    {
        Power p = GetComponent<Power>();
        p.setPower(1000);
        List<AttackBlock> attackBlocks = new List<AttackBlock>();
        for (int i = 0; i < attacksToGenerate; i++)
        {
            AttackBlock b = GenerateAttack.generate(p.power, i == 0);
            b.scales = true;
            attackBlocks.Add(b);
        }
        GetComponent<AbiltyList>().addAbility(attackBlocks);
    }

    [Command]
    void CmdAddClient()
    {
        FindObjectOfType<SharedMaterials>().SyncVisuals(connectionToClient);

    }
}
