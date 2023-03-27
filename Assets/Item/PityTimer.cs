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
