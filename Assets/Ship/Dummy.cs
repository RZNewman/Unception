using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Dummy : MonoBehaviour, TeamOwnership
{
    EventManager events;
    Health health;
    public uint getTeam()
    {
        return TeamOwnership.ENEMY_TEAM;
    }

    // Start is called before the first frame update
    void Start()
    {
        events = GetComponent<EventManager>();
        health = GetComponent<Health>();
        health.setMaxHealth(10_000);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        events.fireTick();
        health.healPercent(0.1f * Time.fixedDeltaTime);
    }
}
