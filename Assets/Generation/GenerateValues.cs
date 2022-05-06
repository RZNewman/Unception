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
        public float width;
    }

    public static float[] generateRandomValues(int valueCount)
    {
        return generateRandomRanges(Enumerable.Repeat(1f,valueCount).ToArray());
    }


    public static float[] generateRandomRanges(float[] ranges)
    {
        int valueCount = ranges.Length;
        Value[] values = new Value[valueCount];
        for (int i = 0; i < valueCount; i++)
        {
            values[i] = new Value
            {
                val = 0.5f,
                width = ranges[i],
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


            float maxTransfer = Mathf.Min(
                worth(values[drain]),
                worth(values[boost], true)
                );
            
            float transfer = Mathf.Abs(GaussRandom(0,1)-0.5f)*2* maxTransfer;
            //Debug.Log(drain + " ->>>> "+transfer +" > "+ boost);
            values[drain].val-=transfer;
            values[boost].val+=transfer; 
        }


        float[] output = new float[valueCount];
        //string debug = "";
        for(int i = 0; i < valueCount; i++) 
        {
            Value v = values[i];
            //debug += v.val+", ";
            output[i] = v.val;
        }
        //Debug.Log(debug);
        return output;
    }
    static int selectValueIndex(List<int> indices, Value[] values, bool boosted =false)
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
            return 1 - val.val - (1 - val.width) / 2;       
        }
        else
        {
            return val.val - (1 - val.width) / 2;
        }
        
    }




}
