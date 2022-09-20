using Mirror;
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

    float stunnedPostureCeilingAcceleration;
    float postureCeilingRecover;



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

    // Start is called before the first frame update
    void Start()
    {
        props = GetComponent<UnitPropsHolder>().props;


        currentPosture = 0;
        maxPostureBase = props.maxPosture;
        currentPostureCeiling = props.maxPosture;
        if (isServer)
        {
            GetComponent<Power>().subscribePower(updateMaxPosture);
        }

    }
    void updateMaxPosture(Power p)
    {
        float lastMax = maxPostureBase;


        maxPostureBase = props.maxPosture * p.scale();
        passivePostureRecover = props.passivePostureRecover * p.scale();
        stunnedPostureRecover = props.stunnedPostureRecover * p.scale();
        stunnedPostureCeilingAcceleration = props.stunnedPostureCeilingAcceleration * p.scale();
        postureCeilingRecover = stunnedPostureCeilingAcceleration * 0.5f;

        float proportion = maxPostureBase / lastMax;

        currentPosture *= proportion;
        currentPostureCeiling *= proportion;
        currentStunHighestPosture *= proportion;
    }


    public void takeStagger(float damage)
    {
        currentPosture += damage;
        if (!stunned && currentPosture > currentPostureCeiling)
        {
            stunned = true;
            currentStunHighestPosture = currentPosture;
        }
        if (stunned)
        {
            if (currentPosture > currentStunHighestPosture)
            {
                currentStunHighestPosture = currentPosture;
            }
        }

    }

    // Update is called once per frame
    public void OrderedUpdate()
    {
        float currentPostureRecover;
        if (stunned)
        {
            currentPostureCeiling += stunnedPostureCeilingAcceleration * Time.fixedDeltaTime;
            currentPostureRecover = stunnedPostureRecover + (currentPostureCeiling - maxPostureBase) * 1.5f;
        }
        else
        {
            currentPostureCeiling -= postureCeilingRecover * Time.fixedDeltaTime;
            if (currentPostureCeiling < maxPostureBase)
            {
                currentPostureCeiling = maxPostureBase;
            }
            currentPostureRecover = passivePostureRecover;
        }

        //change posture
        if (currentPosture > 0)
        {
            currentPosture -= currentPostureRecover * Time.fixedDeltaTime;
        }
        if (stunned && currentPosture <= 0)
        {
            stunned = false;
        }
        if (currentPosture < 0)
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

