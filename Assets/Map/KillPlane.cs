using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillPlane : MonoBehaviour
{
    Atlas atlas;
    GameObject spawn;
    private void Start()
    {
        atlas = FindObjectOfType<Atlas>(true);
        spawn = GameObject.FindWithTag("Spawn");
    }

    private void OnTriggerEnter(Collider other)
    {
        FloorNormal norm = other.GetComponentInParent<FloorNormal>();
        UnitPropsHolder props = other.GetComponentInParent<UnitPropsHolder>();
        LifeManager life = other.GetComponentInParent<LifeManager>();
        UnitMovement mover = other.GetComponentInParent<UnitMovement>();
        Health hp = other.GetComponentInParent<Health>();
        Size s = other.GetComponent<Size>();

        if (props && props.props.isPlayer)
        {
            //TODO atlas clean
            if (atlas.canLaunch)
            {
                hp.takePercentDamage(0.15f);
                mover.stop(true);
                mover.sound.playSound(UnitSound.UnitSoundClip.Fall);
                norm.transform.position = norm.nav + Vector3.up * s.scaledHalfHeight;
            }
            else
            {
                mover.sound.playSound(UnitSound.UnitSoundClip.Fall);
                mover.transform.position = spawn.transform.position;
            }
            
            
        }
        else
        {
            life.die();
        }
    }
}
