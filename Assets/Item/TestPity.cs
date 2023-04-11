using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Utils;

public static class TestPity
{
    // Start is called before the first frame update
    static int testCount = 100000;

    public static void test()
    {
        PityTimerContinuous pity = new PityTimerContinuous();
        System.Random rng = new System.Random();
        List<(int, int)> results = new List<(int, int)>();
        Dictionary<int, int> normalCounts = percentCounts();
        Dictionary<int, int> pityCounts = percentCounts();

        for (int i = 0; i < testCount; i++)
        {
            double value = rng.NextDouble();
            int normal = percent(value);
            int pitied = percent(pity.roll(1, value));
            normalCounts[normal] += 1;
            pityCounts[pitied] += 1;
            results.Add((normal, pitied));
        }

        string output = "";
        foreach (KeyValuePair<int, int> p in normalCounts)
        {
            output += p.Key + ":" + p.Value + '\n';
        }
        Debug.Log(output);

        output = "";
        foreach (KeyValuePair<int, int> p in pityCounts)
        {
            output += p.Key + ":" + p.Value + '\n';
        }
        Debug.Log(output);

        output = "";
        foreach (KeyValuePair<double, int> p in pity.export())
        {
            output += p.Key + ":" + p.Value + '\n';
        }
        Debug.Log(output);

        int expectedCount = testCount / 100;
        Debug.Log("Delta normal:" + normalCounts.Values.Select(v => Mathf.Abs(v - expectedCount)).Sum());
        Debug.Log("Delta pity:" + pityCounts.Values.Select(v => Mathf.Abs(v - expectedCount)).Sum());

        if (testCount < 1000)
        {
            return;
        }
        int period = 100;
        int range = 1000;
        expectedCount = range / 100;
        int deltaNormalSum = 0;
        int deltaNormalMin = range;
        int deltaNormalMax = 0;
        int deltaPitySum = 0;
        int deltaPityMin = range;
        int deltaPityMax = 0;
        for (int i = 0; i + range - 1 < testCount; i += period)
        {
            normalCounts = percentCounts();
            pityCounts = percentCounts();
            for (int j = 0; j < range; j++)
            {
                int index = i + j;
                normalCounts[results[index].Item1] += 1;
                pityCounts[results[index].Item2] += 1;
            }
            int normalDelta = normalCounts.Values.Select(v => Mathf.Abs(v - expectedCount)).Sum();
            int pityDelta = pityCounts.Values.Select(v => Mathf.Abs(v - expectedCount)).Sum();

            if (normalDelta < deltaNormalMin) { deltaNormalMin = normalDelta; }
            if (normalDelta > deltaNormalMax) { deltaNormalMax = normalDelta; }
            deltaNormalSum += normalDelta;
            if (pityDelta < deltaPityMin) { deltaPityMin = pityDelta; }
            if (pityDelta > deltaPityMax) { deltaPityMax = pityDelta; }
            deltaPitySum += pityDelta;
        }
        float deltaNormalAvg = deltaNormalSum / (testCount / (float)period);
        float deltaPityAvg = deltaPitySum / (testCount / (float)period);

        Debug.Log("Delta normal Stats: Min: " + deltaNormalMin + ", Max: " + deltaNormalMax + ", Avg: " + deltaNormalAvg);
        Debug.Log("Delta Pity Stats: Min: " + deltaPityMin + ", Max: " + deltaPityMax + ", Avg: " + deltaPityAvg);



    }


    static int percent(float roll)
    {
        return Mathf.Min(Mathf.FloorToInt(roll * 100), 99);
    }
    static int percent(double roll)
    {
        return Mathf.Min((int)(roll * 100), 99);
    }

    static Dictionary<int, int> percentCounts()
    {
        Dictionary<int, int> counts = new Dictionary<int, int>();
        for (int i = 0; i < 100; i++)
        {
            counts[i] = 0;
        }
        return counts;
    }
}
