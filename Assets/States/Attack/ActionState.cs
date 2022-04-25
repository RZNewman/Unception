using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionState : AttackState
{
	AttackData attackData;
	public ActionState(AttackController c, AttackData data) : base(c)
	{
		attackData = data;
	}
	public override void enter()
	{
		
		List<GameObject> hits = InstantHit.LineAttack(controller.getSpawnBody().transform, 0.5f, attackData.length, attackData.width);
		foreach(GameObject o in hits)
        {
			Debug.Log(o);
        }
	}

	public override void exit(bool expired)
	{
		
	}

	public override StateTransition transition()
	{
		return new StateTransition(null, true);
	}

}
