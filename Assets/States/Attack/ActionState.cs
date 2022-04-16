using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionState : AttackState
{
	GameObject attack;
	public ActionState(AttackController c) : base(c)
	{
	}
	public override void enter()
	{
		attack = Object.Instantiate(Resources.Load("AttackLine") as GameObject, controller.getSpawnBody().transform);
	}

	public override void exit(bool expired)
	{
		//Object.Destroy(attack);
	}

	public override StateTransition transition()
	{
		return new StateTransition(null, true);
	}

}
