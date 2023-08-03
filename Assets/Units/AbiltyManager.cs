using Mirror;
using System.Collections.Generic;
using UnityEngine;
using static UnitControl;
using System.Linq;
using static GenerateAttack;

public class AbiltyManager : NetworkBehaviour
{

    Dictionary<int, Ability> instancedAbilitites = new Dictionary<int, Ability>();
    Dictionary<ItemSlot, int> slotLookups = new Dictionary<ItemSlot, int>();

    private void Start()
    {

    }
    public void addAbility(Dictionary<ItemSlot, AttackBlock> blocks)
    {
        foreach ((ItemSlot slot, AttackBlock block) in blocks)
        {
            instanceAbility(slot, block);
        }

    }
    public void addAbility(List<AttackBlock> blocks)
    {
        for (int i = 0; i < blocks.Count; i++)
        {
            ItemSlot slotFake = (ItemSlot)i;
            AttackBlock block = blocks[i];

            instanceAbility(slotFake, block);
        }


    }
    public Ability addTriggeredAbility(AttackBlock block, float strength)
    {
        return instanceTriggeredAbility(block, strength);

    }
    void instanceAbility(ItemSlot key, AttackBlock block)
    {
        GameObject o = Instantiate(FindObjectOfType<GlobalPrefab>().AbilityRootPre, transform);
        Ability a = o.GetComponent<Ability>();
        a.setFormat(block);
        registerAbility(key, a);
        a.clientSyncKey = key;
        o.GetComponent<ClientAdoption>().parent = gameObject;
        NetworkServer.Spawn(o);
    }
    Ability instanceTriggeredAbility(AttackBlock block, float strength)
    {
        GameObject o = Instantiate(FindObjectOfType<GlobalPrefab>().AbilityRootPre, transform);
        Ability a = o.GetComponent<Ability>();
        a.setFormat(block, strength);
        instancedAbilitites.Add(a.GetInstanceID(), a);
        o.GetComponent<ClientAdoption>().parent = gameObject;
        NetworkServer.Spawn(o);
        return a;
    }

    public void registerAbility(ItemSlot k, Ability a)
    {
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
