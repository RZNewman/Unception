using Mirror;
using System.Collections.Generic;
using UnityEngine;
using static UnitControl;
using System.Linq;

public class AbiltyList : NetworkBehaviour
{


    Dictionary<AttackKey, AttackBlock> abilitiesToCreate = new Dictionary<AttackKey, AttackBlock>();
    Dictionary<AttackKey, Ability> instancedAbilitites = new Dictionary<AttackKey, Ability>();

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
        foreach ((AttackKey key, AttackBlock block) in abilitiesToCreate)
        {
            instanceAbility(key, block);
        }
    }
    void instanceAbility(AttackKey key, AttackBlock block)
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
    public void registerAbility(AttackKey k, Ability a)
    {
        instancedAbilitites.Add(k, a);
    }
    public void addAbility(Dictionary<AttackKey, AttackBlock> blocks)
    {
        if (started)
        {
            foreach ((AttackKey key, AttackBlock block) in blocks)
            {
                instanceAbility(key, block);
            }

        }
        else
        {
            foreach ((AttackKey key, AttackBlock block) in blocks)
            {
                abilitiesToCreate.Add(key, block);
            }

        }

    }
    public void addAbility(List<AttackBlock> blocks)
    {
        for (int i = 0; i < blocks.Count; i++)
        {
            AttackKey key = (AttackKey)i;
            AttackBlock block = blocks[i];
            if (started)
            {
                instanceAbility(key, block);
            }
            else
            {
                abilitiesToCreate.Add(key, block);
            }
        }


    }

    public Ability getAbility(AttackKey key)
    {
        return instancedAbilitites[key];
    }

    public struct AbilityPair
    {
        public AttackKey key;
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
