using System;
using System.Collections.Generic;
using UnityEngine;
using static GenerateAttack;
using static Keybinds;
using static Utils;

public interface UnitControl
{
    [Serializable]
    public class UnitInput
    {
        public Vector2 move;
        public Vector3 lookOffset;
        public bool jump;
        public bool dash;
        public bool cancel;
        public bool interact;
        public bool recall;
        public ItemSlot[] attacks;


        public void reset()
        {
            move = Vector2.zero;
            lookOffset = Vector3.zero;
            jump = false;
            dash = false;
            cancel = false;
            interact = false;
            recall = false;
            attacks = new ItemSlot[0];
        }

        public bool consumeJump()
        {
            bool j = jump;
            jump = false;
            return j;
        }

        public void cleanButtons()
        {
            jump = false;
            dash = false;
            cancel = false;
            interact=false;
            recall = false;
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
                interact = false,
                recall = false,
                attacks = new ItemSlot[0],
            };
        }
        public void merge(UnitInput newer)
        {
            move = newer.move;
            lookOffset = newer.lookOffset;
            jump = jump || newer.jump;
            dash = dash || newer.dash;
            cancel = cancel || newer.cancel;
            interact = interact || newer.interact;
            recall = recall || newer.recall;
            HashSet<ItemSlot> atks = new HashSet<ItemSlot>();
            foreach (ItemSlot k in attacks)
            {
                atks.Add(k);
            }
            foreach (ItemSlot k in newer.attacks)
            {
                atks.Add(k);
            }
            attacks = new ItemSlot[atks.Count];
            atks.CopyTo(attacks);
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
        public float lookVerticalAngle
        {
            get
            {
                Vector3 flatLookDiff = Vector3.ProjectOnPlane(lookOffset, Vector3.up);
                Vector3 cross = Vector3.Cross(flatLookDiff, lookOffset);
                //despite beign signed, the cross product will always be positive
                float angle = Vector3.SignedAngle(flatLookDiff, lookOffset, cross);
                if (lookOffset.y> flatLookDiff.y)
                {
                    angle = -angle;
                }
                return angle;
            }
        }

        public bool lookObstructed(Vector3 position)
        {
            Debug.DrawLine(position, position + lookOffset, Color.black);
            return Physics.Raycast(position,lookOffset,lookOffset.magnitude, LayerMask.GetMask("Terrain")) ;
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
