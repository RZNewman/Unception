using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BigMask
{
    HashSet<int> mask = new HashSet<int>();
    public BigMask()
    {
    }



    public BigMask(params int[] indcies)
    {
        foreach (int index in indcies)
        {
            addIndex(index);
        }

    }

    public BigMask(BigMask copy)
    {
        mask = new HashSet<int>(copy.mask);
    }

    public void addIndex(int index)
    {
        mask.Add(index);
    }

    public void removeIndex(int index)
    {
        mask.Remove(index);
    }



    public bool singleDomain()
    {
        return mask.Count == 1;
    }

    public bool empty
    {
        get
        {
            return mask.Count == 0;
        }



    }

    public List<int> indicies
    {
        get
        {
            return mask.ToList();
        }
    }

    public string cacheKey
    {
        get
        {
            return System.String.Join(',',mask.OrderBy(n=>n));
        }
    }


}
