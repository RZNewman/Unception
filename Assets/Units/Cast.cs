using Mirror;
using System.Collections.Generic;
using UnityEngine;
using static AttackUtils;
using static GenerateAttack;
using static GenerateDash;
using static GenerateHit;

public class Cast : MonoBehaviour, BarValue
{
    BarValue target;



    private void Start()
    {

    }
    public BarValue.BarData getBarFill()
    {
        if (target == null)
        {
            return new BarValue.BarData
            {
                color = Color.cyan,
                fillPercent = 0,
                active = false,
            };
        }
        return target.getBarFill();
    }

    public void setTarget(BarValue s)
    {
        target = s;
    }
    public void removeTarget(BarValue s)
    {
        if (target == s)
        {
            target = null;
        }

    }
    public void OrderedTransition()
    {

        //TODO can all ticks be managed here?
        List<MachineUpdate> toRemove = new List<MachineUpdate>();
        machines.ForEach(m =>
        {
            if (!m.indicatorOnly)
            {
                m.machine.transition();
                if (m.machine.hasFinished)
                {
                    m.machine.exit();
                    toRemove.Add(m);
                    m.callback(m.machine);
                }
            }
        });
        toRemove.ForEach(m => machines.Remove(m));
    }

    private void OnDestroy()
    {
        machines.ForEach(m =>
        {
            m.machine.exit();
        });
    }

    public void OrderedUpdate()
    {


        machines.ForEach(m =>
        {
            if (!m.indicatorOnly) { m.machine.tick(); }
        });
    }

    public void OrderedIndicator()
    {


        machines.ForEach(m => m.machine.indicatorUpdate());
    }
    struct MachineUpdate
    {
        public AttackMachine machine;
        public bool indicatorOnly;
        public MachineEndCallback callback;
    }
    List<MachineUpdate> machines = new List<MachineUpdate>();

    public delegate void MachineEndCallback(AttackMachine m);
    MachineEndCallback nothingCallback = (_) => { };

    public void addSource(AttackMachine source, bool indOnly = false)
    {
        machines.Add(new MachineUpdate { machine = source, indicatorOnly = indOnly, callback = nothingCallback });
    }
    public void addSource(AttackMachine source, MachineEndCallback call, bool indOnly = false)
    {
        machines.Add(new MachineUpdate { machine = source, indicatorOnly = indOnly, callback = call });
    }

    public void removeSource(AttackMachine source)
    {
        machines.Remove(machines.Find(m => m.machine == source));
    }



}
