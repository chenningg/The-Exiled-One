using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunController : MonoBehaviour {

    // References
    public Light sun;

    private TimeManager timeManager;
    private EventManager eventManager;
    private IEnumerator setSunIntensity;

    // Sun variables
    public float sunIntensityDay;
    public float sunIntensityChangeDelay;
    public float sunIntensityChangeSpeed;

    private bool isRaining = false;
    private int sunIntensity = 0;
    private bool changeSunIntensityImmediately = true;

    #region Instance
    private static SunController sunControllerInstance;

    public static SunController Instance { get { return sunControllerInstance; } }

    private void Awake()
    {
        if (sunControllerInstance != null && sunControllerInstance != this)
        {
            if (gameObject != null)
            {
                Destroy(gameObject);
            }
            return;
        }
        sunControllerInstance = this;
    }
    #endregion

    private void OnEnable()
    {

        // Set variables
        sun.transform.rotation = Quaternion.Euler(0, 0, 0);

        // Subscribe to events
        EventManager.Instance.e_dawnTime.AddListener(SetSunIntensity);
        EventManager.Instance.e_dayTime.AddListener(SetSunIntensity);
        EventManager.Instance.e_duskTime.AddListener(SetSunIntensity);
        EventManager.Instance.e_nightTime.AddListener(SetSunIntensity);
        EventManager.Instance.e_raining.AddListener(IsRaining);
        EventManager.Instance.e_rainStop.AddListener(StopRaining);
        EventManager.Instance.e_gameLoaded.AddListener(Load);
    }

    private void OnDisable()
    {
        EventManager.Instance.e_dawnTime.RemoveListener(SetSunIntensity);
        EventManager.Instance.e_dayTime.RemoveListener(SetSunIntensity);
        EventManager.Instance.e_duskTime.RemoveListener(SetSunIntensity);
        EventManager.Instance.e_nightTime.RemoveListener(SetSunIntensity);
        EventManager.Instance.e_raining.RemoveListener(IsRaining);
        EventManager.Instance.e_rainStop.RemoveListener(StopRaining);
        EventManager.Instance.e_gameLoaded.RemoveListener(Load);
    }

    // Change sun intensities

    public void Load()
    {
        changeSunIntensityImmediately = true;
        SetSunIntensity();
    }

    public void SetSunIntensity()
    {
        switch (TimeManager.Instance.GetTimeOfTheDay())
        {
            case ("Dawn"):
                sunIntensity = 50;
                break;
            case ("Day"):
                sunIntensity = 100;
                break;
            case ("Dusk"):
                sunIntensity = 50;
                break;
            case ("Night"):
                sunIntensity = 0;
                break;
        }

        // If raining, dim Sun by 25% unless at night
        if (isRaining)
        {
            if (sunIntensity != 0)
            {
                sunIntensity -= 25;
            }           
        }

        if (setSunIntensity != null)
        {
            StopCoroutine(setSunIntensity);
            setSunIntensity = null;
        }

        setSunIntensity = SetSunIntensityHandler(sunIntensity);
        StartCoroutine(setSunIntensity);
    }

    public void IsRaining()
    {
        isRaining = true;
        SetSunIntensity();
    }

    public void StopRaining()
    {
        isRaining = false;
        SetSunIntensity();
    }

    private IEnumerator SetSunIntensityHandler(int amount)
    {
        var newSunIntensity = (sunIntensityDay / 100) * sunIntensity;

        if (changeSunIntensityImmediately)
        {
            sun.intensity = newSunIntensity;
            changeSunIntensityImmediately = false;
        }
        else
        {
            if (sun.intensity < newSunIntensity)
            {
                while (sun.intensity < newSunIntensity)
                {
                    sun.intensity += sunIntensityChangeSpeed;
                    yield return new WaitForSecondsRealtime(sunIntensityChangeDelay);
                }
            }
            else if (sun.intensity > newSunIntensity)
            {
                while (sun.intensity > newSunIntensity)
                {
                    sun.intensity -= sunIntensityChangeSpeed;
                    yield return new WaitForSecondsRealtime(sunIntensityChangeDelay);
                }
            }
        }      
        
        setSunIntensity = null;
    }
}
