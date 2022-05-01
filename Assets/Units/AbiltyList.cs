using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnitControl;

public class AbiltyList : MonoBehaviour
{
	public GameObject AbilityRootPre;


	List<AttackBlock> abilitiesToCreate;
	Dictionary<AttackKey, GameObject> instancedAbilitites;

	private void Start()
	{
		instancedAbilitites = new Dictionary<AttackKey, GameObject>();
		abilitiesToCreate = GetComponent<UnitPropsHolder>().props.abilitiesToCreate;
		defaultAbilities();
	}
	void defaultAbilities()
	{
		for(int i=0; i < abilitiesToCreate.Count; i++)
		{
			GameObject o = Instantiate(AbilityRootPre, transform);
			o.GetComponent<Ability>().setFormat(abilitiesToCreate[i]);
			AttackKey k = (AttackKey)i;
			instancedAbilitites.Add(k, o);
		}
	}


	public GameObject getAbility(AttackKey key)
	{
        return instancedAbilitites[key];
	}
}
