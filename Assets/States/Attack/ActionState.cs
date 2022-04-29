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
			if(o.GetComponentInParent<TeamOwnership>().getTeam()!= controller.GetComponentInParent<TeamOwnership>().getTeam())
            {
				Health h = o.GetComponentInParent<Health>();
				h.takeDamage(attackData.damage);
                switch (attackData.knockBackType)
                {
					case AttackData.KnockBackType.inDirection:
						o.GetComponentInParent<UnitMovement>().applyForce(attackData.knockback* controller.getSpawnBody().transform.forward);
						break;
					case AttackData.KnockBackType.fromCenter:
						Vector3 dir = o.transform.position - controller.transform.position;
						dir.y = 0;
						dir.Normalize();
						o.GetComponentInParent<UnitMovement>().applyForce(attackData.knockback * dir);
						break;
				}
			}
			
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
