using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    UnitMovement mover;
    Power power;
    UnitProperties props;
    public Animator anim;

    float moveSpeedMult;

    enum AnimationState
    {
        Idle=0,
        Stunned,
        Dash,
        Jumpsquat,
    }
    public enum MoveDirection
    {
        None = 0,
        Forward,
        Right,
        Backward,
        Left,
    }
    // Start is called before the first frame update
    void Start()
    {
        mover = GetComponent<UnitMovement>();
        power = GetComponent<Power>();
        props = GetComponent<UnitPropsHolder>().props;
        anim.SetFloat("Jumpsquat Time", props.jumpsquatTime);
        moveSpeedMult = 1.0f / (2 * power.scale());
        //TODO subscribe scale
    }

    // Update is called once per frame
    public void OrderedUpdate()
    {
        anim.SetBool("Grounded", mover.grounded);
        anim.SetFloat("MoveSpeed", mover.planarVelocity.magnitude * moveSpeedMult);
        
        switch (mover.currentState())
        {
            case StunnedState s:
                anim.SetInteger("BaseState", (int)AnimationState.Stunned);
                break;
            case DashState s:
                anim.SetInteger("BaseState", (int)AnimationState.Dash);
                break;
            case JumpsquatState s:
                anim.SetInteger("BaseState", (int)AnimationState.Jumpsquat);
                break;
            default:
                anim.SetInteger("BaseState", (int)AnimationState.Idle);
                anim.SetInteger("MoveDirection", (int)mover.moveDirection);
                break;
        }
    } 

    public void setAttack()
    {
        anim.SetTrigger("Attack");
    }
}
