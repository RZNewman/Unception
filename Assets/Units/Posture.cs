using Mirror;
using UnityEngine;

public class Posture : NetworkBehaviour, BarValue
{
    [SyncVar]
    float maxPosture;
    [SyncVar]
    float currentPosture;


    float passivePostureRecover;
    float stunnedPostureRecover;

    float recentlyStunnedTime;





    [SyncVar]
    bool stunned = false;
    [SyncVar]
    float currentStunHighestPosture;

    UnitProperties props;
    Power power;
    Combat combat;
    Mezmerize mezmerize;
    public bool isStunned
    {
        get
        {
            return stunned;
        }
    }

    public float remainingToStun
    {
        get
        {
            return isStunned ? stunThreshold : stunThreshold - currentPosture;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        props = GetComponent<UnitPropsHolder>().props;
        power = GetComponent<Power>();
        combat = GetComponent<Combat>();
        mezmerize = GetComponent<Mezmerize>();

        currentPosture = 0;
        maxPosture = props.maxPosture;
        recentlyStunnedTime = 0;
        if (isServer)
        {
            GetComponent<Power>().subscribePower(updateMaxPosture);
        }

    }
    void updateMaxPosture(Power p)
    {
        float lastMax = maxPosture;

        float scaleNum = p.scaleNumerical();

        maxPosture = props.maxPosture * scaleNum;
        passivePostureRecover = props.maxPosture * scaleNum;
        stunnedPostureRecover = props.maxPosture * scaleNum * 2;


        float proportion = maxPosture / lastMax;

        currentPosture *= proportion;
        currentStunHighestPosture *= proportion;
    }


    public void takeStagger(float damage)
    {
        currentPosture += damage;

        if (stunned)
        {
            if (currentPosture > currentStunHighestPosture)
            {
                currentStunHighestPosture = currentPosture;
            }
        }

    }
    float fallOffMult
    {
        get
        {
            float scaleTime = power.scaleTime();
            return (1 + (recentlyStunnedTime / scaleTime) / 3);
        }
    }

    float stunThreshold
    {
        get
        {
            return maxPosture * fallOffMult;
        }

    }

    // Update is called once per frame
    public void OrderedUpdate()
    {
        float scaleTime = power.scaleTime();
        if (!combat.inCombat)
        {
            currentPosture = 0;
        }
        if (!stunned && currentPosture > stunThreshold)
        {
            stunned = true;
            currentStunHighestPosture = currentPosture;
        }

        if (mezmerize.isMezmerized)
        {
            //stun lingers while mezmerized
            return;
        }

        float currentPostureRecover;
        if (stunned)
        {
            currentPostureRecover = stunnedPostureRecover * fallOffMult;
            recentlyStunnedTime += Time.fixedDeltaTime * scaleTime;
        }
        else
        {
            currentPostureRecover = passivePostureRecover;
            recentlyStunnedTime -= Time.fixedDeltaTime * (0.2f) * scaleTime;
            recentlyStunnedTime = Mathf.Max(recentlyStunnedTime, 0);
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
            denom = stunThreshold;
        }
        return new BarValue.BarData
        {
            color = stunned ? GameColors.Stunned : Color.yellow,
            fillPercent = Mathf.Clamp01(currentPosture / denom),
            active = true,
        };
    }

}

