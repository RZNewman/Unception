using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GenerateAttack;
using static StatTypes;

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

    public static string slotPhysical(ItemSlot? slot)
    {
        return slot switch
        {
            ItemSlot.Main => "Sword",
            ItemSlot.OffHand => "Mace",
            ItemSlot.Gloves => "Gloves",
            ItemSlot.Chest => "Chestplate",
            ItemSlot.Boots => "Boots",
            ItemSlot.Helm => "Helm",
            _ => "Amulet",
        };
    }

    public static string statPrefix(Stat stat)
    {
        return stat switch
        {
            Stat.Length => "Long",
            Stat.Width => "Broad",
            Stat.Range => "Far",
            Stat.Knockback => "Pushing",
            Stat.Knockup => "Upward",
            Stat.Stagger => "Heavy",
            Stat.Charges => "Multi",
            Stat.Mezmerize => "Stunning",
            Stat.MovespeedCast => "Agile",
            _ => "Strange",
        };
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
"Endless",
"Pinnacle",
"Destroyer",
"Swift",
"Drunken",
"Blade",
"Edge",
"Needle",
"Billowing",
"Impossible",
"Erupting",
"Charged",
"Hammer",
"Brick",
"Sick",
"Omen",
"Blood",
"Felling",
"Brutal",
"Sly",
"Cunning",
"Unstopable",
"Death",
"Rogue",
"Aligned",
"Hollow"


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
"Kick",
"Obliterate",
"Twist",
"Impale",
"Ripper",
"Rage",
"End",
"Chop",
"Storm",
"Weapon",
"Star",
"Divide",
"Call",
"Surge",
"Blow",
"Bite",



    };
}
