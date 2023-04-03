using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static RewardManager;
using static Utils;

public class PityTimer<T> where T : struct, System.IConvertible
{
    struct PityWeight
    {
        public T category;
        public float chance;
        public float baseChance;
    }
    List<PityWeight> weightList = new List<PityWeight>();

    public void addCategory(T c, float chance, float builtChance = 0f)
    {
        weightList.Add(new PityWeight
        {
            category = c,
            chance = builtChance,
            baseChance = chance,
        });
        weightList.Sort((w1, w2) => w2.baseChance.CompareTo(w1.baseChance));
    }

    float chanceMulitplier;
    T defaultCategory;

    public PityTimer(T defaultCat, float mult = 1f)
    {
        defaultCategory = defaultCat;
        chanceMulitplier = mult;
    }
    public PityTimer(float mult, float rarityChance, float rarityPowerFactor, IDictionary<T, float> startingValues = null)
    {
        chanceMulitplier = mult;
        T[] values = EnumValues<T>().ToArray();
        for (int i = 0; i < values.Length; i++)
        {
            T value = values[i];
            float initial = 0;

            if (i == 0)
            {
                defaultCategory = value;
            }
            else
            {
                if (startingValues != null)
                {
                    startingValues.TryGetValue(value, out initial);
                }
                addCategory(value, rarityChance * Mathf.Pow(rarityPowerFactor, i - 1), initial);
            }
        }
    }

    public T roll(float rarityFactor = 1f)
    {
        float v = Random.value;
        T chosen = defaultCategory;
        bool selected = false;
        for (int i = weightList.Count - 1; i >= 0; i--)
        {
            PityWeight w = weightList[i];
            w.chance += w.baseChance * rarityFactor;
            //Debug.Log(w.chance + " - " + w.category);
            if (v <= w.chance * chanceMulitplier)
            {
                w.chance -= 1;
                chosen = w.category;
                selected = true;
            }
            weightList[i] = w;
            if (selected)
            {
                return chosen;
            }
        }
        return chosen;
    }

    public Dictionary<string, float> export()
    {
        Dictionary<string, float> dict = new Dictionary<string, float>();
        foreach (PityWeight p in weightList)
        {
            dict.Add(p.category.ToString(), p.chance);
        }
        return dict;
    }
    public void debug()
    {
        foreach (PityWeight w in weightList)
        {
            Debug.Log(w.category + " - " + w.chance + " / " + w.baseChance);
        }
    }

}

public class PityTimerContinuous
{
    List<PityRecord> records;
    struct PityRecord
    {
        public float selfCount;
        public int allCount;
        public float value;
    }
    public PityTimerContinuous()
    {
        records = new List<PityRecord>();
    }

    public float roll(int rarityFactor = 1, float fedValue = -1)
    {
        float n = fedValue;
        if (n < 0)
        {
            n = Random.value;
        }
        float cap = 1;
        float cost = 0;
        PityRecord newRecord;
        if (records.Count > 0)
        {
            int sum = records.Select(r => r.allCount).Sum() + 1 * rarityFactor;
            int i;
            float hit = -1;
            for (i = records.Count - 1; i >= 0; i--)
            {
                PityRecord r = records[i];
                float chance = 1 - r.value;


                //float weight = -1 / chance + sum;
                //float weightedChance = (weight < 1 ? 1 / (1 - weight) : weight) * chance;
                //if (n <= weightedChance)
                //{
                //    hit = n.asRange(r.value, cap);
                //    break;
                //}
                //else
                //{
                //    cap = r.value;
                //}

                float chanceCost = -1 / (chance);
                float weight = chanceCost + sum - r.allCount + r.selfCount;
                float nChance = 1 - n;
                float weightedChance = chance * weight;
                if (weight <= 0)
                {
                    cap = r.value;
                }
                else if (nChance <= weightedChance)
                {
                    float percentOfRange = 1 - nChance / weightedChance;
                    hit = percentOfRange.asRange(r.value, cap);
                    cost = chanceCost;
                    break;
                }

                sum -= r.allCount;

            }
            if (hit < 0)
            {
                hit = n.asRange(0, cap);
            }
            if (i >= 0)
            {
                records.RemoveRange(0, i + 1);
            }

            newRecord.value = hit;
            newRecord.allCount = sum;
            newRecord.selfCount = sum + cost;

        }
        else
        {
            newRecord.value = n;
            newRecord.allCount = 1;
            newRecord.selfCount = 0;
        }

        records.Add(newRecord);
        records.Sort((r1, r2) => r1.value.CompareTo(r2.value));


        return newRecord.value;

    }
    public Dictionary<float, int> export()
    {
        Dictionary<float, int> dict = new Dictionary<float, int>();
        foreach (PityRecord p in records)
        {
            dict.Add(p.value, p.allCount);
        }
        return dict;
    }
}
