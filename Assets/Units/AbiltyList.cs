using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnitControl;

public class AbiltyList : MonoBehaviour
{
	public GameObject AbilityRootPre;
    public List<AttackBlock> abilitiesToCreate;

    Dictionary<AttackKey, GameObject> instancedAbilitites;

	private void Start()
	{
		defaultAbilities();
	}
	void defaultAbilities()
	{
		for(int i=0; i < abilitiesToCreate.Count; i++)
		{
			GameObject o = Instantiate(AbilityRootPre, transform);
			AttackKey k = (AttackKey)i;
			instancedAbilitites.Add(k, o);
		}
	}


	public GameObject getAbility(AttackKey key)
	{
        return instancedAbilitites[key];
	}
}
