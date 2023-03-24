using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class Power : NetworkBehaviour, TextValue
{
    [SyncVar(hook = nameof(callbackPower))]
    float currentPower = 100;



    public readonly static float exponentDownscale = 1.5f;
    public readonly static float basePower = 100;

    public readonly static float playerStartingPower = 1000;

    public delegate void OnPowerUpdate(Power p);

    List<OnPowerUpdate> OnPowerUpdateCallbacks = new List<OnPowerUpdate>();

    static float baseWorldScale = 1;
    static float baseTimeScale = 1;

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

    public void addPower(float power)
    {
        currentPower += power;
        powerUpdates();

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
        return Mathf.Pow(power / 100, 2f);
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
        return truncatedPower + " " + symbol;
    }

    public void setPower(float power)
    {
        currentPower = power;
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
    public float scaleNumerical()
    {
        return scaleNumerical(currentPower);
    }

    public float scaleSpeed()
    {
        return scaleSpeed(currentPower);
    }
    public float scalePhysical()
    {
        return scalePhysical(currentPower);
    }

    public float scaleTime()
    {
        return scaleTime(currentPower);
    }

    public static float scaleSpeed(float power)
    {
        return scalePhysical(power) * scaleTime(power);
    }

    public static float scaleNumerical(float power)
    {
        return downscalePower(power) / baseDownscale;
    }


    public static float scalePhysical(float power)
    {
        return downscalePower(power) / baseDownscale / baseWorldScale;
    }

    public static float scaleTime(float power)
    {
        return downscalePower(power) / baseDownscale / baseTimeScale;
    }
    public static float worldScale
    {
        get
        {
            return baseWorldScale;
        }
    }
    public static float timeScale
    {
        get
        {
            return baseTimeScale;
        }
    }
    public static void setPhysicalScale(float scale)
    {
        baseWorldScale = scale;
    }
    public static void setTimeScale(float scale)
    {
        baseTimeScale = scale;
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


