using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class SoundManager : NetworkBehaviour
{
    public GameObject soundPre;

    public AudioClip portalStart;
    public AudioClip portalEnd;

    [Server]
    public void sendSound(SoundClip sound, Vector3 position)
    {
        RpcSoundPosition(sound, position);
    }
    [Server]
    public void sendSoundDuration(SoundClip sound, Vector3 position, float duration)
    {
        RpcSoundDuration(sound, position, duration);
    }


    public enum SoundClip:byte{
        PortalStart,
        PortalEnd,


    }

    [ClientRpc]
    void RpcSoundPosition(SoundClip sound, Vector3 position)
    {
        playSound(sound, position);
    }
    [ClientRpc]
    void RpcSoundDuration(SoundClip sound, Vector3 position, float duration)
    {
        playSound(sound, position, duration);
    }

    [Client]
    public void playSound(SoundClip sound, Vector3 position, float? forcedDuration =null)
    {
        GameObject o = Instantiate(soundPre, position, Quaternion.identity);
        AudioSource source = o.GetComponent<AudioSource>();
        AudioClip clip = getClip(sound);
        source.clip = clip;
        if(forcedDuration.HasValue)
        {
            source.pitch = clip.length / forcedDuration.Value;
        }
        
        source.Play();
        StartCoroutine(cleanupSound(o));
    }

    IEnumerator cleanupSound(GameObject o, float wait = 2f)
    {
        yield return new WaitForSeconds(wait);
        Destroy(o);
    }

    AudioClip getClip(SoundClip clip)
    {
        switch (clip)
        {
            case SoundClip.PortalStart:
                return portalStart;
                case SoundClip.PortalEnd:
                return portalEnd;
            default:
                return null;
        }
    }
}
