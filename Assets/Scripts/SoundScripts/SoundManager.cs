﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;
    public AudioMixer audioMixer;
    public AudioMixerGroup mixerGroup;
    public Sound[] sounds;
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        MakeInstance();

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();

            s.source.clip = s.clip;
            s.source.outputAudioMixerGroup = mixerGroup;
            //s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.playOnAwake = false;

        }
    }
    void MakeInstance()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }
    public void PlaySound(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
            return;
        Debug.Log("PlaySound: " + name);
        s.source.Play();
    }
}
