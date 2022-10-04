using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PityTimer<T>
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

    public PityTimer(T defaultCat, float chance = 1f)
    {
        defaultCategory = defaultCat;
        chanceMulitplier = chance;
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

}
