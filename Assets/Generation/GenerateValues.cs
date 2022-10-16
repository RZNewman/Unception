using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Utils;

public static class GenerateValues
{

    public struct Value
    {
        public float val;
        public float equivalence;
    }

    public static Value[] generateRandomValues(int valueCount, float balance = 2)
    {
        return generateRandomValues(Enumerable.Repeat(1f, valueCount).ToArray(), balance);
    }


    public static Value[] generateRandomValues(float[] worths, float balance = 2)
    {
        int valueCount = worths.Length;
        Value[] values = new Value[valueCount];
        for (int i = 0; i < valueCount; i++)
        {
            values[i] = new Value
            {
                val = 0.5f,
                equivalence = worths[i],
            };
        }

        List<int> unassinged = new List<int>();
        List<int> boosted = new List<int>();
        List<int> drained = new List<int>();
        for (int i = 0; i < valueCount; i++)
        {
            unassinged.Add(i);
        }

        while (unassinged.Count > 0)
        {
            int element = selectValueIndex(unassinged, values);
            bool elementBoosted = Random.value > 0.5f;
            unassinged.Remove(element);

            List<int> candidates = new List<int>(unassinged);
            if (elementBoosted)
            {
                candidates = candidates.Concat(drained).ToList();
            }
            else
            {
                candidates = candidates.Concat(boosted).ToList();
            }
            int alternate = selectValueIndex(candidates, values, !elementBoosted);

            int boost, drain;
            if (elementBoosted)
            {
                boosted.Add(element);
                if (unassinged.Contains(alternate))
                {
                    unassinged.Remove(alternate);
                    drained.Add(alternate);
                }
                boost = element;
                drain = alternate;
            }
            else
            {
                drained.Add(element);
                if (unassinged.Contains(alternate))
                {
                    unassinged.Remove(alternate);
                    boosted.Add(alternate);
                }
                boost = alternate;
                drain = element;
            }

            values = transfer(values, boost, drain, 0, balance);
        }


        return values;
    }

    static Value[] transfer(Value[] values, int drain, int boost, float minTranserFactor = 0.0f, float balance =2)
    {
        float transferFactor = GaussRandomDecline(balance).asRange(minTranserFactor,1.0f);
        float ratio = values[drain].equivalence / values[boost].equivalence;
        float maxTransfer = Mathf.Min(
            worth(values[drain]),
            worth(values[boost], true) / ratio
            );

        float transfer = maxTransfer * transferFactor;
        //Debug.Log(drain + " ->>>> "+transfer +" > "+ boost);
        values[drain].val -= transfer;
        values[boost].val += transfer / ratio;
        return values;
    }
    public static Value[] augment(Value[] prevValues, float[] equivs)
    {
        int oldCount = prevValues.Length;
        int addedCount = equivs.Length;
        Value[] newValues = new Value[oldCount + addedCount];
        for (int i = 0; i < oldCount; i++)
        {
            newValues[i] = prevValues[i];
        }
        for (int i = oldCount; i < newValues.Length; i++)
        {
            newValues[i] = new Value
            {
                val = 0f,
                equivalence = equivs[i - oldCount],
            };
        }

        for (int i = oldCount; i < newValues.Length; i++)
        {
            int oldDrain = Random.Range(0, oldCount - 1);
            newValues = transfer(newValues, oldDrain, i, 0.25f);
        }


        return newValues;

    }
    static int selectValueIndex(List<int> indices, Value[] values, bool boosted = false)
    {
        float drainSum = indices.Sum((valueIndex) => worth(values[valueIndex], boosted));


        float selection = Random.Range(0, drainSum);
        int currentIndex = -1;
        while (selection > 0)
        {
            currentIndex++;
            selection -= worth(values[indices[currentIndex]], boosted);
        }
        if (currentIndex == -1) { currentIndex = 0; }
        return indices[currentIndex];
    }

    static float worth(Value val, bool boosted = false)
    {
        if (boosted)
        {
            return 1 - val.val;
        }
        else
        {
            return val.val;
        }

    }




}
