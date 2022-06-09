using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionState : AttackState
{
	AttackData attackData;
	public ActionState(UnitMovement m, AttackData data) : base(m)
	{
		attackData = data;
	}
	public override void enter()
	{
		GameObject body = mover.getSpawnBody();
		Size s =  body.GetComponentInChildren<Size>();
		List<GameObject> hits = InstantHit.LineAttack(body.transform, s.scaledRadius,s.scaledHalfHeight, attackData.length, attackData.width);
		foreach(GameObject o in hits)
        {
			if(o.GetComponentInParent<TeamOwnership>().getTeam()!= mover.GetComponent<TeamOwnership>().getTeam())
            {
				Health h = o.GetComponentInParent<Health>();
				o.GetComponentInParent<Combat>().getHit(mover.gameObject);
				h.takeDamage(attackData.damageMult * mover.GetComponent<Power>().power);
				o.GetComponentInParent<Posture>().takeStagger(attackData.stagger);
                switch (attackData.knockBackType)
                {
					case AttackData.KnockBackType.inDirection:
						o.GetComponentInParent<UnitMovement>().applyForce(attackData.knockback* mover.getSpawnBody().transform.forward);
						break;
					case AttackData.KnockBackType.fromCenter:
						Vector3 dir = o.transform.position - mover.transform.position;
						dir.y = 0;
						dir.Normalize();
						o.GetComponentInParent<UnitMovement>().applyForce(attackData.knockback * dir);
						break;
				}
			}
			
        }
	}

	public override StateTransition transition()
	{
		return new StateTransition(null, true);
	}

}
