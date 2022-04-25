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
		AttackData data = new AttackData();
		data.length = 3;
		data.width = 6;

		states.Add(new WindState(controller, windup, data));
		states.Add(new ActionState(controller, data));
		states.Add(new WindState(controller,winddown));
		return states;
	}
}
