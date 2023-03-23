using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class StatTypes
{
    public enum Stat :byte
    {
        Length,
        Width,
        Knockback,
        DamageMult,
        Stagger,
        Knockup,
        //Range,
    }
    public struct SyncStat
    {
        public Stat Stat;
        public float value;
    }

    static Dictionary<Stat, float> statValues = new Dictionary<Stat, float>()
    {
        {Stat.Length,5.5f},
        {Stat.Width,3.75f},
        {Stat.Knockback,17},
        {Stat.DamageMult,0.14f},
        {Stat.Stagger,200},
        {Stat.Knockup,20},
    }.Select(p =>  (p.Key, p.Value / Power.baseDownscale)).ToDictionary(tup => tup.Key,tup =>tup.Item2);

    static Dictionary<Stat, float> itemStatMax = new Dictionary<Stat, float>()
    {
        {Stat.Length,60},
        {Stat.Width,52},
        {Stat.Knockback,38},
        {Stat.DamageMult,92},
        {Stat.Stagger,48},
        {Stat.Knockup,32},
    };

    static Dictionary<Stat, float> itemStatBase = new Dictionary<Stat, float>()
    {
        {Stat.Length,6},
        {Stat.Width,9},
        {Stat.DamageMult,230},
    };


}
