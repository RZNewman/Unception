using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Power : NetworkBehaviour
{
    [SyncVar]
    float currentPower =100;

    static float factorDownscale = 1.3f;
    public readonly static float basePower = 100;

    public delegate void OnPowerUpdate(Power p);

    List<OnPowerUpdate> OnPowerUpdateCallbacks = new List<OnPowerUpdate>();

    public void subscribePower(OnPowerUpdate callback)
    {
        OnPowerUpdateCallbacks.Add(callback);
        callback(this);
    }

    public float power
    {
        get
        {
            return currentPower;
        }
    }

    void addPower(float power)
    {
        currentPower += power;
        //TODO network updates for client on sync
        foreach(OnPowerUpdate callback in OnPowerUpdateCallbacks)
        {
            callback(this);
        }
    }
    public void setPower(float power)
    {
        currentPower = power;
    }
    public static float baseDownscale
    {
        get
        {
            return downscalePower(basePower);
        }
    }
    public float downscaled
    {
        get
        {
            return downscalePower(currentPower);
        }
    }
    public float scale()
    {
        return scale(currentPower);
    }

    public static float scale(float power)
    {
        return downscalePower(power) / baseDownscale;
    }

    public static float downscalePower(float power)
    {
        return power / Mathf.Pow(2 / factorDownscale, Mathf.Log(power / basePower, 2) + 1);
    }
    public void absorb(Power other)
    {
        float gathered = other.currentPower * 0.2f;
        if (gathered < currentPower)
        {
            //closes the gap for catchup exp
            gathered *= MonsterSpawn.scaledPowerRewardFactor(currentPower, other.currentPower);
        }
        addPower( gathered);
    }
}
