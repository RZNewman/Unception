using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StatTypes;

public class UiBuffBar : MonoBehaviour
{
    public GameObject BuffIconPre;

    public Sprite length;
    public Sprite width;
    public Sprite range;
    public Sprite turn;

    public Sprite cooldown;
    public Sprite haste;
    public Sprite charges;
    public Sprite devotion;

    public Sprite knockback;
    public Sprite knockup;
    public Sprite stagger;
    public Sprite persist;

    public Sprite move;
    public Sprite chain;
    public Sprite splash;
    public Sprite armor;

    public void displayBuffs(List<Buff> buffs)
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        HashSet<Stat> stats = new HashSet<Stat>();
        foreach (Buff buff in buffs)
        {
            foreach (Stat stat in buff.stats.Keys)
            {
                stats.Add(stat);
            }
        }
        foreach (Stat stat in stats)
        {
            GameObject instance = Instantiate(BuffIconPre, transform);
            instance.GetComponent<UiBuffIcon>().setImage(fromStat(stat));
        }
    }

    public Sprite fromStat(Stat stat)
    {
        switch (stat)
        {
            case Stat.Length:
                return length;
            case Stat.Width:
                return width;
            case Stat.Range:
                return range;
            case Stat.Turnspeed:
                return turn;
            case Stat.Cooldown:
                return cooldown;
            case Stat.Haste:
                return haste;
            case Stat.Charges:
                return charges;
            //devotion
            case Stat.Knockback:
                return knockback;
            case Stat.Knockup:
                return knockup;
            case Stat.Stagger:
                return stagger;
            //persist
            case Stat.Movespeed:
                return move;

                //chain
                //splash
                //armor
        }
        return null;
    }
}
