using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Utils;

public static class GlobalCache
{

    public struct CacheValue<T, TResult> where T : IComparable
    {

        Func<T, TResult> getValue;
        T cachedInput;
        TResult cachedValue;
        public CacheValue(Func<T, TResult> v, T first)
        {

            getValue = v;
            cachedInput = first;
            cachedValue = getValue(cachedInput);
        }

        public TResult get(T input)
        {
            if (!input.Equals(cachedInput))
            {
                cachedInput = input;
                cachedValue = getValue(cachedInput);
            }
            return cachedValue;
        }

        public void recalc()
        {
            cachedValue = getValue(cachedInput);
        }

    }


}
