using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GenerateAttack;
using static RewardManager;

public class ItemDrop : MonoBehaviour
{
    public ColorIndividual itemAura;
    public ColorIndividual itemBase;
    GameObject target;
    SoundManager sound;

    public float waitTimeStart = 3f;
    public float waitTime;

    public float accel = 1f;
    float catchDistance = 1f;

    Gravity grav;
    Rigidbody rb;

    Quality quality;
    AttackFlair flair;

    float targetSpeed = 0;
    public void init(Scales scales, GameObject t, Quality q, AttackFlair f)
    {
        float scalePhys = scales.world;
        float scaleSpeed = scales.speed;

        transform.localScale = Vector3.one * scalePhys;
        grav = GetComponent<Gravity>();
        grav.gravity *= scaleSpeed;
        accel *= scaleSpeed;
        catchDistance *= scalePhys;
        Vector2 dir = Random.insideUnitCircle;
        GetComponent<Rigidbody>().velocity = new Vector3(dir.x * 4, 8, dir.y * 4) * scaleSpeed;
        Color qual = RewardManager.colorQuality(q);
        qual.a = 0.05f;

        quality = q;
        flair = f;

        itemAura.setColor(qual);
        itemBase.setColor(qual);
        target = t;
    }
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        sound = FindObjectOfType<SoundManager>();
        waitTime = waitTimeStart;
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
        Vector3 dir = target.transform.position - transform.position;
        if (waitTime <= 0)
        {
            grav.gravity = 0;
            

            targetSpeed += accel * Time.fixedDeltaTime;
            rb.velocity = targetSpeed * dir.normalized;
            
        }
        if(waitTime < waitTimeStart - 1f)
        {
            if (dir.magnitude < catchDistance)
            {
                sound.playSound(SoundManager.SoundClip.Slurp, transform.position);
                FindObjectOfType<UiItemPopups>().createPop(quality, flair);
                Destroy(gameObject);
            }
        }

    }
}
