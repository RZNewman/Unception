using System.Collections.Generic;
using UnityEngine;
using static GenerateAttack;
using static GenerateHit;
using static UnitControl;
using static AttackUtils;

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
        switch (attackData.type)
        {
            case HitType.Line:


                List<GameObject> hits = LineAttack(body.transform, s.scaledRadius, s.scaledHalfHeight, attackData.length, attackData.width);
                foreach (GameObject o in hits)
                {
                    hit(o, mover, attackData,
                        mover.GetComponent<TeamOwnership>().getTeam(),
                        mover.GetComponent<Power>().power,
                        new KnockBackVectors
                        {
                            center = mover.transform.position,
                            direction = mover.getSpawnBody().transform.forward
                        });

                }
                break;
            case HitType.Projectile:
                SpawnProjectile(body.transform, s.scaledRadius, s.scaledHalfHeight, mover, attackData);
                break;
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
