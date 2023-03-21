using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static RewardManager;

public class ItemDrop : MonoBehaviour
{
    public ColorIndividual itemAura;
    public ColorIndividual itemBase;
    GameObject target;
    SoundManager sound;

    public float waitTime = 3f;

    public float accel = 1f;
    float catchDistance = 1f;

    Gravity grav;
    Rigidbody rb;

    float targetSpeed = 0;
    public void init(float power, GameObject t, Quality q)
    {
        float scalePhys = Power.scalePhysical(power);
        float scaleSpeed = Power.scaleSpeed(power);

        transform.localScale = Vector3.one * scalePhys;
        grav = GetComponent<Gravity>();
        grav.gravity *= scaleSpeed;
        accel *= scaleSpeed;
        catchDistance *= scalePhys;
        Vector2 dir = Random.insideUnitCircle;
        GetComponent<Rigidbody>().velocity = new Vector3(dir.x * 4, 8, dir.y * 4) * scaleSpeed;
        Color qual = RewardManager.colorQuality(q);
        qual.a = 0.05f;
        itemAura.setColor(qual);
        itemBase.setColor(qual);
        target = t;
    }
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        sound = FindObjectOfType<SoundManager>();
    }

    private void Update()
    {

        if (waitTime > 0)
        {
            waitTime -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        if (!target)
        {
            Destroy(gameObject);
            return;
        }
        if (waitTime <= 0)
        {
            grav.gravity = 0;
            Vector3 dir = target.transform.position - transform.position;

            targetSpeed += accel * Time.fixedDeltaTime;
            rb.velocity = targetSpeed * dir.normalized;
            if (dir.magnitude < catchDistance)
            {
                sound.playSound(SoundManager.SoundClip.Slurp, transform.position);
                Destroy(gameObject);
            }
        }

    }
}
