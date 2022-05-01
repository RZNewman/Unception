using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Posture : NetworkBehaviour, BarValue
{
    [SyncVar]
    public float maxPostureBase;
    [SyncVar]
    float currentPosture;
    [SyncVar]
    float currentPostureCeiling;

    public float passivePostureRecover;
    public float stunnedPostureRecover;
    public float stunnedPostureRecoverAcceleration;
    float currentPostureRecover;
    static float postureStunBufferPercent = 0.5f;
    static float postureCeilingRecoverPercent = 0.025f;
    bool stunned = false;

    float currentStunHighestPosture;

    public bool isStunned
    {
        get
        {
            return stunned;
        }
    }
    float postureBuffer
    {
        get
        {
            return maxPostureBase * postureStunBufferPercent;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        currentPosture = 0;
    }

    public void takeStagger(float damage)
    {
        currentPosture += damage;
        if(!stunned && currentPosture > currentPostureCeiling)
        {
            stunned = true;
            currentPostureCeiling = currentPosture + postureBuffer;
            currentPostureRecover = stunnedPostureRecover;
            currentStunHighestPosture = currentPosture;
        }
        if (stunned)
        {
            if(currentPosture> currentStunHighestPosture)
            {
                currentStunHighestPosture = currentPosture;
            }
        }

    }

    // Update is called once per frame
    public void ServerUpdate()
    {
        //Recover ciling limit based on max
        if(currentPostureCeiling > maxPostureBase)
        {
            currentPostureCeiling -= maxPostureBase * postureCeilingRecoverPercent * Time.fixedDeltaTime;
        }
        if(currentPostureCeiling < maxPostureBase)
        {
            currentPostureCeiling = maxPostureBase;
        }

        //set current recover b.o. stun
        if (stunned)
        {
            currentPostureRecover += stunnedPostureRecoverAcceleration *Time.fixedDeltaTime;
        }
        else
        {
            currentPostureRecover = passivePostureRecover;
        }

        //change posture
        if(currentPosture > 0)
        {
            currentPosture -= currentPostureRecover *Time.fixedDeltaTime;
        }
        if (stunned && currentPosture <=0)
        {
            stunned = false;
        }
        if(currentPosture < 0)
        {
            currentPosture = 0;
        }
    }

    public BarValue.BarData getBarFill()
    {
        float denom;
        if (stunned)
        {
            denom = currentStunHighestPosture;
        }
        else
        {
            denom = currentPostureCeiling;
        }
        return new BarValue.BarData
        {
            color = stunned ? new Color(1.0f,0.5f,0.0f): Color.yellow,
            fillPercent = Mathf.Clamp01(currentPosture/ denom),
        };
    }

}

