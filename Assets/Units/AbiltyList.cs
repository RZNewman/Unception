using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnitControl;

public class AbiltyList : MonoBehaviour
{
	public GameObject AbilityRootPre;


	List<AttackBlock> abilitiesToCreate;
	Dictionary<AttackKey, Ability> instancedAbilitites;

	private void Start()
	{
		instancedAbilitites = new Dictionary<AttackKey, Ability>();
		abilitiesToCreate = GetComponent<UnitPropsHolder>().props.abilitiesToCreate;
		defaultAbilities();
	}
	void defaultAbilities()
	{
		for(int i=0; i < abilitiesToCreate.Count; i++)
		{
			GameObject o = Instantiate(AbilityRootPre, transform);
			Ability a = o.GetComponent<Ability>();
			a.setFormat(abilitiesToCreate[i]);
			AttackKey k = (AttackKey)i;
			instancedAbilitites.Add(k, a);
		}
	}


	public Ability getAbility(AttackKey key)
	{
        return instancedAbilitites[key];
	}
}
