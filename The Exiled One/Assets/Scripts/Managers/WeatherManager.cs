using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherManager : MonoBehaviour {

    #region Instance
    private static WeatherManager weatherManagerInstance;

    public static WeatherManager Instance { get { return weatherManagerInstance; } }

    private void Awake()
    {
        if (weatherManagerInstance != null && weatherManagerInstance != this)
        {
            if (gameObject != null)
            {
                Destroy(gameObject);
            }
            return;
        }
        weatherManagerInstance = this;
    }
    #endregion

    // References
    private TimeManager timeManager;
    private EventManager eventManager;
    private SoundManager soundManager;
    private IEnumerator lightningHandler;

    // Weather variables
    [Range(0, 1)]
    public float wetDayThreshold; // Minimum float to be termed a wet day  
    [Range(0, 1)]
    public float rainChance; // Minimum chance of rain on a wet day
    
    [Range(0, 1)]
    public float defaultWetness; // Default wetness of a day

    [Range(0, 24)]
    public int minRainDuration; // Minimum duration of rain in hours
    [Range(0, 24)]
    public int maxRainDuration; // Max duration of rain in hours

    public float lightningDelay; // Minimum time between each lightning strike

    [Range(0, 1)]
    public float lightningChance; // Minimum chance of lightning
    public float lightningDuration; // How long lightning flashes for
    public float minthunderDelay; // Minimum delay after lightning before thunder rumbles
    public float maxthunderDelay; // Maximum delay after lightning before thunder rumbles

    private int hoursSinceRainStart = 0; // Hours since rain started
    private int rainDuration; // Hours it should rain for

    private float wetness; // Wetness of the day, high wetness means higher chance of rain
    public bool isRaining = false;
    private bool isPaused = false; // Is the weather paused?

    private void OnEnable()
    {
        // Set variables
        isRaining = false;
        wetness = defaultWetness;

        // Subscribe to events
        EventManager.Instance.e_hourPass.AddListener(DetermineWeather);
        EventManager.Instance.e_saveGame.AddListener(Save);
        EventManager.Instance.e_loadGame.AddListener(Load);
        EventManager.Instance.e_pauseGame.AddListener(PauseWeather);
        EventManager.Instance.e_resumeGame.AddListener(ResumeWeather);
    }

    private void OnDisable()
    {
        EventManager.Instance.e_hourPass.RemoveListener(DetermineWeather);
        EventManager.Instance.e_saveGame.RemoveListener(Save);
        EventManager.Instance.e_loadGame.RemoveListener(Load);
        EventManager.Instance.e_pauseGame.RemoveListener(PauseWeather);
        EventManager.Instance.e_resumeGame.RemoveListener(ResumeWeather);
    }

    // Check hourly if rain or not
    void DetermineWeather()
    {
        // If raining we don't check until rain stops
        if (!isRaining)
        {
            // Change wetness of the day
            float wetnessChanger = Random.Range(0, 2) == 0 ? -0.1f : 0.1f;

            wetness += wetnessChanger;
            wetness = Mathf.Clamp(wetness, 0, 1);

            // It is a wet day
            if (wetness >= wetDayThreshold)
            {
                float rainCheck = Random.Range(0.0f, 1.0f);
                // Rain
                if (rainCheck <= rainChance)
                { 
                    Rain();
                }
            }
        }
        else // It's raining
        {
            hoursSinceRainStart += 1;

            // Stop rain
            if (hoursSinceRainStart >= rainDuration)
            {
                StopRain();
            }
        }
    }

    void Rain()
    {
        // Call rain event
        EventManager.Instance.e_raining.Invoke();

        // Enable rain
        RainController.Instance.EnableRain();
        isRaining = true;
        SoundManager.Instance.PlaySound("Rain");

        // Set variablse
        wetness = defaultWetness;
        rainDuration = Random.Range(minRainDuration, maxRainDuration + 1);
        hoursSinceRainStart = 0;

        if (lightningHandler != null)
        {
            StopCoroutine(lightningHandler);
            lightningHandler = null;
        }

        lightningHandler = Lightning();
        StartCoroutine(lightningHandler);
    }

    private IEnumerator Lightning()
    {
        while (isRaining)
        {
            float lightningCheck = Random.Range(0.0f, 1.0f);

            if (lightningCheck <= lightningChance && !isPaused)
            {
                LightningController.Instance.EnableLightning();
                Invoke("DisableLightning", lightningDuration);
            }

            yield return new WaitForSecondsRealtime(lightningDelay);
        }

        lightningHandler = null;
    }

    private void DisableLightning()
    {
        LightningController.Instance.DisableLightning();
        if (!isPaused)
        {
            float thunderDelay = Random.Range(minthunderDelay, maxthunderDelay);
            Invoke("Thunder", thunderDelay);
        }
    }

    private void Thunder()
    {
        if (!isPaused)
        {
            SoundManager.Instance.PlaySoundOneShot("Thunder");
        } 
    }

    void StopRain()
    {
        // Call rain stop event
        EventManager.Instance.e_rainStop.Invoke();

        // Disable rain
        isRaining = false;
        SoundManager.Instance.StopSound("Rain");
        Invoke("DisableRain", SoundManager.Instance.sounds["Rain"].fadeOutSpeed);
        hoursSinceRainStart = 0;
        wetness = defaultWetness;
    }

    private void DisableRain()
    {
        RainController.Instance.DisableRain();
    }

    private void PauseWeather()
    {
        RainController.Instance.PauseRain();
        isPaused = true;
    }

    private void ResumeWeather()
    {
        RainController.Instance.ResumeRain();
        isPaused = false;
    }

    // SAVE LOAD WEATHER

    private void Save()
    {
        var data = new WeatherData();

        if (isRaining)
        {
            data.isRaining = isRaining;
            data.rainDuration = rainDuration;
            data.hoursSinceRainStart = hoursSinceRainStart;
            data.wetness = defaultWetness;
        }
        else
        {
            data.isRaining = false;
            data.rainDuration = 0;
            data.hoursSinceRainStart = 0;
            data.wetness = wetness;
        }

        GameManager.Instance.gameData.weather = data;
    }

    private void Load()
    {
        isRaining = GameManager.Instance.gameData.weather.isRaining;

        if (isRaining)
        {
            Rain();
            rainDuration = GameManager.Instance.gameData.weather.rainDuration;
            hoursSinceRainStart = GameManager.Instance.gameData.weather.hoursSinceRainStart;
        }
        else
        {
            RainController.Instance.DisableRainImmediately();
            wetness = GameManager.Instance.gameData.weather.wetness;
        }
    }

    [System.Serializable]
    public class WeatherData
    {
        public bool isRaining;
        public int rainDuration;
        public int hoursSinceRainStart;
        public float wetness;
    }
}

