using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static SoundManager;
using static UnitSound;

public class UnitSound : MonoBehaviour
{
    public AudioClip spawn;
    public AudioClip localStun;
    public AudioClip fall;

    public AudioClip stun;
    public AudioClip dash;
    public AudioClip portalStart;
    public AudioClip portalEnd;

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
        PortalStart,
        PortalEnd,
    }

    public void playSound(UnitSoundClip sound, float? forcedDuration = null)
    {
        AudioClip clip = getClip(sound);
        source.clip = clip;
        if (forcedDuration.HasValue)
        {
            source.pitch = clip.length / forcedDuration.Value;
        }
        else
        {
            source.pitch = 1;
        }

        source.Play();

    }

    AudioClip getClip(UnitSoundClip clip)
    {
        switch (clip)
        {
            case UnitSoundClip.Spawn:
                return  spawn;
            case UnitSoundClip.Stun:
                return  stun;
            case UnitSoundClip.Dash:
                return  dash;
            case UnitSoundClip.Fall:
                return  fall;
            case UnitSoundClip.LocalStun:
                return  localStun;
            case UnitSoundClip.PortalStart:
                return  portalStart;
            case UnitSoundClip.PortalEnd:
                return  portalEnd;
            default:
                return null;
        }

    }

    float portalTime = 1.5f;
    IEnumerator RoutineRecall()
    {
        playSound(UnitSoundClip.PortalStart, portalTime);
        yield return new WaitForSecondsRealtime(portalTime);
        playSound(UnitSoundClip.PortalEnd);
    }


}

