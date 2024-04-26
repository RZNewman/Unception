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
            float initial = -1;

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

    struct Range
    {
        public float min;
        public float max;
    }

    struct PityRecord
    {
        public int count;
        public int powerOfTwoValue;


        float percent
        {
            get
            {
                return percentForPower(powerOfTwoValue);
            }
        }

        public int afterHit()
        {
            return count - Mathf.RoundToInt(Mathf.Pow(2, powerOfTwoValue));
        }
        static float percentForPower(int power)
        {
            return 1f / Mathf.Pow(2, power);
        }


        public Range resultRange
        {
            get {
                float max = percent;
                return new Range
                {
                    max = max,
                    min = max - percentForPower(powerOfTwoValue+1),
                }; 
            }
        }
        public float chance
        {
            get
            {
                return percent * count;
            }
        }

    }
    public PityTimerContinuous()
    {
        records = new List<PityRecord>();

        for(int i = 1; i< 12; i++)
        {
            records.Add(new PityRecord()
            {
                count = 1,
                powerOfTwoValue = i,
            });
        }    
        rng = new System.Random();
    }

    public float roll(int rarityFactor = 1, float fedValue = -1)
    {
        float n = fedValue;
        if (n < 0)
        {
            n = (float)rng.NextDouble();
        }
        float cap = 0;


        int i;
        float chanceMax = 1;
        for(i = records.Count -1; i >= 0; i--)
        {
            float chance = records[i].chance;
            if (n <= chance)
            {
                chanceMax = chance;
                break;
            }
            else
            {
                if (chance > cap)
                {
                    cap = chance;
                }
            }
        }
        PityRecord r;


        Range ToRange = i < 0 ? new Range { max = 1, min = 0.5f } : records[i].resultRange;
        Range FromRange = new Range { max = chanceMax, min = cap };

        float percent = Mathf.InverseLerp(FromRange.min, FromRange.max, n);
        float result = Mathf.Lerp(ToRange.min, ToRange.max, percent);



        for (int j = 0; j < records.Count; j++)
        {
            r = records[j];
            if (j > i)
            {
                r.count++;
            }
            else
            {
                r.count = r.afterHit();
            }
            records[j] = r;
        }

        return result;




    }
    public Dictionary<float, int> export()
    {
        Dictionary<float, int> dict = new Dictionary<float, int>();
        foreach (PityRecord p in records)
        {
            //Debug.Log(p.value + ":" + p.count);
            dict.Add(p.powerOfTwoValue, p.count);
        }
        return dict;
    }

}
