using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public List<AudioSource> AudioList = new List<AudioSource>{};
    public void Play(int Index, bool shot)
    {
        AudioSource a = AudioList[Index];
        if (shot) {a.PlayOneShot(a.clip, 1f);}
        else {AudioList[Index].Play();}
    }
    public void Stop(int Index)
    {
        AudioSource a = AudioList[Index];
        a.Stop();
    }
    public void StopAll()
    {
        foreach(AudioSource a in AudioList)
        {
            a.Stop();
        }
    }

}