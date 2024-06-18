using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    UnitMovement mover;
    Power power;
    Knockdown knockdown;
    UnitProperties props;
    public Animator anim;

    float moveSpeedMult;

    enum AnimationState
    {
        Idle = 0,
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
        knockdown = GetComponent<Knockdown>();
        props = GetComponent<UnitPropsHolder>().props;
        anim.SetFloat("Jumpsquat Time", props.jumpsquatTime);
        power.subscribePower(scaleMovement);
    }

    void scaleMovement(Power p)
    {
        moveSpeedMult = 1.0f / (2 * power.scalePhysical());
    }

    // Update is called once per frame
    public void OrderedUpdate()
    {
        anim.SetBool("Grounded", mover.grounded);
        anim.SetBool("KnockedDownBool", knockdown.knockedDown);
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

    public void Reset()
    {
        anim.SetInteger("BaseState", (int)AnimationState.Idle);
        anim.SetFloat("MoveSpeed", 0);
    }

    public void setAttack()
    {
        anim.SetTrigger("Attack");
    }
    public void setKnockedDown()
    {
        anim.SetTrigger("KnockedDown");
    }
}
