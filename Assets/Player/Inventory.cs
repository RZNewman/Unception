using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public class Inventory : NetworkBehaviour
{
    List<AttackBlock> abilitiesSync = new List<AttackBlock>();

    List<AttackBlockFilled> abilities = new List<AttackBlockFilled>();

    PlayerGhost player;

    GameObject itemPre;
    private void Start()
    {
        player = GetComponent<PlayerGhost>();
        itemPre = GameObject.FindObjectOfType<GlobalPrefab>().ItemDropPre;
    }
    [Server]
    public void AddItem(AttackBlock item, Vector3 otherPosition)
    {
        abilitiesSync.Add(item);
        TargetDropItem(connectionToClient, item, otherPosition);
    }

    [TargetRpc]
    void TargetDropItem(NetworkConnection conn, AttackBlock item, Vector3 location)
    {
        AttackBlockFilled filled = tryFillBlock(item);
        float scale = Power.scale(player.power);
        GameObject i = Instantiate(itemPre, location, Random.rotation);
        i.GetComponent<ItemDrop>().init(scale, player.unit, filled.instance.quality);

    }

    //void OnAbilityUpdated(SyncList<AttackBlock>.Operation op, int index, AttackBlock oldItem, AttackBlock newItem)
    //{
    //    AttackBlockFilled filled;
    //    switch (op)
    //    {
    //        case SyncList<AttackBlock>.Operation.OP_ADD:
    //            filled = tryFillBlock(newItem);
    //            abilities.Add(filled);
    //            break;
    //        case SyncList<AttackBlock>.Operation.OP_INSERT:
    //            filled = tryFillBlock(newItem);
    //            abilities.Insert(index, filled);
    //            break;
    //        case SyncList<AttackBlock>.Operation.OP_REMOVEAT:
    //            abilities.RemoveAt(index);
    //            break;
    //        case SyncList<AttackBlock>.Operation.OP_SET:
    //            filled = tryFillBlock(newItem);
    //            abilities[index] = filled;
    //            break;
    //        case SyncList<AttackBlock>.Operation.OP_CLEAR:
    //            abilities.Clear();
    //            break;
    //    }
    //}
    AttackBlockFilled tryFillBlock(AttackBlock block)
    {
        if (player.unit)
        {
            return GenerateAttack.fillBlock(block, player.power);
        }
        return null;
    }
    [Client]
    public void syncInventory()
    {
        CmdSyncInventory();

    }
    [Command]
    void CmdSyncInventory()
    {
        TargetSyncInventory(connectionToClient, abilitiesSync.ToArray());
    }
    [TargetRpc]
    void TargetSyncInventory(NetworkConnection conn, AttackBlock[] blocks)
    {
        abilitiesSync = blocks.ToList();
        abilities.Clear();
        for (int i = 0; i < abilitiesSync.Count; i++)
        {
            abilities[i] = tryFillBlock(abilitiesSync[i]);
        }
    }
}
