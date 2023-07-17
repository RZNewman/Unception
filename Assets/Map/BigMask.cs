using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigMask
{
    static readonly int maskLen = 64;
    static readonly int maskCount = 2;

    ulong[] masks;
    public BigMask()
    {
        masks = new ulong[maskCount];
    }

    public BigMask(ulong[] mask)
    {
        masks = mask;
    }

    public BigMask(params int[] indcies)
    {
        masks = new ulong[maskCount];
        foreach (int index in indcies)
        {
            addIndex(index);
        }

    }

    public BigMask(BigMask copy)
    {
        masks = new ulong[maskCount];
        copy.masks.CopyTo(masks, 0);
    }

    public void addIndex(int index)
    {
        int maskInd = index / maskLen;
        int subInd = index % maskLen;
        masks[maskInd] |= 1uL << subInd;
    }

    public void removeIndex(int index)
    {
        int maskInd = index / maskLen;
        int subInd = index % maskLen;
        masks[maskInd] ^= 1uL << subInd;
    }

    public static BigMask operator |(BigMask a, BigMask b)
    {
        ulong[] masks;
        masks = new ulong[maskCount];
        for (int i = 0; i < maskCount; i++)
        {
            masks[i] = a.masks[i] | b.masks[i];
        }
        return new BigMask(masks);
    }


    public bool singleDomain()
    {
        bool flagFound = false;
        foreach (ulong mask in masks)
        {
            if (mask.oneFlagSet())
            {
                if (flagFound)
                {
                    return false;
                }
                flagFound = true;
            }
            else if (mask > 0)
            {
                return false;
            }
        }
        return flagFound;
    }

    public bool empty
    {
        get
        {
            foreach (ulong mask in masks)
            {
                if (mask > 0)
                {
                    return false;
                }
            }

            return true;
        }


    }

    public List<int> indicies
    {
        get
        {
            List<int> inds = new List<int>();
            int baseIndex = 0;
            foreach (ulong mask in masks)
            {
                //Debug.Log(mask);
                for (int i = 0; i < maskLen; i++)
                {
                    if ((mask & (1uL << i)) > 0)
                    {
                        inds.Add(baseIndex + i);
                    }
                }

                baseIndex += maskLen;
            }

            //Debug.Log(inds.Count);
            return inds;
        }
    }


}
