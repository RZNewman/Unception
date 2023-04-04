using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StatTypes;

public class Buff : NetworkBehaviour
{
    [SyncVar]
    float duration;

    StatHandler sth;
    public IDictionary<Stat, float> stats
    {
        get
        {
            return sth.stats;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        sth = GetComponent<StatHandler>();
        if (isServer)
        {
            StatHandler.linkStreams(GetComponent<StatHandler>(), transform.parent.GetComponent<StatHandler>());
        }
        transform.parent.GetComponent<BuffManager>().addBuff(this);
    }
    private void OnDestroy()
    {
        if (isServer)
        {
            StatHandler.unlinkStreams(GetComponent<StatHandler>(), transform.parent.GetComponent<StatHandler>());
        }
        transform.parent.GetComponent<BuffManager>().removeBuff(this);
    }
    public void setDuration(float d)
    {
        duration = d;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        duration -= Time.fixedDeltaTime;
        if (isServer && duration <= 0)
        {
            Destroy(gameObject);
        }
    }
}
