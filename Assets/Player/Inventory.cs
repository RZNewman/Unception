using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using static RewardManager;
using static Power;
using static UnitControl;
using static Keybinds;
using static Utils;

public class Inventory : NetworkBehaviour
{
    public static readonly int inventorySlots = 4;
    int inventoryLimit = 10;

    List<AttackBlock> tempDrops = new List<AttackBlock>();

    List<AttackBlock> storage = new List<AttackBlock>();

    Dictionary<AttackKey, AttackBlock> equipped = new Dictionary<AttackKey, AttackBlock>();
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

    AttackBlock[] equipArray
    {
        get
        {
            return EnumValues<AttackKey>().Select(key => equipped[key]).ToArray();
        }
        set
        {
            equipped = new Dictionary<AttackKey, AttackBlock>();
            int index = 0;
            foreach (AttackBlock block in value)
            {
                equipped.Add((AttackKey)index, block);
                index++;
            }
        }

    }

    [Server]
    public void reloadItems(AttackBlock[] items)
    {
        AttackBlock[] equippedArray = items.Take(inventorySlots).ToArray();
        equipArray = equippedArray;
        storage = items.Skip(inventorySlots).ToList();
        genMinItems();
        TargetSyncInventory(connectionToClient, equippedArray, storage.ToArray());
        RpcInvChange();
    }
    [Server]
    public void genMinItems()
    {
        for (int i = equipped.Count; i < inventorySlots; i++)
        {
            AttackBlock item = GenerateAttack.generate(player.power, i == 0, Quality.Common);
            equipped.Add((AttackKey)i, item);
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
        return equipArray.Concat(storage).ToArray();
    }

    [Server]
    public Quality rollQuality()
    {
        return pityQuality.roll();
    }

    //Server
    public Dictionary<AttackKey, AttackBlock> equippedAbilities
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
        TargetSyncInventory(connectionToClient, equipArray, storage.ToArray());
    }

    [TargetRpc]
    void TargetSyncInventory(NetworkConnection conn, AttackBlock[] eq, AttackBlock[] st)
    {
        equipArray = eq;
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
    public void CmdEquipAbility(AttackKey key, string newId, bool fromDrops)
    {
        int newIndex;
        if (fromDrops)
        {
            newIndex = tempDrops.FindIndex(item => item.id == newId);
        }
        else
        {
            newIndex = storage.FindIndex(item => item.id == newId);
        }

        if (newIndex >= 0)
        {
            if (fromDrops)
            {
                AttackBlock unequipped = equipped[key];
                AttackBlock nowEquipped = tempDrops[newIndex];
                equipped[key] = nowEquipped;
                tempDrops.Remove(nowEquipped);
                if (unequipped)
                {
                    tempDrops.Add(unequipped);
                }
            }
            else
            {
                AttackBlock unequipped = equipped[key];
                AttackBlock nowEquipped = storage[newIndex];
                equipped[key] = nowEquipped;
                storage.Remove(nowEquipped);
                if (unequipped)
                {
                    storage.Add(unequipped); ;
                }
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
    public void clearDrops()
    {
        tempDrops.Clear();
    }
}
