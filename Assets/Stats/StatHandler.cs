using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StatTypes;
using Mirror;

public class StatHandler : NetworkBehaviour
{
    Dictionary<Stat, float> objectStats = new Dictionary<Stat, float>();
    readonly SyncDictionary<Stat, float> expressedStats = new SyncDictionary<Stat, float>();

    public IDictionary<Stat, float> stats
    {
        get { return expressedStats; }
    }



    private void Start()
    {
        updateExpression(objectStats);
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
    public float get(Stat stat)
    {
        float value;
        return expressedStats.TryGetValue(stat, out value) ? value : 0;
    }

    #region streams
    List<StatHandler> Upstream = new List<StatHandler>();
    List<StatHandler> Downstream = new List<StatHandler>();
    Dictionary<StatHandler, float> downstreamMultiplers = new Dictionary<StatHandler, float>();
    public static void linkStreams(StatHandler up, StatHandler down, float effectMult = 1)
    {
        if (up == down)
        {
            throw new System.Exception("Same stat handler link");
        }
        down._addUpstream(up);
        up._addDownstream(down, effectMult);

    }

    public static void unlinkStreams(StatHandler up, StatHandler down)
    {
        down._removeUpstream(up);
        up._removeDownstream(down);
    }
    void _addUpstream(StatHandler up)
    {
        Upstream.Add(up);
    }
    void _removeUpstream(StatHandler up)
    {
        Upstream.Remove(up);
    }
    void _addDownstream(StatHandler down, float effectMult)
    {
        Downstream.Add(down);
        downstreamMultiplers.Add(down, effectMult);
        down.updateExpression(expressedStats.scale(effectMult));
    }
    void _removeDownstream(StatHandler down)
    {
        Downstream.Remove(down);
        down.updateExpression(expressedStats.scale(downstreamMultiplers[down]).invert());
        downstreamMultiplers.Remove(down);
    }

    [Server]
    void terminateStreams()
    {

        List<StatHandler> clean = new List<StatHandler>();
        foreach (StatHandler h in Downstream)
        {
            clean.Add(h);

        }
        foreach (StatHandler down in clean)
        {
            unlinkStreams(this, down);
        }

        clean = new List<StatHandler>();

        foreach (StatHandler h in Upstream)
        {
            clean.Add(h);

        }
        foreach (StatHandler up in clean)
        {
            unlinkStreams(up, this);
        }

    }
    [Server]
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
        foreach (StatHandler s in Downstream)
        {
            s.updateExpression(delta);
        }
    }
    #endregion
}
