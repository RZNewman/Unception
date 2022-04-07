using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicBlock : AttackBlock
{
	public float windup;
	public float winddown;

	public override List<AttackState> buildStates()
	{
		List<AttackState> states = new List<AttackState>();
		states.Add(new WindState(windup));
		states.Add(new ActionState());
		states.Add(new WindState(winddown));
		return states;
	}
}
