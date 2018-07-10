using System.Collections;
using UnityEngine.Audio;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Sound {

    public AudioMixerGroup audioMixerGroup;

    public string audioName;

    public AudioClip[] audioClips;

    [Range(0f, 2f)]
    public float volume = 1f;
    [Range(-1f, 2f)]
    public float pitch = 1f;
    [Range(0f, 1f)]
    public float volumeRandom = 0.3f;
    [Range(0f, 1f)]
    public float pitchRandom = 0.3f;
    [Range(0f, 0.1f)]
    public float fadeInSpeed = 0.005f;
    [Range(0f, 0.1f)]
    public float fadeOutSpeed = 0.005f;

    public bool loop;
}
