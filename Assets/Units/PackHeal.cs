using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static LifeManager;

public class PackHeal : NetworkBehaviour, BarValue
{
    public float percentHealKiller = 0;
    Combat combat;

    float currentHealPercent = 0;

    private void Start()
    {
        combat = GetComponent<Combat>();
    }


    public void rewardKill(float percentHealReaped)
    {
        currentHealPercent += percentHealReaped;
    }

    public void OrderedUpdate()
    {
        if (!isServer)
        {
            return;
        }
        if (!GetComponent<LifeManager>().IsDead)
        {
            float percentPerFrame = 0.1f * Time.fixedDeltaTime;
            float percentThisFrame = Mathf.Min(percentPerFrame, currentHealPercent);
            currentHealPercent -= percentThisFrame;
            if (!combat.inCombat)
            {
                GetComponent<Health>().healPercent(percentPerFrame * 0.8f);
            }
            else if (percentThisFrame > 0)
            {
                GetComponent<Health>().healPercent(percentThisFrame * 0.8f);
            }

        }



    }

    public BarValue.BarData getBarFill()
    {
        return new BarValue.BarData
        {
            color = new Color(1, 0.5f, 0.5f),
            fillPercent = Mathf.Clamp01(currentHealPercent),
            active = currentHealPercent > 0,
        };
    }
}
