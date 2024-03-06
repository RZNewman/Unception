using Mirror;
using UnityEngine;
using static EventManager;

public class Mezmerize : NetworkBehaviour, BarValue
{
    [SyncVar]
    float maxFocus;
    [SyncVar]
    float currentFocus;


    float passiveFocusRecover;
    float mezmerizeFocusRecover;





    [SyncVar]
    bool mezmerized = false;
    [SyncVar]
    float currentMezmerizeHighestFocus;

    UnitProperties props;
    Power power;
    Combat combat;
    public bool isMezmerized
    {
        get
        {
            return mezmerized;
        }
    }

    public float remainingToMezmerize
    {
        get
        {
            return isMezmerized ? mezmerizeThreshold : mezmerizeThreshold - currentFocus;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        props = GetComponent<UnitPropsHolder>().props;
        power = GetComponent<Power>();
        combat = GetComponent<Combat>();

        currentFocus = 0;
        maxFocus = props.maxFocus;

        if (isServer)
        {
            power.subscribePower(updateMaxFocus);
            GetComponent<EventManager>().HitEvent += getHit;
        }

    }

    void getHit(GetHitEventData data)
    {
        if (mezmerized)
        {
            float relativeStrength = data.powerByStrength / power.power;
            currentFocus -= maxFocus * 0.1f * relativeStrength;
        }
    }



    void updateMaxFocus(Power p)
    {
        float lastMax = maxFocus;

        float scaleNum = p.scaleNumerical();

        maxFocus = props.maxFocus * scaleNum;
        passiveFocusRecover = props.maxFocus * scaleNum * (1f / 50);
        mezmerizeFocusRecover = props.maxFocus * scaleNum * (1f / 3.5f);


        float proportion = maxFocus / lastMax;

        currentFocus *= proportion;
        currentMezmerizeHighestFocus *= proportion;
    }


    public void takeFocus(float damage)
    {


        if (mezmerized)
        {
            currentFocus += damage * 0.5f;
            if (currentFocus > currentMezmerizeHighestFocus)
            {
                currentMezmerizeHighestFocus = currentFocus;
            }
        }
        else
        {
            currentFocus += damage;
        }

    }


    float mezmerizeThreshold
    {
        get
        {
            return maxFocus;
        }

    }

    // Update is called once per frame
    public void OrderedUpdate()
    {

        if (!combat.inCombat)
        {
            currentFocus = 0;
        }
        if (!mezmerized && currentFocus > mezmerizeThreshold)
        {
            mezmerized = true;
            currentMezmerizeHighestFocus = currentFocus;
        }
        float currentFocusRecover;
        if (mezmerized)
        {
            currentFocusRecover = mezmerizeFocusRecover;
        }
        else
        {
            currentFocusRecover = passiveFocusRecover;
        }

        //change Focus
        if (currentFocus > 0)
        {
            currentFocus -= currentFocusRecover * Time.fixedDeltaTime * power.scaleTime();
        }
        if (mezmerized && currentFocus <= 0)
        {
            mezmerized = false;
        }
        if (currentFocus < 0)
        {
            currentFocus = 0;
        }
    }

    public BarValue.BarData getBarFill()
    {
        float denom;
        if (mezmerized)
        {
            denom = currentMezmerizeHighestFocus;
        }
        else
        {
            denom = mezmerizeThreshold;
        }
        return new BarValue.BarData
        {
            color = mezmerized ? GameColors.Mezmerized : Color.blue,
            fillPercent = Mathf.Clamp01(currentFocus / denom),
            active = true,
        };
    }

}

