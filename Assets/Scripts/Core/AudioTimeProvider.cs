using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AudioTimeProvider : MonoBehaviour
{
    public float AudioTime = 0f; //notes get this value

    float startTime;
    float speed;
    long ticks = 0;
    public bool isStart = false;
    public bool isRecord = false;
    public float offset = 0f;
    public float audioOffset = 0f;
    public AudioSource bgm;
    public SoundEffect SE;

    public void SetStartTime(long _ticks, float _offset, float _speed, bool _isRecord = false)
    {
        ticks = _ticks;
        offset = _offset;
        AudioTime = offset;
        var dateTime = new DateTime(ticks);
        var seconds = (dateTime - DateTime.Now).TotalSeconds;
        isRecord = _isRecord;
        speed = _speed;
        Debug.Log("offset = " + offset);
        SE.generateSoundEffectList(offset, isRecord);
        if (_isRecord)
        {
            startTime = Time.time + 5f + audioOffset;
        }
        else
        {
            startTime = Time.time + audioOffset;
        }
        isStart = true;
        bgm.time = 0f;
        if(isRecord)
            bgm.PlayDelayed(5f);
        else
            bgm.Play();
    }

    public void Pause()
    {
        isStart = false;
        bgm.Stop();
    }

    public void Resume()
    {
        startTime = Time.time + audioOffset;
        offset = AudioTime - audioOffset;
        isStart = true;
        bgm.time = AudioTime - audioOffset;
        bgm.Play();
    }

    public void ResetStartTime()
    {
        offset = 0f;
        AudioTime = 0f;
        bgm.Stop();
        isStart = false;
    }

    void Update()
    {
        if (isStart)
        {
            AudioTime = (Time.time - startTime) * speed + offset;
            // AudioTime = bgm.time;
            SE.SoundEffectUpdate();
            if (AudioTime >= 0 && Mathf.Abs(AudioTime - audioOffset - bgm.time) > 0.05)
            {
                Debug.Log("bgm time delay > 0.05");
                if(AudioTime + audioOffset > bgm.clip.length) {
                    bgm.Stop();
                    ResetStartTime();
                }
                // bgm.time = AudioTime;
                if (AudioTime + audioOffset > bgm.time)
                    startTime += Mathf.Abs(AudioTime - audioOffset - bgm.time);
                else
                    startTime -= Mathf.Abs(AudioTime - audioOffset - bgm.time);
            }
        }
    }
}
