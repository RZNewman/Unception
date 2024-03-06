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
using static Grove;

public class Inventory : NetworkBehaviour
{
    int inventoryLimit = 50;
    int blessingLimit = 4;

    List<CastData> tempDrops = new List<CastData>();

    Dictionary<string, CastData> storage = new Dictionary<string, CastData>();

    Dictionary<string, AbilityDataInstance> filledCache = new Dictionary<string, AbilityDataInstance>();

    TriggerData[] blessingsActive = new TriggerData[0];
    TriggerData blessingPotential;

    PlayerGhost player;
    Grove grove;
    public PlayerPity pity;



    GameObject itemPre;

    private void Start()
    {
        player = GetComponent<PlayerGhost>();
        pity = GetComponent<PlayerPity>();
        itemPre = GlobalPrefab.gPre.ItemDropPre;
        grove = GetComponent<Grove>();
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


    float unusedItems
    {
        get
        {
            return storage.Where(pair => !grove.isPlaced(pair.Key)).Count();
        }
    }


    public bool overburdened
    {
        get
        {
            return unusedItems > inventoryLimit;
        }
    }
    public string inventoryCount
    {
        get
        {
            return unusedItems + " /" + inventoryLimit;
        }
    }



    [Server]
    public void reloadItems(CastData[] storageItems, Dictionary<string, GrovePlacement> placedData)
    {
        storage = storageItems.ToDictionary(i => i.id);
        storage.ToList().ForEach(pair => fillInstanceCache(pair.Value));
        grove.importPlacements(placedData, storage);

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
        blessingsActive.Where(b => b!= null).ToList().ForEach(bless => fillInstanceCache(bless));
        syncInventoryUpwards();
    }


    [Server]
    public void genMinItems()
    {

        CastData item1 = generate(player.power, AttackGenerationType.IntroMain);
        CastData item2 = generate(player.power, AttackGenerationType.IntroOff);


        storage.Add(item1.id, item1);
        storage.Add(item2.id, item2);
        fillInstanceCache(item1);
        fillInstanceCache(item2);
        Vector2Int center = grove.center;
        Dictionary<string, GrovePlacement> placements = new Dictionary<string, GrovePlacement>();
        placements.Add(item1.id, new GrovePlacement
        {
            position = center - Vector2Int.one * 2 ,
            rotation = Rotation.None,
        });
        placements.Add(item2.id, new GrovePlacement
        {
            position = center + Vector2Int.one * 2,
            rotation = Rotation.None,
        });
        grove.importPlacements(placements, storage);

        syncInventoryUpwards();
        RpcInvChange();
    }

    [Server]
    public void genMinBlessings()
    {

        blessingsActive = new TriggerData[maxBlessings];
        syncInventoryUpwards();
    }

    [Server]
    public void genRandomItems()
    {
        for (int i = 0; i < 5; i++)
        {
            CastData item = GenerateAttack.generate(player.power, AttackGenerationType.Player);
            storage.Add(item.id, item);
            fillInstanceCache(item);
        }
        blessingsActive = new TriggerData[blessingLimit];
        blessingsActive[0] = GenerateTrigger.generate(player.power);
        RpcInvChange();
    }

    [Server]
    public void AddItem(CastData item, Vector3 otherPosition)
    {
        tempDrops.Add(item);
        fillInstanceCache(item);
        TargetDropItem(connectionToClient, item, otherPosition);
    }

    [Server]
    public void addBlessing(float power, float difficulty)
    {
        blessingPotential = GenerateTrigger.generate(power, difficulty);
        fillInstanceCache(blessingPotential);
        syncInventoryUpwards();
    }

    //server
    public CastData[] exportStorage()
    {
        return storage.Values.ToArray();
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
            return grove.slotted.Select(pair => (pair.Key, storage[pair.Value])).ToDictionary(tupe => tupe.Key, tupe => tupe.Item2);
        }
    }
    public List<CastData> unequipped
    {
        get
        {
            return storage.Where(pair => !grove.isPlaced(pair.Key)).Select(pair => pair.Value).ToList();
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
            return blessingsActive.Where(b => b!= null).ToList();
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
        GameObject i = Instantiate(itemPre, location, Random.rotation);
        i.GetComponent<ItemDrop>().init(player.scales, player.unit, item.quality);

    }

    void fillInstanceCache(AbilityData block)
    {
        float numericScale = Power.scaleNumerical(player.power);
        filledCache.TryAdd(block.id, block.populate(new FillBlockOptions { 
            overridePower = player.power,
            baseScales = new BaseScales
            {
                world = numericScale,
                time = numericScale,
            }
        }));;
    }

    public AbilityDataInstance getAbilityInstance(string id)
    {
        return filledCache[id];
    }

    [Server]
    public void syncInventoryUpwards()
    {
        TargetSyncInventory(connectionToClient, storage.Values.ToArray(), blessingsActive, blessingPotential);
    }

    [TargetRpc]
    void TargetSyncInventory(NetworkConnection conn,  CastData[] st, TriggerData[] bl, TriggerData blP)
    {
        storage = st.ToDictionary(i=>i.id);
        blessingsActive = bl;
        blessingPotential = blP;
        st.ToList().ForEach(item => fillInstanceCache(item));
        bl.Where(b => b != null).ToList().ForEach(item => fillInstanceCache(item));
        if(blP)
        {
            fillInstanceCache(blP);
        }
        FindObjectOfType<UILoadoutMenu>(true).displayUpgrades();
    }

    [ClientRpc]
    public void RpcInvChange()
    {
        foreach (OnInvUpdate callback in OnInvUpdateCallbacks)
        {
            callback(this);
        }
    }


    [Command]
    [System.Obsolete("Equipment through grove now")]
    public void CmdEquipAbility(string newId)
    {
        //int newIndex = -1;
        //List<CastData> source = storage;
        //bool fromDrops = false;
        //newIndex = tempDrops.FindIndex(item => item.id == newId);
        //if (newIndex >= 0)
        //{
        //    fromDrops = true;
        //    source = tempDrops;
        //}
        //else
        //{
        //    newIndex = storage.FindIndex(item => item.id == newId);
        //}


        //if (newIndex >= 0)
        //{
        //    if (fromDrops)
        //    {

        //        CastData nowEquipped = tempDrops[newIndex];
        //        bool previous = equipped.TryGetValue(nowEquipped.slot.Value, out CastData unequipped);
        //        equipped[nowEquipped.slot.Value] = nowEquipped;
        //        tempDrops.Remove(nowEquipped);
        //        if (previous)
        //        {
        //            storage.Add(unequipped);
        //        }
        //        RpcInvChange();
        //    }
        //    else
        //    {
        //        CastData nowEquipped = storage[newIndex];
        //        bool previous = equipped.TryGetValue(nowEquipped.slot.Value, out CastData unequipped);
        //        equipped[nowEquipped.slot.Value] = nowEquipped;
        //        storage.Remove(nowEquipped);
        //        if (previous)
        //        {
        //            storage.Add(unequipped);
        //        }
        //        RpcInvChange();
        //    }

        //}
    }

    [Command]
    public void CmdSendStorage(string id)
    {
        CastData moved = tempDrops.Find(item => item.id == id);
        if (!moved)
        {
            return;
        }
        tempDrops.Remove(moved);
        storage.Add(moved.id, moved);


        RpcInvChange();
    }
    [Command]
    public void CmdSendTrash(string id)
    {
        CastData moved;
        if (!storage.TryGetValue(id, out moved))
        {
            return;
        }
        storage.Remove(id);
        tempDrops.Add(moved);
        RpcInvChange();
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
