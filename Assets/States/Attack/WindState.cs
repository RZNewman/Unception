using Mirror;
using UnityEngine;
using static GenerateAttack;
using static GenerateHit;
using static GenerateWind;
using static GenerateDash;
using System.Collections.Generic;

public class WindState : AttackState
{
    List<GameObject> indicators = new List<GameObject>();
    List<InstanceDataEffect> previewData;
    bool hasIndicator = false;

    public struct IndicatorOffsets
    {
        public float time;
        public Vector3 distance;

        public IndicatorOffsets sum(IndicatorOffsets b)
        {
            return new IndicatorOffsets
            {
                time = time + b.time,
                distance = distance + b.distance,
            };
        }
    }

    public WindState(UnitMovement m, float d) : base(m, d)
    {
        //Only for defaults
        hasIndicator = false;
    }

    public WindState(UnitMovement m, WindInstanceData d) : base(m, d.duration)
    {
        hasIndicator = false;
        moveMultiplier = d.moveMult;
        lookMultiplier = d.turnMult;
    }

    public WindState(UnitMovement m, WindInstanceData d, List<InstanceDataEffect> previews) : base(m, d.duration)
    {
        previewData = previews;
        hasIndicator = previews.Count > 0;
        moveMultiplier = d.moveMult;
        lookMultiplier = d.turnMult;
    }
    public override void enter()
    {
        GameObject target = mover.getSpawnBody();
        target.GetComponentInParent<Cast>().setTarget(this);
        if (hasIndicator)
        {

            buildIndicator(target, mover.GetComponent<Power>().scale());

        }

    }

    void buildIndicator(GameObject target, float scale)
    {
        IndicatorOffsets offsets = new IndicatorOffsets
        {
            distance = Vector2.zero,
            time = 0,
        };
        foreach (InstanceDataEffect preview in previewData)
        {
            IndicatorInstance i;
            GameObject indicator;
            switch (preview)
            {
                case HitInstanceData attackData:
                    indicator = Object.Instantiate(
                    Resources.Load("Indicator/LineIndicator") as GameObject,
                        target.transform
                    );
                    LineIndicatorVisuals l = indicator.GetComponent<LineIndicatorVisuals>();
                    l.setPosition(attackData);
                    i = l;
                    break;
                case DashInstanceData dashData:
                    indicator = Object.Instantiate(
                    Resources.Load("Indicator/DashIndicator") as GameObject,
                        target.transform
                    );
                    DashIndicatorVisuals d = indicator.GetComponent<DashIndicatorVisuals>();
                    d.setPosition(dashData, scale);
                    i = d;
                    break;
                default:
                    throw new System.Exception("Indicator not assigned!");
            }
            i.setTeam(mover.GetComponent<TeamOwnership>().getTeam());
            i.setLocalOffset(offsets.distance);
            i.setTime(currentDurration + offsets.time);
            ClientAdoption adoptee = indicator.GetComponent<ClientAdoption>();
            adoptee.parent = target.GetComponentInParent<NetworkIdentity>().gameObject;
            adoptee.useSubBody = true;
            NetworkServer.Spawn(indicator);
            indicators.Add(indicator);
            offsets = offsets.sum(preview.GetIndicatorOffsets());
        }




    }

    public override void exit(bool expired)
    {
        GameObject target = mover.getSpawnBody();
        target.GetComponentInParent<Cast>().removeTarget();
        if (hasIndicator)
        {
            foreach (GameObject indicator in indicators)
            {
                Object.Destroy(indicator);
            }

        }
    }

    public BarValue.BarData getProgress()
    {
        return new BarValue.BarData
        {
            color = hasIndicator ? Color.cyan : new Color(0, 0.6f, 1),
            fillPercent = Mathf.Clamp01(hasIndicator ? 1 - (currentDurration / maxDuration) : currentDurration / maxDuration),
            active = true,
        };
    }
}
