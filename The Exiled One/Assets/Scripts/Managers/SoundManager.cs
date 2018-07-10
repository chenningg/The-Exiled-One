using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    #region Instance
    private static SoundManager soundManagerInstance;

    public static SoundManager Instance { get { return soundManagerInstance; } }

    private void Awake()
    {
        if (soundManagerInstance != null && soundManagerInstance != this)
        {
            if (gameObject != null)
            {
                Destroy(gameObject);
            }
            return;
        }
        soundManagerInstance = this;
    }
    #endregion

    // Sound delay variables
    public float audioFadeDelay = 0.1f; // Controls the delay between which audio volume fades in and out

    // Array of all sounds and music
    public Dictionary<string, Sound> sounds = new Dictionary<string, Sound>();
    public Dictionary<string, Sound> music = new Dictionary<string, Sound>();
    private Dictionary<string, AudioSource> soundAudioSources = new Dictionary<string, AudioSource>();
    private Dictionary<string, IEnumerator> soundsBeingPlayed = new Dictionary<string, IEnumerator>();

    public Sound[] musicArray;
    public Sound[] soundsArray;
    public AudioSource soundSource;
    public AudioSource musicSource;

    private Sound sound;
    private Sound musicTrack;
 
    // Sound playing checks
    public bool playingMusic = false;
    private IEnumerator playMusic;

    private void Start()
    {
        // Subscribe to events
        SceneManager.activeSceneChanged += StopSoundOnSceneChange;
        EventManager.Instance.e_pauseGame.AddListener(PauseAllSounds);
        EventManager.Instance.e_resumeGame.AddListener(ResumeAllSounds);

        // Set variables
        foreach (Sound s in soundsArray)
        {
            sounds[s.audioName] = s;
            soundAudioSources[s.audioName] = gameObject.AddComponent<AudioSource>();
            soundAudioSources[s.audioName].playOnAwake = false;
        }

        foreach (Sound s in musicArray)
        {
            music[s.audioName] = s;
            
        }

        //TESTING
        PlayMusic("Main Theme");
    }

    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= StopSoundOnSceneChange;
        EventManager.Instance.e_pauseGame.RemoveListener(PauseAllSounds);
        EventManager.Instance.e_resumeGame.RemoveListener(ResumeAllSounds);
    }

    // STOP SOUNDS ON SCENE CHANGE

    private void StopSoundOnSceneChange(Scene current, Scene next)
    {
        // Stop all playing sounds if scene changes
        StopAllSounds();
    }

    // SOUND CONTROLLERS

    public void PlaySoundOneShot(string soundName)
    {
        AudioClip soundToPlay = null;

        if (sounds.TryGetValue(soundName, out sound))
        {
            if (sound.audioClips.Length == 0) // No sounds in array
            {
                Debug.LogWarning("No sound to be played.");
            }
            else if (sound.audioClips.Length == 1) // Only one sound in array, we use it
            {
                soundToPlay = sound.audioClips[0];
            }
            else // Audioclips has more than one sound, we pick one randomly to play
            {
                var randomIndex = Random.Range(0, sound.audioClips.Length); // Max is excluded so we add 1
                soundToPlay = sound.audioClips[randomIndex];
            }

            var volume = sound.volume + Random.Range(-sound.volumeRandom, sound.volumeRandom);

            soundSource.PlayOneShot(soundToPlay, volume);

        }
        else
        {
            Debug.LogError(soundName + " could not be found.");
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
                soundAudioSources[soundName].clip = sound.audioClips[0];
            }
            else // Audioclips has more than one sound, we pick one randomly to play
            {
                var randomIndex = Random.Range(0, sound.audioClips.Length); // Max is excluded so we add 1
                soundAudioSources[soundName].clip = sound.audioClips[randomIndex];
            }

            soundsBeingPlayed[soundName] = StartPlayingSound(soundName);
            StartCoroutine(soundsBeingPlayed[soundName]);        
        }
        else
        {
            Debug.LogError(soundName + " could not be found.");
        }
    }

    public void StopAllSounds()
    {
        foreach (KeyValuePair<string, Sound> s in sounds)
        {
            if (soundAudioSources[s.Value.audioName].isPlaying)
            {
                StopSound(s.Value.audioName);
            }
        }
    }

    public void StopSound(string soundName)
    {
        StartCoroutine(StopPlayingSound(soundName));
    }

    private void PauseAllSounds()
    {
        foreach (KeyValuePair<string, Sound> s in sounds)
        {
            if (soundAudioSources[s.Value.audioName].isPlaying)
            {
                soundAudioSources[s.Value.audioName].Pause();
            }
        }

        if (soundSource.isPlaying)
        {
            soundSource.Pause();
        }
    }

    private void ResumeAllSounds()
    {
        foreach (KeyValuePair<string, Sound> s in sounds)
        {
            soundAudioSources[s.Value.audioName].UnPause();
        }

        soundSource.UnPause();
    }

    private IEnumerator StartPlayingSound(string soundName)
    {
        bool playSoundCheck = true;

        while (!soundAudioSources[soundName].isPlaying && playSoundCheck)
        {
            playSoundCheck = false;

            // Change any variables required
            soundAudioSources[soundName].outputAudioMixerGroup = sound.audioMixerGroup;
            soundAudioSources[soundName].loop = sound.loop;
            soundAudioSources[soundName].volume = sound.volume + Random.Range(-sound.volumeRandom, sound.volumeRandom);
            soundAudioSources[soundName].pitch = sound.pitch + Random.Range(-sound.pitchRandom, sound.pitchRandom);

            if (sounds[soundName].fadeInSpeed == 0)
            {
                soundAudioSources[soundName].Play();
            }
            else // Fade in sound
            {
                float audioVolume = 0f;
                float audioSourceVolume = soundAudioSources[soundName].volume;
                soundAudioSources[soundName].volume = 0f;
                soundAudioSources[soundName].Play();
                while (audioVolume <= audioSourceVolume)
                {
                    audioVolume += sounds[soundName].fadeInSpeed;
                    soundAudioSources[soundName].volume = audioVolume;
                    yield return new WaitForSecondsRealtime(audioFadeDelay);
                }
            }

            yield return null;
        }   

        // Remove from sounds being played
        soundsBeingPlayed.Remove(soundName);
    }

    private IEnumerator StopPlayingSound(string soundName)
    {
        // Fade out sound
        if (sounds[soundName].fadeOutSpeed != 0)
        {
            // Stop coroutine if playing
            if (soundsBeingPlayed.ContainsKey(soundName))
            {
                StopCoroutine(soundsBeingPlayed[soundName]);
                soundsBeingPlayed.Remove(soundName);
            }

            float audioVolume = soundAudioSources[soundName].volume;

            while (audioVolume > 0)
            {
                audioVolume -= sounds[soundName].fadeOutSpeed;
                soundAudioSources[soundName].volume = audioVolume;
                yield return new WaitForSecondsRealtime(audioFadeDelay);
            }       
        }

        soundAudioSources[soundName].Stop();
    }

    // MUSIC CONTROLLERS

    public void PlayMusic(string musicName)
    {
        if (playingMusic) // Another music track playing, we stop it and play the new one.
        {
            StopMusic();
        }

        if (music.TryGetValue(musicName, out musicTrack))
        {
            if (musicTrack.audioClips.Length == 0) // No music in array
            {
                Debug.LogWarning("No sound to be played.");
            }
            else
            {
                // Change any variables required
                musicSource.clip = musicTrack.audioClips[0];
                musicSource.pitch = musicTrack.pitch;
                musicSource.volume = musicTrack.volume;
                musicSource.loop = musicTrack.loop;
                playMusic = StartPlayingMusic(musicName);
                StartCoroutine(playMusic);
            }
        }
        else
        {
            Debug.LogError(musicName + " does not exist!");
        }
    }

    private IEnumerator StartPlayingMusic(string musicName)
    {
        bool musicPlayCheck = true;

        while (!playingMusic && musicPlayCheck)
        {
            musicPlayCheck = false;

            if (music[musicName].fadeInSpeed == 0)
            {
                musicSource.Play();
            }
            else
            {
                var audioVolume = 0f;
                var audioSourceVolume = musicSource.volume;
                musicSource.volume = 0f;
                musicSource.Play();
                while (audioVolume <= audioSourceVolume)
                {
                    audioVolume += music[musicName].fadeInSpeed;
                    musicSource.volume = audioVolume;
                    yield return new WaitForSecondsRealtime(audioFadeDelay);
                }
            }

            yield return null;
        }
        
        playMusic = null;
    }

    public void StopMusic()
    {
        if (playMusic != null)
        {
            StopCoroutine(playMusic);
        }

        StartCoroutine(StopPlayingMusic());
    }

    private IEnumerator StopPlayingMusic()
    {
        Sound musicToStop = musicTrack;
        var audioVolume = musicSource.volume;

        while (audioVolume > 0)
        {
            audioVolume -= musicToStop.fadeOutSpeed;
            musicSource.volume = audioVolume;
            yield return new WaitForSecondsRealtime(audioFadeDelay);
        }

        musicSource.Stop();             
        playingMusic = false;
    }
}
