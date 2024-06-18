using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;
using static GenerateAttack;
using static GlobalCache;

public class Power : NetworkBehaviour, TextValue, BarValue
{
    [SyncVar(hook = nameof(callbackPower))]
    float currentPower = 100;

    float softcap = 0;
    public readonly static float softcapDiminishPercent = 0.1f;

    public readonly static float exponentDownscale = 1.5f;
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

    public void addPower(float incPower)
    {
        if(softcap > 0)
        {
            float powerOver = currentPower + incPower - softcap;
            if(powerOver > 0)
            {
                float oldCap = Mathf.Max(currentPower, softcap);
                float diff = currentPower + incPower - oldCap;

                float percentOver = (oldCap+diff/2) / softcap * 100;
                currentPower = oldCap + diff * Mathf.Pow(softcapDiminishPercent, percentOver) ;
            }
            else
            {
                currentPower += incPower;
            }
            
        }
        else
        {
            currentPower += incPower;
        }
        
        powerUpdates();

    }

    public void rescale()
    {
        cachedNumerical.recalc();
        cachedSpeed.recalc();
        cachedAccel.recalc();
        cachedPhysical.recalc();
        cachedTime.recalc();
        powerUpdates();
        if (isServer)
        {
            RpcRescale();
        }
    }

    [ClientRpc]
    void RpcRescale()
    {
        if (isClientOnly)
        {
            rescale();
        }
        
    }



    void callbackPower(float old, float newPower)
    {
        powerUpdates();
    }

    void powerUpdates()
    {
        setMetricScale();
        foreach (OnPowerUpdate callback in OnPowerUpdateCallbacks)
        {
            callback(this);
        }
    }
    void setMetricScale()
    {

        displayText = new TextValue.TextData
        {
            color = Color.white,
            text = displayExaggertatedPower(currentPower),

        };

    }
    static float exaggeratedPower(float power)
    {
        //return Mathf.Pow(power / 100, 2f);
        return power;
    }
    public static string displayExaggertatedPower(float power)
    {
        return displayPower(exaggeratedPower(power));
    }

    public static string displayPower(float power)
    {
        int currentDecimalPlaces = power == 0 ? 0 : (int)Mathf.Floor(Mathf.Log10(Mathf.Abs(power)));

        MetricName currentMetricScale = (MetricName)(currentDecimalPlaces / 3);

        int decimalRounding = Mathf.Max(currentDecimalPlaces - 3, -3);
        int decimalMetric = (int)currentMetricScale * 3;
        int decimalDiff = decimalMetric - decimalRounding;
        float roundedPower = Mathf.Round(power / Mathf.Pow(10, decimalRounding));
        float truncatedPower = roundedPower / Mathf.Pow(10, decimalDiff);
        string symbol = metricSymbol(currentMetricScale);
        return truncatedPower + symbol;
    }

    public void setPower(float power, float cap = 0)
    {
        currentPower = power;
        softcap = cap;
    }

    public static float damageFalloff(float powerOfAbility, float powerOfUnit)
    {
        if (powerOfUnit <= powerOfAbility)
        {
            return powerOfUnit;
        }
        float ratio = relativeScale(powerOfUnit, powerOfAbility);
        return ratio * powerOfAbility;
    }
    public static float relativeScale(float powerA, float powerB)
    {
        return scaleNumerical(powerA) / scaleNumerical(powerB);
    }


    public static readonly float baseDownscale = downscalePower(basePower);

    public float downscaled
    {
        get
        {
            return downscalePower(currentPower);
        }
    }
    BaseScales? overrideScales = null;
    [Server]
    public void setOverrideDefault()
    {
        overrideScales = new BaseScales
        {
            world = 1,
            time = Power.scaleNumerical(currentPower),
        };
        RpcSyncOverride(overrideScales.HasValue, overrideScales ?? new BaseScales() );
        rescale();
    }
    [Server]
    public void setOverrideNull()
    {
        overrideScales = null;
        RpcSyncOverride(overrideScales.HasValue, overrideScales ?? new BaseScales());
        rescale();
    }

    [ClientRpc]
    void RpcSyncOverride(bool hasValue, BaseScales scales)
    {
        if (isClientOnly)
        {
            overrideScales = hasValue ? scales : null;
            rescale();
        }  
    }

    public float scaleNumerical()
    {
        return cachedNumerical.get(currentPower);
    }

    public float scaleSpeed()
    {
        return cachedSpeed.get(currentPower);
    }

    public float scaleAccel()
    {
        return cachedAccel.get(currentPower);
    }
    public float scalePhysical()
    {
        return cachedPhysical.get(currentPower);
    }

    public float scaleTime()
    {
        return cachedTime.get(currentPower);
    }

    public Scales getScales()
    {
        return new Scales
        {
            bases = instanceBaseScales,
            numeric = scaleNumerical(),
            world = scalePhysical(),
            time = scaleTime(),
        };
    }

    CacheValue<float, float> cachedNumerical;
    CacheValue<float, float> cachedSpeed;
    CacheValue<float, float> cachedAccel;
    CacheValue<float, float> cachedPhysical;
    CacheValue<float, float> cachedTime;
    private void Awake()
    {
        cachedNumerical = new CacheValue<float, float>(scaleNumerical, currentPower);
        cachedSpeed = new CacheValue<float, float>(_scaleSpeedInstance, currentPower);
        cachedAccel = new CacheValue<float, float>(_scaleAccelInstance, currentPower);
        cachedPhysical = new CacheValue<float, float>(_scalePhysicalInstance, currentPower);
        cachedTime = new CacheValue<float, float>(_scaleTimeInstance, currentPower);
    }

    float _scalePhysicalInstance(float power)
    {
        return downscalePower(power) / baseDownscale / (overrideScales.HasValue ? overrideScales.Value.world : baseScales.world);
    }

    float _scaleTimeInstance(float power)
    {
        return downscalePower(power) / baseDownscale / (overrideScales.HasValue ? overrideScales.Value.time : baseScales.time);
    }
    float _scaleSpeedInstance(float power)
    {
        return _scalePhysicalInstance(power) * _scaleTimeInstance(power);
    }

    float _scaleAccelInstance(float power)
    {
        return _scalePhysicalInstance(power) * Mathf.Pow(_scaleTimeInstance(power), 2);
    }








    public static float scaleNumerical(float power)
    {
        return downscalePower(power) / baseDownscale;
    }


    static float scalePhysical(float power)
    {
        return downscalePower(power) / baseDownscale / baseScales.world;
    }

    static float scaleTime(float power)
    {
        return downscalePower(power) / baseDownscale / baseScales.time;
    }
    static float scaleSpeed(float power)
    {
        return scalePhysical(power) * scaleTime(power);
    }

    public static Scales getScales(float power)
    {
        return new Scales
        {
            bases = baseScales,
            numeric = scaleNumerical(power),
            world = scalePhysical(power),
            time = scaleTime(power),
        };
    }

    public struct BaseScales
    {
        public float world;
        public float time;

        public override string ToString()
        {
            return "World Base:" + world + ", Time Base:" + time;
        }
    }
    public static BaseScales currentBaseScales
    {
        get
        {
            return baseScales;
        }
    }

    public BaseScales instanceBaseScales
    {
        get
        {
            return overrideScales.HasValue ? overrideScales.Value : Power.baseScales;
        }
    }

    static BaseScales baseScales = new BaseScales
    {
        world = 1,
        time = 1,
    };
    

    public static void setScales(BaseScales scales)
    {
        baseScales = scales;
    }

    


    public static float downscalePower(float power)
    {
        return power / Mathf.Pow(2 / exponentDownscale, Mathf.Log(power / basePower, 2) + 1);
    }

    public static float inverseDownscalePower(float downscale)
    {
        return 0.5f * basePower * Mathf.Exp(-(Mathf.Log(2) * Mathf.Log(2 * downscale / basePower)) / Mathf.Log(1 / exponentDownscale));
    }


    TextValue.TextData displayText;
    public TextValue.TextData getText()
    {
        return displayText;
    }


    public enum MetricName
    {
        zero = 0,
        thousand,
        million,
        billion,
        trillion,
        quadrillion,
        pentillion,
        hextillion,
        septillion,
        octillion,
        nintillion,
        dectillion,
    }
    static string metricSymbol(MetricName m)
    {
        switch (m)
        {
            case MetricName.zero: return "";
            case MetricName.thousand: return "k";
            case MetricName.million: return "m";
            case MetricName.billion: return "b";
            case MetricName.trillion: return "t";
            case MetricName.quadrillion: return "q";
            case MetricName.pentillion: return "p";
            case MetricName.hextillion: return "h";
            case MetricName.septillion: return "s";
            case MetricName.octillion: return "o";
            case MetricName.nintillion: return "n";
            case MetricName.dectillion: return "d";
            default: return "";
        }
    }

    public BarValue.BarData getBarFill()
    {
        return new BarValue.BarData
        {
            active = currentPower< softcap,
            color = new Color(1, 0.8f, 0),
            fillPercent = (currentPower - Atlas.playerStartingPower) / (softcap- Atlas.playerStartingPower),

        };
    }

    public enum MetricPrefix
    {
        zero = 0,
        kilo,
        mega,
        giga,
        tera,
        peta,
        exa,
        zetta,
        yotta
    }
}


