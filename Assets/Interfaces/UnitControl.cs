using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface UnitControl
{
    [Serializable]
    public struct UnitInput
	{
        public Vector2 move;
        public Vector2 look;
        public bool jump;
        public AttackKey[] attacks;
        

        public void reset()
		{
            jump = false;
            attacks = new AttackKey[0];
		}
	}
    [Serializable]
    public enum AttackKey:byte
	{
        One =0,
        Two
	}
    public UnitInput getUnitInuput();

    public void refreshInput();
}
