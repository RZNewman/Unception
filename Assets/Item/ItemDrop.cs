using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static RewardManager;

public class ItemDrop : MonoBehaviour
{
    public ColorIndividual itemAura;
    public ColorIndividual itemBase;
    GameObject target;

    public float waitTime = 3f;

    public float accel = 1f;
    float catchDistance = 1f;

    Gravity grav;
    Rigidbody rb;

    float targetSpeed = 0;
    public void init(float scale, GameObject t, Quality q)
    {
        transform.localScale = Vector3.one * scale;
        grav = GetComponent<Gravity>();
        grav.gravity *= scale;
        accel *= scale;
        catchDistance *= scale;
        Vector2 dir = Random.insideUnitCircle;
        GetComponent<Rigidbody>().velocity = new Vector3(dir.x * 4, 8, dir.y * 4) * scale;
        Color qual = RewardManager.colorQuality(q);
        qual.a = 0.05f;
        itemAura.setColor(qual);
        itemBase.setColor(qual);
        target = t;
    }
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (waitTime > 0)
        {
            waitTime -= Time.deltaTime;
        }
        else
        {
            grav.gravity = 0;
            Vector3 dir = target.transform.position - transform.position;

            targetSpeed += accel * Time.fixedDeltaTime;
            rb.velocity = targetSpeed * dir.normalized;
            if (dir.magnitude < catchDistance)
            {
                Destroy(gameObject);
            }
        }

    }
}
