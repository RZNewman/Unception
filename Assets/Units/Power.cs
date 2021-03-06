using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class Power : NetworkBehaviour, TextValue
{
    [SyncVar]
    float currentPower = 100;

    int currentDecimalPlaces = 0;
    public MetricName currentMetricScale
    {
        get
        {
            return (MetricName)(currentDecimalPlaces / 3);
        }
    }

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
        setMetricScale();
        //TODO network updates for client on sync
        foreach (OnPowerUpdate callback in OnPowerUpdateCallbacks)
        {
            callback(this);
        }
    }
    void setMetricScale()
    {
        while (currentPower > Mathf.Pow(10, currentDecimalPlaces + 1))
        {
            currentDecimalPlaces++;
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
        float gathered;
        if (other.currentPower < currentPower)
        {
            //closes the gap for catchup exp
            gathered = MonsterSpawn.scaledPowerReward(currentPower, other.currentPower);
        }
        else
        {
            gathered = other.currentPower;
        }
        gathered *= 0.2f;
        addPower(gathered);
    }

    public TextValue.TextData getText()
    {
        int decimalRounding = Mathf.Max(0, currentDecimalPlaces - 3);
        int decimalMetric = (int)currentMetricScale * 3;
        int decimalDiff = decimalMetric - decimalRounding;
        float roundedPower = Mathf.Round(currentPower / Mathf.Pow(10, decimalRounding));
        float displayPower = roundedPower / Mathf.Pow(10, decimalDiff);
        string symbol = metricSymbol();
        return new TextValue.TextData
        {
            color = Color.white,
            text = displayPower + " " + symbol,

        };
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
    string metricSymbol()
    {
        switch (currentMetricScale)
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


