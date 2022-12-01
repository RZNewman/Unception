using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using static RewardManager;
using static Power;

public class Inventory : NetworkBehaviour
{
    public static readonly int inventorySlots = 4;
    int inventoryLimit = 10;

    List<AttackBlock> tempDrops = new List<AttackBlock>();

    List<AttackBlock> storage = new List<AttackBlock>();

    List<AttackBlock> equipped = new List<AttackBlock>();
    AttackBlock deleteStaged;

    PlayerGhost player;



    GameObject itemPre;
    PityTimer<Quality> pityQuality;
    private void Start()
    {
        player = GetComponent<PlayerGhost>();
        itemPre = GameObject.FindObjectOfType<GlobalPrefab>().ItemDropPre;
    }

    public delegate void OnInvUpdate(Inventory inv);

    List<OnInvUpdate> OnInvUpdateCallbacks = new List<OnInvUpdate>();

    public void subscribeInventory(OnInvUpdate callback)
    {
        OnInvUpdateCallbacks.Add(callback);
        callback(this);
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

    public bool overburdened
    {
        get
        {
            return storage.Count > inventoryLimit;
        }
    }
    public string inventoryCount
    {
        get
        {
            return storage.Count + " /" + inventoryLimit;
        }
    }

    [Server]
    public void reloadItems(AttackBlock[] items)
    {
        equipped = items.Take(inventorySlots).ToList();
        storage = items.Skip(inventorySlots).ToList();
        genMinItems();
        TargetSyncInventory(connectionToClient, equipped.ToArray(), storage.ToArray());
        RpcInvChange();
    }
    [Server]
    public void genMinItems()
    {
        for (int i = equipped.Count; i < inventorySlots; i++)
        {
            AttackBlock item = GenerateAttack.generate(player.power, i == 0, Quality.Common);
            equipped.Add(item);
        }
    }

    [Server]
    public void AddItem(AttackBlock item, Vector3 otherPosition)
    {
        tempDrops.Add(item);
        TargetDropItem(connectionToClient, item, otherPosition);
    }

    //server
    public AttackBlock[] exportItems()
    {
        return equipped.Concat(storage).ToArray();
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
    public List<AttackBlock> dropped
    {
        get
        {
            return tempDrops;
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



    }

    [ClientRpc]
    void RpcInvChange()
    {
        foreach (OnInvUpdate callback in OnInvUpdateCallbacks)
        {
            callback(this);
        }
    }

    [Command]
    public void CmdEquipAbility(string oldId, string newId, bool fromDrops)
    {
        int oldIndex = equipped.FindIndex(item => item.id == oldId);
        int newIndex;
        if (fromDrops)
        {
            newIndex = tempDrops.FindIndex(item => item.id == newId);
        }
        else
        {
            newIndex = storage.FindIndex(item => item.id == newId);
        }

        if (oldIndex >= 0 && newIndex >= 0)
        {
            if (fromDrops)
            {
                AttackBlock unequipped = equipped[oldIndex];
                AttackBlock nowEquipped = tempDrops[newIndex];
                equipped[oldIndex] = nowEquipped;
                tempDrops.Add(unequipped);
                tempDrops.Remove(nowEquipped);
            }
            else
            {
                AttackBlock unequipped = equipped[oldIndex];
                AttackBlock nowEquipped = storage[newIndex];
                equipped[oldIndex] = nowEquipped;
                storage.Add(unequipped);
                storage.Remove(nowEquipped);
                RpcInvChange();
            }

        }
    }

    [Command]
    public void CmdStageDelete(string id)
    {
        int index = storage.FindIndex(item => item.id == id);
        if (index >= 0)
        {
            deleteStaged = storage[index];
            storage.RemoveAt(index);

            RpcInvChange();
        }

    }
    [Command]
    public void CmdUnstageDelete()
    {
        if (deleteStaged)
        {
            storage.Add(deleteStaged);
            deleteStaged = null;
            RpcInvChange();
        }


    }
    [Command]
    public void CmdSendStorage(string id)
    {
        int index = tempDrops.FindIndex(item => item.id == id);
        if (index >= 0)
        {
            AttackBlock moved = tempDrops[index];
            tempDrops.RemoveAt(index);
            storage.Add(moved);
            RpcInvChange();
        }


    }
    //server
    public void clearDelete()
    {
        deleteStaged = null;

    }
}
