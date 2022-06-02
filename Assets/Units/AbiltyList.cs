using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnitControl;

public class AbiltyList : NetworkBehaviour
{
	public GameObject AbilityRootPre;


	public List<AttackBlock> abilitiesToCreate;
	Dictionary<AttackKey, Ability> instancedAbilitites;

	private void Start()
	{
        if (isServer)
        {
			instancedAbilitites = new Dictionary<AttackKey, Ability>();
			createAbilities();
		}
		
	}
	void createAbilities()
	{
		for(int i=0; i < abilitiesToCreate.Count; i++)
		{
			GameObject o = Instantiate(AbilityRootPre, transform);
			Ability a = o.GetComponent<Ability>();
			a.setFormat(abilitiesToCreate[i]);
			AttackKey k = (AttackKey)i;
			instancedAbilitites.Add(k, a);
			o.GetComponent<ClientAdoption>().parent = gameObject;
			NetworkServer.Spawn(o);
		}
	}
	public void addAbility(AttackBlock block)
    {
		abilitiesToCreate.Add(block);
    }
	public void addAbility(List<AttackBlock> blocks)
	{
		abilitiesToCreate.AddRange(blocks);
	}
	public void clear()
    {
		abilitiesToCreate = new List<AttackBlock>();
    }

	public Ability getAbility(AttackKey key)
	{
        return instancedAbilitites[key];
	}
}
