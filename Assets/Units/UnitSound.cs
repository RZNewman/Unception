using Mono.CompilerServices.SymbolWriter;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSound : MonoBehaviour
{
    public AudioClip spawn;
    public AudioClip localStun;
    public AudioClip fall;

    public AudioClip stun;
    public AudioClip dash;

    public struct AudioDistances
    {
        public float min;
        public float max;
    }

    public AudioDistances dists;

    AudioSource source;
    private void Start()
    {
        source = GetComponent<AudioSource>();
        UnitProperties props = GetComponent<UnitPropsHolder>().props;
        if (props.isPlayer)
        {
            playSound(UnitSoundClip.Spawn);
        }
        GetComponent<Power>().subscribePower(setSoundFalloff);
    }


    void setSoundFalloff(Power p)
    {
        float scalePhys = p.scalePhysical();
        dists = new AudioDistances
        {
            min = 4 * scalePhys,
            max = 100 * scalePhys,
        };
        source.minDistance = dists.min;
        source.maxDistance = dists.max;
    }
    public static void setAudioDistances(GameObject o, AudioDistances dists)
    {
        AudioSource source = o.GetComponent<AudioSource>();
        source.minDistance = dists.min;
        source.maxDistance = dists.max;
    }

    public enum UnitSoundClip
    {
        Spawn,
        Stun,
        LocalStun,
        Fall,
        Dash,
    }

    public void playSound(UnitSoundClip sound)
    {
        AudioClip clip = null;
        switch (sound)
        {
            case UnitSoundClip.Spawn:
                clip = spawn;
                break;
            case UnitSoundClip.Stun:
                clip = stun;
                break;
            case UnitSoundClip.Dash:
                clip = dash;
                break;
            case UnitSoundClip.Fall:
                clip = fall;
                break;
            case UnitSoundClip.LocalStun:
                clip = localStun;
                break;
        }

        source.clip = clip;
        source.Play();

    }


}

