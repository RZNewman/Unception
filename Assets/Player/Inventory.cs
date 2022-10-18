using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using static RewardManager;
using Castle.Core.Internal;
using static UnityEditor.Progress;

public class Inventory : NetworkBehaviour
{
    List<AttackBlock> abilitiesSync = new List<AttackBlock>();

    List<AttackBlockFilled> abilities = new List<AttackBlockFilled>();

    PlayerGhost player;
    SaveData save;

    int[] equippedIndices = new int[] { 0, 1, 2, 3 };

    GameObject itemPre;
    PityTimer<Quality> pityQuality;
    private void Start()
    {
        player = GetComponent<PlayerGhost>();
        itemPre = GameObject.FindObjectOfType<GlobalPrefab>().ItemDropPre;
        save = GetComponent<SaveData>();

        if (isServer)
        {
            

            
            
        }
    }
    [Server]
    public void createBasePity()
    {
        pityQuality = new PityTimer<Quality>(Quality.Common, 0.25f);
        float uncChance = RewardManager.uncommonChance;
        pityQuality.addCategory(Quality.Uncommon, uncChance);
        pityQuality.addCategory(Quality.Rare, uncChance * Mathf.Pow(RewardManager.qualityRarityFactor, 1));
        pityQuality.addCategory(Quality.Epic, uncChance * Mathf.Pow(RewardManager.qualityRarityFactor, 2));
        pityQuality.addCategory(Quality.Legendary, uncChance * Mathf.Pow(RewardManager.qualityRarityFactor, 3));
    }

    [Server]
    public void loadPity(Dictionary<string,float> values)
    {
        pityQuality = new PityTimer<Quality>(Quality.Common, 0.25f);
        float uncChance = RewardManager.uncommonChance;
        pityQuality.addCategory(Quality.Uncommon, uncChance, values[Quality.Uncommon.ToString()]);
        pityQuality.addCategory(Quality.Rare, uncChance * Mathf.Pow(RewardManager.qualityRarityFactor, 1), values[Quality.Rare.ToString()]);
        pityQuality.addCategory(Quality.Epic, uncChance * Mathf.Pow(RewardManager.qualityRarityFactor, 2), values[Quality.Epic.ToString()]);
        pityQuality.addCategory(Quality.Legendary, uncChance * Mathf.Pow(RewardManager.qualityRarityFactor, 3), values[Quality.Legendary.ToString()]);
    }

    public Dictionary<string, float> savePity()
    {
        return pityQuality.export();
    }

    [Server]
    public void reloadItems(AttackBlock[] items)
    {
        abilitiesSync = items.ToList();
        genMinItems();
        TargetSyncInventory(connectionToClient, abilitiesSync.ToArray());
    }
    [Server]
    public void genMinItems()
    {
        for (int i = abilitiesSync.Count; i < 4; i++)
        {
            AttackBlock item = GenerateAttack.generate(player.power, i == 0, Quality.Common);
            abilitiesSync.Add(item);
            save.saveItem(item);
        }
    }

    [Server]
    public void AddItem(AttackBlock item, Vector3 otherPosition)
    {
        abilitiesSync.Add(item);
        save.saveItem(item);
        TargetDropItem(connectionToClient, item, otherPosition);
    }

    [Server]
    public Quality rollQuality()
    {
        return pityQuality.roll();
    }

    //Server
    public List<AttackBlock> equipped
    {
        get
        {
            List<AttackBlock> e = new List<AttackBlock>();
            foreach (int i in equippedIndices)
            {
                e.Add(abilitiesSync[i]);
            }
            return e;
        }
    }

    [TargetRpc]
    void TargetDropItem(NetworkConnection conn, AttackBlock item, Vector3 location)
    {
        AttackBlockFilled filled = fillBlock(item);
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
    AttackBlockFilled fillBlock(AttackBlock block)
    {

        return GenerateAttack.fillBlock(block, player.power);
    }
    [Client]
    public void syncInventory()
    {
        CmdSyncInventory();

    }
    [Command]
    void CmdSyncInventory()
    {
        syncInventoryUpwards();
    }
    [Server]
    public void syncInventoryUpwards()
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
            abilities.Add(fillBlock(abilitiesSync[i]));
        }
        FindObjectOfType<ItemList>(true).fillAbilities(abilities, equippedIndices);
    }

    [Command]
    public void CmdEquipAbility(int oldInd, int newInd)
    {
        int i = System.Array.IndexOf(equippedIndices, oldInd);
        if (i >= 0 && newInd < abilitiesSync.Count)
        {
            equippedIndices[i] = newInd;
        }
    }
}
