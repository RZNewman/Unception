using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Healthbar : MonoBehaviour
{
    public RectTransform barBack;
    public RectTransform barFront;
    Health health;
    // Start is called before the first frame update
    void Start()
    {
        health = GetComponentInParent<UnitMovement>().GetComponentInChildren<Health>();
    }

    // Update is called once per frame
    void Update()
    {
        if (health)
        {
            fill(health.percent);
        }
    }

    void fill(float percent)
    {
        barFront.sizeDelta = new Vector2(percent, barFront.sizeDelta.y);
        barBack.sizeDelta = new Vector2(1-percent, barFront.sizeDelta.y);
    }
}
