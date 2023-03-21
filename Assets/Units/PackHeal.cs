using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static LifeManager;
using static NSubstitute.Arg;

public class PackHeal : NetworkBehaviour, BarValue
{
    List<float> packTargets = new List<float>();
    public float packPool = 0;
    Combat combat;

    float currentPackProgress = 0;

    private void Start()
    {
        combat = GetComponent<Combat>();
    }
    public void addPack(float pool)
    {
        packTargets.Add(pool);
    }

    public void rewardKill(float poolPoints)
    {
        currentPackProgress += poolPoints;
    }

    public void OrderedUpdate()
    {
        if (!isServer)
        {
            return;
        }
        if (!GetComponent<LifeManager>().IsDead)
        {
            if (!combat.inCombat)
            {
                packTargets.Clear();
                currentPackProgress = 0;
            }
            if (packTargets.Count > 0 && currentPackProgress >= packTargets[0] * 0.999f)
            {
                GetComponent<Health>().healToFull();
                currentPackProgress -= packTargets[0];
                packTargets.RemoveAt(0);


            }
        }



    }

    public BarValue.BarData getBarFill()
    {
        return new BarValue.BarData
        {
            color = new Color(1, 0.5f, 0.5f),
            fillPercent = packTargets.Count > 0 ? Mathf.Clamp01(currentPackProgress / packTargets[0]) : 0,
            active = packTargets.Count > 0,
        };
    }
}
