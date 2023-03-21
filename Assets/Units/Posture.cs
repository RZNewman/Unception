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
            return isStunned ? maxPosture : maxPosture - currentPosture;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        props = GetComponent<UnitPropsHolder>().props;
        power = GetComponent<Power>();

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
        passivePostureRecover = props.passivePostureRecover * scaleNum;
        stunnedPostureRecover = props.stunnedPostureRecover * scaleNum;


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

    // Update is called once per frame
    public void OrderedUpdate()
    {
        float scaleTime = power.scaleTime();
        if (!stunned && currentPosture > maxPosture)
        {
            stunned = true;
            currentStunHighestPosture = currentPosture;
        }
        float currentPostureRecover;
        if (stunned)
        {
            currentPostureRecover = stunnedPostureRecover * (1 + (recentlyStunnedTime / scaleTime) / 2);
            recentlyStunnedTime += Time.fixedDeltaTime;
        }
        else
        {
            currentPostureRecover = passivePostureRecover;
            recentlyStunnedTime -= Time.fixedDeltaTime * (0.2f);
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
            denom = maxPosture;
        }
        return new BarValue.BarData
        {
            color = stunned ? GameColors.Stunned : Color.yellow,
            fillPercent = Mathf.Clamp01(currentPosture / denom),
            active = true,
        };
    }

}

