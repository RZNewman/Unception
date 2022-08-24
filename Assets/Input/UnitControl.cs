using System;
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
    }
    public UnitInput getUnitInuput();

    public void refreshInput();

    public void init();
    public void reset();
}
