using System;
using System.Collections.Generic;
using UnityEngine;
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
        public AttackKey[] attacks;


        public void reset()
        {
            move = Vector2.zero;
            lookOffset = Vector3.zero;
            jump = false;
            dash = false;
            attacks = new AttackKey[0];
        }
        public void cleanButtons()
        {
            jump = false;
            dash = false;
            attacks = new AttackKey[0];
        }
        public AttackKey popKey()
        {
            if (attacks.Length == 0)
            {
                return AttackKey.None;
            }
            AttackKey key = attacks[0];

            AttackKey[] nextArray = new AttackKey[attacks.Length - 1];
            for (int i = 1; i < attacks.Length; i++)
            {
                nextArray[i - 1] = attacks[i];
            }
            attacks = nextArray;


            return key;
        }
        public static UnitInput zero()
        {
            return new UnitInput
            {
                move = Vector2.zero,
                lookOffset = Vector3.zero,
                jump = false,
                dash = false,
                attacks = new AttackKey[0],
            };
        }
        public UnitInput merge(UnitInput newer)
        {
            HashSet<AttackKey> atks = new HashSet<AttackKey>();
            foreach (AttackKey k in attacks)
            {
                atks.Add(k);
            }
            foreach (AttackKey k in newer.attacks)
            {
                atks.Add(k);
            }
            AttackKey[] aArray = new AttackKey[atks.Count];
            atks.CopyTo(aArray);
            return new UnitInput
            {
                move = newer.move,
                lookOffset = newer.lookOffset,
                jump = jump || newer.jump,
                dash = dash || newer.dash,
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
    [Serializable]
    public enum AttackKey : byte
    {
        One = 0,
        Two,
        Three,
        Four,

        None,
    }
    public UnitInput getUnitInuput();

    public void refreshInput();

    public void init();
}
