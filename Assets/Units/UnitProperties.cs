using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitProperties : ScriptableObject
{
    public bool isPlayer;

    public float maxSpeed = 5f;
    public float acceleration = 5f;
    public float decceleration = 5f;
    public float jumpForce = 10f;
    public float jumpsquatTime = 0.8f;
    public float lookSpeedDegrees = 270f;
    public float sidewaysMoveMultiplier = 0.85f;
    public float backwardsMoveMultiplier = 0.7f;

    public float maxHealth;

    public float maxPosture;
    public float passivePostureRecover;
    public float stunnedPostureRecover;
    public float stunnedPostureRecoverAcceleration;

    public int visualsId;
}