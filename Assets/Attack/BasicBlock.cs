using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicBlock : AttackBlock
{
	public float windup;
	public float winddown;
	public AttackData data;

	public override List<AttackState> buildStates(Ability controller)
	{
		List<AttackState> states = new List<AttackState>();

		states.Add(new WindState(controller, windup, data));
		states.Add(new ActionState(controller, data));
		states.Add(new WindState(controller,winddown));
		return states;
	}
}
