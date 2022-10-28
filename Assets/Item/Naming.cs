﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Naming
{
    public static string name()
    {
        return prefixes.RandomItem() + " " + suffixes.RandomItem();
    }
    public static string identifier()
    {
        return greekUpper.RandomItem() + greekLower.RandomItem();
    }

    static string[] greekLower = new string[] {
        "α",
        "β",
        "γ",
        "δ",
        "ε",
        "ζ",
        "η",
        "θ",
        "ι",
        "κ",
        "λ",
        "μ",
        "ν",
        "ξ",
        "ο",
        "π",
        "ρ",
        "σ",
        "τ",
        "υ",
        "φ",
        "χ",
        "ψ",
        "ω",
    };
    static string[] greekUpper = new string[] {
        "Α",
        "Β",
        "Γ",
        "Δ",
        "Ε",
        "Ζ",
        "Η",
        "Θ",
        "Ι",
        "Κ",
        "Λ",
        "Μ",
        "Ν",
        "Ξ",
        "Ο",
        "Π",
        "Ρ",
        "Σ",
        "Τ",
        "Υ",
        "Φ",
        "Χ",
        "Ψ",
        "Ω",
    };

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
