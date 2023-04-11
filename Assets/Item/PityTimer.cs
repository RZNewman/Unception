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
    System.Random rng;
    struct PityRecord
    {
        public int count;
        public double value;
        public double chance
        {
            get
            {
                return 1 - value;
            }
        }
        public double cost
        {
            get
            {
                return -1 / (chance * 1.3d);
            }
        }
    }
    public PityTimerContinuous()
    {
        records = new List<PityRecord>();
        records.Add(new PityRecord()
        {
            count = 0,
            value = 0.5f,
        });
        rng = new System.Random();
    }

    public double roll(int rarityFactor = 1, double fedValue = -1)
    {
        double n = fedValue;
        if (n < 0)
        {
            n = rng.NextDouble();
        }
        double cap = 1;
        PityRecord newRecord;
        //if (records.Count > 0)
        //{
        int bonus = 1 * rarityFactor;
        for (int k = 0; k < records.Count; k++)
        {
            PityRecord record = records[k];
            record.count += bonus;
            records[k] = record;
        }
        int j;
        for (j = 0; j < records.Count; j++)
        {
            PityRecord record = records[j];
            if (record.count + record.cost < 0)
            {
                cap = record.value;
                break;
            }
        }
        //Debug.Log("Records: " + records.Count + ", Sum: " + sum);
        int i = -1;
        //n = n.asRange(0, cap);
        double hit = -1;
        int count = bonus;
        if (j > 0)
        {
            for (i = j - 1; i >= 0; i--)
            {
                PityRecord r = records[i];
                double weight = r.cost + r.count;
                double weightedChance = weight * r.chance;
                double nChance = 1 - n;
                if (nChance <= weightedChance)
                {
                    double percentOfRange = 1 - nChance / weightedChance;
                    hit = percentOfRange.asRange(r.value, cap);
                    count = (int)(weight);
                    break;
                }

                //float chanceCost = -1 / (chance);
                //float weight = chanceCost + sum - r.allCount + r.selfCount;
                //float nChance = 1 - n;
                //float weightedChance = chance * weight;
                //if (weight <= 0)
                //{
                //    cap = r.value;
                //}
                //else if (nChance <= weightedChance)
                //{
                //    float percentOfRange = 1 - nChance / weightedChance;
                //    hit = percentOfRange.asRange(r.value, cap);
                //    cost = chanceCost;
                //    break;
                //}



            }
        }

        if (hit < 0)
        {
            hit = n.asRange(0, cap);
            //hit = n;
        }
        if (i >= 0)
        {
            records.RemoveRange(0, i + 1);
        }

        newRecord.value = hit;
        newRecord.count = count;

        //if (newRecord.value < 0.5f)
        //{
        //    newRecord.value = records[0].value;
        //    newRecord.count += records[0].count;
        //    records[0] = newRecord;
        //}
        //else
        //{

        //    records.Add(newRecord);
        //    records.Sort((r1, r2) => r1.value.CompareTo(r2.value));
        //}

        records.Add(newRecord);
        records.Sort((r1, r2) => r1.value.CompareTo(r2.value));


        return newRecord.value;

    }
    public Dictionary<double, int> export()
    {
        Dictionary<double, int> dict = new Dictionary<double, int>();
        foreach (PityRecord p in records)
        {
            //Debug.Log(p.value + ":" + p.count);
            dict.Add(p.value, p.count);
        }
        return dict;
    }

}
