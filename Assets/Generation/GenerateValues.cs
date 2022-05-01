using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Utils;

public static class GenerateValues 
{

    struct Value
    {
        public float val;
    }



    public static float[] generateRandomValues(int valueCount)
    {
        Value[] values = new Value[valueCount];
        for (int i = 0; i < valueCount; i++)
        {
            values[i] = new Value
            {
                val = 0.5f,
            };
        }

        List<int> unassinged = new List<int>();
        List<int> boosted = new List<int>();
        List<int> drained = new List<int>();
        for (int i = 0; i < valueCount; i++)
        {
            unassinged.Add(i);
        }

        while(unassinged.Count > 0)
        {
            List<int> drainCandidates = unassinged.Concat(drained).ToList();
            int drain = selectValueIndex(drainCandidates, values);
            if (unassinged.Contains(drain))
            {
                unassinged.Remove(drain);
                drained.Add(drain);
            }

            List<int> boostCandidates = unassinged.Concat(boosted).ToList();
            int boost = selectValueIndex(boostCandidates, values, true);
            if (unassinged.Contains(boost))
            {
                unassinged.Remove(boost);
                boosted.Add(boost);
            }

            float maxTransfer = Mathf.Min(values[drain].val, 1 - values[boost].val);
            float transfer = Mathf.Abs(GaussRandom(0, 1)-0.5f)* maxTransfer;
            values[drain].val-=transfer;
            values[boost].val+=transfer; 
        }


        float[] output = new float[valueCount];
        for(int i = 0; i < valueCount; i++) 
        {
            Value v = values[i];
            //Debug.Log(v.val);
            output[i] = v.val;
        }
        return output;
    }
    static int selectValueIndex(List<int> indices, Value[] values, bool reverse =false)
    {
        float drainSum;
        if (reverse)
        {
            drainSum = indices.Sum((valueIndex) => 1-values[valueIndex].val);
        }
        else
        {
            drainSum = indices.Sum((valueIndex) => values[valueIndex].val);
        }
        
        float selection = Random.Range(0, drainSum);
        int currentIndex = -1;
        while (selection > 0)
        {
            currentIndex++;
            selection -= values[currentIndex].val;
        }
        if (currentIndex == -1) { currentIndex = 0; }
        return currentIndex;
    }




}
