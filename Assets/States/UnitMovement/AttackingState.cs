using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnitControl;

public class AttackingState : PlayerMovementState
{

	public AttackingState(UnitMovement m, AttackKey atk) : base(m)
	{
		Ability a = m.GetComponent<AbiltyList>().getAbility(atk).GetComponent<Ability>();
	}

	public override void enter()
	{
		throw new System.NotImplementedException();
	}

	public override void exit(bool expired)
	{
		throw new System.NotImplementedException();
	}

}
