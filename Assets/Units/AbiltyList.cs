using Mirror;
using System.Collections.Generic;
using UnityEngine;
using static UnitControl;

public class AbiltyList : NetworkBehaviour
{
    public GameObject AbilityRootPre;


    List<AttackBlock> abilitiesToCreate = new List<AttackBlock>();
    Dictionary<AttackKey, Ability> instancedAbilitites;

    bool started = false;
    private void Start()
    {
        if (isServer)
        {
            instancedAbilitites = new Dictionary<AttackKey, Ability>();
            createAbilities();
            started = true;
        }

    }
    void createAbilities()
    {
        for (int i = 0; i < abilitiesToCreate.Count; i++)
        {
            instanceAbility(abilitiesToCreate[i]);
        }
    }
    void instanceAbility(AttackBlock block)
    {
        GameObject o = Instantiate(AbilityRootPre, transform);
        Ability a = o.GetComponent<Ability>();
        a.setFormat(block);
        AttackKey k = (AttackKey)instancedAbilitites.Count;
        instancedAbilitites.Add(k, a);
        o.GetComponent<ClientAdoption>().parent = gameObject;
        NetworkServer.Spawn(o);
    }
    public void addAbility(AttackBlock block)
    {
        if (started)
        {
            instanceAbility(block);
        }
        else
        {
            abilitiesToCreate.Add(block);
        }

    }
    public void addAbility(List<AttackBlock> blocks)
    {
        if (started)
        {
            foreach (AttackBlock block in blocks)
            {
                instanceAbility(block);
            }

        }
        else
        {
            abilitiesToCreate.AddRange(blocks);
        }

    }

    public Ability getAbility(AttackKey key)
    {
        return instancedAbilitites[key];
    }
}
