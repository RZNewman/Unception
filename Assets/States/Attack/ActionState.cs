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
		//attack = Object.Instantiate(Resources.Load("AttackLine") as GameObject, controller.getSpawnBody().transform);
		List<GameObject> hits = InstantHit.LineAttack(controller.getSpawnBody().transform, 0.5f, 2f, 1f);
		foreach(GameObject o in hits)
        {
			//Debug.Log(o);
        }
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
