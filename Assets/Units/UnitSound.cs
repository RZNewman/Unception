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
            playSound(SoundClip.Spawn);
        }
        GetComponent<Power>().subscribePower(setSoundFalloff);
    }


    void setSoundFalloff(Power p)
    {
        dists = new AudioDistances
        {
            min = 4 * p.scale(),
            max = 100 * p.scale(),
        };
        source.minDistance= dists.min;
        source.maxDistance = dists.max;
    }
    public static void setAudioDistances(GameObject o, AudioDistances dists)
    {
        AudioSource source = o.GetComponent<AudioSource>();
        source.minDistance = dists.min;
        source.maxDistance = dists.max;
    }

    public enum SoundClip
    {
        Spawn,
        Stun,
        LocalStun,
        Fall,
        Dash,
    }

    public void playSound(SoundClip sound)
    {
        AudioClip clip = null;
        switch (sound)
        {
            case SoundClip.Spawn:
                clip = spawn;
                break;
            case SoundClip.Stun:
                clip = stun;
                break;
            case SoundClip.Dash:
                clip = dash;
                break;
            case SoundClip.Fall:
                clip = fall;
                break;
            case SoundClip.LocalStun:
                clip = localStun;
                break;
        }

        source.clip = clip;
        source.Play();

    }


}

