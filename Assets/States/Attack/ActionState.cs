using System.Collections.Generic;
using UnityEngine;
using static GenerateAttack;
using static GenerateHit;
using static UnitControl;
using static AttackUtils;
using static GenerateBuff;

public class ActionState : AttackStageState
{
    HitInstanceData actionData;

    BuffInstanceData buffData;

    GameObject groundTarget;
    public ActionState(UnitMovement m, HitInstanceData data) : base(m)
    {
        actionData = data;
    }

    public ActionState(UnitMovement m, HitInstanceData data, BuffInstanceData dataB) : base(m)
    {
        actionData = data;
        buffData = dataB;
    }
    public override void enter()
    {
        mover.GetComponent<AnimationController>().setAttack();

        handleAttack(actionData);


    }

    void handleAttack(HitInstanceData attackData)
    {
        GameObject body = mover.getSpawnBody();
        FloorNormal floorNormal = mover.GetComponent<FloorNormal>();
        Size s = body.GetComponentInChildren<Size>();
        List<GameObject> hits;
        switch (attackData.type)
        {
            case HitType.Line:
                LineInfo info = LineCalculations(floorNormal, body.transform, s.scaledRadius, s.scaledHalfHeight, attackData.range, attackData.length, attackData.width);
                LineParticle(info, attackData.flair, mover.sound.dists);
                if (!mover.isServer)
                {
                    return;
                }
                hits = LineAttack(info);
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
                if (!mover.isServer)
                {
                    return;
                }
                SpawnProjectile(floorNormal, body.transform, s.scaledRadius, s.scaledHalfHeight, mover, attackData, mover.sound.dists);
                break;
            case HitType.Ground:
                float radius = attackData.width / 2;
                Quaternion aim = Quaternion.LookRotation(groundTarget.transform.forward, groundTarget.GetComponent<FloorNormal>().normal);
                GroundParticle(groundTarget.transform.position, radius, aim, attackData.flair, mover.sound.dists);
                if (!mover.isServer)
                {
                    return;
                }
                hits = GroundAttack(groundTarget.transform.position, radius);
                foreach (GameObject o in hits)
                {
                    hit(o, mover, attackData,
                        mover.GetComponent<TeamOwnership>().getTeam(),
                        mover.GetComponent<Power>().power,
                        new KnockBackVectors
                        {
                            center = groundTarget.transform.position,
                            direction = groundTarget.transform.forward
                        });

                }
                break;

        }

        if (buffData != null)
        {
            SpawnBuff(buffData, mover.transform);
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
        return actionData;
    }
    public override void tick()
    {
        base.tick();

        Debug.LogError("Unreachable - Should transition");
        //UnitInput inp = mover.input;


        //mover.rotate(inp, false, lookMultiplier);
        //mover.move(inp, moveMultiplier);


    }

    public override StateTransition transition()
    {
        return new StateTransition(null, true);
    }


    public void setGroundTarget(GameObject t)
    {
        groundTarget = t;
    }
}
