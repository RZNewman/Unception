using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GenerateHit;

public class ProjectileIndicatorVisuals : HitIndicatorInstance
{



    public GameObject shot;
    public GameObject progress;

    float length;
    float width;
    // Start is called before the first frame update


    protected override void setSize()
    {

        length = data.range * 0.3f;
        width = data.width;

        Quaternion turn = Quaternion.LookRotation(Vector3.down, Vector3.forward);
        

        shot.transform.localScale = new Vector3(width, length);
        shot.transform.localPosition = new Vector3(0,0, length / 2);

        progress.transform.localScale = new Vector3(width, 0);

        shot.transform.localRotation = turn;
        progress.transform.localRotation = turn;

    }

    public override void setColor(Color color, Color stunning)
    {

        shot.GetComponent<SpriteRenderer>().color = stunning;
        progress.GetComponent<SpriteRenderer>().color = color;
    }

    protected override void setCurrentProgress(float percent)
    {
        float length_percent = length * percent;
        progress.transform.localScale = new Vector3(progress.transform.localScale.x, length_percent);
        progress.transform.localPosition = new Vector3(0,0, length_percent / 2);
    }

    
}
