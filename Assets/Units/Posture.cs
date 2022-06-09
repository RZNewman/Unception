using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Posture : NetworkBehaviour, BarValue
{
    [SyncVar]
    float maxPostureBase;
    [SyncVar]
    float currentPosture;
    [SyncVar]
    float currentPostureCeiling;

    float passivePostureRecover;
    float stunnedPostureRecover;
    float stunnedPostureRecoverAcceleration;


    float currentPostureRecover;
    static float postureStunBufferPercent = 0.5f;
    static float postureCeilingRecoverPercent = 0.025f;
    [SyncVar]
    bool stunned = false;
    [SyncVar]
    float currentStunHighestPosture;

    UnitProperties props;
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
        props = GetComponent<UnitPropsHolder>().props;
       

        currentPosture = 0;
        maxPostureBase = props.maxPosture;
        currentPostureCeiling = props.maxPosture;
        
        GetComponent<Power>().subscribePower(updateMaxPosture);
    }
    void updateMaxPosture(Power p)
    {
        float lastMax = maxPostureBase;


        maxPostureBase = props.maxPosture * p.scale();
        passivePostureRecover = props.passivePostureRecover * p.scale();
        stunnedPostureRecover = props.stunnedPostureRecover * p.scale();
        stunnedPostureRecoverAcceleration = props.stunnedPostureRecoverAcceleration * p.scale();

        float proportion = maxPostureBase / lastMax;

        currentPosture *= proportion;
        currentPostureCeiling *= proportion;
        currentStunHighestPosture *= proportion;
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
        //Recover ceiling limit based on max
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
            color = stunned ? new Color(1.0f, 0.5f, 0.0f) : Color.yellow,
            fillPercent = Mathf.Clamp01(currentPosture / denom),
            active = true,
        };
    }

}

