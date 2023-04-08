using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StatTypes;

public class Buff : NetworkBehaviour
{
    [SyncVar]
    float duration;

    float timeScale;

    StatHandler sth;
    public IDictionary<Stat, float> stats
    {
        get
        {
            return sth.stats;
        }
    }

    public float relativeScale(float targetTimeScale)
    {
        return timeScale / targetTimeScale;
    }
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<ClientAdoption>().trySetAdopted();
        sth = GetComponent<StatHandler>();
        sth.subscribe();
        transform.parent.GetComponent<BuffManager>().addBuff(this);
    }
    private void OnDestroy()
    {

    }
    public void setup(float durr, float timeS)
    {
        duration = durr;
        timeScale = timeS;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        duration -= Time.fixedDeltaTime;
        if (isServer && duration <= 0)
        {
            transform.parent.GetComponent<BuffManager>().removeBuff(this);
            Destroy(gameObject);
        }
    }
}
