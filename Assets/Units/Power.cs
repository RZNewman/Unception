using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Power : NetworkBehaviour
{
    [SyncVar]
    public float power;

    static float factorDownscale = 1.3f;
    public static float basePower = 100;

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
            return downscalePower(power);
        }
    }

    public static float downscalePower(float power)
    {
        return power / Mathf.Pow(2 / factorDownscale, Mathf.Log(power / basePower, 2) + 1);
    }
    public void absorb(Power other)
    {
        power += other.power * 0.5f;
    }
}
