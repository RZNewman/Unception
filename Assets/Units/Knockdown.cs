using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StatTypes;

public class Knockdown : NetworkBehaviour
{
    [SyncVar]
    float currentKnockdown;

    float recentlyKDTime;

    public bool knockedDown
    {
        get
        {
            return isDown;
        }
    }
    bool isDown = false;
    Power power;

    // Start is called before the first frame update
    void Start()
    {
        power = GetComponent<Power>();
        recentlyKDTime = 0;
    }

    float fallOffMult
    {
        get
        {
            float scaleTime = power.scaleTime();
            return (1 + (recentlyKDTime / scaleTime) / 3);
        }
    }

    float stunThreshold
    {
        get
        {
            return 1.2f * fallOffMult;
        }

    }


    public void tryKnockDown(float back, float up)
    {
        float KDValue = back + 2 * up * relativeKnockup;
        KDValue /= power.scaleSpeed();
        //Debug.Log("Raw value:" + KDValue);
        currentKnockdown += KDValue * 0.12f;
    }

    // Update is called once per frame
    public void OrderedUpdate()
    {
        float scaleTime = power.scaleTime();
        currentKnockdown -= Time.fixedDeltaTime * scaleTime * fallOffMult;

        if (currentKnockdown < 0)
        {
            currentKnockdown = 0;
        }

        if(!isDown&&currentKnockdown >= stunThreshold)
        {
            isDown = true;
            GetComponent<AnimationController>().setKnockedDown();
        }
        else if(isDown && currentKnockdown < stunThreshold)
        {
            isDown = false;
        }


        if (isDown)
        {
            recentlyKDTime += Time.fixedDeltaTime * scaleTime;
        }
        else
        {
            recentlyKDTime -= Time.fixedDeltaTime * (0.2f) * scaleTime;
            recentlyKDTime = Mathf.Max(recentlyKDTime, 0);
        }
    }
}
