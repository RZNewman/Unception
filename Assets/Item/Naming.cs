using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Naming
{
    public static string name()
    {
        return prefixes.RandomItem() + " " + suffixes.RandomItem();
    }


    static string[] prefixes = new string[] {
"Apex",
"Ascending",
"Cross",
"Omega",
"Mega",
"Brilliant",
"Lithe",
"Agile",
"Hunger",
"Piercing",
"Running",
"Precise",
"Dread",
"Alpha",
"Energy",
"Final",
"Deadly",
"Quick",
"Haunted",
"Sun",
"Moon",
"Star",
"Plasma",
"Infernal",
"Hellish",
"Perfect",
"Heroic",
"Cursed",
"Flash",
"Power",
    };
    static string[] suffixes = new string[] {
"Slash",
"Crush",
"Slam",
"Split",
"Thrust",
"Dive",
"Crescent",
"Trick",
"Wreck",
"Train",
"Melt",
"Lash",
"Ravage",
"Piledriver",
"Flay",
"Sting",
"Blade",
"Blast",
"Boom",
"Mangle",
"Joust",
"Slice",
"Rush",
"Swing",
"Shot",
"Punch",
    };
}
