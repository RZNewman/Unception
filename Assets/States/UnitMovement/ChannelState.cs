using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnitControl;

public class ChannelState : PlayerMovementState, BarValue
{
    string channelText;
    Action callbackSuccess;

    static readonly Color channelColor = new Color(0, 0.6f, 1);

    public ChannelState(UnitMovement m,float durration, string text,Action callback) : base(m, durration)
    {
        channelText = text;
        callbackSuccess = callback;
    }
    public override void enter()
    {
        mover.GetComponent<UnitRingInd>().addColor(channelColor);
        mover.GetComponent<Cast>().setTarget(this);
    }
    public override void exit(bool expired)
    {
        mover.GetComponent<UnitRingInd>().removeColor(channelColor);
        mover.GetComponent<Cast>().removeTarget(this);
        if (expired)
        {
            callbackSuccess();
        }
    }


    public BarValue.BarData getBarFill()
    {
        Color c = channelColor;
        c.a = 0.6f;
        return new BarValue.BarData
        {
            color = c,
            fillPercent = Mathf.Clamp01(currentDurration / maxDuration),
            active = true,
            text = channelText,
        };
    }

    public override void tick()
    {
        base.tick();
        UnitInput inp = mover.input;

        mover.rotate(inp, false,0.5f);
        mover.move(inp,0.5f);

    }
    public override StateTransition transition()
    {
        if (mover.isIncapacitated)
        {
            return new StateTransition(new StunnedState(mover), true);
        }
        if (mover.input.cancel)
        {
            return new StateTransition(new FreeState(mover), true);
        }

        return base.transition();
    }
}
