using Mirror;
using System.Collections.Generic;
using UnityEngine;
using static UnitControl;
using System.Linq;
using static GenerateAttack;

public class AbiltyList : NetworkBehaviour
{


    Dictionary<ItemSlot, AttackBlock> abilitiesToCreate = new Dictionary<ItemSlot, AttackBlock>();
    Dictionary<ItemSlot, Ability> instancedAbilitites = new Dictionary<ItemSlot, Ability>();

    bool started = false;
    private void Start()
    {
        if (isServer)
        {
            createAbilities();
            started = true;
        }

    }
    void createAbilities()
    {
        foreach ((ItemSlot key, AttackBlock block) in abilitiesToCreate)
        {
            instanceAbility(key, block);
        }
    }
    void instanceAbility(ItemSlot key, AttackBlock block)
    {
        GameObject o = Instantiate(FindObjectOfType<GlobalPrefab>().AbilityRootPre, transform);
        Ability a = o.GetComponent<Ability>();
        a.setFormat(block);
        instancedAbilitites.Add(key, a);
        a.clientSyncKey = key;
        o.GetComponent<ClientAdoption>().parent = gameObject;
        NetworkServer.Spawn(o);
    }
    [Client]
    public void registerAbility(ItemSlot k, Ability a)
    {
        instancedAbilitites.Add(k, a);
    }
    public void addAbility(Dictionary<ItemSlot, AttackBlock> blocks)
    {
        if (started)
        {
            foreach ((ItemSlot slot, AttackBlock block) in blocks)
            {
                instanceAbility(slot, block);
            }

        }
        else
        {
            foreach ((ItemSlot slot, AttackBlock block) in blocks)
            {
                abilitiesToCreate.Add(slot, block);
            }

        }

    }
    public void addAbility(List<AttackBlock> blocks)
    {
        for (int i = 0; i < blocks.Count; i++)
        {
            ItemSlot slotFake = (ItemSlot)i;
            AttackBlock block = blocks[i];
            if (started)
            {
                instanceAbility(slotFake, block);
            }
            else
            {
                abilitiesToCreate.Add(slotFake, block);
            }
        }


    }

    public Ability getAbility(ItemSlot key)
    {
        return instancedAbilitites.ContainsKey(key) ? instancedAbilitites[key] : null;
    }

    public struct AbilityPair
    {
        public ItemSlot key;
        public Ability ability;
    }
    public AbilityPair getBestAbility()
    {
        return instancedAbilitites.Keys
            .Select(k => new AbilityPair { key = k, ability = instancedAbilitites[k] })
            .Where(p => p.ability.ready)
            .OrderBy(p => p.ability.cooldownPerCharge).Reverse()
            .First();
    }

}
