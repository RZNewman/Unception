using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillPlane : MonoBehaviour
{
    Atlas atlas;
    GameObject spawn;
    GlobalPlayer gp;
    private void Start()
    {
        atlas = FindObjectOfType<Atlas>(true);
        spawn = GameObject.FindWithTag("Spawn");
        gp = FindObjectOfType<GlobalPlayer>(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        FloorNormal norm = other.GetComponentInParent<FloorNormal>();
        UnitPropsHolder props = other.GetComponentInParent<UnitPropsHolder>();
        LifeManager life = other.GetComponentInParent<LifeManager>();
        UnitMovement mover = other.GetComponentInParent<UnitMovement>();
        MusicBox music = FindObjectOfType<MusicBox>();
        Health hp = other.GetComponentInParent<Health>();
        Size s = other.GetComponent<Size>();

        if (props && props.props.isPlayer)
        {
            //TODO atlas clean
            if (atlas.canLaunch)
            {
                if (props.launchedPlayer)
                {
                    hp.takePercentDamage(0.15f);
                    mover.stop(true);
                    mover.sound.playSound(UnitSound.UnitSoundClip.Fall);
                    norm.transform.position = norm.nav + Vector3.up * s.scaledHalfHeight;
                }
                else
                {
                    atlas.setScaleServer(Power.scaleNumerical(atlas.currentMap.power), Power.scaleNumerical(gp.serverPlayer.power));
                    mover.GetComponent<Power>().rescale();
                    FindObjectOfType<MaterialScaling>().game(FindObjectOfType<LocalCamera>().cameraMagnitude);
                    music.Game();
                    props.launchedPlayer = true;
                    mover.transform.position = atlas.playerSpawn;
                    
                }
                
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
