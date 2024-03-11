using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StatTypes;
using Mirror;
using static GenerateHit;
using static GenerateAttack;
using static AttackUtils;

public class StatHandler : NetworkBehaviour
{
    StatStream stream = new StatStream();
    readonly SyncDictionary<Stat, float> expressedStats = new SyncDictionary<Stat, float>();

    public IDictionary<Stat, float> stats
    {
        get
        {
            return expressedStats;
        }
    }

    void mirrorStream(IDictionary<Stat, float> delta)
    {

        foreach (Stat key in delta.Keys)
        {
            if (expressedStats.ContainsKey(key))
            {
                expressedStats[key] += delta[key];
            }
            else
            {
                expressedStats[key] = delta[key];
            }
        }
    }

    public float getValue(Stat stat, Scales scale)
    {
        return stream.getValue(stat, scale);
    }

    bool subscribed = false;
    private void Start()
    {
        if (isServer)
        {
            subscribe();
        }

    }

    public void subscribe()
    {
        if (!subscribed)
        {
            stream.subscribe(mirrorStream);
            subscribed = true;
        }
    }

    public void link(StatHandler downstream, float effectMult = 1)
    {
        StatStream.linkStreams(stream, downstream.stream, effectMult);
    }

    public void link(StatStream downstream, float effectMult = 1)
    {
        StatStream.linkStreams(stream, downstream, effectMult);
    }

    private void OnDestroy()
    {
        if (isServer)
        {
            stream.terminateStreams();
        }
    }

    public void setStats(Dictionary<Stat, float> newStats)
    {
        stream.setStats(newStats);
    }

    public void addStats(Dictionary<Stat, float> delta)
    {
        stream.addStats(delta);
    }
    public float get(Stat stat)
    {
        float value;
        return expressedStats.TryGetValue(stat, out value) ? value : 0;
    }

    public void debugStats()
    {
        string statString = "";
        foreach (Stat key in expressedStats.Keys)
        {
            statString += key + ": " + expressedStats[key] + "\n";
        }
        Debug.Log(statString);
    }

}

public class StatStream
{
    Dictionary<Stat, float> objectStats = new Dictionary<Stat, float>();
    Dictionary<Stat, float> expressedStats = new Dictionary<Stat, float>();

    public IDictionary<Stat, float> stats
    {
        get { return expressedStats; }
    }

    public void setStats(Dictionary<Stat, float> newStats)
    {
        Dictionary<Stat, float> delta = objectStats.invert().sum(newStats);
        objectStats = newStats;
        updateExpression(delta);
    }

    public void addStats(Dictionary<Stat, float> delta)
    {
        objectStats = objectStats.sum(delta);
        updateExpression(delta);
    }
    public float getValue(Stat stat, Scales scales, HitType hitType, EffectShape shape)
    {
        float value;
        return expressedStats.TryGetValue(stat, out value) ? statToValue(stat, value, scales, hitType, shape) : 0;
    }
    public float getValue(Stat stat, Scales scales)
    {
        float value;
        return expressedStats.TryGetValue(stat, out value) ? statToValue(stat, value, scales) : 0;
    }

    #region streams
    List<StatStream> Upstream = new List<StatStream>();
    List<StatStream> Downstream = new List<StatStream>();
    Dictionary<StatStream, float> downstreamMultiplers = new Dictionary<StatStream, float>();
    public delegate void DeltaCallback(IDictionary<Stat, float> delta);
    List<DeltaCallback> callbacks = new List<DeltaCallback>();

    public void subscribe(DeltaCallback callback)
    {
        callbacks.Add(callback);
        callback(expressedStats);
    }

    public static void linkStreams(StatStream up, StatStream down, float effectMult = 1)
    {
        if (up == down)
        {
            throw new System.Exception("Same stat handler link");
        }
        down._addUpstream(up);
        up._addDownstream(down, effectMult);

    }

    public static void unlinkStreams(StatStream up, StatStream down)
    {
        down._removeUpstream(up);
        up._removeDownstream(down);
    }
    void _addUpstream(StatStream up)
    {
        Upstream.Add(up);
    }
    void _removeUpstream(StatStream up)
    {
        Upstream.Remove(up);
    }
    void _addDownstream(StatStream down, float effectMult)
    {
        Downstream.Add(down);
        downstreamMultiplers.Add(down, effectMult);
        down.updateExpression(expressedStats.scale(effectMult));
    }
    void _removeDownstream(StatStream down)
    {
        Downstream.Remove(down);
        down.updateExpression(expressedStats.scale(downstreamMultiplers[down]).invert());
        downstreamMultiplers.Remove(down);
    }


    public void terminateStreams()
    {

        List<StatStream> clean = new List<StatStream>();
        foreach (StatStream h in Downstream)
        {
            clean.Add(h);

        }
        foreach (StatStream down in clean)
        {
            unlinkStreams(this, down);
        }

        clean = new List<StatStream>();

        foreach (StatStream h in Upstream)
        {
            clean.Add(h);

        }
        foreach (StatStream up in clean)
        {
            unlinkStreams(up, this);
        }

    }

    void updateExpression(IDictionary<Stat, float> delta)
    {
        foreach (Stat key in delta.Keys)
        {
            if (expressedStats.ContainsKey(key))
            {
                expressedStats[key] += delta[key];
            }
            else
            {
                expressedStats[key] = delta[key];
            }
        }
        foreach (DeltaCallback d in callbacks)
        {
            d(delta);
        }
        foreach (StatStream s in Downstream)
        {
            s.updateExpression(delta);
        }
    }
    #endregion
}
