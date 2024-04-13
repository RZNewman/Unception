using Mirror;
using System.Collections.Generic;
using UnityEngine;
using static UnitControl;
using System.Linq;
using static GenerateAttack;

public class AbilityManager : NetworkBehaviour
{

    Dictionary<int, Ability> instancedAbilitites = new Dictionary<int, Ability>();
    Dictionary<ItemSlot, int> slotLookups = new Dictionary<ItemSlot, int>();

    EventManager events;

    private void Awake()
    {
        events = GetComponent<EventManager>();
    }
    private void Start()
    {

    }
    public void addAbility(Dictionary<ItemSlot, CastData> blocks)
    {
        foreach ((ItemSlot slot, CastData block) in blocks)
        {
            instanceAbility(slot, block);
        }

    }
    public void addAbility(List<CastData> blocks)
    {
        for (int i = 0; i < blocks.Count; i++)
        {
            ItemSlot slotFake = (ItemSlot)i;
            CastData block = blocks[i];

            instanceAbility(slotFake, block);
        }


    }
    public Ability addTriggeredAbility(TriggerData block)
    {
        return instanceTriggeredAbility(block);

    }
    void instanceAbility(ItemSlot key, CastData block)
    {
        GameObject o = Instantiate(FindObjectOfType<GlobalPrefab>().AbilityRootPre, transform);
        Ability a = o.GetComponent<Ability>();
        a.setFormat(block);
        registerAbility(key, a);
        a.clientSyncKey = key;
        o.GetComponent<ClientAdoption>().parent = gameObject;
        NetworkServer.Spawn(o);
    }
    Ability instanceTriggeredAbility(TriggerData block)
    {
        GameObject o = Instantiate(FindObjectOfType<GlobalPrefab>().AbilityRootPre, transform);
        Ability a = o.GetComponent<Ability>();
        a.setFormat(block);
        instancedAbilitites.Add(a.GetInstanceID(), a);
        o.GetComponent<ClientAdoption>().parent = gameObject;
        NetworkServer.Spawn(o);
        return a;
    }

    public void registerAbility(ItemSlot k, Ability a)
    {
        events.TickEvent += a.Tick;
        instancedAbilitites.Add(a.GetInstanceID(), a);
        slotLookups.Add(k, a.GetInstanceID());
    }


    public Ability getAbility(ItemSlot key)
    {
        return slotLookups.ContainsKey(key) ? instancedAbilitites[slotLookups[key]] : null;
    }

    public Ability getAbility(int key)
    {
        return instancedAbilitites.ContainsKey(key) ? instancedAbilitites[key] : null;
    }
    public List<Ability> allAbilities()
    {
        return instancedAbilitites.Values.ToList();
    }

    public struct AbilityPair
    {
        public ItemSlot key;
        public Ability ability;
    }
    public AbilityPair getBestAbility()
    {
        return slotLookups.Keys
            .Select(k => new AbilityPair { key = k, ability = instancedAbilitites[slotLookups[k]] })
            .Where(p => p.ability.ready)
            .OrderBy(p => p.ability.cooldownPerCharge).Reverse()
            .First();
    }

}
