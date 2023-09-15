using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StatTypes;

public class Knockdown : NetworkBehaviour
{
    [SyncVar]
    float currentKnockdown;

    public bool knockedDown
    {
        get
        {
            return currentKnockdown > 0;
        }
    }
    Power power;

    // Start is called before the first frame update
    void Start()
    {
        power = GetComponent<Power>();
    }

    public void tryKnockDown(float back, float up)
    {
        float KDValue = back + 2 * up * relativeKnockup;
        KDValue /= power.scaleSpeed();
        //Debug.Log("Raw value:" + KDValue);
        KDValue -= 10;
        if (KDValue > 0)
        {
            if (currentKnockdown <= 0)
            {
                GetComponent<AnimationController>().setKnockedDown();
            }
            currentKnockdown += KDValue * 0.12f;
        }
    }

    // Update is called once per frame
    public void OrderedUpdate()
    {
        currentKnockdown -= Time.fixedDeltaTime * power.scaleTime();

        if (currentKnockdown < 0)
        {
            currentKnockdown = 0;
        }
    }
}
