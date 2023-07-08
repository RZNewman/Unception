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

    List<AttackBlock> tempDrops = new List<AttackBlock>();

    List<AttackBlock> storage = new List<AttackBlock>();

    Dictionary<ItemSlot, AttackBlock> equipped = new Dictionary<ItemSlot, AttackBlock>();

    AttackTrigger[] blessingsActive = new AttackTrigger[0];
    AttackTrigger blessingPotential;

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

    AttackBlock[] equipArray
    {
        get
        {
            return equipped.Values.ToArray();
        }
        set
        {
            equipped = new Dictionary<ItemSlot, AttackBlock>();
            int index = 0;
            foreach (AttackBlock block in value)
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
    public void reloadItems(AttackBlock[] storageItems, AttackBlock[] equippedItems)
    {
        equipArray = equippedItems;
        storage = storageItems.ToList();

        syncInventoryUpwards();
        RpcInvChange();
    }

    [Server]
    public void reloadBlessings(AttackTrigger[] bless)
    {
        blessingsActive = new AttackTrigger[blessingLimit];
        for (int i = 0; i < bless.Length; i++)
        {
            blessingsActive[i] = bless[i];
        }
        syncInventoryUpwards();
    }


    [Server]
    public void genMinItems()
    {

        AttackBlock item = generate(player.power, AttackGenerationType.IntroMain);
        equipped.Add(ItemSlot.Main, item);
        item = generate(player.power, AttackGenerationType.IntroOff);
        equipped.Add(ItemSlot.OffHand, item);

    }

    [Server]
    public void genMinBlessings()
    {

        blessingsActive = new AttackTrigger[blessingLimit];
        syncInventoryUpwards();
    }

    [Server]
    public void genRandomItems()
    {
        for (int i = 0; i < 5; i++)
        {
            AttackBlock item = GenerateAttack.generate(player.power, AttackGenerationType.Player);
            storage.Add(item);
        }
        blessingsActive = new AttackTrigger[blessingLimit];
        blessingsActive[0] = GenerateTrigger.generate(player.power);
        RpcInvChange();
    }

    [Server]
    public void AddItem(AttackBlock item, Vector3 otherPosition)
    {
        tempDrops.Add(item);
        TargetDropItem(connectionToClient, item, otherPosition);
    }

    [Server]
    public void addBlessing(float power)
    {
        blessingPotential = GenerateTrigger.generate(power);
    }

    //server
    public AttackBlock[] exportEquipped()
    {
        return equipArray;
    }
    public AttackBlock[] exportStorage()
    {
        return storage.ToArray();
    }

    public AttackTrigger[] exportBlessings()
    {
        return blessings.ToArray();
    }



    //Server
    public Dictionary<ItemSlot, AttackBlock> equippedAbilities
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

    public List<AttackTrigger> blessings
    {
        get
        {
            return blessingsActive.Where(b => b).ToList();
        }
    }
    public AttackTrigger potentialBlessing
    {
        get
        {
            return blessingPotential;
        }
    }

    [TargetRpc]
    void TargetDropItem(NetworkConnection conn, AttackBlock item, Vector3 location)
    {
        AttackBlockInstance filled = fillBlock(item);
        GameObject i = Instantiate(itemPre, location, Random.rotation);
        i.GetComponent<ItemDrop>().init(player.power, player.unit, filled.instance.quality);

    }

    public AttackBlockInstance fillBlock(AttackBlock block)
    {

        return block.fillBlock(null, player.power);
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
    void TargetSyncInventory(NetworkConnection conn, AttackBlock[] eq, AttackBlock[] st, AttackTrigger[] bl, AttackTrigger blP)
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
        List<AttackBlock> source = storage;
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

                AttackBlock nowEquipped = tempDrops[newIndex];
                bool previous = equipped.TryGetValue(nowEquipped.slot.Value, out AttackBlock unequipped);
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
                AttackBlock nowEquipped = storage[newIndex];
                bool previous = equipped.TryGetValue(nowEquipped.slot.Value, out AttackBlock unequipped);
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
        int index = tempDrops.FindIndex(item => item.id == id);
        AttackBlock moved;
        if (index >= 0)
        {
            moved = tempDrops[index];
            tempDrops.RemoveAt(index);
            storage.Add(moved);
            RpcInvChange();
            return;
        }
        KeyValuePair<ItemSlot, AttackBlock> pair = equipped.First(pair => pair.Value.id == id);
        moved = equipped[pair.Key];
        equipped.Remove(pair.Key);
        storage.Add(moved);
        RpcInvChange();



    }
    [Command]
    public void CmdSendTrash(string id)
    {
        int index = storage.FindIndex(item => item.id == id);
        AttackBlock moved;
        if (index >= 0)
        {
            moved = storage[index];
            storage.RemoveAt(index);
            tempDrops.Add(moved);
            RpcInvChange();
            return;
        }
        KeyValuePair<ItemSlot, AttackBlock> pair = equipped.First(pair => pair.Value.id == id);
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
