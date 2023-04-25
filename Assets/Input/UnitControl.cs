using System;
using System.Collections.Generic;
using UnityEngine;
using static GenerateAttack;
using static Keybinds;
using static Utils;

public interface UnitControl
{
    [Serializable]
    public struct UnitInput
    {
        public Vector2 move;
        public Vector3 lookOffset;
        public bool jump;
        public bool dash;
        public bool cancel;
        public ItemSlot[] attacks;


        public void reset()
        {
            move = Vector2.zero;
            lookOffset = Vector3.zero;
            jump = false;
            dash = false;
            cancel = false;
            attacks = new ItemSlot[0];
        }
        public void cleanButtons()
        {
            jump = false;
            dash = false;
            cancel = false;
            attacks = new ItemSlot[0];
        }
        public Optional<ItemSlot> popKey()
        {
            if (attacks.Length == 0)
            {
                return new Optional<ItemSlot>();
            }
            ItemSlot key = attacks[0];

            ItemSlot[] nextArray = new ItemSlot[attacks.Length - 1];
            for (int i = 1; i < attacks.Length; i++)
            {
                nextArray[i - 1] = attacks[i];
            }
            attacks = nextArray;


            return new Optional<ItemSlot>(key);
        }
        public static UnitInput zero()
        {
            return new UnitInput
            {
                move = Vector2.zero,
                lookOffset = Vector3.zero,
                jump = false,
                dash = false,
                cancel = false,
                attacks = new ItemSlot[0],
            };
        }
        public UnitInput merge(UnitInput newer)
        {
            HashSet<ItemSlot> atks = new HashSet<ItemSlot>();
            foreach (ItemSlot k in attacks)
            {
                atks.Add(k);
            }
            foreach (ItemSlot k in newer.attacks)
            {
                atks.Add(k);
            }
            ItemSlot[] aArray = new ItemSlot[atks.Count];
            atks.CopyTo(aArray);
            return new UnitInput
            {
                move = newer.move,
                lookOffset = newer.lookOffset,
                jump = jump || newer.jump,
                dash = dash || newer.dash,
                cancel = cancel || newer.cancel,
                attacks = aArray,
            };
        }
        public Vector2 look
        {
            get
            {
                Vector3 dir = this.lookOffset;
                dir.y = 0;
                dir.Normalize();
                return vec2input(dir);
            }
        }
    }

    public static KeyName toKeyName(ItemSlot slot)
    {
        return (KeyName)((int)slot + (int)KeyName.Attack1);
    }
    public static ItemSlot fromKeyName(KeyName key)
    {
        return (ItemSlot)((int)key - (int)KeyName.Attack1);
    }
    public UnitInput getUnitInuput();

    public void refreshInput();

    public void init();
}
