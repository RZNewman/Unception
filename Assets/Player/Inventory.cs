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
using static GenerateAttack;

public class Inventory : NetworkBehaviour
{
    int inventoryLimit = 10;
    int blessingLimit = 4;

    List<CastData> tempDrops = new List<CastData>();

    List<CastData> storage = new List<CastData>();

    Dictionary<ItemSlot, CastData> equipped = new Dictionary<ItemSlot, CastData>();

    TriggerData[] blessingsActive = new TriggerData[0];
    TriggerData blessingPotential;

    PlayerGhost player;
    public PlayerPity pity;



    GameObject itemPre;

    private void Start()
    {
        player = GetComponent<PlayerGhost>();
        pity = GetComponent<PlayerPity>();
        itemPre = GameObject.FindObjectOfType<GlobalPrefab>().ItemDropPre;
    }

    public delegate void OnInvUpdate(Inventory inv);

    List<OnInvUpdate> OnInvUpdateCallbacks = new List<OnInvUpdate>();

    public void subscribeInventory(OnInvUpdate callback)
    {
        OnInvUpdateCallbacks.Add(callback);
        callback(this);
    }

    public int maxBlessings
    {
        get
        {
            return blessingLimit;
        }
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

    CastData[] equipArray
    {
        get
        {
            return equipped.Values.ToArray();
        }
        set
        {
            equipped = new Dictionary<ItemSlot, CastData>();
            int index = 0;
            foreach (CastData block in value)
            {
                if (block != null && block.slot != null)
                {
                    equipped.Add(block.slot.Value, block);
                }
                index++;

            }
        }

    }

    [Server]
    public void reloadItems(CastData[] storageItems, CastData[] equippedItems)
    {
        equipArray = equippedItems;
        storage = storageItems.ToList();

        syncInventoryUpwards();
        RpcInvChange();
    }

    [Server]
    public void reloadBlessings(TriggerData[] bless)
    {
        blessingsActive = new TriggerData[blessingLimit];
        for (int i = 0; i < bless.Length; i++)
        {
            blessingsActive[i] = bless[i];
        }
        syncInventoryUpwards();
    }


    [Server]
    public void genMinItems()
    {

        CastData item = generate(player.power, AttackGenerationType.IntroMain);
        equipped.Add(ItemSlot.Main, item);
        item = generate(player.power, AttackGenerationType.IntroOff);
        equipped.Add(ItemSlot.OffHand, item);

    }

    [Server]
    public void genMinBlessings()
    {

        blessingsActive = new TriggerData[blessingLimit];
        syncInventoryUpwards();
    }

    [Server]
    public void genRandomItems()
    {
        for (int i = 0; i < 5; i++)
        {
            CastData item = GenerateAttack.generate(player.power, AttackGenerationType.Player);
            storage.Add(item);
        }
        blessingsActive = new TriggerData[blessingLimit];
        blessingsActive[0] = GenerateTrigger.generate(player.power);
        RpcInvChange();
    }

    [Server]
    public void AddItem(CastData item, Vector3 otherPosition)
    {
        tempDrops.Add(item);
        TargetDropItem(connectionToClient, item, otherPosition);
    }

    [Server]
    public void addBlessing(float power, float difficulty)
    {
        blessingPotential = GenerateTrigger.generate(power, difficulty);
    }

    //server
    public CastData[] exportEquipped()
    {
        return equipArray;
    }
    public CastData[] exportStorage()
    {
        return storage.ToArray();
    }

    public TriggerData[] exportBlessings()
    {
        return blessings.ToArray();
    }



    //Server
    public Dictionary<ItemSlot, CastData> equippedAbilities
    {
        get
        {
            return equipped;
        }
    }
    public List<CastData> stored
    {
        get
        {
            return storage;
        }
    }
    public List<CastData> dropped
    {
        get
        {
            return tempDrops;
        }
    }

    public List<TriggerData> blessings
    {
        get
        {
            return blessingsActive.Where(b => b).ToList();
        }
    }
    public TriggerData potentialBlessing
    {
        get
        {
            return blessingPotential;
        }
    }

    [TargetRpc]
    void TargetDropItem(NetworkConnection conn, CastData item, Vector3 location)
    {
        CastDataInstance filled = (CastDataInstance)fillBlock(item);
        GameObject i = Instantiate(itemPre, location, Random.rotation);
        i.GetComponent<ItemDrop>().init(player.power, player.unit, filled.quality);

    }

    public AbilityDataInstance fillBlock(AbilityData block, float? triggerStrength = null)
    {
        return block.populate(new FillBlockOptions { overridePower = player.power, addedStrength = triggerStrength, reduceWindValue = triggerStrength.HasValue });
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
        TargetSyncInventory(connectionToClient, equipArray, storage.ToArray(), blessingsActive, blessingPotential);
    }

    [TargetRpc]
    void TargetSyncInventory(NetworkConnection conn, CastData[] eq, CastData[] st, TriggerData[] bl, TriggerData blP)
    {
        equipArray = eq;
        storage = st.ToList();
        blessingsActive = bl;
        blessingPotential = blP;

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
    public void CmdEquipAbility(string newId)
    {
        int newIndex = -1;
        List<CastData> source = storage;
        bool fromDrops = false;
        newIndex = tempDrops.FindIndex(item => item.id == newId);
        if (newIndex >= 0)
        {
            fromDrops = true;
            source = tempDrops;
        }
        else
        {
            newIndex = storage.FindIndex(item => item.id == newId);
        }


        if (newIndex >= 0)
        {
            if (fromDrops)
            {

                CastData nowEquipped = tempDrops[newIndex];
                bool previous = equipped.TryGetValue(nowEquipped.slot.Value, out CastData unequipped);
                equipped[nowEquipped.slot.Value] = nowEquipped;
                tempDrops.Remove(nowEquipped);
                if (previous)
                {
                    storage.Add(unequipped);
                }
                RpcInvChange();
            }
            else
            {
                CastData nowEquipped = storage[newIndex];
                bool previous = equipped.TryGetValue(nowEquipped.slot.Value, out CastData unequipped);
                equipped[nowEquipped.slot.Value] = nowEquipped;
                storage.Remove(nowEquipped);
                if (previous)
                {
                    storage.Add(unequipped);
                }
                RpcInvChange();
            }

        }
    }

    [Command]
    public void CmdSendStorage(string id)
    {
        int index;
        index = storage.FindIndex(item => item.id == id);
        if (index >= 0)
        {
            return;
        }
        index = tempDrops.FindIndex(item => item.id == id);
        CastData moved;
        if (index >= 0)
        {
            moved = tempDrops[index];
            tempDrops.RemoveAt(index);
            storage.Add(moved);
            RpcInvChange();
            return;
        }
        KeyValuePair<ItemSlot, CastData> pair = equipped.First(pair => pair.Value.id == id);
        moved = equipped[pair.Key];
        equipped.Remove(pair.Key);
        storage.Add(moved);
        RpcInvChange();



    }
    [Command]
    public void CmdSendTrash(string id)
    {
        int index;
        index = tempDrops.FindIndex(item => item.id == id);
        if (index >= 0)
        {
            return;
        }
        index = storage.FindIndex(item => item.id == id);
        CastData moved;
        if (index >= 0)
        {
            moved = storage[index];
            storage.RemoveAt(index);
            tempDrops.Add(moved);
            RpcInvChange();
            return;
        }
        KeyValuePair<ItemSlot, CastData> pair = equipped.First(pair => pair.Value.id == id);
        moved = equipped[pair.Key];
        equipped.Remove(pair.Key);
        tempDrops.Add(moved);

    }
    [Command]
    public void CmdEquipBlessing(int blessingSlot)
    {
        if (blessingPotential && blessingSlot >= 0 && blessingSlot < maxBlessings)
        {
            blessingsActive[blessingSlot] = blessingPotential;
            blessingPotential = null;

            syncInventoryUpwards();
            RpcInvChange();
        }
    }
    public void clearDrops()
    {
        tempDrops.Clear();
    }
}
