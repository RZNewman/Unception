using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicBlock : AttackBlock
{
	public float windup;
	public float winddown;

	public override List<AttackState> buildStates(AttackController controller)
	{
		List<AttackState> states = new List<AttackState>();
		states.Add(new WindState(controller, windup));
		states.Add(new ActionState(controller));
		states.Add(new WindState(controller,winddown));
		return states;
	}
}
