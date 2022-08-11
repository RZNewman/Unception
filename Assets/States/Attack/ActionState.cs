using System.Collections.Generic;
using UnityEngine;
using static GenerateAttack;
using static GenerateHit;
using static UnitControl;

public class ActionState : AttackStageState
{
    HitInstanceData attackData;
    public ActionState(UnitMovement m, HitInstanceData data) : base(m)
    {
        attackData = data;
    }
    public override void enter()
    {
        GameObject body = mover.getSpawnBody();
        Size s = body.GetComponentInChildren<Size>();
        List<GameObject> hits = InstantHit.LineAttack(body.transform, s.scaledRadius, s.scaledHalfHeight, attackData.length, attackData.width);
        foreach (GameObject o in hits)
        {
            if (o.GetComponentInParent<TeamOwnership>().getTeam() != mover.GetComponent<TeamOwnership>().getTeam())
            {
                Health h = o.GetComponentInParent<Health>();
                o.GetComponentInParent<Combat>().getHit(mover.gameObject);
                h.takeDamage(attackData.damageMult * mover.GetComponent<Power>().power);
                o.GetComponentInParent<Posture>().takeStagger(attackData.stagger);
                switch (attackData.knockBackType)
                {
                    case KnockBackType.inDirection:
                        o.GetComponentInParent<UnitMovement>().applyForce(attackData.knockback * mover.getSpawnBody().transform.forward);
                        break;
                    case KnockBackType.fromCenter:
                        Vector3 dir = o.transform.position - mover.transform.position;
                        dir.y = 0;
                        dir.Normalize();
                        o.GetComponentInParent<UnitMovement>().applyForce(attackData.knockback * dir);
                        break;
                }
                o.GetComponentInParent<UnitMovement>().applyForce(attackData.knockUp * Vector3.up);
            }

        }
    }

    public override Cast.IndicatorOffsets GetIndicatorOffsets()
    {
        return new Cast.IndicatorOffsets
        {
            distance = Vector3.zero,
            time = 0,
        };
    }

    public HitInstanceData getSource()
    {
        return attackData;
    }
    public override void tick()
    {
        base.tick();
        UnitInput inp = mover.input;


        mover.rotate(inp, lookMultiplier);
        mover.move(inp, moveMultiplier, moveMultiplier);


    }

    public override StateTransition transition()
    {
        return new StateTransition(null, true);
    }

}
