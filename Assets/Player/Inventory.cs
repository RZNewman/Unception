using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Inventory : NetworkBehaviour
{
    readonly SyncList<AttackBlock> abilitiesSync = new SyncList<AttackBlock>();

    List<AttackBlockFilled> abilities = new List<AttackBlockFilled>();

    GlobalPlayer player;
    private void Start()
    {
        player = FindObjectOfType<GlobalPlayer>();
        if (isClient)
        {
            abilitiesSync.Callback += OnAbilityUpdated;
        }

    }
    [Server]
    public void AddItem(AttackBlock item)
    {
        abilitiesSync.Add(item);

    }

    void OnAbilityUpdated(SyncList<AttackBlock>.Operation op, int index, AttackBlock oldItem, AttackBlock newItem)
    {
        AttackBlockFilled filled;
        switch (op)
        {
            case SyncList<AttackBlock>.Operation.OP_ADD:
                filled = tryFillBlock(newItem);
                abilities.Add(filled);
                break;
            case SyncList<AttackBlock>.Operation.OP_INSERT:
                filled = tryFillBlock(newItem);
                abilities.Insert(index, filled);
                break;
            case SyncList<AttackBlock>.Operation.OP_REMOVEAT:
                abilities.RemoveAt(index);
                break;
            case SyncList<AttackBlock>.Operation.OP_SET:
                filled = tryFillBlock(newItem);
                abilities[index] = filled;
                break;
            case SyncList<AttackBlock>.Operation.OP_CLEAR:
                abilities.Clear();
                break;
        }
    }
    AttackBlockFilled tryFillBlock(AttackBlock block)
    {
        if (player.isSet)
        {
            return GenerateAttack.fillBlock(block, player.localPower);
        }
        return null;
    }
    [Client]
    public void forceFill()
    {
        abilities.Clear();
        for (int i = 0; i < abilitiesSync.Count; i++)
        {
            abilities[i] = tryFillBlock(abilitiesSync[i]);
        }
    }
}
