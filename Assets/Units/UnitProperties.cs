using System;
using UnityEngine;

public class UnitProperties : ScriptableObject
{
    public bool isPlayer;

    public float maxSpeed = 5f;
    public float acceleration = 5f;
    public float decceleration = 5f;
    public float friction = 5f;
    public float jumpForce = 10f;
    public float jumpsquatTime = 0.8f;
    public float lookSpeedDegrees = 270f;
    public float sidewaysMoveMultiplier = 0.85f;
    public float backwardsMoveMultiplier = 0.7f;
    public float dashDistance = 3f;
    public float dashSpeed = 9f;
    public bool canFloat = false;

    public float maxHealthMult;

    public float maxPosture;
    public float maxFocus;

    public float maxStamina;
    public float staminaRecover;

    public UnitVisuals visuals;
}

[Serializable]
public struct UnitVisuals
{
    public Color[] colors;
    public int[] parts;
}
