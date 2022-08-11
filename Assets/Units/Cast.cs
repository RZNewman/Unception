using Mirror;
using System.Collections.Generic;
using UnityEngine;
using static GenerateAttack;
using static GenerateDash;
using static GenerateHit;

public class Cast : MonoBehaviour, BarValue
{
    BarValue target;
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
        //TODO cast bar client
        return target.getBarFill();
    }

    public void setTarget(WindState s)
    {
        target = s;
    }
    public void removeTarget()
    {
        target = null;
    }


    public void ServerUpdate()
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
                ind.ServerUpdate();
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
    public void buildIndicator(List<AttackStageState> stages)
    {

        GameObject indicatorBody = GetComponent<UnitMovement>().getSpawnBody();
        float scale = GetComponent<Power>().scale();
        foreach (AttackStageState stage in stages)
        {
            IndicatorInstance i = null;
            GameObject indicator = null;
            switch (stage)
            {
                case ActionState attackData:
                    indicator = Object.Instantiate(
                    Resources.Load("Indicator/LineIndicator") as GameObject,
                        indicatorBody.transform
                    );
                    LineIndicatorVisuals l = indicator.GetComponent<LineIndicatorVisuals>();
                    l.setSource(attackData);
                    i = l;
                    break;
                case DashState dashData:
                    indicator = Object.Instantiate(
                    Resources.Load("Indicator/DashIndicator") as GameObject,
                        indicatorBody.transform
                    );
                    DashIndicatorVisuals d = indicator.GetComponent<DashIndicatorVisuals>();
                    d.setSource(dashData, scale);
                    i = d;
                    break;

            }
            if (indicator && i)
            {
                i.setTeam(GetComponent<TeamOwnership>().getTeam());
                ClientAdoption adoptee = indicator.GetComponent<ClientAdoption>();
                adoptee.parent = gameObject;
                adoptee.useSubBody = true;
                NetworkServer.Spawn(indicator);
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
