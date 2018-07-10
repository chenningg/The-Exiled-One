using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

// Add this to any gameobject to play sounds from it
[RequireComponent(typeof(AudioSource))]
public class AudioPlayer : MonoBehaviour {

    // Sounds
    private Dictionary<string, Sound> sounds = new Dictionary<string, Sound>();

    public Sound[] soundsArray;

    public AudioSource audioSource;

    public AudioClip audioToPlay;

    private Sound sound;

    private void Start()
    {
        foreach (Sound s in soundsArray)
        {
            sounds[s.audioName] = s;
        }
    }

    public void PlaySound(string soundName)
    {
        if (sounds.TryGetValue(soundName, out sound))
        {
            if (sound.audioClips.Length == 0) // No sounds in array
            {
                Debug.LogWarning("No sound to be played.");
            }
            else if (sound.audioClips.Length == 1) // Only one sound in array, we use it
            {
                audioToPlay = sound.audioClips[0];
            }
            else // Audioclips has more than one sound, we pick one randomly to play
            {
                var randomIndex = Random.Range(0, sound.audioClips.Length); // Max is excluded so we add 1
                audioToPlay = sound.audioClips[randomIndex];
            }

            // Randomize volume and pitch slightly
            var randomVolume = sound.volume + Random.Range(-sound.volumeRandom, sound.volumeRandom);
            audioSource.pitch = sound.pitch + Random.Range(-sound.pitchRandom, sound.pitchRandom);
            audioSource.PlayOneShot(audioToPlay, randomVolume);
        }
        else
        {
            Debug.LogError(soundName + " could not be found.");
        }
    }
}
