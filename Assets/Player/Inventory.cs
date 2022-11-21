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
    public static readonly int inventorySlots = 4;
    List<AttackBlock> storage = new List<AttackBlock>();

    List<AttackBlock> equipped = new List<AttackBlock>();


    PlayerGhost player;
    SaveData save;



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
    public void loadPity(Dictionary<string, float> values)
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
        equipped = items.Take(inventorySlots).ToList();
        storage = items.Skip(inventorySlots).ToList();
        genMinItems();
        TargetSyncInventory(connectionToClient, equipped.ToArray(), storage.ToArray());
    }
    [Server]
    public void genMinItems()
    {
        for (int i = equipped.Count; i < 4; i++)
        {
            AttackBlock item = GenerateAttack.generate(player.power, i == 0, Quality.Common);
            equipped.Add(item);
            save.saveItem(item);
        }
    }

    [Server]
    public void AddItem(AttackBlock item, Vector3 otherPosition)
    {
        storage.Add(item);
        save.saveItem(item);
        TargetDropItem(connectionToClient, item, otherPosition);
    }

    [Server]
    public Quality rollQuality()
    {
        return pityQuality.roll();
    }

    //Server
    public List<AttackBlock> equippedAbilities
    {
        get
        {
            return equipped;
        }
    }
    public List<AttackBlock> stored
    {
        get
        {
            return storage;
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
    public AttackBlockFilled fillBlock(AttackBlock block)
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
        TargetSyncInventory(connectionToClient, equipped.ToArray(), storage.ToArray());
    }

    [TargetRpc]
    void TargetSyncInventory(NetworkConnection conn, AttackBlock[] eq, AttackBlock[] st)
    {
        equipped = eq.ToList();
        storage = st.ToList();


        FindObjectOfType<ItemList>(true).fillAbilities(this);
    }

    [Command]
    public void CmdEquipAbility(string oldId, string newId)
    {
        int oldIndex = equipped.FindIndex(item => item.id == oldId);
        int newIndex = storage.FindIndex(item => item.id == newId);
        if (oldIndex >= 0 && newIndex >=0)
        {
            AttackBlock unequipped = equipped[oldIndex];
            equipped[oldIndex] = storage[newIndex];
            storage.Add(unequipped);
        }
    }
}
