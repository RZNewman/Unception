using Mirror;
using System.Collections.Generic;
using UnityEngine;
using static AttackUtils;
using static GenerateAttack;
using static GenerateDash;
using static GenerateHit;

public class Cast : MonoBehaviour, BarValue
{
    BarValue target;

    GlobalPrefab global;

    private void Start()
    {
        global = FindObjectOfType<GlobalPrefab>();
    }
    public BarValue.BarData getBarFill()
    {
        if (target == null)
        {
            return new BarValue.BarData
            {
                color = Color.cyan,
                fillPercent = 0,
                active = false,
            };
        }
        return target.getBarFill();
    }

    public void setTarget(BarValue s)
    {
        target = s;
    }
    public void removeTarget(BarValue s)
    {
        if (target == s)
        {
            target = null;
        }

    }


    public void OrderedUpdate()
    {
        IndicatorOffsets offsets = new IndicatorOffsets
        {
            distance = Vector2.zero,
            time = 0,
        };
        for (int i = 0; i < indicators.Count; i++)
        {
            (AttackStageState stage, GameObject indObj) = indicators[i];
            if (indObj)
            {
                IndicatorInstance ind = indObj.GetComponent<IndicatorInstance>();
                ind.setLocalOffsets(offsets);
                ind.OrderedUpdate();
            }
            offsets = offsets.sum(stage.GetIndicatorOffsets());
        }

    }
    public struct IndicatorOffsets
    {
        public float time;
        public Vector3 distance;

        public IndicatorOffsets sum(IndicatorOffsets b)
        {
            return new IndicatorOffsets
            {
                time = this.time + b.time,
                distance = this.distance + b.distance,
            };
        }
    }

    List<(AttackStageState, GameObject)> indicators = new List<(AttackStageState, GameObject)>();
    public void buildIndicator(List<AttackStageState> states, AttackSegment segment)
    {
        GameObject indicatorBody = GetComponent<UnitMovement>().getSpawnBody();
        Power pow = GetComponent<Power>();
        foreach (AttackStageState stage in states)
        {
            IndicatorInstance i = null;
            GameObject indicator = null;
            switch (stage)
            {
                case ActionState attackData:
                    switch (attackData.getSource().type)
                    {
                        case HitType.Line:
                            indicator = Object.Instantiate(
                   global.LineIndPre,
                        indicatorBody.transform
                    );
                            LineIndicatorVisuals l = indicator.GetComponent<LineIndicatorVisuals>();
                            l.setSource(attackData);
                            i = l;
                            break;
                        case HitType.Projectile:
                            indicator = Object.Instantiate(
                   global.ProjIndPre,
                        indicatorBody.transform
                    );
                            ProjectileIndicatorVisuals p = indicator.GetComponent<ProjectileIndicatorVisuals>();
                            p.setSource(attackData);
                            i = p;
                            break;
                        case HitType.Ground:
                            indicator = Object.Instantiate(
                   global.GroundIndPre,
                        segment.groundTargetInstance.transform
                    );
                            GroundIndicatorVisuals g = indicator.GetComponent<GroundIndicatorVisuals>();
                            g.setSource(attackData);
                            i = g;
                            break;

                    }

                    break;
                case DashState dashData:
                    indicator = Object.Instantiate(
                    global.DashIndPre,
                        indicatorBody.transform
                    );
                    DashIndicatorVisuals d = indicator.GetComponent<DashIndicatorVisuals>();
                    d.setSource(dashData, pow);
                    i = d;
                    break;

            }
            if (indicator && i)
            {
                i.setTeam(GetComponent<TeamOwnership>().getTeam());
                //ClientAdoption adoptee = indicator.GetComponent<ClientAdoption>();
                //adoptee.parent = gameObject;
                //adoptee.useSubBody = true;
                //NetworkServer.Spawn(indicator);
            }

            indicators.Add((stage, indicator));

        }

    }
    public void nextStage()
    {
        if (indicators.Count > 0)
        {
            indicators.RemoveAt(0);
        }
        if (indicators.Count > 0)
        {
            GameObject o = indicators[0].Item2;
            if (o)
            {
                Destroy(o);
            }
        }

    }
    public void killIndicators()
    {
        foreach ((_, GameObject o) in indicators)
        {
            if (o)
            {
                Destroy(o);
            }
        }
        indicators.Clear();
    }

}
